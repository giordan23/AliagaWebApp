using Backend.Data;
using Backend.Models;
using Backend.Repositories.Interfaces;
using Microsoft.EntityFrameworkCore;

namespace Backend.Repositories.Implementations;

public class ZonaRepository : IZonaRepository
{
    private readonly AppDbContext _context;

    public ZonaRepository(AppDbContext context)
    {
        _context = context;
    }

    public async Task<Zona?> GetByIdAsync(int id)
    {
        return await _context.Zonas
            .Include(z => z.Clientes)
            .FirstOrDefaultAsync(z => z.Id == id);
    }

    public async Task<Zona?> GetByNombreAsync(string nombre)
    {
        return await _context.Zonas
            .FirstOrDefaultAsync(z => z.Nombre.ToLower() == nombre.ToLower());
    }

    public async Task<List<Zona>> GetAllAsync(int skip = 0, int take = 50)
    {
        return await _context.Zonas
            .Include(z => z.Clientes)
            .OrderBy(z => z.Nombre)
            .Skip(skip)
            .Take(take)
            .ToListAsync();
    }

    public async Task<int> GetTotalCountAsync()
    {
        return await _context.Zonas.CountAsync();
    }

    public async Task<Zona> AddAsync(Zona zona)
    {
        _context.Zonas.Add(zona);
        await _context.SaveChangesAsync();
        return zona;
    }

    public async Task UpdateAsync(Zona zona)
    {
        zona.FechaModificacion = DateTime.Now;
        _context.Zonas.Update(zona);
        await _context.SaveChangesAsync();
    }

    public async Task<bool> ExisteNombreAsync(string nombre, int? excludeId = null)
    {
        var query = _context.Zonas
            .Where(z => z.Nombre.ToLower() == nombre.ToLower());

        if (excludeId.HasValue)
        {
            query = query.Where(z => z.Id != excludeId.Value);
        }

        return await query.AnyAsync();
    }
}
