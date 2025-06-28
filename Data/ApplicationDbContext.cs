using Microsoft.AspNetCore.Identity.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore;
using SequeMusic.Models;

namespace SequeMusic.Data
{
    /// <summary>
    /// Classe responsável pela gestão da base de dados da aplicação SequeMusic.
    /// Herda de IdentityDbContext para suporte à autenticação e gestão de utilizadores.
    /// </summary>
    public class ApplicationDbContext : IdentityDbContext<Utilizador>
    {
        /// <summary>
        /// Construtor da classe ApplicationDbContext que recebe opções de configuração.
        /// </summary>
        /// <param name="options">Opções de configuração do contexto.</param>
        public ApplicationDbContext(DbContextOptions<ApplicationDbContext> options)
            : base(options)
        {
        }

        /// <summary>
        /// Representa a tabela de músicas na base de dados.
        /// </summary>
        public DbSet<Musica> Musicas { get; set; }

        /// <summary>
        /// Representa a tabela de artistas na base de dados.
        /// </summary>
        public DbSet<Artista> Artistas { get; set; }

        /// <summary>
        /// Representa a tabela de géneros musicais.
        /// </summary>
        public DbSet<Genero> Generos { get; set; }

        /// <summary>
        /// Representa a tabela de avaliações feitas por utilizadores.
        /// </summary>
        public DbSet<Avaliacao> Avaliacoes { get; set; }

        /// <summary>
        /// Representa a tabela de registos de streaming.
        /// </summary>
        public DbSet<Streaming> Streamings { get; set; }

        /// <summary>
        /// Representa a tabela de notícias associadas a artistas.
        /// </summary>
        public DbSet<Noticia> Noticias { get; set; }

        /// <summary>
        /// Representa a tabela de utilizadores da aplicação.
        /// </summary>
        public DbSet<Utilizador> Utilizadors { get; set; }
        
        /// <summary>
        /// Representa a tabela de Artistas e Musicas.
        /// </summary>
        public DbSet<ArtistaMusica> ArtistasMusicas { get; set; }


        /// <summary>
        /// Configura as relações entre entidades e comportamentos de eliminação em cascata.
        /// </summary>
        /// <param name="modelBuilder">Construtor de modelos do EF Core.</param>
        protected override void OnModelCreating(ModelBuilder modelBuilder)
        {
            // Invoca a configuração base do Identity
            base.OnModelCreating(modelBuilder);

            // Relação 1:N entre Música e Artista
            modelBuilder.Entity<Musica>()
                .HasOne(m => m.Artista)
                .WithMany(a => a.Musicas)
                .HasForeignKey(m => m.ArtistaId)
                .OnDelete(DeleteBehavior.Cascade); // Ao eliminar um artista, elimina as suas músicas

            // Relação 1:N entre Música e Género
            modelBuilder.Entity<Musica>()
                .HasOne(m => m.Genero)
                .WithMany(g => g.Musicas)
                .HasForeignKey(m => m.GeneroId)
                .OnDelete(DeleteBehavior.Cascade); // Ao eliminar um género, elimina as suas músicas

            // Relação 1:N entre Avaliação e Música
            modelBuilder.Entity<Avaliacao>()
                .HasOne(a => a.Musica)
                .WithMany(m => m.Avaliacoes)
                .HasForeignKey(a => a.MusicaId)
                .OnDelete(DeleteBehavior.Cascade); // Ao eliminar uma música, elimina as avaliações associadas

            // Relação 1:N entre Avaliação e Utilizador
            modelBuilder.Entity<Avaliacao>()
                .HasOne(a => a.Utilizador)
                .WithMany(u => u.Avaliacoes)
                .HasForeignKey(a => a.UtilizadorId)
                .OnDelete(DeleteBehavior.Cascade); // Ao eliminar um utilizador, elimina as suas avaliações

            // Relação 1:N entre Streaming e Música
            modelBuilder.Entity<Streaming>()
                .HasOne(s => s.Musica)
                .WithMany(m => m.Streamings)
                .HasForeignKey(s => s.MusicaId)
                .OnDelete(DeleteBehavior.Cascade); // Ao eliminar uma música, elimina os streamings

            // Relação 1:N entre Notícia e Artista
            modelBuilder.Entity<Noticia>()
                .HasOne(n => n.Artista)
                .WithMany(a => a.Noticias)
                .HasForeignKey(n => n.ArtistaId)
                .OnDelete(DeleteBehavior.Cascade); // Ao eliminar um artista, elimina as suas notícias
            
            // Relação N:N entre Música e Artista
            modelBuilder.Entity<ArtistaMusica>()
                .HasKey(am => new { am.ArtistaId, am.MusicaId });

            modelBuilder.Entity<ArtistaMusica>()
                .HasOne(am => am.Musica)
                .WithMany(m => m.ArtistasMusicas)
                .HasForeignKey(am => am.MusicaId)
                .OnDelete(DeleteBehavior.Restrict);

            modelBuilder.Entity<ArtistaMusica>()
                .HasOne(am => am.Musica)
                .WithMany(m => m.ArtistasMusicas)
                .HasForeignKey(am => am.MusicaId);

        }
    }
}
