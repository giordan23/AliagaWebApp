using System.ComponentModel.DataAnnotations;
using Backend.Enums;

namespace Backend.Models;

public class MovimientoCaja
{
    public int Id { get; set; }

    [Required]
    public int CajaId { get; set; }

    [Required]
    public TipoMovimiento TipoMovimiento { get; set; }

    public int? ReferenciaId { get; set; }

    [MaxLength(500)]
    public string Concepto { get; set; } = string.Empty;

    [Required]
    public decimal Monto { get; set; }

    [Required]
    public TipoOperacion TipoOperacion { get; set; }

    public DateTime FechaMovimiento { get; set; } = DateTime.Now;

    public bool EsAjustePosterior { get; set; } = false;

    // Navigation properties
    public Caja Caja { get; set; } = null!;
}
