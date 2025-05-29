using System.Diagnostics;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;
using SequeMusic.Data;
using SequeMusic.Models;
using SequeMusic.ViewModels; // <- Certifique-se que o ViewModel está neste namespace

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

        public IActionResult Index()
        {
            var noticias = _context.Noticias
                .Include(n => n.Artista)
                .OrderByDescending(n => n.Data_Publicacao)
                .Take(3)
                .ToList();

            var musicas = _context.Musicas
                .Include(m => m.Artista)
                .OrderByDescending(m => m.Streamings.Count) // ou .AcessosSemanais se existir
                .Take(10)
                .ToList();

            ViewBag.Noticias = noticias;
            ViewBag.TopMusicas = musicas;

            return View();
        }

        public async Task<IActionResult> Pesquisar(string query)
        {
            if (string.IsNullOrWhiteSpace(query))
            {
                return View("Pesquisar", new PesquisaViewModel());
            }

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
        
        [HttpGet]
        public async Task<IActionResult> Sugestoes(string termo)
        {
            if (string.IsNullOrWhiteSpace(termo))
                return Json(new { artistas = new List<object>(), musicas = new List<object>() });

            var artistas = await _context.Artistas
                .Where(a => a.Nome_Artista.Contains(termo))
                .Select(a => new { tipo = "artista", id = a.Id, nome = a.Nome_Artista })
                .ToListAsync();

            var musicas = await _context.Musicas
                .Include(m => m.Artista)
                .Where(m => m.Titulo.Contains(termo))
                .Select(m => new {
                    tipo = "musica",
                    id = m.ID,
                    titulo = m.Titulo,
                    artista = m.Artista.Nome_Artista
                }).ToListAsync();

            return Json(new { artistas, musicas });
        }



        public IActionResult Privacy()
        {
            return View();
        }

        [ResponseCache(Duration = 0, Location = ResponseCacheLocation.None, NoStore = true)]
        public IActionResult Error()
        {
            return View(new ErrorViewModel { RequestId = Activity.Current?.Id ?? HttpContext.TraceIdentifier });
        }
    }
}
