using Backend.Data;
using Backend.DTOs.Responses;
using Backend.Enums;
using Backend.Helpers;
using Backend.Services.Interfaces;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;

namespace Backend.Services.Implementations;

public class DashboardService : IDashboardService
{
    private readonly AppDbContext _context;
    private readonly ILogger<DashboardService> _logger;

    public DashboardService(AppDbContext context, ILogger<DashboardService> logger)
    {
        _context = context;
        _logger = logger;
    }

    public async Task<ResumenDiaResponse> ObtenerResumenDelDiaAsync()
    {
        try
        {
            var hoy = DateTime.Today;
            var cajaHoy = await _context.Cajas
                .FirstOrDefaultAsync(c => c.Fecha.Date == hoy);

            if (cajaHoy == null)
            {
                return new ResumenDiaResponse
                {
                    Fecha = hoy,
                    CajaAbierta = false
                };
            }

            // Obtener movimientos del día
            var movimientos = await _context.MovimientosCaja
                .Where(m => m.CajaId == cajaHoy.Id)
                .ToListAsync();

            // Calcular totales de compras
            var comprasHoy = await _context.Compras
                .Where(c => c.CajaId == cajaHoy.Id)
                .ToListAsync();

            // Calcular totales de ventas
            var ventasHoy = await _context.Ventas
                .Where(v => v.CajaId == cajaHoy.Id)
                .ToListAsync();

            // Calcular totales de préstamos y abonos
            var prestamosHoy = await _context.Prestamos
                .Where(p => p.CajaId == cajaHoy.Id && p.TipoMovimiento == TipoMovimiento.Prestamo)
                .ToListAsync();

            var abonosHoy = await _context.Prestamos
                .Where(p => p.CajaId == cajaHoy.Id && p.TipoMovimiento == TipoMovimiento.Abono)
                .ToListAsync();

            // Calcular saldo esperado
            var totalIngresos = movimientos
                .Where(m => m.TipoOperacion == TipoOperacion.Ingreso)
                .Sum(m => m.Monto);

            var totalEgresos = movimientos
                .Where(m => m.TipoOperacion == TipoOperacion.Egreso)
                .Sum(m => m.Monto);

            var saldoEsperado = CalculosHelper.CalcularSaldoEsperado(
                cajaHoy.MontoInicial,
                totalIngresos,
                totalEgresos);

            return new ResumenDiaResponse
            {
                Fecha = cajaHoy.Fecha,
                CajaAbierta = cajaHoy.Estado == EstadoCaja.Abierta,
                CajaId = cajaHoy.Id,
                MontoInicialCaja = cajaHoy.MontoInicial,
                TotalCompras = comprasHoy.Count,
                MontoTotalCompras = comprasHoy.Sum(c => c.MontoTotal),
                PesoTotalComprasKg = comprasHoy.Sum(c => c.PesoTotal),
                TotalVentas = ventasHoy.Count,
                MontoTotalVentas = ventasHoy.Sum(v => v.MontoTotal),
                TotalPrestamos = prestamosHoy.Count,
                MontoTotalPrestamos = prestamosHoy.Sum(p => p.Monto),
                TotalAbonos = abonosHoy.Count,
                MontoTotalAbonos = abonosHoy.Sum(a => a.Monto),
                SaldoEsperado = saldoEsperado
            };
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error al obtener resumen del día");
            throw;
        }
    }

