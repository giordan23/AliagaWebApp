using System.ComponentModel.DataAnnotations;
using Backend.Enums;

namespace Backend.Models;

public class Prestamo
{
    public int Id { get; set; }

    [Required]
    public int ClienteProveedorId { get; set; }

    [Required]
    public int CajaId { get; set; }

    [Required]
    public TipoMovimiento TipoMovimiento { get; set; }

    [Required]
    public decimal Monto { get; set; }

    [MaxLength(500)]
    public string? Descripcion { get; set; }

    public DateTime FechaMovimiento { get; set; } = DateTime.Now;

    public decimal SaldoDespues { get; set; }

    public bool EsAjustePosterior { get; set; } = false;

    // Navigation properties
    public ClienteProveedor ClienteProveedor { get; set; } = null!;
    public Caja Caja { get; set; } = null!;
}
