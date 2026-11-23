using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace GymFlow.ContextoBD.Migrations
{
    /// <inheritdoc />
    public partial class UnificarDniPersonaUnico : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.Sql(@"
                DECLARE @Detalle nvarchar(max);

                SELECT @Detalle = STUFF((
                    SELECT N'; Dni=' + CONVERT(nvarchar(20), p.Dni)
                        + N', PersonaIds='
                        + STUFF((
                            SELECT N',' + CONVERT(nvarchar(20), p2.Id)
                            FROM Personas p2
                            WHERE p2.Dni = p.Dni
                            ORDER BY p2.Id
                            FOR XML PATH(N''), TYPE
                        ).value(N'.', N'nvarchar(max)'), 1, 1, N'')
                    FROM Personas p
                    GROUP BY p.Dni
                    HAVING COUNT(*) > 1
                    ORDER BY p.Dni
                    FOR XML PATH(N''), TYPE
                ).value(N'.', N'nvarchar(max)'), 1, 2, N'');

                IF @Detalle IS NOT NULL
                BEGIN
                    DECLARE @Mensaje nvarchar(2047) =
                        LEFT(N'La migración se detuvo porque existen DNI duplicados en Personas: ' + @Detalle, 2047);
                    THROW 51140, @Mensaje, 1;
                END
            ");

            migrationBuilder.CreateIndex(
                name: "IX_Personas_Dni",
                table: "Personas",
                column: "Dni",
                unique: true);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropIndex(
                name: "IX_Personas_Dni",
                table: "Personas");
        }
    }
}
