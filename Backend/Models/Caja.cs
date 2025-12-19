using System.ComponentModel.DataAnnotations;
using Backend.Enums;

namespace Backend.Models;

public class Caja
{
    public int Id { get; set; }

    [Required]
    public DateTime Fecha { get; set; }

    [Required]
    public decimal MontoInicial { get; set; }

    public decimal MontoEsperado { get; set; }

    public decimal? ArqueoReal { get; set; }

    public decimal Diferencia { get; set; }

    [Required]
    public EstadoCaja Estado { get; set; } = EstadoCaja.Abierta;

    public DateTime FechaApertura { get; set; } = DateTime.Now;

    public DateTime? FechaCierre { get; set; }

    [MaxLength(100)]
    public string UsuarioApertura { get; set; } = "Sistema";

    [MaxLength(100)]
    public string? UsuarioCierre { get; set; }

    // Navigation properties
    public ICollection<Compra> Compras { get; set; } = new List<Compra>();
    public ICollection<Venta> Ventas { get; set; } = new List<Venta>();
    public ICollection<Prestamo> Prestamos { get; set; } = new List<Prestamo>();
    public ICollection<MovimientoCaja> Movimientos { get; set; } = new List<MovimientoCaja>();
}
