namespace Backend.DTOs.Responses;

public class AlertaResponse
{
    public string Tipo { get; set; } = string.Empty; // "prestamo", "caja", "advertencia"
    public string Mensaje { get; set; } = string.Empty;
    public string? Detalle { get; set; }
    public int? ReferenciaId { get; set; }
    public string Prioridad { get; set; } = "normal"; // "alta", "media", "normal"
}
