using Backend.Models;

namespace Backend.Repositories.Interfaces;

public interface ICajaRepository
{
    Task<Caja?> GetByIdAsync(int id);
    Task<Caja?> GetByFechaAsync(DateTime fecha);
    Task<Caja?> GetCajaAbiertaAsync();
    Task<List<Caja>> GetAllAsync(int skip = 0, int take = 50);
    Task<int> GetTotalCountAsync();
    Task<Caja> AddAsync(Caja caja);
    Task UpdateAsync(Caja caja);
    Task<Caja?> GetUltimaCajaSinCerrarAsync();
    Task<List<MovimientoCaja>> GetMovimientosByCajaIdAsync(int cajaId);
}
