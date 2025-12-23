using Backend.Models;

namespace Backend.Repositories.Interfaces;

public interface IClienteRepository
{
    // Clientes Proveedores
    Task<ClienteProveedor?> GetProveedorByIdAsync(int id);
    Task<ClienteProveedor?> GetProveedorByDniAsync(string dni);
    Task<List<ClienteProveedor>> GetProveedoresAsync(int skip = 0, int take = 50, string? searchTerm = null, int? zonaId = null);
    Task<int> GetTotalProveedoresCountAsync(string? searchTerm = null, int? zonaId = null);
    Task<ClienteProveedor> AddProveedorAsync(ClienteProveedor cliente);
    Task UpdateProveedorAsync(ClienteProveedor cliente);
    Task DeleteProveedorAsync(int id);
    Task<bool> ExisteDniAsync(string dni, int? excludeId = null);
    Task<Dictionary<int, decimal>> GetTotalKgVendidosPorProveedorAsync();

    // Clientes Compradores
    Task<ClienteComprador?> GetCompradorByIdAsync(int id);
    Task<List<ClienteComprador>> GetCompradoresAsync();
    Task<ClienteComprador> AddCompradorAsync(ClienteComprador cliente);
    Task UpdateCompradorAsync(ClienteComprador cliente);
    Task DeleteCompradorAsync(int id);
}
