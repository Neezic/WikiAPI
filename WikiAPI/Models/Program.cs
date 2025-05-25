using Microsoft.EntityFrameworkCore;
using WikiAPI;
using WikiAPI.Data;

var builder = WebApplication.CreateBuilder(args);

builder.Services.AddDbContext<ContextoWiki>(options => options.UseSqlite(builder.Configuration.GetConnectionString("ContextoWiki")));
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

// Add services to the container.
// Learn more about configuring Swagger/OpenAPI at https://aka.ms/aspnetcore/swashbuckle
builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen();

var app = builder.Build();

// Configure the HTTP request pipeline.
if (app.Environment.IsDevelopment())
{
    app.UseSwagger();
    app.UseSwaggerUI();
}

app.UseHttpsRedirection();

using (var escopo = app.Services.CreateScope())
{
    var servicos = escopo.ServiceProvider;
    var contexto = servicos.GetRequiredService<ContextoWiki>();
    contexto.Database.EnsureCreated();
}


app.Run();

