using Backend.DTOs.Requests;
using Backend.Services.Interfaces;
using Microsoft.AspNetCore.Mvc;

namespace Backend.Controllers;

[ApiController]
[Route("api/[controller]")]
public class ZonasController : ControllerBase
{
    private readonly IZonaService _zonaService;
    private readonly ILogger<ZonasController> _logger;

    public ZonasController(IZonaService zonaService, ILogger<ZonasController> logger)
    {
        _zonaService = zonaService;
        _logger = logger;
    }

    /// <summary>
    /// Obtener todas las zonas con paginación
    /// </summary>
    [HttpGet]
    public async Task<IActionResult> GetAll([FromQuery] int skip = 0, [FromQuery] int take = 50)
    {
        try
        {
            var zonas = await _zonaService.GetAllAsync(skip, take);
            var total = await _zonaService.GetTotalCountAsync();

            return Ok(new
            {
                items = zonas,
                total,
                skip,
                take
            });
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error al obtener zonas");
            return StatusCode(500, new { message = "Error al obtener las zonas" });
        }
    }

    /// <summary>
    /// Obtener zona por ID
    /// </summary>
    [HttpGet("{id}")]
    public async Task<IActionResult> GetById(int id)
    {
        try
        {
            var zona = await _zonaService.GetByIdAsync(id);
            if (zona == null)
                return NotFound(new { message = "Zona no encontrada" });

            return Ok(zona);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error al obtener zona {ZonaId}", id);
            return StatusCode(500, new { message = "Error al obtener la zona" });
        }
    }

    /// <summary>
    /// Crear nueva zona
    /// </summary>
    [HttpPost]
    public async Task<IActionResult> Create([FromBody] CrearZonaRequest request)
    {
        try
        {
            var zona = await _zonaService.CreateAsync(request);
            return CreatedAtAction(nameof(GetById), new { id = zona.Id }, zona);
        }
        catch (InvalidOperationException ex)
        {
            _logger.LogWarning(ex, "Error al crear zona");
            return BadRequest(new { message = ex.Message });
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error inesperado al crear zona");
            return StatusCode(500, new { message = "Error al crear la zona" });
        }
    }

    /// <summary>
    /// Actualizar zona existente
    /// </summary>
    [HttpPut("{id}")]
    public async Task<IActionResult> Update(int id, [FromBody] ActualizarZonaRequest request)
    {
        try
        {
            var zona = await _zonaService.UpdateAsync(id, request);
            return Ok(zona);
        }
        catch (InvalidOperationException ex)
        {
            _logger.LogWarning(ex, "Error al actualizar zona {ZonaId}", id);
            return BadRequest(new { message = ex.Message });
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error inesperado al actualizar zona {ZonaId}", id);
            return StatusCode(500, new { message = "Error al actualizar la zona" });
        }
    }

    /// <summary>
    /// Obtener clientes de una zona específica
    /// </summary>
    [HttpGet("{id}/clientes")]
    public async Task<IActionResult> GetClientesByZona(int id)
    {
        try
        {
            var clientes = await _zonaService.GetClientesByZonaAsync(id);
            return Ok(clientes);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error al obtener clientes de zona {ZonaId}", id);
            return StatusCode(500, new { message = "Error al obtener los clientes de la zona" });
        }
    }
}
