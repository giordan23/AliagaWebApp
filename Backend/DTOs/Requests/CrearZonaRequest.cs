using System.ComponentModel.DataAnnotations;

namespace Backend.DTOs.Requests;

public class CrearZonaRequest
{
    [Required(ErrorMessage = "El nombre es obligatorio")]
    [MaxLength(100, ErrorMessage = "El nombre no puede exceder 100 caracteres")]
    public string Nombre { get; set; } = string.Empty;
}
