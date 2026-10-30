IF DB_NAME() NOT LIKE '%Pruebas%'
    THROW 50000, 'Este script es solo para bases de pruebas: el nombre de la base debe contener "Pruebas".', 1;

IF NOT EXISTS (SELECT 1 FROM T_TIPO_MOVIMIENTO WHERE Nombre = 'DEBITO')
    INSERT INTO T_TIPO_MOVIMIENTO (Nombre) VALUES ('DEBITO');

IF NOT EXISTS (SELECT 1 FROM T_TIPO_MOVIMIENTO WHERE Nombre = 'CREDITO')
    INSERT INTO T_TIPO_MOVIMIENTO (Nombre) VALUES ('CREDITO');

IF NOT EXISTS (SELECT 1 FROM Cuotas WHERE Nombre = 'Mensual')
    INSERT INTO Cuotas (Nombre, Precio) VALUES ('Mensual', 15000.00);

SELECT Id, Nombre FROM T_TIPO_MOVIMIENTO;
SELECT Id, Nombre, Precio FROM Cuotas;
