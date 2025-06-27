using System.ComponentModel.DataAnnotations;

namespace SequeMusic.ViewModels
{
    /// <summary>
    /// ViewModel usado para o formulário de registo de um novo utilizador.
    /// Contém os campos obrigatórios e opcionais necessários para criar uma conta.
    /// </summary>
    public class RegisterRequest
    {
        /// <summary>
        /// Nome completo do utilizador.
        /// Campo obrigatório com um máximo de 50 caracteres.
        /// </summary>
        [Required]
        [StringLength(50)]
        public string Nome { get; set; }

        /// <summary>
        /// Endereço de email do utilizador.
        /// Campo obrigatório e validado como email.
        /// </summary>
        [Required]
        [EmailAddress]
        public string Email { get; set; }

        /// <summary>
        /// Palavra-passe definida pelo utilizador.
        /// Campo obrigatório.
        /// </summary>
        [Required]
        public string Password { get; set; }

        /// <summary>
        /// Data de nascimento do utilizador.
        /// Campo obrigatório.
        /// </summary>
        [Required]
        public DateTime DataNascimento { get; set; }

        /// <summary>
        /// Número de telemóvel do utilizador (opcional).
        /// </summary>
        public string? Telemovel { get; set; }
    }
}