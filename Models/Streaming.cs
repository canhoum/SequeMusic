// Modelo que representa um registo de streaming de uma música
// Inclui a plataforma, número de streams, link, e a ligação à música

using System.ComponentModel.DataAnnotations;

namespace SequeMusic.Models
{
    public class Streaming
    {
        public int Id { get; set; } // Identificador único do registo de streaming (chave primária)

        [Required]
        public string Plataforma { get; set; } // Nome da plataforma (ex: Spotify, YouTube) — obrigatório

        public int NumeroDeStreams { get; set; } // Número total de reproduções (streams)

        [Url]
        public string Link { get; set; } // Link direto para a música na plataforma (validação de URL)

        // FK para Musica
        public int MusicaId { get; set; } // Chave estrangeira que liga à música

        public virtual Musica Musica { get; set; } // Objeto de navegação para a música relacionada
    }
}