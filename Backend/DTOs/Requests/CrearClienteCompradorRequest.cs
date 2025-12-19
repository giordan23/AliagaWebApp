using System.ComponentModel.DataAnnotations;

namespace Backend.DTOs.Requests;

public class CrearClienteCompradorRequest
{
    [Required(ErrorMessage = "El nombre es obligatorio")]
    [MaxLength(200)]
    public string Nombre { get; set; } = string.Empty;
}
