namespace Backend.DTOs.Responses;

public class CompraResponse
{
    public int Id { get; set; }
    public string NumeroVoucher { get; set; } = string.Empty;
    public int ClienteProveedorId { get; set; }
    public string ClienteNombre { get; set; } = string.Empty;
    public string ClienteDNI { get; set; } = string.Empty;
    public int CajaId { get; set; }
    public List<DetalleCompraResponse> Detalles { get; set; } = new();
    public decimal PesoTotal { get; set; }
    public decimal MontoTotal { get; set; }
    public DateTime FechaCompra { get; set; }
    public bool Editada { get; set; }
    public DateTime? FechaEdicion { get; set; }
    public bool EsAjustePosterior { get; set; }
}
