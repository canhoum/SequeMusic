// Importa as bibliotecas necessárias para o funcionamento da base de dados
using Microsoft.AspNetCore.Identity.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore;
using SequeMusic.Models;

namespace SequeMusic.Data
{
    // Classe responsável pela gestão da base de dados
    // Herda de IdentityDbContext para integrar a gestão de utilizadores
    public class ApplicationDbContext : IdentityDbContext<Utilizador>
    {
        // Construtor que recebe as opções de configuração da base de dados
        public ApplicationDbContext(DbContextOptions<ApplicationDbContext> options)
            : base(options)
        {
        }

        // Tabelas da base de dados (DbSets)
        public DbSet<Musica> Musicas { get; set; }
        public DbSet<Artista> Artistas { get; set; }
        public DbSet<Genero> Generos { get; set; }
        public DbSet<Avaliacao> Avaliacoes { get; set; }
        public DbSet<Streaming> Streamings { get; set; }
        public DbSet<Noticia> Noticias { get; set; }
        public DbSet<Utilizador> Utilizadors { get; set; }

        // Configuração das relações entre as entidades
        protected override void OnModelCreating(ModelBuilder modelBuilder)
        {
            // Invoca a configuração base do Identity
            base.OnModelCreating(modelBuilder);

            // Relação 1:N entre Musica e Artista
            modelBuilder.Entity<Musica>()
                .HasOne(m => m.Artista)
                .WithMany(a => a.Musicas)
                .HasForeignKey(m => m.ArtistaId)
                .OnDelete(DeleteBehavior.Cascade); // Ao eliminar um artista, elimina as músicas associadas

            // Relação 1:N entre Musica e Genero
            modelBuilder.Entity<Musica>()
                .HasOne(m => m.Genero)
                .WithMany(g => g.Musicas)
                .HasForeignKey(m => m.GeneroId)
                .OnDelete(DeleteBehavior.Cascade); // Ao eliminar um género, elimina as músicas associadas

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
                .OnDelete(DeleteBehavior.Cascade); // Ao eliminar uma música, elimina os streamings associados

            // Relação 1:N entre Notícia e Artista
            modelBuilder.Entity<Noticia>()
                .HasOne(n => n.Artista)
                .WithMany(a => a.Noticias)
                .HasForeignKey(n => n.ArtistaId)
                .OnDelete(DeleteBehavior.Cascade); // Ao eliminar um artista, elimina as suas notícias
        }
    }
}
