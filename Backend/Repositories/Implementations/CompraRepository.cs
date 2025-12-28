using Backend.Data;
using Backend.Models;
using Backend.Repositories.Interfaces;
using Microsoft.EntityFrameworkCore;

namespace Backend.Repositories.Implementations;

public class CompraRepository : ICompraRepository
{
    private readonly AppDbContext _context;

    public CompraRepository(AppDbContext context)
    {
        _context = context;
    }

    public async Task<Compra?> GetByIdAsync(int id)
    {
        return await _context.Compras
            .Include(c => c.ClienteProveedor)
            .Include(c => c.Detalles)
                .ThenInclude(d => d.Producto)
            .Include(c => c.Caja)
            .FirstOrDefaultAsync(c => c.Id == id);
    }

    public async Task<Compra?> GetByNumeroVoucherAsync(string numeroVoucher)
    {
        return await _context.Compras
            .Include(c => c.ClienteProveedor)
            .Include(c => c.Detalles)
                .ThenInclude(d => d.Producto)
            .FirstOrDefaultAsync(c => c.NumeroVoucher == numeroVoucher);
    }

    public async Task<List<Compra>> GetAllAsync(
        int skip = 0,
        int take = 50,
        int? clienteId = null,
        int? productoId = null,
        DateTime? fechaInicio = null,
        DateTime? fechaFin = null)
    {
        var query = _context.Compras
            .Include(c => c.ClienteProveedor)
            .Include(c => c.Detalles)
                .ThenInclude(d => d.Producto)
            .AsQueryable();

        if (clienteId.HasValue)
        {
            query = query.Where(c => c.ClienteProveedorId == clienteId.Value);
        }

        if (productoId.HasValue)
        {
            query = query.Where(c => c.Detalles.Any(d => d.ProductoId == productoId.Value));
        }

        if (fechaInicio.HasValue)
        {
            query = query.Where(c => c.FechaCompra.Date >= fechaInicio.Value.Date);
        }

        if (fechaFin.HasValue)
        {
            query = query.Where(c => c.FechaCompra.Date <= fechaFin.Value.Date);
        }

        return await query
            .OrderByDescending(c => c.FechaCompra)
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
        var query = _context.Compras.AsQueryable();

        if (clienteId.HasValue)
        {
            query = query.Where(c => c.ClienteProveedorId == clienteId.Value);
        }

        if (productoId.HasValue)
        {
            query = query.Where(c => c.Detalles.Any(d => d.ProductoId == productoId.Value));
        }

        if (fechaInicio.HasValue)
        {
            query = query.Where(c => c.FechaCompra.Date >= fechaInicio.Value.Date);
        }

        if (fechaFin.HasValue)
        {
            query = query.Where(c => c.FechaCompra.Date <= fechaFin.Value.Date);
        }

        return await query.CountAsync();
    }

    public async Task<List<Compra>> GetByCajaIdAsync(int cajaId)
    {
        return await _context.Compras
            .Include(c => c.ClienteProveedor)
            .Include(c => c.Detalles)
                .ThenInclude(d => d.Producto)
            .Where(c => c.CajaId == cajaId)
            .OrderByDescending(c => c.FechaCompra)
            .ToListAsync();
    }

    public async Task<Compra> AddAsync(Compra compra)
    {
        _context.Compras.Add(compra);
        await _context.SaveChangesAsync();
        return compra;
    }

    public async Task UpdateAsync(Compra compra)
    {
        _context.Compras.Update(compra);
        await _context.SaveChangesAsync();
    }

    public async Task<bool> EsCompraDelDiaActualAsync(int compraId)
    {
        var compra = await _context.Compras.FindAsync(compraId);
        if (compra == null)
            return false;

        return compra.FechaCompra.Date == DateTime.Today;
    }

    public async Task<Compra?> GetByIdWithDetailsAsync(int id)
    {
        return await _context.Compras
            .Include(c => c.ClienteProveedor)
            .Include(c => c.Detalles)
                .ThenInclude(d => d.Producto)
            .Include(c => c.Caja)
            .FirstOrDefaultAsync(c => c.Id == id);
    }

    public async Task<bool> PuedeEditarseAsync(int compraId)
    {
        var compra = await _context.Compras.FindAsync(compraId);
        if (compra == null)
            return false;

        // Permitir editar hasta las 23:59 del día siguiente
        var fechaLimite = compra.FechaCompra.Date.AddDays(2);
        return DateTime.Now < fechaLimite;
    }
}
