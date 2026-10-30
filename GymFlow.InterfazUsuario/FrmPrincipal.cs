using Microsoft.Extensions.DependencyInjection;
using System;
using System.Drawing;
using System.Windows.Forms;

namespace GymFlow.InterfazUsuario
{
    public class FrmPrincipal : Form
    {
        private readonly IServiceProvider _serviceProvider;

        private MenuStrip menuPrincipal;
        private Label lblBienvenida;

        public FrmPrincipal(IServiceProvider serviceProvider)
        {
            _serviceProvider = serviceProvider;

            InitializeComponent();
        }

        private void InitializeComponent()
        {
            this.menuPrincipal = new MenuStrip();
            this.lblBienvenida = new Label();

            ToolStripMenuItem menuClientes = new ToolStripMenuItem("Clientes");
            menuClientes.DropDownItems.Add(CrearItem("Gestionar clientes", new EventHandler(this.itemClientes_Click)));
            menuClientes.DropDownItems.Add(CrearItem("Estado de clientes", new EventHandler(this.itemEstadoClientes_Click)));
            menuClientes.DropDownItems.Add(CrearItem("Autoingreso", new EventHandler(this.itemAutoingreso_Click)));

            ToolStripMenuItem menuActividades = new ToolStripMenuItem("Actividades");
            menuActividades.DropDownItems.Add(CrearItem("Gestionar actividades", new EventHandler(this.itemActividades_Click)));
            menuActividades.DropDownItems.Add(CrearItem("Recepción (inscribir)", new EventHandler(this.itemRecepcion_Click)));
            menuActividades.DropDownItems.Add(CrearItem("Clientes por actividad", new EventHandler(this.itemClientesPorActividad_Click)));
            menuActividades.DropDownItems.Add(CrearItem("Desinscribir cliente", new EventHandler(this.itemDesinscripcion_Click)));
            menuActividades.DropDownItems.Add(CrearItem("Baja de inscripciones por morosidad", new EventHandler(this.itemBajaMorosos_Click)));

            ToolStripMenuItem menuFinanzas = new ToolStripMenuItem("Finanzas");
            menuFinanzas.DropDownItems.Add(CrearItem("Cuenta corriente", new EventHandler(this.itemCuentaCorriente_Click)));
            menuFinanzas.DropDownItems.Add(CrearItem("Registrar cobro", new EventHandler(this.itemCobro_Click)));
            menuFinanzas.DropDownItems.Add(CrearItem("Débito mensual", new EventHandler(this.itemDebitoMensual_Click)));
            menuFinanzas.DropDownItems.Add(CrearItem("Nota de crédito", new EventHandler(this.itemNotaCredito_Click)));

            ToolStripMenuItem menuSeguridad = new ToolStripMenuItem("Seguridad");
            menuSeguridad.DropDownItems.Add(CrearItem("Gestionar usuarios", new EventHandler(this.itemUsuarios_Click)));
            menuSeguridad.DropDownItems.Add(CrearItem("Cambiar contraseña", new EventHandler(this.itemCambiarClave_Click)));
            menuSeguridad.DropDownItems.Add(CrearItem("Cerrar sesión", new EventHandler(this.itemCerrarSesion_Click)));

            this.menuPrincipal.Items.Add(menuClientes);
            this.menuPrincipal.Items.Add(menuActividades);
            this.menuPrincipal.Items.Add(menuFinanzas);
            this.menuPrincipal.Items.Add(menuSeguridad);

            this.lblBienvenida.Location = new Point(30, 60);
            this.lblBienvenida.Size = new Size(500, 40);
            this.lblBienvenida.Text = "Sesión iniciada: " + SesionActual.EmpleadoActual.Apellido + " " + SesionActual.EmpleadoActual.Nombre;
            this.lblBienvenida.Font = new Font(this.lblBienvenida.Font, FontStyle.Bold);

            this.Text = "GymFlow";
            this.Size = new Size(600, 300);
            this.StartPosition = FormStartPosition.CenterScreen;
            this.MainMenuStrip = this.menuPrincipal;

            this.Controls.Add(this.menuPrincipal);
            this.Controls.Add(this.lblBienvenida);
        }

        private ToolStripMenuItem CrearItem(string texto, EventHandler manejador)
        {
            ToolStripMenuItem item = new ToolStripMenuItem(texto);
            item.Click += manejador;
            return item;
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

        private void itemEstadoClientes_Click(object sender, EventArgs e)
        {
            Abrir<FrmEstadoClientes>();
        }

        private void itemAutoingreso_Click(object sender, EventArgs e)
        {
            Abrir<FrmAutoingreso>();
        }

        private void itemActividades_Click(object sender, EventArgs e)
        {
            Abrir<FrmActividades>();
        }

        private void itemRecepcion_Click(object sender, EventArgs e)
        {
            Abrir<FrmRecepcion>();
        }

        private void itemClientesPorActividad_Click(object sender, EventArgs e)
        {
            Abrir<FrmClientesPorActividad>();
        }

        private void itemDesinscripcion_Click(object sender, EventArgs e)
        {
            Abrir<FrmDesinscripcion>();
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

        private void itemUsuarios_Click(object sender, EventArgs e)
        {
            Abrir<FrmUsuarios>();
        }

        private void itemCambiarClave_Click(object sender, EventArgs e)
        {
            Abrir<FrmCambiarClave>();
        }

        private void itemCerrarSesion_Click(object sender, EventArgs e)
        {
            SesionActual.CerrarSesion();
            this.Close();
        }
    }
}
