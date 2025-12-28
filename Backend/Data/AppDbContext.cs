using Microsoft.EntityFrameworkCore;
using Backend.Models;

namespace Backend.Data;

public class AppDbContext : DbContext
{
    public AppDbContext(DbContextOptions<AppDbContext> options) : base(options)
    {
    }

    public DbSet<Zona> Zonas { get; set; }
    public DbSet<ClienteProveedor> ClientesProveedores { get; set; }
    public DbSet<ClienteComprador> ClientesCompradores { get; set; }
    public DbSet<Producto> Productos { get; set; }
    public DbSet<Caja> Cajas { get; set; }
    public DbSet<Compra> Compras { get; set; }
    public DbSet<DetalleCompra> DetallesCompra { get; set; }
    public DbSet<Venta> Ventas { get; set; }
    public DbSet<Prestamo> Prestamos { get; set; }
    public DbSet<MovimientoCaja> MovimientosCaja { get; set; }
    public DbSet<ConfiguracionNegocio> ConfiguracionNegocio { get; set; }

    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        base.OnModelCreating(modelBuilder);

        // Configurar índices únicos
        modelBuilder.Entity<ClienteProveedor>()
            .HasIndex(c => c.DNI)
            .IsUnique();

        modelBuilder.Entity<Compra>()
            .HasIndex(c => c.NumeroVoucher)
            .IsUnique();

        modelBuilder.Entity<Caja>()
            .HasIndex(c => c.Fecha)
            .IsUnique();

        modelBuilder.Entity<Zona>()
            .HasIndex(z => z.Nombre)
            .IsUnique();

        // Configurar relaciones
        modelBuilder.Entity<ClienteProveedor>()
            .HasOne(c => c.Zona)
            .WithMany(z => z.Clientes)
            .HasForeignKey(c => c.ZonaId)
            .OnDelete(DeleteBehavior.SetNull);

        modelBuilder.Entity<Compra>()
            .HasOne(c => c.ClienteProveedor)
            .WithMany(cp => cp.Compras)
            .HasForeignKey(c => c.ClienteProveedorId)
            .OnDelete(DeleteBehavior.Restrict);

        modelBuilder.Entity<Compra>()
            .HasOne(c => c.Caja)
            .WithMany(ca => ca.Compras)
            .HasForeignKey(c => c.CajaId)
            .OnDelete(DeleteBehavior.Restrict);

        // Relación Compra -> DetalleCompra
        modelBuilder.Entity<DetalleCompra>()
            .HasOne(d => d.Compra)
            .WithMany(c => c.Detalles)
            .HasForeignKey(d => d.CompraId)
            .OnDelete(DeleteBehavior.Cascade);

        // Relación DetalleCompra -> Producto
        modelBuilder.Entity<DetalleCompra>()
            .HasOne(d => d.Producto)
            .WithMany()
            .HasForeignKey(d => d.ProductoId)
            .OnDelete(DeleteBehavior.Restrict);

        modelBuilder.Entity<Venta>()
            .HasOne(v => v.ClienteComprador)
            .WithMany(cc => cc.Ventas)
            .HasForeignKey(v => v.ClienteCompradorId)
            .OnDelete(DeleteBehavior.Restrict);

        modelBuilder.Entity<Venta>()
            .HasOne(v => v.Producto)
            .WithMany(p => p.Ventas)
            .HasForeignKey(v => v.ProductoId)
            .OnDelete(DeleteBehavior.Restrict);

        modelBuilder.Entity<Venta>()
            .HasOne(v => v.Caja)
            .WithMany(ca => ca.Ventas)
            .HasForeignKey(v => v.CajaId)
            .OnDelete(DeleteBehavior.Restrict);

        modelBuilder.Entity<Prestamo>()
            .HasOne(p => p.ClienteProveedor)
            .WithMany(cp => cp.Prestamos)
            .HasForeignKey(p => p.ClienteProveedorId)
            .OnDelete(DeleteBehavior.Restrict);

        modelBuilder.Entity<Prestamo>()
            .HasOne(p => p.Caja)
            .WithMany(ca => ca.Prestamos)
            .HasForeignKey(p => p.CajaId)
            .OnDelete(DeleteBehavior.Restrict);

        modelBuilder.Entity<MovimientoCaja>()
            .HasOne(m => m.Caja)
            .WithMany(ca => ca.Movimientos)
            .HasForeignKey(m => m.CajaId)
            .OnDelete(DeleteBehavior.Restrict);

        // Configurar precisión de decimales
        modelBuilder.Entity<Producto>()
            .Property(p => p.PrecioSugeridoPorKg)
            .HasPrecision(18, 2);

        modelBuilder.Entity<Caja>()
            .Property(c => c.MontoInicial)
            .HasPrecision(18, 2);

        modelBuilder.Entity<Caja>()
            .Property(c => c.MontoEsperado)
            .HasPrecision(18, 2);

