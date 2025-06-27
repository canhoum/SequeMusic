using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using Microsoft.AspNetCore.Mvc.ModelBinding.Validation;

namespace SequeMusic.Models
{
    /// <summary>
    /// Modelo que representa um género musical (ex: Rock, Pop, Jazz).
    /// Cada género pode estar associado a várias músicas (relação 1:N).
    /// </summary>
    public class Genero
    {
        /// <summary>
        /// Identificador único do género (chave primária).
        /// </summary>
        public int Id { get; set; }

        /// <summary>
        /// Nome do género musical (ex: Rock, Jazz, Pop). Campo obrigatório.
        /// </summary>
        [Required] 
        public string Nome { get; set; } = "";

        /// <summary>
        /// Descrição adicional sobre o género (opcional).
        /// </summary>
        public string Descricao { get; set; } = "";

        /// <summary>
        /// Lista de músicas associadas a este género (relação 1:N).
        /// Ignorada na validação de formulários para evitar loops.
        /// </summary>
        [ValidateNever]        
        public virtual ICollection<Musica> Musicas { get; set; }
    }
}