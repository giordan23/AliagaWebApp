using Backend.Data;
using Backend.DTOs.Requests;
using Backend.DTOs.Responses;
using Backend.Enums;
using Backend.Helpers;
using Backend.Models;
using Backend.Repositories.Interfaces;
using Backend.Services.Interfaces;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;

namespace Backend.Services.Implementations;

public class ComprasService : IComprasService
{
    private readonly AppDbContext _context;
    private readonly ICajaRepository _cajaRepository;
    private readonly ICompraRepository _compraRepository;
    private readonly IClienteRepository _clienteRepository;
    private readonly IVoucherService _voucherService;
    private readonly ILogger<ComprasService> _logger;

    public ComprasService(
        AppDbContext context,
        ICajaRepository cajaRepository,
        ICompraRepository compraRepository,
        IClienteRepository clienteRepository,
        IVoucherService voucherService,
        ILogger<ComprasService> logger)
    {
        _context = context;
        _cajaRepository = cajaRepository;
        _compraRepository = compraRepository;
        _clienteRepository = clienteRepository;
        _voucherService = voucherService;
        _logger = logger;
    }

    public async Task<CompraResponse> RegistrarCompraAsync(RegistrarCompraRequest request)
    {
        using var transaction = await _context.Database.BeginTransactionAsync();

        try
        {
            // 1. Validar que existe caja abierta del día actual
            var cajaActual = await _cajaRepository.GetCajaAbiertaAsync();
            if (cajaActual == null)
            {
                throw new InvalidOperationException("No existe una caja abierta. Por favor, abra la caja antes de registrar compras.");
            }

            // 2. Validar que el cliente existe
            var cliente = await _clienteRepository.GetProveedorByIdAsync(request.ClienteProveedorId);
            if (cliente == null)
            {
                throw new InvalidOperationException("El cliente proveedor no existe.");
            }

            // 3. Obtener configuración para generar número de voucher
            var config = await _context.ConfiguracionNegocio.FirstOrDefaultAsync();
            if (config == null)
            {
                throw new InvalidOperationException("No se encontró la configuración del negocio.");
            }

            // 4. Calcular peso neto y monto total
            var pesoNeto = CalculosHelper.CalcularPesoNeto(request.PesoBruto, request.DescuentoKg);
            var montoTotal = CalculosHelper.CalcularMontoTotal(pesoNeto, request.PrecioPorKg);

            // 5. Crear la compra
            var compra = new Compra
            {
                NumeroVoucher = config.ContadorVoucher.ToString().PadLeft(8, '0'),
                ClienteProveedorId = request.ClienteProveedorId,
                ProductoId = request.ProductoId,
                CajaId = cajaActual.Id,
                NivelSecado = request.NivelSecado,
                Calidad = request.Calidad,
                TipoPesado = request.TipoPesado,
                PesoBruto = request.PesoBruto,
                DescuentoKg = request.DescuentoKg,
                PesoNeto = pesoNeto,
                PrecioPorKg = request.PrecioPorKg,
                MontoTotal = montoTotal,
                FechaCompra = DateTime.Now,
                Editada = false,
                EsAjustePosterior = false
            };

            // 6. Guardar la compra
            var compraNueva = await _compraRepository.AddAsync(compra);

            // 7. Incrementar contador de voucher
            config.ContadorVoucher++;
            _context.ConfiguracionNegocio.Update(config);
            await _context.SaveChangesAsync();

            // 8. Crear movimiento de caja (egreso)
            var movimiento = new MovimientoCaja
            {
                CajaId = cajaActual.Id,
                TipoMovimiento = TipoMovimiento.Compra,
                ReferenciaId = compraNueva.Id,
                Concepto = $"Compra de {compraNueva.Producto?.Nombre ?? "producto"} - Voucher {compraNueva.NumeroVoucher}",
                Monto = montoTotal,
                TipoOperacion = TipoOperacion.Egreso,
                FechaMovimiento = DateTime.Now,
                EsAjustePosterior = false
            };

            _context.MovimientosCaja.Add(movimiento);
            await _context.SaveChangesAsync();

            // 9. Si hay abono implícito (cuando request tiene un monto de abono)
            // Por ahora no implementamos esto, se manejará en una versión futura

            // 10. Commit de la transacción
            await transaction.CommitAsync();

            // 11. Cargar las relaciones para el voucher
            await _context.Entry(compraNueva).Reference(c => c.ClienteProveedor).LoadAsync();
            await _context.Entry(compraNueva.ClienteProveedor!).Reference(c => c.Zona).LoadAsync();
            await _context.Entry(compraNueva).Reference(c => c.Producto).LoadAsync();

            // 12. Generar e imprimir voucher (no bloquea si falla)
            try
            {
                await _voucherService.GenerarYImprimirVoucherAsync(compraNueva, esDuplicado: false);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error al generar voucher para compra {CompraId}, pero la compra fue registrada", compraNueva.Id);
            }

            // 13. Retornar respuesta
            return MapToResponse(compraNueva);
        }
        catch (Exception ex)
        {
            await transaction.RollbackAsync();
            _logger.LogError(ex, "Error al registrar compra");
            throw;
        }
    }

