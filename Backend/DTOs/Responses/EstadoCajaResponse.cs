using Backend.Enums;

namespace Backend.DTOs.Responses;

public class EstadoCajaResponse
{
    public int? CajaId { get; set; }
    public DateTime? Fecha { get; set; }
    public bool CajaAbierta { get; set; }
    public EstadoCaja? Estado { get; set; }
    public decimal MontoInicial { get; set; }
    public decimal MontoEsperado { get; set; }
    public decimal? ArqueoReal { get; set; }
    public decimal Diferencia { get; set; }
    public string Mensaje { get; set; } = string.Empty;
}
