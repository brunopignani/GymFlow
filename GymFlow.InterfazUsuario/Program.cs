using GymFlow.Abstracciones;
using GymFlow.Aplicacion;
using GymFlow.ContextoBD;
using GymFlow.Repositorio;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;
using System;
using System.Diagnostics;
using System.Windows.Forms;

namespace GymFlow.InterfazUsuario
{
    internal static class Program
    {
        [STAThread]
        static void Main()
        {
            Application.EnableVisualStyles();
            Application.SetCompatibleTextRenderingDefault(false);

            ServiceCollection servicios = new ServiceCollection();

            servicios.AddDbContext<GymFlowDbContext>(options =>
                options.UseSqlServer(ConfiguracionBD.CadenaConexion));

            servicios.AddScoped<IContextoUsuarioActual, ContextoUsuarioActual>();

            servicios.AddScoped<IUsuarioRepositorio, UsuarioRepositorio>();
            servicios.AddScoped<IPersonaRepositorio, PersonaRepositorio>();
            servicios.AddScoped<IClienteRepositorio, ClienteRepositorio>();
            servicios.AddScoped<IEmpleadoRepositorio, EmpleadoRepositorio>();
            servicios.AddScoped<IActividadRepositorio, ActividadRepositorio>();
            servicios.AddScoped<IInscripcionRepositorio, InscripcionRepositorio>();
            servicios.AddScoped<ICuotaClienteRepositorio, CuotaClienteRepositorio>();
            servicios.AddScoped<ICuotaRepositorio, CuotaRepositorio>();
            servicios.AddScoped<ICuentaCorrienteRepositorio, CuentaCorrienteRepositorio>();
            servicios.AddScoped<IMovimientoRepositorio, MovimientoRepositorio>();
            servicios.AddScoped<ITipoMovimientoRepositorio, TipoMovimientoRepositorio>();
            servicios.AddScoped<IComprobanteRepositorio, ComprobanteRepositorio>();
            servicios.AddScoped<IAjusteCuentaRepositorio, AjusteCuentaRepositorio>();
            servicios.AddScoped<IGrupoRepositorio, GrupoRepositorio>();
            servicios.AddScoped<IAccionRepositorio, AccionRepositorio>();
            servicios.AddScoped<IAuditoriaRepositorio, AuditoriaRepositorio>();
            servicios.AddScoped<IRespaldoRepositorio>(_ => new RespaldoRepositorio(ConfiguracionBD.CadenaConexion));
            servicios.AddScoped<ITransaccionBD, TransaccionBD>();

            servicios.AddScoped<InscripcionServicio>();
            servicios.AddScoped<ClienteServicio>();
            servicios.AddScoped<ActividadServicio>();
            servicios.AddScoped<CuentaCorrienteServicio>();
            servicios.AddScoped<CobroServicio>();
            servicios.AddScoped<CobroFacade>();
            servicios.AddScoped<DebitoMensualServicio>();
            servicios.AddScoped<NotaCreditoServicio>();
            servicios.AddScoped<AjusteCuentaServicio>();
            servicios.AddScoped<UsuarioServicio>();
            servicios.AddScoped<GrupoServicio>();
            servicios.AddScoped<CuotaServicio>();
            servicios.AddScoped<AuditoriaServicio>();
            servicios.AddScoped<RespaldoServicio>();
            servicios.AddScoped<InicializadorDatosServicio>();

            servicios.AddTransient<FrmAdminInicial>();
            servicios.AddTransient<FrmLogin>();
            servicios.AddTransient<FrmPrincipal>();
            servicios.AddTransient<FrmAutoingreso>();
            servicios.AddTransient<FrmInscripciones>();
            servicios.AddTransient<FrmClientes>();
            servicios.AddTransient<FrmActividades>();
            servicios.AddTransient<FrmBajaMorosos>();
            servicios.AddTransient<FrmCuentaCorriente>();
            servicios.AddTransient<FrmCobro>();
            servicios.AddTransient<FrmDebitoMensual>();
            servicios.AddTransient<FrmNotaCredito>();
            servicios.AddTransient<FrmAjusteCuenta>();
            servicios.AddTransient<FrmUsuarios>();
            servicios.AddTransient<FrmGrupos>();
            servicios.AddTransient<FrmPrecioCuota>();
            servicios.AddTransient<FrmCambiarClave>();
            servicios.AddTransient<FrmAuditoria>();
            servicios.AddTransient<FrmRespaldo>();

            ServiceProviderOptions opciones = new ServiceProviderOptions();
            opciones.ValidateScopes = true;
            opciones.ValidateOnBuild = true;

            using (ServiceProvider proveedor = servicios.BuildServiceProvider(opciones))
            {
                using (IServiceScope alcanceInicial = proveedor.CreateScope())
                {
                    try
                    {
                        InicializadorDatosServicio inicializador = alcanceInicial.ServiceProvider.GetRequiredService<InicializadorDatosServicio>();
                        inicializador.Inicializar();
                    }
                    catch (Exception ex)
                    {
                        Debug.WriteLine(ex.ToString());
                        MessageBox.Show(
                            "Error crítico al inicializar los datos base del sistema. La aplicación se cerrará.\n\n" + ex.Message,
                            "GymFlow - Error de inicialización",
                            MessageBoxButtons.OK,
                            MessageBoxIcon.Error);
                        return;
                    }

                    UsuarioServicio usuarioServicio = alcanceInicial.ServiceProvider.GetRequiredService<UsuarioServicio>();
                    if (!usuarioServicio.ExisteAdministradorActivo())
                    {
                        using (FrmAdminInicial frmAdmin = alcanceInicial.ServiceProvider.GetRequiredService<FrmAdminInicial>())
                        {
                            if (frmAdmin.ShowDialog() != DialogResult.OK || !usuarioServicio.ExisteAdministradorActivo())
                            {
                                return;
                            }
                        }
                    }
                }

                while (true)
                {
                    using (IServiceScope alcance = proveedor.CreateScope())
                    {
                        using (FrmLogin login = alcance.ServiceProvider.GetRequiredService<FrmLogin>())
                        {
                            if (login.ShowDialog() != DialogResult.OK)
                            {
                                break;
                            }
                        }

                        using (FrmPrincipal principal = alcance.ServiceProvider.GetRequiredService<FrmPrincipal>())
                        {
                            Application.Run(principal);
                        }

                        if (SesionActual.Instancia.EmpleadoActual != null)
                        {
                            break;
                        }
                    }
                }
            }
        }
    }
}
