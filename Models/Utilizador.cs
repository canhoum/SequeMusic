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
        public string Nome { get; set; }

        [Display(Name = "Telemóvel")]
        [StringLength(9)]
        [RegularExpression("9[1236][0-9]{7}", ErrorMessage = "O {0} só aceita 9 dígitos.")]
        public string Telemovel { get; set; }

        [Required(ErrorMessage = "A {0} é de preenchimento obrigatório.")]
        [Display(Name = "Data de Nascimento")]
        public DateOnly DataNascimento { get; set; }

        public bool IsAdmin { get; set; } = false;

        [NotMapped]
        [Display(Name = "Password")]
        public string Password { get; set; }

        [NotMapped]
        [Display(Name = "Remember Me")]
        public bool RememberMe { get; set; }

        [NotMapped]
        public string DataNascFormatted => DataNascimento.ToString("dd/MM/yyyy");

        // Relacionamento com Avaliações
        public virtual ICollection<Avaliacao> Avaliacoes { get; set; }
    }
}