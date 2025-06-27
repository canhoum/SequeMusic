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
    /// <summary>
    /// Controlador responsável por gerir as avaliações feitas pelos utilizadores autenticados.
    /// </summary>
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

        /// <summary>
        /// Cria uma nova avaliação feita pelo utilizador autenticado.
        /// </summary>
        /// <param name="avaliacao">Objeto da avaliação com ID da música, comentário e nota.</param>
        /// <returns>Redireciona para a página de detalhes da música avaliada ou retorna Unauthorized.</returns>
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

            return RedirectToAction("Details", "Musicas", new { id = avaliacao.MusicaId });
        }

        /// <summary>
        /// Lista todas as avaliações feitas pelo utilizador autenticado.
        /// </summary>
        /// <returns>View com a lista de avaliações do utilizador.</returns>
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
