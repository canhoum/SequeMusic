using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using SequeMusic.Data;
using SequeMusic.Models;
using System.Linq;
using System.Threading.Tasks;

namespace SequeMusic.Controllers
{
    /// <summary>
    /// Controlador responsável pela gestão de streamings de músicas.
    /// Apenas administradores e utilizadores premium podem criar ou eliminar streamings.
    /// </summary>
    [Authorize]
    public class StreamingController : Controller
    {
        private readonly ApplicationDbContext _context;

        public StreamingController(ApplicationDbContext context)
        {
            _context = context;
        }

        /// <summary>
        /// Lista todos os streamings associados a uma música específica.
        /// </summary>
        /// <param name="musicaId">ID da música.</param>
        /// <returns>View com a lista de streamings ou NotFound.</returns>
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

        /// <summary>
        /// Mostra o formulário para criar um novo streaming.
        /// Apenas utilizadores Premium ou Admin podem aceder.
        /// </summary>
        /// <param name="musicaId">ID da música a associar o streaming.</param>
        /// <returns>View de criação ou Forbid/NotFound.</returns>
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

        /// <summary>
        /// Submete um novo streaming para a base de dados.
        /// Apenas utilizadores Premium/Admin podem criar.
        /// </summary>
        /// <param name="streaming">Objeto do streaming preenchido no formulário.</param>
        /// <returns>Redirect para detalhes da música ou View com erros.</returns>
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Create(Streaming streaming)
        {
            var user = await _context.Users.FirstOrDefaultAsync(u => u.UserName == User.Identity.Name);
            if (user == null || (!user.IsAdmin && !user.IsPremium))
                return Forbid();

            ModelState.Remove("Musica"); // Ignora validação da propriedade de navegação

            if (ModelState.IsValid)
            {
                _context.Add(streaming);
                await _context.SaveChangesAsync();
                return RedirectToAction("Details", "Musicas", new { id = streaming.MusicaId });
            }

            ViewBag.Musica = await _context.Musicas.FindAsync(streaming.MusicaId);
            return View(streaming);
        }

        /// <summary>
        /// Mostra a página de confirmação para apagar um streaming.
        /// Apenas administradores têm acesso.
        /// </summary>
        /// <param name="id">ID do streaming.</param>
        /// <returns>View com os dados do streaming ou NotFound.</returns>
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

        /// <summary>
        /// Submete a eliminação definitiva de um streaming (Admin).
        /// </summary>
        /// <param name="id">ID do streaming a eliminar.</param>
        /// <returns>Redirect para detalhes da música após eliminação.</returns>
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
