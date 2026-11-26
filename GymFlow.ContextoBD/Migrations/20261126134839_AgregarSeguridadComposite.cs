using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace GymFlow.ContextoBD.Migrations
{
    /// <inheritdoc />
    public partial class AgregarSeguridadComposite : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.CreateTable(
                name: "T_GRUPO",
                columns: table => new
                {
                    Id = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    Descripcion = table.Column<string>(type: "nvarchar(250)", maxLength: 250, nullable: true),
                    Estado = table.Column<string>(type: "nvarchar(20)", maxLength: 20, nullable: false),
                    Nombre = table.Column<string>(type: "nvarchar(100)", maxLength: 100, nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_T_GRUPO", x => x.Id);
                });

            migrationBuilder.CreateTable(
                name: "T_MODULO",
                columns: table => new
                {
                    Id = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    Nombre = table.Column<string>(type: "nvarchar(100)", maxLength: 100, nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_T_MODULO", x => x.Id);
                });

            migrationBuilder.CreateTable(
                name: "T_GRUPO_GRUPO",
                columns: table => new
                {
                    GrupoPadreId = table.Column<int>(type: "int", nullable: false),
                    GrupoHijoId = table.Column<int>(type: "int", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_T_GRUPO_GRUPO", x => new { x.GrupoPadreId, x.GrupoHijoId });
                    table.CheckConstraint("CK_T_GRUPO_GRUPO_NoAutoReferencia", "[GrupoPadreId] <> [GrupoHijoId]");
                    table.ForeignKey(
                        name: "FK_T_GRUPO_GRUPO_T_GRUPO_GrupoHijoId",
                        column: x => x.GrupoHijoId,
                        principalTable: "T_GRUPO",
                        principalColumn: "Id");
                    table.ForeignKey(
                        name: "FK_T_GRUPO_GRUPO_T_GRUPO_GrupoPadreId",
                        column: x => x.GrupoPadreId,
                        principalTable: "T_GRUPO",
                        principalColumn: "Id");
                });

            migrationBuilder.CreateTable(
                name: "T_USUARIO_GRUPO",
                columns: table => new
                {
                    UsuarioId = table.Column<int>(type: "int", nullable: false),
                    GrupoId = table.Column<int>(type: "int", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_T_USUARIO_GRUPO", x => new { x.UsuarioId, x.GrupoId });
                    table.ForeignKey(
                        name: "FK_T_USUARIO_GRUPO_T_GRUPO_GrupoId",
                        column: x => x.GrupoId,
                        principalTable: "T_GRUPO",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                    table.ForeignKey(
                        name: "FK_T_USUARIO_GRUPO_Usuarios_UsuarioId",
                        column: x => x.UsuarioId,
                        principalTable: "Usuarios",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                });

            migrationBuilder.CreateTable(
                name: "T_FORMULARIO",
                columns: table => new
                {
                    Id = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    Nombre = table.Column<string>(type: "nvarchar(100)", maxLength: 100, nullable: false),
                    ModuloId = table.Column<int>(type: "int", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_T_FORMULARIO", x => x.Id);
                    table.ForeignKey(
                        name: "FK_T_FORMULARIO_T_MODULO_ModuloId",
                        column: x => x.ModuloId,
                        principalTable: "T_MODULO",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                });

            migrationBuilder.CreateTable(
                name: "T_ACCION",
                columns: table => new
                {
                    Id = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    Codigo = table.Column<string>(type: "nvarchar(80)", maxLength: 80, nullable: false),
                    FormularioId = table.Column<int>(type: "int", nullable: false),
                    Nombre = table.Column<string>(type: "nvarchar(150)", maxLength: 150, nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_T_ACCION", x => x.Id);
                    table.ForeignKey(
                        name: "FK_T_ACCION_T_FORMULARIO_FormularioId",
                        column: x => x.FormularioId,
                        principalTable: "T_FORMULARIO",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                });

            migrationBuilder.CreateTable(
                name: "T_GRUPO_ACCION",
                columns: table => new
                {
                    GrupoId = table.Column<int>(type: "int", nullable: false),
                    AccionId = table.Column<int>(type: "int", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_T_GRUPO_ACCION", x => new { x.GrupoId, x.AccionId });
                    table.ForeignKey(
                        name: "FK_T_GRUPO_ACCION_T_ACCION_AccionId",
                        column: x => x.AccionId,
                        principalTable: "T_ACCION",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                    table.ForeignKey(
                        name: "FK_T_GRUPO_ACCION_T_GRUPO_GrupoId",
                        column: x => x.GrupoId,
                        principalTable: "T_GRUPO",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "T_USUARIO_ACCION",
                columns: table => new
                {
                    UsuarioId = table.Column<int>(type: "int", nullable: false),
                    AccionId = table.Column<int>(type: "int", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_T_USUARIO_ACCION", x => new { x.UsuarioId, x.AccionId });
                    table.ForeignKey(
                        name: "FK_T_USUARIO_ACCION_T_ACCION_AccionId",
                        column: x => x.AccionId,
                        principalTable: "T_ACCION",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                    table.ForeignKey(
                        name: "FK_T_USUARIO_ACCION_Usuarios_UsuarioId",
                        column: x => x.UsuarioId,
                        principalTable: "Usuarios",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                });

            migrationBuilder.CreateIndex(
                name: "IX_T_ACCION_Codigo",
                table: "T_ACCION",
                column: "Codigo",
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_T_ACCION_FormularioId",
                table: "T_ACCION",
                column: "FormularioId");

            migrationBuilder.CreateIndex(
                name: "IX_T_FORMULARIO_ModuloId_Nombre",
                table: "T_FORMULARIO",
                columns: new[] { "ModuloId", "Nombre" },
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_T_GRUPO_Nombre",
                table: "T_GRUPO",
                column: "Nombre",
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_T_GRUPO_ACCION_AccionId",
                table: "T_GRUPO_ACCION",
                column: "AccionId");

            migrationBuilder.CreateIndex(
                name: "IX_T_GRUPO_GRUPO_GrupoHijoId",
                table: "T_GRUPO_GRUPO",
                column: "GrupoHijoId");

            migrationBuilder.CreateIndex(
                name: "IX_T_MODULO_Nombre",
                table: "T_MODULO",
                column: "Nombre",
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_T_USUARIO_ACCION_AccionId",
                table: "T_USUARIO_ACCION",
                column: "AccionId");

            migrationBuilder.CreateIndex(
                name: "IX_T_USUARIO_GRUPO_GrupoId",
                table: "T_USUARIO_GRUPO",
                column: "GrupoId");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "T_GRUPO_ACCION");

            migrationBuilder.DropTable(
                name: "T_GRUPO_GRUPO");

            migrationBuilder.DropTable(
                name: "T_USUARIO_ACCION");

            migrationBuilder.DropTable(
                name: "T_USUARIO_GRUPO");

            migrationBuilder.DropTable(
                name: "T_ACCION");

            migrationBuilder.DropTable(
                name: "T_GRUPO");

            migrationBuilder.DropTable(
                name: "T_FORMULARIO");

            migrationBuilder.DropTable(
                name: "T_MODULO");
        }
    }
}
