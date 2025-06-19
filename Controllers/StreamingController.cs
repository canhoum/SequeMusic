using Microsoft.AspNetCore.Authorization; // Permite aplicar regras de autorização por função ou identidade
using Microsoft.AspNetCore.Mvc; // Para construir controladores e respostas HTTP
using Microsoft.EntityFrameworkCore; // Permite operações com base de dados usando Entity Framework
using SequeMusic.Data; // Referência ao contexto da base de dados
using SequeMusic.Models; // Referência aos modelos da aplicação
using System.Linq; // Para operações de filtragem e ordenação
using System.Threading.Tasks; // Para suporte a métodos assíncronos

namespace SequeMusic.Controllers
{
    [Authorize] // Todos os métodos requerem autenticação por defeito
    public class StreamingController : Controller
    {
        private readonly ApplicationDbContext _context;

        public StreamingController(ApplicationDbContext context)
        {
            _context = context;
        }

        // Mostra todos os streamings de uma música específica
        [AllowAnonymous] // Permite acesso a utilizadores não autenticados
        public async Task<IActionResult> Index(int musicaId)
        {
            var musica = await _context.Musicas
                .Include(m => m.Artista) // Inclui informação do artista
                .Include(m => m.Streamings) // Inclui lista de streamings
                .FirstOrDefaultAsync(m => m.ID == musicaId); // Procura a música pelo ID

            if (musica == null) return NotFound(); // Se não existir, retorna 404

            ViewBag.Musica = musica; // Envia a música para a view
            return View(musica.Streamings.ToList()); // Envia os streamings para a view
        }

        // Mostra o formulário para criar um novo streaming
        [Authorize] // Apenas utilizadores autenticados
        public async Task<IActionResult> Create(int musicaId)
        {
            var user = await _context.Users.FirstOrDefaultAsync(u => u.UserName == User.Identity.Name);
            if (user == null || (!user.IsAdmin && !user.IsPremium))
                return Forbid(); // Bloqueia se o utilizador não for admin ou premium

            var musica = await _context.Musicas.FindAsync(musicaId);
            if (musica == null) return NotFound(); // Se a música não existir

            ViewBag.Musica = musica; // Envia dados da música para a view
            return View(new Streaming { MusicaId = musicaId }); // Inicializa novo objeto Streaming
        }

        // Submete o formulário para criar um novo streaming
        [HttpPost]
        [ValidateAntiForgeryToken]
        [Authorize]
        public async Task<IActionResult> Create(Streaming streaming)
        {
            var user = await _context.Users.FirstOrDefaultAsync(u => u.UserName == User.Identity.Name);
            if (user == null || (!user.IsAdmin && !user.IsPremium))
                return Forbid(); // Só admin ou premium podem submeter

            ModelState.Remove("Musica"); // Ignora a validação automática do campo "Musica" (nav.prop)

            if (ModelState.IsValid)
            {
                _context.Add(streaming); // Adiciona o streaming à BD
                await _context.SaveChangesAsync(); // Guarda as alterações
                return RedirectToAction("Details", "Musicas", new { id = streaming.MusicaId }); // Redireciona para detalhes da música
            }

            ViewBag.Musica = await _context.Musicas.FindAsync(streaming.MusicaId);
            return View(streaming); // Volta ao formulário com erros
        }

        // Mostra o formulário de confirmação para eliminar um streaming
        [Authorize(Roles = "Admin")] // Apenas administradores podem eliminar
        public async Task<IActionResult> Delete(int? id)
        {
            if (id == null) return NotFound();

            var streaming = await _context.Streamings
                .Include(s => s.Musica)
                .FirstOrDefaultAsync(m => m.Id == id);

            if (streaming == null) return NotFound();

            return View(streaming); // Mostra confirmação da eliminação
        }

        // Submete a eliminação do streaming
        [HttpPost, ActionName("Delete")]
        [ValidateAntiForgeryToken]
        [Authorize(Roles = "Admin")] // Apenas administradores
        public async Task<IActionResult> DeleteConfirmed(int id)
        {
            var streaming = await _context.Streamings.FindAsync(id);
            if (streaming != null)
            {
                _context.Streamings.Remove(streaming); // Remove da BD
                await _context.SaveChangesAsync(); // Guarda alterações
            }

            return RedirectToAction("Details", "Musicas", new { id = streaming.MusicaId }); // Volta aos detalhes da música
        }
    }
}
