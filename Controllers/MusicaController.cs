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
    /// <summary>
    /// Controlador responsável pela gestão de músicas na aplicação SequeMusic.
    /// Inclui listagem, criação, edição, eliminação, detalhes e promoção.
    /// </summary>
    public class MusicasController : Controller
    {
        private readonly ApplicationDbContext _context;
        private readonly UserManager<Utilizador> _userManager;

        public MusicasController(ApplicationDbContext context, UserManager<Utilizador> userManager)
        {
            _context = context;
            _userManager = userManager;
        }

        /// <summary>
        /// Lista de músicas.
        /// Mostra o Top 10 se o utilizador for normal; Admins podem filtrar por género, artista e ano.
        /// </summary>
        /// <param name="generoFiltro">Género a filtrar (opcional).</param>
        /// <param name="artistaFiltro">Artista a filtrar (opcional).</param>
        /// <param name="anoFiltro">Ano de lançamento a filtrar (opcional).</param>
        /// <returns>View com a lista de músicas.</returns>
        public async Task<IActionResult> Index(string generoFiltro, string artistaFiltro, int? anoFiltro)
        {
            var query = _context.Musicas
                .Include(m => m.Artista)
                .Include(m => m.Genero)
                .Include(m => m.Streamings)
                .AsQueryable();

            // Se não for admin, mostra o Top 10 com mais streamings
            if (!User.IsInRole("Admin"))
            {
                var top10 = await query
                    .Where(m => m.PosicaoBillboard.HasValue)
                    .OrderBy(m => m.PosicaoBillboard)
                    .Take(10)
                    .ToListAsync();
                return View(top10);
            }

            // Aplica filtros para admins
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

        /// <summary>
        /// Mostra os detalhes de uma música específica.
        /// </summary>
        /// <param name="id">ID da música.</param>
        /// <returns>View com os detalhes da música ou NotFound.</returns>
        public async Task<IActionResult> Details(int? id)
        {
            if (id == null) return NotFound();

            var musica = await _context.Musicas
                .Include(m => m.Artista)
                .Include(m => m.Genero)
                .Include(m => m.Avaliacoes).ThenInclude(a => a.Utilizador)
                .Include(m => m.Streamings)
                .Include(m => m.ArtistasMusicas).ThenInclude(am => am.Artista)
                .FirstOrDefaultAsync(m => m.ID == id);

            if (musica == null) return NotFound();

            return View(musica);
        }

        /// <summary>
        /// Mostra o formulário de criação de música (apenas para Premium ou Admin).
        /// </summary>
        /// <returns>View do formulário ou Forbid se não autorizado.</returns>
        [Authorize]
        public async Task<IActionResult> Create()
        {
            var user = await _userManager.GetUserAsync(User);
            if (!user.IsAdmin && !user.IsPremium)
                return Forbid();

            // Para dropdown do artista principal
            ViewData["ArtistaId"] = new SelectList(_context.Artistas, "Id", "Nome_Artista");

            // Para múltiplas colaborações (select2)
            ViewBag.TodosArtistas = await _context.Artistas.ToListAsync();

            // Géneros disponíveis
            ViewData["GeneroId"] = new SelectList(_context.Generos, "Id", "Nome");

            return View();
        }



        /// <summary>
        /// Submete uma nova música e guarda o ficheiro .mp3.
        /// </summary>
        /// <param name="musica">Objeto música a criar.</param>
        /// <param name="ficheiroAudio">Ficheiro de áudio .mp3 enviado.</param>
        /// <returns>Redirect ou View com erros.</returns>
        [HttpPost]
        [ValidateAntiForgeryToken]
        [Authorize]
        public async Task<IActionResult> Create(Musica musica, IFormFile ficheiroAudio, int[] artistasSelecionados)
        {
            ModelState.Remove("Genero");
            ModelState.Remove("Artista");

            var user = await _userManager.GetUserAsync(User);
            if (!user.IsPremium && !user.IsAdmin)
                return Forbid();

            // Cria o artista principal se ainda não existir
            var artistaPrincipal = await _context.Artistas.FirstOrDefaultAsync(a => a.Nome_Artista == user.Nome);
            if (artistaPrincipal == null)
            {
                artistaPrincipal = new Artista
                {
                    Nome_Artista = user.Nome,
                    Biografia = "Artista registado por conta premium.",
                    Pais_Origem = "Desconhecido"
                };
                _context.Artistas.Add(artistaPrincipal);
                await _context.SaveChangesAsync();
            }

            musica.ArtistaId = artistaPrincipal.Id;

            if (ModelState.IsValid)
            {
                // Lida com o ficheiro de áudio
                if (ficheiroAudio != null && ficheiroAudio.Length > 0)
                {
                    var extensao = Path.GetExtension(ficheiroAudio.FileName).ToLower();
                    if (extensao != ".mp3")
                    {
                        ModelState.AddModelError("", "Apenas ficheiros .mp3 são permitidos.");
                        ViewBag.TodosArtistas = new MultiSelectList(await _context.Artistas.ToListAsync(), "Id", "Nome_Artista", artistasSelecionados);
                        ViewData["GeneroId"] = new SelectList(_context.Generos, "Id", "Nome", musica.GeneroId);
                        return View("PromoverCreate", musica);
                    }

                    var nomeUnico = Guid.NewGuid() + extensao;
                    var caminho = Path.Combine(Directory.GetCurrentDirectory(), "wwwroot/uploads", nomeUnico);

                    using var stream = new FileStream(caminho, FileMode.Create);
                    await ficheiroAudio.CopyToAsync(stream);

                    musica.NomeFicheiroAudio = nomeUnico;
                }

                _context.Musicas.Add(musica);
                await _context.SaveChangesAsync();

                // Relação N:N (Artistas secundários)
                foreach (var artistaId in artistasSelecionados.Distinct())
                {
                    _context.ArtistasMusicas.Add(new ArtistaMusica
                    {
                        ArtistaId = artistaId,
                        MusicaId = musica.ID
                    });
                }

                // Adiciona também o artista principal à relação N:N (se desejado)
                if (!artistasSelecionados.Contains(artistaPrincipal.Id))
                {
                    _context.ArtistasMusicas.Add(new ArtistaMusica
                    {
                        ArtistaId = artistaPrincipal.Id,
                        MusicaId = musica.ID
                    });
                }

                await _context.SaveChangesAsync();
                TempData["Mensagem"] = "🎉 Música criada com artista principal e colaborações!";
                return RedirectToAction("Index");
            }

            ViewBag.TodosArtistas = new MultiSelectList(await _context.Artistas.ToListAsync(), "Id", "Nome_Artista", artistasSelecionados);
            ViewData["GeneroId"] = new SelectList(_context.Generos, "Id", "Nome", musica.GeneroId);
            return View("PromoverCreate", musica);
        }



        /// <summary>
        /// Mostra o formulário para editar uma música (apenas Admin).
        /// </summary>
        /// <param name="id">ID da música.</param>
        /// <returns>View de edição ou NotFound.</returns>
        [Authorize(Roles = "Admin")]
        public async Task<IActionResult> Edit(int? id)
        {
            if (id == null) return NotFound();

            var musica = await _context.Musicas
                .Include(m => m.ArtistasMusicas)
                .FirstOrDefaultAsync(m => m.ID == id);

            if (musica == null) return NotFound();

            ViewData["ArtistaId"] = new SelectList(_context.Artistas, "Id", "Nome_Artista", musica.ArtistaId);
            ViewData["GeneroId"] = new SelectList(_context.Generos, "Id", "Nome", musica.GeneroId);
            ViewBag.TodosArtistas = await _context.Artistas.ToListAsync();
            ViewBag.ArtistasSelecionadosIds = musica.ArtistasMusicas.Select(am => am.ArtistaId).ToList();

            return View(musica);
        }


        /// <summary>
        /// Submete alterações a uma música (pode incluir novo .mp3).
        /// </summary>
        /// <param name="id">ID da música.</param>
        /// <param name="musica">Objeto atualizado.</param>
        /// <param name="ficheiroAudio">Novo ficheiro (opcional).</param>
        /// <returns>Redirect ou View com erros.</returns>
        [HttpPost]
        [ValidateAntiForgeryToken]
        [Authorize(Roles = "Admin")]
        public async Task<IActionResult> Edit(int id, Musica musica, IFormFile ficheiroAudio, int[] artistasSelecionados)
        {
            if (id != musica.ID) return NotFound();

            if (ModelState.IsValid)
            {
                try
                {
                    // Substituir ficheiro mp3, se fornecido
                    if (ficheiroAudio != null && ficheiroAudio.Length > 0)
                    {
                        var extensao = Path.GetExtension(ficheiroAudio.FileName).ToLower();
                        if (extensao != ".mp3")
                        {
                            ModelState.AddModelError("NomeFicheiroAudio", "Apenas ficheiros .mp3 são permitidos.");
                            ViewData["ArtistaId"] = new SelectList(_context.Artistas, "Id", "Nome_Artista", musica.ArtistaId);
                            ViewData["GeneroId"] = new SelectList(_context.Generos, "Id", "Nome", musica.GeneroId);
                            ViewBag.TodosArtistas = await _context.Artistas.ToListAsync();
                            ViewBag.ArtistasSelecionadosIds = artistasSelecionados.ToList();
                            return View(musica);
                        }

                        var nomeUnico = Guid.NewGuid() + extensao;
                        var caminho = Path.Combine(Directory.GetCurrentDirectory(), "wwwroot/uploads", nomeUnico);

                        using var stream = new FileStream(caminho, FileMode.Create);
                        await ficheiroAudio.CopyToAsync(stream);

                        musica.NomeFicheiroAudio = nomeUnico;
                    }

                    // Atualizar música
                    _context.Update(musica);
                    await _context.SaveChangesAsync();

                    // Atualizar colaborações N:N
                    var antigos = _context.ArtistasMusicas.Where(am => am.MusicaId == musica.ID);
                    _context.ArtistasMusicas.RemoveRange(antigos);
                    await _context.SaveChangesAsync();

                    // Adicionar os novos artistas selecionados
                    foreach (var artistaId in artistasSelecionados.Distinct())
                    {
                        _context.ArtistasMusicas.Add(new ArtistaMusica
                        {
                            ArtistaId = artistaId,
                            MusicaId = musica.ID
                        });
                    }

                    // Garante que o artista principal também está na lista de colaborações
                    if (!artistasSelecionados.Contains(musica.ArtistaId))
                    {
                        _context.ArtistasMusicas.Add(new ArtistaMusica
                        {
                            ArtistaId = musica.ArtistaId,
                            MusicaId = musica.ID
                        });
                    }

                    await _context.SaveChangesAsync();
                }
                catch (DbUpdateConcurrencyException)
                {
                    if (!_context.Musicas.Any(e => e.ID == musica.ID)) return NotFound();
                    throw;
                }

                return RedirectToAction(nameof(Index));
            }

            // Em caso de erro, recarrega dropdowns e colaborações
            ViewData["ArtistaId"] = new SelectList(_context.Artistas, "Id", "Nome_Artista", musica.ArtistaId);
            ViewData["GeneroId"] = new SelectList(_context.Generos, "Id", "Nome", musica.GeneroId);
            ViewBag.TodosArtistas = await _context.Artistas.ToListAsync();
            ViewBag.ArtistasSelecionadosIds = artistasSelecionados.ToList();

            return View(musica);
        }


        /// <summary>
        /// Mostra confirmação antes de apagar uma música (Admin).
        /// </summary>
        /// <param name="id">ID da música.</param>
        /// <returns>View de confirmação ou NotFound.</returns>
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

        /// <summary>
        /// Elimina definitivamente a música da base de dados (Admin).
        /// </summary>
        /// <param name="id">ID da música.</param>
        /// <returns>Redireciona para Index após apagar.</returns>
        [HttpPost, ActionName("Delete")]
        [ValidateAntiForgeryToken]
        [Authorize(Roles = "Admin")]
        public async Task<IActionResult> DeleteConfirmed(int id)
        {
            var musica = await _context.Musicas
                .Include(m => m.ArtistasMusicas)
                .Include(m => m.Streamings)
                .Include(m => m.Avaliacoes)
                .FirstOrDefaultAsync(m => m.ID == id);

            if (musica == null) return NotFound();

            // Remove colaborações
            _context.ArtistasMusicas.RemoveRange(musica.ArtistasMusicas);

            // Remove streamings
            _context.Streamings.RemoveRange(musica.Streamings);

            // Remove avaliações
            _context.Avaliacoes.RemoveRange(musica.Avaliacoes);

            // Remove a música
            _context.Musicas.Remove(musica);

            await _context.SaveChangesAsync();
            return RedirectToAction(nameof(Index));
        }


        /// <summary>
        /// Atualiza a posição Billboard de uma música (Admin).
        /// </summary>
        /// <param name="id">ID da música.</param>
        /// <param name="posicao">Nova posição Billboard.</param>
        /// <returns>Redireciona para Index.</returns>
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

        /// <summary>
        /// Mostra o formulário para promoção de música (Premium/Admin).
        /// </summary>
        /// <returns>View de promoção ou informativa se não tiver permissões.</returns>
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
