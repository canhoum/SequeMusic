using Microsoft.AspNetCore.Identity;
using Microsoft.Extensions.DependencyInjection;
using SequeMusic.Models;

namespace SequeMusic.Data
{
    public static class SeedData
    {
        public static async Task CriarUtilizadorAdmin(IServiceProvider serviceProvider)
        {
            var userManager = serviceProvider.GetRequiredService<UserManager<Utilizador>>();
            var roleManager = serviceProvider.GetRequiredService<RoleManager<IdentityRole>>();

            string email = "admin@sequemusic.com";
            string password = "Admin123!";
            string roleName = "Admin";

            // Criar role se não existir
            if (!await roleManager.RoleExistsAsync(roleName))
            {
                await roleManager.CreateAsync(new IdentityRole(roleName));
            }

            // Verificar se o utilizador já existe
            var adminUser = await userManager.FindByEmailAsync(email);
            if (adminUser == null)
            {
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

                var result = await userManager.CreateAsync(novoAdmin, password);
                if (result.Succeeded)
                {
                    await userManager.AddToRoleAsync(novoAdmin, roleName);
                }
            }
        }
    }
}