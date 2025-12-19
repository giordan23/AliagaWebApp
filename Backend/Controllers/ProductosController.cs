using Backend.DTOs.Requests;
using Backend.Services.Interfaces;
using Microsoft.AspNetCore.Mvc;

namespace Backend.Controllers;

[ApiController]
[Route("api/[controller]")]
public class ProductosController : ControllerBase
{
    private readonly IProductoService _productoService;
    private readonly ILogger<ProductosController> _logger;

    public ProductosController(IProductoService productoService, ILogger<ProductosController> logger)
    {
        _productoService = productoService;
        _logger = logger;
    }

    /// <summary>
    /// Obtener todos los productos (Café, Cacao, Maíz, Achiote)
    /// </summary>
    [HttpGet]
    public async Task<IActionResult> GetAll()
    {
        try
        {
            var productos = await _productoService.GetAllAsync();
            return Ok(productos);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error al obtener productos");
            return StatusCode(500, new { message = "Error al obtener los productos" });
        }
    }

    /// <summary>
    /// Obtener producto por ID
    /// </summary>
    [HttpGet("{id}")]
    public async Task<IActionResult> GetById(int id)
    {
        try
        {
            var producto = await _productoService.GetByIdAsync(id);
            if (producto == null)
                return NotFound(new { message = "Producto no encontrado" });

            return Ok(producto);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error al obtener producto {ProductoId}", id);
            return StatusCode(500, new { message = "Error al obtener el producto" });
        }
    }

    /// <summary>
    /// Actualizar precio sugerido del producto
    /// </summary>
    [HttpPut("{id}/precio")]
    public async Task<IActionResult> UpdatePrecio(int id, [FromBody] ActualizarPrecioProductoRequest request)
    {
        try
        {
            var producto = await _productoService.UpdatePrecioAsync(id, request);
            return Ok(producto);
        }
        catch (InvalidOperationException ex)
        {
            _logger.LogWarning(ex, "Error al actualizar precio del producto {ProductoId}", id);
            return BadRequest(new { message = ex.Message });
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error inesperado al actualizar precio del producto {ProductoId}", id);
            return StatusCode(500, new { message = "Error al actualizar el precio del producto" });
        }
    }
}
