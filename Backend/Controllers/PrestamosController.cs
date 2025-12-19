using Backend.DTOs.Requests;
using Backend.DTOs.Responses;
using Backend.Services.Interfaces;
using Microsoft.AspNetCore.Mvc;

namespace Backend.Controllers;

[ApiController]
[Route("api/[controller]")]
public class PrestamosController : ControllerBase
{
    private readonly IPrestamosService _prestamosService;
    private readonly ILogger<PrestamosController> _logger;

    public PrestamosController(
        IPrestamosService prestamosService,
        ILogger<PrestamosController> logger)
    {
        _prestamosService = prestamosService;
        _logger = logger;
    }

    /// <summary>
    /// Registra un nuevo préstamo
    /// </summary>
    [HttpPost("prestamo")]
    public async Task<ActionResult<PrestamoResponse>> RegistrarPrestamo([FromBody] RegistrarPrestamoRequest request)
    {
        try
        {
            var prestamo = await _prestamosService.RegistrarPrestamoAsync(request);
            return CreatedAtAction(nameof(GetById), new { id = prestamo.Id }, prestamo);
        }
        catch (InvalidOperationException ex)
        {
            return BadRequest(new { message = ex.Message });
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error al registrar préstamo");
            return StatusCode(500, new { message = "Error interno al registrar el préstamo" });
        }
    }

    /// <summary>
    /// Registra un abono a préstamo
    /// </summary>
    [HttpPost("abono")]
    public async Task<ActionResult<PrestamoResponse>> RegistrarAbono([FromBody] RegistrarAbonoRequest request)
    {
        try
        {
            var abono = await _prestamosService.RegistrarAbonoAsync(request);
            return CreatedAtAction(nameof(GetById), new { id = abono.Id }, abono);
        }
        catch (InvalidOperationException ex)
        {
            return BadRequest(new { message = ex.Message });
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error al registrar abono");
            return StatusCode(500, new { message = "Error interno al registrar el abono" });
        }
    }

    /// <summary>
    /// Obtiene un préstamo o abono por ID
    /// </summary>
    [HttpGet("{id}")]
    public async Task<ActionResult<PrestamoResponse>> GetById(int id)
    {
        try
        {
            var prestamo = await _prestamosService.GetByIdAsync(id);

            if (prestamo == null)
            {
                return NotFound(new { message = "Préstamo no encontrado" });
            }

            return Ok(prestamo);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error al obtener préstamo {PrestamoId}", id);
            return StatusCode(500, new { message = "Error interno al obtener el préstamo" });
        }
    }

    /// <summary>
    /// Obtiene todos los préstamos y abonos con filtros opcionales
    /// </summary>
    [HttpGet]
    public async Task<ActionResult<object>> GetAll(
        [FromQuery] int page = 1,
        [FromQuery] int pageSize = 50,
        [FromQuery] int? clienteId = null,
        [FromQuery] DateTime? fechaInicio = null,
        [FromQuery] DateTime? fechaFin = null)
    {
        try
        {
            var (prestamos, total) = await _prestamosService.GetAllAsync(
                page, pageSize, clienteId, fechaInicio, fechaFin);

            return Ok(new
            {
                data = prestamos,
                total,
                page,
                pageSize,
                totalPages = (int)Math.Ceiling(total / (double)pageSize)
            });
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error al obtener préstamos");
            return StatusCode(500, new { message = "Error interno al obtener los préstamos" });
        }
    }

    /// <summary>
    /// Obtiene todos los préstamos y abonos de un cliente específico
    /// </summary>
    [HttpGet("cliente/{clienteId}")]
    public async Task<ActionResult<List<PrestamoResponse>>> GetByClienteId(int clienteId)
    {
        try
        {
            var prestamos = await _prestamosService.GetByClienteIdAsync(clienteId);
            return Ok(prestamos);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error al obtener préstamos del cliente {ClienteId}", clienteId);
            return StatusCode(500, new { message = "Error interno al obtener los préstamos" });
        }
    }

    /// <summary>
    /// Obtiene todos los préstamos y abonos de una caja específica
    /// </summary>
    [HttpGet("caja/{cajaId}")]
    public async Task<ActionResult<List<PrestamoResponse>>> GetByCajaId(int cajaId)
    {
        try
        {
            var prestamos = await _prestamosService.GetByCajaIdAsync(cajaId);
            return Ok(prestamos);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error al obtener préstamos de la caja {CajaId}", cajaId);
            return StatusCode(500, new { message = "Error interno al obtener los préstamos" });
        }
    }
}