    public async Task<EstadoCajaResponse> ObtenerEstadoCajaAsync()
    {
        try
        {
            var hoy = DateTime.Today;
            var cajaHoy = await _context.Cajas
                .FirstOrDefaultAsync(c => c.Fecha.Date == hoy);

            if (cajaHoy == null)
            {
                return new EstadoCajaResponse
                {
                    CajaAbierta = false,
                    Mensaje = "No hay caja abierta para el día de hoy"
                };
            }

            // Calcular saldo esperado
            var movimientos = await _context.MovimientosCaja
                .Where(m => m.CajaId == cajaHoy.Id)
                .ToListAsync();

            var totalIngresos = movimientos
                .Where(m => m.TipoOperacion == TipoOperacion.Ingreso)
                .Sum(m => m.Monto);

            var totalEgresos = movimientos
                .Where(m => m.TipoOperacion == TipoOperacion.Egreso)
                .Sum(m => m.Monto);

            var saldoEsperado = CalculosHelper.CalcularSaldoEsperado(
                cajaHoy.MontoInicial,
                totalIngresos,
                totalEgresos);

            return new EstadoCajaResponse
            {
                CajaId = cajaHoy.Id,
                Fecha = cajaHoy.Fecha,
                CajaAbierta = cajaHoy.Estado == EstadoCaja.Abierta,
                Estado = cajaHoy.Estado,
                MontoInicial = cajaHoy.MontoInicial,
                MontoEsperado = saldoEsperado,
                ArqueoReal = cajaHoy.ArqueoReal,
                Diferencia = cajaHoy.Diferencia,
                Mensaje = cajaHoy.Estado == EstadoCaja.Abierta
                    ? $"Caja abierta - Saldo esperado: S/ {saldoEsperado:N2}"
                    : "Caja cerrada"
            };
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error al obtener estado de caja");
            throw;
        }
    }

    public async Task<List<AlertaResponse>> ObtenerAlertasAsync()
    {
        try
        {
            var alertas = new List<AlertaResponse>();

            // Alerta 1: Verificar si hay caja abierta
            var hoy = DateTime.Today;
            var cajaHoy = await _context.Cajas
                .FirstOrDefaultAsync(c => c.Fecha.Date == hoy);

            if (cajaHoy == null)
            {
                alertas.Add(new AlertaResponse
                {
                    Tipo = "caja",
                    Mensaje = "No hay caja abierta",
                    Detalle = "Debe abrir la caja antes de realizar operaciones",
                    Prioridad = "alta"
                });
            }
            else if (cajaHoy.Estado != EstadoCaja.Abierta)
            {
                alertas.Add(new AlertaResponse
                {
                    Tipo = "caja",
                    Mensaje = "La caja del día ya fue cerrada",
                    Detalle = "No se pueden realizar más operaciones del día",
                    Prioridad = "media"
                });
            }

            // Alerta 2: Verificar cajas de días anteriores sin cerrar
            var cajaSinCerrar = await _context.Cajas
                .Where(c => c.Estado == EstadoCaja.Abierta && c.Fecha.Date < hoy)
                .OrderByDescending(c => c.Fecha)
                .FirstOrDefaultAsync();

            if (cajaSinCerrar != null)
            {
                alertas.Add(new AlertaResponse
                {
                    Tipo = "caja",
                    Mensaje = "Hay una caja de día anterior sin cerrar",
                    Detalle = $"Caja del {cajaSinCerrar.Fecha:dd/MM/yyyy} permanece abierta",
                    ReferenciaId = cajaSinCerrar.Id,
                    Prioridad = "alta"
                });
            }

            // Alerta 3: Top deudores (clientes con préstamos altos)
            var topDeudores = await _context.ClientesProveedores
                .Where(c => c.SaldoPrestamo > 0 && !c.EsAnonimo)
                .OrderByDescending(c => c.SaldoPrestamo)
                .Take(5)
                .ToListAsync();

            if (topDeudores.Any())
            {
                var totalDeuda = topDeudores.Sum(c => c.SaldoPrestamo);
                alertas.Add(new AlertaResponse
                {
                    Tipo = "prestamo",
                    Mensaje = $"{topDeudores.Count} clientes con préstamos pendientes",
                    Detalle = $"Deuda total: S/ {totalDeuda:N2}",
                    Prioridad = "normal"
                });
            }

            // Alerta 4: Clientes con préstamos muy altos (más de 1000)
            var deudoresAltos = await _context.ClientesProveedores
                .Where(c => c.SaldoPrestamo >= 1000 && !c.EsAnonimo)
                .CountAsync();

            if (deudoresAltos > 0)
            {
                alertas.Add(new AlertaResponse
                {
                    Tipo = "prestamo",
                    Mensaje = $"{deudoresAltos} cliente(s) con deuda mayor a S/ 1,000",
                    Detalle = "Revisar estado de préstamos altos",
                    Prioridad = "media"
                });
            }

            return alertas;
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error al obtener alertas");
            throw;
        }
    }

