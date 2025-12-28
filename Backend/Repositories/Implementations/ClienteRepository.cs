using Backend.Data;
using Backend.Models;
using Backend.Repositories.Interfaces;
using Microsoft.EntityFrameworkCore;

namespace Backend.Repositories.Implementations;

public class ClienteRepository : IClienteRepository
{
    private readonly AppDbContext _context;

    public ClienteRepository(AppDbContext context)
    {
        _context = context;
    }

    // Clientes Proveedores
    public async Task<ClienteProveedor?> GetProveedorByIdAsync(int id)
    {
        return await _context.ClientesProveedores
            .Include(c => c.Zona)
            .FirstOrDefaultAsync(c => c.Id == id && !c.Eliminado);
    }

    public async Task<ClienteProveedor?> GetProveedorByDniAsync(string dni)
    {
        return await _context.ClientesProveedores
            .Include(c => c.Zona)
            .FirstOrDefaultAsync(c => c.DNI == dni && !c.Eliminado);
    }

    public async Task<List<ClienteProveedor>> GetProveedoresAsync(int skip = 0, int take = 50, string? searchTerm = null, int? zonaId = null)
    {
        var query = _context.ClientesProveedores
            .Include(c => c.Zona)
            .Where(c => !c.Eliminado)
            .AsQueryable();

        if (!string.IsNullOrWhiteSpace(searchTerm))
        {
            var searchTermUpper = searchTerm.ToUpper();
            query = query.Where(c =>
                c.DNI.ToUpper().Contains(searchTermUpper) ||
                c.NombreCompleto.ToUpper().Contains(searchTermUpper));
        }

        if (zonaId.HasValue)
        {
            query = query.Where(c => c.ZonaId == zonaId.Value);
        }

        return await query
            .OrderByDescending(c => c.FechaCreacion)
            .Skip(skip)
            .Take(take)
            .ToListAsync();
    }

    public async Task<int> GetTotalProveedoresCountAsync(string? searchTerm = null, int? zonaId = null)
    {
        var query = _context.ClientesProveedores
            .Where(c => !c.Eliminado)
            .AsQueryable();

        if (!string.IsNullOrWhiteSpace(searchTerm))
        {
            var searchTermUpper = searchTerm.ToUpper();
            query = query.Where(c =>
                c.DNI.ToUpper().Contains(searchTermUpper) ||
                c.NombreCompleto.ToUpper().Contains(searchTermUpper));
        }

        if (zonaId.HasValue)
        {
            query = query.Where(c => c.ZonaId == zonaId.Value);
        }

        return await query.CountAsync();
    }

    public async Task<ClienteProveedor> AddProveedorAsync(ClienteProveedor cliente)
    {
        _context.ClientesProveedores.Add(cliente);
        await _context.SaveChangesAsync();
        return cliente;
    }

    public async Task UpdateProveedorAsync(ClienteProveedor cliente)
    {
        cliente.FechaModificacion = DateTime.Now;
        _context.ClientesProveedores.Update(cliente);
        await _context.SaveChangesAsync();
    }

    public async Task DeleteProveedorAsync(int id)
    {
        var cliente = await _context.ClientesProveedores.FindAsync(id);
        if (cliente != null && !cliente.EsAnonimo)
        {
            cliente.Eliminado = true;
            cliente.FechaModificacion = DateTime.Now;
            _context.ClientesProveedores.Update(cliente);
            await _context.SaveChangesAsync();
        }
    }

    public async Task<bool> ExisteDniAsync(string dni, int? excludeId = null)
    {
        var query = _context.ClientesProveedores
            .Where(c => c.DNI == dni && !c.Eliminado);

        if (excludeId.HasValue)
        {
            query = query.Where(c => c.Id != excludeId.Value);
        }

        return await query.AnyAsync();
    }

    public async Task<Dictionary<int, decimal>> GetTotalKgVendidosPorProveedorAsync()
    {
        return await _context.Compras
            .GroupBy(c => c.ClienteProveedorId)
            .Select(g => new { ProveedorId = g.Key, TotalKg = g.Sum(c => c.PesoTotal) })
            .ToDictionaryAsync(x => x.ProveedorId, x => x.TotalKg);
    }

    // Clientes Compradores
    public async Task<ClienteComprador?> GetCompradorByIdAsync(int id)
    {
        return await _context.ClientesCompradores
            .FirstOrDefaultAsync(c => c.Id == id && !c.Eliminado);
    }

    public async Task<List<ClienteComprador>> GetCompradoresAsync()
    {
        return await _context.ClientesCompradores
            .Where(c => !c.Eliminado)
            .OrderBy(c => c.Nombre)
            .ToListAsync();
    }

    public async Task<ClienteComprador> AddCompradorAsync(ClienteComprador cliente)
    {
        _context.ClientesCompradores.Add(cliente);
        await _context.SaveChangesAsync();
        return cliente;
    }

    public async Task UpdateCompradorAsync(ClienteComprador cliente)
    {
        cliente.FechaModificacion = DateTime.Now;
        _context.ClientesCompradores.Update(cliente);
        await _context.SaveChangesAsync();
    }

    public async Task DeleteCompradorAsync(int id)
    {
        var cliente = await _context.ClientesCompradores.FindAsync(id);
        if (cliente != null)
        {
            cliente.Eliminado = true;
            cliente.FechaModificacion = DateTime.Now;
            _context.ClientesCompradores.Update(cliente);
            await _context.SaveChangesAsync();
        }
    }
}
