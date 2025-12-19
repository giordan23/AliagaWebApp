namespace Backend.DTOs.Responses;

public class ZonaResponse
{
    public int Id { get; set; }
    public string Nombre { get; set; } = string.Empty;
    public int CantidadClientes { get; set; }
    public DateTime FechaCreacion { get; set; }
    public DateTime FechaModificacion { get; set; }
}
