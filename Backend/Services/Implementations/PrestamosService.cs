using Backend.Data;
using Backend.DTOs.Requests;
using Backend.DTOs.Responses;
using Backend.Enums;
using Backend.Models;
using Backend.Repositories.Interfaces;
using Backend.Services.Interfaces;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;

namespace Backend.Services.Implementations;

public class PrestamosService : IPrestamosService
{
    private readonly AppDbContext _context;
    private readonly ICajaRepository _cajaRepository;
    private readonly IPrestamosRepository _prestamosRepository;
    private readonly IClienteRepository _clienteRepository;
    private readonly ILogger<PrestamosService> _logger;

    public PrestamosService(
        AppDbContext context,
        ICajaRepository cajaRepository,
        IPrestamosRepository prestamosRepository,
        IClienteRepository clienteRepository,
        ILogger<PrestamosService> logger)
    {
        _context = context;
        _cajaRepository = cajaRepository;
        _prestamosRepository = prestamosRepository;
        _clienteRepository = clienteRepository;
        _logger = logger;
    }

    public async Task<PrestamoResponse> RegistrarPrestamoAsync(RegistrarPrestamoRequest request)
    {
        using var transaction = await _context.Database.BeginTransactionAsync();

        try
        {
            // 1. Validar que existe caja abierta del día actual
            var cajaActual = await _cajaRepository.GetCajaAbiertaAsync();
            if (cajaActual == null)
            {
                throw new InvalidOperationException("No existe una caja abierta. Por favor, abra la caja antes de registrar préstamos.");
            }

            // 2. Validar que el cliente existe
            var cliente = await _clienteRepository.GetProveedorByIdAsync(request.ClienteProveedorId);
            if (cliente == null)
            {
                throw new InvalidOperationException("El cliente proveedor no existe.");
            }

            if (cliente.EsAnonimo)
            {
                throw new InvalidOperationException("No se pueden registrar préstamos al cliente anónimo.");
            }

            // 3. Incrementar saldo de préstamo del cliente
            var saldoAnterior = cliente.SaldoPrestamo;
            cliente.SaldoPrestamo += request.Monto;
            await _clienteRepository.UpdateProveedorAsync(cliente);

            // 4. Crear registro de préstamo
            var prestamo = new Prestamo
            {
                ClienteProveedorId = request.ClienteProveedorId,
                CajaId = cajaActual.Id,
                TipoMovimiento = TipoMovimiento.Prestamo,
                Monto = request.Monto,
                Descripcion = request.Concepto,
                FechaMovimiento = DateTime.Now,
                SaldoDespues = cliente.SaldoPrestamo,
                EsAjustePosterior = false
            };

            var prestamoNuevo = await _prestamosRepository.AddAsync(prestamo);

            // 5. Crear movimiento de caja (egreso)
            var movimiento = new MovimientoCaja
            {
                CajaId = cajaActual.Id,
                TipoMovimiento = TipoMovimiento.Prestamo,
                ReferenciaId = prestamoNuevo.Id,
                Concepto = $"PRÉSTAMO A {cliente.NombreCompleto.ToUpper()} - {request.Concepto.ToUpper()}",
                Monto = request.Monto,
                TipoOperacion = TipoOperacion.Egreso,
                FechaMovimiento = DateTime.Now,
                EsAjustePosterior = false
            };

            _context.MovimientosCaja.Add(movimiento);
            await _context.SaveChangesAsync();

            // 6. Commit de la transacción
            await transaction.CommitAsync();

            _logger.LogInformation("Préstamo registrado para cliente {ClienteId}. Saldo anterior: {SaldoAnterior}, Nuevo saldo: {NuevoSaldo}",
                cliente.Id, saldoAnterior, cliente.SaldoPrestamo);

            // 7. Cargar las relaciones
            await _context.Entry(prestamoNuevo).Reference(p => p.ClienteProveedor).LoadAsync();

            // 8. Retornar respuesta
            return MapToResponse(prestamoNuevo);
        }
        catch (Exception ex)
        {
            await transaction.RollbackAsync();
            _logger.LogError(ex, "Error al registrar préstamo");
            throw;
        }
    }

