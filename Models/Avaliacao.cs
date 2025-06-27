using System;
using System.ComponentModel.DataAnnotations;

namespace SequeMusic.Models
{
    /// <summary>
    /// Modelo que representa uma avaliação feita por um utilizador a uma música.
    /// Contém nota, comentário, data e referências à música e ao utilizador.
    /// </summary>
    public class Avaliacao
    {
        /// <summary>
        /// Identificador único da avaliação (chave primária).
        /// </summary>
        public int Id { get; set; }

        /// <summary>
        /// Nota atribuída pelo utilizador (entre 1 e 5).
        /// </summary>
        [Range(1, 5)]
        public int Nota { get; set; }

        /// <summary>
        /// Comentário opcional escrito pelo utilizador.
        /// </summary>
        public string Comentario { get; set; }

        /// <summary>
        /// Data em que a avaliação foi submetida.
        /// </summary>
        public DateTime Data_Avaliacao { get; set; }

        /// <summary>
        /// Chave estrangeira para a música avaliada.
        /// </summary>
        public int MusicaId { get; set; }

        /// <summary>
        /// Navegação para a música associada (relação N:1).
        /// </summary>
        public virtual Musica Musica { get; set; }

        /// <summary>
        /// Chave estrangeira para o utilizador que avaliou.
        /// </summary>
        public string UtilizadorId { get; set; }

        /// <summary>
        /// Navegação para o utilizador que realizou a avaliação (relação N:1).
        /// </summary>
        public virtual Utilizador Utilizador { get; set; }
    }
}