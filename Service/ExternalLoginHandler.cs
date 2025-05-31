// using Microsoft.AspNetCore.Identity;
// using SequeMusic.Models;
// using System.Security.Claims;

// namespace SequeMusic.Services
// {
//     public class ExternalLoginHandler
//     {
//         private readonly UserManager<Utilizador> _userManager;
//         private readonly SignInManager<Utilizador> _signInManager;

//         public ExternalLoginHandler(UserManager<Utilizador> userManager, SignInManager<Utilizador> signInManager)
//         {
//             _userManager = userManager;
//             _signInManager = signInManager;
//         }

//         public async Task HandleGoogleLoginAsync(ClaimsPrincipal principal)
//         {
//             var email = principal.FindFirstValue(ClaimTypes.Email);
//             var nome = principal.FindFirstValue(ClaimTypes.Name);

//             if (string.IsNullOrEmpty(email)) return;

//             var utilizador = await _userManager.FindByEmailAsync(email);

//             if (utilizador == null)
//             {
//                 utilizador = new Utilizador
//                 {
//                     UserName = email,
//                     Email = email,
//                     Nome = nome
//                 };

//                 await _userManager.CreateAsync(utilizador);
//             }

//             await _signInManager.SignInAsync(utilizador, isPersistent: false);
//         }
//     }
// }
