using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Rendering;
using Microsoft.EntityFrameworkCore;
using SequeMusic.Data;
using SequeMusic.Models;
using System.IO;
using System.Linq;
using System.Threading.Tasks;
using System;

namespace SequeMusic.Controllers
{
    public class ArtistasController : Controller
    {
        private readonly ApplicationDbContext _context;

        public ArtistasController(ApplicationDbContext context)
        {
            _context = context;
        }

        // GET: Artistas
        public async Task<IActionResult> Index()
        {
            return View(await _context.Artistas.ToListAsync());
        }

        // GET: Artistas/Details/5
        public async Task<IActionResult> Details(int? id)
        {
            if (id == null) return NotFound();

            var artista = await _context.Artistas
                .Include(a => a.Musicas)
                .Include(a => a.Noticias)
                .FirstOrDefaultAsync(a => a.Id == id);

            if (artista == null) return NotFound();

            return View(artista);
        }

        // GET: Artistas/Create
        [Authorize(Roles = "Admin")]
        public IActionResult Create()
        {
            return View();
        }

        // POST: Artistas/Create
        [HttpPost]
        [ValidateAntiForgeryToken]
        [Authorize(Roles = "Admin")]
        public async Task<IActionResult> Create(Artista artista, IFormFile fotoUpload)
        {
            if (ModelState.IsValid)
            {
                var nomeFicheiro = await GuardarImagemAsync(fotoUpload);
                if (nomeFicheiro == null && fotoUpload != null)
                {
                    ModelState.AddModelError("Foto", "Só são permitidas imagens JPG, PNG ou GIF.");
                    return View(artista);
                }

                if (nomeFicheiro != null)
                    artista.Foto = nomeFicheiro;

                _context.Add(artista);
                await _context.SaveChangesAsync();
                return RedirectToAction(nameof(Index));
            }
            return View(artista);
        }

        // GET: Artistas/Edit/5
        [Authorize(Roles = "Admin")]
        public async Task<IActionResult> Edit(int? id)
        {
            if (id == null) return NotFound();

            var artista = await _context.Artistas.FindAsync(id);
            if (artista == null) return NotFound();

            return View(artista);
        }

        // POST: Artistas/Edit/5
        [HttpPost]
        [ValidateAntiForgeryToken]
        [Authorize(Roles = "Admin")]
        public async Task<IActionResult> Edit(int id, Artista artista, IFormFile fotoUpload)
        {
            if (id != artista.Id) return NotFound();

            if (ModelState.IsValid)
            {
                try
                {
                    var nomeFicheiro = await GuardarImagemAsync(fotoUpload);
                    if (nomeFicheiro == null && fotoUpload != null)
                    {
                        ModelState.AddModelError("Foto", "Só são permitidas imagens JPG, PNG ou GIF.");
                        return View(artista);
                    }

                    if (nomeFicheiro != null)
                        artista.Foto = nomeFicheiro;

                    _context.Update(artista);
                    await _context.SaveChangesAsync();
                }
                catch (DbUpdateConcurrencyException)
                {
                    if (!_context.Artistas.Any(e => e.Id == id)) return NotFound();
                    else throw;
                }
                return RedirectToAction(nameof(Index));
            }
            return View(artista);
        }

        // GET: Artistas/Delete/5
        [Authorize(Roles = "Admin")]
        public async Task<IActionResult> Delete(int? id)
        {
            if (id == null) return NotFound();

            var artista = await _context.Artistas
                .FirstOrDefaultAsync(a => a.Id == id);

            if (artista == null) return NotFound();

            return View(artista);
        }

        // POST: Artistas/Delete/5
        [HttpPost, ActionName("Delete")]
        [ValidateAntiForgeryToken]
        [Authorize(Roles = "Admin")]
        public async Task<IActionResult> DeleteConfirmed(int id)
        {
            var artista = await _context.Artistas.FindAsync(id);
            _context.Artistas.Remove(artista);
            await _context.SaveChangesAsync();
            return RedirectToAction(nameof(Index));
        }

        // Método privado para guardar imagem
        private async Task<string> GuardarImagemAsync(IFormFile ficheiro)
        {
            if (ficheiro != null && ficheiro.Length > 0)
            {
                var extensao = Path.GetExtension(ficheiro.FileName).ToLower();
                var permitidas = new[] { ".jpg", ".jpeg", ".png", ".gif" };

                if (!permitidas.Contains(extensao)) return null;

                var nomeUnico = Guid.NewGuid().ToString() + extensao;
                var caminho = Path.Combine(Directory.GetCurrentDirectory(), "wwwroot", "uploads", nomeUnico);

                using var stream = new FileStream(caminho, FileMode.Create);
                await ficheiro.CopyToAsync(stream);

                return nomeUnico;
            }
            return null;
        }
    }
}
