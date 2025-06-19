// Modelo que representa um género musical (ex: Rock, Pop, Jazz)
// Cada género pode estar associado a várias músicas (relação 1:N)

using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using Microsoft.AspNetCore.Mvc.ModelBinding.Validation;

namespace SequeMusic.Models
{
    public class Genero
    {
        public int Id { get; set; } // Identificador único do género (chave primária)

        [Required] 
        public string Nome { get; set; } = ""; // Nome do género (obrigatório)

        public string Descricao { get; set; } = ""; // Descrição opcional do género

        [ValidateNever]        
        public virtual ICollection<Musica> Musicas { get; set; } 
        // Lista de músicas associadas a este género (relação 1:N)
        // Ignorada na validação de formulários (evita loops ou erros de binding)
    }
}