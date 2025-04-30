using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Microsoft.AspNetCore.Identity;
using SequeMusic.Data;
using SequeMusic.Models;
using SequeMusic.ViewModels;
using Microsoft.AspNetCore.Http;

namespace SequeMusic.Controllers
{
    [Authorize] // <- Adicionar aqui
    public class UtilizadorsController : Controller
    {
        private readonly ApplicationDbContext _context;
        private readonly UserManager<Utilizador> _userManager;
        private readonly SignInManager<Utilizador> _signInManager;

        public UtilizadorsController(ApplicationDbContext context, UserManager<Utilizador> userManager, SignInManager<Utilizador> signInManager)
        {
            _context = context;
            _userManager = userManager;
            _signInManager = signInManager;
        }

        [AllowAnonymous]
        [HttpGet]
        public IActionResult Login()
        {
            return View();
        }

        [AllowAnonymous]
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Login(Utilizador model)
        {
            ModelState.Remove("Nome");
            ModelState.Remove("Telemovel");
            ModelState.Remove("DataNascimento");
            if (ModelState.IsValid)
            {
                try
                {
                    var user = await _userManager.FindByEmailAsync(model.Email);
                    if (user == null)
                    {
                        ModelState.AddModelError("Email", "Email não encontrado.");
                    }
                    else if (!await _userManager.CheckPasswordAsync(user, model.Password))
                    {
                        ModelState.AddModelError("Password", "Password incorreta.");
                    }
                    else
                    {
                        var result = await _signInManager.PasswordSignInAsync(user, model.Password, model.RememberMe, false);
                        if (result.Succeeded)
                        {
                            Global.LoggedUser = user;

                            if (model.RememberMe)
                            {
                                var cookieOptions = new CookieOptions
                                {
                                    Expires = DateTime.Now.AddDays(30),
                                    Secure = true,
                                    HttpOnly = true,
                                    SameSite = SameSiteMode.Strict
                                };
                                Response.Cookies.Append("UserAuthCookie", user.Id, cookieOptions);
                            }

                            return RedirectToAction("Index", "Home");
                        }
                    }
                }
                catch
                {
                    return RedirectToAction("Error", "Home"); // Se quiseres tratar falhas gerais
                }
            }
            return View(model);
        }

        [AllowAnonymous]
        [HttpGet]
        public IActionResult Register()
        {
            return View("~/Views/Utilizadors/Register.cshtml");
        }

        [AllowAnonymous]
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Register(RegisterRequest model)
        {
            if (ModelState.IsValid)
            {
                var user = new Utilizador
                {
                    UserName = model.Email,
                    Email = model.Email,
                    Nome = model.Nome,
                    DataNascimento = model.DataNascimento,
                    Telemovel = model.Telemovel,
                    EmailConfirmed = true
                };

                var result = await _userManager.CreateAsync(user, model.Password);

                if (result.Succeeded)
                {
                    // Aqui é onde adicionas o código
                    var createdUser = await _userManager.FindByEmailAsync(model.Email);
                    Console.WriteLine($"UTILIZADOR CRIADO: {createdUser.Id} - {createdUser.Email}");

                    await _signInManager.SignInAsync(user, isPersistent: false);
                    return RedirectToAction("Index", "Home");
                }
                else
                {
                    foreach (var error in result.Errors)
                    {
                        ModelState.AddModelError(string.Empty, error.Description);
                    }
                }
            }
            return View(model);
        }


        public async Task<IActionResult> Index()
        {
            var users = await _context.Users.ToListAsync();
            return View(users);
        }

        public async Task<IActionResult> Logout()
        {
            Global.LoggedUser = null;
            await _signInManager.SignOutAsync();
            Response.Cookies.Delete("UserAuthCookie");
            return RedirectToAction("Index", "Home");
        }
    }
}
