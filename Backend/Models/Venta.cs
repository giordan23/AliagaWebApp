using System.ComponentModel.DataAnnotations;

namespace Backend.Models;

public class Venta
{
    public int Id { get; set; }

    [Required]
    public int ClienteCompradorId { get; set; }

    [Required]
    public int ProductoId { get; set; }

    // CajaId es opcional ya que las ventas son registros históricos independientes de la caja
    public int? CajaId { get; set; }

    [Required]
    public decimal PesoBruto { get; set; }

    [Required]
    public decimal PesoNeto { get; set; }

    [Required]
    public decimal PrecioPorKg { get; set; }

    [Required]
    public decimal MontoTotal { get; set; }

    public DateTime FechaVenta { get; set; } = DateTime.Now;

    public bool Editada { get; set; } = false;

    public DateTime? FechaEdicion { get; set; }

    public bool EsAjustePosterior { get; set; } = false;

    // Navigation properties
    public ClienteComprador ClienteComprador { get; set; } = null!;
    public Producto Producto { get; set; } = null!;
    public Caja? Caja { get; set; }
}