        modelBuilder.Entity<Caja>()
            .Property(c => c.ArqueoReal)
            .HasPrecision(18, 2);

        modelBuilder.Entity<Caja>()
            .Property(c => c.Diferencia)
            .HasPrecision(18, 2);

        modelBuilder.Entity<Compra>()
            .Property(c => c.PesoTotal)
            .HasPrecision(18, 1);

        modelBuilder.Entity<Compra>()
            .Property(c => c.MontoTotal)
            .HasPrecision(18, 2);

        modelBuilder.Entity<DetalleCompra>()
            .Property(d => d.PesoBruto)
            .HasPrecision(18, 1);

        modelBuilder.Entity<DetalleCompra>()
            .Property(d => d.DescuentoKg)
            .HasPrecision(18, 1);

        modelBuilder.Entity<DetalleCompra>()
            .Property(d => d.PesoNeto)
            .HasPrecision(18, 1);

        modelBuilder.Entity<DetalleCompra>()
            .Property(d => d.PrecioPorKg)
            .HasPrecision(18, 2);

        modelBuilder.Entity<DetalleCompra>()
            .Property(d => d.Subtotal)
            .HasPrecision(18, 2);

        modelBuilder.Entity<Venta>()
            .Property(v => v.PesoBruto)
            .HasPrecision(18, 1);

        modelBuilder.Entity<Venta>()
            .Property(v => v.PesoNeto)
            .HasPrecision(18, 1);

        modelBuilder.Entity<Venta>()
            .Property(v => v.PrecioPorKg)
            .HasPrecision(18, 2);

        modelBuilder.Entity<Venta>()
            .Property(v => v.MontoTotal)
            .HasPrecision(18, 2);

        modelBuilder.Entity<ClienteProveedor>()
            .Property(c => c.SaldoPrestamo)
            .HasPrecision(18, 2);

        modelBuilder.Entity<Prestamo>()
            .Property(p => p.Monto)
            .HasPrecision(18, 2);

        modelBuilder.Entity<Prestamo>()
            .Property(p => p.SaldoDespues)
            .HasPrecision(18, 2);

        modelBuilder.Entity<MovimientoCaja>()
            .Property(m => m.Monto)
            .HasPrecision(18, 2);

        // Seed Data
        SeedData(modelBuilder);
    }

    private void SeedData(ModelBuilder modelBuilder)
    {
        // 1. ConfiguracionNegocio
        modelBuilder.Entity<ConfiguracionNegocio>().HasData(
            new ConfiguracionNegocio
            {
                Id = 1,
                NombreNegocio = "Comercial Aliaga",
                Direccion = "Dirección del negocio",
                Telefono = "000-000-000",
                RUC = "00000000000",
                MensajeVoucher = "Gracias por su venta",
                ContadorVoucher = 1
            }
        );

        // 2. Cliente Anónimo (DNI 00000000)
        modelBuilder.Entity<ClienteProveedor>().HasData(
            new ClienteProveedor
            {
                Id = 1,
                DNI = "00000000",
                NombreCompleto = "ANÓNIMO",
                EsAnonimo = true,
                SaldoPrestamo = 0,
                FechaCreacion = new DateTime(2025, 1, 1),
                FechaModificacion = new DateTime(2025, 1, 1)
            }
        );

        // 3. Productos fijos con características
        modelBuilder.Entity<Producto>().HasData(
            new Producto
            {
                Id = 1,
                Nombre = "Café",
                PrecioSugeridoPorKg = 8.50m,
                NivelesSecado = "[\"Mote-baba\",\"Húmedo\",\"Estándar\",\"Seco\"]",
                Calidades = "[\"Bajo\",\"Medio\",\"Alto\"]",
                PermiteValdeo = true,
                FechaModificacion = new DateTime(2025, 1, 1)
            },
            new Producto
            {
                Id = 2,
                Nombre = "Cacao",
                PrecioSugeridoPorKg = 7.00m,
                NivelesSecado = "[\"Mote-baba\",\"Húmedo\",\"Estándar\",\"Seco\"]",
                Calidades = "[\"Bajo\",\"Normal\",\"Alto\"]",
                PermiteValdeo = true,
                FechaModificacion = new DateTime(2025, 1, 1)
            },
            new Producto
            {
                Id = 3,
                Nombre = "Maíz",
                PrecioSugeridoPorKg = 2.50m,
                NivelesSecado = "[\"Estándar\"]",
                Calidades = "[\"Normal\"]",
                PermiteValdeo = false,
                FechaModificacion = new DateTime(2025, 1, 1)
            },
            new Producto
            {
                Id = 4,
                Nombre = "Achiote",
                PrecioSugeridoPorKg = 15.00m,
                NivelesSecado = "[\"Estándar\"]",
                Calidades = "[\"Normal\"]",
                PermiteValdeo = false,
                FechaModificacion = new DateTime(2025, 1, 1)
            }
        );
    }
}