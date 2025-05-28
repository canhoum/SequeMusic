using System;
using System.ComponentModel.DataAnnotations;

namespace SequeMusic.Models
{
    public class Avaliacao
    {
        public int Id { get; set; }

        [Range(1, 5)]
        public int Nota { get; set; }

        public string Comentario { get; set; }

        public DateTime Data_Avaliacao { get; set; }

        // FK para Musica
        
        public int MusicaId { get; set; }
        
        public virtual Musica Musica { get; set; }

        // FK para Utilizador
        public string UtilizadorId { get; set; }
        public virtual Utilizador Utilizador { get; set; }
    }
}