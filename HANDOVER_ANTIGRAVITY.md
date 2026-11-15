# GymFlow - traspaso a Antigravity

Fecha de respaldo: 2026-11-15

## 1. Descripción

GymFlow es un sistema universitario de gestión de gimnasio desarrollado en C# con .NET 8, WinForms, Entity Framework Core 8 y SQL Server. La solución usa una arquitectura N-Capas tradicional y está orientada a la administración de clientes, cuotas, actividades, inscripciones, cuenta corriente, cobros y comprobantes.

La solución contiene siete proyectos:

- `GymFlow.Dominio`: entidades persistentes y reglas estructurales del dominio.
- `GymFlow.Abstracciones`: interfaces de repositorios y transacciones.
- `GymFlow.ContextoBD`: `GymFlowDbContext`, mapeo Fluent API, TPT y migraciones.
- `GymFlow.Repositorio`: implementaciones EF Core de los repositorios.
- `GymFlow.Aplicacion`: servicios y reglas de negocio.
- `GymFlow.InterfazUsuario`: aplicación WinForms, composición DI, formularios y configuración.
- `GymFlow.Tests`: pruebas MSTest con repositorios y transacciones en memoria.

La solución está en `GymFlow.sln` y es compatible con Visual Studio 2022.

## 2. Arquitectura y convenciones

La dependencia principal es:

```text
Dominio <- Abstracciones <- Aplicacion
Dominio <- ContextoBD <- Repositorio
Aplicacion + Repositorio + ContextoBD <- InterfazUsuario
Dominio + Abstracciones + Aplicacion <- Tests
```

La capa Aplicación depende de `Abstracciones` y `Dominio`, no de EF Core. Las implementaciones concretas de persistencia están en `GymFlow.Repositorio`. La UI registra `DbContext`, repositorios y servicios mediante `Microsoft.Extensions.DependencyInjection`.

EF Core usa herencia TPT para:

- `Persona`, `Cliente` y `Empleado`.
- `Comprobante`, `ReciboPago`, `NotaVenta` y `NotaCredito`.

Las operaciones que modifican varias entidades usan `ITransaccionBD`. No existe una unidad de trabajo independiente ni se incorporaron patrones adicionales para esta tarea.

## 3. Tecnologías y dependencias

- .NET 8.
- C#.
- Windows Forms (`net8.0-windows`).
- Entity Framework Core 8.0.10.
- Proveedor SQL Server 8.0.10.
- `Microsoft.Extensions.DependencyInjection` 8.0.1.
- MSTest 3.6.1.
- `Microsoft.NET.Test.Sdk` 17.11.1.

La cadena predeterminada está en `GymFlow.InterfazUsuario/ConfiguracionBD.cs` y apunta a `GymFlowDB`. Para pruebas debe utilizarse explícitamente `GYMFLOW_CADENA_CONEXION` con una base cuyo nombre contenga `Pruebas`.

## 4. Modelo y funcionalidades

Entidades principales:

- `Cliente`, `Persona`, `Empleado`, `Usuario`.
- `Cuota` y `CuotaCliente`.
- `CuentaCorriente` y `Movimiento`.
- `Actividad` e `Inscripcion`.
- `Pago`.
- `Comprobante`, `ReciboPago`, `NotaVenta` y `NotaCredito`.

Relaciones importantes:

- Cliente tiene cuenta corriente uno a uno.
- Cliente puede tener varias asignaciones `CuotaCliente`; la aplicación utiliza la de mayor `Id` como actual.
- `NotaVenta` pertenece a una `CuotaCliente`.
- `NotaCredito` referencia obligatoriamente una `NotaVenta`.
- `Movimiento` referencia obligatoriamente un comprobante, una cuenta y un tipo de movimiento.

Funcionalidades implementadas:

- Alta de cliente con cuenta corriente y cuota mensual pendiente.
- Estados de cuota: sin cuota, pendiente, activo y vencido.
- Autoingreso según vencimiento vigente.
- Gestión de actividades y control de capacidad.
- Inscripciones, desinscripciones y baja de inscripciones de morosos.
- Cobros con recibo, movimiento crédito y activación por 30 días cuando el saldo final es `<= 0`.
- Notas de crédito parciales con límite acumulado contra la nota de venta.
- Débito mensual con detalle por cliente y estados emitido, ya emitido y no procesado.
- Cuenta corriente con movimientos y comprobantes descriptivos.
- Autenticación de usuarios, contraseñas con PBKDF2 y baja lógica.

