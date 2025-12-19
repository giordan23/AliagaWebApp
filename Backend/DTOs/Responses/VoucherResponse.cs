namespace Backend.DTOs.Responses;

public class VoucherResponse
{
    public string NumeroVoucher { get; set; } = string.Empty;
    public string ContenidoVoucher { get; set; } = string.Empty;
    public bool ImpresionExitosa { get; set; }
    public string? MensajeError { get; set; }
}
