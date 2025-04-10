using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace SequeMusic.Models;

public class Musica
{
    /// <summary>
    /// Chave Primária (PK)
    /// </summary>
    [Key]
    public int MusicaId { get; set; }
    [StringLength(50)]
    [Required(ErrorMessage = "O {0} é de preenchimento obrigatório.")]
    public string Nome { get; set; }
    /// <summary>
    /// número de telemóvel do Utilizador
    /// </summary>
}