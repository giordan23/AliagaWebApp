using Backend.DTOs.Requests;
using Backend.Services.Interfaces;
using Microsoft.AspNetCore.Mvc;

namespace Backend.Controllers;

[ApiController]
[Route("api/[controller]")]
public class ReportesController : ControllerBase
{
    private readonly IReportesService _reportesService;
    private readonly ILogger<ReportesController> _logger;

    public ReportesController(
        IReportesService reportesService,
        ILogger<ReportesController> logger)
    {
        _reportesService = reportesService;
        _logger = logger;
    }

    /// <summary>
    /// Genera reporte de compras agrupado por cliente
    /// </summary>
    [HttpPost("compras-cliente")]
    public async Task<IActionResult> GenerarReporteComprasPorCliente([FromBody] FiltrosReporteRequest filtros)
    {
        try
        {
            var reporte = await _reportesService.GenerarReporteComprasPorClienteAsync(filtros);
            return Ok(reporte);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error al generar reporte de compras por cliente");
            return StatusCode(500, new { message = "Error al generar el reporte" });
        }
    }

    /// <summary>
    /// Exporta reporte de compras por cliente a Excel
    /// </summary>
    [HttpPost("compras-cliente/exportar")]
    public async Task<IActionResult> ExportarReporteComprasPorCliente([FromBody] FiltrosReporteRequest filtros)
    {
        try
        {
            var reporte = await _reportesService.GenerarReporteComprasPorClienteAsync(filtros);
            var excel = await _reportesService.ExportarReporteAExcelAsync(reporte, "Compras por Cliente");

            var nombreArchivo = $"reporte_compras_cliente_{DateTime.Now:yyyyMMdd_HHmmss}.xlsx";
            return File(excel, "application/vnd.openxmlformats-officedocument.spreadsheetml.sheet", nombreArchivo);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error al exportar reporte de compras por cliente");
            return StatusCode(500, new { message = "Error al exportar el reporte" });
        }
    }

    /// <summary>
    /// Genera reporte de compras agrupado por producto
    /// </summary>
    [HttpPost("compras-producto")]
    public async Task<IActionResult> GenerarReporteComprasPorProducto([FromBody] FiltrosReporteRequest filtros)
    {
        try
        {
            var reporte = await _reportesService.GenerarReporteComprasPorProductoAsync(filtros);
            return Ok(reporte);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error al generar reporte de compras por producto");
            return StatusCode(500, new { message = "Error al generar el reporte" });
        }
    }

    /// <summary>
    /// Exporta reporte de compras por producto a Excel
    /// </summary>
    [HttpPost("compras-producto/exportar")]
    public async Task<IActionResult> ExportarReporteComprasPorProducto([FromBody] FiltrosReporteRequest filtros)
    {
        try
        {
            var reporte = await _reportesService.GenerarReporteComprasPorProductoAsync(filtros);
            var excel = await _reportesService.ExportarReporteAExcelAsync(reporte, "Compras por Producto");

            var nombreArchivo = $"reporte_compras_producto_{DateTime.Now:yyyyMMdd_HHmmss}.xlsx";
            return File(excel, "application/vnd.openxmlformats-officedocument.spreadsheetml.sheet", nombreArchivo);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error al exportar reporte de compras por producto");
            return StatusCode(500, new { message = "Error al exportar el reporte" });
        }
    }

    /// <summary>
    /// Genera reporte de resumen por zonas
    /// </summary>
    [HttpPost("zonas")]
    public async Task<IActionResult> GenerarReporteResumenPorZonas([FromBody] FiltrosReporteRequest filtros)
    {
        try
        {
            var reporte = await _reportesService.GenerarReporteResumenPorZonasAsync(filtros);
            return Ok(reporte);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error al generar reporte de resumen por zonas");
            return StatusCode(500, new { message = "Error al generar el reporte" });
        }
    }

    /// <summary>
    /// Exporta reporte de resumen por zonas a Excel
    /// </summary>
    [HttpPost("zonas/exportar")]
    public async Task<IActionResult> ExportarReporteResumenPorZonas([FromBody] FiltrosReporteRequest filtros)
    {
        try
        {
            var reporte = await _reportesService.GenerarReporteResumenPorZonasAsync(filtros);
            var excel = await _reportesService.ExportarReporteAExcelAsync(reporte, "Resumen por Zonas");

            var nombreArchivo = $"reporte_zonas_{DateTime.Now:yyyyMMdd_HHmmss}.xlsx";
            return File(excel, "application/vnd.openxmlformats-officedocument.spreadsheetml.sheet", nombreArchivo);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error al exportar reporte de resumen por zonas");
            return StatusCode(500, new { message = "Error al exportar el reporte" });
        }
    }

    /// <summary>
    /// Genera reporte de movimientos de caja
    /// </summary>
    [HttpPost("movimientos-caja")]
    public async Task<IActionResult> GenerarReporteMovimientosCaja([FromBody] FiltrosReporteRequest filtros)
    {
        try
        {
            var reporte = await _reportesService.GenerarReporteMovimientosCajaAsync(filtros);
            return Ok(reporte);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error al generar reporte de movimientos de caja");
            return StatusCode(500, new { message = "Error al generar el reporte" });
        }
    }

    /// <summary>
    /// Exporta reporte de movimientos de caja a Excel
    /// </summary>
    [HttpPost("movimientos-caja/exportar")]
    public async Task<IActionResult> ExportarReporteMovimientosCaja([FromBody] FiltrosReporteRequest filtros)
    {
        try
        {
            var reporte = await _reportesService.GenerarReporteMovimientosCajaAsync(filtros);
            var excel = await _reportesService.ExportarReporteAExcelAsync(reporte, "Movimientos de Caja");

            var nombreArchivo = $"reporte_movimientos_caja_{DateTime.Now:yyyyMMdd_HHmmss}.xlsx";
            return File(excel, "application/vnd.openxmlformats-officedocument.spreadsheetml.sheet", nombreArchivo);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error al exportar reporte de movimientos de caja");
            return StatusCode(500, new { message = "Error al exportar el reporte" });
        }
    }

    /// <summary>
    /// Genera reporte de ventas
    /// </summary>
    [HttpPost("ventas")]
    public async Task<IActionResult> GenerarReporteVentas([FromBody] FiltrosReporteRequest filtros)
    {
        try
        {
            var reporte = await _reportesService.GenerarReporteVentasAsync(filtros);
            return Ok(reporte);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error al generar reporte de ventas");
            return StatusCode(500, new { message = "Error al generar el reporte" });
        }
    }

    /// <summary>
    /// Exporta reporte de ventas a Excel
    /// </summary>
    [HttpPost("ventas/exportar")]
    public async Task<IActionResult> ExportarReporteVentas([FromBody] FiltrosReporteRequest filtros)
    {
        try
        {
            var reporte = await _reportesService.GenerarReporteVentasAsync(filtros);
            var excel = await _reportesService.ExportarReporteAExcelAsync(reporte, "Ventas");

            var nombreArchivo = $"reporte_ventas_{DateTime.Now:yyyyMMdd_HHmmss}.xlsx";
            return File(excel, "application/vnd.openxmlformats-officedocument.spreadsheetml.sheet", nombreArchivo);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error al exportar reporte de ventas");
            return StatusCode(500, new { message = "Error al exportar el reporte" });
        }
    }
}
