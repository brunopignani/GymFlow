using GymFlow.Aplicacion;
using GymFlow.Dominio;
using Microsoft.Extensions.DependencyInjection;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Drawing;
using System.Linq;
using System.Windows.Forms;

namespace GymFlow.InterfazUsuario
{
    public class FrmPrincipal : Form
    {
        private readonly IServiceProvider _serviceProvider;
        private readonly UsuarioServicio _usuarioServicio;
        private readonly Dictionary<ToolStripMenuItem, string> _permisosPorItem = new Dictionary<ToolStripMenuItem, string>();
        private bool _logoutRegistrado;

        private MenuStrip menuPrincipal;
        private ToolStripMenuItem menuMostrador;
        private ToolStripMenuItem menuGestion;
        private ToolStripMenuItem menuFinanzas;
        private ToolStripMenuItem menuProcesosMasivos;
        private ToolStripMenuItem menuSeguridad;
        private Label lblBienvenida;
        private Label lblSinPermisos;

        public FrmPrincipal(IServiceProvider serviceProvider, UsuarioServicio usuarioServicio)
        {
            _serviceProvider = serviceProvider;
            _usuarioServicio = usuarioServicio;

            InitializeComponent();
            SesionActual.PermisosActualizados += OnPermisosActualizados;
            this.FormClosed += FrmPrincipal_FormClosed;
            AplicarPermisosMenu();
        }

