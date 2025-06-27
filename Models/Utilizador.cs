using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using Microsoft.AspNetCore.Identity;

namespace SequeMusic.Models
{
    /// <summary>
    /// Modelo que representa um utilizador da aplicação.
    /// Herda de IdentityUser para integrar autenticação e autorização.
    /// Contém dados adicionais como nome, telemóvel, data de nascimento, etc.
    /// </summary>
    public class Utilizador : IdentityUser
    {
        /// <summary>
        /// Nome completo do utilizador. Campo obrigatório com limite de 50 caracteres.
        /// </summary>
        [StringLength(50)]
        [Required(ErrorMessage = "O {0} é de preenchimento obrigatório.")]
        public string Nome { get; set; }

        /// <summary>
        /// Número de telemóvel do utilizador (formato português: começa por 9 e tem 9 dígitos).
        /// Campo opcional.
        /// </summary>
        [Display(Name = "Telemóvel")]
        [StringLength(9)]
        [RegularExpression("9[1236][0-9]{7}", ErrorMessage = "O {0} só aceita 9 dígitos.")]
        public string? Telemovel { get; set; }

        /// <summary>
        /// Data de nascimento do utilizador. Campo obrigatório.
        /// </summary>
        [Required(ErrorMessage = "A {0} é de preenchimento obrigatório.")]
        [Display(Name = "Data de Nascimento")]
        public DateTime DataNascimento { get; set; }

        /// <summary>
        /// Indica se o utilizador é administrador do sistema.
        /// </summary>
        public bool IsAdmin { get; set; } = false;

        /// <summary>
        /// Palavra-passe usada no momento de registo. Não é armazenada na base de dados.
        /// </summary>
        [NotMapped]
        [Display(Name = "Password")]
        public string Password { get; set; }

        /// <summary>
        /// Indica se o utilizador escolheu manter a sessão iniciada. Usado apenas para login.
        /// </summary>
        [NotMapped]
        [Display(Name = "Remember Me")]
        public bool RememberMe { get; set; }

        /// <summary>
        /// Representação formatada da data de nascimento (dd/MM/yyyy).
        /// </summary>
        [NotMapped]
        public string DataNascFormatted => DataNascimento.ToString("dd/MM/yyyy");

        /// <summary>
        /// Indica se o utilizador tem subscrição premium.
        /// </summary>
        public bool IsPremium { get; set; } = false;

        /// <summary>
        /// Lista de avaliações feitas por este utilizador. Relação 1:N.
        /// </summary>
        public virtual ICollection<Avaliacao> Avaliacoes { get; set; }
    }
}
