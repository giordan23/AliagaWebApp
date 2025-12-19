using Backend.Data;
using Backend.Enums;
using Backend.Models;
using Backend.Repositories.Interfaces;
using Microsoft.EntityFrameworkCore;

namespace Backend.Repositories.Implementations;

public class CajaRepository : ICajaRepository
{
    private readonly AppDbContext _context;

    public CajaRepository(AppDbContext context)
    {
        _context = context;
    }

    public async Task<Caja?> GetByIdAsync(int id)
    {
        return await _context.Cajas.FindAsync(id);
    }

    public async Task<Caja?> GetByFechaAsync(DateTime fecha)
    {
        var fechaSoloFecha = fecha.Date;
        return await _context.Cajas
            .FirstOrDefaultAsync(c => c.Fecha.Date == fechaSoloFecha);
    }

    public async Task<Caja?> GetCajaAbiertaAsync()
    {
        return await _context.Cajas
            .FirstOrDefaultAsync(c => c.Estado == EstadoCaja.Abierta);
    }

    public async Task<List<Caja>> GetAllAsync(int skip = 0, int take = 50)
    {
        return await _context.Cajas
            .OrderByDescending(c => c.Fecha)
            .Skip(skip)
            .Take(take)
            .ToListAsync();
    }

    public async Task<int> GetTotalCountAsync()
    {
        return await _context.Cajas.CountAsync();
    }

    public async Task<Caja> AddAsync(Caja caja)
    {
        _context.Cajas.Add(caja);
        await _context.SaveChangesAsync();
        return caja;
    }

    public async Task UpdateAsync(Caja caja)
    {
        _context.Cajas.Update(caja);
        await _context.SaveChangesAsync();
    }

    public async Task<Caja?> GetUltimaCajaSinCerrarAsync()
    {
        var hoy = DateTime.Today;
        return await _context.Cajas
            .Where(c => c.Estado == EstadoCaja.Abierta && c.Fecha.Date < hoy)
            .OrderByDescending(c => c.Fecha)
            .FirstOrDefaultAsync();
    }

    public async Task<List<MovimientoCaja>> GetMovimientosByCajaIdAsync(int cajaId)
    {
        return await _context.MovimientosCaja
            .Where(m => m.CajaId == cajaId)
            .OrderByDescending(m => m.FechaMovimiento)
            .ToListAsync();
    }
}
