using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Rendering;
using Microsoft.EntityFrameworkCore;
using SequeMusic.Data;
using SequeMusic.Models;
using System.IO;
using System.Threading.Tasks;

namespace SequeMusic.Controllers
{
    public class MusicasController : Controller
    {
        private readonly ApplicationDbContext _context;
        private readonly UserManager<Utilizador> _userManager;

        // Construtor que injeta o contexto da base de dados e o gestor de utilizadores
        public MusicasController(ApplicationDbContext context, UserManager<Utilizador> userManager)
        {
            _context = context;
            _userManager = userManager;
        }

        // Apresenta uma lista de músicas
        // Se o utilizador for admin, pode usar filtros
        // Caso contrário, apresenta o top 10 por número de streams
        public async Task<IActionResult> Index(string generoFiltro, string artistaFiltro, int? anoFiltro)
        {
            // Query base com includes para carregar relações
            var query = _context.Musicas
                .Include(m => m.Artista)
                .Include(m => m.Genero)
                .Include(m => m.Streamings)
                .AsQueryable();

            // Se não for admin, devolve top 10 por número de streams
            if (!User.IsInRole("Admin"))
            {
                var top10 = await query
                    .OrderByDescending(m => m.Streamings.Sum(s => s.NumeroDeStreams))
                    .Take(10)
                    .ToListAsync();
                return View(top10);
            }

            // Aplica filtros se forem fornecidos
            if (!string.IsNullOrEmpty(generoFiltro))
                query = query.Where(m => m.Genero.Nome == generoFiltro);

            if (!string.IsNullOrEmpty(artistaFiltro))
                query = query.Where(m => m.Artista.Nome_Artista == artistaFiltro);

            if (anoFiltro.HasValue)
                query = query.Where(m => m.AnoDeLancamento == anoFiltro.Value);

            // Preenche dropdowns para os filtros
            ViewBag.Generos = new SelectList(await _context.Generos.ToListAsync(), "Nome", "Nome");
            ViewBag.Artistas = new SelectList(await _context.Artistas.ToListAsync(), "Nome_Artista", "Nome_Artista");

            // Ordena por posição Billboard (ou 999 caso não tenha)
            return View(await query.OrderBy(m => m.PosicaoBillboard ?? 999).ToListAsync());
        }

        // Mostra detalhes de uma música específica
        public async Task<IActionResult> Details(int? id)
        {
            if (id == null) return NotFound();

            var musica = await _context.Musicas
                .Include(m => m.Artista)
                .Include(m => m.Genero)
                .Include(m => m.Avaliacoes).ThenInclude(a => a.Utilizador)
                .Include(m => m.Streamings)
                .FirstOrDefaultAsync(m => m.ID == id);

            if (musica == null) return NotFound();

            return View(musica);
        }

        // Mostra formulário para criar nova música (Premium/Admin)
        [Authorize]
        public async Task<IActionResult> Create()
        {
            var user = await _userManager.GetUserAsync(User);
            if (!user.IsAdmin && !user.IsPremium)
                return Forbid();

            ViewData["ArtistaId"] = new SelectList(_context.Artistas, "Id", "Nome_Artista");
            ViewData["GeneroId"] = new SelectList(_context.Generos, "Id", "Nome");
            return View();
        }

        // Submete criação de música com upload de .mp3
        [HttpPost]
        [ValidateAntiForgeryToken]
        [Authorize]
        public async Task<IActionResult> Create(Musica musica, IFormFile ficheiroAudio)
        {
            ModelState.Remove("Genero");
            ModelState.Remove("Artista");

            var user = await _userManager.GetUserAsync(User);
            if (!user.IsPremium && !user.IsAdmin)
                return Forbid();

            // Garante que o artista existe ou cria novo com base no nome do utilizador
            var artista = await _context.Artistas.FirstOrDefaultAsync(a => a.Nome_Artista == user.Nome);
            if (artista == null)
            {
                artista = new Artista
                {
                    Nome_Artista = user.Nome,
                    Biografia = "Artista registado por conta premium.",
                    Pais_Origem = "Desconhecido"
                };
                _context.Artistas.Add(artista);
                await _context.SaveChangesAsync();
            }

            musica.ArtistaId = artista.Id;

            if (ModelState.IsValid)
            {
                if (ficheiroAudio != null && ficheiroAudio.Length > 0)
                {
                    var extensao = Path.GetExtension(ficheiroAudio.FileName).ToLower();
                    if (extensao != ".mp3")
                    {
                        ModelState.AddModelError(string.Empty, "Apenas ficheiros .mp3 são permitidos.");
                        return View("PromoverCreate", musica);
                    }

                    var nomeUnico = Guid.NewGuid() + extensao;
                    var caminho = Path.Combine(Directory.GetCurrentDirectory(), "wwwroot/uploads", nomeUnico);

                    using (var stream = new FileStream(caminho, FileMode.Create))
                        await ficheiroAudio.CopyToAsync(stream);

                    musica.NomeFicheiroAudio = nomeUnico;
                }

                _context.Add(musica);
                await _context.SaveChangesAsync();
                TempData["Mensagem"] = "🎉 Música promovida com sucesso!";
                return RedirectToAction("Index");
            }

            ViewData["GeneroId"] = new SelectList(_context.Generos, "Id", "Nome", musica.GeneroId);
            return View("PromoverCreate", musica);
        }