    public async Task<PrestamoResponse> RegistrarAbonoAsync(RegistrarAbonoRequest request)
    {
        using var transaction = await _context.Database.BeginTransactionAsync();

        try
        {
            // 1. Validar que existe caja abierta del día actual
            var cajaActual = await _cajaRepository.GetCajaAbiertaAsync();
            if (cajaActual == null)
            {
                throw new InvalidOperationException("No existe una caja abierta. Por favor, abra la caja antes de registrar abonos.");
            }

            // 2. Validar que el cliente existe
            var cliente = await _clienteRepository.GetProveedorByIdAsync(request.ClienteProveedorId);
            if (cliente == null)
            {
                throw new InvalidOperationException("El cliente proveedor no existe.");
            }

            if (cliente.EsAnonimo)
            {
                throw new InvalidOperationException("No se pueden registrar abonos al cliente anónimo.");
            }

            // 3. Validar que el cliente tiene saldo pendiente
            if (cliente.SaldoPrestamo <= 0)
            {
                throw new InvalidOperationException("El cliente no tiene préstamos pendientes.");
            }

            // 4. Validar que el monto del abono no exceda el saldo
            if (request.Monto > cliente.SaldoPrestamo)
            {
                throw new InvalidOperationException($"El monto del abono (S/ {request.Monto:N2}) no puede exceder el saldo pendiente (S/ {cliente.SaldoPrestamo:N2}).");
            }

            // 5. Decrementar saldo de préstamo del cliente
            var saldoAnterior = cliente.SaldoPrestamo;
            cliente.SaldoPrestamo -= request.Monto;
            await _clienteRepository.UpdateProveedorAsync(cliente);

            // 6. Crear registro de abono
            var abono = new Prestamo
            {
                ClienteProveedorId = request.ClienteProveedorId,
                CajaId = cajaActual.Id,
                TipoMovimiento = TipoMovimiento.Abono,
                Monto = request.Monto,
                Descripcion = request.Concepto,
                FechaMovimiento = DateTime.Now,
                SaldoDespues = cliente.SaldoPrestamo,
                EsAjustePosterior = false
            };

            var abonoNuevo = await _prestamosRepository.AddAsync(abono);

            // 7. Crear movimiento de caja (ingreso)
            var movimiento = new MovimientoCaja
            {
                CajaId = cajaActual.Id,
                TipoMovimiento = TipoMovimiento.Abono,
                ReferenciaId = abonoNuevo.Id,
                Concepto = $"ABONO DE {cliente.NombreCompleto.ToUpper()} - {request.Concepto.ToUpper()}",
                Monto = request.Monto,
                TipoOperacion = TipoOperacion.Ingreso,
                FechaMovimiento = DateTime.Now,
                EsAjustePosterior = false
            };

            _context.MovimientosCaja.Add(movimiento);
            await _context.SaveChangesAsync();

            // 8. Commit de la transacción
            await transaction.CommitAsync();

            _logger.LogInformation("Abono registrado para cliente {ClienteId}. Saldo anterior: {SaldoAnterior}, Nuevo saldo: {NuevoSaldo}",
                cliente.Id, saldoAnterior, cliente.SaldoPrestamo);

            // 9. Cargar las relaciones
            await _context.Entry(abonoNuevo).Reference(p => p.ClienteProveedor).LoadAsync();

            // 10. Retornar respuesta
            return MapToResponse(abonoNuevo);
        }
        catch (Exception ex)
        {
            await transaction.RollbackAsync();
            _logger.LogError(ex, "Error al registrar abono");
            throw;
        }
    }

    public async Task<PrestamoResponse?> GetByIdAsync(int id)
    {
        var prestamo = await _prestamosRepository.GetByIdAsync(id);
        return prestamo == null ? null : MapToResponse(prestamo);
    }

    public async Task<(List<PrestamoResponse> Prestamos, int Total)> GetAllAsync(
        int page = 1,
        int pageSize = 50,
        int? clienteId = null,
        DateTime? fechaInicio = null,
        DateTime? fechaFin = null)
    {
        var skip = (page - 1) * pageSize;

        var prestamos = await _prestamosRepository.GetAllAsync(skip, pageSize, clienteId, fechaInicio, fechaFin);
        var total = await _prestamosRepository.GetTotalCountAsync(clienteId, fechaInicio, fechaFin);

        var prestamosResponse = prestamos.Select(MapToResponse).ToList();

        return (prestamosResponse, total);
    }

    public async Task<List<PrestamoResponse>> GetByClienteIdAsync(int clienteId)
    {
        var prestamos = await _prestamosRepository.GetByClienteIdAsync(clienteId);
        return prestamos.Select(MapToResponse).ToList();
    }

    public async Task<List<PrestamoResponse>> GetByCajaIdAsync(int cajaId)
    {
        var prestamos = await _prestamosRepository.GetByCajaIdAsync(cajaId);
        return prestamos.Select(MapToResponse).ToList();
    }

    #region Mapeo

    private PrestamoResponse MapToResponse(Prestamo prestamo)
    {
        return new PrestamoResponse
        {
            Id = prestamo.Id,
            ClienteProveedorId = prestamo.ClienteProveedorId,
            ClienteNombre = prestamo.ClienteProveedor?.NombreCompleto ?? string.Empty,
            ClienteDNI = prestamo.ClienteProveedor?.DNI ?? string.Empty,
            CajaId = prestamo.CajaId,
            TipoMovimiento = prestamo.TipoMovimiento,
            Monto = prestamo.Monto,
            SaldoDespues = prestamo.SaldoDespues,
            Descripcion = prestamo.Descripcion ?? string.Empty,
            FechaMovimiento = prestamo.FechaMovimiento,
            EsAjustePosterior = prestamo.EsAjustePosterior
        };
    }

    #endregion
}
