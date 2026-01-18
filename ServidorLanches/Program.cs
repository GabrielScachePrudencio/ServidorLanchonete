using ServidorLanches.Services;
using ServidorLanches.Repositories;
using ServidorLanches.repository;
using ServidorLanches.service;

var builder = WebApplication.CreateBuilder(args);

builder.Services.AddControllers();

builder.Services.AddDistributedMemoryCache();
builder.Services.AddSession(options =>
{
    options.IdleTimeout = TimeSpan.FromMinutes(30);
    options.Cookie.HttpOnly = true;
    options.Cookie.IsEssential = true;
});



builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen();

builder.Logging.ClearProviders();
builder.Logging.AddConsole();
builder.Logging.SetMinimumLevel(Microsoft.Extensions.Logging.LogLevel.Debug);



// 🔥 REGISTRA TUDO QUE USA NO CONSTRUTOR
builder.Services.AddScoped<UsuarioRepository>();
builder.Services.AddScoped<UsuarioService>();
builder.Services.AddScoped<PedidosRepository>();
builder.Services.AddScoped<PedidosService>();
builder.Services.AddScoped<CardapioRepository>();
builder.Services.AddScoped<CardapioService>();
builder.Services.AddScoped<AdministrativoRepository>();
builder.Services.AddScoped<AdministrativoService>();

var app = builder.Build();

app.UseSession();

if (app.Environment.IsDevelopment())
{
    app.UseDeveloperExceptionPage();
    app.UseSwagger();
    app.UseSwaggerUI();
}

app.UseHttpsRedirection();
app.UseAuthorization();
app.MapControllers();
app.Run();
