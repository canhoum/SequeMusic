using System.ComponentModel.DataAnnotations;

namespace SequeMusic.ViewModels
{
    public class RegisterRequest
    {
        [Required]
        [StringLength(50)]
        public string Nome { get; set; }

        [Required]
        [EmailAddress]
        public string Email { get; set; }

        [Required]
        public string Password { get; set; }

        [Required]
        public DateOnly DataNascimento { get; set; }

        public string? Telemovel { get; set; }
    }
}