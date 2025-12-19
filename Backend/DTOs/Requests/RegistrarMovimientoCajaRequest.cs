using System.ComponentModel.DataAnnotations;
using Backend.Enums;

namespace Backend.DTOs.Requests;

public class RegistrarMovimientoCajaRequest
{
    [Required]
    public TipoMovimiento TipoMovimiento { get; set; }

    [Required(ErrorMessage = "El monto es obligatorio")]
    [Range(0.01, double.MaxValue, ErrorMessage = "El monto debe ser mayor a 0")]
    public decimal Monto { get; set; }

    [MaxLength(500)]
    public string? Descripcion { get; set; }
}
