using Microsoft.AspNetCore.Identity;
using Microsoft.Extensions.DependencyInjection;
using SequeMusic.Models;

namespace SequeMusic.Data
{
    /// <summary>
    /// Classe estática responsável por inserir dados iniciais (seed) na base de dados.
    /// Neste caso, cria automaticamente um utilizador administrador ao iniciar a aplicação.
    /// </summary>
    public static class SeedData
    {
        /// <summary>
        /// Cria um utilizador admin com as suas respectivas permissões, caso ainda não exista.
        /// </summary>
        /// <param name="serviceProvider">Fornece os serviços necessários, como UserManager e RoleManager.</param>
        public static async Task CriarUtilizadorAdmin(IServiceProvider serviceProvider)
        {
            // Obtém os serviços necessários para criar utilizadores e roles
            var userManager = serviceProvider.GetRequiredService<UserManager<Utilizador>>();
            var roleManager = serviceProvider.GetRequiredService<RoleManager<IdentityRole>>();

            // Dados do utilizador admin
            string email = "admin@sequemusic.com";
            string password = "Admin123!";
            string roleName = "Admin";

            // Verifica se a role "Admin" já existe, e cria se necessário
            if (!await roleManager.RoleExistsAsync(roleName))
            {
                await roleManager.CreateAsync(new IdentityRole(roleName));
            }

            // Verifica se já existe um utilizador com este email
            var adminUser = await userManager.FindByEmailAsync(email);
            if (adminUser == null)
            {
                // Cria novo utilizador admin com dados predefinidos
                var novoAdmin = new Utilizador
                {
                    UserName = email,
                    Email = email,
                    Nome = "Administrador",
                    EmailConfirmed = true,
                    IsAdmin = true,
                    DataNascimento = new DateTime(2003, 11, 24),
                    Telemovel = "912345678"
                };

                // Cria o utilizador na base de dados com a password definida
                var result = await userManager.CreateAsync(novoAdmin, password);
                if (result.Succeeded)
                {
                    // Associa o utilizador à role "Admin"
                    await userManager.AddToRoleAsync(novoAdmin, roleName);
                }
            }
        }
    }
}
