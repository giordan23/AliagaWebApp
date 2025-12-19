using Backend.DTOs.Requests;
using Backend.DTOs.Responses;

namespace Backend.Services.Interfaces;

public interface IClienteService
{
    // Clientes Proveedores
    Task<ClienteProveedorResponse?> GetProveedorByIdAsync(int id);
    Task<ClienteProveedorResponse?> GetProveedorByDniAsync(string dni);
    Task<List<ClienteProveedorResponse>> GetProveedoresAsync(int skip = 0, int take = 50, string? searchTerm = null, int? zonaId = null);
    Task<int> GetTotalProveedoresCountAsync(string? searchTerm = null, int? zonaId = null);
    Task<ClienteProveedorResponse> CreateProveedorAsync(CrearClienteProveedorRequest request);
    Task<ClienteProveedorResponse> UpdateProveedorAsync(int id, ActualizarClienteProveedorRequest request);

    // Clientes Compradores
    Task<ClienteCompradorResponse?> GetCompradorByIdAsync(int id);
    Task<List<ClienteCompradorResponse>> GetCompradoresAsync();
    Task<ClienteCompradorResponse> CreateCompradorAsync(CrearClienteCompradorRequest request);
    Task<ClienteCompradorResponse> UpdateCompradorAsync(int id, CrearClienteCompradorRequest request);
}
