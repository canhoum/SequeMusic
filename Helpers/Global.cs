using SequeMusic.Models;

namespace SequeMusic
{
    /// <summary>
    /// Classe Global para guardar o utilizador que está logado.
    /// Global class to store the currently logged-in user.
    /// </summary>
    public static class Global
    {
        public static Utilizador LoggedUser { get; set; }
    }
}