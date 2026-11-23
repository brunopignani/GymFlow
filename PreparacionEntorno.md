# Preparación del entorno de pruebas (en tu PC con Windows)

Requisitos: SDK de .NET 8, SQL Server (LocalDB, Express o Developer) y acceso a internet para restaurar NuGet.

## 1. Compilar y correr los tests
```powershell
dotnet restore GymFlow.sln
dotnet build GymFlow.sln
dotnet test GymFlow.Tests/GymFlow.Tests.csproj
```

## 2. Apuntar a una base de PRUEBAS (nunca a la real)
La aplicación y `dotnet ef` leen la variable de entorno `GYMFLOW_CADENA_CONEXION`. Si no existe, `ConfiguracionBD.cs` utiliza la cadena predeterminada apuntando a `GymFlowDB_Pruebas`:
```text
Server=.\SQLEXPRESS;Database=GymFlowDB_Pruebas;Trusted_Connection=True;TrustServerCertificate=True;
```

Para sobreescribirla en PowerShell (válida solo para esa ventana):
```powershell
$env:GYMFLOW_CADENA_CONEXION = "Server=localhost;Database=GymFlowDB_Pruebas;Trusted_Connection=True;TrustServerCertificate=True;"
```
LocalDB: `Server=(localdb)\MSSQLLocalDB;Database=GymFlowDB_Pruebas;Trusted_Connection=True;`

El nombre de la base debe contener `Pruebas`: los scripts de datos se niegan a correr en cualquier otra.

## 3. Actualizar una base existente con EF Core y scripts SQL

El proyecto no tenía migraciones iniciales de baseline: las migraciones de `GymFlow.ContextoBD/Migrations` son incrementales y presuponen que el esquema base ya fue provisionado. No usarlas para inicializar una base vacía ni generar una migración `EsquemaInicial` contra una base existente.

Para aplicar todas las migraciones pendientes con EF Core (con `GYMFLOW_CADENA_CONEXION` apuntando a la base correcta):
```powershell
dotnet ef database update --project GymFlow.ContextoBD --startup-project GymFlow.InterfazUsuario
```

Orden de las migraciones del proyecto:

### 3.1. `20261102200704_ActualizacionNotasCreditoYFechaVencimiento`
Presupone que ya existen `T_NOTA_CREDITO`, `T_NOTA_VENTA` y `CuotasCliente`. Agrega `T_NOTA_CREDITO.NotaVentaId` (`INT NOT NULL`, FK `NO_ACTION` hacia `T_NOTA_VENTA(Id)`, índice `IX_T_NOTA_CREDITO_NotaVentaId`) y permite valores `NULL` en `CuotasCliente.FechaVencimiento` conservando `DATETIME2`.

Alternativas (elegir solo una, nunca ambas sobre la misma base):
- **Opción A — EF Core:** `dotnet ef database update ...`
- **Opción B — Script SQL:** ejecutar `Scripts/03_VincularNotasCreditoNotaVenta.sql` una sola vez en SSMS o `sqlcmd` (registra la migración en `__EFMigrationsHistory`).

La migración aborta sin modificar el esquema si encuentra notas de crédito históricas, una columna `NotaVentaId` preexistente o una definición distinta de la esperada para `FechaVencimiento`.

### 3.2. `20261113130000_ActualizarCapacidadMaximaActividad`
Renombra `Actividades.CupoMaximo` a `CapacidadMaxima` y convierte los datos sumando la cantidad de inscriptos actuales (`CapacidadMaxima = CupoMaximo_anterior + Inscriptos`), abortando si alguna actividad superara el tope de 30.

La migración EF y `Scripts/04_MigracionCapacidadActividad.sql` son alternativas (no ejecutar ambas).
> **Atención:** `Scripts/04_MigracionCapacidadActividad.sql` comienza con `USE GymFlowDB;`; si se usa en una base de pruebas, revisar y ajustar la base destino antes de ejecutarlo.

