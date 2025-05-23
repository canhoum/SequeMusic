using Microsoft.AspNetCore.Authorization; 
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Rendering;
using Microsoft.EntityFrameworkCore;
using SequeMusic.Data;
using SequeMusic.Models;
using System.Linq;
using System.Threading.Tasks;

namespace SequeMusic.Controllers
{
    public class NoticiaController : Controller
    {
        private readonly ApplicationDbContext _context;

        public NoticiaController(ApplicationDbContext context)
        {
            _context = context;
        }

        // GET: Noticia
        public async Task<IActionResult> Index()
        {
            var noticias = await _context.Noticias.Include(n => n.Artista).ToListAsync();
            return View(noticias);
        }

        // GET: Noticia/Details/5
        public async Task<IActionResult> Details(int? id)
        {
            if (id == null) return NotFound();

            var noticia = await _context.Noticias
                .Include(n => n.Artista)
                .FirstOrDefaultAsync(m => m.Id == id);

            if (noticia == null) return NotFound();

            return View(noticia);
        }

        // GET: Noticia/Create
        [Authorize]
        public IActionResult Create()
        {
            PreencherArtistasDropdown();
            return View();
        }

        // POST: Noticia/Create
        [HttpPost]
        [ValidateAntiForgeryToken]
        [Authorize]
        public async Task<IActionResult> Create([Bind("Id,Titulo,Conteudo,Data_Publicacao,Fonte,ArtistaId")] Noticia noticia)
        {
            if (ModelState.IsValid)
            {
                _context.Add(noticia);
                await _context.SaveChangesAsync();
                return RedirectToAction(nameof(Index));
            }

            PreencherArtistasDropdown(noticia.ArtistaId);
            return View(noticia);
        }

        // GET: Noticia/Edit/5
        [Authorize]
        public async Task<IActionResult> Edit(int? id)
        {
            if (id == null) return NotFound();

            var noticia = await _context.Noticias.FindAsync(id);
            if (noticia == null) return NotFound();

            PreencherArtistasDropdown(noticia.ArtistaId);
            return View(noticia);
        }

        // POST: Noticia/Edit/5
        [HttpPost]
        [ValidateAntiForgeryToken]
        [Authorize]
        public async Task<IActionResult> Edit(int id, [Bind("Id,Titulo,Conteudo,Data_Publicacao,Fonte,ArtistaId")] Noticia noticia)
        {
            if (id != noticia.Id) return NotFound();

            if (ModelState.IsValid)
            {
                try
                {
                    _context.Update(noticia);
                    await _context.SaveChangesAsync();
                    return RedirectToAction(nameof(Index));
                }
                catch (DbUpdateConcurrencyException)
                {
                    if (!_context.Noticias.Any(e => e.Id == noticia.Id))
                        return NotFound();
                    else
                        throw;
                }
            }

            PreencherArtistasDropdown(noticia.ArtistaId);
            return View(noticia);
        }

        // GET: Noticia/Delete/5
        [Authorize]
        public async Task<IActionResult> Delete(int? id)
        {
            if (id == null) return NotFound();

            var noticia = await _context.Noticias
                .Include(n => n.Artista)
                .FirstOrDefaultAsync(m => m.Id == id);

            if (noticia == null) return NotFound();

            return View(noticia);
        }

        // POST: Noticia/Delete/5
        [HttpPost, ActionName("Delete")]
        [ValidateAntiForgeryToken]
        [Authorize]
        public async Task<IActionResult> DeleteConfirmed(int id)
        {
            var noticia = await _context.Noticias.FindAsync(id);
            if (noticia != null)
            {
                _context.Noticias.Remove(noticia);
                await _context.SaveChangesAsync();
            }
            return RedirectToAction(nameof(Index));
        }

        private void PreencherArtistasDropdown(int? artistaSelecionado = null)
        {
            ViewData["ArtistaId"] = new SelectList(_context.Artistas, "Id", "Nome_Artista", artistaSelecionado);
        }
    }
}