        private void InitializeComponent()
        {
            this.menuPrincipal = new MenuStrip();
            this.lblBienvenida = new Label();
            this.lblSinPermisos = new Label();

            this.menuMostrador = new ToolStripMenuItem("Mostrador");
            this.menuMostrador.DropDownItems.Add(CrearItem("Inscribir a actividad", new EventHandler(this.itemInscripciones_Click), CodigosPermiso.InscripcionesGestionar));
            this.menuMostrador.DropDownItems.Add(CrearItem("Autoingreso", new EventHandler(this.itemAutoingreso_Click), CodigosPermiso.AutoingresoUsar));

            this.menuGestion = new ToolStripMenuItem("Gestión");
            this.menuGestion.DropDownItems.Add(CrearItem("Gestionar clientes", new EventHandler(this.itemClientes_Click), CodigosPermiso.ClientesGestionar));
            this.menuGestion.DropDownItems.Add(CrearItem("Gestionar actividades", new EventHandler(this.itemActividades_Click), CodigosPermiso.ActividadesGestionar));
            this.menuGestion.DropDownItems.Add(CrearItem("Precio de cuota mensual", new EventHandler(this.itemPrecioCuota_Click), CodigosPermiso.CuotaEditarPrecio));

            this.menuProcesosMasivos = new ToolStripMenuItem("Procesos masivos");
            this.menuProcesosMasivos.DropDownItems.Add(CrearItem("Débito mensual", new EventHandler(this.itemDebitoMensual_Click), CodigosPermiso.DebitoMensualEmitir));
            this.menuProcesosMasivos.DropDownItems.Add(CrearItem("Baja de inscripciones por morosidad", new EventHandler(this.itemBajaMorosos_Click), CodigosPermiso.MorososBaja));

            this.menuFinanzas = new ToolStripMenuItem("Finanzas");
            this.menuFinanzas.DropDownItems.Add(CrearItem("Registrar cobro", new EventHandler(this.itemCobro_Click), CodigosPermiso.CobroRegistrar));
            this.menuFinanzas.DropDownItems.Add(CrearItem("Cuenta corriente", new EventHandler(this.itemCuentaCorriente_Click), CodigosPermiso.CuentaCorrienteVer));
            this.menuFinanzas.DropDownItems.Add(CrearItem("Nota de crédito", new EventHandler(this.itemNotaCredito_Click), CodigosPermiso.NotaCreditoEmitir));
            this.menuFinanzas.DropDownItems.Add(CrearItem("Ajuste de cuenta", new EventHandler(this.itemAjusteCuenta_Click), CodigosPermiso.AjusteCuentaRegistrar));
            this.menuFinanzas.DropDownItems.Add(this.menuProcesosMasivos);

            this.menuSeguridad = new ToolStripMenuItem("Seguridad");
            this.menuSeguridad.DropDownItems.Add(CrearItem("Gestionar usuarios", new EventHandler(this.itemUsuarios_Click), CodigosPermiso.UsuariosGestionar));
            this.menuSeguridad.DropDownItems.Add(CrearItem("Gestionar grupos", new EventHandler(this.itemGrupos_Click), CodigosPermiso.GruposGestionar));
            this.menuSeguridad.DropDownItems.Add(CrearItem("Auditoría", new EventHandler(this.itemAuditoria_Click), CodigosPermiso.AuditoriaVer));
            this.menuSeguridad.DropDownItems.Add(CrearItem("Resguardo y restauración", new EventHandler(this.itemRespaldo_Click), CodigosPermiso.RespaldoGestionar));
            this.menuSeguridad.DropDownItems.Add(CrearItem("Cambiar contraseña", new EventHandler(this.itemCambiarClave_Click)));
            this.menuSeguridad.DropDownItems.Add(CrearItem("Cerrar sesión", new EventHandler(this.itemCerrarSesion_Click)));

            this.menuPrincipal.Items.Add(this.menuMostrador);
            this.menuPrincipal.Items.Add(this.menuGestion);
            this.menuPrincipal.Items.Add(this.menuFinanzas);
            this.menuPrincipal.Items.Add(this.menuSeguridad);

            this.lblBienvenida.Location = new Point(30, 60);
            this.lblBienvenida.Size = new Size(520, 30);
            this.lblBienvenida.Text = "Sesión iniciada: " + SesionActual.EmpleadoActual.Apellido + " " + SesionActual.EmpleadoActual.Nombre;
            this.lblBienvenida.Font = new Font(this.lblBienvenida.Font, FontStyle.Bold);

            this.lblSinPermisos.Location = new Point(30, 120);
            this.lblSinPermisos.Size = new Size(520, 50);
            this.lblSinPermisos.TextAlign = ContentAlignment.MiddleCenter;
            this.lblSinPermisos.ForeColor = Color.DarkRed;
            this.lblSinPermisos.Text = "No posee permisos asignados. Contacte a un administrador.";
            this.lblSinPermisos.Anchor = AnchorStyles.Top | AnchorStyles.Left | AnchorStyles.Right;
            this.lblSinPermisos.Visible = false;

            this.Text = "GymFlow";
            this.Size = new Size(600, 300);
            this.StartPosition = FormStartPosition.CenterScreen;
            this.MainMenuStrip = this.menuPrincipal;

            this.Controls.Add(this.menuPrincipal);
            this.Controls.Add(this.lblBienvenida);
            this.Controls.Add(this.lblSinPermisos);
        }

        private ToolStripMenuItem CrearItem(string texto, EventHandler manejador, string codigoPermiso = null)
        {
            ToolStripMenuItem item = new ToolStripMenuItem(texto);
            item.Click += manejador;
            if (!string.IsNullOrWhiteSpace(codigoPermiso))
            {
                _permisosPorItem[item] = codigoPermiso;
            }
            return item;
        }

        private void OnPermisosActualizados()
        {
            if (this.IsDisposed)
            {
                return;
            }

            if (this.InvokeRequired)
            {
                this.BeginInvoke(new Action(AplicarPermisosMenu));
                return;
            }

            AplicarPermisosMenu();
        }

        private void FrmPrincipal_FormClosed(object sender, FormClosedEventArgs e)
        {
            RegistrarLogoutSiCorresponde();
            SesionActual.PermisosActualizados -= OnPermisosActualizados;
        }

