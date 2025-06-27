using System.ComponentModel.DataAnnotations;

namespace SequeMusic.ViewModels
{
    /// <summary>
    /// ViewModel usado para o formulário de login do utilizador.
    /// Contém os campos necessários para autenticação: Email, Password e RememberMe.
    /// </summary>
    public class LoginViewModel
    {
        /// <summary>
        /// Endereço de email do utilizador.
        /// Campo obrigatório e validado como email.
        /// </summary>
        [Required]
        [EmailAddress]
        public string Email { get; set; }

        /// <summary>
        /// Palavra-passe do utilizador.
        /// Campo obrigatório.
        /// </summary>
        [Required]
        public string Password { get; set; }

        /// <summary>
        /// Indica se o utilizador deseja manter a sessão iniciada após o login.
        /// </summary>
        public bool RememberMe { get; set; } = false;
    }
}