namespace Backend.DTOs.Responses;

public class ReporteComprasClienteResponse
{
    public string ClienteDNI { get; set; } = string.Empty;
    public string ClienteNombre { get; set; } = string.Empty;
    public string Zona { get; set; } = string.Empty;
    public int TotalCompras { get; set; }
    public decimal PesoTotalKg { get; set; }
    public decimal MontoTotal { get; set; }
    public decimal SaldoPrestamo { get; set; }
    public DateTime? UltimaCompra { get; set; }
}
