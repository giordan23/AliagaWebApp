using Backend.Data;
using Backend.Models;
using Backend.Repositories.Interfaces;
using Microsoft.EntityFrameworkCore;

namespace Backend.Repositories.Implementations;

public class PrestamosRepository : IPrestamosRepository
{
    private readonly AppDbContext _context;

    public PrestamosRepository(AppDbContext context)
    {
        _context = context;
    }

    public async Task<Prestamo?> GetByIdAsync(int id)
    {
        return await _context.Prestamos
            .Include(p => p.ClienteProveedor)
            .Include(p => p.Caja)
            .FirstOrDefaultAsync(p => p.Id == id);
    }

    public async Task<List<Prestamo>> GetAllAsync(
        int skip = 0,
        int take = 50,
        int? clienteId = null,
        DateTime? fechaInicio = null,
        DateTime? fechaFin = null)
    {
        var query = _context.Prestamos
            .Include(p => p.ClienteProveedor)
            .AsQueryable();

        if (clienteId.HasValue)
        {
            query = query.Where(p => p.ClienteProveedorId == clienteId.Value);
        }

        if (fechaInicio.HasValue)
        {
            query = query.Where(p => p.FechaMovimiento.Date >= fechaInicio.Value.Date);
        }

        if (fechaFin.HasValue)
        {
            query = query.Where(p => p.FechaMovimiento.Date <= fechaFin.Value.Date);
        }

        return await query
            .OrderByDescending(p => p.FechaMovimiento)
            .Skip(skip)
            .Take(take)
            .ToListAsync();
    }

    public async Task<int> GetTotalCountAsync(
        int? clienteId = null,
        DateTime? fechaInicio = null,
        DateTime? fechaFin = null)
    {
        var query = _context.Prestamos.AsQueryable();

        if (clienteId.HasValue)
        {
            query = query.Where(p => p.ClienteProveedorId == clienteId.Value);
        }

        if (fechaInicio.HasValue)
        {
            query = query.Where(p => p.FechaMovimiento.Date >= fechaInicio.Value.Date);
        }

        if (fechaFin.HasValue)
        {
            query = query.Where(p => p.FechaMovimiento.Date <= fechaFin.Value.Date);
        }

        return await query.CountAsync();
    }

    public async Task<List<Prestamo>> GetByClienteIdAsync(int clienteId)
    {
        return await _context.Prestamos
            .Include(p => p.ClienteProveedor)
            .Where(p => p.ClienteProveedorId == clienteId)
            .OrderByDescending(p => p.FechaMovimiento)
            .ToListAsync();
    }

    public async Task<List<Prestamo>> GetByCajaIdAsync(int cajaId)
    {
        return await _context.Prestamos
            .Include(p => p.ClienteProveedor)
            .Where(p => p.CajaId == cajaId)
            .OrderByDescending(p => p.FechaMovimiento)
            .ToListAsync();
    }

    public async Task<Prestamo> AddAsync(Prestamo prestamo)
    {
        _context.Prestamos.Add(prestamo);
        await _context.SaveChangesAsync();
        return prestamo;
    }
}
