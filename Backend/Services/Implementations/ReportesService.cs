using Backend.Data;
using Backend.DTOs.Requests;
using Backend.DTOs.Responses;
using Backend.Enums;
using Backend.Services.Interfaces;
using ClosedXML.Excel;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;

namespace Backend.Services.Implementations;

public class ReportesService : IReportesService
{
    private readonly AppDbContext _context;
    private readonly ILogger<ReportesService> _logger;

    public ReportesService(AppDbContext context, ILogger<ReportesService> logger)
    {
        _context = context;
        _logger = logger;
    }

    public async Task<List<ReporteComprasClienteResponse>> GenerarReporteComprasPorClienteAsync(FiltrosReporteRequest filtros)
    {
        try
        {
            var query = _context.Compras
                .Include(c => c.ClienteProveedor)
                    .ThenInclude(cp => cp.Zona)
                .AsQueryable();

            // Aplicar filtros
            if (filtros.FechaInicio.HasValue)
                query = query.Where(c => c.FechaCompra.Date >= filtros.FechaInicio.Value.Date);

            if (filtros.FechaFin.HasValue)
                query = query.Where(c => c.FechaCompra.Date <= filtros.FechaFin.Value.Date);

            if (filtros.ClienteId.HasValue)
                query = query.Where(c => c.ClienteProveedorId == filtros.ClienteId.Value);

            if (filtros.ZonaId.HasValue)
                query = query.Where(c => c.ClienteProveedor.ZonaId == filtros.ZonaId.Value);

            // Agrupar por cliente
            var resultado = await query
                .GroupBy(c => new
                {
                    c.ClienteProveedorId,
                    c.ClienteProveedor.DNI,
                    c.ClienteProveedor.NombreCompleto,
                    ZonaNombre = c.ClienteProveedor.Zona != null ? c.ClienteProveedor.Zona.Nombre : "Sin zona",
                    c.ClienteProveedor.SaldoPrestamo
                })
                .Select(g => new ReporteComprasClienteResponse
                {
                    ClienteDNI = g.Key.DNI,
                    ClienteNombre = g.Key.NombreCompleto,
                    Zona = g.Key.ZonaNombre,
                    TotalCompras = g.Count(),
                    PesoTotalKg = g.Sum(c => c.PesoTotal),
                    MontoTotal = g.Sum(c => c.MontoTotal),
                    SaldoPrestamo = g.Key.SaldoPrestamo,
                    UltimaCompra = g.Max(c => c.FechaCompra)
                })
                .OrderByDescending(r => r.MontoTotal)
                .ToListAsync();

            return resultado;
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error al generar reporte de compras por cliente");
            throw;
        }
    }

    public async Task<List<ReporteComprasProductoResponse>> GenerarReporteComprasPorProductoAsync(FiltrosReporteRequest filtros)
    {
        try
        {
            // Ahora trabajamos con DetallesCompra en lugar de Compras directamente
            var query = _context.DetallesCompra
                .Include(d => d.Producto)
                .Include(d => d.Compra)
                .AsQueryable();

            // Aplicar filtros de fecha
            if (filtros.FechaInicio.HasValue)
                query = query.Where(d => d.Compra.FechaCompra.Date >= filtros.FechaInicio.Value.Date);

            if (filtros.FechaFin.HasValue)
                query = query.Where(d => d.Compra.FechaCompra.Date <= filtros.FechaFin.Value.Date);

            if (filtros.ProductoId.HasValue)
                query = query.Where(d => d.ProductoId == filtros.ProductoId.Value);

            // Agrupar por producto
            var resultado = await query
                .GroupBy(d => new
                {
                    d.ProductoId,
                    d.Producto.Nombre
                })
                .Select(g => new ReporteComprasProductoResponse
                {
                    ProductoNombre = g.Key.Nombre,
                    TotalCompras = g.Select(d => d.CompraId).Distinct().Count(), // Contar compras únicas
                    PesoTotalKg = g.Sum(d => d.PesoNeto),
                    MontoTotal = g.Sum(d => d.Subtotal),
                    PrecioPromedioPorKg = g.Average(d => d.PrecioPorKg),
                    PesoPromedioCompra = g.Average(d => d.PesoNeto)
                })
                .OrderByDescending(r => r.PesoTotalKg)
                .ToListAsync();

            return resultado;
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error al generar reporte de compras por producto");
            throw;
        }
    }

