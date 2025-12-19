using System.ComponentModel.DataAnnotations;

namespace Backend.DTOs.Requests;

public class CrearClienteProveedorRequest
{
    [Required(ErrorMessage = "El DNI es obligatorio")]
    [StringLength(8, MinimumLength = 8, ErrorMessage = "El DNI debe tener 8 dígitos")]
    [RegularExpression(@"^\d{8}$", ErrorMessage = "El DNI debe contener solo dígitos")]
    public string DNI { get; set; } = string.Empty;

    [Required(ErrorMessage = "El nombre completo es obligatorio")]
    [MaxLength(200)]
    public string NombreCompleto { get; set; } = string.Empty;

    [MaxLength(20)]
    public string? Telefono { get; set; }

    [MaxLength(300)]
    public string? Direccion { get; set; }

    public DateTime? FechaNacimiento { get; set; }

    public int? ZonaId { get; set; }
}
