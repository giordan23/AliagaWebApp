namespace Backend.DTOs.Requests;

public class FiltrosReporteRequest
{
    public DateTime? FechaInicio { get; set; }
    public DateTime? FechaFin { get; set; }
    public int? ClienteId { get; set; }
    public int? ProductoId { get; set; }
    public int? ZonaId { get; set; }
    public int? CajaId { get; set; }
}