        // Editar música (apenas Admin)
        [Authorize(Roles = "Admin")]
        public async Task<IActionResult> Edit(int? id)
        {
            if (id == null) return NotFound();

            var musica = await _context.Musicas.FindAsync(id);
            if (musica == null) return NotFound();

            ViewData["ArtistaId"] = new SelectList(_context.Artistas, "Id", "Nome_Artista", musica.ArtistaId);
            ViewData["GeneroId"] = new SelectList(_context.Generos, "Id", "Nome", musica.GeneroId);
            return View(musica);
        }

        // Submete edição (pode incluir novo ficheiro .mp3)
        [HttpPost]
        [ValidateAntiForgeryToken]
        [Authorize(Roles = "Admin")]
        public async Task<IActionResult> Edit(int id, Musica musica, IFormFile ficheiroAudio)
        {
            if (id != musica.ID) return NotFound();

            if (ModelState.IsValid)
            {
                try
                {
                    if (ficheiroAudio != null && ficheiroAudio.Length > 0)
                    {
                        var extensao = Path.GetExtension(ficheiroAudio.FileName).ToLower();
                        if (extensao != ".mp3")
                        {
                            ModelState.AddModelError("NomeFicheiroAudio", "Apenas ficheiros .mp3 são permitidos.");
                            return View(musica);
                        }

                        var nomeUnico = Guid.NewGuid() + extensao;
                        var caminho = Path.Combine(Directory.GetCurrentDirectory(), "wwwroot/uploads", nomeUnico);

                        using (var stream = new FileStream(caminho, FileMode.Create))
                            await ficheiroAudio.CopyToAsync(stream);

                        musica.NomeFicheiroAudio = nomeUnico;
                    }

                    _context.Update(musica);
                    await _context.SaveChangesAsync();
                }
                catch (DbUpdateConcurrencyException)
                {
                    if (!_context.Musicas.Any(e => e.ID == id)) return NotFound();
                    throw;
                }

                return RedirectToAction(nameof(Index));
            }

            ViewData["ArtistaId"] = new SelectList(_context.Artistas, "Id", "Nome_Artista", musica.ArtistaId);
            ViewData["GeneroId"] = new SelectList(_context.Generos, "Id", "Nome", musica.GeneroId);
            return View(musica);
        }

        // Confirmação para apagar música
        [Authorize(Roles = "Admin")]
        public async Task<IActionResult> Delete(int? id)
        {
            if (id == null) return NotFound();

            var musica = await _context.Musicas
                .Include(m => m.Artista)
                .FirstOrDefaultAsync(m => m.ID == id);

            if (musica == null) return NotFound();

            return View(musica);
        }

        // Submete a eliminação da música
        [HttpPost, ActionName("Delete")]
        [ValidateAntiForgeryToken]
        [Authorize(Roles = "Admin")]
        public async Task<IActionResult> DeleteConfirmed(int id)
        {
            var musica = await _context.Musicas.FindAsync(id);
            _context.Musicas.Remove(musica);
            await _context.SaveChangesAsync();
            return RedirectToAction(nameof(Index));
        }

        // Atualiza a posição Billboard (Admin)
        [HttpPost]
        [Authorize(Roles = "Admin")]
        public async Task<IActionResult> AtualizarPosicao(int id, int posicao)
        {
            var musica = await _context.Musicas.FindAsync(id);
            if (musica == null) return NotFound();

            musica.PosicaoBillboard = posicao;
            await _context.SaveChangesAsync();

            return RedirectToAction("Index");
        }

        // Formulário para promover música (exclusivo Premium/Admin)
        [Authorize]
        public async Task<IActionResult> Promover()
        {
            var user = await _userManager.GetUserAsync(User);
            if (user.IsPremium || user.IsAdmin)
            {
                ViewData["ArtistaId"] = new SelectList(_context.Artistas, "Id", "Nome_Artista");
                ViewData["GeneroId"] = new SelectList(_context.Generos, "Id", "Nome");
                return View("PromoverCreate");
            }

            return View("PromoverInfo");
        }
    }
}
