using System.ComponentModel.DataAnnotations;
using System.Text.Json.Serialization;
using Microsoft.AspNetCore.Mvc.ModelBinding.Validation;

namespace SequeMusic.Models;

/// <summary>
/// Modelo que representa uma música no sistema.
/// Contém propriedades como título, álbum, letra, ano de lançamento,
/// link de áudio, posição no ranking Billboard, e relações com artista, género, avaliações e streamings.
/// </summary>
public class Musica
{
    /// <summary>
    /// Identificador único da música (chave primária).
    /// </summary>
    [Key]
    public int ID { get; set; }

    /// <summary>
    /// Título da música. Campo obrigatório.
    /// </summary>
    [Required(ErrorMessage = "O título é obrigatório.")]
    public string? Titulo { get; set; }

    /// <summary>
    /// Nome do álbum ao qual a música pertence (opcional).
    /// </summary>
    public string? Album { get; set; }

    /// <summary>
    /// Letra da música (opcional).
    /// </summary>
    public string? Letra { get; set; }

    /// <summary>
    /// Ano em que a música foi lançada.
    /// </summary>
    [Display(Name = "Ano de Lançamento")]
    public int AnoDeLancamento { get; set; }

    /// <summary>
    /// URL externo para escutar a música (Spotify, YouTube, etc.).
    /// </summary>
    public string LinkAudio { get; set; } = "";

    /// <summary>
    /// Nome do ficheiro de áudio armazenado localmente.
    /// </summary>
    public string NomeFicheiroAudio { get; set; } = "";

    /// <summary>
    /// Posição da música no ranking Billboard (opcional).
    /// </summary>
    public int? PosicaoBillboard { get; set; }

    /// <summary>
    /// FK para o artista responsável pela música.
    /// </summary>
    public int ArtistaId { get; set; }

    /// <summary>
    /// Objeto de navegação para o artista associado à música.
    /// </summary>
    [JsonIgnore]
    [ValidateNever]
    public virtual Artista Artista { get; set; }

    /// <summary>
    /// FK para o género musical da música.
    /// </summary>
    public int GeneroId { get; set; }

    /// <summary>
    /// Objeto de navegação para o género musical associado.
    /// </summary>
    [JsonIgnore]
    [ValidateNever]
    public virtual Genero Genero { get; set; }

    /// <summary>
    /// Lista de avaliações associadas a esta música.
    /// </summary>
    [ValidateNever]
    public virtual ICollection<Avaliacao> Avaliacoes { get; set; } = new List<Avaliacao>();

    /// <summary>
    /// Lista de streamings registados para esta música.
    /// </summary>
    [ValidateNever]
    public virtual ICollection<Streaming> Streamings { get; set; } = new List<Streaming>();
}
