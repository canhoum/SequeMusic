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
        private readonly ApplicationDbContext _context;
        private readonly UserManager<Utilizador> _userManager;

        public APIController(ApplicationDbContext context, UserManager<Utilizador> userManager)
        {
            _context = context;
            _userManager = userManager;
        }
        
        /// <summary>
        /// Devolve uma lista de músicas, com filtros opcionais por género, artista e ano.
        /// </summary>
        /// <param name="genero">Nome parcial ou completo do género musical.</param>
        /// <param name="artista">Nome parcial ou completo do artista.</param>
        /// <param name="ano">Ano de lançamento da música.</param>
        /// <returns>Lista de músicas filtradas.</returns>
        [HttpGet]
        public async Task<IActionResult> Musicas(string? genero, string? artista, int? ano)
        {
            var query = _context.Musicas.Include(m => m.Artista).Include(m => m.Genero).AsQueryable();
            if (!string.IsNullOrEmpty(genero)) query = query.Where(m => m.Genero.Nome.Contains(genero));
            if (!string.IsNullOrEmpty(artista)) query = query.Where(m => m.Artista.Nome_Artista.Contains(artista));
            if (ano.HasValue) query = query.Where(m => m.AnoDeLancamento == ano);
            return Ok(await query.ToListAsync());
        }

        /// <summary>
        /// Devolve os detalhes de uma música específica.
        /// </summary>
        /// <param name="id">ID da música.</param>
        /// <returns>Detalhes da música.</returns>
        [HttpGet("{id}")]
        public async Task<IActionResult> Musica(int id) =>
            Ok(await _context.Musicas.Include(m => m.Artista).Include(m => m.Genero)
                .FirstOrDefaultAsync(m => m.ID == id));

        /// <summary>
        /// Cria uma nova música (apenas para utilizadores Premium ou Admin).
        /// </summary>
        /// <param name="musica">Objeto com os dados da música.</param>
        /// <returns>Música criada com respetivo URI.</returns>
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

        /// <summary>
        /// Atualiza uma música existente (Admin apenas).
        /// </summary>
        /// <param name="id">ID da música.</param>
        /// <param name="musica">Dados atualizados da música.</param>
        /// <returns>Resposta HTTP sem conteúdo.</returns>
        [HttpPut("{id}")]
        [Authorize(Roles = "Admin", AuthenticationSchemes = JwtBearerDefaults.AuthenticationScheme)]
        public async Task<IActionResult> AtualizarMusica(int id, [FromBody] Musica musica)
        {
            if (id != musica.ID) return BadRequest();
            _context.Entry(musica).State = EntityState.Modified;
            await _context.SaveChangesAsync();
            return NoContent();
        }

        /// <summary>
        /// Elimina uma música existente (Admin apenas).
        /// </summary>
        /// <param name="id">ID da música.</param>
        /// <returns>Resposta HTTP sem conteúdo.</returns>
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


        /// <summary>
        /// Devolve a lista de todos os artistas.
        /// </summary>
        /// <returns>Lista de artistas.</returns>
        [HttpGet]
        public async Task<IActionResult> Artistas() => Ok(await _context.Artistas.ToListAsync());

        /// <summary>
        /// Devolve os detalhes de um artista, incluindo músicas e notícias associadas.
        /// </summary>
        /// <param name="id">ID do artista.</param>
        /// <returns>Detalhes do artista.</returns>
        [HttpGet("{id}")]
        public async Task<IActionResult> Artista(int id) =>
            Ok(await _context.Artistas.Include(a => a.Musicas).Include(a => a.Noticias)
                .FirstOrDefaultAsync(a => a.Id == id));

        /// <summary>
        /// Cria um novo artista (apenas para administradores).
        /// </summary>
        /// <param name="artista">Objeto artista a criar.</param>
        /// <returns>Artista criado.</returns>
        [HttpPost]
        [Authorize(Roles = "Admin", AuthenticationSchemes = JwtBearerDefaults.AuthenticationScheme)]
        public async Task<IActionResult> CriarArtista([FromBody] Artista artista)
        {
            _context.Artistas.Add(artista);
            await _context.SaveChangesAsync();
            return CreatedAtAction(nameof(Artista), new { id = artista.Id }, artista);
        }

        /// <summary>
        /// Atualiza os dados de um artista (Admin apenas).
        /// </summary>
        /// <param name="id">ID do artista.</param>
        /// <param name="artista">Dados atualizados.</param>
        /// <returns>Resposta HTTP sem conteúdo.</returns>
        [HttpPut("{id}")]
        [Authorize(Roles = "Admin", AuthenticationSchemes = JwtBearerDefaults.AuthenticationScheme)]
        public async Task<IActionResult> AtualizarArtista(int id, [FromBody] Artista artista)
        {
            if (id != artista.Id) return BadRequest();
            _context.Entry(artista).State = EntityState.Modified;
            await _context.SaveChangesAsync();
            return NoContent();
        }

        /// <summary>
        /// Elimina um artista existente (Admin apenas).
        /// </summary>
        /// <param name="id">ID do artista.</param>
        /// <returns>Resposta HTTP sem conteúdo.</returns>
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



        /// <summary>
        /// Devolve todas as notícias com o artista associado.
        /// </summary>
        /// <returns>Lista de notícias com respetivos artistas.</returns>
        [HttpGet]
        public async Task<IActionResult> Noticias() =>
            Ok(await _context.Noticias.Include(n => n.Artista).ToListAsync());

        /// <summary>
        /// Cria uma nova notícia (apenas administradores).
        /// </summary>
        /// <param name="noticia">Notícia a ser criada.</param>
        /// <returns>Notícia criada.</returns>
        [HttpPost]
        [Authorize(Roles = "Admin", AuthenticationSchemes = JwtBearerDefaults.AuthenticationScheme)]
        public async Task<IActionResult> CriarNoticia([FromBody] Noticia noticia)
        {
            _context.Noticias.Add(noticia);
            await _context.SaveChangesAsync();
            return CreatedAtAction(nameof(Noticias), new { id = noticia.Id }, noticia);
        }

        /// <summary>
        /// Atualiza uma notícia existente (apenas administradores).
        /// </summary>
        /// <param name="id">ID da notícia.</param>
        /// <param name="noticia">Objeto notícia com os dados atualizados.</param>
        /// <returns>Resposta HTTP sem conteúdo.</returns>
        [HttpPut("{id}")]
        [Authorize(Roles = "Admin", AuthenticationSchemes = JwtBearerDefaults.AuthenticationScheme)]
        public async Task<IActionResult> AtualizarNoticia(int id, [FromBody] Noticia noticia)
        {
            if (id != noticia.Id) return BadRequest();
            _context.Entry(noticia).State = EntityState.Modified;
            await _context.SaveChangesAsync();
            return NoContent();
        }

        /// <summary>
        /// Elimina uma notícia da base de dados (apenas administradores).
        /// </summary>
        /// <param name="id">ID da notícia.</param>
        /// <returns>Resposta HTTP sem conteúdo.</returns>
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

        /// <summary>
        /// Devolve todos os géneros musicais existentes.
        /// </summary>
        /// <returns>Lista de géneros musicais.</returns>
        [HttpGet]
        public async Task<IActionResult> Generos() => Ok(await _context.Generos.ToListAsync());

        /// <summary>
        /// Devolve os detalhes de um género específico.
        /// </summary>
        /// <param name="id">ID do género.</param>
        /// <returns>Detalhes do género.</returns>
        [HttpGet("{id}")]
        public async Task<IActionResult> Genero(int id) => Ok(await _context.Generos.FindAsync(id));

        /// <summary>
        /// Cria um novo género musical (apenas administradores).
        /// </summary>
        /// <param name="genero">Género a ser criado.</param>
        /// <returns>Género criado.</returns>
        [HttpPost]
        [Authorize(Roles = "Admin", AuthenticationSchemes = JwtBearerDefaults.AuthenticationScheme)]
        public async Task<IActionResult> CriarGenero([FromBody] Genero genero)
        {
            _context.Generos.Add(genero);
            await _context.SaveChangesAsync();
            return CreatedAtAction(nameof(Generos), new { id = genero.Id }, genero);
        }

        /// <summary>
        /// Atualiza um género existente (apenas administradores).
        /// </summary>
        /// <param name="id">ID do género.</param>
        /// <param name="genero">Dados atualizados do género.</param>
        /// <returns>Resposta HTTP sem conteúdo.</returns>
        [HttpPut("{id}")]
        [Authorize(Roles = "Admin", AuthenticationSchemes = JwtBearerDefaults.AuthenticationScheme)]
        public async Task<IActionResult> AtualizarGenero(int id, [FromBody] Genero genero)
        {
            if (id != genero.Id) return BadRequest();
            _context.Entry(genero).State = EntityState.Modified;
            await _context.SaveChangesAsync();
            return NoContent();
        }

        /// <summary>
        /// Elimina um género musical da base de dados (apenas administradores).
        /// </summary>
        /// <param name="id">ID do género.</param>
        /// <returns>Resposta HTTP sem conteúdo.</returns>
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


        /// <summary>
        /// Devolve todos os streamings de uma música específica.
        /// </summary>
        /// <param name="musicaId">ID da música.</param>
        /// <returns>Lista de streamings associados à música.</returns>
        [HttpGet("{musicaId}")]
        public async Task<IActionResult> StreamingsPorMusica(int musicaId) =>
            Ok(await _context.Streamings.Where(s => s.MusicaId == musicaId).ToListAsync());

        /// <summary>
        /// Devolve todas as avaliações de uma música.
        /// </summary>
        /// <param name="musicaId">ID da música.</param>
        /// <returns>Lista de avaliações da música.</returns>
        [HttpGet("{musicaId}")]
        public async Task<IActionResult> AvaliacoesPorMusica(int musicaId) =>
            Ok(await _context.Avaliacoes.Include(a => a.Utilizador).Where(a => a.MusicaId == musicaId).ToListAsync());

        /// <summary>
        /// Devolve todas as avaliações feitas pelo utilizador autenticado.
        /// </summary>
        /// <returns>Lista de avaliações do utilizador autenticado.</returns>
        [HttpGet]
        [Authorize(AuthenticationSchemes = JwtBearerDefaults.AuthenticationScheme)]
        public async Task<IActionResult> MinhasAvaliacoes()
        {
            var user = await _userManager.GetUserAsync(User);
            if (user == null) return Unauthorized();
            var avaliacoes = await _context.Avaliacoes.Include(a => a.Musica).Where(a => a.UtilizadorId == user.Id)
                .ToListAsync();
            return Ok(avaliacoes);
        }

        /// <summary>
        /// Realiza o login de um utilizador e devolve um token JWT válido.
        /// </summary>
        /// <param name="model">Email e password do utilizador.</param>
        /// <returns>Token JWT para autenticação.</returns>
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