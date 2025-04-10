using System.ComponentModel.DataAnnotations;

namespace SequeMusic.Models
{
    public class Streaming
    {
        public int Id { get; set; }

        [Required]
        public string Plataforma { get; set; }

        public int NumeroDeStreams { get; set; }

        // FK para Musica
        public int MusicaId { get; set; }
        public virtual Musica Musica { get; set; }
    }
}