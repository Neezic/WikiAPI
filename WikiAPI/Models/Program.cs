using Microsoft.EntityFrameworkCore;
using WikiAPI.Data;
using WikiAPI.Models;

var builder = WebApplication.CreateBuilder(args);

// Configuração dos serviços
builder.Services.AddDbContext<ContextoWiki>(options => 
    options.UseSqlite(builder.Configuration.GetConnectionString("ContextoWiki")));

builder.Services.AddAuthentication("cookieAuth")
    .AddCookie("cookieAuth", options =>
    {
        options.Cookie.Name = "WikiAuth";
        options.LoginPath = "/api/auth/login";
        options.AccessDeniedPath = "/api/auth/acessonegado";
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
    options.AddPolicy("AllowFrontend", builder =>
    {
        builder.WithOrigins("http://localhost:5073") // Ajuste para o seu frontend
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

app.UseHttpsRedirection();
app.UseStaticFiles();

app.UseCookiePolicy(new CookiePolicyOptions
{
    MinimumSameSitePolicy = SameSiteMode.None,
    Secure = CookieSecurePolicy.Always
});

app.UseRouting();
app.UseCors("AllowFrontend");
app.UseAuthentication();
app.UseAuthorization();

app.MapControllers();

// Rota raiz para verificar se a API está funcionando
app.MapGet("/", () => "WikiAPI está funcionando! Acesse /swagger para a documentação da API.");

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

