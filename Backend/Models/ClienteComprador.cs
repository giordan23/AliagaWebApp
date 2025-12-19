using System.ComponentModel.DataAnnotations;

namespace Backend.Models;

public class ClienteComprador
{
    public int Id { get; set; }

    [Required]
    [MaxLength(200)]
    public string Nombre { get; set; } = string.Empty;

    public DateTime FechaCreacion { get; set; } = DateTime.Now;
    public DateTime FechaModificacion { get; set; } = DateTime.Now;

    // Navigation properties
    public ICollection<Venta> Ventas { get; set; } = new List<Venta>();
}
