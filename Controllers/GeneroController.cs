// Controlador MVC responsável pela gestão dos géneros musicais
// Permite listar, visualizar, criar, editar e eliminar géneros

using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using SequeMusic.Data;
using SequeMusic.Models;
using System.Linq;
using System.Threading.Tasks;

namespace SequeMusic.Controllers
{
    public class GenerosController : Controller
    {
        private readonly ApplicationDbContext _context;

        public GenerosController(ApplicationDbContext context)
        {
            _context = context;
        }

        // Lista todos os géneros disponíveis
        public async Task<IActionResult> Index()
        {
            return View(await _context.Generos.ToListAsync());
        }

        // Mostra os detalhes de um género específico, incluindo as músicas associadas
        public async Task<IActionResult> Details(int? id)
        {
            if (id == null)
                return NotFound();

            var genero = await _context.Generos
                .Include(g => g.Musicas) // Inclui músicas associadas
                .FirstOrDefaultAsync(m => m.Id == id);

            if (genero == null)
                return NotFound();

            return View(genero);
        }

        // Apresenta o formulário para criar um novo género
        public IActionResult Create()
        {
            return View();
        }

        // Submete o novo género para a base de dados
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

        // Apresenta o formulário de edição de um género existente
        public async Task<IActionResult> Edit(int? id)
        {
            if (id == null)
                return NotFound();

            var genero = await _context.Generos.FindAsync(id);
            if (genero == null)
                return NotFound();

            return View(genero);
        }

        // Submete a edição do género para a base de dados
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

        // Mostra a confirmação para apagar um género
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

        // Confirma e executa a eliminação do género
        [HttpPost, ActionName("Delete")]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> DeleteConfirmed(int id)
        {
            var genero = await _context.Generos.FindAsync(id);
            _context.Generos.Remove(genero);
            await _context.SaveChangesAsync();
            return RedirectToAction(nameof(Index));
        }

        // Verifica se um género com o ID fornecido existe na base de dados
        private bool GeneroExists(int id)
        {
            return _context.Generos.Any(e => e.Id == id);
        }
    }
}
