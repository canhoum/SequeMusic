// ViewModel usado para o formulário de login do utilizador
// Contém os campos necessários para autenticação: Email, Password e opção "Remember Me"

using System.ComponentModel.DataAnnotations;

namespace SequeMusic.ViewModels
{
    public class LoginViewModel
    {
        [Required]
        [EmailAddress]
        public string Email { get; set; } 
        // Email do utilizador (obrigatório e validado como endereço de email)

        [Required]
        public string Password { get; set; } 
        // Password do utilizador (obrigatória)

        public bool RememberMe { get; set; } = false; 
        // Indica se o utilizador pretende manter sessão iniciada (login persistente)
    }
}