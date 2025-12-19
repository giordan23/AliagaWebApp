namespace Backend.DTOs.Responses;

public class ReporteVentasResponse
{
    public DateTime FechaVenta { get; set; }
    public string ClienteNombre { get; set; } = string.Empty;
    public string ProductoNombre { get; set; } = string.Empty;
    public decimal PesoNeto { get; set; }
    public decimal PrecioPorKg { get; set; }
    public decimal MontoTotal { get; set; }
    public bool Editada { get; set; }
}
