using Backend.DTOs.Requests;
using Backend.DTOs.Responses;

namespace Backend.Services.Interfaces;

public interface IReportesService
{
    Task<List<ReporteComprasClienteResponse>> GenerarReporteComprasPorClienteAsync(FiltrosReporteRequest filtros);
    Task<List<ReporteComprasProductoResponse>> GenerarReporteComprasPorProductoAsync(FiltrosReporteRequest filtros);
    Task<List<ReporteZonasResponse>> GenerarReporteResumenPorZonasAsync(FiltrosReporteRequest filtros);
    Task<List<ReporteMovimientosCajaResponse>> GenerarReporteMovimientosCajaAsync(FiltrosReporteRequest filtros);
    Task<List<ReporteVentasResponse>> GenerarReporteVentasAsync(FiltrosReporteRequest filtros);
    Task<byte[]> ExportarReporteAExcelAsync<T>(List<T> datos, string nombreHoja) where T : class;
}