    public async Task<EstadisticasGeneralesResponse> ObtenerEstadisticasGeneralesAsync()
    {
        try
        {
            var hace30Dias = DateTime.Today.AddDays(-30);

            var totalProveedores = await _context.ClientesProveedores
                .Where(c => !c.EsAnonimo)
                .CountAsync();

            var proveedoresActivos = await _context.Compras
                .Where(c => c.FechaCompra >= hace30Dias)
                .Select(c => c.ClienteProveedorId)
                .Distinct()
                .CountAsync();

            var totalCompradores = await _context.ClientesCompradores.CountAsync();

            var totalZonas = await _context.Zonas.CountAsync();

            var totalPrestamosVigentes = await _context.ClientesProveedores
                .Where(c => c.SaldoPrestamo > 0)
                .SumAsync(c => c.SaldoPrestamo);

            var clientesConPrestamos = await _context.ClientesProveedores
                .Where(c => c.SaldoPrestamo > 0)
                .CountAsync();

            // Promedios de últimos 30 días
            var comprasUltimos30Dias = await _context.Compras
                .Where(c => c.FechaCompra >= hace30Dias)
                .CountAsync();

            var ventasUltimos30Dias = await _context.Ventas
                .Where(v => v.FechaVenta >= hace30Dias)
                .CountAsync();

            return new EstadisticasGeneralesResponse
            {
                TotalProveedores = totalProveedores,
                ProveedoresActivos = proveedoresActivos,
                TotalCompradores = totalCompradores,
                TotalZonas = totalZonas,
                TotalPrestamosVigentes = totalPrestamosVigentes,
                ClientesConPrestamos = clientesConPrestamos,
                PromedioComprasDiarias = comprasUltimos30Dias / 30m,
                PromedioVentasDiarias = ventasUltimos30Dias / 30m
            };
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error al obtener estadísticas generales");
            throw;
        }
    }

    public async Task<List<DeudorResponse>> ObtenerTopDeudoresAsync(int cantidad = 5)
    {
        try
        {
            var deudores = await _context.ClientesProveedores
                .Include(c => c.Zona)
                .Where(c => c.SaldoPrestamo > 0 && !c.EsAnonimo)
                .OrderByDescending(c => c.SaldoPrestamo)
                .Take(cantidad)
                .ToListAsync();

            var resultado = new List<DeudorResponse>();

            foreach (var deudor in deudores)
            {
                var ultimaCompra = await _context.Compras
                    .Where(c => c.ClienteProveedorId == deudor.Id)
                    .OrderByDescending(c => c.FechaCompra)
                    .Select(c => c.FechaCompra)
                    .FirstOrDefaultAsync();

                var ultimoAbono = await _context.Prestamos
                    .Where(p => p.ClienteProveedorId == deudor.Id && p.TipoMovimiento == TipoMovimiento.Abono)
                    .OrderByDescending(p => p.FechaMovimiento)
                    .Select(p => p.FechaMovimiento)
                    .FirstOrDefaultAsync();

                resultado.Add(new DeudorResponse
                {
                    ClienteId = deudor.Id,
                    DNI = deudor.DNI,
                    NombreCompleto = deudor.NombreCompleto,
                    Zona = deudor.Zona?.Nombre ?? "Sin zona",
                    SaldoPrestamo = deudor.SaldoPrestamo,
                    UltimaCompra = ultimaCompra == default ? null : ultimaCompra,
                    UltimoAbono = ultimoAbono == default ? null : ultimoAbono
                });
            }

            return resultado;
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error al obtener top deudores");
            throw;
        }
    }
}
