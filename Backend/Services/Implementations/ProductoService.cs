using Backend.Data;
using Backend.DTOs.Requests;
using Backend.DTOs.Responses;
using Backend.Services.Interfaces;
using Microsoft.EntityFrameworkCore;
using System.Text.Json;

namespace Backend.Services.Implementations;

public class ProductoService : IProductoService
{
    private readonly AppDbContext _context;

    public ProductoService(AppDbContext context)
    {
        _context = context;
    }

    public async Task<List<ProductoResponse>> GetAllAsync()
    {
        var productos = await _context.Productos.ToListAsync();
        return productos.Select(p => MapToResponse(p)).ToList();
    }

    public async Task<ProductoResponse?> GetByIdAsync(int id)
    {
        var producto = await _context.Productos.FindAsync(id);
        if (producto == null)
            return null;

        return MapToResponse(producto);
    }

    public async Task<ProductoResponse> UpdatePrecioAsync(int id, ActualizarPrecioProductoRequest request)
    {
        var producto = await _context.Productos.FindAsync(id);
        if (producto == null)
        {
            throw new InvalidOperationException("Producto no encontrado");
        }

        producto.PrecioSugeridoPorKg = request.PrecioSugeridoPorKg;
        producto.FechaModificacion = DateTime.Now;

        await _context.SaveChangesAsync();
        return MapToResponse(producto);
    }

    private ProductoResponse MapToResponse(Backend.Models.Producto producto)
    {
        return new ProductoResponse
        {
            Id = producto.Id,
            Nombre = producto.Nombre,
            PrecioSugeridoPorKg = producto.PrecioSugeridoPorKg,
            NivelesSecado = JsonSerializer.Deserialize<List<string>>(producto.NivelesSecado) ?? new List<string>(),
            Calidades = JsonSerializer.Deserialize<List<string>>(producto.Calidades) ?? new List<string>(),
            PermiteValdeo = producto.PermiteValdeo,
            FechaModificacion = producto.FechaModificacion
        };
    }
}
