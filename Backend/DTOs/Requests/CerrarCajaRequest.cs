using System.ComponentModel.DataAnnotations;

namespace Backend.DTOs.Requests;

public class CerrarCajaRequest
{
    [Required(ErrorMessage = "El arqueo real es obligatorio")]
    [Range(0, double.MaxValue, ErrorMessage = "El arqueo real debe ser mayor o igual a 0")]
    public decimal ArqueoReal { get; set; }
}
