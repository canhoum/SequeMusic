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
    /// <summary>
    /// Controlador responsável pela navegação principal da aplicação SequeMusic.
    /// Gera a homepage, pesquisa global, sugestões em tempo real, créditos, privacidade e página de erro.
    /// </summary>
    public class HomeController : Controller
    {
        private readonly ILogger<HomeController> _logger;
        private readonly ApplicationDbContext _context;

        public HomeController(ILogger<HomeController> logger, ApplicationDbContext context)
        {
            _logger = logger;
            _context = context;
        }

        /// <summary>
        /// Página inicial da aplicação.
        /// Mostra as 3 notícias mais recentes e as 10 músicas com mais streamings.
        /// </summary>
        /// <returns>View com notícias e músicas em destaque.</returns>
        public IActionResult Index()
        {
            // Obtem as 3 notícias mais recentes com os respetivos artistas
            var noticias = _context.Noticias
                .Include(n => n.Artista)
                .OrderByDescending(n => n.Data_Publicacao)
                .Take(3)
                .ToList();

            // Obtem as 10 músicas mais populares com base no número de streamings
            var musicas = _context.Musicas
                .Include(m => m.Artista)
                .OrderByDescending(m => m.Streamings.Count)
                .Take(10)
                .ToList();

            ViewBag.Noticias = noticias;
            ViewBag.TopMusicas = musicas;

            return View();
        }

        /// <summary>
        /// Executa uma pesquisa global de artistas e músicas com base no termo fornecido.
        /// </summary>
        /// <param name="query">Termo de pesquisa introduzido pelo utilizador.</param>
        /// <returns>View com os resultados correspondentes.</returns>
        public async Task<IActionResult> Pesquisar(string query)
        {
            if (string.IsNullOrWhiteSpace(query))
                return View("Pesquisar", new PesquisaViewModel());

            // Pesquisa por artistas com nomes que contenham o termo
            var artistas = await _context.Artistas
                .Where(a => a.Nome_Artista.Contains(query))
                .ToListAsync();

            // Pesquisa por músicas com títulos que contenham o termo
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

        /// <summary>
        /// Devolve sugestões em tempo real (autocomplete), em formato JSON.
        /// </summary>
        /// <param name="termo">Termo parcial introduzido pelo utilizador.</param>
        /// <returns>JSON com artistas e músicas correspondentes.</returns>
        [HttpGet]
        public async Task<IActionResult> Sugestoes(string termo)
        {
            if (string.IsNullOrWhiteSpace(termo))
                return Json(new { artistas = new List<object>(), musicas = new List<object>() });

            // Sugestões de artistas
            var artistas = await _context.Artistas
                .Where(a => a.Nome_Artista.ToLower().Contains(termo.ToLower()))
                .Select(a => new { tipo = "artista", id = a.Id, nome = a.Nome_Artista })
                .ToListAsync();

            // Sugestões de músicas
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

        /// <summary>
        /// Página estática com créditos da aplicação.
        /// </summary>
        /// <returns>View com informações sobre os autores/créditos.</returns>
        public IActionResult Creditos()
        {
            return View();
        }

        /// <summary>
        /// Página da política de privacidade.
        /// </summary>
        /// <returns>View de privacidade.</returns>
        public IActionResult Privacy()
        {
            return View();
        }

        /// <summary>
        /// Página genérica de erro (404, 500, etc.).
        /// </summary>
        /// <returns>View com mensagem de erro e código de rastreamento.</returns>
        [ResponseCache(Duration = 0, Location = ResponseCacheLocation.None, NoStore = true)]
        public IActionResult Error()
        {
            return View(new ErrorViewModel
            {
                RequestId = Activity.Current?.Id ?? HttpContext.TraceIdentifier
            });
        }
    }
}
