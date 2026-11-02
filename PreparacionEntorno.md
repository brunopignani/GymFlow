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

## 3. Actualizar una base existente con el esquema anterior

La migración `ActualizacionNotasCreditoYFechaVencimiento` es incremental: presupone que ya existen las tablas `T_NOTA_CREDITO`, `T_NOTA_VENTA` y `CuotasCliente` con el esquema anterior. Agrega la relación obligatoria entre nota de crédito y nota de venta, y permite valores `NULL` en `CuotasCliente.FechaVencimiento` conservando `DATETIME2`.

El proyecto no tenía migraciones anteriores. Esta es la primera entrada del historial EF y **no crea el esquema completo**. No usarla para inicializar una base vacía ni generar una migración `EsquemaInicial` contra la base existente.

Antes de aplicarla, confirmar mediante consultas `SELECT` que:
- `T_NOTA_CREDITO` está vacía y todavía no tiene `NotaVentaId`.
- `CuotasCliente.FechaVencimiento` existe como `DATETIME2 NOT NULL`.
- La base seleccionada es la base existente que se desea actualizar.

Hay dos formas alternativas de aplicarla; elegir solo una:

**Opción A — EF Core:** con `GYMFLOW_CADENA_CONEXION` apuntando a la base correcta:
```
dotnet ef database update --project GymFlow.ContextoBD --startup-project GymFlow.InterfazUsuario
```

**Opción B — script SQL de EF:** generar o revisar `Scripts/03_VincularNotasCreditoNotaVenta.sql` y ejecutarlo una sola vez en SSMS o `sqlcmd`, conectado a la base correcta. El script idempotente generado por EF registra la migración en `__EFMigrationsHistory`.
```
dotnet ef migrations script --idempotent --project GymFlow.ContextoBD --startup-project GymFlow.InterfazUsuario --output Scripts\03_VincularNotasCreditoNotaVenta.sql
```
No ejecutar `database update` y el script para la misma base: ambos aplican la misma migración. La migración aborta sin modificar el esquema si encuentra notas de crédito históricas, una columna `NotaVentaId` preexistente o una definición distinta de la esperada para `FechaVencimiento`. No asigna ventas automáticamente.

Después, verificar que `CuotasCliente.FechaVencimiento` sea `DATETIME2 NULL`, que `T_NOTA_CREDITO.NotaVentaId` sea `INT NOT NULL`, y que exista la FK hacia `T_NOTA_VENTA(Id)` con acción `NO_ACTION`, el índice `IX_T_NOTA_CREDITO_NotaVentaId` y la entrada de migración en `__EFMigrationsHistory`.

## 4. Crear una base de pruebas y cargar datos

La migración anterior no crea las tablas de una base vacía. Para una base nueva se necesita primero provisionar el esquema inicial completo correspondiente al modelo; ese baseline todavía no está versionado en el proyecto. No marcar esta migración como aplicada manualmente para sustituir esa provisión.

Con el esquema completo creado y la actualización de la sección 3 aplicada cuando corresponda, en SSMS o `sqlcmd`:
1. `Scripts/01_DatosIniciales.sql`: tipos `DEBITO` y `CREDITO` y una cuota de ejemplo.
2. Abrir la aplicación → **Registrarse** (crea el primer empleado) → iniciar sesión.
3. **Clientes → Gestionar clientes**: dar de alta los clientes de prueba (esto crea su cuenta corriente).
4. `Scripts/02_AsignarCuotaACliente.sql`: editar `@Dni` y `@DiasParaVencer` (negativo = cuota vencida) y ejecutar una vez por cliente.
5. **Actividades → Gestionar actividades**: crear actividades.

Si los scripts SQL fallan por un nombre de tabla o columna, el esquema real es el de `migracion.sql`: ajustalos a ese.

## 5. Volver a empezar
Borrá la base de pruebas desde SSMS (`DROP DATABASE GymFlowDB_Pruebas`) y repetí desde el paso 4. No se borra nada desde el código.
