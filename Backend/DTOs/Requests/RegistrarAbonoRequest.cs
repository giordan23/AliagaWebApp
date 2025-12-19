using System.ComponentModel.DataAnnotations;

namespace Backend.DTOs.Requests;

public class RegistrarAbonoRequest
{
    [Required(ErrorMessage = "El cliente proveedor es obligatorio")]
    public int ClienteProveedorId { get; set; }

    [Required(ErrorMessage = "El monto es obligatorio")]
    [Range(0.01, double.MaxValue, ErrorMessage = "El monto debe ser mayor a 0")]
    public decimal Monto { get; set; }

    [MaxLength(500, ErrorMessage = "El concepto no puede exceder 500 caracteres")]
    public string Concepto { get; set; } = "Abono a préstamo";
}
