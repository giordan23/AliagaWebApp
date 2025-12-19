using Backend.Enums;

namespace Backend.DTOs.Responses;

public class ReporteMovimientosCajaResponse
{
    public DateTime Fecha { get; set; }
    public int CajaId { get; set; }
    public TipoMovimiento TipoMovimiento { get; set; }
    public string Concepto { get; set; } = string.Empty;
    public decimal Monto { get; set; }
    public TipoOperacion TipoOperacion { get; set; }
    public bool EsAjustePosterior { get; set; }
}
