using Backend.Enums;

namespace Backend.DTOs.Responses;

public class PrestamoResponse
{
    public int Id { get; set; }
    public int ClienteProveedorId { get; set; }
    public string ClienteNombre { get; set; } = string.Empty;
    public string ClienteDNI { get; set; } = string.Empty;
    public int CajaId { get; set; }
    public TipoMovimiento TipoMovimiento { get; set; }
    public decimal Monto { get; set; }
    public decimal SaldoDespues { get; set; }
    public string Descripcion { get; set; } = string.Empty;
    public DateTime FechaMovimiento { get; set; }
    public bool EsAjustePosterior { get; set; }
}
