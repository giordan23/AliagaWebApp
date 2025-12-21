using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace Backend.Migrations
{
    /// <inheritdoc />
    public partial class ComprasMultiProducto : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            // PASO 1: Agregar nueva columna PesoTotal a Compras
            migrationBuilder.AddColumn<decimal>(
                name: "PesoTotal",
                table: "Compras",
                type: "decimal(18,1)",
                precision: 18,
                scale: 1,
                nullable: false,
                defaultValue: 0m);

            // PASO 2: Crear la tabla DetallesCompra
            migrationBuilder.CreateTable(
                name: "DetallesCompra",
                columns: table => new
                {
                    Id = table.Column<int>(type: "INTEGER", nullable: false)
                        .Annotation("Sqlite:Autoincrement", true),
                    CompraId = table.Column<int>(type: "INTEGER", nullable: false),
                    ProductoId = table.Column<int>(type: "INTEGER", nullable: false),
                    NivelSecado = table.Column<string>(type: "TEXT", maxLength: 50, nullable: false),
                    Calidad = table.Column<string>(type: "TEXT", maxLength: 50, nullable: false),
                    TipoPesado = table.Column<int>(type: "INTEGER", nullable: false),
                    PesoBruto = table.Column<decimal>(type: "decimal(18,1)", precision: 18, scale: 1, nullable: false),
                    DescuentoKg = table.Column<decimal>(type: "decimal(18,1)", precision: 18, scale: 1, nullable: false),
                    PesoNeto = table.Column<decimal>(type: "decimal(18,1)", precision: 18, scale: 1, nullable: false),
                    PrecioPorKg = table.Column<decimal>(type: "decimal(18,2)", precision: 18, scale: 2, nullable: false),
                    Subtotal = table.Column<decimal>(type: "decimal(18,2)", precision: 18, scale: 2, nullable: false),
                    FechaCreacion = table.Column<DateTime>(type: "TEXT", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_DetallesCompra", x => x.Id);
                    table.ForeignKey(
                        name: "FK_DetallesCompra_Compras_CompraId",
                        column: x => x.CompraId,
                        principalTable: "Compras",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                    table.ForeignKey(
                        name: "FK_DetallesCompra_Productos_ProductoId",
                        column: x => x.ProductoId,
                        principalTable: "Productos",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                });

            migrationBuilder.CreateIndex(
                name: "IX_DetallesCompra_CompraId",
                table: "DetallesCompra",
                column: "CompraId");

            migrationBuilder.CreateIndex(
                name: "IX_DetallesCompra_ProductoId",
                table: "DetallesCompra",
                column: "ProductoId");

            // PASO 3: Migrar datos existentes de Compras a DetallesCompra
            migrationBuilder.Sql(@"
                INSERT INTO DetallesCompra
                    (CompraId, ProductoId, NivelSecado, Calidad, TipoPesado, PesoBruto, DescuentoKg, PesoNeto, PrecioPorKg, Subtotal, FechaCreacion)
                SELECT
                    Id,
                    ProductoId,
                    NivelSecado,
                    Calidad,
                    TipoPesado,
                    PesoBruto,
                    DescuentoKg,
                    PesoNeto,
                    PrecioPorKg,
                    MontoTotal,
                    FechaCompra
                FROM Compras
                WHERE ProductoId IS NOT NULL;
            ");

            // PASO 4: Actualizar PesoTotal en Compras (copiar de PesoNeto)
            migrationBuilder.Sql(@"
                UPDATE Compras
                SET PesoTotal = PesoNeto
                WHERE PesoNeto IS NOT NULL;
            ");

            // PASO 5: Eliminar columnas antiguas y foreign key
            migrationBuilder.DropForeignKey(
                name: "FK_Compras_Productos_ProductoId",
                table: "Compras");

            migrationBuilder.DropIndex(
                name: "IX_Compras_ProductoId",
                table: "Compras");

            migrationBuilder.DropColumn(
                name: "Calidad",
                table: "Compras");

            migrationBuilder.DropColumn(
                name: "DescuentoKg",
                table: "Compras");

            migrationBuilder.DropColumn(
                name: "NivelSecado",
                table: "Compras");

            migrationBuilder.DropColumn(
                name: "PesoBruto",
                table: "Compras");

            migrationBuilder.DropColumn(
                name: "PesoNeto",
                table: "Compras");

            migrationBuilder.DropColumn(
                name: "PrecioPorKg",
                table: "Compras");

            migrationBuilder.DropColumn(
                name: "ProductoId",
                table: "Compras");

            migrationBuilder.DropColumn(
                name: "TipoPesado",
                table: "Compras");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "DetallesCompra");

            migrationBuilder.DropColumn(
                name: "PesoTotal",
                table: "Compras");

            migrationBuilder.AlterColumn<decimal>(
                name: "MontoTotal",
                table: "Compras",
                type: "TEXT",
                precision: 18,
                scale: 2,
                nullable: false,
                oldClrType: typeof(decimal),
                oldType: "decimal(18,2)",
                oldPrecision: 18,
                oldScale: 2);

            migrationBuilder.AddColumn<string>(
                name: "Calidad",
                table: "Compras",
                type: "TEXT",
                maxLength: 50,
                nullable: false,
                defaultValue: "");

            migrationBuilder.AddColumn<decimal>(
                name: "DescuentoKg",
                table: "Compras",
                type: "TEXT",
                precision: 18,
                scale: 1,
                nullable: false,
                defaultValue: 0m);

            migrationBuilder.AddColumn<string>(
                name: "NivelSecado",
                table: "Compras",
                type: "TEXT",
                maxLength: 50,
                nullable: false,
                defaultValue: "");

            migrationBuilder.AddColumn<decimal>(
                name: "PesoBruto",
                table: "Compras",
                type: "TEXT",
                precision: 18,
                scale: 1,
                nullable: false,
                defaultValue: 0m);

            migrationBuilder.AddColumn<decimal>(
                name: "PesoNeto",
                table: "Compras",
                type: "TEXT",
                precision: 18,
                scale: 1,
                nullable: false,
                defaultValue: 0m);

            migrationBuilder.AddColumn<decimal>(
                name: "PrecioPorKg",
                table: "Compras",
                type: "TEXT",
                precision: 18,
                scale: 2,
                nullable: false,
                defaultValue: 0m);

            migrationBuilder.AddColumn<int>(
                name: "ProductoId",
                table: "Compras",
                type: "INTEGER",
                nullable: false,
                defaultValue: 0);

            migrationBuilder.AddColumn<int>(
                name: "TipoPesado",
                table: "Compras",
                type: "INTEGER",
                nullable: false,
                defaultValue: 0);

            migrationBuilder.CreateIndex(
                name: "IX_Compras_ProductoId",
                table: "Compras",
                column: "ProductoId");

            migrationBuilder.AddForeignKey(
                name: "FK_Compras_Productos_ProductoId",
                table: "Compras",
                column: "ProductoId",
                principalTable: "Productos",
                principalColumn: "Id",
                onDelete: ReferentialAction.Restrict);
        }
    }
}
