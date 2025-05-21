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

        private void PopularDropDowns(object artistaSelecionado = null, object generoSelecionado = null)
        {
            var artistas = _context.Artistas.ToList() ?? new List<Artista>();
            var generos = _context.Generos.ToList() ?? new List<Genero>();

            ViewBag.ArtistaId = new SelectList(artistas, "ID", "Nome", artistaSelecionado);
            ViewBag.GeneroId = new SelectList(generos, "ID", "Nome", generoSelecionado);
        }

        public async Task<IActionResult> Index()
        {
            var musicas = _context.Musicas
                .Include(m => m.Artista)
                .Include(m => m.Genero);
            return View(await musicas.ToListAsync());
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
        public IActionResult Create()
        {
            PopularDropDowns();
            return View();
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        [Authorize]
        public async Task<IActionResult> Create(Musica musica, IFormFile ficheiroAudio)
        {
            if (ModelState.IsValid)
            {
                if (ficheiroAudio != null && ficheiroAudio.Length > 0)
                {
                    var extensao = Path.GetExtension(ficheiroAudio.FileName);
                    if (extensao.ToLower() != ".mp3")
                    {
                        ModelState.AddModelError("NomeFicheiroAudio", "Apenas ficheiros .mp3 são permitidos.");
                        PopularDropDowns(musica.ArtistaId, musica.GeneroId);
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

            PopularDropDowns(musica.ArtistaId, musica.GeneroId);
            return View(musica);
        }

        [Authorize]
        public async Task<IActionResult> Edit(int? id)
        {
            if (id == null) return NotFound();

            var musica = await _context.Musicas.FindAsync(id);
            if (musica == null) return NotFound();

            PopularDropDowns(musica.ArtistaId, musica.GeneroId);
            return View(musica);
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        [Authorize]
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
                            PopularDropDowns(musica.ArtistaId, musica.GeneroId);
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

            PopularDropDowns(musica.ArtistaId, musica.GeneroId);
            return View(musica);
        }

        [Authorize]
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
        [Authorize]
        public async Task<IActionResult> DeleteConfirmed(int id)
        {
            var musica = await _context.Musicas.FindAsync(id);
            if (musica != null)
            {
                _context.Musicas.Remove(musica);
                await _context.SaveChangesAsync();
            }
            return RedirectToAction(nameof(Index));
        }
    }
}
