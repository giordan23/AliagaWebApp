using System.ComponentModel.DataAnnotations;
using Backend.Enums;

namespace Backend.Models;

public class Compra
{
    public int Id { get; set; }

    [Required]
    [MaxLength(8)]
    public string NumeroVoucher { get; set; } = string.Empty;

    [Required]
    public int ClienteProveedorId { get; set; }

    [Required]
    public int ProductoId { get; set; }

    [Required]
    public int CajaId { get; set; }

    [Required]
    [MaxLength(50)]
    public string NivelSecado { get; set; } = string.Empty;

    [Required]
    [MaxLength(50)]
    public string Calidad { get; set; } = string.Empty;

    [Required]
    public TipoPesado TipoPesado { get; set; }

    [Required]
    public decimal PesoBruto { get; set; }

    public decimal DescuentoKg { get; set; } = 0;

    public decimal PesoNeto { get; set; }

    [Required]
    public decimal PrecioPorKg { get; set; }

    public decimal MontoTotal { get; set; }

    public DateTime FechaCompra { get; set; } = DateTime.Now;

    public bool Editada { get; set; } = false;

    public DateTime? FechaEdicion { get; set; }

    public bool EsAjustePosterior { get; set; } = false;

    // Navigation properties
    public ClienteProveedor ClienteProveedor { get; set; } = null!;
    public Producto Producto { get; set; } = null!;
    public Caja Caja { get; set; } = null!;
}
