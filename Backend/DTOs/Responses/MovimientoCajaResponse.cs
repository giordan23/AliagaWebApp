using Backend.Enums;

namespace Backend.DTOs.Responses;

public class MovimientoCajaResponse
{
    public int Id { get; set; }
    public TipoMovimiento TipoMovimiento { get; set; }
    public int? ReferenciaId { get; set; }
    public string Concepto { get; set; } = string.Empty;
    public decimal Monto { get; set; }
    public TipoOperacion TipoOperacion { get; set; }
    public DateTime FechaMovimiento { get; set; }
    public bool EsAjustePosterior { get; set; }
}
