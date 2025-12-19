using Backend.Services.Interfaces;
using Microsoft.AspNetCore.Mvc;

namespace Backend.Controllers;

[ApiController]
[Route("api/[controller]")]
public class DashboardController : ControllerBase
{
    private readonly IDashboardService _dashboardService;
    private readonly ILogger<DashboardController> _logger;

    public DashboardController(
        IDashboardService dashboardService,
        ILogger<DashboardController> logger)
    {
        _dashboardService = dashboardService;
        _logger = logger;
    }

    /// <summary>
    /// Obtiene el resumen del día actual
    /// </summary>
    [HttpGet("resumen-dia")]
    public async Task<IActionResult> ObtenerResumenDelDia()
    {
        try
        {
            var resumen = await _dashboardService.ObtenerResumenDelDiaAsync();
            return Ok(resumen);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error al obtener resumen del día");
            return StatusCode(500, new { message = "Error al obtener el resumen del día" });
        }
    }

    /// <summary>
    /// Obtiene el estado de la caja actual
    /// </summary>
    [HttpGet("estado-caja")]
    public async Task<IActionResult> ObtenerEstadoCaja()
    {
        try
        {
            var estado = await _dashboardService.ObtenerEstadoCajaAsync();
            return Ok(estado);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error al obtener estado de caja");
            return StatusCode(500, new { message = "Error al obtener el estado de caja" });
        }
    }

    /// <summary>
    /// Obtiene las alertas del sistema
    /// </summary>
    [HttpGet("alertas")]
    public async Task<IActionResult> ObtenerAlertas()
    {
        try
        {
            var alertas = await _dashboardService.ObtenerAlertasAsync();
            return Ok(alertas);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error al obtener alertas");
            return StatusCode(500, new { message = "Error al obtener las alertas" });
        }
    }

    /// <summary>
    /// Obtiene estadísticas generales del negocio
    /// </summary>
    [HttpGet("estadisticas")]
    public async Task<IActionResult> ObtenerEstadisticasGenerales()
    {
        try
        {
            var estadisticas = await _dashboardService.ObtenerEstadisticasGeneralesAsync();
            return Ok(estadisticas);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error al obtener estadísticas generales");
            return StatusCode(500, new { message = "Error al obtener las estadísticas" });
        }
    }

    /// <summary>
    /// Obtiene el top de clientes con mayor deuda
    /// </summary>
    [HttpGet("top-deudores")]
    public async Task<IActionResult> ObtenerTopDeudores([FromQuery] int cantidad = 5)
    {
        try
        {
            if (cantidad < 1 || cantidad > 20)
            {
                return BadRequest(new { message = "La cantidad debe estar entre 1 y 20" });
            }

            var deudores = await _dashboardService.ObtenerTopDeudoresAsync(cantidad);
            return Ok(deudores);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error al obtener top deudores");
            return StatusCode(500, new { message = "Error al obtener los deudores" });
        }
    }
}
