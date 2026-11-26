using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace GymFlow.ContextoBD.Migrations
{
    /// <inheritdoc />
    public partial class AgregarAjusteCuenta : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.CreateTable(
                name: "T_AJUSTE_CUENTA",
                columns: table => new
                {
                    Id = table.Column<int>(type: "int", nullable: false),
                    Motivo = table.Column<string>(type: "nvarchar(500)", maxLength: 500, nullable: false),
                    EmpleadoId = table.Column<int>(type: "int", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_T_AJUSTE_CUENTA", x => x.Id);
                    table.ForeignKey(
                        name: "FK_T_AJUSTE_CUENTA_Empleados_EmpleadoId",
                        column: x => x.EmpleadoId,
                        principalTable: "Empleados",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                    table.ForeignKey(
                        name: "FK_T_AJUSTE_CUENTA_T_COMPROBANTE_Id",
                        column: x => x.Id,
                        principalTable: "T_COMPROBANTE",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateIndex(
                name: "IX_T_AJUSTE_CUENTA_EmpleadoId",
                table: "T_AJUSTE_CUENTA",
                column: "EmpleadoId");

            // Secuencia propia para la numeración AJ-xxxxxxxx (migración aditiva: no toca datos existentes).
            migrationBuilder.Sql("IF NOT EXISTS (SELECT 1 FROM sys.sequences WHERE name = 'Seq_AjusteCuenta' AND SCHEMA_NAME(schema_id) = 'dbo') CREATE SEQUENCE dbo.Seq_AjusteCuenta AS int START WITH 1 INCREMENT BY 1;");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.Sql("DROP SEQUENCE IF EXISTS dbo.Seq_AjusteCuenta;");

            migrationBuilder.DropTable(
                name: "T_AJUSTE_CUENTA");
        }
    }
}
