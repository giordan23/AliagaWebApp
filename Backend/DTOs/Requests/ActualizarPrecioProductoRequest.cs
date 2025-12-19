using System.ComponentModel.DataAnnotations;

namespace Backend.DTOs.Requests;

public class ActualizarPrecioProductoRequest
{
    [Required]
    [Range(0.01, double.MaxValue, ErrorMessage = "El precio debe ser mayor a 0")]
    public decimal PrecioSugeridoPorKg { get; set; }
}
