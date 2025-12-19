using Backend.Data;
using Backend.Models;
using Backend.Repositories.Interfaces;
using Microsoft.EntityFrameworkCore;

namespace Backend.Repositories.Implementations;

public class VentasRepository : IVentasRepository
{
    private readonly AppDbContext _context;

    public VentasRepository(AppDbContext context)
    {
        _context = context;
    }

    public async Task<Venta?> GetByIdAsync(int id)
    {
        return await _context.Ventas
            .Include(v => v.ClienteComprador)
            .Include(v => v.Producto)
            .Include(v => v.Caja)
            .FirstOrDefaultAsync(v => v.Id == id);
    }

    public async Task<List<Venta>> GetAllAsync(
        int skip = 0,
        int take = 50,
        int? clienteId = null,
        int? productoId = null,
        DateTime? fechaInicio = null,
        DateTime? fechaFin = null)
    {
        var query = _context.Ventas
            .Include(v => v.ClienteComprador)
            .Include(v => v.Producto)
            .AsQueryable();

        if (clienteId.HasValue)
        {
            query = query.Where(v => v.ClienteCompradorId == clienteId.Value);
        }

        if (productoId.HasValue)
        {
            query = query.Where(v => v.ProductoId == productoId.Value);
        }

        if (fechaInicio.HasValue)
        {
            query = query.Where(v => v.FechaVenta.Date >= fechaInicio.Value.Date);
        }

        if (fechaFin.HasValue)
        {
            query = query.Where(v => v.FechaVenta.Date <= fechaFin.Value.Date);
        }

        return await query
            .OrderByDescending(v => v.FechaVenta)
            .Skip(skip)
            .Take(take)
            .ToListAsync();
    }

    public async Task<int> GetTotalCountAsync(
        int? clienteId = null,
        int? productoId = null,
        DateTime? fechaInicio = null,
        DateTime? fechaFin = null)
    {
        var query = _context.Ventas.AsQueryable();

        if (clienteId.HasValue)
        {
            query = query.Where(v => v.ClienteCompradorId == clienteId.Value);
        }

        if (productoId.HasValue)
        {
            query = query.Where(v => v.ProductoId == productoId.Value);
        }

        if (fechaInicio.HasValue)
        {
            query = query.Where(v => v.FechaVenta.Date >= fechaInicio.Value.Date);
        }

        if (fechaFin.HasValue)
        {
            query = query.Where(v => v.FechaVenta.Date <= fechaFin.Value.Date);
        }

        return await query.CountAsync();
    }

    public async Task<List<Venta>> GetByCajaIdAsync(int cajaId)
    {
        return await _context.Ventas
            .Include(v => v.ClienteComprador)
            .Include(v => v.Producto)
            .Where(v => v.CajaId == cajaId)
            .OrderByDescending(v => v.FechaVenta)
            .ToListAsync();
    }

    public async Task<Venta> AddAsync(Venta venta)
    {
        _context.Ventas.Add(venta);
        await _context.SaveChangesAsync();
        return venta;
    }

    public async Task UpdateAsync(Venta venta)
    {
        _context.Ventas.Update(venta);
        await _context.SaveChangesAsync();
    }

    public async Task<bool> EsVentaDelDiaActualAsync(int ventaId)
    {
        var venta = await _context.Ventas.FindAsync(ventaId);
        if (venta == null)
            return false;

        return venta.FechaVenta.Date == DateTime.Today;
    }
}
