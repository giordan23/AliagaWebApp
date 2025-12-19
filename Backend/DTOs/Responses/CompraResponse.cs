using Backend.Enums;

namespace Backend.DTOs.Responses;

public class CompraResponse
{
    public int Id { get; set; }
    public string NumeroVoucher { get; set; } = string.Empty;
    public int ClienteProveedorId { get; set; }
    public string ClienteNombre { get; set; } = string.Empty;
    public string ClienteDNI { get; set; } = string.Empty;
    public int ProductoId { get; set; }
    public string ProductoNombre { get; set; } = string.Empty;
    public int CajaId { get; set; }
    public string NivelSecado { get; set; } = string.Empty;
    public string Calidad { get; set; } = string.Empty;
    public TipoPesado TipoPesado { get; set; }
    public decimal PesoBruto { get; set; }
    public decimal DescuentoKg { get; set; }
    public decimal PesoNeto { get; set; }
    public decimal PrecioPorKg { get; set; }
    public decimal MontoTotal { get; set; }
    public DateTime FechaCompra { get; set; }
    public bool Editada { get; set; }
    public DateTime? FechaEdicion { get; set; }
    public bool EsAjustePosterior { get; set; }
}
