using Microsoft.AspNetCore.Authorization;
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

        public MusicasController(ApplicationDbContext context)
        {
            _context = context;
        }

        // GET: Musicas
        public async Task<IActionResult> Index(string generoFiltro, string artistaFiltro, int? anoFiltro)
        {
            var query = _context.Musicas
                .Include(m => m.Artista)
                .Include(m => m.Genero)
                .OrderBy(m => m.PosicaoBillboard ?? 999)
                .AsQueryable();

            if (!User.IsInRole("Admin"))
            {
                // Apenas o sistema define o Top 10 (ex: por número de streamings)
                var top10 = await query
                    .OrderByDescending(m => m.Streamings.Sum(s => s.NumeroDeStreams))
                    .Take(10)
                    .ToListAsync();

                return View(top10);
            }

            // Admin: aplicar filtros se existirem
            if (!string.IsNullOrEmpty(generoFiltro))
                query = query.Where(m => m.Genero.Nome == generoFiltro);

            if (!string.IsNullOrEmpty(artistaFiltro))
                query = query.Where(m => m.Artista.Nome_Artista == artistaFiltro);

            if (anoFiltro.HasValue)
                query = query.Where(m => m.AnoDeLancamento == anoFiltro.Value);

            ViewBag.Generos = new SelectList(await _context.Generos.ToListAsync(), "Nome", "Nome");
            ViewBag.Artistas = new SelectList(await _context.Artistas.ToListAsync(), "Nome_Artista", "Nome_Artista");

            return View(await query.ToListAsync());
        }


        // GET: Musicas/Details/5
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

        // GET: Musicas/Create
        [Authorize(Roles = "Admin")]
        public IActionResult Create()
        {
            ViewData["ArtistaId"] = new SelectList(_context.Artistas, "Id", "Nome_Artista");
            ViewData["GeneroId"] = new SelectList(_context.Generos, "Id", "Nome");
            return View();
        }

        // POST: Musicas/Create
        [HttpPost]
        [ValidateAntiForgeryToken]
        [Authorize(Roles = "Admin")]
        public async Task<IActionResult> Create(Musica musica, IFormFile ficheiroAudio)
        {
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

                    var nomeUnico = Guid.NewGuid().ToString() + extensao;
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

        // GET: Musicas/Edit/5
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

        // POST: Musicas/Edit/5
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

                        var nomeUnico = Guid.NewGuid().ToString() + extensao;
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

        // GET: Musicas/Delete/5
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

        // POST: Musicas/Delete/5
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

    }
}
