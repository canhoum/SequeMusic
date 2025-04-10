using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;

namespace SequeMusic.Models
{
    public class Artista
    {
        public int Id { get; set; }

        [Required]
        public string Nome_Artista { get; set; }

        public string Biografia { get; set; }

        public string Pais_Origem { get; set; }

        public string Foto { get; set; }

        public virtual ICollection<Musica> Musicas { get; set; }
        public virtual ICollection<Noticia> Noticias { get; set; }
    }
}