using Backend.DTOs.Responses;

namespace Backend.Services.Interfaces;

public interface IDashboardService
{
    Task<ResumenDiaResponse> ObtenerResumenDelDiaAsync();
    Task<EstadoCajaResponse> ObtenerEstadoCajaAsync();
    Task<List<AlertaResponse>> ObtenerAlertasAsync();
    Task<EstadisticasGeneralesResponse> ObtenerEstadisticasGeneralesAsync();
    Task<List<DeudorResponse>> ObtenerTopDeudoresAsync(int cantidad = 5);
}
