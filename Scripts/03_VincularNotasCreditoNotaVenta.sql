IF OBJECT_ID(N'[__EFMigrationsHistory]') IS NULL
BEGIN
    CREATE TABLE [__EFMigrationsHistory] (
        [MigrationId] nvarchar(150) NOT NULL,
        [ProductVersion] nvarchar(32) NOT NULL,
        CONSTRAINT [PK___EFMigrationsHistory] PRIMARY KEY ([MigrationId])
    );
END;
GO

BEGIN TRANSACTION;
GO

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20261102200704_ActualizacionNotasCreditoYFechaVencimiento'
)
BEGIN
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
END;
GO

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20261102200704_ActualizacionNotasCreditoYFechaVencimiento'
)
BEGIN
    ALTER TABLE [T_NOTA_CREDITO] ADD [NotaVentaId] int NOT NULL;
END;
GO

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20261102200704_ActualizacionNotasCreditoYFechaVencimiento'
)
BEGIN
    CREATE INDEX [IX_T_NOTA_CREDITO_NotaVentaId] ON [T_NOTA_CREDITO] ([NotaVentaId]);
END;
GO

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20261102200704_ActualizacionNotasCreditoYFechaVencimiento'
)
BEGIN
    ALTER TABLE [T_NOTA_CREDITO] ADD CONSTRAINT [FK_T_NOTA_CREDITO_T_NOTA_VENTA_NotaVentaId] FOREIGN KEY ([NotaVentaId]) REFERENCES [T_NOTA_VENTA] ([Id]);
END;
GO

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20261102200704_ActualizacionNotasCreditoYFechaVencimiento'
)
BEGIN
    ALTER TABLE [dbo].[CuotasCliente] ALTER COLUMN [FechaVencimiento] datetime2 NULL;
END;
GO

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20261102200704_ActualizacionNotasCreditoYFechaVencimiento'
)
BEGIN
    INSERT INTO [__EFMigrationsHistory] ([MigrationId], [ProductVersion])
    VALUES (N'20261102200704_ActualizacionNotasCreditoYFechaVencimiento', N'8.0.10');
END;
GO

COMMIT;
GO
