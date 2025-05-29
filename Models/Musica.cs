using System.ComponentModel.DataAnnotations;
using Microsoft.AspNetCore.Mvc.ModelBinding.Validation;

namespace SequeMusic.Models;

public class Musica
{
    [Key]
    public int ID { get; set; }

    [Required(ErrorMessage = "O título é obrigatório.")]
    public string? Titulo { get; set; }

    public string? Album { get; set; }

    public string? Letra { get; set; }

    [Display(Name = "Ano de Lançamento")]
    public int AnoDeLancamento { get; set; }

    public string LinkAudio { get; set; } = "";

    public string NomeFicheiroAudio { get; set; } = "";
    
    public int? PosicaoBillboard { get; set; } // 1-100


    // Relação N:1 com Artista
    public int ArtistaId { get; set; }
    public virtual Artista Artista { get; set; }

    // Relação N:1 com Genero
    public int GeneroId { get; set; }
    public virtual Genero Genero { get; set; }

    // Relação 1:N com Avaliacao
    [ValidateNever]
    public virtual ICollection<Avaliacao> Avaliacoes { get; set; } = new List<Avaliacao>();

    // Relação 1:N com Streaming
    [ValidateNever]
    public virtual ICollection<Streaming> Streamings { get; set; } = new List<Streaming>();
}