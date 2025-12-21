using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using Backend.Enums;

namespace Backend.Models;

public class DetalleCompra
{
    [Key]
    public int Id { get; set; }

    [Required]
    public int CompraId { get; set; }

    [ForeignKey(nameof(CompraId))]
    public Compra Compra { get; set; } = null!;

    [Required]
    public int ProductoId { get; set; }

    [ForeignKey(nameof(ProductoId))]
    public Producto Producto { get; set; } = null!;

    [Required]
    [MaxLength(50)]
    public string NivelSecado { get; set; } = string.Empty;

    [Required]
    [MaxLength(50)]
    public string Calidad { get; set; } = string.Empty;

    [Required]
    public TipoPesado TipoPesado { get; set; }

    [Required]
    [Column(TypeName = "decimal(18,1)")]
    public decimal PesoBruto { get; set; }

    [Required]
    [Column(TypeName = "decimal(18,1)")]
    public decimal DescuentoKg { get; set; }

    [Required]
    [Column(TypeName = "decimal(18,1)")]
    public decimal PesoNeto { get; set; }

    [Required]
    [Column(TypeName = "decimal(18,2)")]
    public decimal PrecioPorKg { get; set; }

    [Required]
    [Column(TypeName = "decimal(18,2)")]
    public decimal Subtotal { get; set; }

    [Required]
    public DateTime FechaCreacion { get; set; }
}
