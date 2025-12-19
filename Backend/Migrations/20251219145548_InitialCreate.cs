using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

#pragma warning disable CA1814 // Prefer jagged arrays over multidimensional

namespace Backend.Migrations
{
    /// <inheritdoc />
    public partial class InitialCreate : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.CreateTable(
                name: "Cajas",
                columns: table => new
                {
                    Id = table.Column<int>(type: "INTEGER", nullable: false)
                        .Annotation("Sqlite:Autoincrement", true),
                    Fecha = table.Column<DateTime>(type: "TEXT", nullable: false),
                    MontoInicial = table.Column<decimal>(type: "TEXT", precision: 18, scale: 2, nullable: false),
                    MontoEsperado = table.Column<decimal>(type: "TEXT", precision: 18, scale: 2, nullable: false),
                    ArqueoReal = table.Column<decimal>(type: "TEXT", precision: 18, scale: 2, nullable: true),
                    Diferencia = table.Column<decimal>(type: "TEXT", precision: 18, scale: 2, nullable: false),
                    Estado = table.Column<int>(type: "INTEGER", nullable: false),
                    FechaApertura = table.Column<DateTime>(type: "TEXT", nullable: false),
                    FechaCierre = table.Column<DateTime>(type: "TEXT", nullable: true),
                    UsuarioApertura = table.Column<string>(type: "TEXT", maxLength: 100, nullable: false),
                    UsuarioCierre = table.Column<string>(type: "TEXT", maxLength: 100, nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Cajas", x => x.Id);
                });

            migrationBuilder.CreateTable(
                name: "ClientesCompradores",
                columns: table => new
                {
                    Id = table.Column<int>(type: "INTEGER", nullable: false)
                        .Annotation("Sqlite:Autoincrement", true),
                    Nombre = table.Column<string>(type: "TEXT", maxLength: 200, nullable: false),
                    FechaCreacion = table.Column<DateTime>(type: "TEXT", nullable: false),
                    FechaModificacion = table.Column<DateTime>(type: "TEXT", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_ClientesCompradores", x => x.Id);
                });

            migrationBuilder.CreateTable(
                name: "ConfiguracionNegocio",
                columns: table => new
                {
                    Id = table.Column<int>(type: "INTEGER", nullable: false)
                        .Annotation("Sqlite:Autoincrement", true),
                    NombreNegocio = table.Column<string>(type: "TEXT", maxLength: 200, nullable: false),
                    Direccion = table.Column<string>(type: "TEXT", maxLength: 300, nullable: false),
                    Telefono = table.Column<string>(type: "TEXT", maxLength: 20, nullable: false),
                    RUC = table.Column<string>(type: "TEXT", maxLength: 11, nullable: false),
                    MensajeVoucher = table.Column<string>(type: "TEXT", maxLength: 500, nullable: false),
                    ContadorVoucher = table.Column<int>(type: "INTEGER", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_ConfiguracionNegocio", x => x.Id);
                });

            migrationBuilder.CreateTable(
                name: "Productos",
                columns: table => new
                {
                    Id = table.Column<int>(type: "INTEGER", nullable: false)
                        .Annotation("Sqlite:Autoincrement", true),
                    Nombre = table.Column<string>(type: "TEXT", maxLength: 50, nullable: false),
                    PrecioSugeridoPorKg = table.Column<decimal>(type: "TEXT", precision: 18, scale: 2, nullable: false),
                    NivelesSecado = table.Column<string>(type: "TEXT", nullable: false),
                    Calidades = table.Column<string>(type: "TEXT", nullable: false),
                    PermiteValdeo = table.Column<bool>(type: "INTEGER", nullable: false),
                    FechaModificacion = table.Column<DateTime>(type: "TEXT", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Productos", x => x.Id);
                });

            migrationBuilder.CreateTable(
                name: "Zonas",
                columns: table => new
                {
                    Id = table.Column<int>(type: "INTEGER", nullable: false)
                        .Annotation("Sqlite:Autoincrement", true),
                    Nombre = table.Column<string>(type: "TEXT", maxLength: 100, nullable: false),
                    FechaCreacion = table.Column<DateTime>(type: "TEXT", nullable: false),
                    FechaModificacion = table.Column<DateTime>(type: "TEXT", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Zonas", x => x.Id);
                });

            migrationBuilder.CreateTable(
                name: "MovimientosCaja",
                columns: table => new
                {
                    Id = table.Column<int>(type: "INTEGER", nullable: false)
                        .Annotation("Sqlite:Autoincrement", true),
                    CajaId = table.Column<int>(type: "INTEGER", nullable: false),
                    TipoMovimiento = table.Column<int>(type: "INTEGER", nullable: false),
                    ReferenciaId = table.Column<int>(type: "INTEGER", nullable: true),
                    Concepto = table.Column<string>(type: "TEXT", maxLength: 500, nullable: false),
                    Monto = table.Column<decimal>(type: "TEXT", precision: 18, scale: 2, nullable: false),
                    TipoOperacion = table.Column<int>(type: "INTEGER", nullable: false),
                    FechaMovimiento = table.Column<DateTime>(type: "TEXT", nullable: false),
                    EsAjustePosterior = table.Column<bool>(type: "INTEGER", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_MovimientosCaja", x => x.Id);
                    table.ForeignKey(
                        name: "FK_MovimientosCaja_Cajas_CajaId",
                        column: x => x.CajaId,
                        principalTable: "Cajas",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                });

            migrationBuilder.CreateTable(
                name: "Ventas",
                columns: table => new
                {
                    Id = table.Column<int>(type: "INTEGER", nullable: false)
                        .Annotation("Sqlite:Autoincrement", true),
                    ClienteCompradorId = table.Column<int>(type: "INTEGER", nullable: false),
                    ProductoId = table.Column<int>(type: "INTEGER", nullable: false),
                    CajaId = table.Column<int>(type: "INTEGER", nullable: false),
                    PesoBruto = table.Column<decimal>(type: "TEXT", precision: 18, scale: 1, nullable: false),
                    PesoNeto = table.Column<decimal>(type: "TEXT", precision: 18, scale: 1, nullable: false),
                    PrecioPorKg = table.Column<decimal>(type: "TEXT", precision: 18, scale: 2, nullable: false),
                    MontoTotal = table.Column<decimal>(type: "TEXT", precision: 18, scale: 2, nullable: false),
                    FechaVenta = table.Column<DateTime>(type: "TEXT", nullable: false),
                    Editada = table.Column<bool>(type: "INTEGER", nullable: false),
                    FechaEdicion = table.Column<DateTime>(type: "TEXT", nullable: true),
                    EsAjustePosterior = table.Column<bool>(type: "INTEGER", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Ventas", x => x.Id);
                    table.ForeignKey(
                        name: "FK_Ventas_Cajas_CajaId",
                        column: x => x.CajaId,
                        principalTable: "Cajas",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                    table.ForeignKey(
                        name: "FK_Ventas_ClientesCompradores_ClienteCompradorId",
                        column: x => x.ClienteCompradorId,
                        principalTable: "ClientesCompradores",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                    table.ForeignKey(
                        name: "FK_Ventas_Productos_ProductoId",
                        column: x => x.ProductoId,
                        principalTable: "Productos",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                });

            migrationBuilder.CreateTable(
                name: "ClientesProveedores",
                columns: table => new
                {
                    Id = table.Column<int>(type: "INTEGER", nullable: false)
                        .Annotation("Sqlite:Autoincrement", true),
                    DNI = table.Column<string>(type: "TEXT", maxLength: 8, nullable: false),
                    NombreCompleto = table.Column<string>(type: "TEXT", maxLength: 200, nullable: false),
                    Telefono = table.Column<string>(type: "TEXT", maxLength: 20, nullable: true),
                    Direccion = table.Column<string>(type: "TEXT", maxLength: 300, nullable: true),
                    FechaNacimiento = table.Column<DateTime>(type: "TEXT", nullable: true),
                    ZonaId = table.Column<int>(type: "INTEGER", nullable: true),
                    SaldoPrestamo = table.Column<decimal>(type: "TEXT", precision: 18, scale: 2, nullable: false),
                    EsAnonimo = table.Column<bool>(type: "INTEGER", nullable: false),
                    FechaCreacion = table.Column<DateTime>(type: "TEXT", nullable: false),
                    FechaModificacion = table.Column<DateTime>(type: "TEXT", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_ClientesProveedores", x => x.Id);
                    table.ForeignKey(
                        name: "FK_ClientesProveedores_Zonas_ZonaId",
                        column: x => x.ZonaId,
                        principalTable: "Zonas",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.SetNull);
                });

            migrationBuilder.CreateTable(
                name: "Compras",
                columns: table => new
                {
                    Id = table.Column<int>(type: "INTEGER", nullable: false)
                        .Annotation("Sqlite:Autoincrement", true),
                    NumeroVoucher = table.Column<string>(type: "TEXT", maxLength: 8, nullable: false),
                    ClienteProveedorId = table.Column<int>(type: "INTEGER", nullable: false),
                    ProductoId = table.Column<int>(type: "INTEGER", nullable: false),
                    CajaId = table.Column<int>(type: "INTEGER", nullable: false),
                    NivelSecado = table.Column<string>(type: "TEXT", maxLength: 50, nullable: false),
                    Calidad = table.Column<string>(type: "TEXT", maxLength: 50, nullable: false),
                    TipoPesado = table.Column<int>(type: "INTEGER", nullable: false),
                    PesoBruto = table.Column<decimal>(type: "TEXT", precision: 18, scale: 1, nullable: false),
                    DescuentoKg = table.Column<decimal>(type: "TEXT", precision: 18, scale: 1, nullable: false),
                    PesoNeto = table.Column<decimal>(type: "TEXT", precision: 18, scale: 1, nullable: false),
                    PrecioPorKg = table.Column<decimal>(type: "TEXT", precision: 18, scale: 2, nullable: false),
                    MontoTotal = table.Column<decimal>(type: "TEXT", precision: 18, scale: 2, nullable: false),
                    FechaCompra = table.Column<DateTime>(type: "TEXT", nullable: false),
                    Editada = table.Column<bool>(type: "INTEGER", nullable: false),
                    FechaEdicion = table.Column<DateTime>(type: "TEXT", nullable: true),
                    EsAjustePosterior = table.Column<bool>(type: "INTEGER", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Compras", x => x.Id);
                    table.ForeignKey(
                        name: "FK_Compras_Cajas_CajaId",
                        column: x => x.CajaId,
                        principalTable: "Cajas",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                    table.ForeignKey(
                        name: "FK_Compras_ClientesProveedores_ClienteProveedorId",
                        column: x => x.ClienteProveedorId,
                        principalTable: "ClientesProveedores",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                    table.ForeignKey(
                        name: "FK_Compras_Productos_ProductoId",
                        column: x => x.ProductoId,
                        principalTable: "Productos",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                });

            migrationBuilder.CreateTable(
                name: "Prestamos",
                columns: table => new
                {
                    Id = table.Column<int>(type: "INTEGER", nullable: false)
                        .Annotation("Sqlite:Autoincrement", true),
                    ClienteProveedorId = table.Column<int>(type: "INTEGER", nullable: false),
                    CajaId = table.Column<int>(type: "INTEGER", nullable: false),
                    TipoMovimiento = table.Column<int>(type: "INTEGER", nullable: false),
                    Monto = table.Column<decimal>(type: "TEXT", precision: 18, scale: 2, nullable: false),
                    Descripcion = table.Column<string>(type: "TEXT", maxLength: 500, nullable: true),
                    FechaMovimiento = table.Column<DateTime>(type: "TEXT", nullable: false),
                    SaldoDespues = table.Column<decimal>(type: "TEXT", precision: 18, scale: 2, nullable: false),
                    EsAjustePosterior = table.Column<bool>(type: "INTEGER", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Prestamos", x => x.Id);
                    table.ForeignKey(
                        name: "FK_Prestamos_Cajas_CajaId",
                        column: x => x.CajaId,
                        principalTable: "Cajas",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                    table.ForeignKey(
                        name: "FK_Prestamos_ClientesProveedores_ClienteProveedorId",
                        column: x => x.ClienteProveedorId,
                        principalTable: "ClientesProveedores",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                });

            migrationBuilder.InsertData(
                table: "ClientesProveedores",
                columns: new[] { "Id", "DNI", "Direccion", "EsAnonimo", "FechaCreacion", "FechaModificacion", "FechaNacimiento", "NombreCompleto", "SaldoPrestamo", "Telefono", "ZonaId" },
                values: new object[] { 1, "00000000", null, true, new DateTime(2025, 1, 1, 0, 0, 0, 0, DateTimeKind.Unspecified), new DateTime(2025, 1, 1, 0, 0, 0, 0, DateTimeKind.Unspecified), null, "Anónimo", 0m, null, null });

            migrationBuilder.InsertData(
                table: "ConfiguracionNegocio",
                columns: new[] { "Id", "ContadorVoucher", "Direccion", "MensajeVoucher", "NombreNegocio", "RUC", "Telefono" },
                values: new object[] { 1, 1, "Dirección del negocio", "Gracias por su venta", "Comercial Aliaga", "00000000000", "000-000-000" });

            migrationBuilder.InsertData(
                table: "Productos",
                columns: new[] { "Id", "Calidades", "FechaModificacion", "NivelesSecado", "Nombre", "PermiteValdeo", "PrecioSugeridoPorKg" },
                values: new object[,]
                {
                    { 1, "[\"Bajo\",\"Medio\",\"Alto\"]", new DateTime(2025, 1, 1, 0, 0, 0, 0, DateTimeKind.Unspecified), "[\"Mote-baba\",\"Húmedo\",\"Estándar\",\"Seco\"]", "Café", true, 8.50m },
                    { 2, "[\"Normal\",\"Alto\"]", new DateTime(2025, 1, 1, 0, 0, 0, 0, DateTimeKind.Unspecified), "[\"Mote-baba\",\"Húmedo\",\"Estándar\",\"Seco\"]", "Cacao", true, 7.00m },
                    { 3, "[\"Normal\"]", new DateTime(2025, 1, 1, 0, 0, 0, 0, DateTimeKind.Unspecified), "[\"Estándar\"]", "Maíz", false, 2.50m },
                    { 4, "[\"Normal\"]", new DateTime(2025, 1, 1, 0, 0, 0, 0, DateTimeKind.Unspecified), "[\"Estándar\"]", "Achiote", false, 15.00m }
                });

            migrationBuilder.CreateIndex(
                name: "IX_Cajas_Fecha",
                table: "Cajas",
                column: "Fecha",
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_ClientesProveedores_DNI",
                table: "ClientesProveedores",
                column: "DNI",
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_ClientesProveedores_ZonaId",
                table: "ClientesProveedores",
                column: "ZonaId");

            migrationBuilder.CreateIndex(
                name: "IX_Compras_CajaId",
                table: "Compras",
                column: "CajaId");

            migrationBuilder.CreateIndex(
                name: "IX_Compras_ClienteProveedorId",
                table: "Compras",
                column: "ClienteProveedorId");

            migrationBuilder.CreateIndex(
                name: "IX_Compras_NumeroVoucher",
                table: "Compras",
                column: "NumeroVoucher",
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_Compras_ProductoId",
                table: "Compras",
                column: "ProductoId");

            migrationBuilder.CreateIndex(
                name: "IX_MovimientosCaja_CajaId",
                table: "MovimientosCaja",
                column: "CajaId");

            migrationBuilder.CreateIndex(
                name: "IX_Prestamos_CajaId",
                table: "Prestamos",
                column: "CajaId");

            migrationBuilder.CreateIndex(
                name: "IX_Prestamos_ClienteProveedorId",
                table: "Prestamos",
                column: "ClienteProveedorId");

            migrationBuilder.CreateIndex(
                name: "IX_Ventas_CajaId",
                table: "Ventas",
                column: "CajaId");

            migrationBuilder.CreateIndex(
                name: "IX_Ventas_ClienteCompradorId",
                table: "Ventas",
                column: "ClienteCompradorId");

            migrationBuilder.CreateIndex(
                name: "IX_Ventas_ProductoId",
                table: "Ventas",
                column: "ProductoId");

            migrationBuilder.CreateIndex(
                name: "IX_Zonas_Nombre",
                table: "Zonas",
                column: "Nombre",
                unique: true);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "Compras");

            migrationBuilder.DropTable(
                name: "ConfiguracionNegocio");

            migrationBuilder.DropTable(
                name: "MovimientosCaja");

            migrationBuilder.DropTable(
                name: "Prestamos");

            migrationBuilder.DropTable(
                name: "Ventas");

            migrationBuilder.DropTable(
                name: "ClientesProveedores");

            migrationBuilder.DropTable(
                name: "Cajas");

            migrationBuilder.DropTable(
                name: "ClientesCompradores");

            migrationBuilder.DropTable(
                name: "Productos");

            migrationBuilder.DropTable(
                name: "Zonas");
        }
    }
}
