using Microsoft.AspNetCore.Identity.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore;
using SequeMusic.Models;


namespace SequeMusic.Data
{
    public class ApplicationDbContext : IdentityDbContext<Utilizador>
    {
        public ApplicationDbContext(DbContextOptions<ApplicationDbContext> options)
            : base(options)
        {
        }
        public DbSet<Musica> Musicas { get; set; }
        public DbSet<Artista> Artistas { get; set; }
        public DbSet<Genero> Generos { get; set; }
        public DbSet<Avaliacao> Avaliacoes { get; set; }
        public DbSet<Streaming> Streamings { get; set; }
        public DbSet<Noticia> Noticias { get; set; }
        

        protected override void OnModelCreating(ModelBuilder modelBuilder)
        {
            base.OnModelCreating(modelBuilder);
            modelBuilder.Entity<Musica>()
                .HasOne(m => m.Artista)
                .WithMany(a => a.Musicas)
                .HasForeignKey(m => m.ArtistaId)
                .OnDelete(DeleteBehavior.Cascade);

            // Música -> Gênero (N:1)
            modelBuilder.Entity<Musica>()
                .HasOne(m => m.Genero)
                .WithMany(g => g.Musicas)
                .HasForeignKey(m => m.GeneroId)
                .OnDelete(DeleteBehavior.Cascade);

            // Avaliação -> Música (N:1)
            modelBuilder.Entity<Avaliacao>()
                .HasOne(a => a.Musica)
                .WithMany(m => m.Avaliacoes)
                .HasForeignKey(a => a.MusicaId)
                .OnDelete(DeleteBehavior.Cascade);
            
            // Avaliação -> Utilizador (N:1)
            modelBuilder.Entity<Avaliacao>()
                .HasOne(a => a.Utilizador)
                .WithMany(u => u.Avaliacoes)
                .HasForeignKey(a => a.UtilizadorId)
                .OnDelete(DeleteBehavior.Cascade);

// Streaming -> Música (N:1)
            modelBuilder.Entity<Streaming>()
                .HasOne(s => s.Musica)
                .WithMany(m => m.Streamings)
                .HasForeignKey(s => s.MusicaId)
                .OnDelete(DeleteBehavior.Cascade);

// Notícia -> Artista (N:1)
            modelBuilder.Entity<Noticia>()
                .HasOne(n => n.Artista)
                .WithMany(a => a.Noticias)
                .HasForeignKey(n => n.ArtistaId)
                .OnDelete(DeleteBehavior.Cascade);


    }
}
}