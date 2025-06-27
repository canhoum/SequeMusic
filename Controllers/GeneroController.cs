using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using SequeMusic.Data;
using SequeMusic.Models;
using System.Linq;
using System.Threading.Tasks;

namespace SequeMusic.Controllers
{
    /// <summary>
    /// Controller responsável pela gestão dos géneros musicais.
    /// Permite listar, criar, editar, visualizar detalhes e eliminar géneros.
    /// </summary>
    public class GenerosController : Controller
    {
        private readonly ApplicationDbContext _context;

        public GenerosController(ApplicationDbContext context)
        {
            _context = context;
        }

        /// <summary>
        /// Lista todos os géneros disponíveis na base de dados.
        /// </summary>
        /// <returns>View com a lista de géneros.</returns>
        public async Task<IActionResult> Index()
        {
            return View(await _context.Generos.ToListAsync());
        }

        /// <summary>
        /// Mostra os detalhes de um género específico, incluindo as músicas associadas.
        /// </summary>
        /// <param name="id">ID do género.</param>
        /// <returns>View com os detalhes do género ou NotFound se não existir.</returns>
        public async Task<IActionResult> Details(int? id)
        {
            if (id == null)
                return NotFound();

            var genero = await _context.Generos
                .Include(g => g.Musicas)
                .FirstOrDefaultAsync(m => m.Id == id);

            if (genero == null)
                return NotFound();

            return View(genero);
        }

        /// <summary>
        /// Apresenta o formulário para criar um novo género musical.
        /// </summary>
        /// <returns>View com o formulário de criação.</returns>
        public IActionResult Create()
        {
            return View();
        }

        /// <summary>
        /// Submete os dados de um novo género musical.
        /// </summary>
        /// <param name="genero">Objeto com os dados do género.</param>
        /// <returns>Redireciona para Index ou retorna a View com erros de validação.</returns>
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Create([Bind("Id,Nome,Descricao")] Genero genero)
        {
            if (ModelState.IsValid)
            {
                _context.Add(genero);
                await _context.SaveChangesAsync();
                return RedirectToAction(nameof(Index));
            }
            return View(genero);
        }

        /// <summary>
        /// Apresenta o formulário de edição de um género existente.
        /// </summary>
        /// <param name="id">ID do género a editar.</param>
        /// <returns>View de edição ou NotFound se não existir.</returns>
        public async Task<IActionResult> Edit(int? id)
        {
            if (id == null)
                return NotFound();

            var genero = await _context.Generos.FindAsync(id);
            if (genero == null)
                return NotFound();

            return View(genero);
        }

        /// <summary>
        /// Submete as alterações feitas ao género musical.
        /// </summary>
        /// <param name="id">ID do género.</param>
        /// <param name="genero">Objeto atualizado do género.</param>
        /// <returns>Redirect para Index ou View com erros.</returns>
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Edit(int id, [Bind("Id,Nome,Descricao")] Genero genero)
        {
            if (id != genero.Id)
                return NotFound();

            if (ModelState.IsValid)
            {
                try
                {
                    _context.Update(genero);
                    await _context.SaveChangesAsync();
                }
                catch (DbUpdateConcurrencyException)
                {
                    if (!GeneroExists(genero.Id))
                        return NotFound();
                    else
                        throw;
                }
                return RedirectToAction(nameof(Index));
            }
            return View(genero);
        }

        /// <summary>
        /// Mostra a confirmação para apagar um género.
        /// </summary>
        /// <param name="id">ID do género.</param>
        /// <returns>View de confirmação ou NotFound se o género não existir.</returns>
        public async Task<IActionResult> Delete(int? id)
        {
            if (id == null)
                return NotFound();

            var genero = await _context.Generos
                .FirstOrDefaultAsync(m => m.Id == id);
            if (genero == null)
                return NotFound();

            return View(genero);
        }

        /// <summary>
        /// Elimina definitivamente um género da base de dados.
        /// </summary>
        /// <param name="id">ID do género a eliminar.</param>
        /// <returns>Redireciona para Index após a eliminação.</returns>
        [HttpPost, ActionName("Delete")]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> DeleteConfirmed(int id)
        {
            var genero = await _context.Generos.FindAsync(id);
            _context.Generos.Remove(genero);
            await _context.SaveChangesAsync();
            return RedirectToAction(nameof(Index));
        }

        /// <summary>
        /// Verifica se existe um género com o ID especificado.
        /// </summary>
        /// <param name="id">ID do género a verificar.</param>
        /// <returns>True se existir, false caso contrário.</returns>
        private bool GeneroExists(int id)
        {
            return _context.Generos.Any(e => e.Id == id);
        }
    }
}
