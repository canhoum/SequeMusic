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
    [Authorize] // Garante que apenas utilizadores autenticados podem aceder às ações deste controlador
    public class AvaliacaoController : Controller
    {
        private readonly ApplicationDbContext _context;
        private readonly UserManager<Utilizador> _userManager;

        public AvaliacaoController(ApplicationDbContext context, UserManager<Utilizador> userManager)
        {
            _context = context;
            _userManager = userManager;
        }

        // Cria uma nova avaliação submetida pelo utilizador autenticado
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Create([Bind("MusicaId,Comentario,Nota")] Avaliacao avaliacao)
        {
            var user = await _userManager.GetUserAsync(User);
            if (user == null) return Unauthorized();

            avaliacao.UtilizadorId = user.Id; // Associa o ID do utilizador à avaliação
            avaliacao.Data_Avaliacao = DateTime.Now; // Define a data atual

            _context.Avaliacoes.Add(avaliacao);
            await _context.SaveChangesAsync();

            // Redireciona para a página de detalhes da música após avaliação
            return RedirectToAction("Details", "Musicas", new { id = avaliacao.MusicaId });
        }

        // Mostra todas as avaliações feitas pelo utilizador autenticado
        public async Task<IActionResult> Utilizador()
        {
            var user = await _userManager.GetUserAsync(User);
            if (user == null) return Unauthorized();

            var avaliacoes = await _context.Avaliacoes
                .Include(a => a.Musica) // Inclui dados da música associada
                .Where(a => a.UtilizadorId == user.Id) // Filtra pelo utilizador atual
                .ToListAsync();

            return View(avaliacoes);
        }
    }
}
