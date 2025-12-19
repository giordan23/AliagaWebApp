using Backend.Enums;

namespace Backend.DTOs.Responses;

public class CajaDetalleResponse
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

    // Desglose de movimientos
    public decimal TotalCompras { get; set; }
    public decimal TotalVentas { get; set; }
    public decimal TotalPrestamos { get; set; }
    public decimal TotalAbonos { get; set; }
    public decimal TotalInyecciones { get; set; }
    public decimal TotalRetiros { get; set; }
    public decimal TotalGastos { get; set; }

    public int NumeroCompras { get; set; }
    public int NumeroVentas { get; set; }

    public List<MovimientoCajaResponse> Movimientos { get; set; } = new();
}
