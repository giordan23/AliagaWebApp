using Backend.DTOs.Requests;
using Backend.DTOs.Responses;

namespace Backend.Services.Interfaces;

public interface ICajaService
{
    Task<CajaResumenResponse?> AbrirCajaAsync(AbrirCajaRequest request);
    Task<CajaResumenResponse?> CerrarCajaAsync(CerrarCajaRequest request);
    Task<CajaResumenResponse?> ReabrirCajaAsync(int cajaId);
    Task<CajaResumenResponse?> GetCajaActualAsync();
    Task<CajaDetalleResponse?> GetCajaDetalleAsync(int cajaId);
    Task<List<CajaResumenResponse>> GetHistorialCajasAsync(int skip = 0, int take = 50);
    Task<int> GetTotalCajasCountAsync();
    Task<MovimientoCajaResponse?> RegistrarMovimientoAsync(RegistrarMovimientoCajaRequest request);
    Task<bool> ExisteCajaAbiertaAsync();
}
