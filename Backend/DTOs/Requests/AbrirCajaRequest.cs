using System.ComponentModel.DataAnnotations;

namespace Backend.DTOs.Requests;

public class AbrirCajaRequest
{
    [Required(ErrorMessage = "El monto inicial es obligatorio")]
    [Range(0, double.MaxValue, ErrorMessage = "El monto inicial debe ser mayor o igual a 0")]
    public decimal MontoInicial { get; set; }
}
