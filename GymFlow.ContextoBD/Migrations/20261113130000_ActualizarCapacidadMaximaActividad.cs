using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace GymFlow.ContextoBD.Migrations
{
    /// <inheritdoc />
    public partial class ActualizarCapacidadMaximaActividad : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            // 1. Control previo estricto: no limitar silenciosamente a 30.
            // Si alguna actividad superaría el límite de 30, se aborta la migración
            // sin renombrar ni alterar datos y se incluyen sus valores en el error.
            migrationBuilder.Sql(@"
                DECLARE @Detalle nvarchar(max);

                SELECT @Detalle = STUFF((
                    SELECT N'; Id=' + CONVERT(nvarchar(20), a.Id)
                        + N', Nombre=' + COALESCE(a.Nombre, N'<sin nombre>')
                        + N', CupoMaximo=' + CONVERT(nvarchar(20), a.CupoMaximo)
                        + N', Inscriptos=' + CONVERT(nvarchar(20), ISNULL(i.TotalInscriptos, 0))
                        + N', Resultado=' + CONVERT(nvarchar(20), a.CupoMaximo + ISNULL(i.TotalInscriptos, 0))
                    FROM Actividades a
                    LEFT JOIN (
                        SELECT ActividadId, COUNT(*) AS TotalInscriptos
                        FROM Inscripciones
                        GROUP BY ActividadId
                    ) i ON a.Id = i.ActividadId
                    WHERE a.CupoMaximo + ISNULL(i.TotalInscriptos, 0) > 30
                    ORDER BY a.Id
                    FOR XML PATH(N''), TYPE
                ).value(N'.', N'nvarchar(max)'), 1, 2, N'');

                IF @Detalle IS NOT NULL
                BEGIN
                    DECLARE @Mensaje nvarchar(2047) =
                        LEFT(N'La migración se detuvo: la capacidad resultante supera 30.' + @Detalle, 2047);
                    THROW 51000, @Mensaje, 1;
                END
            ");

            // 2. Renombrar columna
            migrationBuilder.RenameColumn(
                name: "CupoMaximo",
                table: "Actividades",
                newName: "CapacidadMaxima");

            // 3. Conversión de datos: CapacidadMaxima = CupoMaximo_anterior + COUNT(Inscripciones)
            migrationBuilder.Sql(@"
                UPDATE a
                SET a.CapacidadMaxima = a.CapacidadMaxima + ISNULL(i.TotalInscriptos, 0)
                FROM Actividades a
                LEFT JOIN (
                    SELECT ActividadId, COUNT(*) AS TotalInscriptos
                    FROM Inscripciones
                    GROUP BY ActividadId
                ) i ON a.Id = i.ActividadId;
            ");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            // Revertir datos: CupoMaximo = CapacidadMaxima - COUNT(Inscripciones actuales).
            // NOTA: se recuperan los lugares disponibles según el estado actual de las
            // inscripciones; no necesariamente el valor histórico exacto si cambiaron después de Up().
            migrationBuilder.Sql(@"
                DECLARE @Detalle nvarchar(max);

                SELECT @Detalle = STUFF((
                    SELECT N'; Id=' + CONVERT(nvarchar(20), a.Id)
                        + N', Nombre=' + COALESCE(a.Nombre, N'<sin nombre>')
                        + N', CapacidadMaxima=' + CONVERT(nvarchar(20), a.CapacidadMaxima)
                        + N', Inscriptos=' + CONVERT(nvarchar(20), ISNULL(i.TotalInscriptos, 0))
                        + N', Resultado=' + CONVERT(nvarchar(20), a.CapacidadMaxima - ISNULL(i.TotalInscriptos, 0))
                    FROM Actividades a
                    LEFT JOIN (
                        SELECT ActividadId, COUNT(*) AS TotalInscriptos
                        FROM Inscripciones
                        GROUP BY ActividadId
                    ) i ON a.Id = i.ActividadId
                    WHERE a.CapacidadMaxima - ISNULL(i.TotalInscriptos, 0) < 0
                    ORDER BY a.Id
                    FOR XML PATH(N''), TYPE
                ).value(N'.', N'nvarchar(max)'), 1, 2, N'');

                IF @Detalle IS NOT NULL
                BEGIN
                    DECLARE @Mensaje nvarchar(2047) =
                        LEFT(N'La reversión se detuvo: el valor resultante de CupoMaximo sería negativo.' + @Detalle, 2047);
                    THROW 51003, @Mensaje, 1;
                END

                UPDATE a
                SET a.CapacidadMaxima = a.CapacidadMaxima - ISNULL(i.TotalInscriptos, 0)
                FROM Actividades a
                LEFT JOIN (
                    SELECT ActividadId, COUNT(*) AS TotalInscriptos
                    FROM Inscripciones
                    GROUP BY ActividadId
                ) i ON a.Id = i.ActividadId;
            ");

            migrationBuilder.RenameColumn(
                name: "CapacidadMaxima",
                table: "Actividades",
                newName: "CupoMaximo");
        }
    }
}
