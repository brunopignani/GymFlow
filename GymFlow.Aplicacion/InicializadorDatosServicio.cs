using GymFlow.Abstracciones;
using GymFlow.Dominio;
using System;
using System.Collections.Generic;
using System.Linq;

namespace GymFlow.Aplicacion
{
    public class InicializadorDatosServicio
    {
        public const decimal PrecioInicialCuotaMensual = 15000m;

        private readonly ITipoMovimientoRepositorio _tipoMovimientoRepositorio;
        private readonly ICuotaRepositorio _cuotaRepositorio;
        private readonly IAccionRepositorio _accionRepositorio;
        private readonly IGrupoRepositorio _grupoRepositorio;
        private readonly ITransaccionBD _transaccion;

        private static readonly (string Modulo, (string Formulario, string Codigo, string NombreAccion)[] Acciones)[] CatalogoSeguridad =
        {
            (
                "Mostrador",
                new[]
                {
                    ("Inscripción a actividades", CodigosPermiso.InscripcionesGestionar, "Gestionar inscripciones"),
                    ("Autoingreso", CodigosPermiso.AutoingresoUsar, "Validar autoingreso")
                }
            ),
            (
                "Gestión",
                new[]
                {
                    ("Gestionar clientes", CodigosPermiso.ClientesGestionar, "Gestionar clientes"),
                    ("Gestionar actividades", CodigosPermiso.ActividadesGestionar, "Gestionar actividades"),
                    ("Precio de cuota mensual", CodigosPermiso.CuotaEditarPrecio, "Editar precio de cuota mensual")
                }
            ),
            (
                "Finanzas",
                new[]
                {
                    ("Registrar cobro", CodigosPermiso.CobroRegistrar, "Registrar cobros"),
                    ("Cuenta corriente", CodigosPermiso.CuentaCorrienteVer, "Consultar cuenta corriente"),
                    ("Nota de crédito", CodigosPermiso.NotaCreditoEmitir, "Emitir notas de crédito"),
                    ("Ajuste de cuenta", CodigosPermiso.AjusteCuentaRegistrar, "Registrar ajustes de cuenta"),
                    ("Débito mensual", CodigosPermiso.DebitoMensualEmitir, "Emitir débitos mensuales"),
                    ("Baja por morosidad", CodigosPermiso.MorososBaja, "Dar de baja inscripciones por morosidad")
                }
            ),
            (
                "Seguridad",
                new[]
                {
                    ("Gestionar usuarios", CodigosPermiso.UsuariosGestionar, "Gestionar usuarios"),
                    ("Gestionar grupos", CodigosPermiso.GruposGestionar, "Gestionar grupos"),
                    ("Auditoría", CodigosPermiso.AuditoriaVer, "Consultar auditoría del sistema"),
                    ("Resguardo y restauración", CodigosPermiso.RespaldoGestionar, "Generar resguardos y restaurar base de datos")
                }
            )
        };

        private static readonly string[] PermisosRecepcionista =
        {
            CodigosPermiso.ClientesGestionar,
            CodigosPermiso.ActividadesGestionar,
            CodigosPermiso.InscripcionesGestionar,
            CodigosPermiso.AutoingresoUsar,
            CodigosPermiso.CobroRegistrar,
            CodigosPermiso.CuentaCorrienteVer,
            CodigosPermiso.DebitoMensualEmitir,
            CodigosPermiso.MorososBaja
        };

        public InicializadorDatosServicio(
            ITipoMovimientoRepositorio tipoMovimientoRepositorio,
            ICuotaRepositorio cuotaRepositorio,
            IAccionRepositorio accionRepositorio,
            IGrupoRepositorio grupoRepositorio,
            ITransaccionBD transaccion)
        {
            _tipoMovimientoRepositorio = tipoMovimientoRepositorio;
            _cuotaRepositorio = cuotaRepositorio;
            _accionRepositorio = accionRepositorio;
            _grupoRepositorio = grupoRepositorio;
            _transaccion = transaccion;
        }

        public void Inicializar()
        {
            _transaccion.IniciarTransaccion();

            try
            {
                AsegurarTiposMovimiento();
                AsegurarCuotaMensual();
                List<Accion> accionesNuevas = AsegurarCatalogoSeguridad();
                AsegurarGruposBase(accionesNuevas);

                _transaccion.ConfirmarTransaccion();
            }
            catch
            {
                _transaccion.RevertirTransaccion();
                throw;
            }
        }

        private void AsegurarTiposMovimiento()
        {
            List<TipoMovimiento> existentes = _tipoMovimientoRepositorio.GetAllTiposMovimiento();

            foreach (string nombre in new[] { NombresTipoMovimiento.Debito, NombresTipoMovimiento.Credito })
            {
                bool existe = existentes.Any(t => string.Equals(t.Nombre, nombre, StringComparison.OrdinalIgnoreCase));
                if (!existe)
                {
                    TipoMovimiento nuevo = new TipoMovimiento { Nombre = nombre };
                    if (!_tipoMovimientoRepositorio.AddTipoMovimiento(nuevo))
                    {
                        throw new InvalidOperationException($"No se pudo crear el tipo de movimiento '{nombre}'.");
                    }
                    existentes.Add(nuevo);
                }
            }
        }