    public async Task<List<ReporteZonasResponse>> GenerarReporteResumenPorZonasAsync(FiltrosReporteRequest filtros)
    {
        try
        {
            var query = _context.Compras
                .Include(c => c.ClienteProveedor)
                    .ThenInclude(cp => cp.Zona)
                .AsQueryable();

            // Aplicar filtros
            if (filtros.FechaInicio.HasValue)
                query = query.Where(c => c.FechaCompra.Date >= filtros.FechaInicio.Value.Date);

            if (filtros.FechaFin.HasValue)
                query = query.Where(c => c.FechaCompra.Date <= filtros.FechaFin.Value.Date);

            if (filtros.ZonaId.HasValue)
                query = query.Where(c => c.ClienteProveedor.ZonaId == filtros.ZonaId.Value);

            // Agrupar por zona
            var resultado = await query
                .Where(c => c.ClienteProveedor.Zona != null)
                .GroupBy(c => new
                {
                    ZonaId = c.ClienteProveedor.ZonaId,
                    ZonaNombre = c.ClienteProveedor.Zona!.Nombre
                })
                .Select(g => new
                {
                    g.Key.ZonaNombre,
                    TotalCompras = g.Count(),
                    PesoTotalKg = g.Sum(c => c.PesoTotal),
                    MontoTotal = g.Sum(c => c.MontoTotal),
                    ProveedoresIds = g.Select(c => c.ClienteProveedorId).Distinct()
                })
                .ToListAsync();

            var reporteZonas = resultado.Select(r => new ReporteZonasResponse
            {
                ZonaNombre = r.ZonaNombre,
                TotalProveedores = r.ProveedoresIds.Count(),
                TotalCompras = r.TotalCompras,
                PesoTotalKg = r.PesoTotalKg,
                MontoTotal = r.MontoTotal,
                PromedioComprasPorProveedor = r.ProveedoresIds.Count() > 0
                    ? (decimal)r.TotalCompras / r.ProveedoresIds.Count()
                    : 0
            })
            .OrderByDescending(r => r.MontoTotal)
            .ToList();

            return reporteZonas;
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error al generar reporte de resumen por zonas");
            throw;
        }
    }

    public async Task<List<ReporteMovimientosCajaResponse>> GenerarReporteMovimientosCajaAsync(FiltrosReporteRequest filtros)
    {
        try
        {
            var query = _context.MovimientosCaja
                .Include(m => m.Caja)
                .AsQueryable();

            // Aplicar filtros
            if (filtros.FechaInicio.HasValue)
                query = query.Where(m => m.FechaMovimiento.Date >= filtros.FechaInicio.Value.Date);

            if (filtros.FechaFin.HasValue)
                query = query.Where(m => m.FechaMovimiento.Date <= filtros.FechaFin.Value.Date);

            if (filtros.CajaId.HasValue)
                query = query.Where(m => m.CajaId == filtros.CajaId.Value);

            var resultado = await query
                .OrderByDescending(m => m.FechaMovimiento)
                .Select(m => new ReporteMovimientosCajaResponse
                {
                    Fecha = m.FechaMovimiento,
                    CajaId = m.CajaId,
                    TipoMovimiento = m.TipoMovimiento,
                    Concepto = m.Concepto,
                    Monto = m.Monto,
                    TipoOperacion = m.TipoOperacion,
                    EsAjustePosterior = m.EsAjustePosterior
                })
                .ToListAsync();

            return resultado;
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error al generar reporte de movimientos de caja");
            throw;
        }
    }

