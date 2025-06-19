// Controlador API REST para a aplicação SequeMusic
// Fornece endpoints protegidos por JWT para gerir músicas, artistas, géneros, notícias, streamings e avaliações

using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Microsoft.OpenApi.Models;
using SequeMusic.Data;
using SequeMusic.Models;
using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;
using Microsoft.IdentityModel.Tokens;
using System.Text;
using Microsoft.AspNetCore.Authentication.JwtBearer;

namespace SequeMusic.Controllers
{
    [ApiController]
    [Route("api/v1/[controller]/[action]")]
    public class APIController : ControllerBase
    {
        // Base de dados e gestão de utilizadores
        private readonly ApplicationDbContext _context;
        private readonly UserManager<Utilizador> _userManager;

        public APIController(ApplicationDbContext context, UserManager<Utilizador> userManager)
        {
            _context = context;
            _userManager = userManager;
        }

        // ------------------- MUSICAS -------------------

        // Lista todas as músicas com filtros opcionais por género, artista e ano
        [HttpGet]
        public async Task<IActionResult> Musicas(string? genero, string? artista, int? ano)
        {
            var query = _context.Musicas.Include(m => m.Artista).Include(m => m.Genero).AsQueryable();
            if (!string.IsNullOrEmpty(genero)) query = query.Where(m => m.Genero.Nome.Contains(genero));
            if (!string.IsNullOrEmpty(artista)) query = query.Where(m => m.Artista.Nome_Artista.Contains(artista));
            if (ano.HasValue) query = query.Where(m => m.AnoDeLancamento == ano);
            return Ok(await query.ToListAsync());
        }

        // Devolve os detalhes de uma música por ID
        [HttpGet("{id}")]
        public async Task<IActionResult> Musica(int id) =>
            Ok(await _context.Musicas.Include(m => m.Artista).Include(m => m.Genero).FirstOrDefaultAsync(m => m.ID == id));

        // Criar nova música (acesso reservado a utilizadores Premium ou Admin)
        [HttpPost]
        [Authorize(AuthenticationSchemes = JwtBearerDefaults.AuthenticationScheme)]
        public async Task<IActionResult> CriarMusica([FromBody] Musica musica)
        {
            var user = await _userManager.GetUserAsync(User);
            if (user == null || (!user.IsPremium && !user.IsAdmin)) return Forbid();
            _context.Musicas.Add(musica);
            await _context.SaveChangesAsync();
            return CreatedAtAction(nameof(Musica), new { id = musica.ID }, musica);
        }

        // Atualizar música (apenas Admin)
        [HttpPut("{id}")]
        [Authorize(Roles = "Admin", AuthenticationSchemes = JwtBearerDefaults.AuthenticationScheme)]
        public async Task<IActionResult> AtualizarMusica(int id, [FromBody] Musica musica)
        {
            if (id != musica.ID) return BadRequest();
            _context.Entry(musica).State = EntityState.Modified;
            await _context.SaveChangesAsync();
            return NoContent();
        }

        // Apagar música (apenas Admin)
        [HttpDelete("{id}")]
        [Authorize(Roles = "Admin", AuthenticationSchemes = JwtBearerDefaults.AuthenticationScheme)]
        public async Task<IActionResult> ApagarMusica(int id)
        {
            var musica = await _context.Musicas.FindAsync(id);
            if (musica == null) return NotFound();
            _context.Musicas.Remove(musica);
            await _context.SaveChangesAsync();
            return NoContent();
        }

        // ------------------- ARTISTAS -------------------

        // Lista todos os artistas
        [HttpGet]
        public async Task<IActionResult> Artistas() => Ok(await _context.Artistas.ToListAsync());

        // Detalhes de um artista com respetivas músicas e notícias
        [HttpGet("{id}")]
        public async Task<IActionResult> Artista(int id) =>
            Ok(await _context.Artistas.Include(a => a.Musicas).Include(a => a.Noticias).FirstOrDefaultAsync(a => a.Id == id));

        // Criar artista (apenas Admin)
        [HttpPost]
        [Authorize(Roles = "Admin", AuthenticationSchemes = JwtBearerDefaults.AuthenticationScheme)]
        public async Task<IActionResult> CriarArtista([FromBody] Artista artista)
        {
            _context.Artistas.Add(artista);
            await _context.SaveChangesAsync();
            return CreatedAtAction(nameof(Artista), new { id = artista.Id }, artista);
        }

        // Atualizar artista (Admin)
        [HttpPut("{id}")]
        [Authorize(Roles = "Admin", AuthenticationSchemes = JwtBearerDefaults.AuthenticationScheme)]
        public async Task<IActionResult> AtualizarArtista(int id, [FromBody] Artista artista)
        {
            if (id != artista.Id) return BadRequest();
            _context.Entry(artista).State = EntityState.Modified;
            await _context.SaveChangesAsync();
            return NoContent();
        }

        // Apagar artista (Admin)
        [HttpDelete("{id}")]
        [Authorize(Roles = "Admin", AuthenticationSchemes = JwtBearerDefaults.AuthenticationScheme)]
        public async Task<IActionResult> ApagarArtista(int id)
        {
            var artista = await _context.Artistas.FindAsync(id);
            if (artista == null) return NotFound();
            _context.Artistas.Remove(artista);
            await _context.SaveChangesAsync();
            return NoContent();
        }

        // ------------------- NOTICIAS -------------------

        // Lista todas as notícias com o artista associado
        [HttpGet]
        public async Task<IActionResult> Noticias() => Ok(await _context.Noticias.Include(n => n.Artista).ToListAsync());

