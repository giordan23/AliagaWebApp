using System.ComponentModel.DataAnnotations;
using Backend.Enums;

namespace Backend.DTOs.Requests;

public class RegistrarCompraRequest
{
    [Required(ErrorMessage = "El ID del cliente proveedor es obligatorio")]
    public int ClienteProveedorId { get; set; }

    [Required(ErrorMessage = "El ID del producto es obligatorio")]
    public int ProductoId { get; set; }

    [Required(ErrorMessage = "El nivel de secado es obligatorio")]
    [MaxLength(50)]
    public string NivelSecado { get; set; } = string.Empty;

    [Required(ErrorMessage = "La calidad es obligatoria")]
    [MaxLength(50)]
    public string Calidad { get; set; } = string.Empty;

    [Required]
    public TipoPesado TipoPesado { get; set; }

    [Required(ErrorMessage = "El peso bruto es obligatorio")]
    [Range(0.1, double.MaxValue, ErrorMessage = "El peso bruto debe ser mayor a 0")]
    public decimal PesoBruto { get; set; }

    [Range(0, double.MaxValue, ErrorMessage = "El descuento debe ser mayor o igual a 0")]
    public decimal DescuentoKg { get; set; } = 0;

    [Required(ErrorMessage = "El precio por kg es obligatorio")]
    [Range(0.01, double.MaxValue, ErrorMessage = "El precio debe ser mayor a 0")]
    public decimal PrecioPorKg { get; set; }
}
