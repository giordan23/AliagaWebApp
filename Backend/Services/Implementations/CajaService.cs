using Backend.Data;
using Backend.DTOs.Requests;
using Backend.DTOs.Responses;
using Backend.Enums;
using Backend.Helpers;
using Backend.Models;
using Backend.Repositories.Interfaces;
using Backend.Services.Interfaces;
using Microsoft.EntityFrameworkCore;

namespace Backend.Services.Implementations;

public class CajaService : ICajaService
{
    private readonly ICajaRepository _cajaRepository;
    private readonly AppDbContext _context;

    public CajaService(ICajaRepository cajaRepository, AppDbContext context)
    {
        _cajaRepository = cajaRepository;
        _context = context;
    }

    public async Task<CajaResumenResponse?> AbrirCajaAsync(AbrirCajaRequest request)
    {
        var hoy = DateTime.Today;

        // Validar que no exista caja abierta del día actual
        var cajaHoy = await _cajaRepository.GetByFechaAsync(hoy);
        if (cajaHoy != null)
        {
            throw new InvalidOperationException("Ya existe una caja abierta para el día de hoy");
        }

        // Cerrar automáticamente cajas anteriores sin cerrar
        var cajaSinCerrar = await _cajaRepository.GetUltimaCajaSinCerrarAsync();
        if (cajaSinCerrar != null)
        {
            await CerrarCajaAutomaticamenteAsync(cajaSinCerrar);
        }

        // Crear nueva caja
        var nuevaCaja = new Caja
        {
            Fecha = hoy,
            MontoInicial = request.MontoInicial,
            MontoEsperado = request.MontoInicial,
            Estado = EstadoCaja.Abierta,
            FechaApertura = DateTime.Now,
            UsuarioApertura = "Sistema",
            Diferencia = 0
        };

        var cajaCreada = await _cajaRepository.AddAsync(nuevaCaja);
        return await MapToCajaResumenAsync(cajaCreada);
    }

    public async Task<CajaResumenResponse?> CerrarCajaAsync(CerrarCajaRequest request)
    {
        var cajaAbierta = await _cajaRepository.GetCajaAbiertaAsync();
        if (cajaAbierta == null)
        {
            throw new InvalidOperationException("No hay caja abierta para cerrar");
        }

        // Solo se puede cerrar caja del día actual
        if (cajaAbierta.Fecha.Date != DateTime.Today)
        {
            throw new InvalidOperationException("Solo se puede cerrar la caja del día actual");
        }

        // Calcular saldo esperado
        var totales = await CalcularTotalesCajaAsync(cajaAbierta.Id);
        cajaAbierta.MontoEsperado = CalculosHelper.CalcularSaldoEsperado(
            cajaAbierta.MontoInicial,
            totales.TotalIngresos,
            totales.TotalEgresos);

        // Registrar arqueo y diferencia
        cajaAbierta.ArqueoReal = request.ArqueoReal;
        cajaAbierta.Diferencia = CalculosHelper.CalcularDiferencia(
            request.ArqueoReal,
            cajaAbierta.MontoEsperado);

        // Cerrar caja
        cajaAbierta.Estado = EstadoCaja.CerradaManual;
        cajaAbierta.FechaCierre = DateTime.Now;
        cajaAbierta.UsuarioCierre = "Sistema";

        await _cajaRepository.UpdateAsync(cajaAbierta);
        return await MapToCajaResumenAsync(cajaAbierta);
    }

    public async Task<CajaResumenResponse?> ReabrirCajaAsync(int cajaId)
    {
        var caja = await _cajaRepository.GetByIdAsync(cajaId);
        if (caja == null)
        {
            throw new InvalidOperationException("Caja no encontrada");
        }

        // Solo se puede reabrir caja del mismo día
        if (caja.Fecha.Date != DateTime.Today)
        {
            throw new InvalidOperationException("Solo se puede reabrir la caja del día actual");
        }

        // Verificar que esté cerrada
        if (caja.Estado == EstadoCaja.Abierta)
        {
            throw new InvalidOperationException("La caja ya está abierta");
        }

        // Reabrir
        caja.Estado = EstadoCaja.Abierta;
        caja.FechaCierre = null;
        caja.UsuarioCierre = null;
        caja.ArqueoReal = null;
        caja.Diferencia = 0;

        await _cajaRepository.UpdateAsync(caja);
        return await MapToCajaResumenAsync(caja);
    }

    public async Task<CajaResumenResponse?> GetCajaActualAsync()
    {
        var caja = await _cajaRepository.GetCajaAbiertaAsync();
        if (caja == null)
            return null;

        return await MapToCajaResumenAsync(caja);
    }

