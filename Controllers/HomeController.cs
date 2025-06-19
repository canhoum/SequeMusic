// Controlador responsável pela navegação principal da aplicação SequeMusic
// Gera a homepage, pesquisa, sugestões em tempo real, créditos, privacidade e página de erro

using System.Diagnostics;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;
using SequeMusic.Data;
using SequeMusic.Models;
using SequeMusic.ViewModels;

namespace SequeMusic.Controllers
{
    public class HomeController : Controller
    {
        private readonly ILogger<HomeController> _logger;
        private readonly ApplicationDbContext _context;

        public HomeController(ILogger<HomeController> logger, ApplicationDbContext context)
        {
            _logger = logger;
            _context = context;
        }

        // Página inicial da aplicação
        // Mostra as 3 notícias mais recentes e as 10 músicas com mais streamings
        public IActionResult Index()
        {
            var noticias = _context.Noticias
                .Include(n => n.Artista)
                .OrderByDescending(n => n.Data_Publicacao)
                .Take(3)
                .ToList();

            var musicas = _context.Musicas
                .Include(m => m.Artista)
                .OrderByDescending(m => m.Streamings.Count)
                .Take(10)
                .ToList();

            ViewBag.Noticias = noticias;
            ViewBag.TopMusicas = musicas;

            return View();
        }

        // Página de pesquisa global
        // Procura por artistas e músicas com base no termo introduzido pelo utilizador
        public async Task<IActionResult> Pesquisar(string query)
        {
            if (string.IsNullOrWhiteSpace(query))
                return View("Pesquisar", new PesquisaViewModel());

            var artistas = await _context.Artistas
                .Where(a => a.Nome_Artista.Contains(query))
                .ToListAsync();

            var musicas = await _context.Musicas
                .Include(m => m.Artista)
                .Where(m => m.Titulo.Contains(query))
                .ToListAsync();

            var viewModel = new PesquisaViewModel
            {
                Query = query,
                Artistas = artistas,
                Musicas = musicas
            };

            return View("Pesquisar", viewModel);
        }

        // Endpoint para sugestões automáticas (autocomplete via AJAX)
        // Devolve artistas e músicas em formato JSON com base no termo
        [HttpGet]
        public async Task<IActionResult> Sugestoes(string termo)
        {
            if (string.IsNullOrWhiteSpace(termo))
                return Json(new { artistas = new List<object>(), musicas = new List<object>() });

            var artistas = await _context.Artistas
                .Where(a => a.Nome_Artista.ToLower().Contains(termo.ToLower()))
                .Select(a => new { tipo = "artista", id = a.Id, nome = a.Nome_Artista })
                .ToListAsync();

            var musicas = await _context.Musicas
                .Include(m => m.Artista)
                .Where(m => m.Titulo.ToLower().Contains(termo.ToLower()))
                .Select(m => new {
                    tipo = "musica",
                    id = m.ID,
                    titulo = m.Titulo,
                    artista = m.Artista.Nome_Artista
                }).ToListAsync();

            return Json(new { artistas, musicas });
        }

        // Página estática com créditos da aplicação
        public IActionResult Creditos()
        {
            return View();
        }

        // Página da política de privacidade
        public IActionResult Privacy()
        {
            return View();
        }

        // Página de erro genérica (ex: 404, 500)
        [ResponseCache(Duration = 0, Location = ResponseCacheLocation.None, NoStore = true)]
        public IActionResult Error()
        {
            return View(new ErrorViewModel { RequestId = Activity.Current?.Id ?? HttpContext.TraceIdentifier });
        }
    }
}
