// ViewModel usado para apresentar os resultados de uma pesquisa
// Armazena a query inserida pelo utilizador e listas de artistas e músicas encontradas

using System.Collections.Generic;
using SequeMusic.Models;

public class PesquisaViewModel
{
    public string Query { get; set; } 
    // Texto de pesquisa introduzido pelo utilizador

    public List<Artista> Artistas { get; set; } = new(); 
    // Lista de artistas encontrados com base na pesquisa

    public List<Musica> Musicas { get; set; } = new(); 
    // Lista de músicas encontradas com base na pesquisa
}