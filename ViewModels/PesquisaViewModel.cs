using System.Collections.Generic;
using SequeMusic.Models;

public class PesquisaViewModel
{
    public string Query { get; set; }
    public List<Artista> Artistas { get; set; } = new();
    public List<Musica> Musicas { get; set; } = new();
}