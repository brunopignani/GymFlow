# GymFlow - handover para continuidad

## 1. Resumen y arquitectura

GymFlow es un sistema universitario de gestión de gimnasio. La solución usa C# con .NET 8, Windows Forms, Entity Framework Core 8 y SQL Server. La arquitectura es N-Capas tradicional: dominio, contratos, aplicación, persistencia, interfaz y pruebas. La solución está en formato de Visual Studio 2022: `GymFlow.sln` declara `VisualStudioVersion = 17.0`, por lo que es compatible con Visual Studio 2022 con el workload de desarrollo de escritorio .NET y el SDK de .NET 8.

Los siete proyectos son:

- `GymFlow.Dominio`: entidades persistentes y relaciones de dominio (`Cliente`, `Empleado`, `Persona`, `Usuario`, `Cuota`, `CuotaCliente`, `CuentaCorriente`, `Movimiento`, `TipoMovimiento`, `Inscripcion`, `Actividad`, `Pago` y comprobantes).
- `GymFlow.Abstracciones`: interfaces de repositorios y de transacción (`IClienteRepositorio`, `ICuotaClienteRepositorio`, `IComprobanteRepositorio`, `IMovimientoRepositorio`, `ITransaccionBD`, entre otras).
- `GymFlow.ContextoBD`: `GymFlowDbContext`, configuración Fluent API, estrategia TPT y migraciones EF Core.
- `GymFlow.Repositorio`: implementaciones concretas de acceso a datos con EF Core.
- `GymFlow.Aplicacion`: servicios con reglas de negocio (`ClienteServicio`, `CobroServicio`, `DebitoMensualServicio`, `NotaCreditoServicio`, `InscripcionServicio`, `UsuarioServicio`, etc.).
- `GymFlow.InterfazUsuario`: aplicación WinForms, formularios, composición de dependencias, sesión y configuración de conexión.
- `GymFlow.Tests`: pruebas MSTest con repositorios y transacciones en memoria.

Los proyectos de código apuntan a `net8.0`; la UI apunta a `net8.0-windows` y usa `UseWindowsForms`. EF Core y el proveedor SQL Server están en la línea 8.0.10; la inyección usa `Microsoft.Extensions.DependencyInjection` 8.0.1. El proyecto de pruebas usa MSTest 3.6.1 y `Microsoft.NET.Test.Sdk` 17.11.1.

## 2. Mapa del código

El punto de composición es `GymFlow.InterfazUsuario/Program.cs`. Registra `GymFlowDbContext` con SQL Server, todos los repositorios por sus interfaces, los servicios de aplicación y los formularios. `ConfiguracionBD.cs` lee `GYMFLOW_CADENA_CONEXION`; si no existe, usa `Server=.\SQLEXPRESS;Database=GymFlowDB;Trusted_Connection=True;TrustServerCertificate=True;`.

`GymFlow.ContextoBD/GymFlowDbContext.cs` declara los `DbSet` y configura:

- herencia TPT para `Persona`/`Cliente`/`Empleado`;
- herencia TPT para `Comprobante`/`ReciboPago`/`NotaCredito`/`NotaVenta`;
- cuenta corriente, movimientos, tipos, cuotas, asignaciones e inscripciones;
- `NotaCredito.NotaVentaId` con FK, índice y `DeleteBehavior.Restrict`;
- `CuotaCliente.FechaVencimiento` como nullable por el tipo `DateTime?`.

Las migraciones están en `GymFlow.ContextoBD/Migrations`; también existe el script `Scripts/03_VincularNotasCreditoNotaVenta.sql`. El repositorio contiene la migración de actualización de notas de crédito y vencimiento; la base verificada anteriormente tenía además un historial inicial, por lo que el historial debe tratarse como un punto operativo a confirmar antes de nuevas migraciones.

Los repositorios principales son `ClienteRepositorio`, `EmpleadoRepositorio`, `UsuarioRepositorio`, `ActividadRepositorio`, `InscripcionRepositorio`, `CuotaRepositorio`, `CuotaClienteRepositorio`, `CuentaCorrienteRepositorio`, `MovimientoRepositorio`, `ComprobanteRepositorio`, `TipoMovimientoRepositorio` y `PagoRepositorio`. `TransaccionBD` encapsula `BeginTransaction`, `Commit`, `Rollback` y limpieza del `ChangeTracker`.