## 5. Cambios recientes verificados

### Capacidad de actividades

`CapacidadMaxima` reemplazó el nombre anterior `CupoMaximo`. Se actualizaron `Actividad`, servicios, formularios, contexto, migración y script alternativo:

- `GymFlow.Dominio/Actividad.cs`
- `GymFlow.Aplicacion/ActividadServicio.cs`
- `GymFlow.InterfazUsuario/FrmActividades.cs`
- `GymFlow.ContextoBD/GymFlowDbContext.cs`
- `GymFlow.ContextoBD/Migrations/20261113130000_ActualizarCapacidadMaximaActividad.*`
- `Scripts/04_MigracionCapacidadActividad.sql`

### Relación Persona-Usuario

`Persona.UsuarioId` es nullable y se agregó la corrección de unicidad filtrada para valores no nulos:

- `GymFlow.Dominio/Persona.cs`
- `GymFlow.ContextoBD/GymFlowDbContext.cs`
- `GymFlow.ContextoBD/Migrations/20261113160336_CorregirRelacionPersonaUsuario.*`
- `Scripts/05_CorregirRelacionPersonaUsuario.sql`

### Cuotas y estados

`FechaVencimiento` es `DateTime?`. El estado efectivo se determina por fecha, no solamente por el texto almacenado en `CuotaCliente.Estado`. `ClienteServicio` selecciona la asignación más reciente y centraliza la clasificación.

Archivos principales:

- `GymFlow.Dominio/CuotaCliente.cs`
- `GymFlow.Aplicacion/ClienteServicio.cs`
- `GymFlow.Repositorio/CuotaClienteRepositorio.cs`
- `GymFlow.InterfazUsuario/FrmClientes.cs`
- `GymFlow.InterfazUsuario/FrmEstadoClientes.cs`

### Validaciones y baja segura

Las validaciones de DNI, nombres, teléfono y usuarios están centralizadas en `ValidacionesDatos.cs` y se reflejan en los formularios. `ClienteServicio.DeleteClienteConResultado` ejecuta una baja transaccional y rechaza clientes con cuotas activadas, inscripciones, movimientos, comprobantes, pagos o usuario asociado.

### Finanzas

`CobroServicio` registra `ReciboPago` y movimiento crédito, calcula el saldo como débitos menos créditos y activa la cuota durante 30 días cuando el saldo final es `<= 0`.

`NotaCreditoServicio`:

- identifica la nota de venta original;
- bloquea la venta dentro de una transacción serializable;
- acumula notas de crédito anteriores;
- rechaza superar el total original;
- guarda el motivo y el `NotaVentaId`.

`Pago` continúa mapeado y probado, pero no participa del flujo operativo de cobro, que utiliza `ReciboPago` y `Movimiento`.

### Débito mensual e idempotencia

`DebitoMensualServicio` procesa cada cliente en una transacción individual, crea una `NotaVenta` y un movimiento `DEBITO`, conserva los contadores y entrega detalle individual en `ResultadoDebitoMensual`.

La regla vigente es una nota por `CuotaCliente` y período mensual. La búsqueda actual se realiza directamente por `CuotaClienteId + Periodo`, sin cargar todos los movimientos de la cuenta para localizar la nota.

No se implementaron reintentos automáticos, recuperación especial de deadlocks, `IServiceScopeFactory`, `IDbContextFactory` ni cambios de aislamiento. La solución mantiene la complejidad existente.

## 6. Numeración y migración más reciente

La migración:

`GymFlow.ContextoBD/Migrations/20261115191151_AgregarPeriodoYNumeracionSegura.cs`

realiza lo siguiente:

- valida números históricos nulos, vacíos, inválidos, duplicados o mayores a 11 caracteres;
- agrega `NotaVenta.Periodo`;
- completa el período con el primer día del mes de `FechaEmision`;
- crea el índice único `(CuotaClienteId, Periodo)`;
- cambia `Comprobante.Numero` a `nvarchar(11)` obligatorio;
- crea el índice único de `Comprobante.Numero`;
- crea secuencias SQL Server con inicio dinámico calculado desde los máximos históricos.