    public async Task<CajaDetalleResponse?> GetCajaDetalleAsync(int cajaId)
    {
        var caja = await _cajaRepository.GetByIdAsync(cajaId);
        if (caja == null)
            return null;

        var totales = await CalcularTotalesCajaAsync(cajaId);
        var movimientos = await _cajaRepository.GetMovimientosByCajaIdAsync(cajaId);

        return new CajaDetalleResponse
        {
            Id = caja.Id,
            Fecha = caja.Fecha,
            MontoInicial = caja.MontoInicial,
            MontoEsperado = caja.MontoEsperado,
            ArqueoReal = caja.ArqueoReal,
            Diferencia = caja.Diferencia,
            Estado = caja.Estado,
            FechaApertura = caja.FechaApertura,
            FechaCierre = caja.FechaCierre,
            TotalCompras = totales.TotalCompras,
            TotalVentas = totales.TotalVentas,
            TotalPrestamos = totales.TotalPrestamos,
            TotalAbonos = totales.TotalAbonos,
            TotalInyecciones = totales.TotalInyecciones,
            TotalRetiros = totales.TotalRetiros,
            TotalGastos = totales.TotalGastos,
            NumeroCompras = totales.NumeroCompras,
            NumeroVentas = totales.NumeroVentas,
            Movimientos = movimientos.Select(m => new MovimientoCajaResponse
            {
                Id = m.Id,
                TipoMovimiento = m.TipoMovimiento,
                ReferenciaId = m.ReferenciaId,
                Concepto = m.Concepto,
                Monto = m.Monto,
                TipoOperacion = m.TipoOperacion,
                FechaMovimiento = m.FechaMovimiento,
                EsAjustePosterior = m.EsAjustePosterior
            }).ToList()
        };
    }

    public async Task<List<CajaResumenResponse>> GetHistorialCajasAsync(int skip = 0, int take = 50)
    {
        var cajas = await _cajaRepository.GetAllAsync(skip, take);
        var result = new List<CajaResumenResponse>();

        foreach (var caja in cajas)
        {
            result.Add(await MapToCajaResumenAsync(caja));
        }

        return result;
    }

    public async Task<int> GetTotalCajasCountAsync()
    {
        return await _cajaRepository.GetTotalCountAsync();
    }

    public async Task<MovimientoCajaResponse?> RegistrarMovimientoAsync(RegistrarMovimientoCajaRequest request)
    {
        var cajaAbierta = await _cajaRepository.GetCajaAbiertaAsync();
        if (cajaAbierta == null)
        {
            throw new InvalidOperationException("Debe abrir la caja del día para realizar esta operación");
        }

        // Validar retiros no superen saldo disponible
        if (request.TipoMovimiento == TipoMovimiento.Retiro)
        {
            var totales = await CalcularTotalesCajaAsync(cajaAbierta.Id);
            var saldoDisponible = CalculosHelper.CalcularSaldoEsperado(
                cajaAbierta.MontoInicial,
                totales.TotalIngresos,
                totales.TotalEgresos);

            if (request.Monto > saldoDisponible)
            {
                throw new InvalidOperationException("El monto a retirar supera el saldo disponible en caja");
            }
        }

        // Determinar tipo de operación
        var tipoOperacion = request.TipoMovimiento switch
        {
            TipoMovimiento.Inyeccion => TipoOperacion.Ingreso,
            TipoMovimiento.Retiro => TipoOperacion.Egreso,
            TipoMovimiento.GastoOperativo => TipoOperacion.Egreso,
            _ => throw new InvalidOperationException("Tipo de movimiento no válido")
        };

        var movimiento = new MovimientoCaja
        {
            CajaId = cajaAbierta.Id,
            TipoMovimiento = request.TipoMovimiento,
            Concepto = request.Descripcion ?? GetConceptoDefault(request.TipoMovimiento),
            Monto = request.Monto,
            TipoOperacion = tipoOperacion,
            FechaMovimiento = DateTime.Now,
            EsAjustePosterior = false
        };

        _context.MovimientosCaja.Add(movimiento);
        await _context.SaveChangesAsync();

        return new MovimientoCajaResponse
        {
            Id = movimiento.Id,
            TipoMovimiento = movimiento.TipoMovimiento,
            ReferenciaId = movimiento.ReferenciaId,
            Concepto = movimiento.Concepto,
            Monto = movimiento.Monto,
            TipoOperacion = movimiento.TipoOperacion,
            FechaMovimiento = movimiento.FechaMovimiento,
            EsAjustePosterior = movimiento.EsAjustePosterior
        };
    }