## 3. Procesos principales

**Clientes y cuotas.** `ClienteServicio.AddCliente` valida datos y DNI, exige que exista exactamente una cuota llamada `Mensual`, crea una `CuentaCorriente` y una asignación `CuotaCliente` pendiente, sin vencimiento, estado ni movimiento. `FrmClientes` gestiona altas y modificaciones. La cuota pendiente se muestra en Recepción como “Pendiente de pago”.

**Actividades e inscripciones.** `ActividadServicio` y `ActividadRepositorio` administran actividades y cupos. `InscripcionServicio.InscribirCliente` exige cuota con vencimiento vigente, evita duplicados, reduce el cupo y guarda la inscripción en una transacción. `DesinscribirCliente` elimina la inscripción y libera cupo. `FrmRecepcion`, `FrmDesinscripcion`, `FrmActividades` y `FrmClientesPorActividad` exponen estas operaciones.

**Autoingreso y estado.** `ClienteServicio.ValidarAutoingreso` permite el acceso solo con cuota y vencimiento no menor que la fecha actual. `GetEstadoClientes` distingue cuota inexistente, pendiente y vigencia usando la fecha nullable. Los formularios son `FrmAutoingreso` y `FrmEstadoClientes`.

**Morosidad.** `InscripcionServicio.DarDeBajaInscripcionesDeMorosos` recorre clientes, detecta cuotas vencidas y elimina sus inscripciones liberando cupos. El acceso es `FrmBajaMorosos`. No está confirmado mediante una prueba funcional del usuario que este caso falle con una cuota realmente vencida.

**Cuenta corriente.** `CuentaCorrienteServicio.ConsultarCuentaCorriente` obtiene la cuenta y sus movimientos. El saldo suma débitos y resta créditos según `TipoMovimiento`. `FrmCuentaCorriente` presenta el historial y distingue `NotaVenta`, `ReciboPago` y `NotaCredito`.

**Cobros.** `FrmCobro` llama a `CobroServicio.RegistrarCobro`, que valida importe, medio de pago, empleado, cuenta, cuota y tipo `CREDITO`. Crea `ReciboPago`, lo guarda mediante `IComprobanteRepositorio`, crea un `Movimiento` de crédito y lo guarda mediante `IMovimientoRepositorio`. Si el saldo final es `<= 0`, actualiza la cuota a `ACTIVO` y fija 30 días. La operación usa una transacción.

**Débitos.** `FrmDebitoMensual` llama a `DebitoMensualServicio.EmitirDebitosMensuales`. Por cliente obtiene cuenta, `CuotaCliente`, cuota y precio; verifica en los movimientos si ya existe una `NotaVenta` del mes. `EmitirDebito` crea la nota, la guarda como comprobante, crea el movimiento `DEBITO` asociado por `ComprobanteId` y lo guarda en la misma transacción por cliente. El formulario muestra emitidos, ya emitidos y no procesados.

**Notas de crédito.** `FrmNotaCredito` selecciona una `NotaVenta` de la cuenta del cliente. `NotaCreditoServicio` valida pertenencia, usa transacción serializable, bloquea la venta, suma créditos previos, verifica que el acumulado no supere el total y registra `NotaCredito` con `NotaVentaId` y un movimiento de crédito.

**Seguridad.** `UsuarioServicio` autentica usuarios activos vinculados a empleados. `ClaveSegura` usa PBKDF2 con SHA-256, sal aleatoria de 16 bytes, hash de 32 bytes y 100.000 iteraciones. `SesionActual` mantiene empleado y usuario actuales; `FrmLogin`, `FrmRegistro`, `FrmUsuarios` y `FrmCambiarClave` gestionan el acceso. El estado del usuario se conserva para baja lógica.

## 4. Decisiones técnicas actuales

Se usan repositorios por interfaz y servicios de aplicación inyectados por DI. EF Core usa TPT para personas y comprobantes. Las operaciones que modifican varias entidades usan `ITransaccionBD`; no existe una unidad de trabajo independiente. Las consultas y altas se implementan con EF Core y `SaveChanges`.

