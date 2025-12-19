namespace Backend.DTOs.Responses;

public class VentaResponse
{
    public int Id { get; set; }
    public int ClienteCompradorId { get; set; }
    public string ClienteNombre { get; set; } = string.Empty;
    public string ClienteRUC { get; set; } = string.Empty;
    public int ProductoId { get; set; }
    public string ProductoNombre { get; set; } = string.Empty;
    public int CajaId { get; set; }
    public decimal PesoNeto { get; set; }
    public decimal PrecioPorKg { get; set; }
    public decimal MontoTotal { get; set; }
    public DateTime FechaVenta { get; set; }
    public bool Editada { get; set; }
    public DateTime? FechaEdicion { get; set; }
    public bool EsAjustePosterior { get; set; }
}
