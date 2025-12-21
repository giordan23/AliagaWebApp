using System.ComponentModel.DataAnnotations;

namespace Backend.Models;

public class Producto
{
    public int Id { get; set; }

    [Required]
    [MaxLength(50)]
    public string Nombre { get; set; } = string.Empty;

    public decimal PrecioSugeridoPorKg { get; set; }

    // Almacenado como JSON string
    public string NivelesSecado { get; set; } = string.Empty;

    // Almacenado como JSON string
    public string Calidades { get; set; } = string.Empty;

    public bool PermiteValdeo { get; set; }

    public DateTime FechaModificacion { get; set; } = DateTime.Now;

    // Navigation properties
    public ICollection<Venta> Ventas { get; set; } = new List<Venta>();
}