### 3.3. `20261113160336_CorregirRelacionPersonaUsuario`
Reemplaza el índice no único `IX_Personas_UsuarioId` por un índice único filtrado (`WHERE [UsuarioId] IS NOT NULL`) para garantizar la relación `1:0..1` entre `Persona` y `Usuario`, abortando si detecta duplicados.

La migración EF y `Scripts/05_CorregirRelacionPersonaUsuario.sql` son alternativas (no ejecutar ambas).
> **Atención:** `Scripts/05_CorregirRelacionPersonaUsuario.sql` también comienza con `USE GymFlowDB;`; verificar la base destino antes de ejecutarlo.

### 3.4. `20261115191151_AgregarPeriodoYNumeracionSegura`
Valida los números históricos de `T_COMPROBANTE`, agrega `T_NOTA_VENTA.Periodo` (`date`) con índice único `(CuotaClienteId, Periodo)`, cambia `T_COMPROBANTE.Numero` a `nvarchar(11) NOT NULL` único y crea las secuencias SQL Server `dbo.Seq_ReciboPago`, `dbo.Seq_NotaVenta` y `dbo.Seq_NotaCredito` con inicio dinámico desde los máximos históricos.
> **Importante:** Esta migración incluye una guarda que exige `DB_NAME() = N'GymFlowDB_Pruebas'` y aborta en cualquier otra base.

### 3.5. `20261123154325_EliminarEntidadPago`
Elimina la tabla obsoleta `Pagos` (con sus FKs e índices). Incluye una validación previa que aborta si `dbo.Pagos` contiene registros históricos.
> **Importante:** No aplicar esta migración sobre las bases de demo sin autorización explícita.

## 4. Cargar datos de prueba y probar flujos

Con el esquema y migraciones actualizados, ejecutar en SSMS o `sqlcmd` sobre `GymFlowDB_Pruebas`:

1. `Scripts/01_DatosIniciales.sql`: carga los tipos de movimiento `DEBITO` y `CREDITO` y la cuota `Mensual` (`15000.00`).
2. Abrir la aplicación → **Registrarse** (crea el primer empleado y usuario activo) → iniciar sesión.
3. **Gestión → Gestionar clientes**: dar de alta los clientes de prueba.
   - El alta crea automáticamente su `CuentaCorriente` y una asignación inicial `CuotaCliente` en estado **pendiente** (`FechaVencimiento = NULL`, `Estado = NULL`), sin generar deuda hasta que se cobre o se emita el débito mensual.
   - **No ejecutes `Scripts/02_AsignarCuotaACliente.sql` sobre clientes dados de alta desde la aplicación**, ya que ese script hace un `INSERT INTO CuotasCliente` y generaría una segunda fila `CuotaCliente` duplicada. Usá `Scripts/02_AsignarCuotaACliente.sql` únicamente si insertaste un cliente manualmente por SQL sin cuota asignada.
   - Para activar la cuota de un cliente por 30 días seguí el flujo normal desde **Finanzas → Registrar cobro** (emite la `NotaVenta` del período si no existe, el `ReciboPago` y actualiza la cuota a `ACTIVO`).
   - Si necesitás simular una cuota vencida para probar morosidad o renovación en la base de pruebas, actualizá directamente la fila existente en `CuotasCliente` con un `UPDATE` (por ejemplo, `FechaVencimiento = DATEADD(DAY, -5, CAST(GETDATE() AS DATE))` y `Estado = 'VENCIDO'`) en lugar de insertar una fila nueva.
4. **Gestión → Gestionar actividades**: crear las actividades con capacidad entre 1 y 30.

## 5. Volver a empezar
Borrá la base de pruebas desde SSMS (`DROP DATABASE GymFlowDB_Pruebas`), provisioná una base limpia con el esquema base y repetí desde el paso 4. No se borran bases desde el código.
