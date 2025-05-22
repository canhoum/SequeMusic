using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using Microsoft.AspNetCore.Mvc.ModelBinding.Validation;

namespace SequeMusic.Models
{
    public class Genero
    {
        public int Id { get; set; }

        [Required] public string Nome { get; set; } = "";

        public string Descricao { get; set; } = "";

        [ValidateNever]        
        public virtual ICollection<Musica> Musicas { get; set; }
    }
}