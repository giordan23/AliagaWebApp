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

public class VentasService : IVentasService
{
    private readonly AppDbContext _context;
    private readonly ICajaRepository _cajaRepository;
    private readonly IVentasRepository _ventasRepository;
    private readonly ILogger<VentasService> _logger;

    public VentasService(
        AppDbContext context,
        ICajaRepository cajaRepository,
        IVentasRepository ventasRepository,
        ILogger<VentasService> logger)
    {
        _context = context;
        _cajaRepository = cajaRepository;
        _ventasRepository = ventasRepository;
        _logger = logger;
    }

    public async Task<VentaResponse> RegistrarVentaAsync(RegistrarVentaRequest request)
    {
        using var transaction = await _context.Database.BeginTransactionAsync();

        try
        {
            // 1. Validar que existe caja abierta del día actual
            var cajaActual = await _cajaRepository.GetCajaAbiertaAsync();
            if (cajaActual == null)
            {
                throw new InvalidOperationException("No existe una caja abierta. Por favor, abra la caja antes de registrar ventas.");
            }

            // 2. Calcular monto total
            var montoTotal = CalculosHelper.CalcularMontoTotal(request.PesoNeto, request.PrecioPorKg);

            // 3. Crear la venta
            var venta = new Venta
            {
                ClienteCompradorId = request.ClienteCompradorId,
                ProductoId = request.ProductoId,
                CajaId = cajaActual.Id,
                PesoBruto = request.PesoNeto, // En ventas, peso bruto = peso neto
                PesoNeto = request.PesoNeto,
                PrecioPorKg = request.PrecioPorKg,
                MontoTotal = montoTotal,
                FechaVenta = DateTime.Now,
                Editada = false,
                EsAjustePosterior = false
            };

            // 4. Guardar la venta
            var ventaNueva = await _ventasRepository.AddAsync(venta);

            // 5. Crear movimiento de caja (ingreso)
            var movimiento = new MovimientoCaja
            {
                CajaId = cajaActual.Id,
                TipoMovimiento = TipoMovimiento.Venta,
                ReferenciaId = ventaNueva.Id,
                Concepto = $"Venta de {ventaNueva.Producto?.Nombre ?? "producto"}",
                Monto = montoTotal,
                TipoOperacion = TipoOperacion.Ingreso,
                FechaMovimiento = DateTime.Now,
                EsAjustePosterior = false
            };

            _context.MovimientosCaja.Add(movimiento);
            await _context.SaveChangesAsync();

            // 6. Commit de la transacción
            await transaction.CommitAsync();

            // 7. Cargar las relaciones
            await _context.Entry(ventaNueva).Reference(v => v.ClienteComprador).LoadAsync();
            await _context.Entry(ventaNueva).Reference(v => v.Producto).LoadAsync();

            // 8. Retornar respuesta
            return MapToResponse(ventaNueva);
        }
        catch (Exception ex)
        {
            await transaction.RollbackAsync();
            _logger.LogError(ex, "Error al registrar venta");
            throw;
        }
    }

    public async Task<VentaResponse> EditarVentaAsync(int ventaId, EditarVentaRequest request)
    {
        using var transaction = await _context.Database.BeginTransactionAsync();

        try
        {
            // 1. Obtener la venta
            var venta = await _ventasRepository.GetByIdAsync(ventaId);
            if (venta == null)
            {
                throw new InvalidOperationException("La venta no existe.");
            }

            // 2. Validar que la venta es del día actual
            var esDelDiaActual = await _ventasRepository.EsVentaDelDiaActualAsync(ventaId);
            if (!esDelDiaActual)
            {
                throw new InvalidOperationException("Solo se pueden editar ventas del día actual. Para modificaciones de días anteriores, use la función de Ajuste Posterior.");
            }

            // 3. Recalcular monto total
            var montoTotalAnterior = venta.MontoTotal;

            venta.PesoNeto = request.PesoNeto;
            venta.PesoBruto = request.PesoNeto; // En ventas, peso bruto = peso neto
            venta.PrecioPorKg = request.PrecioPorKg;
            venta.MontoTotal = CalculosHelper.CalcularMontoTotal(request.PesoNeto, request.PrecioPorKg);
            venta.Editada = true;
            venta.FechaEdicion = DateTime.Now;

            // 4. Actualizar la venta
            await _ventasRepository.UpdateAsync(venta);

            // 5. Actualizar el movimiento de caja correspondiente
            var movimiento = await _context.MovimientosCaja
                .FirstOrDefaultAsync(m => m.TipoMovimiento == TipoMovimiento.Venta && m.ReferenciaId == ventaId);

            if (movimiento != null)
            {
                movimiento.Monto = venta.MontoTotal;
                movimiento.Concepto = $"Venta de {venta.Producto?.Nombre ?? "producto"} (Editada)";
                _context.MovimientosCaja.Update(movimiento);
                await _context.SaveChangesAsync();
            }

            // 6. Commit de la transacción
            await transaction.CommitAsync();

            _logger.LogInformation("Venta {VentaId} editada. Monto anterior: {MontoAnterior}, Nuevo monto: {MontoNuevo}",
                ventaId, montoTotalAnterior, venta.MontoTotal);

            // 7. Retornar respuesta
            return MapToResponse(venta);
        }
        catch (Exception ex)
        {
            await transaction.RollbackAsync();
            _logger.LogError(ex, "Error al editar venta {VentaId}", ventaId);
            throw;
        }
    }

    public async Task<VentaResponse?> GetByIdAsync(int id)
    {
        var venta = await _ventasRepository.GetByIdAsync(id);
        return venta == null ? null : MapToResponse(venta);
    }

    public async Task<(List<VentaResponse> Ventas, int Total)> GetAllAsync(
        int page = 1,
        int pageSize = 50,
        int? clienteId = null,
        int? productoId = null,
        DateTime? fechaInicio = null,
        DateTime? fechaFin = null)
    {
        var skip = (page - 1) * pageSize;

        var ventas = await _ventasRepository.GetAllAsync(skip, pageSize, clienteId, productoId, fechaInicio, fechaFin);
        var total = await _ventasRepository.GetTotalCountAsync(clienteId, productoId, fechaInicio, fechaFin);

        var ventasResponse = ventas.Select(MapToResponse).ToList();

        return (ventasResponse, total);
    }

    public async Task<List<VentaResponse>> GetByCajaIdAsync(int cajaId)
    {
        var ventas = await _ventasRepository.GetByCajaIdAsync(cajaId);
        return ventas.Select(MapToResponse).ToList();
    }

    #region Mapeo

    private VentaResponse MapToResponse(Venta venta)
    {
        return new VentaResponse
        {
            Id = venta.Id,
            ClienteCompradorId = venta.ClienteCompradorId,
            ClienteNombre = venta.ClienteComprador?.Nombre ?? string.Empty,
            ClienteRUC = string.Empty, // ClienteComprador no tiene RUC
            ProductoId = venta.ProductoId,
            ProductoNombre = venta.Producto?.Nombre ?? string.Empty,
            CajaId = venta.CajaId,
            PesoNeto = venta.PesoNeto,
            PrecioPorKg = venta.PrecioPorKg,
            MontoTotal = venta.MontoTotal,
            FechaVenta = venta.FechaVenta,
            Editada = venta.Editada,
            FechaEdicion = venta.FechaEdicion,
            EsAjustePosterior = venta.EsAjustePosterior
        };
    }

    #endregion
}
