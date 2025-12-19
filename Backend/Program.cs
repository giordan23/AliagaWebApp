using Backend.Data;
using Backend.Repositories.Interfaces;
using Backend.Repositories.Implementations;
using Backend.Services.Interfaces;
using Backend.Services.Implementations;
using Microsoft.EntityFrameworkCore;

var builder = WebApplication.CreateBuilder(args);

// Agregar DbContext con SQLite
builder.Services.AddDbContext<AppDbContext>(options =>
    options.UseSqlite("Data Source=miapp.db"));

// Registrar HttpClientFactory para servicios externos
builder.Services.AddHttpClient();

// Registrar Repositorios
builder.Services.AddScoped<ICajaRepository, CajaRepository>();
builder.Services.AddScoped<IZonaRepository, ZonaRepository>();
builder.Services.AddScoped<IClienteRepository, ClienteRepository>();
builder.Services.AddScoped<ICompraRepository, CompraRepository>();
builder.Services.AddScoped<IVentasRepository, VentasRepository>();
builder.Services.AddScoped<IPrestamosRepository, PrestamosRepository>();

// Registrar Servicios
builder.Services.AddScoped<ICajaService, CajaService>();
builder.Services.AddScoped<IZonaService, ZonaService>();
builder.Services.AddScoped<IProductoService, ProductoService>();
builder.Services.AddScoped<IClienteService, ClienteService>();
builder.Services.AddScoped<IReniecService, ReniecService>();
builder.Services.AddScoped<IComprasService, ComprasService>();
builder.Services.AddScoped<IVoucherService, VoucherService>();
builder.Services.AddScoped<IVentasService, VentasService>();
builder.Services.AddScoped<IPrestamosService, PrestamosService>();
builder.Services.AddScoped<IReportesService, ReportesService>();
builder.Services.AddScoped<IDashboardService, DashboardService>();
builder.Services.AddScoped<IConfiguracionService, ConfiguracionService>();

// Configurar CORS para permitir peticiones desde Angular
builder.Services.AddCors(options =>
{
    options.AddPolicy("AllowAngular",
        policy =>
        {
            policy.WithOrigins("http://localhost:4200")
                  .AllowAnyHeader()
                  .AllowAnyMethod();
        });
});

builder.Services.AddControllers();
builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen();

var app = builder.Build();

if (app.Environment.IsDevelopment())
{
    app.UseSwagger();
    app.UseSwaggerUI();
}

app.UseCors("AllowAngular");
app.UseAuthorization();
app.MapControllers();

app.Run();