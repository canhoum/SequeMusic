using System.Collections.Generic;
using SequeMusic.Models;

/// <summary>
/// ViewModel usado para apresentar os resultados de uma pesquisa.
/// Armazena a query inserida pelo utilizador e listas de artistas e músicas encontrados.
/// </summary>
public class PesquisaViewModel
{
    /// <summary>
    /// Termo de pesquisa inserido pelo utilizador.
    /// </summary>
    public string Query { get; set; }

    /// <summary>
    /// Lista de artistas que correspondem à pesquisa.
    /// </summary>
    public List<Artista> Artistas { get; set; } = new();

    /// <summary>
    /// Lista de músicas que correspondem à pesquisa.
    /// </summary>
    public List<Musica> Musicas { get; set; } = new();
}