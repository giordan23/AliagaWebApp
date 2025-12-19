namespace Backend.DTOs.Responses;

public class DeudorResponse
{
    public int ClienteId { get; set; }
    public string DNI { get; set; } = string.Empty;
    public string NombreCompleto { get; set; } = string.Empty;
    public string Zona { get; set; } = string.Empty;
    public decimal SaldoPrestamo { get; set; }
    public DateTime? UltimaCompra { get; set; }
    public DateTime? UltimoAbono { get; set; }
}
