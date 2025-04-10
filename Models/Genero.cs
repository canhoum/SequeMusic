using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;

namespace SequeMusic.Models
{
    public class Genero
    {
        public int Id { get; set; }

        [Required]
        public string Nome { get; set; }

        public string Descricao { get; set; }

        public virtual ICollection<Musica> Musicas { get; set; }
    }
}