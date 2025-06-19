// Modelo que representa um utilizador da aplicação
// Herda de IdentityUser para suportar autenticação e autorização
// Adiciona campos personalizados como Nome, Telemóvel, Data de Nascimento, etc.

using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using Microsoft.AspNetCore.Identity;

namespace SequeMusic.Models
{
    public class Utilizador : IdentityUser
    {
        [StringLength(50)]
        [Required(ErrorMessage = "O {0} é de preenchimento obrigatório.")]
        public string Nome { get; set; } // Nome completo do utilizador

        [Display(Name = "Telemóvel")]
        [StringLength(9)]
        [RegularExpression("9[1236][0-9]{7}", ErrorMessage = "O {0} só aceita 9 dígitos.")]
        public string? Telemovel { get; set; } // Telemóvel com validação de formato (números portugueses)

        [Required(ErrorMessage = "A {0} é de preenchimento obrigatório.")]
        [Display(Name = "Data de Nascimento")]
        public DateTime DataNascimento { get; set; } // Data de nascimento obrigatória

        public bool IsAdmin { get; set; } = false; // Flag para indicar se o utilizador é administrador

        [NotMapped]
        [Display(Name = "Password")]
        public string Password { get; set; } // Usado apenas para registo (não mapeado para a BD)

        [NotMapped]
        [Display(Name = "Remember Me")]
        public bool RememberMe { get; set; } // Usado para login persistente (não mapeado para a BD)

        [NotMapped]
        public string DataNascFormatted => DataNascimento.ToString("dd/MM/yyyy"); 
        // Formatação pronta da data de nascimento para exibição

        public bool IsPremium { get; set; } = false; // Indica se o utilizador tem conta premium

        // Relacionamento com Avaliações feitas pelo utilizador
        public virtual ICollection<Avaliacao> Avaliacoes { get; set; }
    }
}