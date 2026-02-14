using ServidorLanches.Controllers;
using ServidorLanches.model;
using ServidorLanches.Repositories;
using ServidorLanches.repository;
using ServidorLanches.service;
using ServidorLanches.Services;

var builder = WebApplication.CreateBuilder(args);

builder.Services.AddControllers();

builder.Services.AddDistributedMemoryCache();
builder.Services.AddSession(options =>
{
    options.IdleTimeout = TimeSpan.FromMinutes(30);
    options.Cookie.HttpOnly = true;
    options.Cookie.IsEssential = true;
});


//isso aqui faz ele abrir e aceitar todas as conexoes
builder.WebHost.UseUrls("http://0.0.0.0:5000");

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
builder.Services.AddScoped<ProdutoRepository>();
builder.Services.AddScoped<ProdutoService>();
builder.Services.AddScoped<AdministrativoRepository>();
builder.Services.AddScoped<AdministrativoService>();
builder.Services.AddScoped<EstoqueRepository>();
builder.Services.AddScoped<EstoqueService>();
builder.Services.AddScoped<CaixaRepository>();
builder.Services.AddScoped<CaixaService>();
builder.Services.AddScoped<ConsignacaoRepository>();
builder.Services.AddScoped<ConsignacaoService>();
builder.Services.AddHealthChecks();
builder.Services.AddSingleton<DbConnectionManager>();


builder.Configuration
    .SetBasePath(AppDomain.CurrentDomain.BaseDirectory)
    .AddJsonFile("appsettings.json", optional: false, reloadOnChange: true);



// 2. Registre os serviços
builder.Services.AddControllers();
builder.Services.AddSingleton<DbConnectionManager>(); // O Singleton lerá o config acima

// ... os outros registros (Repositories, Services, etc)
builder.Services.AddScoped<UsuarioRepository>();
// ...

var app = builder.Build();
app.MapHealthChecks("/ping");
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
