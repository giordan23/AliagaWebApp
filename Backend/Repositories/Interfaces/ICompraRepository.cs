using Backend.Models;

namespace Backend.Repositories.Interfaces;

public interface ICompraRepository
{
    Task<Compra?> GetByIdAsync(int id);
    Task<Compra?> GetByIdWithDetailsAsync(int id);
    Task<Compra?> GetByNumeroVoucherAsync(string numeroVoucher);
    Task<List<Compra>> GetAllAsync(int skip = 0, int take = 50, int? clienteId = null, int? productoId = null, DateTime? fechaInicio = null, DateTime? fechaFin = null);
    Task<int> GetTotalCountAsync(int? clienteId = null, int? productoId = null, DateTime? fechaInicio = null, DateTime? fechaFin = null);
    Task<List<Compra>> GetByCajaIdAsync(int cajaId);
    Task<Compra> AddAsync(Compra compra);
    Task UpdateAsync(Compra compra);
    Task<bool> EsCompraDelDiaActualAsync(int compraId);
    Task<bool> PuedeEditarseAsync(int compraId);
}
