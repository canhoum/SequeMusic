using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;
using Microsoft.AspNetCore.Http;
using SequeMusic.Data;
using SequeMusic.Models;
using SequeMusic.ViewModels;
using System.Security.Claims;

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

        // ---------------------- LOGIN ----------------------

        [AllowAnonymous]
        [HttpGet]
        public IActionResult Login() => View();

        [AllowAnonymous]
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Login(LoginRequest model)
        {
            if (!ModelState.IsValid) return View(model);

            var user = await _userManager.FindByEmailAsync(model.Email);
            if (user == null)
            {
                ModelState.AddModelError("Email", "Email não encontrado.");
                return View(model);
            }

            if (!await _userManager.CheckPasswordAsync(user, model.Password))
            {
                ModelState.AddModelError("Password", "Password incorreta.");
                return View(model);
            }

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

            ModelState.AddModelError(string.Empty, "Login inválido.");
            return View(model);
        }

        // ---------------------- REGISTO ----------------------

        [AllowAnonymous]
        [HttpGet]
        public IActionResult Register() => View("~/Views/Utilizadors/Register.cshtml");

        [AllowAnonymous]
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Register(RegisterRequest model)
        {
            if (!ModelState.IsValid) return View(model);

            var user = new Utilizador
            {
                UserName = model.Email,
                Email = model.Email,
                Nome = model.Nome,
                DataNascimento = model.DataNascimento,
                Telemovel = model.Telemovel,
                EmailConfirmed = true,
                IsPremium = false
            };

            var result = await _userManager.CreateAsync(user, model.Password);
            if (result.Succeeded)
            {
                await _signInManager.SignInAsync(user, isPersistent: false);
                return RedirectToAction("Index", "Home");
            }

            foreach (var error in result.Errors)
                ModelState.AddModelError(string.Empty, error.Description);

            return View(model);
        }

        // ---------------------- LOGOUT ----------------------

        public async Task<IActionResult> Logout()
        {
            Global.LoggedUser = null;
            await _signInManager.SignOutAsync();
            Response.Cookies.Delete("UserAuthCookie");
            return RedirectToAction("Index", "Home");
        }

        // ---------------------- LISTA DE UTILIZADORES ----------------------

        public async Task<IActionResult> Index()
        {
            var user = await _userManager.GetUserAsync(User);
            if (user == null || !user.IsAdmin)
                return View("~/Views/Utilizadors/AccessDenied.cshtml");

            var users = await _userManager.Users.ToListAsync();
            return View(users);
        }

        // ---------------------- DETALHES ----------------------

        public async Task<IActionResult> Details(string id)
        {
            if (id == null) return NotFound();

            var utilizador = await _userManager.Users.FirstOrDefaultAsync(u => u.Id == id);
            if (utilizador == null) return NotFound();

            return View(utilizador);
        }

        // ---------------------- EDITAR PERFIL ----------------------

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

            if (!ModelState.IsValid) return View(utilizador);

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

            return RedirectToAction("Details", new { id });
        }

        // ---------------------- APAGAR CONTA ----------------------

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
                return View("Delete", utilizador);
            }

            if (!isAdmin)
            {
                await _signInManager.SignOutAsync();
                Response.Cookies.Delete("UserAuthCookie");
                TempData["Mensagem"] = "A tua conta foi removida com sucesso.";
                return RedirectToAction("Index", "Home");
            }

            TempData["Mensagem"] = "Conta apagada com sucesso.";
            return RedirectToAction(nameof(Index));
        }

        // ---------------------- UPGRADE PARA PREMIUM ----------------------

        [Authorize]
        public async Task<IActionResult> Upgrade()
        {
            var user = await _userManager.GetUserAsync(User);
            if (user.IsPremium)
                return RedirectToAction("Details", new { id = user.Id });

            return View();
        }

        [HttpPost]
        [Authorize]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> ConfirmUpgrade(string nome, string cartao, string validade, string cvc)
        {
            var user = await _userManager.GetUserAsync(User);
            if (user == null) return NotFound();

            user.IsPremium = true;
            await _userManager.UpdateAsync(user);

            TempData["Mensagem"] = "Parabéns! A tua conta foi atualizada para Premium 🎉";
            return RedirectToAction("Details", new { id = user.Id });
        }

        // ---------------------- VERIFICAÇÃO ----------------------

        private async Task<bool> UtilizadorExists(string id)
        {
            return await _userManager.Users.AnyAsync(e => e.Id == id);
        }
    }
}
