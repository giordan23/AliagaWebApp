using Backend.Enums;

namespace Backend.DTOs.Responses;

public class DetalleCompraResponse
{
    public int Id { get; set; }
    public int ProductoId { get; set; }
    public string ProductoNombre { get; set; } = string.Empty;
    public string NivelSecado { get; set; } = string.Empty;
    public string Calidad { get; set; } = string.Empty;
    public TipoPesado TipoPesado { get; set; }
    public decimal PesoBruto { get; set; }
    public decimal DescuentoKg { get; set; }
    public decimal PesoNeto { get; set; }
    public decimal PrecioPorKg { get; set; }
    public decimal Subtotal { get; set; }
}
