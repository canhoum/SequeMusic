using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using SequeMusic.Data;
using SequeMusic.Models;
using System;
using System.Linq;
using System.Threading.Tasks;

namespace SequeMusic.Controllers
{
    [Authorize]
    public class AvaliacaoController : Controller
    {
        private readonly ApplicationDbContext _context;
        private readonly UserManager<Utilizador> _userManager;

        public AvaliacaoController(ApplicationDbContext context, UserManager<Utilizador> userManager)
        {
            _context = context;
            _userManager = userManager;
        }

        // POST: Avaliacao/Create
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Create([Bind("MusicaId,Comentario,Nota")] Avaliacao avaliacao)
        {
            var user = await _userManager.GetUserAsync(User);
            if (user == null) return Unauthorized();

            avaliacao.UtilizadorId = user.Id;
            avaliacao.Data_Avaliacao = DateTime.Now;

            _context.Avaliacoes.Add(avaliacao);
            await _context.SaveChangesAsync();

            return RedirectToAction("Details", "Musicas", new { id = avaliacao.MusicaId });
        }

        // GET: Avaliacao/Utilizador
        public async Task<IActionResult> Utilizador()
        {
            var user = await _userManager.GetUserAsync(User);
            if (user == null) return Unauthorized();

            var avaliacoes = await _context.Avaliacoes
                .Include(a => a.Musica)
                .Where(a => a.UtilizadorId == user.Id)
                .ToListAsync();

            return View(avaliacoes);
        }
    }
}