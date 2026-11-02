using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace GymFlow.ContextoBD.Migrations
{
    public partial class ActualizacionNotasCreditoYFechaVencimiento : Migration
    {
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.Sql(
                """
                SET XACT_ABORT ON;

                IF OBJECT_ID(N'dbo.T_NOTA_CREDITO', N'U') IS NULL
                    THROW 50000, 'No existe dbo.T_NOTA_CREDITO.', 1;

                IF OBJECT_ID(N'dbo.T_NOTA_VENTA', N'U') IS NULL
                    THROW 50001, 'No existe dbo.T_NOTA_VENTA.', 1;

                IF OBJECT_ID(N'dbo.CuotasCliente', N'U') IS NULL
                    THROW 50002, 'No existe dbo.CuotasCliente.', 1;

                IF COL_LENGTH(N'dbo.T_NOTA_CREDITO', N'NotaVentaId') IS NOT NULL
                    THROW 50003, 'dbo.T_NOTA_CREDITO.NotaVentaId ya existe; revise el esquema y el historial de migraciones.', 1;

                IF EXISTS (SELECT 1 FROM dbo.T_NOTA_CREDITO)
                    THROW 50004, 'T_NOTA_CREDITO contiene filas. No se asignara una venta por defecto; reconcilie los datos antes de aplicar esta migracion.', 1;

                IF NOT EXISTS (
                    SELECT 1
                    FROM sys.columns AS c
                    JOIN sys.types AS t ON t.user_type_id = c.user_type_id
                    WHERE c.object_id = OBJECT_ID(N'dbo.CuotasCliente')
                      AND c.name = N'FechaVencimiento'
                      AND t.name = N'datetime2'
                      AND c.is_nullable = 0)
                    THROW 50005, 'CuotasCliente.FechaVencimiento no coincide con DATETIME2 NOT NULL; revise el esquema antes de aplicar esta migracion.', 1;
                """);

            migrationBuilder.AddColumn<int>(
                name: "NotaVentaId",
                table: "T_NOTA_CREDITO",
                type: "int",
                nullable: false);

            migrationBuilder.CreateIndex(
                name: "IX_T_NOTA_CREDITO_NotaVentaId",
                table: "T_NOTA_CREDITO",
                column: "NotaVentaId");

            migrationBuilder.AddForeignKey(
                name: "FK_T_NOTA_CREDITO_T_NOTA_VENTA_NotaVentaId",
                table: "T_NOTA_CREDITO",
                column: "NotaVentaId",
                principalTable: "T_NOTA_VENTA",
                principalColumn: "Id",
                onDelete: ReferentialAction.NoAction);

            migrationBuilder.Sql(
                "ALTER TABLE [dbo].[CuotasCliente] ALTER COLUMN [FechaVencimiento] datetime2 NULL;");
        }

        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.Sql(
                """
                SET XACT_ABORT ON;

                IF OBJECT_ID(N'dbo.CuotasCliente', N'U') IS NULL
                    THROW 50009, 'No existe dbo.CuotasCliente.', 1;

                IF OBJECT_ID(N'dbo.T_NOTA_CREDITO', N'U') IS NULL
                    THROW 50010, 'No existe dbo.T_NOTA_CREDITO.', 1;

                IF COL_LENGTH(N'dbo.T_NOTA_CREDITO', N'NotaVentaId') IS NULL
                    THROW 50011, 'dbo.T_NOTA_CREDITO.NotaVentaId no existe.', 1;

                IF EXISTS (SELECT 1 FROM dbo.T_NOTA_CREDITO)
                    THROW 50012, 'No se puede revertir la migracion mientras existan notas de credito: se perderia su asociacion con la venta.', 1;

                IF EXISTS (
                    SELECT 1
                    FROM dbo.CuotasCliente
                    WHERE FechaVencimiento IS NULL)
                    THROW 50013, 'No se puede restaurar NOT NULL: existen fechas de vencimiento NULL.', 1;
                """);

            migrationBuilder.DropForeignKey(
                name: "FK_T_NOTA_CREDITO_T_NOTA_VENTA_NotaVentaId",
                table: "T_NOTA_CREDITO");

            migrationBuilder.DropIndex(
                name: "IX_T_NOTA_CREDITO_NotaVentaId",
                table: "T_NOTA_CREDITO");

            migrationBuilder.DropColumn(
                name: "NotaVentaId",
                table: "T_NOTA_CREDITO");

            migrationBuilder.Sql(
                "ALTER TABLE [dbo].[CuotasCliente] ALTER COLUMN [FechaVencimiento] datetime2 NOT NULL;");
        }
    }
}
