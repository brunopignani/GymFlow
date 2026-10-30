using GymFlow.Abstracciones;
using GymFlow.Aplicacion;
using GymFlow.ContextoBD;
using GymFlow.Repositorio;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;
using System;
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

            servicios.AddScoped<IUsuarioRepositorio, UsuarioRepositorio>();
            servicios.AddScoped<IClienteRepositorio, ClienteRepositorio>();
            servicios.AddScoped<IEmpleadoRepositorio, EmpleadoRepositorio>();
            servicios.AddScoped<IActividadRepositorio, ActividadRepositorio>();
            servicios.AddScoped<IInscripcionRepositorio, InscripcionRepositorio>();
            servicios.AddScoped<ICuotaClienteRepositorio, CuotaClienteRepositorio>();
            servicios.AddScoped<ICuotaRepositorio, CuotaRepositorio>();
            servicios.AddScoped<IPagoRepositorio, PagoRepositorio>();
            servicios.AddScoped<ICuentaCorrienteRepositorio, CuentaCorrienteRepositorio>();
            servicios.AddScoped<IMovimientoRepositorio, MovimientoRepositorio>();
            servicios.AddScoped<ITipoMovimientoRepositorio, TipoMovimientoRepositorio>();
            servicios.AddScoped<IComprobanteRepositorio, ComprobanteRepositorio>();
            servicios.AddScoped<ITransaccionBD, TransaccionBD>();

            servicios.AddScoped<PagoServicio>();
            servicios.AddScoped<InscripcionServicio>();
            servicios.AddScoped<ClienteServicio>();
            servicios.AddScoped<ActividadServicio>();
            servicios.AddScoped<CuentaCorrienteServicio>();
            servicios.AddScoped<CobroServicio>();
            servicios.AddScoped<DebitoMensualServicio>();
            servicios.AddScoped<NotaCreditoServicio>();
            servicios.AddScoped<UsuarioServicio>();

            servicios.AddTransient<FrmLogin>();
            servicios.AddTransient<FrmRegistro>();
            servicios.AddTransient<FrmPrincipal>();
            servicios.AddTransient<FrmAutoingreso>();
            servicios.AddTransient<FrmRecepcion>();
            servicios.AddTransient<FrmClientes>();
            servicios.AddTransient<FrmEstadoClientes>();
            servicios.AddTransient<FrmActividades>();
            servicios.AddTransient<FrmClientesPorActividad>();
            servicios.AddTransient<FrmDesinscripcion>();
            servicios.AddTransient<FrmBajaMorosos>();
            servicios.AddTransient<FrmCuentaCorriente>();
            servicios.AddTransient<FrmCobro>();
            servicios.AddTransient<FrmDebitoMensual>();
            servicios.AddTransient<FrmNotaCredito>();
            servicios.AddTransient<FrmUsuarios>();
            servicios.AddTransient<FrmCambiarClave>();

            ServiceProviderOptions opciones = new ServiceProviderOptions();
            opciones.ValidateScopes = true;
            opciones.ValidateOnBuild = true;

            ServiceProvider proveedor = servicios.BuildServiceProvider(opciones);

            using (IServiceScope alcance = proveedor.CreateScope())
            {
                while (true)
                {
                    FrmLogin login = alcance.ServiceProvider.GetRequiredService<FrmLogin>();

                    if (login.ShowDialog() != DialogResult.OK)
                    {
                        break;
                    }

                    FrmPrincipal principal = alcance.ServiceProvider.GetRequiredService<FrmPrincipal>();
                    Application.Run(principal);

                    if (SesionActual.EmpleadoActual != null)
                    {
                        break;
                    }
                }
            }
        }
    }
}
