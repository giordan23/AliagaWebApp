using Backend.DTOs.Requests;
using Backend.DTOs.Responses;

namespace Backend.Services.Interfaces;

public interface IProductoService
{
    Task<List<ProductoResponse>> GetAllAsync();
    Task<ProductoResponse?> GetByIdAsync(int id);
    Task<ProductoResponse> UpdatePrecioAsync(int id, ActualizarPrecioProductoRequest request);
}
