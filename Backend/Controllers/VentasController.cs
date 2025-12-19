using Backend.DTOs.Requests;
using Backend.DTOs.Responses;
using Backend.Services.Interfaces;
using Microsoft.AspNetCore.Mvc;

namespace Backend.Controllers;

[ApiController]
[Route("api/[controller]")]
public class VentasController : ControllerBase
{
    private readonly IVentasService _ventasService;
    private readonly ILogger<VentasController> _logger;

    public VentasController(
        IVentasService ventasService,
        ILogger<VentasController> logger)
    {
        _ventasService = ventasService;
        _logger = logger;
    }

    /// <summary>
    /// Registra una nueva venta
    /// </summary>
    [HttpPost]
    public async Task<ActionResult<VentaResponse>> RegistrarVenta([FromBody] RegistrarVentaRequest request)
    {
        try
        {
            var venta = await _ventasService.RegistrarVentaAsync(request);
            return CreatedAtAction(nameof(GetById), new { id = venta.Id }, venta);
        }
        catch (InvalidOperationException ex)
        {
            return BadRequest(new { message = ex.Message });
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error al registrar venta");
            return StatusCode(500, new { message = "Error interno al registrar la venta" });
        }
    }

    /// <summary>
    /// Edita una venta existente (solo del día actual)
    /// </summary>
    [HttpPut("{id}")]
    public async Task<ActionResult<VentaResponse>> EditarVenta(int id, [FromBody] EditarVentaRequest request)
    {
        try
        {
            var venta = await _ventasService.EditarVentaAsync(id, request);
            return Ok(venta);
        }
        catch (InvalidOperationException ex)
        {
            return BadRequest(new { message = ex.Message });
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error al editar venta {VentaId}", id);
            return StatusCode(500, new { message = "Error interno al editar la venta" });
        }
    }

    /// <summary>
    /// Obtiene una venta por ID
    /// </summary>
    [HttpGet("{id}")]
    public async Task<ActionResult<VentaResponse>> GetById(int id)
    {
        try
        {
            var venta = await _ventasService.GetByIdAsync(id);

            if (venta == null)
            {
                return NotFound(new { message = "Venta no encontrada" });
            }

            return Ok(venta);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error al obtener venta {VentaId}", id);
            return StatusCode(500, new { message = "Error interno al obtener la venta" });
        }
    }

    /// <summary>
    /// Obtiene todas las ventas con filtros opcionales
    /// </summary>
    [HttpGet]
    public async Task<ActionResult<object>> GetAll(
        [FromQuery] int page = 1,
        [FromQuery] int pageSize = 50,
        [FromQuery] int? clienteId = null,
        [FromQuery] int? productoId = null,
        [FromQuery] DateTime? fechaInicio = null,
        [FromQuery] DateTime? fechaFin = null)
    {
        try
        {
            var (ventas, total) = await _ventasService.GetAllAsync(
                page, pageSize, clienteId, productoId, fechaInicio, fechaFin);

            return Ok(new
            {
                data = ventas,
                total,
                page,
                pageSize,
                totalPages = (int)Math.Ceiling(total / (double)pageSize)
            });
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error al obtener ventas");
            return StatusCode(500, new { message = "Error interno al obtener las ventas" });
        }
    }

    /// <summary>
    /// Obtiene todas las ventas de una caja específica
    /// </summary>
    [HttpGet("caja/{cajaId}")]
    public async Task<ActionResult<List<VentaResponse>>> GetByCajaId(int cajaId)
    {
        try
        {
            var ventas = await _ventasService.GetByCajaIdAsync(cajaId);
            return Ok(ventas);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error al obtener ventas de la caja {CajaId}", cajaId);
            return StatusCode(500, new { message = "Error interno al obtener las ventas" });
        }
    }
}
