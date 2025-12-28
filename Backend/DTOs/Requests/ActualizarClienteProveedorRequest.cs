using System.ComponentModel.DataAnnotations;

namespace Backend.DTOs.Requests;

public class ActualizarClienteProveedorRequest
{
    [Required(ErrorMessage = "El nombre completo es obligatorio")]
    [MaxLength(200)]
    public string NombreCompleto { get; set; } = string.Empty;

    [MaxLength(20)]
    public string? Telefono { get; set; }

    [MaxLength(300)]
    public string? Direccion { get; set; }

    public DateTime? FechaNacimiento { get; set; }

    public int? ZonaId { get; set; }

    [MaxLength(100)]
    public string? ZonaNombre { get; set; }
}