        private void AsegurarCuotaMensual()
        {
            List<Cuota> cuotas = _cuotaRepositorio.GetAllCuotas();
            bool existe = cuotas.Any(c => string.Equals(c.Nombre, CuotaServicio.NombreCuotaMensual, StringComparison.OrdinalIgnoreCase));

            if (!existe)
            {
                Cuota mensual = new Cuota
                {
                    Nombre = CuotaServicio.NombreCuotaMensual,
                    Precio = PrecioInicialCuotaMensual
                };

                if (!_cuotaRepositorio.AddCuota(mensual))
                {
                    throw new InvalidOperationException("No se pudo crear la cuota base 'Mensual'.");
                }
            }
        }

        private List<Accion> AsegurarCatalogoSeguridad()
        {
            List<Modulo> modulosExistentes = _accionRepositorio.GetAllModulosConFormulariosYAcciones();
            List<Accion> accionesNuevas = new List<Accion>();

            foreach (var definicionModulo in CatalogoSeguridad)
            {
                Modulo modulo = modulosExistentes.FirstOrDefault(
                    m => string.Equals(m.Nombre, definicionModulo.Modulo, StringComparison.OrdinalIgnoreCase));

                if (modulo == null)
                {
                    modulo = new Modulo { Nombre = definicionModulo.Modulo };
                    if (!_accionRepositorio.AddModulo(modulo))
                    {
                        throw new InvalidOperationException($"No se pudo crear el módulo '{definicionModulo.Modulo}'.");
                    }
                    modulosExistentes.Add(modulo);
                }

                foreach (var definicionAccion in definicionModulo.Acciones)
                {
                    Formulario formulario = modulo.Formularios.FirstOrDefault(
                        f => string.Equals(f.Nombre, definicionAccion.Formulario, StringComparison.OrdinalIgnoreCase));

                    if (formulario == null)
                    {
                        formulario = new Formulario
                        {
                            Nombre = definicionAccion.Formulario,
                            ModuloId = modulo.Id
                        };

                        if (!_accionRepositorio.AddFormulario(formulario))
                        {
                            throw new InvalidOperationException($"No se pudo crear el formulario '{definicionAccion.Formulario}'.");
                        }

                        if (!modulo.Formularios.Any(f => f.Id == formulario.Id))
                        {
                            modulo.Formularios.Add(formulario);
                        }
                    }

                    Accion accionExistente = _accionRepositorio.GetAccionByCodigo(definicionAccion.Codigo);
                    if (accionExistente == null)
                    {
                        Accion accion = new Accion
                        {
                            Codigo = definicionAccion.Codigo,
                            Nombre = definicionAccion.NombreAccion,
                            FormularioId = formulario.Id
                        };

                        if (!_accionRepositorio.AddAccion(accion))
                        {
                            throw new InvalidOperationException($"No se pudo crear la acción '{definicionAccion.Codigo}'.");
                        }

                        accionesNuevas.Add(accion);
                    }
                }
            }

            return accionesNuevas;
        }

        private void AsegurarGruposBase(List<Accion> accionesNuevas)
        {
            List<Accion> todasLasAcciones = _accionRepositorio.GetAllAccionesConFormularioYModulo();
            Grupo administradorExistente = _grupoRepositorio.GetGrupoByNombre(NombresGrupo.Administrador);

            if (administradorExistente == null)
            {
                Grupo administrador = new Grupo
                {
                    Nombre = NombresGrupo.Administrador,
                    Descripcion = "Acceso completo al sistema",
                    Estado = EstadosGrupo.Activo
                };

                foreach (var definicionModulo in CatalogoSeguridad)
                {
                    foreach (var definicionAccion in definicionModulo.Acciones)
                    {
                        Accion accion = todasLasAcciones.First(
                            a => string.Equals(a.Codigo, definicionAccion.Codigo, StringComparison.OrdinalIgnoreCase));
                        administrador.Acciones.Add(accion);
                    }
                }

                if (!_grupoRepositorio.AddGrupo(administrador))
                {
                    throw new InvalidOperationException("No se pudo crear el grupo base 'Administrador'.");
                }
            }
            else if (accionesNuevas != null && accionesNuevas.Count > 0)
            {
                Grupo administrador = _grupoRepositorio.GetGrupoByIdConRelaciones(administradorExistente.Id);

                if (administrador != null)
                {
                    bool modificado = false;

                    foreach (Accion nueva in accionesNuevas)
                    {
                        if (!administrador.Acciones.Any(a => a.Id == nueva.Id))
                        {
                            administrador.Acciones.Add(nueva);
                            modificado = true;
                        }
                    }

                    if (modificado && !_grupoRepositorio.UpdateGrupo(administrador))
                    {
                        throw new InvalidOperationException("No se pudo vincular las nuevas acciones al grupo 'Administrador'.");
                    }
                }
            }

            if (_grupoRepositorio.GetGrupoByNombre(NombresGrupo.Recepcionista) == null)
            {
                Grupo recepcionista = new Grupo
                {
                    Nombre = NombresGrupo.Recepcionista,
                    Descripcion = "Operaciones diarias de mostrador y cobros",
                    Estado = EstadosGrupo.Activo
                };

                foreach (string codigo in PermisosRecepcionista)
                {
                    Accion accion = todasLasAcciones.First(
                        a => string.Equals(a.Codigo, codigo, StringComparison.OrdinalIgnoreCase));
                    recepcionista.Acciones.Add(accion);
                }

                if (!_grupoRepositorio.AddGrupo(recepcionista))
                {
                    throw new InvalidOperationException("No se pudo crear el grupo base 'Recepcionista'.");
                }
            }
        }
    }
}
