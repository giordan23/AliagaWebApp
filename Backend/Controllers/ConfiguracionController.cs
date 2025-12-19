using Backend.DTOs.Requests;
using Backend.Services.Interfaces;
using Microsoft.AspNetCore.Mvc;

namespace Backend.Controllers;

[ApiController]
[Route("api/[controller]")]
public class ConfiguracionController : ControllerBase
{
    private readonly IConfiguracionService _configuracionService;
    private readonly IComprasService _comprasService;
    private readonly IVentasService _ventasService;
    private readonly IPrestamosService _prestamosService;
    private readonly ILogger<ConfiguracionController> _logger;

    public ConfiguracionController(
        IConfiguracionService configuracionService,
        IComprasService comprasService,
        IVentasService ventasService,
        IPrestamosService prestamosService,
        ILogger<ConfiguracionController> logger)
    {
        _configuracionService = configuracionService;
        _comprasService = comprasService;
        _ventasService = ventasService;
        _prestamosService = prestamosService;
        _logger = logger;
    }

    /// <summary>
    /// Genera y descarga un backup de la base de datos
    /// </summary>
    [HttpGet("backup")]
    public async Task<IActionResult> DescargarBackup()
    {
        try
        {
            var backupData = await _configuracionService.GenerarBackupAsync();
            var nombreArchivo = _configuracionService.ObtenerNombreArchivoBackup();

            return File(backupData, "application/x-sqlite3", nombreArchivo);
        }
        catch (FileNotFoundException ex)
        {
            _logger.LogError(ex, "Archivo de base de datos no encontrado");
            return NotFound(new { message = "No se encontró el archivo de base de datos" });
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error al generar backup");
            return StatusCode(500, new { message = "Error al generar el backup" });
        }
    }

    /// <summary>
    /// Registra una compra en una caja histórica (ajuste posterior)
    /// </summary>
    /// <remarks>
    /// NOTA: Esta funcionalidad permite registrar compras en días anteriores.
    /// Se recomienda usar solo en casos excepcionales y con la debida autorización.
    /// Los ajustes posteriores quedan marcados con la bandera EsAjustePosterior = true.
    /// </remarks>
    [HttpPost("ajuste-posterior/compra")]
    public async Task<IActionResult> RegistrarCompraAjustePosterior([FromBody] RegistrarCompraRequest request)
    {
        try
        {
            // NOTA: En una implementación completa, aquí se validaría:
            // 1. Que la caja histórica exista y esté cerrada
            // 2. Que el usuario tenga permisos para ajustes posteriores
            // 3. Se marcaría la compra con EsAjustePosterior = true
            // 4. Se recalcularía la diferencia de la caja histórica

            // Por ahora, retornamos un mensaje indicando que esta funcionalidad
            // requiere implementación adicional con validaciones de seguridad

            return BadRequest(new
            {
                message = "Los ajustes posteriores requieren validaciones adicionales de seguridad",
                detalle = "Esta funcionalidad debe implementarse con controles de auditoría apropiados"
            });
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error al registrar compra como ajuste posterior");
            return StatusCode(500, new { message = "Error al registrar el ajuste posterior" });
        }
    }

    /// <summary>
    /// Registra una venta en una caja histórica (ajuste posterior)
    /// </summary>
    [HttpPost("ajuste-posterior/venta")]
    public async Task<IActionResult> RegistrarVentaAjustePosterior([FromBody] RegistrarVentaRequest request)
    {
        try
        {
            return BadRequest(new
            {
                message = "Los ajustes posteriores requieren validaciones adicionales de seguridad",
                detalle = "Esta funcionalidad debe implementarse con controles de auditoría apropiados"
            });
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error al registrar venta como ajuste posterior");
            return StatusCode(500, new { message = "Error al registrar el ajuste posterior" });
        }
    }

    /// <summary>
    /// Registra un préstamo en una caja histórica (ajuste posterior)
    /// </summary>
    [HttpPost("ajuste-posterior/prestamo")]
    public async Task<IActionResult> RegistrarPrestamoAjustePosterior([FromBody] RegistrarPrestamoRequest request)
    {
        try
        {
            return BadRequest(new
            {
                message = "Los ajustes posteriores requieren validaciones adicionales de seguridad",
                detalle = "Esta funcionalidad debe implementarse con controles de auditoría apropiados"
            });
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error al registrar préstamo como ajuste posterior");
            return StatusCode(500, new { message = "Error al registrar el ajuste posterior" });
        }
    }
}
