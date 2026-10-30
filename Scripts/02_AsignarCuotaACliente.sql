IF DB_NAME() NOT LIKE '%Pruebas%'
    THROW 50000, 'Este script es solo para bases de pruebas: el nombre de la base debe contener "Pruebas".', 1;

DECLARE @Dni INT = 30111222;
DECLARE @DiasParaVencer INT = 10;

DECLARE @ClienteId INT = (SELECT p.Id FROM Personas p INNER JOIN Clientes c ON c.Id = p.Id WHERE p.Dni = @Dni);
DECLARE @CuotaId INT = (SELECT TOP 1 Id FROM Cuotas ORDER BY Id);

IF @ClienteId IS NULL
    THROW 50001, 'No existe un cliente con ese DNI. Primero denlo de alta desde la aplicación.', 1;

IF @CuotaId IS NULL
    THROW 50002, 'No hay cuotas cargadas. Ejecute primero 01_DatosIniciales.sql.', 1;

INSERT INTO CuotasCliente (ClienteId, CuotaId, FechaVencimiento, Estado)
VALUES (
    @ClienteId,
    @CuotaId,
    DATEADD(DAY, @DiasParaVencer, CAST(GETDATE() AS DATE)),
    CASE WHEN @DiasParaVencer >= 0 THEN 'ACTIVO' ELSE 'VENCIDO' END);

SELECT Id, ClienteId, CuotaId, FechaVencimiento, Estado FROM CuotasCliente WHERE ClienteId = @ClienteId;
