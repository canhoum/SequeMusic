using System.Diagnostics;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;
using SequeMusic.Data;
using SequeMusic.Models;

namespace SequeMusic.Controllers;

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
            .OrderByDescending(m => m.Streamings.Count) // ou .AcessosSemanais se tiveres essa propriedade
            .Take(10)
            .ToList();

        ViewBag.Noticias = noticias;
        ViewBag.TopMusicas = musicas;

        return View();
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