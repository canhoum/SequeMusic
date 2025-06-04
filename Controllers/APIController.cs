using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Microsoft.OpenApi.Models;
using SequeMusic.Data;
using SequeMusic.Models;

namespace SequeMusic.Controllers
{
    [ApiController]
    [Route("api/v1/[controller]/[action]")]
    public class APIController : ControllerBase
    {
        private readonly ApplicationDbContext _context;
        private readonly UserManager<Utilizador> _userManager;

        public APIController(ApplicationDbContext context, UserManager<Utilizador> userManager)
        {
            _context = context;
            _userManager = userManager;
        }

        // --- MUSICAS ---
        [HttpGet]
        public async Task<IActionResult> Musicas(string? genero, string? artista, int? ano)
        {
            var query = _context.Musicas.Include(m => m.Artista).Include(m => m.Genero).AsQueryable();
            if (!string.IsNullOrEmpty(genero))
                query = query.Where(m => m.Genero.Nome.Contains(genero));
            if (!string.IsNullOrEmpty(artista))
                query = query.Where(m => m.Artista.Nome_Artista.Contains(artista));
            if (ano.HasValue)
                query = query.Where(m => m.AnoDeLancamento == ano);
            return Ok(await query.ToListAsync());
        }

        [HttpGet("{id}")]
        public async Task<IActionResult> Musica(int id) =>
            Ok(await _context.Musicas.Include(m => m.Artista).Include(m => m.Genero).FirstOrDefaultAsync(m => m.ID == id));

        [HttpPost]
        [Authorize]
        public async Task<IActionResult> CriarMusica([FromBody] Musica musica)
        {
            var user = await _userManager.GetUserAsync(User);
            if (user == null || (!user.IsPremium && !user.IsAdmin))
                return Forbid();

            _context.Musicas.Add(musica);
            await _context.SaveChangesAsync();
            return CreatedAtAction(nameof(Musica), new { id = musica.ID }, musica);
        }

        [HttpPut("{id}")]
        [Authorize(Roles = "Admin")]
        public async Task<IActionResult> AtualizarMusica(int id, [FromBody] Musica musica)
        {
            if (id != musica.ID) return BadRequest();
            _context.Entry(musica).State = EntityState.Modified;
            await _context.SaveChangesAsync();
            return NoContent();
        }

        [HttpDelete("{id}")]
        [Authorize(Roles = "Admin")]
        public async Task<IActionResult> ApagarMusica(int id)
        {
            var musica = await _context.Musicas.FindAsync(id);
            if (musica == null) return NotFound();
            _context.Musicas.Remove(musica);
            await _context.SaveChangesAsync();
            return NoContent();
        }

        // --- ARTISTAS ---
        [HttpGet] public async Task<IActionResult> Artistas() => Ok(await _context.Artistas.ToListAsync());

        [HttpGet("{id}")] public async Task<IActionResult> Artista(int id) =>
            Ok(await _context.Artistas.Include(a => a.Musicas).Include(a => a.Noticias).FirstOrDefaultAsync(a => a.Id == id));

        [HttpPost]
        [Authorize(Roles = "Admin")]
        public async Task<IActionResult> CriarArtista([FromBody] Artista artista)
        {
            _context.Artistas.Add(artista);
            await _context.SaveChangesAsync();
            return CreatedAtAction(nameof(Artista), new { id = artista.Id }, artista);
        }

        [HttpPut("{id}")]
        [Authorize(Roles = "Admin")]
        public async Task<IActionResult> AtualizarArtista(int id, [FromBody] Artista artista)
        {
            if (id != artista.Id) return BadRequest();
            _context.Entry(artista).State = EntityState.Modified;
            await _context.SaveChangesAsync();
            return NoContent();
        }

        [HttpDelete("{id}")]
        [Authorize(Roles = "Admin")]
        public async Task<IActionResult> ApagarArtista(int id)
        {
            var artista = await _context.Artistas.FindAsync(id);
            if (artista == null) return NotFound();
            _context.Artistas.Remove(artista);
            await _context.SaveChangesAsync();
            return NoContent();
        }

        // --- NOTICIAS ---
        [HttpGet] public async Task<IActionResult> Noticias() => Ok(await _context.Noticias.Include(n => n.Artista).ToListAsync());

        [HttpPost]
        [Authorize(Roles = "Admin")]
        public async Task<IActionResult> CriarNoticia([FromBody] Noticia noticia)
        {
            _context.Noticias.Add(noticia);
            await _context.SaveChangesAsync();
            return CreatedAtAction(nameof(Noticias), new { id = noticia.Id }, noticia);
        }

        [HttpPut("{id}")]
        [Authorize(Roles = "Admin")]
        public async Task<IActionResult> AtualizarNoticia(int id, [FromBody] Noticia noticia)
        {
            if (id != noticia.Id) return BadRequest();
            _context.Entry(noticia).State = EntityState.Modified;
            await _context.SaveChangesAsync();
            return NoContent();
        }

        [HttpDelete("{id}")]
        [Authorize(Roles = "Admin")]
        public async Task<IActionResult> ApagarNoticia(int id)
        {
            var noticia = await _context.Noticias.FindAsync(id);
            if (noticia == null) return NotFound();
            _context.Noticias.Remove(noticia);
            await _context.SaveChangesAsync();
            return NoContent();
        }

        // --- GENEROS ---
        [HttpGet] public async Task<IActionResult> Generos() => Ok(await _context.Generos.ToListAsync());

        // --- STREAMINGS ---
        [HttpGet("{musicaId}")]
        public async Task<IActionResult> StreamingsPorMusica(int musicaId) =>
            Ok(await _context.Streamings.Where(s => s.MusicaId == musicaId).ToListAsync());

        // --- AVALIACOES ---
        [HttpGet("{musicaId}")]
        public async Task<IActionResult> AvaliacoesPorMusica(int musicaId) =>
            Ok(await _context.Avaliacoes.Include(a => a.Utilizador).Where(a => a.MusicaId == musicaId).ToListAsync());

        [HttpGet]
        [Authorize]
        public async Task<IActionResult> MinhasAvaliacoes()
        {
            var user = await _userManager.GetUserAsync(User);
            if (user == null) return Unauthorized();
            var avaliacoes = await _context.Avaliacoes.Include(a => a.Musica).Where(a => a.UtilizadorId == user.Id).ToListAsync();
            return Ok(avaliacoes);
        }
    }
}