    public async Task<bool> ExisteCajaAbiertaAsync()
    {
        var caja = await _cajaRepository.GetCajaAbiertaAsync();
        return caja != null;
    }

    // Métodos privados
    private async Task CerrarCajaAutomaticamenteAsync(Caja caja)
    {
        var totales = await CalcularTotalesCajaAsync(caja.Id);
        caja.MontoEsperado = CalculosHelper.CalcularSaldoEsperado(
            caja.MontoInicial,
            totales.TotalIngresos,
            totales.TotalEgresos);

        caja.ArqueoReal = caja.MontoEsperado;
        caja.Diferencia = 0;
        caja.Estado = EstadoCaja.CerradaAutomatica;
        caja.FechaCierre = DateTime.Now;
        caja.UsuarioCierre = "Sistema Automático";

        await _cajaRepository.UpdateAsync(caja);
    }

    private async Task<(decimal TotalIngresos, decimal TotalEgresos, decimal TotalCompras, decimal TotalVentas,
        decimal TotalPrestamos, decimal TotalAbonos, decimal TotalInyecciones, decimal TotalRetiros,
        decimal TotalGastos, int NumeroCompras, int NumeroVentas)> CalcularTotalesCajaAsync(int cajaId)
    {
        var compras = await _context.Compras.Where(c => c.CajaId == cajaId).ToListAsync();
        var ventas = await _context.Ventas.Where(v => v.CajaId == cajaId).ToListAsync();
        var prestamos = await _context.Prestamos
            .Where(p => p.CajaId == cajaId && p.TipoMovimiento == TipoMovimiento.Prestamo)
            .ToListAsync();
        var abonos = await _context.Prestamos
            .Where(p => p.CajaId == cajaId && p.TipoMovimiento == TipoMovimiento.Abono)
            .ToListAsync();
        var movimientos = await _context.MovimientosCaja.Where(m => m.CajaId == cajaId).ToListAsync();

        var totalCompras = compras.Sum(c => c.MontoTotal);
        var totalVentas = ventas.Sum(v => v.MontoTotal);
        var totalPrestamos = prestamos.Sum(p => p.Monto);
        var totalAbonos = abonos.Sum(a => a.Monto);
        var totalInyecciones = movimientos
            .Where(m => m.TipoMovimiento == TipoMovimiento.Inyeccion)
            .Sum(m => m.Monto);
        var totalRetiros = movimientos
            .Where(m => m.TipoMovimiento == TipoMovimiento.Retiro)
            .Sum(m => m.Monto);
        var totalGastos = movimientos
            .Where(m => m.TipoMovimiento == TipoMovimiento.GastoOperativo)
            .Sum(m => m.Monto);

        var totalIngresos = totalCompras + totalAbonos + totalInyecciones;
        var totalEgresos = totalVentas + totalPrestamos + totalRetiros + totalGastos;

        return (totalIngresos, totalEgresos, totalCompras, totalVentas, totalPrestamos,
            totalAbonos, totalInyecciones, totalRetiros, totalGastos, compras.Count, ventas.Count);
    }

    private async Task<CajaResumenResponse> MapToCajaResumenAsync(Caja caja)
    {
        var totales = await CalcularTotalesCajaAsync(caja.Id);

        return new CajaResumenResponse
        {
            Id = caja.Id,
            Fecha = caja.Fecha,
            MontoInicial = caja.MontoInicial,
            MontoEsperado = caja.MontoEsperado,
            ArqueoReal = caja.ArqueoReal,
            Diferencia = caja.Diferencia,
            Estado = caja.Estado,
            FechaApertura = caja.FechaApertura,
            FechaCierre = caja.FechaCierre,
            UsuarioApertura = caja.UsuarioApertura,
            UsuarioCierre = caja.UsuarioCierre,
            TotalIngresos = totales.TotalIngresos,
            TotalEgresos = totales.TotalEgresos,
            SaldoActual = CalculosHelper.CalcularSaldoEsperado(
                caja.MontoInicial,
                totales.TotalIngresos,
                totales.TotalEgresos)
        };
    }

    private string GetConceptoDefault(TipoMovimiento tipo)
    {
        return tipo switch
        {
            TipoMovimiento.Inyeccion => "Inyección de dinero",
            TipoMovimiento.Retiro => "Retiro de dinero",
            TipoMovimiento.GastoOperativo => "Gasto operativo",
            _ => "Movimiento de caja"
        };
    }
}
