namespace Backend.DTOs.Responses;

public class ReporteZonasResponse
{
    public string ZonaNombre { get; set; } = string.Empty;
    public int TotalProveedores { get; set; }
    public int TotalCompras { get; set; }
    public decimal PesoTotalKg { get; set; }
    public decimal MontoTotal { get; set; }
    public decimal PromedioComprasPorProveedor { get; set; }
}
