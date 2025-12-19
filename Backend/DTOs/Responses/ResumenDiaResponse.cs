namespace Backend.DTOs.Responses;

public class ResumenDiaResponse
{
    public DateTime Fecha { get; set; }
    public bool CajaAbierta { get; set; }
    public int CajaId { get; set; }
    public decimal MontoInicialCaja { get; set; }
    public int TotalCompras { get; set; }
    public decimal MontoTotalCompras { get; set; }
    public decimal PesoTotalComprasKg { get; set; }
    public int TotalVentas { get; set; }
    public decimal MontoTotalVentas { get; set; }
    public int TotalPrestamos { get; set; }
    public decimal MontoTotalPrestamos { get; set; }
    public int TotalAbonos { get; set; }
    public decimal MontoTotalAbonos { get; set; }
    public decimal SaldoEsperado { get; set; }
}
