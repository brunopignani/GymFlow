using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace GymFlow.ContextoBD.Migrations
{
    /// <inheritdoc />
    public partial class EliminarEntidadPago : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.Sql(@"
                IF OBJECT_ID(N'dbo.Pagos', N'U') IS NOT NULL
                   AND EXISTS (SELECT 1 FROM dbo.Pagos)
                    THROW 51130, N'La migracion se detuvo porque la tabla Pagos contiene registros historicos.', 1;
            ");

            migrationBuilder.DropTable(
                name: "Pagos");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.CreateTable(
                name: "Pagos",
                columns: table => new
                {
                    Id = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    CuotaClienteId = table.Column<int>(type: "int", nullable: false),
                    EmpleadoId = table.Column<int>(type: "int", nullable: false),
                    Fecha = table.Column<DateTime>(type: "datetime2", nullable: false),
                    Monto = table.Column<decimal>(type: "decimal(18,2)", precision: 18, scale: 2, nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Pagos", x => x.Id);
                    table.ForeignKey(
                        name: "FK_Pagos_CuotasCliente_CuotaClienteId",
                        column: x => x.CuotaClienteId,
                        principalTable: "CuotasCliente",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                    table.ForeignKey(
                        name: "FK_Pagos_Empleados_EmpleadoId",
                        column: x => x.EmpleadoId,
                        principalTable: "Empleados",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                });

            migrationBuilder.CreateIndex(
                name: "IX_Pagos_CuotaClienteId",
                table: "Pagos",
                column: "CuotaClienteId");

            migrationBuilder.CreateIndex(
                name: "IX_Pagos_EmpleadoId",
                table: "Pagos",
                column: "EmpleadoId");
        }
    }
}
