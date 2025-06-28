using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using Microsoft.AspNetCore.Mvc.ModelBinding.Validation;

namespace SequeMusic.Models
{
    /// <summary>
    /// Modelo de dados que representa um artista na aplicação SequeMusic.
    /// Contém propriedades como nome, biografia, país, imagem e relações com músicas e notícias.
    /// </summary>
    public class Artista
    {
        /// <summary>
        /// Identificador único do artista (chave primária).
        /// </summary>
        public int Id { get; set; }

        /// <summary>
        /// Nome do artista (campo obrigatório).
        /// </summary>
        [Required]
        public string Nome_Artista { get; set; }

        /// <summary>
        /// Biografia do artista (opcional).
        /// </summary>
        public string Biografia { get; set; } = "";

        /// <summary>
        /// País de origem do artista (opcional).
        /// </summary>
        public string Pais_Origem { get; set; } = "";

        /// <summary>
        /// Caminho para a imagem ou fotografia do artista.
        /// </summary>
        public string Foto { get; set; } = "";

        /// <summary>
        /// Lista de músicas associadas ao artista (relação 1:N).
        /// </summary>
        [ValidateNever]
        public virtual ICollection<Musica> Musicas { get; set; } = [];

        /// <summary>
        /// Lista de notícias associadas ao artista (relação 1:N).
        /// </summary>
        [ValidateNever]
        public virtual ICollection<Noticia> Noticias { get; set; } = [];
        
        /// <summary>
        /// Vários  artistas poderem estar associados a músicas(relação 1:N).
        /// </summary>
        [ValidateNever]
        public virtual ICollection<ArtistaMusica> ArtistasMusicas { get; set; } = new List<ArtistaMusica>();

    }
}