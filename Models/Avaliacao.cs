// Modelo que representa uma avaliação feita por um utilizador a uma música
// Inclui nota (1 a 5), comentário, data, e ligações à música e ao utilizador

using System;
using System.ComponentModel.DataAnnotations;

namespace SequeMusic.Models
{
    public class Avaliacao
    {
        public int Id { get; set; } // Identificador único da avaliação (chave primária)

        [Range(1, 5)]
        public int Nota { get; set; } // Nota dada pelo utilizador (entre 1 e 5)

        public string Comentario { get; set; } // Comentário opcional associado à avaliação

        public DateTime Data_Avaliacao { get; set; } // Data em que a avaliação foi feita

        
        public int MusicaId { get; set; } // FK para a música avaliada

        public virtual Musica Musica { get; set; } // Navegação para a música (relação N:1)

        // FK para Utilizador
        public string UtilizadorId { get; set; } // FK para o utilizador que avaliou

        public virtual Utilizador Utilizador { get; set; } // Navegação para o utilizador (relação N:1)
    }
}