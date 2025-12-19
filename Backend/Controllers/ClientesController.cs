using Backend.DTOs.Requests;
using Backend.Services.Interfaces;
using Microsoft.AspNetCore.Mvc;

namespace Backend.Controllers;

[ApiController]
[Route("api/[controller]")]
public class ClientesController : ControllerBase
{
    private readonly IClienteService _clienteService;
    private readonly IReniecService _reniecService;
    private readonly ILogger<ClientesController> _logger;

    public ClientesController(
        IClienteService clienteService,
        IReniecService reniecService,
        ILogger<ClientesController> logger)
    {
        _clienteService = clienteService;
        _reniecService = reniecService;
        _logger = logger;
    }

    // ==================== CLIENTES PROVEEDORES ====================

    /// <summary>
    /// Obtener todos los proveedores con filtros y paginación
    /// </summary>
    [HttpGet("proveedores")]
    public async Task<IActionResult> GetProveedores(
        [FromQuery] int skip = 0,
        [FromQuery] int take = 50,
        [FromQuery] string? search = null,
        [FromQuery] int? zonaId = null)
    {
        try
        {
            var clientes = await _clienteService.GetProveedoresAsync(skip, take, search, zonaId);
            var total = await _clienteService.GetTotalProveedoresCountAsync(search, zonaId);

            return Ok(new
            {
                items = clientes,
                total,
                skip,
                take
            });
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error al obtener proveedores");
            return StatusCode(500, new { message = "Error al obtener los clientes proveedores" });
        }
    }

    /// <summary>
    /// Obtener proveedor por ID
    /// </summary>
    [HttpGet("proveedores/{id}")]
    public async Task<IActionResult> GetProveedorById(int id)
    {
        try
        {
            var cliente = await _clienteService.GetProveedorByIdAsync(id);
            if (cliente == null)
                return NotFound(new { message = "Cliente no encontrado" });

            return Ok(cliente);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error al obtener proveedor {ClienteId}", id);
            return StatusCode(500, new { message = "Error al obtener el cliente" });
        }
    }

    /// <summary>
    /// Buscar proveedor por DNI
    /// </summary>
    [HttpGet("proveedores/dni/{dni}")]
    public async Task<IActionResult> GetProveedorByDni(string dni)
    {
        try
        {
            var cliente = await _clienteService.GetProveedorByDniAsync(dni);
            if (cliente == null)
                return NotFound(new { message = "Cliente no encontrado" });

            return Ok(cliente);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error al buscar proveedor por DNI {DNI}", dni);
            return StatusCode(500, new { message = "Error al buscar el cliente" });
        }
    }

    /// <summary>
    /// Consultar DNI en RENIEC
    /// </summary>
    [HttpGet("reniec/{dni}")]
    public async Task<IActionResult> ConsultarReniec(string dni)
    {
        try
        {
            var resultado = await _reniecService.ConsultarDniAsync(dni);
            return Ok(resultado);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error al consultar RENIEC para DNI {DNI}", dni);
            return StatusCode(500, new { message = "Error al consultar RENIEC" });
        }
    }

    /// <summary>
    /// Crear nuevo cliente proveedor
    /// </summary>
    [HttpPost("proveedores")]
    public async Task<IActionResult> CreateProveedor([FromBody] CrearClienteProveedorRequest request)
    {
        try
        {
            var cliente = await _clienteService.CreateProveedorAsync(request);
            return CreatedAtAction(nameof(GetProveedorById), new { id = cliente.Id }, cliente);
        }
        catch (InvalidOperationException ex)
        {
            _logger.LogWarning(ex, "Error al crear proveedor");
            return BadRequest(new { message = ex.Message });
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error inesperado al crear proveedor");
            return StatusCode(500, new { message = "Error al crear el cliente" });
        }
    }

    /// <summary>
    /// Actualizar cliente proveedor
    /// </summary>
    [HttpPut("proveedores/{id}")]
    public async Task<IActionResult> UpdateProveedor(int id, [FromBody] ActualizarClienteProveedorRequest request)
    {
        try
        {
            var cliente = await _clienteService.UpdateProveedorAsync(id, request);
            return Ok(cliente);
        }
        catch (InvalidOperationException ex)
        {
            _logger.LogWarning(ex, "Error al actualizar proveedor {ClienteId}", id);
            return BadRequest(new { message = ex.Message });
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error inesperado al actualizar proveedor {ClienteId}", id);
            return StatusCode(500, new { message = "Error al actualizar el cliente" });
        }
    }

    // ==================== CLIENTES COMPRADORES ====================

    /// <summary>
    /// Obtener todos los compradores
    /// </summary>
    [HttpGet("compradores")]
    public async Task<IActionResult> GetCompradores()
    {
        try
        {
            var clientes = await _clienteService.GetCompradoresAsync();
            return Ok(clientes);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error al obtener compradores");
            return StatusCode(500, new { message = "Error al obtener los clientes compradores" });
        }
    }

    /// <summary>
    /// Obtener comprador por ID
    /// </summary>
    [HttpGet("compradores/{id}")]
    public async Task<IActionResult> GetCompradorById(int id)
    {
        try
        {
            var cliente = await _clienteService.GetCompradorByIdAsync(id);
            if (cliente == null)
                return NotFound(new { message = "Cliente comprador no encontrado" });

            return Ok(cliente);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error al obtener comprador {ClienteId}", id);
            return StatusCode(500, new { message = "Error al obtener el cliente comprador" });
        }
    }

    /// <summary>
    /// Crear nuevo cliente comprador
    /// </summary>
    [HttpPost("compradores")]
    public async Task<IActionResult> CreateComprador([FromBody] CrearClienteCompradorRequest request)
    {
        try
        {
            var cliente = await _clienteService.CreateCompradorAsync(request);
            return CreatedAtAction(nameof(GetCompradorById), new { id = cliente.Id }, cliente);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error inesperado al crear comprador");
            return StatusCode(500, new { message = "Error al crear el cliente comprador" });
        }
    }

    /// <summary>
    /// Actualizar cliente comprador
    /// </summary>
    [HttpPut("compradores/{id}")]
    public async Task<IActionResult> UpdateComprador(int id, [FromBody] CrearClienteCompradorRequest request)
    {
        try
        {
            var cliente = await _clienteService.UpdateCompradorAsync(id, request);
            return Ok(cliente);
        }
        catch (InvalidOperationException ex)
        {
            _logger.LogWarning(ex, "Error al actualizar comprador {ClienteId}", id);
            return BadRequest(new { message = ex.Message });
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error inesperado al actualizar comprador {ClienteId}", id);
            return StatusCode(500, new { message = "Error al actualizar el cliente comprador" });
        }
    }
}
