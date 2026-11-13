-- ==============================================================================
-- Script 04: Migración de Capacidad de Actividades (CupoMaximo -> CapacidadMaxima)
-- Gimnasio GymFlow - NO EJECUTAR DIRECTAMENTE SOBRE BASE DE PRODUCCIÓN SIN REVISIÓN
-- Este script es una alternativa a la migración EF
-- 20261113130000_ActualizarCapacidadMaximaActividad y registra esa migración
-- en __EFMigrationsHistory solamente después de completar el cambio.
-- ==============================================================================

USE GymFlowDB;
GO

BEGIN TRANSACTION;

BEGIN TRY
    DECLARE @MigrationId nvarchar(150) = N'20261113130000_ActualizarCapacidadMaximaActividad';
    DECLARE @ProductVersion nvarchar(64) = N'8.0.10';
    DECLARE @TieneCupoMaximo BIT = 0;
    DECLARE @TieneCapacidadMaxima BIT = 0;
    DECLARE @TieneRegistroMigracion BIT = 0;

    IF OBJECT_ID(N'dbo.__EFMigrationsHistory', N'U') IS NULL
    BEGIN
        THROW 51000, 'Error: no existe dbo.__EFMigrationsHistory. No se puede dejar la base sincronizada con EF Core.', 1;
    END

    IF EXISTS (SELECT 1 FROM sys.columns WHERE object_id = OBJECT_ID(N'dbo.Actividades') AND name = 'CupoMaximo')
        SET @TieneCupoMaximo = 1;

    IF EXISTS (SELECT 1 FROM sys.columns WHERE object_id = OBJECT_ID(N'dbo.Actividades') AND name = 'CapacidadMaxima')
        SET @TieneCapacidadMaxima = 1;

    IF EXISTS (
        SELECT 1
        FROM dbo.__EFMigrationsHistory
        WHERE MigrationId = @MigrationId
    )
        SET @TieneRegistroMigracion = 1;

    -- Casos ambiguos o inválidos: deben detenerse sin modificar datos.
    IF @TieneCupoMaximo = 1 AND @TieneCapacidadMaxima = 1
    BEGIN
        THROW 51001, 'Error: Actividades contiene CupoMaximo y CapacidadMaxima. No se puede determinar el estado de la migracion.', 1;
    END

    IF @TieneCupoMaximo = 0 AND @TieneCapacidadMaxima = 0
    BEGIN
        THROW 51002, 'Error: Actividades no contiene CupoMaximo ni CapacidadMaxima. Verifique la estructura de la base de datos.', 1;
    END

    IF @TieneRegistroMigracion = 1 AND @TieneCupoMaximo = 1 AND @TieneCapacidadMaxima = 0
    BEGIN
        THROW 51003, 'Error: __EFMigrationsHistory indica que la migracion ya fue aplicada, pero Actividades aun tiene CupoMaximo.', 1;
    END

    -- Caso de idempotencia: ya fue migrada previamente
    IF @TieneCupoMaximo = 0 AND @TieneCapacidadMaxima = 1
    BEGIN
        IF @TieneRegistroMigracion = 0
        BEGIN
            THROW 51004, 'Error: Actividades ya tiene CapacidadMaxima, pero falta la entrada de la migracion en __EFMigrationsHistory. Verifique el esquema y el historial antes de registrar nada manualmente.', 1;
        END

        PRINT 'La columna CupoMaximo ya fue migrada a CapacidadMaxima previamente. No se reaplicará la conversión.';
        COMMIT TRANSACTION;
        RETURN;
    END

    -- 2. Validación estricta previa: no truncar capacidades resultantes.
    DECLARE @Detalle nvarchar(max);

    SELECT @Detalle = STUFF((
        SELECT '; Id=' + CONVERT(nvarchar(20), a.Id)
            + ', Nombre=' + COALESCE(a.Nombre, '<sin nombre>')
            + ', CupoMaximo=' + CONVERT(nvarchar(20), a.CupoMaximo)
            + ', Inscriptos=' + CONVERT(nvarchar(20), ISNULL(i.TotalInscriptos, 0))
            + ', Resultado=' + CONVERT(nvarchar(20), a.CupoMaximo + ISNULL(i.TotalInscriptos, 0))
        FROM dbo.Actividades a
        LEFT JOIN (
            SELECT ActividadId, COUNT(*) AS TotalInscriptos
            FROM dbo.Inscripciones
            GROUP BY ActividadId
        ) i ON a.Id = i.ActividadId
        WHERE a.CupoMaximo + ISNULL(i.TotalInscriptos, 0) > 30
        ORDER BY a.Id
        FOR XML PATH(''), TYPE
    ).value('.', 'nvarchar(max)'), 1, 2, '');

    IF @Detalle IS NOT NULL
    BEGIN
        PRINT 'ERROR: Se encontraron actividades cuya capacidad resultante supera el máximo permitido de 30:';

        SELECT
            a.Id AS ActividadId,
            a.Nombre AS Actividad,
            a.CupoMaximo AS CuposAnteriores,
            ISNULL(i.TotalInscriptos, 0) AS InscriptosActuales,
            (a.CupoMaximo + ISNULL(i.TotalInscriptos, 0)) AS CapacidadResultante
        FROM dbo.Actividades a
        LEFT JOIN (
            SELECT ActividadId, COUNT(*) AS TotalInscriptos
            FROM dbo.Inscripciones
            GROUP BY ActividadId
        ) i ON a.Id = i.ActividadId
        WHERE (a.CupoMaximo + ISNULL(i.TotalInscriptos, 0)) > 30;

        DECLARE @Mensaje nvarchar(2047) =
            LEFT('La migración se detuvo sin modificar datos. Actividades afectadas:' + @Detalle, 2047);
        THROW 51002, @Mensaje, 1;
    END

    -- 3. Renombre y conversión de datos en una única operación transaccional.
    PRINT 'Renombrando columna CupoMaximo a CapacidadMaxima...';
    EXEC sp_rename 'dbo.Actividades.CupoMaximo', 'CapacidadMaxima', 'COLUMN';

    PRINT 'Convirtiendo datos: CapacidadMaxima = CupoMaximo_anterior + Inscriptos_Actuales...';
    UPDATE a
    SET a.CapacidadMaxima = a.CapacidadMaxima + ISNULL(i.TotalInscriptos, 0)
    FROM dbo.Actividades a
    LEFT JOIN (
        SELECT ActividadId, COUNT(*) AS TotalInscriptos
        FROM dbo.Inscripciones
        GROUP BY ActividadId
    ) i ON a.Id = i.ActividadId;

    INSERT INTO dbo.__EFMigrationsHistory (MigrationId, ProductVersion)
    VALUES (@MigrationId, @ProductVersion);

    PRINT 'Migración de Capacidad completada exitosamente.';
    COMMIT TRANSACTION;
END TRY
BEGIN CATCH
    IF @@TRANCOUNT > 0
        ROLLBACK TRANSACTION;

    PRINT 'Error durante la ejecución del script de migración.';
    THROW;
END CATCH;
GO
