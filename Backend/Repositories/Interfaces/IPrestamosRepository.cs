using Backend.Models;

namespace Backend.Repositories.Interfaces;

public interface IPrestamosRepository
{
    Task<Prestamo?> GetByIdAsync(int id);
    Task<List<Prestamo>> GetAllAsync(int skip = 0, int take = 50, int? clienteId = null, DateTime? fechaInicio = null, DateTime? fechaFin = null);
    Task<int> GetTotalCountAsync(int? clienteId = null, DateTime? fechaInicio = null, DateTime? fechaFin = null);
    Task<List<Prestamo>> GetByClienteIdAsync(int clienteId);
    Task<List<Prestamo>> GetByCajaIdAsync(int cajaId);
    Task<Prestamo> AddAsync(Prestamo prestamo);
}
