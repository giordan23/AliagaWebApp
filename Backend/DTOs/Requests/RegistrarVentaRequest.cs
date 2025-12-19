using System.ComponentModel.DataAnnotations;

namespace Backend.DTOs.Requests;

public class RegistrarVentaRequest
{
    [Required(ErrorMessage = "El cliente comprador es obligatorio")]
    public int ClienteCompradorId { get; set; }

    [Required(ErrorMessage = "El producto es obligatorio")]
    public int ProductoId { get; set; }

    [Required(ErrorMessage = "El peso neto es obligatorio")]
    [Range(0.1, double.MaxValue, ErrorMessage = "El peso neto debe ser mayor a 0")]
    public decimal PesoNeto { get; set; }

    [Required(ErrorMessage = "El precio por kg es obligatorio")]
    [Range(0.01, double.MaxValue, ErrorMessage = "El precio debe ser mayor a 0")]
    public decimal PrecioPorKg { get; set; }
}
