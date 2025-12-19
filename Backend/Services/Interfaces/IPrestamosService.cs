using Backend.DTOs.Requests;
using Backend.DTOs.Responses;

namespace Backend.Services.Interfaces;

public interface IPrestamosService
{
    Task<PrestamoResponse> RegistrarPrestamoAsync(RegistrarPrestamoRequest request);
    Task<PrestamoResponse> RegistrarAbonoAsync(RegistrarAbonoRequest request);
    Task<PrestamoResponse?> GetByIdAsync(int id);
    Task<(List<PrestamoResponse> Prestamos, int Total)> GetAllAsync(
        int page = 1,
        int pageSize = 50,
        int? clienteId = null,
        DateTime? fechaInicio = null,
        DateTime? fechaFin = null);
    Task<List<PrestamoResponse>> GetByClienteIdAsync(int clienteId);
    Task<List<PrestamoResponse>> GetByCajaIdAsync(int cajaId);
}
