namespace Backend.DTOs.Responses;

public class ProductoResponse
{
    public int Id { get; set; }
    public string Nombre { get; set; } = string.Empty;
    public decimal PrecioSugeridoPorKg { get; set; }
    public List<string> NivelesSecado { get; set; } = new();
    public List<string> Calidades { get; set; } = new();
    public bool PermiteValdeo { get; set; }
    public DateTime FechaModificacion { get; set; }
}
