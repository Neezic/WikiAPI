var builder = WebApplication.CreateBuilder(args);

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

builder.Services.AddDbContext<ContextoWiki>(options => 
    options.UseSqlite(builder.Configuration.GetConnectionString("ContextoWiki")));

using (var escopo = app.Services.CreateScope()){
    var servicos = escopo.ServiceProvider;
    var contexto = servicos.GetRequiredService<ContextoWiki>();
    contexto.Database.EnsuredCreated();
}