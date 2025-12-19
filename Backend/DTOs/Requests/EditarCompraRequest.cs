using System.ComponentModel.DataAnnotations;

namespace Backend.DTOs.Requests;

public class EditarCompraRequest
{
    [Required(ErrorMessage = "El peso bruto es obligatorio")]
    [Range(0.1, double.MaxValue, ErrorMessage = "El peso bruto debe ser mayor a 0")]
    public decimal PesoBruto { get; set; }

    [Range(0, double.MaxValue, ErrorMessage = "El descuento debe ser mayor o igual a 0")]
    public decimal DescuentoKg { get; set; } = 0;

    [Required(ErrorMessage = "El precio por kg es obligatorio")]
    [Range(0.01, double.MaxValue, ErrorMessage = "El precio debe ser mayor a 0")]
    public decimal PrecioPorKg { get; set; }
}
