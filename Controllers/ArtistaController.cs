// Controlador MVC para gestão de artistas na aplicação
// Permite listar, visualizar, criar, editar e apagar artistas
// Algumas ações estão protegidas com [Authorize(Roles = "Admin")]

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

        // Lista todos os artistas existentes na base de dados
        public async Task<IActionResult> Index()
        {
            return View(await _context.Artistas.ToListAsync());
        }

        // Mostra os detalhes de um artista, incluindo as músicas e notícias associadas
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

        // Mostra o formulário para criar um novo artista (acesso restrito a administradores)
        [Authorize(Roles = "Admin")]
        public IActionResult Create()
        {
            return View();
        }

        // Submete o novo artista preenchido no formulário (Admin)
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

        // Mostra o formulário de edição de um artista (Admin)
        [Authorize(Roles = "Admin")]
        public async Task<IActionResult> Edit(int? id)
        {
            if (id == null) return NotFound();

            var artista = await _context.Artistas.FindAsync(id);
            if (artista == null) return NotFound();

            return View(artista);
        }

        // Submete as alterações feitas a um artista (Admin)
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

        // Mostra a confirmação antes de eliminar um artista (Admin)
        [Authorize(Roles = "Admin")]
        public async Task<IActionResult> Delete(int? id)
        {
            if (id == null) return NotFound();

            var artista = await _context.Artistas.FirstOrDefaultAsync(a => a.Id == id);
            if (artista == null) return NotFound();

            return View(artista);
        }

        // Elimina definitivamente o artista da base de dados (Admin)
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

        // Função auxiliar para guardar a imagem do artista (validação + gravação em wwwroot/uploads)
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
