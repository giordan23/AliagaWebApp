namespace Backend.DTOs.Responses;

public class ReporteComprasProductoResponse
{
    public string ProductoNombre { get; set; } = string.Empty;
    public int TotalCompras { get; set; }
    public decimal PesoTotalKg { get; set; }
    public decimal MontoTotal { get; set; }
    public decimal PrecioPromedioPorKg { get; set; }
    public decimal PesoPromedioCompra { get; set; }
}
