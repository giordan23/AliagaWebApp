using Backend.Models;

namespace Backend.Repositories.Interfaces;

public interface IVentasRepository
{
    Task<Venta?> GetByIdAsync(int id);
    Task<List<Venta>> GetAllAsync(int skip = 0, int take = 50, int? clienteId = null, int? productoId = null, DateTime? fechaInicio = null, DateTime? fechaFin = null);
    Task<int> GetTotalCountAsync(int? clienteId = null, int? productoId = null, DateTime? fechaInicio = null, DateTime? fechaFin = null);
    Task<List<Venta>> GetByCajaIdAsync(int cajaId);
    Task<Venta> AddAsync(Venta venta);
    Task UpdateAsync(Venta venta);
    Task<bool> EsVentaDelDiaActualAsync(int ventaId);
}
