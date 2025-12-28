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
            // 1. Validar que hay al menos un detalle
            if (request.Detalles == null || !request.Detalles.Any())
            {
                throw new InvalidOperationException("Debe incluir al menos un producto en la compra.");
            }

            // 2. Validar que existe caja abierta del día actual
            var cajaActual = await _cajaRepository.GetCajaAbiertaAsync();
            if (cajaActual == null)
            {
                throw new InvalidOperationException("No existe una caja abierta. Por favor, abra la caja antes de registrar compras.");
            }

            // 3. Obtener o crear el cliente proveedor
            int clienteProveedorId;

            if (request.ClienteProveedorId.HasValue && request.ClienteProveedorId.Value > 0)
            {
                // Usar cliente existente
                var clienteExistente = await _clienteRepository.GetProveedorByIdAsync(request.ClienteProveedorId.Value);
                if (clienteExistente == null)
                {
                    throw new InvalidOperationException("El cliente proveedor no existe.");
                }
                clienteProveedorId = clienteExistente.Id;
            }
            else if (request.NuevoCliente != null)
            {
                // Verificar si ya existe un cliente con ese DNI
                var clientePorDni = await _clienteRepository.GetProveedorByDniAsync(request.NuevoCliente.Dni);

                if (clientePorDni != null)
                {
                    // Cliente ya existe, usar su ID
                    clienteProveedorId = clientePorDni.Id;
                    _logger.LogInformation("Cliente con DNI {DNI} ya existe, usando ID {ClienteId}", request.NuevoCliente.Dni, clienteProveedorId);
                }
                else
                {
                    // Crear nuevo cliente
                    var nuevoCliente = new ClienteProveedor
                    {
                        DNI = request.NuevoCliente.Dni,
                        NombreCompleto = request.NuevoCliente.NombreCompleto.ToUpper(),
                        FechaCreacion = DateTime.Now,
                        FechaModificacion = DateTime.Now,
                        SaldoPrestamo = 0,
                        EsAnonimo = false
                    };

                    _context.ClientesProveedores.Add(nuevoCliente);
                    await _context.SaveChangesAsync();
                    clienteProveedorId = nuevoCliente.Id;
                    _logger.LogInformation("Nuevo cliente creado con DNI {DNI}, ID {ClienteId}", request.NuevoCliente.Dni, clienteProveedorId);
                }
            }
            else
            {
                throw new InvalidOperationException("Debe proporcionar un ClienteProveedorId o datos de NuevoCliente.");
            }

            // 4. Obtener configuración para generar número de voucher
            var config = await _context.ConfiguracionNegocio.FirstOrDefaultAsync();
            if (config == null)
            {
                throw new InvalidOperationException("No se encontró la configuración del negocio.");
            }

            // 5. Crear la compra base
            var compra = new Compra
            {
                NumeroVoucher = config.ContadorVoucher.ToString().PadLeft(8, '0'),
                ClienteProveedorId = clienteProveedorId,
                CajaId = cajaActual.Id,
                PesoTotal = 0,
                MontoTotal = 0,
                FechaCompra = DateTime.Now,
                Editada = false,
                EsAjustePosterior = false,
                Detalles = new List<DetalleCompra>()
            };

            // 6. Procesar cada detalle
            foreach (var detalleReq in request.Detalles)
            {
                var producto = await _context.Productos.FindAsync(detalleReq.ProductoId);
                if (producto == null)
                {
                    throw new InvalidOperationException($"Producto {detalleReq.ProductoId} no encontrado.");
                }

                var pesoNeto = CalculosHelper.CalcularPesoNeto(detalleReq.PesoBruto, detalleReq.DescuentoKg);
                var subtotal = CalculosHelper.CalcularMontoTotal(pesoNeto, detalleReq.PrecioPorKg);

                var detalle = new DetalleCompra
                {
                    ProductoId = detalleReq.ProductoId,
                    NivelSecado = detalleReq.NivelSecado,
                    Calidad = detalleReq.Calidad,
                    TipoPesado = detalleReq.TipoPesado,
                    PesoBruto = detalleReq.PesoBruto,
                    DescuentoKg = detalleReq.DescuentoKg,
                    PesoNeto = pesoNeto,
                    PrecioPorKg = detalleReq.PrecioPorKg,
                    Subtotal = subtotal,
                    FechaCreacion = DateTime.Now
                };

                compra.Detalles.Add(detalle);
                compra.PesoTotal += pesoNeto;
                compra.MontoTotal += subtotal;
            }

            // 7. Guardar la compra con detalles
            var compraNueva = await _compraRepository.AddAsync(compra);

            // 8. Incrementar contador de voucher
            config.ContadorVoucher++;
            _context.ConfiguracionNegocio.Update(config);
            await _context.SaveChangesAsync();

            // 9. Crear movimiento de caja (UN SOLO egreso por el total)
            var movimiento = new MovimientoCaja
            {
                CajaId = cajaActual.Id,
                TipoMovimiento = TipoMovimiento.Compra,
                ReferenciaId = compraNueva.Id,
                Concepto = $"COMPRA CON {compra.Detalles.Count} PRODUCTO(S) - VOUCHER {compraNueva.NumeroVoucher}",
                Monto = compra.MontoTotal,
                TipoOperacion = TipoOperacion.Egreso,
                FechaMovimiento = DateTime.Now,
                EsAjustePosterior = false
            };

            _context.MovimientosCaja.Add(movimiento);
            await _context.SaveChangesAsync();

            // 10. Commit de la transacción
            await transaction.CommitAsync();

            // 11. Cargar las relaciones para el voucher
            await _context.Entry(compraNueva).Reference(c => c.ClienteProveedor).LoadAsync();
            await _context.Entry(compraNueva.ClienteProveedor!).Reference(c => c.Zona).LoadAsync();
            await _context.Entry(compraNueva).Collection(c => c.Detalles).LoadAsync();

            foreach (var detalle in compraNueva.Detalles)
            {
                await _context.Entry(detalle).Reference(d => d.Producto).LoadAsync();
            }

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
            // 1. Validar existencia
            var compra = await _compraRepository.GetByIdWithDetailsAsync(compraId);
            if (compra == null)
                throw new InvalidOperationException("Compra no encontrada.");

            // 2. Validar plazo temporal (máximo 1 día)
            var fechaLimite = compra.FechaCompra.Date.AddDays(2);
            if (DateTime.Now >= fechaLimite)
                throw new InvalidOperationException(
                    $"Solo se pueden editar compras hasta el día siguiente de su registro. " +
                    $"Esta compra fue registrada el {compra.FechaCompra:dd/MM/yyyy}.");

            // 3. Validar cantidad de detalles (NO agregar/eliminar)
            if (request.Detalles == null || !request.Detalles.Any())
                throw new InvalidOperationException("Debe incluir al menos un detalle.");

            if (request.Detalles.Count != compra.Detalles.Count)
                throw new InvalidOperationException(
                    $"La compra tiene {compra.Detalles.Count} detalle(s). " +
                    $"No se pueden agregar ni eliminar detalles, solo modificarlos.");

            // 4. Validar IDs de detalles
            var idsDetallesOriginales = compra.Detalles.Select(d => d.Id).ToHashSet();
            foreach (var detalleReq in request.Detalles)
            {
                if (!idsDetallesOriginales.Contains(detalleReq.Id))
                    throw new InvalidOperationException(
                        $"El detalle con ID {detalleReq.Id} no pertenece a esta compra.");
            }

            // 5. Validar nuevo cliente si cambió
            if (request.ClienteProveedorId != compra.ClienteProveedorId)
            {
                var nuevoCliente = await _clienteRepository.GetProveedorByIdAsync(request.ClienteProveedorId);
                if (nuevoCliente == null)
                    throw new InvalidOperationException("El nuevo cliente proveedor no existe.");
            }

            // 6. Validar productos
            var productosIds = request.Detalles.Select(d => d.ProductoId).Distinct();
            foreach (var productoId in productosIds)
            {
                var producto = await _context.Productos.FindAsync(productoId);
                if (producto == null)
                    throw new InvalidOperationException($"Producto {productoId} no encontrado.");
            }

            // 7. Guardar valores originales
            var montoOriginal = compra.MontoTotal;
            var cajaId = compra.CajaId;

            // 8. Actualizar cliente
            compra.ClienteProveedorId = request.ClienteProveedorId;

            // 9. Actualizar detalles y recalcular totales
            compra.PesoTotal = 0;
            compra.MontoTotal = 0;

            foreach (var detalleReq in request.Detalles)
            {
                var detalleOriginal = compra.Detalles.First(d => d.Id == detalleReq.Id);

                detalleOriginal.ProductoId = detalleReq.ProductoId;
                detalleOriginal.NivelSecado = detalleReq.NivelSecado;
                detalleOriginal.Calidad = detalleReq.Calidad;
                detalleOriginal.TipoPesado = detalleReq.TipoPesado;
                detalleOriginal.PesoBruto = detalleReq.PesoBruto;
                detalleOriginal.DescuentoKg = detalleReq.DescuentoKg;
                detalleOriginal.PrecioPorKg = detalleReq.PrecioPorKg;

                detalleOriginal.PesoNeto = CalculosHelper.CalcularPesoNeto(
                    detalleReq.PesoBruto,
                    detalleReq.DescuentoKg);
                detalleOriginal.Subtotal = CalculosHelper.CalcularMontoTotal(
                    detalleOriginal.PesoNeto,
                    detalleReq.PrecioPorKg);

                compra.PesoTotal += detalleOriginal.PesoNeto;
                compra.MontoTotal += detalleOriginal.Subtotal;
            }

            // 10. Marcar como editada
            compra.Editada = true;
            compra.FechaEdicion = DateTime.Now;

            // 11. Guardar cambios en compra
            await _compraRepository.UpdateAsync(compra);

            // 12. Actualizar MovimientoCaja existente
            var movimientoCaja = await _context.MovimientosCaja
                .FirstOrDefaultAsync(m =>
                    m.CajaId == cajaId &&
                    m.TipoMovimiento == TipoMovimiento.Compra &&
                    m.ReferenciaId == compraId);

            if (movimientoCaja == null)
                throw new InvalidOperationException(
                    "No se encontró el movimiento de caja asociado a esta compra.");

            movimientoCaja.Monto = compra.MontoTotal;
            movimientoCaja.Concepto = $"Compra con {compra.Detalles.Count} producto(s) - Voucher {compra.NumeroVoucher} (EDITADA)";
            _context.MovimientosCaja.Update(movimientoCaja);
            await _context.SaveChangesAsync();

            // 13. Recalcular MontoEsperado de la caja
            var caja = await _cajaRepository.GetByIdAsync(cajaId);
            if (caja != null)
            {
                var diferenciaMonto = compra.MontoTotal - montoOriginal;
                caja.MontoEsperado -= diferenciaMonto; // Compra es egreso

                // Recalcular Diferencia solo si la caja está cerrada
                if (caja.ArqueoReal.HasValue)
                {
                    caja.Diferencia = CalculosHelper.CalcularDiferencia(
                        caja.ArqueoReal.Value,
                        caja.MontoEsperado);
                }

                await _cajaRepository.UpdateAsync(caja);
            }

            // 14. Commit transacción
            await transaction.CommitAsync();

            // 15. Cargar relaciones para response
            await _context.Entry(compra).Reference(c => c.ClienteProveedor).LoadAsync();
            await _context.Entry(compra).Collection(c => c.Detalles).LoadAsync();
            foreach (var detalle in compra.Detalles)
            {
                await _context.Entry(detalle).Reference(d => d.Producto).LoadAsync();
            }

            _logger.LogInformation(
                "Compra {CompraId} editada. Monto: {MontoOriginal} → {MontoNuevo}",
                compraId, montoOriginal, compra.MontoTotal);

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
            CajaId = compra.CajaId,
            FechaCaja = compra.Caja?.Fecha ?? DateTime.MinValue,
            Detalles = compra.Detalles.Select(d => new DetalleCompraResponse
            {
                Id = d.Id,
                ProductoId = d.ProductoId,
                ProductoNombre = d.Producto?.Nombre ?? string.Empty,
                NivelSecado = d.NivelSecado,
                Calidad = d.Calidad,
                TipoPesado = d.TipoPesado,
                PesoBruto = d.PesoBruto,
                DescuentoKg = d.DescuentoKg,
                PesoNeto = d.PesoNeto,
                PrecioPorKg = d.PrecioPorKg,
                Subtotal = d.Subtotal
            }).ToList(),
            PesoTotal = compra.PesoTotal,
            MontoTotal = compra.MontoTotal,
            FechaCompra = compra.FechaCompra,
            Editada = compra.Editada,
            FechaEdicion = compra.FechaEdicion,
            EsAjustePosterior = compra.EsAjustePosterior
        };
    }

    #endregion
}
