using Backend.DTOs.Requests;
using Backend.Services.Interfaces;
using Microsoft.AspNetCore.Mvc;

namespace Backend.Controllers;

[ApiController]
[Route("api/[controller]")]
public class CajaController : ControllerBase
{
    private readonly ICajaService _cajaService;
    private readonly ILogger<CajaController> _logger;

    public CajaController(ICajaService cajaService, ILogger<CajaController> logger)
    {
        _cajaService = cajaService;
        _logger = logger;
    }

    /// <summary>
    /// Abrir caja del día
    /// </summary>
    [HttpPost("abrir")]
    public async Task<IActionResult> AbrirCaja([FromBody] AbrirCajaRequest request)
    {
        try
        {
            var result = await _cajaService.AbrirCajaAsync(request);
            return Ok(result);
        }
        catch (InvalidOperationException ex)
        {
            _logger.LogWarning(ex, "Error al abrir caja");
            return BadRequest(new { message = ex.Message });
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error inesperado al abrir caja");
            return StatusCode(500, new { message = "Error al abrir la caja" });
        }
    }

    /// <summary>
    /// Cerrar caja del día
    /// </summary>
    [HttpPost("cerrar")]
    public async Task<IActionResult> CerrarCaja([FromBody] CerrarCajaRequest request)
    {
        try
        {
            var result = await _cajaService.CerrarCajaAsync(request);
            return Ok(result);
        }
        catch (InvalidOperationException ex)
        {
            _logger.LogWarning(ex, "Error al cerrar caja");
            return BadRequest(new { message = ex.Message });
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error inesperado al cerrar caja");
            return StatusCode(500, new { message = "Error al cerrar la caja" });
        }
    }

    /// <summary>
    /// Reabrir caja del día actual
    /// </summary>
    [HttpPost("{id}/reabrir")]
    public async Task<IActionResult> ReabrirCaja(int id)
    {
        try
        {
            var result = await _cajaService.ReabrirCajaAsync(id);
            return Ok(result);
        }
        catch (InvalidOperationException ex)
        {
            _logger.LogWarning(ex, "Error al reabrir caja");
            return BadRequest(new { message = ex.Message });
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error inesperado al reabrir caja");
            return StatusCode(500, new { message = "Error al reabrir la caja" });
        }
    }

    /// <summary>
    /// Obtener caja actual (abierta)
    /// </summary>
    [HttpGet("actual")]
    public async Task<IActionResult> GetCajaActual()
    {
        try
        {
            var result = await _cajaService.GetCajaActualAsync();
            if (result == null)
                return NotFound(new { message = "No hay caja abierta" });

            return Ok(result);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error al obtener caja actual");
            return StatusCode(500, new { message = "Error al obtener la caja actual" });
        }
    }

    /// <summary>
    /// Verificar si existe caja abierta
    /// </summary>
    [HttpGet("existe-abierta")]
    public async Task<IActionResult> ExisteCajaAbierta()
    {
        try
        {
            var existe = await _cajaService.ExisteCajaAbiertaAsync();
            return Ok(new { existe });
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error al verificar caja abierta");
            return StatusCode(500, new { message = "Error al verificar caja abierta" });
        }
    }

    /// <summary>
    /// Obtener detalle de una caja específica
    /// </summary>
    [HttpGet("{id}")]
    public async Task<IActionResult> GetCajaDetalle(int id)
    {
        try
        {
            var result = await _cajaService.GetCajaDetalleAsync(id);
            if (result == null)
                return NotFound(new { message = "Caja no encontrada" });

            return Ok(result);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error al obtener detalle de caja {CajaId}", id);
            return StatusCode(500, new { message = "Error al obtener el detalle de la caja" });
        }
    }

    /// <summary>
    /// Obtener historial de cajas con paginación
    /// </summary>
    [HttpGet("historial")]
    public async Task<IActionResult> GetHistorialCajas([FromQuery] int skip = 0, [FromQuery] int take = 50)
    {
        try
        {
            var cajas = await _cajaService.GetHistorialCajasAsync(skip, take);
            var total = await _cajaService.GetTotalCajasCountAsync();

            return Ok(new
            {
                items = cajas,
                total,
                skip,
                take
            });
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error al obtener historial de cajas");
            return StatusCode(500, new { message = "Error al obtener el historial de cajas" });
        }
    }

    /// <summary>
    /// Registrar movimiento directo de caja (inyección, retiro, gasto)
    /// </summary>
    [HttpPost("movimiento")]
    public async Task<IActionResult> RegistrarMovimiento([FromBody] RegistrarMovimientoCajaRequest request)
    {
        try
        {
            var result = await _cajaService.RegistrarMovimientoAsync(request);
            return Ok(result);
        }
        catch (InvalidOperationException ex)
        {
            _logger.LogWarning(ex, "Error al registrar movimiento de caja");
            return BadRequest(new { message = ex.Message });
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error inesperado al registrar movimiento de caja");
            return StatusCode(500, new { message = "Error al registrar el movimiento" });
        }
    }
}
