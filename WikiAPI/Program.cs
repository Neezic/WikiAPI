using Microsoft.EntityFrameworkCore;
using Microsoft.AspNetCore.StaticFiles;
using WikiAPI.Data;
using WikiAPI.Models;
using Microsoft.AspNetCore.Authentication.Cookies;

var builder = WebApplication.CreateBuilder(args);

// Configuração dos serviços
builder.Services.AddDbContext<ContextoWiki>(options => 
    options.UseSqlite(builder.Configuration.GetConnectionString("ContextoWiki")));

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

builder.Services.Configure<CookiePolicyOptions>(options => {
    options.MinimumSameSitePolicy = SameSiteMode.None;
    options.Secure = CookieSecurePolicy.SameAsRequest;
});

builder.Services.AddAuthorization(options =>
{
    options.AddPolicy("Editor", policy =>
        policy.RequireAssertion(context =>
            context.User.HasClaim(c =>
                c.Type == "Perfil" && c.Value == "Editor")));
});

builder.Services.AddCors(options =>
{
    options.AddPolicy("AllowFrontend", policy =>
    {
        policy.WithOrigins("http://localhost:5073") // Ajuste para o seu frontend
            .AllowAnyMethod()
            .AllowAnyHeader()
            .AllowCredentials();
    });
});

builder.Services.AddControllers(); // Adiciona suporte a controllers
builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen();

var app = builder.Build();

// Configuração do pipeline HTTP
if (app.Environment.IsDevelopment())
{
    app.UseSwagger();
    app.UseSwaggerUI();
}


app.UseStaticFiles();  // 1° - Arquivos estáticos
app.UseRouting();      // 2° - Roteamento
app.UseAuthentication();
app.UseAuthorization();// 3° - Autenticação

// Mapeamento CRÍTICO (atenção à ordem):
app.MapControllers();  // Primeiro mapeia os endpoints da API
app.UseCors("AllowAll");
// Só então o fallback para SPA (deve vir DEPOIS dos controllers)
app.MapWhen(ctx => !ctx.Request.Path.StartsWithSegments("/api"), appBuilder => {
    appBuilder.UseRouting();
    app.UseAuthorization();
    appBuilder.UseEndpoints(endpoints => {
        endpoints.MapFallbackToFile("index.html");
    });
});


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

