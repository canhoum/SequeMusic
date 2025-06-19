// Modelo que representa uma música no sistema
// Inclui dados como título, álbum, ano, letra, e relações com artista, género, avaliações e streamings

using System.ComponentModel.DataAnnotations;
using System.Text.Json.Serialization;
using Microsoft.AspNetCore.Mvc.ModelBinding.Validation;

namespace SequeMusic.Models;

public class Musica
{
    [Key]
    public int ID { get; set; } // Identificador único da música (chave primária)

    [Required(ErrorMessage = "O título é obrigatório.")]
    public string? Titulo { get; set; } // Título da música (obrigatório)

    public string? Album { get; set; } // Nome do álbum (opcional)

    public string? Letra { get; set; } // Letra da música (opcional)

    [Display(Name = "Ano de Lançamento")]
    public int AnoDeLancamento { get; set; } // Ano em que a música foi lançada

    public string LinkAudio { get; set; } = ""; // URL ou link para escutar a música

    public string NomeFicheiroAudio { get; set; } = ""; // Nome do ficheiro de áudio armazenado no sistema

    public int? PosicaoBillboard { get; set; } // Posição no ranking Billboard (opcional)

    // Relação N:1 com Artista
    public int ArtistaId { get; set; } // Chave estrangeira para o artista
    [JsonIgnore]
    [ValidateNever]
    public virtual Artista Artista { get; set; } // Objeto de navegação para o artista

    // Relação N:1 com Genero
    public int GeneroId { get; set; } // Chave estrangeira para o género musical
    [JsonIgnore]
    [ValidateNever]
    public virtual Genero Genero { get; set; } // Objeto de navegação para o género

    // Relação 1:N com Avaliacao
    [ValidateNever]
    public virtual ICollection<Avaliacao> Avaliacoes { get; set; } = new List<Avaliacao>();
    // Lista de avaliações associadas à música

    // Relação 1:N com Streaming
    [ValidateNever]
    public virtual ICollection<Streaming> Streamings { get; set; } = new List<Streaming>();
    // Lista de streamings registados desta música
}