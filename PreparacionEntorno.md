# Preparación del entorno de pruebas (en tu PC con Windows)

Requisitos: SDK de .NET 8, SQL Server (LocalDB, Express o Developer) y acceso a internet para restaurar NuGet.

## 1. Compilar y correr los tests
```
dotnet restore GymFlow.sln
dotnet build GymFlow.sln
dotnet test GymFlow.Tests/GymFlow.Tests.csproj
```

## 2. Apuntar a una base de PRUEBAS (nunca a la real)
La aplicación y `dotnet ef` leen la variable de entorno `GYMFLOW_CADENA_CONEXION`; si no existe, usan la cadena predeterminada (`GymFlowDB`).

PowerShell (valida solo para esa ventana):
```
$env:GYMFLOW_CADENA_CONEXION = "Server=localhost;Database=GymFlowDB_Pruebas;Trusted_Connection=True;TrustServerCertificate=True;"
```
LocalDB: `Server=(localdb)\MSSQLLocalDB;Database=GymFlowDB_Pruebas;Trusted_Connection=True;`

El nombre de la base debe contener `Pruebas`: los scripts de datos se niegan a correr en cualquier otra.

## 3. Generar y revisar la migración (todavía no existe ninguna)
```
dotnet tool install --global dotnet-ef --version 8.0.10
dotnet ef migrations add EsquemaInicial --project GymFlow.ContextoBD --startup-project GymFlow.InterfazUsuario
dotnet ef migrations script --idempotent --project GymFlow.ContextoBD --startup-project GymFlow.InterfazUsuario --output migracion.sql
```
Antes de aplicar, abrí `migracion.sql` y verificá:
- No hay `DROP TABLE` ni `DROP COLUMN`.
- Existen 16 tablas: Usuarios, Personas, Clientes, Empleados, Actividades, Cuotas, CuotasCliente, Inscripciones, Pagos, T_CUENTA_CORRIENTE, T_MOVIMIENTO, T_TIPO_MOVIMIENTO, T_COMPROBANTE, T_RECIBO_PAGO, T_NOTA_CREDITO, T_NOTA_VENTA.
- `Pagos` no tiene columna `ClienteId`.
- La FK `Pagos.EmpleadoId` no usa `ON DELETE CASCADE`.
- Los importes son `decimal(18,2)`.
- `Clientes`, `Empleados`, `T_RECIBO_PAGO`, `T_NOTA_CREDITO` y `T_NOTA_VENTA` no tienen `IDENTITY` en su `Id`.

## 4. Crear la base de pruebas y cargar datos
```
dotnet ef database update --project GymFlow.ContextoBD --startup-project GymFlow.InterfazUsuario
```
Luego, en SSMS o `sqlcmd`, conectado a la base de pruebas:
1. `Scripts/01_DatosIniciales.sql`: tipos `DEBITO` y `CREDITO` y una cuota de ejemplo.
2. Abrir la aplicación → **Registrarse** (crea el primer empleado) → iniciar sesión.
3. **Clientes → Gestionar clientes**: dar de alta los clientes de prueba (esto crea su cuenta corriente).
4. `Scripts/02_AsignarCuotaACliente.sql`: editar `@Dni` y `@DiasParaVencer` (negativo = cuota vencida) y ejecutar una vez por cliente.
5. **Actividades → Gestionar actividades**: crear actividades.

Si los scripts SQL fallan por un nombre de tabla o columna, el esquema real es el de `migracion.sql`: ajustalos a ese.

## 5. Volver a empezar
Borrá la base de pruebas desde SSMS (`DROP DATABASE GymFlowDB_Pruebas`) y repetí desde el paso 4. No se borra nada desde el código.