    public async Task<List<ReporteVentasResponse>> GenerarReporteVentasAsync(FiltrosReporteRequest filtros)
    {
        try
        {
            var query = _context.Ventas
                .Include(v => v.ClienteComprador)
                .Include(v => v.Producto)
                .AsQueryable();

            // Aplicar filtros
            if (filtros.FechaInicio.HasValue)
                query = query.Where(v => v.FechaVenta.Date >= filtros.FechaInicio.Value.Date);

            if (filtros.FechaFin.HasValue)
                query = query.Where(v => v.FechaVenta.Date <= filtros.FechaFin.Value.Date);

            if (filtros.ClienteId.HasValue)
                query = query.Where(v => v.ClienteCompradorId == filtros.ClienteId.Value);

            if (filtros.ProductoId.HasValue)
                query = query.Where(v => v.ProductoId == filtros.ProductoId.Value);

            var resultado = await query
                .OrderByDescending(v => v.FechaVenta)
                .Select(v => new ReporteVentasResponse
                {
                    FechaVenta = v.FechaVenta,
                    ClienteNombre = v.ClienteComprador.Nombre,
                    ProductoNombre = v.Producto.Nombre,
                    PesoNeto = v.PesoNeto,
                    PrecioPorKg = v.PrecioPorKg,
                    MontoTotal = v.MontoTotal,
                    Editada = v.Editada
                })
                .ToListAsync();

            return resultado;
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error al generar reporte de ventas");
            throw;
        }
    }

    public async Task<byte[]> ExportarReporteAExcelAsync<T>(List<T> datos, string nombreHoja) where T : class
    {
        try
        {
            using var workbook = new XLWorkbook();
            var worksheet = workbook.Worksheets.Add(nombreHoja);

            // Obtener las propiedades del tipo T
            var propiedades = typeof(T).GetProperties();

            // Escribir encabezados
            for (int i = 0; i < propiedades.Length; i++)
            {
                worksheet.Cell(1, i + 1).Value = propiedades[i].Name;
                worksheet.Cell(1, i + 1).Style.Font.Bold = true;
                worksheet.Cell(1, i + 1).Style.Fill.BackgroundColor = XLColor.LightGray;
            }

            // Escribir datos
            for (int fila = 0; fila < datos.Count; fila++)
            {
                for (int col = 0; col < propiedades.Length; col++)
                {
                    var valor = propiedades[col].GetValue(datos[fila]);

                    if (valor != null)
                    {
                        var cell = worksheet.Cell(fila + 2, col + 1);

                        // Formatear según el tipo de dato
                        if (valor is DateTime fecha)
                        {
                            cell.Value = fecha;
                            cell.Style.DateFormat.Format = "dd/MM/yyyy HH:mm";
                        }
                        else if (valor is decimal || valor is double || valor is float)
                        {
                            cell.Value = Convert.ToDouble(valor);
                            cell.Style.NumberFormat.Format = "#,##0.00";
                        }
                        else if (valor is int || valor is long)
                        {
                            cell.Value = Convert.ToInt64(valor);
                            cell.Style.NumberFormat.Format = "#,##0";
                        }
                        else
                        {
                            cell.Value = valor.ToString();
                        }
                    }
                }
            }

            // Ajustar ancho de columnas
            worksheet.Columns().AdjustToContents();

            // Aplicar bordes a toda la tabla
            var rango = worksheet.Range(1, 1, datos.Count + 1, propiedades.Length);
            rango.Style.Border.OutsideBorder = XLBorderStyleValues.Thin;
            rango.Style.Border.InsideBorder = XLBorderStyleValues.Thin;

            // Convertir a bytes
            using var stream = new MemoryStream();
            workbook.SaveAs(stream);
            return await Task.FromResult(stream.ToArray());
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error al exportar reporte a Excel");
            throw;
        }
    }
}
