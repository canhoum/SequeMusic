using System;
using System.ComponentModel.DataAnnotations;
using System.Text.Json.Serialization;
using Microsoft.AspNetCore.Mvc.ModelBinding.Validation;

namespace SequeMusic.Models
{
    public class Noticia
    {
        public int Id { get; set; }

        [Required]
        public string Titulo { get; set; }

        public string Conteudo { get; set; }= "";

        public DateTime Data_Publicacao { get; set; }

        public string Fonte { get; set; }= "";

        // FK para Artista
        public int ArtistaId { get; set; }
        
        [JsonIgnore]
        [ValidateNever]
        public virtual Artista Artista { get; set; }
    }
    
}