    public async Task<CompraResponse> EditarCompraAsync(int compraId, EditarCompraRequest request)
    {
        using var transaction = await _context.Database.BeginTransactionAsync();

        try
        {
            // 1. Obtener la compra
            var compra = await _compraRepository.GetByIdAsync(compraId);
            if (compra == null)
            {
                throw new InvalidOperationException("La compra no existe.");
            }

            // 2. Validar que la compra es del día actual
            var esDelDiaActual = await _compraRepository.EsCompraDelDiaActualAsync(compraId);
            if (!esDelDiaActual)
            {
                throw new InvalidOperationException("Solo se pueden editar compras del día actual. Para modificaciones de días anteriores, use la función de Ajuste Posterior.");
            }

            // 3. Recalcular peso neto y monto total
            var pesoNetoAnterior = compra.PesoNeto;
            var montoTotalAnterior = compra.MontoTotal;

            compra.PesoBruto = request.PesoBruto;
            compra.DescuentoKg = request.DescuentoKg;
            compra.PrecioPorKg = request.PrecioPorKg;
            compra.PesoNeto = CalculosHelper.CalcularPesoNeto(request.PesoBruto, request.DescuentoKg);
            compra.MontoTotal = CalculosHelper.CalcularMontoTotal(compra.PesoNeto, request.PrecioPorKg);
            compra.Editada = true;
            compra.FechaEdicion = DateTime.Now;

            // 4. Actualizar la compra
            await _compraRepository.UpdateAsync(compra);

            // 5. Actualizar el movimiento de caja correspondiente
            var movimiento = await _context.MovimientosCaja
                .FirstOrDefaultAsync(m => m.TipoMovimiento == TipoMovimiento.Compra && m.ReferenciaId == compraId);

            if (movimiento != null)
            {
                movimiento.Monto = compra.MontoTotal;
                movimiento.Concepto = $"Compra de {compra.Producto?.Nombre ?? "producto"} - Voucher {compra.NumeroVoucher} (Editada)";
                _context.MovimientosCaja.Update(movimiento);
                await _context.SaveChangesAsync();
            }

            // 6. Commit de la transacción
            await transaction.CommitAsync();

            _logger.LogInformation("Compra {CompraId} editada. Monto anterior: {MontoAnterior}, Nuevo monto: {MontoNuevo}",
                compraId, montoTotalAnterior, compra.MontoTotal);

            // 7. Retornar respuesta
            return MapToResponse(compra);
        }
        catch (Exception ex)
        {
            await transaction.RollbackAsync();
            _logger.LogError(ex, "Error al editar compra {CompraId}", compraId);
            throw;
        }
    }

    public async Task<CompraResponse?> GetByIdAsync(int id)
    {
        var compra = await _compraRepository.GetByIdAsync(id);
        return compra == null ? null : MapToResponse(compra);
    }

    public async Task<CompraResponse?> GetByNumeroVoucherAsync(string numeroVoucher)
    {
        var compra = await _compraRepository.GetByNumeroVoucherAsync(numeroVoucher);
        return compra == null ? null : MapToResponse(compra);
    }

    public async Task<(List<CompraResponse> Compras, int Total)> GetAllAsync(
        int page = 1,
        int pageSize = 50,
        int? clienteId = null,
        int? productoId = null,
        DateTime? fechaInicio = null,
        DateTime? fechaFin = null)
    {
        var skip = (page - 1) * pageSize;

        var compras = await _compraRepository.GetAllAsync(skip, pageSize, clienteId, productoId, fechaInicio, fechaFin);
        var total = await _compraRepository.GetTotalCountAsync(clienteId, productoId, fechaInicio, fechaFin);

        var comprasResponse = compras.Select(MapToResponse).ToList();

        return (comprasResponse, total);
    }

    public async Task<List<CompraResponse>> GetByCajaIdAsync(int cajaId)
    {
        var compras = await _compraRepository.GetByCajaIdAsync(cajaId);
        return compras.Select(MapToResponse).ToList();
    }

    #region Mapeo

    private CompraResponse MapToResponse(Compra compra)
    {
        return new CompraResponse
        {
            Id = compra.Id,
            NumeroVoucher = compra.NumeroVoucher,
            ClienteProveedorId = compra.ClienteProveedorId,
            ClienteNombre = compra.ClienteProveedor?.NombreCompleto ?? string.Empty,
            ClienteDNI = compra.ClienteProveedor?.DNI ?? string.Empty,
            ProductoId = compra.ProductoId,
            ProductoNombre = compra.Producto?.Nombre ?? string.Empty,
            CajaId = compra.CajaId,
            NivelSecado = compra.NivelSecado,
            Calidad = compra.Calidad,
            TipoPesado = compra.TipoPesado,
            PesoBruto = compra.PesoBruto,
            DescuentoKg = compra.DescuentoKg,
            PesoNeto = compra.PesoNeto,
            PrecioPorKg = compra.PrecioPorKg,
            MontoTotal = compra.MontoTotal,
            FechaCompra = compra.FechaCompra,
            Editada = compra.Editada,
            FechaEdicion = compra.FechaEdicion,
            EsAjustePosterior = compra.EsAjustePosterior
        };
    }

    #endregion
}
