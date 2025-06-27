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
    /// <summary>
    /// Controlador responsável pela gestão de notícias associadas a artistas.
    /// Apenas administradores podem criar, editar ou eliminar notícias.
    /// </summary>
    public class NoticiaController : Controller
    {
        private readonly ApplicationDbContext _context;

        public NoticiaController(ApplicationDbContext context)
        {
            _context = context;
        }

        /// <summary>
        /// Lista todas as notícias com os respetivos artistas associados.
        /// </summary>
        /// <returns>View com a lista de notícias.</returns>
        public async Task<IActionResult> Index()
        {
            var noticias = await _context.Noticias.Include(n => n.Artista).ToListAsync();
            return View(noticias);
        }

        /// <summary>
        /// Mostra os detalhes de uma notícia específica.
        /// </summary>
        /// <param name="id">ID da notícia.</param>
        /// <returns>View com os detalhes ou NotFound.</returns>
        public async Task<IActionResult> Details(int? id)
        {
            if (id == null) return NotFound();

            var noticia = await _context.Noticias
                .Include(n => n.Artista)
                .FirstOrDefaultAsync(m => m.Id == id);

            if (noticia == null) return NotFound();

            return View(noticia);
        }

        /// <summary>
        /// Mostra o formulário para criar uma nova notícia (apenas Admin).
        /// </summary>
        /// <returns>View com o formulário de criação.</returns>
        [Authorize(Roles = "Admin")]
        public IActionResult Create()
        {
            PreencherArtistasDropdown(); // Preenche dropdown de artistas
            return View();
        }

        /// <summary>
        /// Submete os dados da nova notícia (Admin).
        /// </summary>
        /// <param name="noticia">Objeto da notícia a ser criada.</param>
        /// <returns>Redirect ou View com erros.</returns>
        [HttpPost]
        [ValidateAntiForgeryToken]
        [Authorize(Roles = "Admin")]
        public async Task<IActionResult> Create([Bind("Id,Titulo,Conteudo,Data_Publicacao,Fonte,Resumo,ImagemUrl,ArtistaId")] Noticia noticia)
        {
            ModelState.Remove("Artista");
            if (ModelState.IsValid)
            {
                _context.Add(noticia);
                await _context.SaveChangesAsync();
                return RedirectToAction(nameof(Index));
            }

            PreencherArtistasDropdown(noticia.ArtistaId);
            return View(noticia);
        }

        /// <summary>
        /// Mostra o formulário de edição de uma notícia existente (Admin).
        /// </summary>
        /// <param name="id">ID da notícia.</param>
        /// <returns>View de edição ou NotFound.</returns>
        [Authorize(Roles = "Admin")]
        public async Task<IActionResult> Edit(int? id)
        {
            if (id == null) return NotFound();

            var noticia = await _context.Noticias.FindAsync(id);
            if (noticia == null) return NotFound();

            PreencherArtistasDropdown(noticia.ArtistaId);
            return View(noticia);
        }

        /// <summary>
        /// Submete alterações a uma notícia existente (Admin).
        /// </summary>
        /// <param name="id">ID da notícia.</param>
        /// <param name="noticia">Objeto da notícia atualizado.</param>
        /// <returns>Redirect ou View com erros.</returns>
        [HttpPost]
        [ValidateAntiForgeryToken]
        [Authorize(Roles = "Admin")]
        public async Task<IActionResult> Edit(int id, [Bind("Id,Titulo,Conteudo,Data_Publicacao,Fonte,Resumo,ImagemUrl,ArtistaId")] Noticia noticia)
        {
            if (id != noticia.Id) return NotFound();

            ModelState.Remove("Artista");
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

        /// <summary>
        /// Mostra a confirmação de remoção de uma notícia (Admin).
        /// </summary>
        /// <param name="id">ID da notícia.</param>
        /// <returns>View com os dados da notícia ou NotFound.</returns>
        [Authorize(Roles = "Admin")]
        public async Task<IActionResult> Delete(int? id)
        {
            if (id == null) return NotFound();

            var noticia = await _context.Noticias
                .Include(n => n.Artista)
                .FirstOrDefaultAsync(m => m.Id == id);

            if (noticia == null) return NotFound();

            return View(noticia);
        }

        /// <summary>
        /// Elimina uma notícia da base de dados (Admin).
        /// </summary>
        /// <param name="id">ID da notícia.</param>
        /// <returns>Redireciona para Index.</returns>
        [HttpPost, ActionName("Delete")]
        [ValidateAntiForgeryToken]
        [Authorize(Roles = "Admin")]
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

        /// <summary>
        /// Preenche a dropdown com artistas para usar em formulários.
        /// </summary>
        /// <param name="idSelecionado">ID do artista pré-selecionado (opcional).</param>
        private void PreencherArtistasDropdown(int? idSelecionado = null)
        {
            ViewData["ArtistaId"] = new SelectList(_context.Artistas, "Id", "Nome_Artista", idSelecionado);
        }
    }
}