        private void RegistrarLogoutSiCorresponde()
        {
            if (_logoutRegistrado || SesionActual.EmpleadoActual == null)
            {
                return;
            }

            _logoutRegistrado = true;

            try
            {
                _usuarioServicio.RegistrarLogout();
            }
            catch (Exception ex)
            {
                Debug.WriteLine(ex.ToString());
            }
        }

        private void AplicarPermisosMenu()
        {
            foreach (KeyValuePair<ToolStripMenuItem, string> par in _permisosPorItem)
            {
                par.Key.Visible = SesionActual.TienePermiso(par.Value);
            }

            ActualizarVisibilidadContenedor(this.menuProcesosMasivos);
            ActualizarVisibilidadContenedor(this.menuMostrador);
            ActualizarVisibilidadContenedor(this.menuGestion);
            ActualizarVisibilidadContenedor(this.menuFinanzas);
            ActualizarVisibilidadContenedor(this.menuSeguridad);

            this.lblSinPermisos.Visible = SesionActual.Permisos.Count == 0;
        }

        private static void ActualizarVisibilidadContenedor(ToolStripMenuItem contenedor)
        {
            bool tieneHijoVisible = contenedor.DropDownItems
                .OfType<ToolStripItem>()
                .Any(item => item.Available);
            contenedor.Visible = tieneHijoVisible;
        }

        private void Abrir<T>() where T : Form
        {
            using (T formulario = _serviceProvider.GetRequiredService<T>())
            {
                formulario.ShowDialog(this);
            }
        }

        private void itemClientes_Click(object sender, EventArgs e)
        {
            Abrir<FrmClientes>();
        }

        private void itemAutoingreso_Click(object sender, EventArgs e)
        {
            Abrir<FrmAutoingreso>();
        }

        private void itemActividades_Click(object sender, EventArgs e)
        {
            Abrir<FrmActividades>();
        }

        private void itemPrecioCuota_Click(object sender, EventArgs e)
        {
            Abrir<FrmPrecioCuota>();
        }

        private void itemInscripciones_Click(object sender, EventArgs e)
        {
            Abrir<FrmInscripciones>();
        }

        private void itemBajaMorosos_Click(object sender, EventArgs e)
        {
            Abrir<FrmBajaMorosos>();
        }

        private void itemCuentaCorriente_Click(object sender, EventArgs e)
        {
            Abrir<FrmCuentaCorriente>();
        }

        private void itemCobro_Click(object sender, EventArgs e)
        {
            Abrir<FrmCobro>();
        }

        private void itemDebitoMensual_Click(object sender, EventArgs e)
        {
            Abrir<FrmDebitoMensual>();
        }

        private void itemNotaCredito_Click(object sender, EventArgs e)
        {
            Abrir<FrmNotaCredito>();
        }

        private void itemAjusteCuenta_Click(object sender, EventArgs e)
        {
            Abrir<FrmAjusteCuenta>();
        }

        private void itemUsuarios_Click(object sender, EventArgs e)
        {
            Abrir<FrmUsuarios>();
        }

        private void itemGrupos_Click(object sender, EventArgs e)
        {
            Abrir<FrmGrupos>();
        }

        private void itemAuditoria_Click(object sender, EventArgs e)
        {
            Abrir<FrmAuditoria>();
        }

        private void itemRespaldo_Click(object sender, EventArgs e)
        {
            Abrir<FrmRespaldo>();
        }

        public void CerrarSesionPostRestauracion()
        {
            _logoutRegistrado = true;
            SesionActual.CerrarSesion();
            this.Close();
        }

        private void itemCambiarClave_Click(object sender, EventArgs e)
        {
            Abrir<FrmCambiarClave>();
        }

        private void itemCerrarSesion_Click(object sender, EventArgs e)
        {
            RegistrarLogoutSiCorresponde();
            SesionActual.CerrarSesion();
            this.Close();
        }
    }
}
