using Backend.DTOs.Responses;
using Backend.Models;

namespace Backend.Services.Interfaces;

public interface IVoucherService
{
    Task<VoucherResponse> GenerarYImprimirVoucherAsync(Compra compra, bool esDuplicado = false);
    Task<VoucherResponse> ReimprimirVoucherAsync(int compraId);
    string GenerarContenidoVoucher(Compra compra, bool esDuplicado = false);
}
