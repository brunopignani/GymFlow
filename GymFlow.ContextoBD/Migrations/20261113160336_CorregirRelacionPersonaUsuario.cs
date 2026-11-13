using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace GymFlow.ContextoBD.Migrations
{
    /// <inheritdoc />
    public partial class CorregirRelacionPersonaUsuario : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.Sql(@"
                DECLARE @Detalle nvarchar(max);

                SELECT @Detalle = STUFF((
                    SELECT N'; UsuarioId=' + CONVERT(nvarchar(20), p.UsuarioId)
                        + N', PersonaIds='
                        + STUFF((
                            SELECT N',' + CONVERT(nvarchar(20), p2.Id)
                            FROM Personas p2
                            WHERE p2.UsuarioId = p.UsuarioId
                            ORDER BY p2.Id
                            FOR XML PATH(N''), TYPE
                        ).value(N'.', N'nvarchar(max)'), 1, 1, N'')
                    FROM Personas p
                    WHERE p.UsuarioId IS NOT NULL
                    GROUP BY p.UsuarioId
                    HAVING COUNT(*) > 1
                    ORDER BY p.UsuarioId
                    FOR XML PATH(N''), TYPE
                ).value(N'.', N'nvarchar(max)'), 1, 2, N'');

                IF @Detalle IS NOT NULL
                BEGIN
                    DECLARE @Mensaje nvarchar(2047) =
                        LEFT(N'La migracion se detuvo por duplicados Persona-Usuario:' + @Detalle, 2047);
                    THROW 51107, @Mensaje, 1;
                END
            ");

            migrationBuilder.DropIndex(
                name: "IX_Personas_UsuarioId",
                table: "Personas");

            migrationBuilder.CreateIndex(
                name: "IX_Personas_UsuarioId",
                table: "Personas",
                column: "UsuarioId",
                unique: true,
                filter: "[UsuarioId] IS NOT NULL");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropIndex(
                name: "IX_Personas_UsuarioId",
                table: "Personas");

            migrationBuilder.CreateIndex(
                name: "IX_Personas_UsuarioId",
                table: "Personas",
                column: "UsuarioId");
        }
    }
}
