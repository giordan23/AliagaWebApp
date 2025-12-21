using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
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
    public int CajaId { get; set; }

    [Required]
    [Column(TypeName = "decimal(18,1)")]
    public decimal PesoTotal { get; set; }

    [Required]
    [Column(TypeName = "decimal(18,2)")]
    public decimal MontoTotal { get; set; }

    public DateTime FechaCompra { get; set; } = DateTime.Now;

    public bool Editada { get; set; } = false;

    public DateTime? FechaEdicion { get; set; }

    public bool EsAjustePosterior { get; set; } = false;

    // Navigation properties
    public ClienteProveedor ClienteProveedor { get; set; } = null!;
    public Caja Caja { get; set; } = null!;

    public ICollection<DetalleCompra> Detalles { get; set; } = new List<DetalleCompra>();
}
