using Backend.DTOs.Requests;
using Backend.DTOs.Responses;

namespace Backend.Services.Interfaces;

public interface IVentasService
{
    Task<VentaResponse> RegistrarVentaAsync(RegistrarVentaRequest request);
    Task<VentaResponse> EditarVentaAsync(int ventaId, EditarVentaRequest request);
    Task<VentaResponse?> GetByIdAsync(int id);
    Task<(List<VentaResponse> Ventas, int Total)> GetAllAsync(
        int page = 1,
        int pageSize = 50,
        int? clienteId = null,
        int? productoId = null,
        DateTime? fechaInicio = null,
        DateTime? fechaFin = null);
    Task<List<VentaResponse>> GetByCajaIdAsync(int cajaId);
}