Secuencias:

- `Seq_ReciboPago`: prefijo `RP-`.
- `Seq_NotaVenta`: prefijo `NV-`.
- `Seq_NotaCredito`: prefijo `NC-`.

En la auditoría de `GymFlowDB_Pruebas` los máximos fueron RP=30, NV=26 y NC=29; por eso los siguientes valores configurados son RP-00000031, NV-00000027 y NC-00000030.

La migración se aplicó exclusivamente en `GymFlowDB_Pruebas`. La migración contiene una protección para impedir su ejecución sobre otra base.

Los servicios consumen las secuencias a través de `IComprobanteRepositorio` y `ComprobanteRepositorio`:

- `CobroServicio`: `Seq_ReciboPago`.
- `DebitoMensualServicio`: `Seq_NotaVenta`.
- `NotaCreditoServicio`: `Seq_NotaCredito`.

No quedan generadores productivos basados en `COUNT + 1`.

Importante: cualquier otra base, incluida `GymFlowDB`, necesita aplicar la migración antes de utilizar `NotaVenta.Periodo`, los índices nuevos o los servicios que consumen las secuencias. No se modificó `GymFlowDB`.

## 7. Pruebas y comandos habituales

Resultado informado de la última validación:

- Build de la solución: correcto.
- Suite completa: **160/160 pruebas aprobadas**.
- No se ejecutaron nuevamente como parte de este respaldo, por instrucción expresa.

Comandos habituales documentados en `PreparacionEntorno.md`:

```powershell
dotnet restore GymFlow.sln
dotnet build GymFlow.sln
dotnet test GymFlow.Tests\GymFlow.Tests.csproj
```

Para una base de pruebas:

```powershell
$env:GYMFLOW_CADENA_CONEXION = "Server=.\SQLEXPRESS;Database=GymFlowDB_Pruebas;Trusted_Connection=True;TrustServerCertificate=True;"
```

La suite actual usa repositorios y transacciones en memoria. No existe todavía una prueba de concurrencia real con dos contextos SQL Server.

## 8. Pendientes

### Funcionales

- Confirmar con el profesor si la regla definitiva de facturación es por `CuotaCliente` y mes, o por cliente y mes.
- Validar manualmente el flujo de alta seguido del cobro inicial.
- Confirmar el comportamiento esperado de morosidad y baja de clientes nuevos.
- Definir si `Pago` seguirá siendo un caso de uso visible o permanecerá como entidad paralela.
- Confirmar la organización final de accesos entre Recepción, Finanzas y Actividades.

### Técnicos

- Aplicar la migración de numeración a las bases autorizadas antes de usar allí los servicios nuevos.
- Incorporar pruebas de integración SQL Server si se autoriza validar concurrencia real.
- Revisar el historial de migraciones cuando se prepare otra base.
- Documentar y verificar cualquier cambio futuro de numeración o período.

### Opcionales

- Optimizar consultas no críticas que todavía cargan colecciones completas.
- Crear reportes utilizando movimientos, comprobantes, clientes, cuotas e inscripciones.
- Mejorar mensajes y experiencia de algunos formularios después de la devolución académica.

No se recomiendan refactorizaciones generales ni patrones nuevos como parte de estos pendientes.

## 9. Restricciones para continuar

- No ejecutar escrituras sobre `GymFlowDB`.
- Antes de cualquier escritura SQL, verificar explícitamente `SELECT DB_NAME()` y confirmar `GymFlowDB_Pruebas`.
- No ejecutar migraciones sobre una base no verificada.
- No eliminar, renumerar ni modificar comprobantes históricos.
- No utilizar `EnsureDeleted`.
- No crear otra migración sin auditar primero el esquema y los datos.
- Mantener la arquitectura N-Capas y las dependencias actuales.
- No implementar reintentos, scopes nuevos o recuperación avanzada de deadlocks sin autorización específica.
- No modificar `HANDOVER.md`; este archivo es el documento separado de traspaso.

## 10. Alcance del respaldo

El ZIP `GymFlow_Backup_Antigravity_20261115.zip` contiene la solución, proyectos, código fuente, migraciones, scripts y documentación. Se excluyen `bin`, `obj`, `.vs`, `TestResults`, archivos de usuario locales y artefactos generados.

