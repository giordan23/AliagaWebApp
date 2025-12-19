using Backend.Enums;

namespace Backend.DTOs.Responses;

public class CajaResumenResponse
{
    public int Id { get; set; }
    public DateTime Fecha { get; set; }
    public decimal MontoInicial { get; set; }
    public decimal MontoEsperado { get; set; }
    public decimal? ArqueoReal { get; set; }
    public decimal Diferencia { get; set; }
    public EstadoCaja Estado { get; set; }
    public DateTime FechaApertura { get; set; }
    public DateTime? FechaCierre { get; set; }
    public string UsuarioApertura { get; set; } = string.Empty;
    public string? UsuarioCierre { get; set; }

    // Totales calculados
    public decimal TotalIngresos { get; set; }
    public decimal TotalEgresos { get; set; }
    public decimal SaldoActual { get; set; }
}
