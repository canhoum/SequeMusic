using System;
using System.ComponentModel.DataAnnotations;
using System.Text.Json.Serialization;
using Microsoft.AspNetCore.Mvc.ModelBinding.Validation;

namespace SequeMusic.Models
{
    /// <summary>
    /// Modelo que representa uma notícia associada a um artista.
    /// Contém título, conteúdo, data de publicação, fonte, resumo, imagem e ligação ao artista.
    /// </summary>
    public class Noticia
    {
        /// <summary>
        /// Identificador único da notícia (chave primária).
        /// </summary>
        public int Id { get; set; }

        /// <summary>
        /// Título da notícia (obrigatório).
        /// </summary>
        [Required]
        public string Titulo { get; set; }

        /// <summary>
        /// Texto completo da notícia.
        /// </summary>
        public string Conteudo { get; set; } = "";

        /// <summary>
        /// Data em que a notícia foi publicada.
        /// </summary>
        public DateTime Data_Publicacao { get; set; }

        /// <summary>
        /// Fonte da notícia (ex: nome do site ou jornal).
        /// </summary>
        public string Fonte { get; set; } = "";

        /// <summary>
        /// Resumo curto da notícia (máx. 300 caracteres).
        /// </summary>
        [Display(Name = "Resumo")]
        [StringLength(300)]
        public string Resumo { get; set; } = "";

        /// <summary>
        /// URL para imagem associada à notícia.
        /// </summary>
        [Display(Name = "URL da Imagem")]
        public string ImagemUrl { get; set; } = "";

        /// <summary>
        /// Chave estrangeira que liga a notícia a um artista.
        /// </summary>
        public int ArtistaId { get; set; }

        /// <summary>
        /// Objeto de navegação para o artista associado à notícia.
        /// Ignorado em validação de formulários e na serialização JSON.
        /// </summary>
        [JsonIgnore]
        [ValidateNever]
        public virtual Artista Artista { get; set; }
    }
}