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

        public MusicasController(ApplicationDbContext context, UserManager<Utilizador> userManager)
        {
            _context = context;
            _userManager = userManager;
        }

        public async Task<IActionResult> Index(string generoFiltro, string artistaFiltro, int? anoFiltro)
        {
            var query = _context.Musicas
                .Include(m => m.Artista)
                .Include(m => m.Genero)
                .Include(m => m.Streamings)
                .AsQueryable();

            if (!User.IsInRole("Admin"))
            {
                var top10 = await query
                    .OrderByDescending(m => m.Streamings.Sum(s => s.NumeroDeStreams))
                    .Take(10)
                    .ToListAsync();
                return View(top10);
            }

            if (!string.IsNullOrEmpty(generoFiltro))
                query = query.Where(m => m.Genero.Nome == generoFiltro);

            if (!string.IsNullOrEmpty(artistaFiltro))
                query = query.Where(m => m.Artista.Nome_Artista == artistaFiltro);

            if (anoFiltro.HasValue)
                query = query.Where(m => m.AnoDeLancamento == anoFiltro.Value);

            ViewBag.Generos = new SelectList(await _context.Generos.ToListAsync(), "Nome", "Nome");
            ViewBag.Artistas = new SelectList(await _context.Artistas.ToListAsync(), "Nome_Artista", "Nome_Artista");

            return View(await query.OrderBy(m => m.PosicaoBillboard ?? 999).ToListAsync());
        }

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

        [Authorize]
        public async Task<IActionResult> Create()
        {
            var user = await _userManager.GetUserAsync(User);
            if (!user.IsAdmin && !user.IsPremium)
            {
                return Forbid();
            }

            ViewData["ArtistaId"] = new SelectList(_context.Artistas, "Id", "Nome_Artista");
            ViewData["GeneroId"] = new SelectList(_context.Generos, "Id", "Nome");
            return View();
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        [Authorize]
        public async Task<IActionResult> Create(Musica musica, IFormFile ficheiroAudio)
        {
            var user = await _userManager.GetUserAsync(User);
            if (!user.IsAdmin && !user.IsPremium) return Forbid();

            ModelState.Remove("Genero");
            ModelState.Remove("Artista");

            if (ModelState.IsValid)
            {
                if (ficheiroAudio != null && ficheiroAudio.Length > 0)
                {
                    var extensao = Path.GetExtension(ficheiroAudio.FileName);
                    if (extensao.ToLower() != ".mp3")
                    {
                        ModelState.AddModelError(string.Empty, "Apenas ficheiros .mp3 são permitidos.");
                        return View(musica);
                    }

                    var nomeUnico = Guid.NewGuid() + extensao;
                    var caminho = Path.Combine(Directory.GetCurrentDirectory(), "wwwroot/uploads", nomeUnico);

                    using (var stream = new FileStream(caminho, FileMode.Create))
                    {
                        await ficheiroAudio.CopyToAsync(stream);
                    }

                    musica.NomeFicheiroAudio = nomeUnico;
                }

                _context.Add(musica);
                await _context.SaveChangesAsync();
                return RedirectToAction(nameof(Index));
            }

            ViewData["ArtistaId"] = new SelectList(_context.Artistas, "Id", "Nome_Artista", musica.ArtistaId);
            ViewData["GeneroId"] = new SelectList(_context.Generos, "Id", "Nome", musica.GeneroId);
            return View(musica);
        }

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
                        var extensao = Path.GetExtension(ficheiroAudio.FileName);
                        if (extensao.ToLower() != ".mp3")
                        {
                            ModelState.AddModelError("NomeFicheiroAudio", "Apenas ficheiros .mp3 são permitidos.");
                            return View(musica);
                        }

                        var nomeUnico = Guid.NewGuid() + extensao;
                        var caminho = Path.Combine(Directory.GetCurrentDirectory(), "wwwroot/uploads", nomeUnico);

                        using (var stream = new FileStream(caminho, FileMode.Create))
                        {
                            await ficheiroAudio.CopyToAsync(stream);
                        }

                        musica.NomeFicheiroAudio = nomeUnico;
                    }

                    _context.Update(musica);
                    await _context.SaveChangesAsync();
                }
                catch (DbUpdateConcurrencyException)
                {
                    if (!_context.Musicas.Any(e => e.ID == id)) return NotFound();
                    else throw;
                }
                return RedirectToAction(nameof(Index));
            }

            ViewData["ArtistaId"] = new SelectList(_context.Artistas, "Id", "Nome_Artista", musica.ArtistaId);
            ViewData["GeneroId"] = new SelectList(_context.Generos, "Id", "Nome", musica.GeneroId);
            return View(musica);
        }

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
