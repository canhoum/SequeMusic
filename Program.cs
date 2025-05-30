using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;
using SequeMusic.Data;
using SequeMusic.Models;

var builder = WebApplication.CreateBuilder(args);

// --------------------
// Configurar a Base de Dados
// --------------------
builder.Services.AddDbContext<ApplicationDbContext>(options =>
    options.UseSqlServer(builder.Configuration.GetConnectionString("DefaultConnection")));


// --------------------
// Configurar o Identity
// --------------------
builder.Services.AddIdentity<Utilizador, IdentityRole>()
    .AddEntityFrameworkStores<ApplicationDbContext>()
    .AddDefaultTokenProviders();

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
// Ativar CORS
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
// MVC + API
// --------------------
builder.Services.AddControllersWithViews();
builder.Services.AddControllers();
builder.Services.AddRazorPages(); // <- Faltava esta linha!!

var app = builder.Build();

// --------------------
// Pipeline
// --------------------
if (!app.Environment.IsDevelopment())
{
    app.UseExceptionHandler("/Home/Error");
    app.UseHsts();
}


// --------------------
// Role de Admin
// --------------------
using (var scope = app.Services.CreateScope())
{
    var services = scope.ServiceProvider;
    await SeedData.CriarUtilizadorAdmin(services);
}



app.UseHttpsRedirection();
app.UseStaticFiles();

app.UseRouting();

app.UseCors("AllowAll");

app.UseAuthentication();
app.UseAuthorization();

// --------------------
// Rotas MVC
// --------------------
app.MapControllerRoute(
    name: "default",
    pattern: "{controller=Home}/{action=Index}/{id?}");

// --------------------
// Rotas API
// --------------------
app.MapControllers();

// --------------------
// Razor Pages
// --------------------
app.MapRazorPages(); // <-- Mapeia Razor Pages ANTES de Run!

app.Run(); // <-- ÚLTIMA LINHA
