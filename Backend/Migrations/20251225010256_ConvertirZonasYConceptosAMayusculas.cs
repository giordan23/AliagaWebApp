using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace Backend.Migrations
{
    /// <inheritdoc />
    public partial class ConvertirZonasYConceptosAMayusculas : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            // Convertir nombres de zonas a mayúsculas
            migrationBuilder.Sql("UPDATE Zonas SET Nombre = UPPER(Nombre);");

            // Convertir conceptos de movimientos de caja a mayúsculas
            migrationBuilder.Sql("UPDATE MovimientosCaja SET Concepto = UPPER(Concepto);");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            // No se puede revertir a minúsculas automáticamente
            // porque no se conoce el formato original
        }
    }
}