La relación Persona–Usuario está configurada como `HasOne(p => p.Usuario).WithMany().IsRequired(false)`: `UsuarioId` es nullable y el modelo permite más de una persona referenciada por un usuario desde la configuración, aunque el diseño esperado pueda ser uno a uno. Cuenta corriente–Cliente sí está configurada como uno a uno. Nota de crédito–Nota de venta es muchos a uno desde créditos, con FK obligatoria y eliminación restrictiva.

## 5. Navegación actual

`FrmPrincipal` organiza:

- **Clientes:** gestionar clientes, estado de clientes y autoingreso.
- **Actividades:** gestionar actividades, Recepción, clientes por actividad, desinscripción y baja por morosidad.
- **Finanzas:** cuenta corriente, registrar cobro, débito mensual y nota de crédito.
- **Seguridad:** gestionar usuarios, cambiar contraseña y cerrar sesión.

`FrmRecepcion` también abre cuenta corriente, cobro, débito mensual, nota de crédito e inscripción. Por eso varias finanzas son accesibles desde el menú principal y desde Recepción. El botón de asignación manual de cuota fue eliminado; el alta asigna automáticamente la cuota Mensual.

## 6. Problemas y mejoras pendientes

1. Revisar la relación Persona–Usuario: `.WithMany()` no expresa el diseño uno a uno esperado.
2. La baja de un cliente nuevo puede quedar bloqueada porque la asignación de cuota pendiente ya cuenta como historial.
3. Las consultas que solo ordenan por `FechaVencimiento` pueden presentar una cuota pendiente de fecha `NULL` como vencida si la capa visual no distingue explícitamente el estado pendiente.
4. Validar funcionalmente los cobros mínimos: un crédito pequeño puede activar la cuota si el saldo resultante queda `<= 0`; la regla CU03 debe confirmarse con ejemplos reales.
5. Revisar validaciones de DNI, importes, medios de pago y mensajes de error.
6. Mejorar la experiencia de desinscripción y la consulta de inscriptos por actividad.
7. El alta de usuarios está separada de la administración posterior y requiere validar su integración funcional.
8. Conservar la baja lógica de usuarios; no convertirla en eliminación física.
9. La morosidad no está confirmada como defectuosa: falta una prueba del usuario con una cuota realmente vencida.
10. `Pago` permanece mapeado y probado, pero no es invocado por un flujo activo de la interfaz; el cobro operativo usa `ReciboPago`.

## 7. Objetivo de refactorización y punto de partida

La refactorización buscada es principalmente de organización de interfaz: reducir menús redundantes, unificar accesos relacionados y mejorar la organización visual. Debe conservar la arquitectura N-Capas, EF Core, SQL Server y .NET 8; no se debe migrar a .NET 10 ni reemplazar WinForms.

Prioridades iniciales:

1. Validar con el usuario las reglas de cuota pendiente, activación por saldo y baja de clientes nuevos.
2. Confirmar Persona–Usuario y las cardinalidades que deben reflejarse en modelo y diagramas.
3. Probar morosidad con datos vencidos reales.
4. Decidir qué accesos deben quedar en Recepción y cuáles exclusivamente en Finanzas/Actividades.
5. Determinar si `Pago` seguirá siendo un caso de uso soportado o solo una entidad histórica.
6. Revisar el historial real de migraciones antes de generar otra migración.

Riesgos principales: cambios en reglas financieras, diferencias entre la base real y el historial local, duplicación de operaciones desde dos formularios, bloqueo de clientes nuevos por asignaciones automáticas y confusión entre `Pago` y `ReciboPago`. Las decisiones anteriores requieren validación del usuario antes de tocar código o persistencia.

## Verificación y límites

Se revisaron la solución, los siete proyectos, archivos `.csproj`, `GymFlow.sln`, contexto EF, entidades, repositorios, servicios, formularios, DI, seguridad, migraciones y navegación. También se tomó como referencia la estructura SQL Server ya consultada durante la auditoría previa. No se volvió a ejecutar la aplicación ni la suite en esta documentación, no se modificó la base y no se verificó visualmente el comportamiento de cada formulario. Las observaciones de morosidad y de uso operativo de `Pago` deben confirmarse con pruebas funcionales antes de convertirlas en cambios.
