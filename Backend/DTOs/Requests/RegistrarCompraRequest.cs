using System.ComponentModel.DataAnnotations;

namespace Backend.DTOs.Requests;

public class RegistrarCompraRequest
{
    /// <summary>
    /// ID del cliente proveedor existente (requerido si no se envía NuevoCliente)
    /// </summary>
    public int? ClienteProveedorId { get; set; }

    /// <summary>
    /// Datos de nuevo cliente (requerido si no se envía ClienteProveedorId)
    /// </summary>
    public NuevoClienteData? NuevoCliente { get; set; }

    [Required(ErrorMessage = "Debe incluir al menos un detalle de producto")]
    [MinLength(1, ErrorMessage = "Debe incluir al menos un producto")]
    public List<DetalleCompraRequest> Detalles { get; set; } = new();
}

public class NuevoClienteData
{
    [Required(ErrorMessage = "El DNI es obligatorio")]
    [StringLength(8, MinimumLength = 8, ErrorMessage = "El DNI debe tener 8 dígitos")]
    [RegularExpression(@"^\d{8}$", ErrorMessage = "El DNI debe contener solo dígitos")]
    public string Dni { get; set; } = string.Empty;

    [Required(ErrorMessage = "El nombre completo es obligatorio")]
    [MaxLength(200)]
    public string NombreCompleto { get; set; } = string.Empty;
}
