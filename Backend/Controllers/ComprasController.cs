using Backend.DTOs.Requests;
using Backend.DTOs.Responses;
using Backend.Services.Interfaces;
using Microsoft.AspNetCore.Mvc;

namespace Backend.Controllers;

[ApiController]
[Route("api/[controller]")]
public class ComprasController : ControllerBase
{
    private readonly IComprasService _comprasService;
    private readonly IVoucherService _voucherService;
    private readonly ILogger<ComprasController> _logger;

    public ComprasController(
        IComprasService comprasService,
        IVoucherService voucherService,
        ILogger<ComprasController> logger)
    {
        _comprasService = comprasService;
        _voucherService = voucherService;
        _logger = logger;
    }

    /// <summary>
    /// Registra una nueva compra
    /// </summary>
    [HttpPost]
    public async Task<ActionResult<CompraResponse>> RegistrarCompra([FromBody] RegistrarCompraRequest request)
    {
        try
        {
            var compra = await _comprasService.RegistrarCompraAsync(request);
            return CreatedAtAction(nameof(GetById), new { id = compra.Id }, compra);
        }
        catch (InvalidOperationException ex)
        {
            return BadRequest(new { message = ex.Message });
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error al registrar compra");
            return StatusCode(500, new { message = "Error interno al registrar la compra" });
        }
    }

    /// <summary>
    /// Edita una compra existente (solo del día actual)
    /// </summary>
    [HttpPut("{id}")]
    public async Task<ActionResult<CompraResponse>> EditarCompra(int id, [FromBody] EditarCompraRequest request)
    {
        try
        {
            var compra = await _comprasService.EditarCompraAsync(id, request);
            return Ok(compra);
        }
        catch (InvalidOperationException ex)
        {
            return BadRequest(new { message = ex.Message });
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error al editar compra {CompraId}", id);
            return StatusCode(500, new { message = "Error interno al editar la compra" });
        }
    }

    /// <summary>
    /// Obtiene una compra por ID
    /// </summary>
    [HttpGet("{id}")]
    public async Task<ActionResult<CompraResponse>> GetById(int id)
    {
        try
        {
            var compra = await _comprasService.GetByIdAsync(id);

            if (compra == null)
            {
                return NotFound(new { message = "Compra no encontrada" });
            }

            return Ok(compra);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error al obtener compra {CompraId}", id);
            return StatusCode(500, new { message = "Error interno al obtener la compra" });
        }
    }

    /// <summary>
    /// Obtiene una compra por número de voucher
    /// </summary>
    [HttpGet("voucher/{numeroVoucher}")]
    public async Task<ActionResult<CompraResponse>> GetByNumeroVoucher(string numeroVoucher)
    {
        try
        {
            var compra = await _comprasService.GetByNumeroVoucherAsync(numeroVoucher);

            if (compra == null)
            {
                return NotFound(new { message = "Compra no encontrada" });
            }

            return Ok(compra);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error al obtener compra por voucher {NumeroVoucher}", numeroVoucher);
            return StatusCode(500, new { message = "Error interno al obtener la compra" });
        }
    }

    /// <summary>
    /// Obtiene todas las compras con filtros opcionales
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
            var (compras, total) = await _comprasService.GetAllAsync(
                page, pageSize, clienteId, productoId, fechaInicio, fechaFin);

            return Ok(new
            {
                data = compras,
                total,
                page,
                pageSize,
                totalPages = (int)Math.Ceiling(total / (double)pageSize)
            });
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error al obtener compras");
            return StatusCode(500, new { message = "Error interno al obtener las compras" });
        }
    }

    /// <summary>
    /// Obtiene todas las compras de una caja específica
    /// </summary>
    [HttpGet("caja/{cajaId}")]
    public async Task<ActionResult<List<CompraResponse>>> GetByCajaId(int cajaId)
    {
        try
        {
            var compras = await _comprasService.GetByCajaIdAsync(cajaId);
            return Ok(compras);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error al obtener compras de la caja {CajaId}", cajaId);
            return StatusCode(500, new { message = "Error interno al obtener las compras" });
        }
    }

    /// <summary>
    /// Reimprime el voucher de una compra (marca como DUPLICADO)
    /// </summary>
    [HttpPost("{id}/reimprimir")]
    public async Task<ActionResult<VoucherResponse>> ReimprimirVoucher(int id)
    {
        try
        {
            var voucherResponse = await _voucherService.ReimprimirVoucherAsync(id);

            if (!voucherResponse.ImpresionExitosa && !string.IsNullOrEmpty(voucherResponse.MensajeError))
            {
                return Ok(new
                {
                    success = false,
                    message = voucherResponse.MensajeError,
                    voucher = voucherResponse
                });
            }

            return Ok(new
            {
                success = true,
                message = "Voucher reimpreso exitosamente",
                voucher = voucherResponse
            });
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error al reimprimir voucher de compra {CompraId}", id);
            return StatusCode(500, new { message = "Error interno al reimprimir el voucher" });
        }
    }
}
