using System.ComponentModel.DataAnnotations;

namespace Backend.Models;

public class ConfiguracionNegocio
{
    public int Id { get; set; }

    [Required]
    [MaxLength(200)]
    public string NombreNegocio { get; set; } = "Comercial Aliaga";

    [MaxLength(300)]
    public string Direccion { get; set; } = string.Empty;

    [MaxLength(20)]
    public string Telefono { get; set; } = string.Empty;

    [MaxLength(11)]
    public string RUC { get; set; } = string.Empty;

    [MaxLength(500)]
    public string MensajeVoucher { get; set; } = string.Empty;

    public int ContadorVoucher { get; set; } = 1;
}
