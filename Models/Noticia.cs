// Modelo que representa uma notícia associada a um artista
// Inclui título, conteúdo, data, fonte, imagem, resumo e ligação ao artista

using System;
using System.ComponentModel.DataAnnotations;
using System.Text.Json.Serialization;
using Microsoft.AspNetCore.Mvc.ModelBinding.Validation;

namespace SequeMusic.Models
{
    public class Noticia
    {
        public int Id { get; set; } // Identificador único da notícia (chave primária)

        [Required]
        public string Titulo { get; set; } // Título da notícia (obrigatório)

        public string Conteudo { get; set; } = ""; // Texto completo da notícia

        public DateTime Data_Publicacao { get; set; } // Data em que a notícia foi publicada

        public string Fonte { get; set; } = ""; // Fonte da notícia (opcional)

        [Display(Name = "Resumo")]
        [StringLength(300)]
        public string Resumo { get; set; } = ""; 
        // Resumo curto da notícia, útil para listagens (limitado a 300 caracteres)

        [Display(Name = "URL da Imagem")]
        public string ImagemUrl { get; set; } = ""; 
        // URL para imagem de capa associada à notícia

        // FK para Artista
        public int ArtistaId { get; set; } // Chave estrangeira que liga à entidade Artista

        [JsonIgnore]
        [ValidateNever]
        public virtual Artista Artista { get; set; } 
        // Objeto de navegação para o artista relacionado
        // Ignorado em validação de formulários e na serialização JSON (evita loops)
    }
}