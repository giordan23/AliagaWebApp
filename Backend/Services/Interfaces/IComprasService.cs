using Backend.DTOs.Requests;
using Backend.DTOs.Responses;

namespace Backend.Services.Interfaces;

public interface IComprasService
{
    Task<CompraResponse> RegistrarCompraAsync(RegistrarCompraRequest request);
    Task<CompraResponse> EditarCompraAsync(int compraId, EditarCompraRequest request);
    Task<CompraResponse?> GetByIdAsync(int id);
    Task<CompraResponse?> GetByNumeroVoucherAsync(string numeroVoucher);
    Task<(List<CompraResponse> Compras, int Total)> GetAllAsync(
        int page = 1,
        int pageSize = 50,
        int? clienteId = null,
        int? productoId = null,
        DateTime? fechaInicio = null,
        DateTime? fechaFin = null);
    Task<List<CompraResponse>> GetByCajaIdAsync(int cajaId);
}
