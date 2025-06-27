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

        /// <summary>
        /// Lista todos os artistas existentes na base de dados.
        /// </summary>
        /// <returns>View com a lista de artistas.</returns>
        public async Task<IActionResult> Index()
        {
            return View(await _context.Artistas.ToListAsync());
        }

        /// <summary>
        /// Mostra os detalhes de um artista, incluindo músicas e notícias associadas.
        /// </summary>
        /// <param name="id">ID do artista.</param>
        /// <returns>View com os detalhes ou NotFound se não existir.</returns>
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

        /// <summary>
        /// Mostra o formulário de criação de um novo artista (Admin).
        /// </summary>
        /// <returns>View de criação.</returns>
        [Authorize(Roles = "Admin")]
        public IActionResult Create()
        {
            return View();
        }

        /// <summary>
        /// Submete o novo artista preenchido no formulário (Admin).
        /// </summary>
        /// <param name="artista">Dados do artista.</param>
        /// <param name="fotoUpload">Ficheiro da imagem enviada.</param>
        /// <returns>Redirect para Index ou View com erros.</returns>
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

        /// <summary>
        /// Mostra o formulário de edição de um artista (Admin).
        /// </summary>
        /// <param name="id">ID do artista.</param>
        /// <returns>View com dados do artista ou NotFound.</returns>
        [Authorize(Roles = "Admin")]
        public async Task<IActionResult> Edit(int? id)
        {
            if (id == null) return NotFound();

            var artista = await _context.Artistas.FindAsync(id);
            if (artista == null) return NotFound();

            return View(artista);
        }

        /// <summary>
        /// Submete as alterações feitas ao artista (Admin).
        /// </summary>
        /// <param name="id">ID do artista.</param>
        /// <param name="artista">Dados atualizados do artista.</param>
        /// <param name="fotoUpload">Nova imagem enviada (opcional).</param>
        /// <returns>Redirect para Index ou View com erros.</returns>
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

        /// <summary>
        /// Mostra a confirmação de remoção de um artista (Admin).
        /// </summary>
        /// <param name="id">ID do artista a remover.</param>
        /// <returns>View com dados do artista ou NotFound.</returns>
        [Authorize(Roles = "Admin")]
        public async Task<IActionResult> Delete(int? id)
        {
            if (id == null) return NotFound();

            var artista = await _context.Artistas.FirstOrDefaultAsync(a => a.Id == id);
            if (artista == null) return NotFound();

            return View(artista);
        }

        /// <summary>
        /// Elimina definitivamente um artista (Admin).
        /// </summary>
        /// <param name="id">ID do artista.</param>
        /// <returns>Redirect para Index após apagar.</returns>
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

        /// <summary>
        /// Função auxiliar para validar e guardar imagem de artista em disco.
        /// </summary>
        /// <param name="ficheiro">Ficheiro recebido via form.</param>
        /// <returns>Nome do ficheiro guardado ou null se inválido.</returns>
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
