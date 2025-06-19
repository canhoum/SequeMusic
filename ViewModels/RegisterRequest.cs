// ViewModel usado para o formulário de registo de um novo utilizador
// Contém os campos obrigatórios e opcionais necessários para criar uma conta

using System.ComponentModel.DataAnnotations;

namespace SequeMusic.ViewModels
{
    public class RegisterRequest
    {
        [Required]
        [StringLength(50)]
        public string Nome { get; set; } 
        // Nome completo do utilizador (obrigatório, máximo 50 caracteres)

        [Required]
        [EmailAddress]
        public string Email { get; set; } 
        // Email do utilizador (obrigatório e validado como endereço de email)

        [Required]
        public string Password { get; set; } 
        // Password definida pelo utilizador (obrigatória)

        [Required]
        public DateTime DataNascimento { get; set; } 
        // Data de nascimento (obrigatória)

        public string? Telemovel { get; set; } 
        // Telemóvel (opcional)
    }
}