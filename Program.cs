using Microsoft.AspNetCore.Authentication.Cookies;
using Microsoft.AspNetCore.Authentication.Google;
using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;
using SequeMusic.Data;
using SequeMusic.Models;
using System.Text.Json;

var builder = WebApplication.CreateBuilder(args);

// --------------------
// Carregar credenciais Google OAuth do ficheiro JSON
// --------------------
string? googleClientId = null;
string? googleClientSecret = null;

try
{
    var googleJsonPath = Path.Combine(Directory.GetCurrentDirectory(), "auth", "google-credentials.json");

    if (!File.Exists(googleJsonPath))
        throw new FileNotFoundException("Ficheiro de credenciais Google não encontrado.", googleJsonPath);

    var json = File.ReadAllText(googleJsonPath);
    using var doc = JsonDocument.Parse(json);
    var googleData = doc.RootElement.GetProperty("web");

    googleClientId = googleData.GetProperty("client_id").GetString();
    googleClientSecret = googleData.GetProperty("client_secret").GetString();

    if (string.IsNullOrEmpty(googleClientId) || string.IsNullOrEmpty(googleClientSecret))
        throw new Exception("client_id ou client_secret está vazio no ficheiro JSON.");
}
catch (Exception ex)
{
    Console.WriteLine("⚠️ Erro ao carregar as credenciais do Google:");
    Console.WriteLine(ex.Message);
    Environment.Exit(1);
}

// --------------------
// Configurar a Base de Dados
// --------------------
builder.Services.AddDbContext<ApplicationDbContext>(options =>
    options.UseSqlServer(builder.Configuration.GetConnectionString("DefaultConnection")));

// --------------------
// Configurar Identity
// --------------------
builder.Services.AddIdentity<Utilizador, IdentityRole>()
    .AddEntityFrameworkStores<ApplicationDbContext>()
    .AddDefaultTokenProviders();

// --------------------
// Configurar autenticação com cookies e Google (sem handler externo)
// --------------------
builder.Services.AddAuthentication(options =>
{
    options.DefaultScheme = CookieAuthenticationDefaults.AuthenticationScheme;
    options.DefaultChallengeScheme = GoogleDefaults.AuthenticationScheme;
})
.AddCookie()
.AddGoogle(googleOptions =>
{
    googleOptions.ClientId = googleClientId!;
    googleOptions.ClientSecret = googleClientSecret!;
    googleOptions.CallbackPath = "/signin-google";
});

// --------------------
// Configuração personalizada para cookies de autenticação
// --------------------
builder.Services.ConfigureApplicationCookie(options =>
{
    options.LoginPath = "/Utilizadors/Login";
    options.AccessDeniedPath = "/AccessDenied";

    options.Events.OnRedirectToLogin = context =>
    {
        context.Response.StatusCode = 401;
        return Task.CompletedTask;
    };
});

// --------------------
// Configurar CORS (permitir todos os pedidos para desenvolvimento)
// --------------------
builder.Services.AddCors(options =>
{
    options.AddPolicy("AllowAll", builder =>
    {
        builder.AllowAnyOrigin()
               .AllowAnyHeader()
               .AllowAnyMethod();
    });
});

// --------------------
// MVC + Razor Pages
// --------------------
builder.Services.AddControllersWithViews();
builder.Services.AddRazorPages();

var app = builder.Build();

// --------------------
// Pipeline HTTP
// --------------------
if (!app.Environment.IsDevelopment())
{
    app.UseExceptionHandler("/Home/Error");
    app.UseHsts();
}

// ** Comentar esta linha para evitar redirecionamento obrigatório para HTTPS **
// app.UseHttpsRedirection();

app.UseStaticFiles();

app.UseRouting();

app.UseCors("AllowAll");

app.UseAuthentication();
app.UseAuthorization();

// --------------------
// Rotas MVC e Razor Pages
// --------------------
app.MapControllerRoute(
    name: "default",
    pattern: "{controller=Home}/{action=Index}/{id?}");

app.MapRazorPages();

app.Run();
