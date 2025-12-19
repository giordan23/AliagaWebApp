using Backend.DTOs.Requests;
using Backend.DTOs.Responses;

namespace Backend.Services.Interfaces;

public interface IZonaService
{
    Task<ZonaResponse?> GetByIdAsync(int id);
    Task<List<ZonaResponse>> GetAllAsync(int skip = 0, int take = 50);
    Task<int> GetTotalCountAsync();
    Task<ZonaResponse> CreateAsync(CrearZonaRequest request);
    Task<ZonaResponse> UpdateAsync(int id, ActualizarZonaRequest request);
    Task<List<ClienteProveedorResponse>> GetClientesByZonaAsync(int zonaId);
}
