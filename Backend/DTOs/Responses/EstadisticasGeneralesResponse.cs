namespace Backend.DTOs.Responses;

public class EstadisticasGeneralesResponse
{
    public int TotalProveedores { get; set; }
    public int ProveedoresActivos { get; set; } // Con compras en los últimos 30 días
    public int TotalCompradores { get; set; }
    public int TotalZonas { get; set; }
    public decimal TotalPrestamosVigentes { get; set; }
    public int ClientesConPrestamos { get; set; }
    public decimal PromedioComprasDiarias { get; set; }
    public decimal PromedioVentasDiarias { get; set; }
}
