using Microsoft.AspNetCore.Authorization; 
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Microsoft.AspNetCore.Identity;
using SequeMusic.Data;
using SequeMusic.Models;
using SequeMusic.ViewModels;
using Microsoft.AspNetCore.Http;
using System.Security.Claims;
using Microsoft.AspNetCore.Authentication;
using Microsoft.AspNetCore.Authentication.Cookies;

namespace SequeMusic.Controllers
{
    [Authorize]
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
        public IActionResult Login() => View();

        [AllowAnonymous]
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Login(LoginViewModel model)
        
        {
            if (ModelState.IsValid)
            {
                try
                {
                    var user = await _userManager.FindByEmailAsync(model.Email);
                    if (user == null)
                        ModelState.AddModelError("Email", "Email nÃ£o encontrado.");
                    else if (!await _userManager.CheckPasswordAsync(user, model.Password))
                        ModelState.AddModelError("Password", "Password incorreta.");
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

                            // Verifica se o utilizador tem dados em falta
                            if (string.IsNullOrEmpty(user.Telemovel) || user.DataNascimento == default)
                            {
                                return RedirectToAction("CompletarPerfil", new { id = user.Id });
                            }
                            var identity = new ClaimsIdentity(CookieAuthenticationDefaults.AuthenticationScheme);
                            identity.AddClaim(new Claim(ClaimTypes.NameIdentifier, user.Id.ToString()));
                            identity.AddClaim(new Claim(ClaimTypes.Name, user.Email));

                            var principal = new ClaimsPrincipal(identity);
                            await _signInManager.SignInAsync(user,model.RememberMe);
                           return RedirectToAction("Index", "Home");
                        }

                    }
                }
                catch
                {
                    return RedirectToAction("Error", "Home");
                }
            }

            return View(model);
        }

        [AllowAnonymous]
        [HttpPost]
        public IActionResult ExternalLogin(string provider, string returnUrl = null)
        {
            var redirectUrl = Url.Action("ExternalLoginCallback", "Utilizadors", new { returnUrl });
            var properties = _signInManager.ConfigureExternalAuthenticationProperties(provider, redirectUrl);
            return Challenge(properties, provider);
        }

        [AllowAnonymous]
        public async Task<IActionResult> ExternalLoginCallback(string returnUrl = null, string remoteError = null)
        {
            returnUrl ??= Url.Content("~/");

            if (remoteError != null)
            {
                ModelState.AddModelError(string.Empty, $"Erro de login externo: {remoteError}");
                return RedirectToAction(nameof(Login));
            }

            var info = await _signInManager.GetExternalLoginInfoAsync();
            if (info == null) return RedirectToAction(nameof(Login));

            var email = info.Principal.FindFirstValue(ClaimTypes.Email);
            var nome = info.Principal.FindFirstValue(ClaimTypes.Name);

            var utilizador = await _userManager.FindByEmailAsync(email);

            if (utilizador == null)
            {
                utilizador = new Utilizador
                {
                    UserName = email,
                    Email = email,
                    Nome = nome,
                    EmailConfirmed = true
                };

                var createResult = await _userManager.CreateAsync(utilizador);
                if (!createResult.Succeeded)
                {
                    foreach (var error in createResult.Errors)
                        ModelState.AddModelError(string.Empty, error.Description);
                    return RedirectToAction(nameof(Login));
                }

                await _userManager.AddLoginAsync(utilizador, info);
            }

            await _signInManager.SignInAsync(utilizador, isPersistent: false);

            // Verifica se o utilizador precisa de completar o perfil
            if (utilizador.DataNascimento == default || string.IsNullOrWhiteSpace(utilizador.Telemovel))
            {
                return RedirectToAction("CompletarPerfil", new { id = utilizador.Id });
            }


            return LocalRedirect(returnUrl);

        }

        [AllowAnonymous]
        [HttpGet]
        public IActionResult Register() => View("~/Views/Utilizadors/Register.cshtml");

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
                    await _signInManager.SignInAsync(user, isPersistent: false);
                    return RedirectToAction("Index", "Home");
                }

                foreach (var error in result.Errors)
                    ModelState.AddModelError(string.Empty, error.Description);
            }

            return View(model);
        }

        public async Task<IActionResult> Index()
        {
            var userId = _userManager.GetUserId(User);
            var user = await _userManager.FindByIdAsync(userId);
            if (user == null || !user.IsAdmin)
                return View("~/Views/Utilizadors/AccessDenied.cshtml");

            var users = await _userManager.Users.ToListAsync();
            return View(users);
        }

        public async Task<IActionResult> Logout()
        {
            Global.LoggedUser = null;
            await _signInManager.SignOutAsync();
            Response.Cookies.Delete("UserAuthCookie");
            return RedirectToAction("Index", "Home");
        }

        public async Task<IActionResult> Details(string id)
        {
            if (id == null) return NotFound();
            var utilizador = await _userManager.Users.FirstOrDefaultAsync(u => u.Id == id);
            if (utilizador == null) return NotFound();
            return View(utilizador);
        }

        public async Task<IActionResult> Edit(string id)
        {
            if (id == null) return NotFound();
            var userId = _userManager.GetUserId(User);
            var isAdmin = User.IsInRole("Admin");
            if (id != userId && !isAdmin) return Forbid();

            var utilizador = await _userManager.Users.FirstOrDefaultAsync(u => u.Id == id);
            if (utilizador == null) return NotFound();
            return View(utilizador);
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Edit(string id, [Bind("Id,Nome,Email,Telemovel,DataNascimento")] Utilizador utilizador)
        {
            if (id != utilizador.Id) return NotFound();
            var userId = _userManager.GetUserId(User);
            var isAdmin = User.IsInRole("Admin");
            if (id != userId && !isAdmin) return Forbid();

            if (ModelState.IsValid)
            {
                try
                {
                    var userToUpdate = await _userManager.FindByIdAsync(id);
                    if (userToUpdate == null) return NotFound();

                    userToUpdate.Nome = utilizador.Nome;
                    userToUpdate.Email = utilizador.Email;
                    userToUpdate.UserName = utilizador.Email;
                    userToUpdate.Telemovel = utilizador.Telemovel;
                    userToUpdate.DataNascimento = utilizador.DataNascimento;

                    var result = await _userManager.UpdateAsync(userToUpdate);
                    if (!result.Succeeded)
                    {
                        foreach (var error in result.Errors)
                            ModelState.AddModelError(string.Empty, error.Description);
                        return View(utilizador);
                    }
                }
                catch (DbUpdateConcurrencyException)
                {
                    if (!await UtilizadorExists(utilizador.Id)) return NotFound();
                    else throw;
                }

                return RedirectToAction("Details", new { id = id });
            }

            return View(utilizador);
        }

        public async Task<IActionResult> Delete(string id)
        {
            if (id == null) return NotFound();
            var userId = _userManager.GetUserId(User);
            var isAdmin = User.IsInRole("Admin");
            if (id != userId && !isAdmin) return Forbid();

            var utilizador = await _userManager.Users.FirstOrDefaultAsync(u => u.Id == id);
            if (utilizador == null) return NotFound();
            return View(utilizador);
        }

        [HttpPost, ActionName("Delete")]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> DeleteConfirmed(string id)
        {
            var userId = _userManager.GetUserId(User);
            var isAdmin = User.IsInRole("Admin");
            if (id != userId && !isAdmin) return Forbid();

            var utilizador = await _userManager.FindByIdAsync(id);
            if (utilizador == null) return NotFound();

            var result = await _userManager.DeleteAsync(utilizador);
            if (!result.Succeeded)
            {
                foreach (var error in result.Errors)
                    ModelState.AddModelError(string.Empty, error.Description);
                return View(utilizador);
            }

            if (!isAdmin)
            {
                await _signInManager.SignOutAsync();
                Response.Cookies.Delete("UserAuthCookie");
                return RedirectToAction("Index", "Home");
            }

            return RedirectToAction(nameof(Index));
        }
        [HttpGet]
        public async Task<IActionResult> Upgrade()
        {
            var user = await _userManager.GetUserAsync(User);
            if (user == null) return NotFound();

            if (user.IsPremium)
            {
                TempData["Mensagem"] = "Já tens uma conta Premium!";
                return RedirectToAction("Details", new { id = user.Id });
            }

            return View(); // Vai carregar a view Upgrade.cshtml
        }
        
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> ConfirmUpgrade(string nome, string cartao, string validade, string cvc)
        {
            var user = await _userManager.GetUserAsync(User);
            if (user == null) return NotFound();

            // Simulação: pagamento aceite
            user.IsPremium = true;
            await _userManager.UpdateAsync(user);

            TempData["Mensagem"] = "Parabéns! A tua conta foi atualizada para Premium 🎉";
            return RedirectToAction("Details", new { id = user.Id });
        }


// ---------------------- COMPLETAR PERFIL ----------------------

        [HttpGet]
        public async Task<IActionResult> CompletarPerfil(string id)
        {
            if (id == null) return NotFound();

            var userId = _userManager.GetUserId(User);
            if (id != userId) return Forbid();

            var utilizador = await _userManager.FindByIdAsync(id);
            if (utilizador == null) return NotFound();

            return View(utilizador);
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> CompletarPerfil(string id, [Bind("Telemovel,DataNascimento")] Utilizador dados)
        {
            var user = await _userManager.FindByIdAsync(id);
            if (user == null) return NotFound();

            // Atualiza os dados
            user.Telemovel = dados.Telemovel;
            user.DataNascimento = dados.DataNascimento;

            var result = await _userManager.UpdateAsync(user);
            if (!result.Succeeded)
            {
                foreach (var error in result.Errors)
                    ModelState.AddModelError(string.Empty, error.Description);

                return View(dados); // Mostra novamente o formulário com erros
            }

            // Se tudo correu bem, redireciona para a página de detalhes
            TempData["Mensagem"] = "Perfil atualizado com sucesso.";
            return RedirectToAction("Details", new { id = user.Id });
        }


        private async Task<bool> UtilizadorExists(string id)
        {
            return await _userManager.Users.AnyAsync(e => e.Id == id);
        }
    }
}