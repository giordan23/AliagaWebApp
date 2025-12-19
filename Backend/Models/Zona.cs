using System.ComponentModel.DataAnnotations;

namespace Backend.Models;

public class Zona
{
    public int Id { get; set; }

    [Required]
    [MaxLength(100)]
    public string Nombre { get; set; } = string.Empty;

    public DateTime FechaCreacion { get; set; } = DateTime.Now;
    public DateTime FechaModificacion { get; set; } = DateTime.Now;

    // Navigation properties
    public ICollection<ClienteProveedor> Clientes { get; set; } = new List<ClienteProveedor>();
}
