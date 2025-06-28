using System.ComponentModel.DataAnnotations.Schema;

namespace SequeMusic.Models
{
    /// <summary>
    /// Entidade de junção que representa a relação muitos-para-muitos entre Artistas e Músicas.
    /// </summary>
    public class ArtistaMusica
    {
        /// <summary>
        /// Chave estrangeira para o Artista.
        /// </summary>
        public int ArtistaId { get; set; }

        /// <summary>
        /// Navegação para o Artista.
        /// </summary>
        public Artista Artista { get; set; }

        /// <summary>
        /// Chave estrangeira para a Música.
        /// </summary>
        public int MusicaId { get; set; }

        /// <summary>
        /// Navegação para a Música.
        /// </summary>
        public Musica Musica { get; set; }
    }
}