using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace GymFlow.ContextoBD.Migrations
{
    /// <inheritdoc />
    public partial class AgregarPeriodoYNumeracionSegura : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.Sql(@"
                IF DB_NAME() <> N'GymFlowDB_Pruebas'
                    THROW 51120, N'La migracion de numeracion solo puede ejecutarse sobre GymFlowDB_Pruebas.', 1;

                IF EXISTS (
                    SELECT 1
                    FROM T_COMPROBANTE
                    WHERE Numero IS NULL
                       OR LTRIM(RTRIM(Numero)) = N''
                       OR LEN(Numero) > 11
                       OR (
                            Numero NOT LIKE N'RP-%'
                            AND Numero NOT LIKE N'NV-%'
                            AND Numero NOT LIKE N'NC-%'
                       )
                       OR TRY_CONVERT(int, SUBSTRING(Numero, 4, 50)) IS NULL
                )
                    THROW 51121, N'La migracion detecto numeros de comprobante incompatibles.', 1;

                IF EXISTS (
                    SELECT Numero
                    FROM T_COMPROBANTE
                    GROUP BY Numero
                    HAVING COUNT(*) > 1
                )
                    THROW 51122, N'La migracion detecto numeros de comprobante duplicados.', 1;

                IF EXISTS (
                    SELECT 1
                    FROM T_NOTA_VENTA nv
                    INNER JOIN T_COMPROBANTE c ON c.Id = nv.Id
                    WHERE nv.CuotaClienteId IS NULL OR c.FechaEmision IS NULL
                )
                    THROW 51123, N'La migracion detecto notas de venta sin cuota o fecha de emision.', 1;

                IF EXISTS (
                    SELECT nv.CuotaClienteId, YEAR(c.FechaEmision), MONTH(c.FechaEmision)
                    FROM T_NOTA_VENTA nv
                    INNER JOIN T_COMPROBANTE c ON c.Id = nv.Id
                    GROUP BY nv.CuotaClienteId, YEAR(c.FechaEmision), MONTH(c.FechaEmision)
                    HAVING COUNT(*) > 1
                )
                    THROW 51124, N'La migracion detecto notas de venta duplicadas por cuota y periodo.', 1;
            ");

            migrationBuilder.DropIndex(
                name: "IX_T_NOTA_VENTA_CuotaClienteId",
                table: "T_NOTA_VENTA");

            migrationBuilder.AddColumn<DateTime>(
                name: "Periodo",
                table: "T_NOTA_VENTA",
                type: "date",
                nullable: true);

            migrationBuilder.Sql(@"
                UPDATE nv
                SET Periodo = DATEFROMPARTS(YEAR(c.FechaEmision), MONTH(c.FechaEmision), 1)
                FROM T_NOTA_VENTA nv
                INNER JOIN T_COMPROBANTE c ON c.Id = nv.Id;

                IF EXISTS (
                    SELECT 1
                    FROM T_NOTA_VENTA
                    WHERE Periodo IS NULL
                )
                    THROW 51125, N'La migracion no pudo completar Periodo para todas las notas de venta.', 1;
            ");

            migrationBuilder.AlterColumn<DateTime>(
                name: "Periodo",
                table: "T_NOTA_VENTA",
                type: "date",
                nullable: false,
                oldClrType: typeof(DateTime),
                oldType: "date",
                oldNullable: true);

            migrationBuilder.AlterColumn<string>(
                name: "Numero",
                table: "T_COMPROBANTE",
                type: "nvarchar(11)",
                maxLength: 11,
                nullable: false,
                oldClrType: typeof(string),
                oldType: "nvarchar(max)",
                oldNullable: true);

            migrationBuilder.CreateIndex(
                name: "IX_T_COMPROBANTE_Numero",
                table: "T_COMPROBANTE",
                column: "Numero",
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_T_NOTA_VENTA_CuotaClienteId_Periodo",
                table: "T_NOTA_VENTA",
                columns: new[] { "CuotaClienteId", "Periodo" },
                unique: true);

            migrationBuilder.Sql(@"
                DECLARE @InicioRP int = ISNULL((
                    SELECT MAX(TRY_CONVERT(int, SUBSTRING(Numero, 4, 50)))
                    FROM T_COMPROBANTE
                    WHERE Numero LIKE N'RP-%'
                ), 0) + 1;
                DECLARE @InicioNV int = ISNULL((
                    SELECT MAX(TRY_CONVERT(int, SUBSTRING(Numero, 4, 50)))
                    FROM T_COMPROBANTE
                    WHERE Numero LIKE N'NV-%'
                ), 0) + 1;
                DECLARE @InicioNC int = ISNULL((
                    SELECT MAX(TRY_CONVERT(int, SUBSTRING(Numero, 4, 50)))
                    FROM T_COMPROBANTE
                    WHERE Numero LIKE N'NC-%'
                ), 0) + 1;

                DECLARE @Sql nvarchar(max);

                SET @Sql = N'CREATE SEQUENCE dbo.Seq_ReciboPago AS int START WITH '
                    + CONVERT(nvarchar(20), @InicioRP) + N' INCREMENT BY 1;';
                EXEC(@Sql);

                SET @Sql = N'CREATE SEQUENCE dbo.Seq_NotaVenta AS int START WITH '
                    + CONVERT(nvarchar(20), @InicioNV) + N' INCREMENT BY 1;';
                EXEC(@Sql);

                SET @Sql = N'CREATE SEQUENCE dbo.Seq_NotaCredito AS int START WITH '
                    + CONVERT(nvarchar(20), @InicioNC) + N' INCREMENT BY 1;';
                EXEC(@Sql);
            ");

        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropIndex(
                name: "IX_T_NOTA_VENTA_CuotaClienteId_Periodo",
                table: "T_NOTA_VENTA");

            migrationBuilder.DropIndex(
                name: "IX_T_COMPROBANTE_Numero",
                table: "T_COMPROBANTE");

            migrationBuilder.DropColumn(
                name: "Periodo",
                table: "T_NOTA_VENTA");

            migrationBuilder.AlterColumn<string>(
                name: "Numero",
                table: "T_COMPROBANTE",
                type: "nvarchar(max)",
                nullable: true,
                oldClrType: typeof(string),
                oldType: "nvarchar(11)",
                oldMaxLength: 11);

            migrationBuilder.CreateIndex(
                name: "IX_T_NOTA_VENTA_CuotaClienteId",
                table: "T_NOTA_VENTA",
                column: "CuotaClienteId");

            migrationBuilder.Sql("DROP SEQUENCE IF EXISTS dbo.Seq_ReciboPago;");
            migrationBuilder.Sql("DROP SEQUENCE IF EXISTS dbo.Seq_NotaVenta;");
            migrationBuilder.Sql("DROP SEQUENCE IF EXISTS dbo.Seq_NotaCredito;");
        }
    }
}
