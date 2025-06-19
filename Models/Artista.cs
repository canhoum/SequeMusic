// Modelo de dados para representar um artista na aplicação
// Contém propriedades como nome, biografia, país, foto e listas de músicas e notícias associadas

using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.ModelBinding.Validation;

namespace SequeMusic.Models
{
    public class Artista
    {
        public int Id { get; set; } // Identificador único do artista (chave primária)

        [Required] 
        public string Nome_Artista { get; set; } // Nome do artista (obrigatório)

        public string Biografia { get; set; } = ""; // Texto biográfico do artista (opcional)

        public string Pais_Origem { get; set; } = ""; // País de origem (opcional)

        public string Foto { get; set; } = ""; // Caminho para a imagem/foto do artista

        [ValidateNever]
        public virtual ICollection<Musica> Musicas { get; set; } = []; 
        // Lista de músicas associadas ao artista (relação 1:N)
        // 'ValidateNever' evita a validação deste campo no formulário

        [ValidateNever]
        public virtual ICollection<Noticia> Noticias { get; set; } = []; 
        // Lista de notícias associadas ao artista (relação 1:N)
        // Também ignorado na validação dos formulários
    }
}