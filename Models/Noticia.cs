using System;
using System.ComponentModel.DataAnnotations;

namespace SequeMusic.Models
{
    public class Noticia
    {
        public int Id { get; set; }

        [Required]
        public string Titulo { get; set; }

        public string Conteudo { get; set; } = "";

        public DateTime Data_Publicacao { get; set; }

        public string Fonte { get; set; } = "";

        // Campo para mostrar resumo/introdução
        [Display(Name = "Resumo")]
        [StringLength(300)]
        public string Resumo { get; set; } = "";

        // Campo para imagem de capa
        [Display(Name = "URL da Imagem")]
        public string ImagemUrl { get; set; } = "";

        // FK para Artista
        public int ArtistaId { get; set; }
        public virtual Artista Artista { get; set; }
    }
}