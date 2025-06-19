// Dependências
using System.Security.Claims;
using Microsoft.AspNetCore.Authentication.Cookies;
using Microsoft.AspNetCore.Authentication.Google;
using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;
using Microsoft.IdentityModel.Tokens;
using Microsoft.OpenApi.Models;
using SequeMusic.Data;
using SequeMusic.Models;
using System.Text;
using System.Text.Json;

var builder = WebApplication.CreateBuilder(args);

// -------------------------
// CREDENCIAIS GOOGLE OAUTH
// -------------------------
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
    Environment.Exit(1); // Encerra a app se falhar
}

// -----------------
// BASE DE DADOS
// -----------------
builder.Services.AddDbContext<ApplicationDbContext>(options =>
    options.UseSqlServer(builder.Configuration.GetConnectionString("DefaultConnection")));

// -----------------
// IDENTITY
// -----------------
builder.Services.AddIdentity<Utilizador, IdentityRole>()
    .AddEntityFrameworkStores<ApplicationDbContext>()
    .AddDefaultTokenProviders();

// -----------------
// JWT BEARER
// -----------------
var jwtKey = builder.Configuration["Jwt:Key"] ?? "CHAVE_SUPER_SECRETA_DEV_2025_SEGURA_XYZ123";
var jwtIssuer = builder.Configuration["Jwt:Issuer"] ?? "SequeMusicAPI";

builder.Services.AddAuthentication()
    .AddGoogle(googleOptions =>
    {
        googleOptions.ClientId = googleClientId!;
        googleOptions.ClientSecret = googleClientSecret!;
        googleOptions.CallbackPath = "/signin-google";
    })
    .AddJwtBearer(options =>
    {
        options.TokenValidationParameters = new TokenValidationParameters
        {
            ValidateIssuer = true,
            ValidateAudience = false,
            ValidateLifetime = true,
            ValidateIssuerSigningKey = true,
            ValidIssuer = jwtIssuer,
            IssuerSigningKey = new SymmetricSecurityKey(Encoding.UTF8.GetBytes(jwtKey))
        };
    });

// -----------------------------
// COOKIES DE AUTENTICAÇÃO
// -----------------------------
builder.Services.ConfigureApplicationCookie(options =>
{
    options.LoginPath = "/Utilizadors/Login";
    options.AccessDeniedPath = "/AccessDenied";
    options.Cookie.Name = "SequeMusic.Cookie";
    options.SlidingExpiration = true;

    options.Events.OnSigningIn = context =>
    {
        Console.WriteLine("Cookie de autenticação está sendo assinado para: " + context.Principal.Identity.Name);
        return Task.CompletedTask;
    };

    options.Events.OnSignedIn = async context =>
    {
        var identity = (ClaimsIdentity)context.Principal.Identity;
        if (!identity.HasClaim(c => c.Type == ClaimTypes.Name))
        {
            identity.AddClaim(new Claim(ClaimTypes.Name, context.Principal.Identity.Name));
        }
        await Task.CompletedTask;
    };
});

// -----------------
// CORS (Permissivo)
// -----------------
builder.Services.AddCors(options =>
{
    options.AddPolicy("AllowAll", builder =>
    {
        builder.AllowAnyOrigin()
               .AllowAnyHeader()
               .AllowAnyMethod();
    });
});

// ------------------------------
// MVC, JSON, Razor, Swagger
// ------------------------------
builder.Services.AddControllersWithViews()
    .AddJsonOptions(options =>
    {
        options.JsonSerializerOptions.ReferenceHandler = System.Text.Json.Serialization.ReferenceHandler.Preserve;
        options.JsonSerializerOptions.WriteIndented = true;
    });

builder.Services.AddRazorPages();
builder.Services.AddEndpointsApiExplorer();

builder.Services.AddSwaggerGen(c =>
{
    c.SwaggerDoc("v1", new OpenApiInfo { Title = "SequeMusic API", Version = "v1" });

    c.AddSecurityDefinition("Bearer", new OpenApiSecurityScheme
    {
        Description = "Insere o token JWT assim: Bearer {teu_token}",
        Name = "Authorization",
        In = ParameterLocation.Header,
        Type = SecuritySchemeType.ApiKey,
        Scheme = "Bearer"
    });

    c.AddSecurityRequirement(new OpenApiSecurityRequirement
    {
        {
            new OpenApiSecurityScheme
            {
                Reference = new OpenApiReference
                {
                    Type = ReferenceType.SecurityScheme,
                    Id = "Bearer"
                }
            },
            Array.Empty<string>()
        }
    });
});

// --------------------
// CONFIGURAÇÃO FINAL
// --------------------
var app = builder.Build();

// Swagger apenas em DEV
if (app.Environment.IsDevelopment())
{
    app.UseSwagger();
    app.UseSwaggerUI(options =>
    {
        options.SwaggerEndpoint("/swagger/v1/swagger.json", "SequeMusic API v1");
        options.RoutePrefix = "swagger";
    });
}

// Seed de Admin
using (var scope = app.Services.CreateScope())
{
    var services = scope.ServiceProvider;
    await SeedData.CriarUtilizadorAdmin(services);
}

// Middlewares
app.UseStaticFiles();
app.UseRouting();
app.UseCors("AllowAll");
app.UseAuthentication();
app.UseAuthorization();

// Rotas
app.MapControllerRoute(
    name: "default",
    pattern: "{controller=Home}/{action=Index}/{id?}");

app.MapRazorPages();
app.MapControllers();

app.Run();
