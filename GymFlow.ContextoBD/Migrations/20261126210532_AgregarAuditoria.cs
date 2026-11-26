using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace GymFlow.ContextoBD.Migrations
{
    /// <inheritdoc />
    public partial class AgregarAuditoria : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.CreateTable(
                name: "T_AUDITORIA_CLIENTE",
                columns: table => new
                {
                    Id = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    FechaHora = table.Column<DateTime>(type: "datetime2", nullable: false),
                    Operacion = table.Column<string>(type: "nvarchar(20)", maxLength: 20, nullable: false),
                    ClienteId = table.Column<int>(type: "int", nullable: false),
                    DniCliente = table.Column<int>(type: "int", nullable: false),
                    UsuarioId = table.Column<int>(type: "int", nullable: false),
                    EmpleadoId = table.Column<int>(type: "int", nullable: true),
                    NombreUsuario = table.Column<string>(type: "nvarchar(100)", maxLength: 100, nullable: false),
                    ValoresAnteriores = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    ValoresNuevos = table.Column<string>(type: "nvarchar(max)", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_T_AUDITORIA_CLIENTE", x => x.Id);
                    table.ForeignKey(
                        name: "FK_T_AUDITORIA_CLIENTE_Usuarios_UsuarioId",
                        column: x => x.UsuarioId,
                        principalTable: "Usuarios",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                });

            migrationBuilder.CreateTable(
                name: "T_AUDITORIA_SESION",
                columns: table => new
                {
                    Id = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    FechaHora = table.Column<DateTime>(type: "datetime2", nullable: false),
                    TipoEvento = table.Column<string>(type: "nvarchar(20)", maxLength: 20, nullable: false),
                    UsuarioId = table.Column<int>(type: "int", nullable: false),
                    EmpleadoId = table.Column<int>(type: "int", nullable: true),
                    NombreUsuario = table.Column<string>(type: "nvarchar(100)", maxLength: 100, nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_T_AUDITORIA_SESION", x => x.Id);
                    table.ForeignKey(
                        name: "FK_T_AUDITORIA_SESION_Usuarios_UsuarioId",
                        column: x => x.UsuarioId,
                        principalTable: "Usuarios",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                });

            migrationBuilder.CreateIndex(
                name: "IX_T_AUDITORIA_CLIENTE_ClienteId",
                table: "T_AUDITORIA_CLIENTE",
                column: "ClienteId");

            migrationBuilder.CreateIndex(
                name: "IX_T_AUDITORIA_CLIENTE_DniCliente",
                table: "T_AUDITORIA_CLIENTE",
                column: "DniCliente");

            migrationBuilder.CreateIndex(
                name: "IX_T_AUDITORIA_CLIENTE_FechaHora",
                table: "T_AUDITORIA_CLIENTE",
                column: "FechaHora");

            migrationBuilder.CreateIndex(
                name: "IX_T_AUDITORIA_CLIENTE_UsuarioId",
                table: "T_AUDITORIA_CLIENTE",
                column: "UsuarioId");

            migrationBuilder.CreateIndex(
                name: "IX_T_AUDITORIA_SESION_FechaHora",
                table: "T_AUDITORIA_SESION",
                column: "FechaHora");

            migrationBuilder.CreateIndex(
                name: "IX_T_AUDITORIA_SESION_UsuarioId",
                table: "T_AUDITORIA_SESION",
                column: "UsuarioId");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "T_AUDITORIA_CLIENTE");

            migrationBuilder.DropTable(
                name: "T_AUDITORIA_SESION");
        }
    }
}
