
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using SequeMusic.Data;
using SequeMusic.Models;
using System.Linq;
using System.Threading.Tasks;

namespace SequeMusic.Controllers
{
    [Authorize]
    public class StreamingController : Controller
    {
        private readonly ApplicationDbContext _context;

        public StreamingController(ApplicationDbContext context)
        {
            _context = context;
        }

        // GET: Musicas/{musicaId}/Streamings
        [AllowAnonymous]
        public async Task<IActionResult> Index(int musicaId)
        {
            var musica = await _context.Musicas
                .Include(m => m.Artista)
                .Include(m => m.Streamings)
                .FirstOrDefaultAsync(m => m.ID == musicaId);

            if (musica == null) return NotFound();

            ViewBag.Musica = musica;
            return View(musica.Streamings.ToList());
        }

        // GET: Musicas/{musicaId}/Streamings/Create
        [Authorize]
        public async Task<IActionResult> Create(int musicaId)
        {
            var user = await _context.Users.FirstOrDefaultAsync(u => u.UserName == User.Identity.Name);
            if (user == null || (!user.IsAdmin && !user.IsPremium))
                return Forbid();

            var musica = await _context.Musicas.FindAsync(musicaId);
            if (musica == null) return NotFound();

            ViewBag.Musica = musica;
            return View(new Streaming { MusicaId = musicaId });
        }

        // POST: Musicas/{musicaId}/Streamings/Create
        [HttpPost]
        [ValidateAntiForgeryToken]
        [Authorize]
        public async Task<IActionResult> Create(Streaming streaming)
        {
            var user = await _context.Users.FirstOrDefaultAsync(u => u.UserName == User.Identity.Name);
            if (user == null || (!user.IsAdmin && !user.IsPremium))
                return Forbid();

            ModelState.Remove("Musica");
            if (ModelState.IsValid)
            {
                _context.Add(streaming);
                await _context.SaveChangesAsync();
                return RedirectToAction("Details", "Musicas", new { id = streaming.MusicaId });
            }

            ViewBag.Musica = await _context.Musicas.FindAsync(streaming.MusicaId);
            return View(streaming);
        }

        // GET: Streamings/Delete/5
        [Authorize(Roles = "Admin")]
        public async Task<IActionResult> Delete(int? id)
        {
            if (id == null) return NotFound();

            var streaming = await _context.Streamings
                .Include(s => s.Musica)
                .FirstOrDefaultAsync(m => m.Id == id);

            if (streaming == null) return NotFound();

            return View(streaming);
        }

        // POST: Streamings/Delete/5
        [HttpPost, ActionName("Delete")]
        [ValidateAntiForgeryToken]
        [Authorize(Roles = "Admin")]
        public async Task<IActionResult> DeleteConfirmed(int id)
        {
            var streaming = await _context.Streamings.FindAsync(id);
            if (streaming != null)
            {
                _context.Streamings.Remove(streaming);
                await _context.SaveChangesAsync();
            }

            return RedirectToAction("Details", "Musicas", new { id = streaming.MusicaId });
        }
    }
}
