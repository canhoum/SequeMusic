using System.ComponentModel.DataAnnotations;

namespace SequeMusic.Models
{
    /// <summary>
    /// Modelo que representa um registo de streaming de uma música.
    /// Contém informações como a plataforma de streaming, número de reproduções,
    /// link para a música e a ligação à entidade Musica.
    /// </summary>
    public class Streaming
    {
        /// <summary>
        /// Identificador único do registo de streaming (chave primária).
        /// </summary>
        public int Id { get; set; }

        /// <summary>
        /// Nome da plataforma onde a música foi reproduzida (ex: Spotify, YouTube).
        /// Campo obrigatório.
        /// </summary>
        [Required]
        public string Plataforma { get; set; }

        /// <summary>
        /// Número total de reproduções (streams) registadas nesta plataforma.
        /// </summary>
        public int NumeroDeStreams { get; set; }

        /// <summary>
        /// URL direto para a música na plataforma de streaming.
        /// Validação automática de URL.
        /// </summary>
        [Url]
        public string Link { get; set; }

        /// <summary>
        /// Chave estrangeira que indica a música à qual este registo de streaming pertence.
        /// </summary>
        public int MusicaId { get; set; }

        /// <summary>
        /// Objeto de navegação para a música relacionada (relação N:1).
        /// </summary>
        public virtual Musica Musica { get; set; }
    }
}