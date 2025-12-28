using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace Backend.Migrations
{
    /// <inheritdoc />
    public partial class ActualizarNombresAMayusculas : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            // Actualizar cliente anónimo de seed data
            migrationBuilder.UpdateData(
                table: "ClientesProveedores",
                keyColumn: "Id",
                keyValue: 1,
                column: "NombreCompleto",
                value: "ANÓNIMO");

            // Convertir todos los nombres de clientes existentes a mayúsculas
            migrationBuilder.Sql("UPDATE ClientesProveedores SET NombreCompleto = UPPER(NombreCompleto) WHERE Eliminado = 0");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.UpdateData(
                table: "ClientesProveedores",
                keyColumn: "Id",
                keyValue: 1,
                column: "NombreCompleto",
                value: "Anónimo");
        }
    }
}
