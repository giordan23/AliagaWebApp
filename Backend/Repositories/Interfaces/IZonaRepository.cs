using Backend.Models;

namespace Backend.Repositories.Interfaces;

public interface IZonaRepository
{
    Task<Zona?> GetByIdAsync(int id);
    Task<Zona?> GetByNombreAsync(string nombre);
    Task<List<Zona>> GetAllAsync(int skip = 0, int take = 50);
    Task<int> GetTotalCountAsync();
    Task<Zona> AddAsync(Zona zona);
    Task UpdateAsync(Zona zona);
    Task<bool> ExisteNombreAsync(string nombre, int? excludeId = null);
}
