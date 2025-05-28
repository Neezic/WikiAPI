using Microsoft.AspNetCore.Authentication.Cookies;
using Microsoft.EntityFrameworkCore;
using WikiAPI.Data;
using WikiAPI.Models;

var builder = WebApplication.CreateBuilder(args);

// Add services to the container.
builder.Services.AddControllers();

// Configure Database Context
builder.Services.AddDbContext<ContextoWiki>(options =>
    options.UseSqlite(builder.Configuration.GetConnectionString("ContextoWiki")));

// Configure Authentication
builder.Services.AddAuthentication(CookieAuthenticationDefaults.AuthenticationScheme)
    .AddCookie(options =>
    {
        options.Cookie.Name = "WikiAuth";
        options.LoginPath = "/api/auth/login";
        options.AccessDeniedPath = "/api/auth/acessonegado";
        options.Cookie.HttpOnly = true;
        options.Cookie.SecurePolicy = builder.Environment.IsDevelopment() 
            ? CookieSecurePolicy.None 
            : CookieSecurePolicy.Always;
        options.Cookie.SameSite = SameSiteMode.Lax;
        options.ExpireTimeSpan = TimeSpan.FromHours(3);
    });

// Configure Authorization
builder.Services.AddAuthorization(options =>
{
    options.AddPolicy("Editor", policy => 
        policy.RequireClaim("Perfil", "Editor"));
});

// Configure CORS
builder.Services.AddCors(options =>
{
    options.AddPolicy("AllowAll", policy =>
    {
        policy.AllowAnyOrigin()
              .AllowAnyMethod()
              .AllowAnyHeader();
    });
});

var app = builder.Build();

// Configure the HTTP request pipeline.
if (app.Environment.IsDevelopment())
{
    app.UseDeveloperExceptionPage();
}

app.UseHttpsRedirection();
app.UseStaticFiles();

app.UseRouting();

app.UseCors("AllowAll");

app.UseAuthentication();
app.UseAuthorization();

app.MapControllers();

// SPA fallback
app.MapFallbackToFile("index.html");

if (app.Environment.IsDevelopment())
{
    app.UseCors("AllowAll");
}

// Seed inicial do banco de dados
using (var escopo = app.Services.CreateScope())
{
    var servicos = escopo.ServiceProvider;
    var contexto = servicos.GetRequiredService<ContextoWiki>();
    
    if (!contexto.Usuario.Any(u => u.Email == "editor@wiki.com"))
    {
        contexto.Usuario.Add(new Usuario
        {
            Nome = "Editor Principal",
            Email = "editor@wiki.com",
            senhaHash = Convert.ToBase64String(System.Text.Encoding.UTF8.GetBytes("senha123")),
            Perfil = "Editor"
        });
        await contexto.SaveChangesAsync();
    }
    contexto.Database.EnsureCreated();
}

app.Run();