        // Criar nova notícia (Admin)
        [HttpPost]
        [Authorize(Roles = "Admin", AuthenticationSchemes = JwtBearerDefaults.AuthenticationScheme)]
        public async Task<IActionResult> CriarNoticia([FromBody] Noticia noticia)
        {
            _context.Noticias.Add(noticia);
            await _context.SaveChangesAsync();
            return CreatedAtAction(nameof(Noticias), new { id = noticia.Id }, noticia);
        }

        // Atualizar notícia (Admin)
        [HttpPut("{id}")]
        [Authorize(Roles = "Admin", AuthenticationSchemes = JwtBearerDefaults.AuthenticationScheme)]
        public async Task<IActionResult> AtualizarNoticia(int id, [FromBody] Noticia noticia)
        {
            if (id != noticia.Id) return BadRequest();
            _context.Entry(noticia).State = EntityState.Modified;
            await _context.SaveChangesAsync();
            return NoContent();
        }

        // Apagar notícia (Admin)
        [HttpDelete("{id}")]
        [Authorize(Roles = "Admin", AuthenticationSchemes = JwtBearerDefaults.AuthenticationScheme)]
        public async Task<IActionResult> ApagarNoticia(int id)
        {
            var noticia = await _context.Noticias.FindAsync(id);
            if (noticia == null) return NotFound();
            _context.Noticias.Remove(noticia);
            await _context.SaveChangesAsync();
            return NoContent();
        }

        // ------------------- GENEROS -------------------

        // Lista todos os géneros musicais
        [HttpGet]
        public async Task<IActionResult> Generos() => Ok(await _context.Generos.ToListAsync());

        // Detalhes de um género
        [HttpGet("{id}")]
        public async Task<IActionResult> Genero(int id) => Ok(await _context.Generos.FindAsync(id));

        // Criar género (Admin)
        [HttpPost]
        [Authorize(Roles = "Admin", AuthenticationSchemes = JwtBearerDefaults.AuthenticationScheme)]
        public async Task<IActionResult> CriarGenero([FromBody] Genero genero)
        {
            _context.Generos.Add(genero);
            await _context.SaveChangesAsync();
            return CreatedAtAction(nameof(Generos), new { id = genero.Id }, genero);
        }

        // Atualizar género (Admin)
        [HttpPut("{id}")]
        [Authorize(Roles = "Admin", AuthenticationSchemes = JwtBearerDefaults.AuthenticationScheme)]
        public async Task<IActionResult> AtualizarGenero(int id, [FromBody] Genero genero)
        {
            if (id != genero.Id) return BadRequest();
            _context.Entry(genero).State = EntityState.Modified;
            await _context.SaveChangesAsync();
            return NoContent();
        }

        // Apagar género (Admin)
        [HttpDelete("{id}")]
        [Authorize(Roles = "Admin", AuthenticationSchemes = JwtBearerDefaults.AuthenticationScheme)]
        public async Task<IActionResult> ApagarGenero(int id)
        {
            var genero = await _context.Generos.FindAsync(id);
            if (genero == null) return NotFound();
            _context.Generos.Remove(genero);
            await _context.SaveChangesAsync();
            return NoContent();
        }

        // ------------------- STREAMINGS -------------------

        // Lista os streamings de uma música
        [HttpGet("{musicaId}")]
        public async Task<IActionResult> StreamingsPorMusica(int musicaId) =>
            Ok(await _context.Streamings.Where(s => s.MusicaId == musicaId).ToListAsync());

        // ------------------- AVALIACOES -------------------

        // Lista as avaliações de uma música
        [HttpGet("{musicaId}")]
        public async Task<IActionResult> AvaliacoesPorMusica(int musicaId) =>
            Ok(await _context.Avaliacoes.Include(a => a.Utilizador).Where(a => a.MusicaId == musicaId).ToListAsync());

        // Lista avaliações feitas pelo utilizador autenticado
        [HttpGet]
        [Authorize(AuthenticationSchemes = JwtBearerDefaults.AuthenticationScheme)]
        public async Task<IActionResult> MinhasAvaliacoes()
        {
            var user = await _userManager.GetUserAsync(User);
            if (user == null) return Unauthorized();
            var avaliacoes = await _context.Avaliacoes.Include(a => a.Musica).Where(a => a.UtilizadorId == user.Id).ToListAsync();
            return Ok(avaliacoes);
        }

        // ------------------- LOGIN JWT -------------------

        // Endpoint para login via JWT
        [HttpPost]
        [Route("/api/v1/API/LoginAPI")]
        [AllowAnonymous]
        public async Task<IActionResult> Login([FromBody] LoginApiRequest model)
        {
            var user = await _userManager.FindByEmailAsync(model.Email);
            if (user == null || !await _userManager.CheckPasswordAsync(user, model.Password))
                return Unauthorized();

            var claims = new[]
            {
                new Claim(ClaimTypes.NameIdentifier, user.Id),
                new Claim(ClaimTypes.Email, user.Email),
                new Claim(ClaimTypes.Role, user.IsAdmin ? "Admin" : "User")
            };

            var key = new SymmetricSecurityKey(Encoding.UTF8.GetBytes("CHAVE_SUPER_SECRETA_DEV_2025_SEGURA_XYZ123"));
            var creds = new SigningCredentials(key, SecurityAlgorithms.HmacSha256);

            var token = new JwtSecurityToken(
                issuer: "SequeMusicAPI",
                claims: claims,
                expires: DateTime.Now.AddHours(1),
                signingCredentials: creds);

            return Ok(new { token = new JwtSecurityTokenHandler().WriteToken(token) });
        }
    }
}
