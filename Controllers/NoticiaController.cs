using Microsoft.AspNetCore.Authorization; // Gere controlo de acesso com base em roles (ex: Admin)
using Microsoft.AspNetCore.Mvc; // Funcionalidades base dos controladores MVC
using Microsoft.AspNetCore.Mvc.Rendering; // Permite criar listas para dropdowns
using Microsoft.EntityFrameworkCore; // Suporte para operações com base de dados via Entity Framework
using SequeMusic.Data; // Contexto da base de dados
using SequeMusic.Models; // Modelos usados nesta aplicação
using System.Linq; // Para usar métodos LINQ (ex: Any, FirstOrDefault)
using System.Threading.Tasks; // Para métodos assíncronos

namespace SequeMusic.Controllers
{
    public class NoticiaController : Controller
    {
        private readonly ApplicationDbContext _context; // Contexto da BD para interagir com dados

        public NoticiaController(ApplicationDbContext context)
        {
            _context = context;
        }

        // GET: Noticia
        // Lista todas as notícias, incluindo os dados do artista associado
        public async Task<IActionResult> Index()
        {
            var noticias = await _context.Noticias.Include(n => n.Artista).ToListAsync();
            return View(noticias);
        }

        // GET: Noticia/Details/5
        // Mostra os detalhes de uma notícia específica
        public async Task<IActionResult> Details(int? id)
        {
            if (id == null) return NotFound(); // ID inválido

            var noticia = await _context.Noticias
                .Include(n => n.Artista)
                .FirstOrDefaultAsync(m => m.Id == id);

            if (noticia == null) return NotFound(); // Notícia não encontrada

            return View(noticia);
        }

        // GET: Noticia/Create
        // Apenas administradores podem criar notícias
        [Authorize(Roles = "Admin")]
        public IActionResult Create()
        {
            PreencherArtistasDropdown(); // Preenche dropdown de artistas
            return View();
        }

        // POST: Noticia/Create
        // Guarda nova notícia na base de dados
        [HttpPost]
        [ValidateAntiForgeryToken]
        [Authorize(Roles = "Admin")]
        public async Task<IActionResult> Create([Bind("Id,Titulo,Conteudo,Data_Publicacao,Fonte,Resumo,ImagemUrl,ArtistaId")] Noticia noticia)
        {
            ModelState.Remove("Artista"); // Ignora validação automática de navegação
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
        // Editar uma notícia existente (admin)
        [Authorize(Roles = "Admin")]
        public async Task<IActionResult> Edit(int? id)
        {
            if (id == null) return NotFound();

            var noticia = await _context.Noticias.FindAsync(id);
            if (noticia == null) return NotFound();

            PreencherArtistasDropdown(noticia.ArtistaId);
            return View(noticia);
        }

        // POST: Noticia/Edit/5
        // Guarda alterações feitas a uma notícia existente
        [HttpPost]
        [ValidateAntiForgeryToken]
        [Authorize(Roles = "Admin")]
        public async Task<IActionResult> Edit(int id, [Bind("Id,Titulo,Conteudo,Data_Publicacao,Fonte,Resumo,ImagemUrl,ArtistaId")] Noticia noticia)
        {
            if (id != noticia.Id) return NotFound(); // ID da rota não coincide com o da entidade

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
                        throw; // Outro erro de concorrência
                }
            }

            PreencherArtistasDropdown(noticia.ArtistaId);
            return View(noticia);
        }

        // GET: Noticia/Delete/5
        // Confirmação de remoção de uma notícia
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

        // POST: Noticia/Delete/5
        // Elimina uma notícia da base de dados
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

        // Método auxiliar para preencher dropdown com artistas
        private void PreencherArtistasDropdown(int? artistaSelecionado = null)
        {
            ViewData["ArtistaId"] = new SelectList(_context.Artistas, "Id", "Nome_Artista", artistaSelecionado);
        }
    }
}
