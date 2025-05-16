using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace SequeMusic.Models;


public class Musica
{
    [Key]
    public int ID { get; set; }

    [Required]
    public string Titulo { get; set; }

    public string Album { get; set; }

    [Display(Name = "Ano de Lançamento")]
    public int AnoDeLancamento { get; set; }
    
    [Required]
    public string LinkAudio { get; set; }
    
    [Required]
    public string NomeFicheiroAudio { get; set; }
    

    // Relação N:1 com Artista
    public int ArtistaId { get; set; }
    public virtual Artista Artista { get; set; }

    // Relação N:1 com Genero
    public int GeneroId { get; set; }
    public virtual Genero Genero { get; set; }

    // Relação 1:N com Avaliacao
    public virtual ICollection<Avaliacao> Avaliacoes { get; set; }

    // Relação 1:N com Streaming
    public virtual ICollection<Streaming> Streamings { get; set; }

}