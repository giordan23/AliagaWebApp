namespace Backend.DTOs.Responses;

public class ClienteProveedorResponse
{
    public int Id { get; set; }
    public string DNI { get; set; } = string.Empty;
    public string NombreCompleto { get; set; } = string.Empty;
    public string? Telefono { get; set; }
    public string? Direccion { get; set; }
    public DateTime? FechaNacimiento { get; set; }
    public int? ZonaId { get; set; }
    public string? ZonaNombre { get; set; }
    public decimal SaldoPrestamo { get; set; }
    public bool EsAnonimo { get; set; }
    public DateTime FechaCreacion { get; set; }
}
