public class LoginApiRequest
{
    // Email do utilizador, fornecido no corpo da requisição
    public string Email { get; set; }

    // Palavra-passe do utilizador, fornecida no corpo da requisição
    public string Password { get; set; }
}