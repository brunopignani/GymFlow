-- ==============================================================================
-- Script 05: Persona.UsuarioId opcional, único por Usuario
-- GymFlow - alternativa a la migración EF
--
-- Migración equivalente:
-- 20261113160336_CorregirRelacionPersonaUsuario
-- ProductVersion: 8.0.10
--
-- No ejecutar junto con la migración EF sobre la misma base.
-- No resuelve duplicados automáticamente ni registra historial a ciegas.
-- ==============================================================================

USE GymFlowDB;
GO

BEGIN TRANSACTION;

BEGIN TRY
    DECLARE @MigrationId nvarchar(150) =
        N'20261113160336_CorregirRelacionPersonaUsuario';
    DECLARE @ProductVersion nvarchar(64) = N'8.0.10';
    DECLARE @TieneHistorial bit = 0;
    DECLARE @TieneIndiceNoUnico bit = 0;
    DECLARE @TieneIndiceUnicoFiltrado bit = 0;
    DECLARE @Detalle nvarchar(max);

    IF OBJECT_ID(N'dbo.__EFMigrationsHistory', N'U') IS NULL
    BEGIN
        THROW 51100, 'Error: no existe dbo.__EFMigrationsHistory. Revise el esquema antes de continuar.', 1;
    END

    IF OBJECT_ID(N'dbo.Personas', N'U') IS NULL
    BEGIN
        THROW 51101, 'Error: no existe dbo.Personas. Revise el esquema antes de continuar.', 1;
    END

    IF NOT EXISTS (
        SELECT 1
        FROM sys.columns
        WHERE object_id = OBJECT_ID(N'dbo.Personas')
          AND name = N'UsuarioId'
          AND is_nullable = 1
    )
    BEGIN
        THROW 51102, 'Error: dbo.Personas.UsuarioId no existe como columna nullable.', 1;
    END

    IF EXISTS (
        SELECT 1
        FROM dbo.__EFMigrationsHistory
        WHERE MigrationId = @MigrationId
          AND ProductVersion <> @ProductVersion
    )
    BEGIN
        THROW 51103, 'Error: la migracion ya figura en __EFMigrationsHistory con otra version de EF Core.', 1;
    END

    IF EXISTS (
        SELECT 1
        FROM dbo.__EFMigrationsHistory
        WHERE MigrationId = @MigrationId
          AND ProductVersion = @ProductVersion
    )
        SET @TieneHistorial = 1;

    IF EXISTS (
        SELECT 1
        FROM sys.indexes i
        JOIN sys.index_columns ic
          ON ic.object_id = i.object_id
         AND ic.index_id = i.index_id
        JOIN sys.columns c
          ON c.object_id = ic.object_id
         AND c.column_id = ic.column_id
        WHERE i.object_id = OBJECT_ID(N'dbo.Personas')
          AND i.name = N'IX_Personas_UsuarioId'
          AND i.is_unique = 0
          AND c.name = N'UsuarioId'
    )
        SET @TieneIndiceNoUnico = 1;

    IF EXISTS (
        SELECT 1
        FROM sys.indexes i
        JOIN sys.index_columns ic
          ON ic.object_id = i.object_id
         AND ic.index_id = i.index_id
        JOIN sys.columns c
          ON c.object_id = ic.object_id
         AND c.column_id = ic.column_id
        WHERE i.object_id = OBJECT_ID(N'dbo.Personas')
          AND i.name = N'IX_Personas_UsuarioId'
          AND i.is_unique = 1
          AND i.has_filter = 1
          AND i.filter_definition = N'([UsuarioId] IS NOT NULL)'
          AND c.name = N'UsuarioId'
    )
        SET @TieneIndiceUnicoFiltrado = 1;

    IF @TieneHistorial = 1 AND @TieneIndiceUnicoFiltrado = 1
    BEGIN
        PRINT 'La migracion Persona-Usuario ya esta aplicada. No se realizaron cambios.';
        COMMIT TRANSACTION;
        RETURN;
    END

    IF @TieneHistorial = 1
    BEGIN
        THROW 51104, 'Error: la migracion figura en el historial, pero el indice unico esperado no existe. Revise manualmente.', 1;
    END

    IF @TieneIndiceUnicoFiltrado = 1
    BEGIN
        THROW 51105, 'Error: el indice unico ya existe, pero falta la entrada de la migracion en __EFMigrationsHistory. No se registra automaticamente.', 1;
    END

    IF @TieneIndiceNoUnico = 0
    BEGIN
        THROW 51106, 'Error: no existe el indice no unico IX_Personas_UsuarioId esperado por la migracion.', 1;
    END

    SELECT @Detalle = STUFF((
        SELECT N'; UsuarioId=' + CONVERT(nvarchar(20), p.UsuarioId)
            + N', PersonaIds='
            + STUFF((
                SELECT N',' + CONVERT(nvarchar(20), p2.Id)
                FROM dbo.Personas p2
                WHERE p2.UsuarioId = p.UsuarioId
                ORDER BY p2.Id
                FOR XML PATH(N''), TYPE
            ).value(N'.', N'nvarchar(max)'), 1, 1, N'')
        FROM dbo.Personas p
        WHERE p.UsuarioId IS NOT NULL
        GROUP BY p.UsuarioId
        HAVING COUNT(*) > 1
        ORDER BY p.UsuarioId
        FOR XML PATH(N''), TYPE
    ).value(N'.', N'nvarchar(max)'), 1, 2, N'');

    IF @Detalle IS NOT NULL
    BEGIN
        SELECT p.UsuarioId, p.Id AS PersonaId
        FROM dbo.Personas p
        WHERE p.UsuarioId IN (
            SELECT UsuarioId
            FROM dbo.Personas
            WHERE UsuarioId IS NOT NULL
            GROUP BY UsuarioId
            HAVING COUNT(*) > 1
        )
        ORDER BY p.UsuarioId, p.Id;

        DECLARE @Mensaje nvarchar(2047) =
            LEFT(N'La migracion se detuvo por duplicados Persona-Usuario:' + @Detalle, 2047);
        THROW 51107, @Mensaje, 1;
    END

    DROP INDEX IX_Personas_UsuarioId ON dbo.Personas;

    CREATE UNIQUE INDEX IX_Personas_UsuarioId
        ON dbo.Personas (UsuarioId)
        WHERE UsuarioId IS NOT NULL;

    INSERT INTO dbo.__EFMigrationsHistory (MigrationId, ProductVersion)
    VALUES (@MigrationId, @ProductVersion);

    PRINT 'Migracion Persona-Usuario aplicada correctamente.';
    COMMIT TRANSACTION;
END TRY
BEGIN CATCH
    IF @@TRANCOUNT > 0
        ROLLBACK TRANSACTION;

    PRINT 'Error durante la migracion Persona-Usuario.';
    THROW;
END CATCH;
GO
