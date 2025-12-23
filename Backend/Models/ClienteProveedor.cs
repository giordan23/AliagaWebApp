using System.ComponentModel.DataAnnotations;

namespace Backend.Models;

public class ClienteProveedor
{
    public int Id { get; set; }

    [Required]
    [MaxLength(8)]
    public string DNI { get; set; } = string.Empty;

    [Required]
    [MaxLength(200)]
    public string NombreCompleto { get; set; } = string.Empty;

    [MaxLength(20)]
    public string? Telefono { get; set; }

    [MaxLength(300)]
    public string? Direccion { get; set; }

    public DateTime? FechaNacimiento { get; set; }

    public int? ZonaId { get; set; }

    public decimal SaldoPrestamo { get; set; } = 0;

    public bool EsAnonimo { get; set; } = false;

    public bool Eliminado { get; set; } = false;

    public DateTime FechaCreacion { get; set; } = DateTime.Now;
    public DateTime FechaModificacion { get; set; } = DateTime.Now;

    // Navigation properties
    public Zona? Zona { get; set; }
    public ICollection<Compra> Compras { get; set; } = new List<Compra>();
    public ICollection<Prestamo> Prestamos { get; set; } = new List<Prestamo>();
}
