using GymFlow.Aplicacion;
using GymFlow.Dominio;
using Microsoft.Extensions.DependencyInjection;
using System;
using System.Drawing;
using System.Linq;
using System.Windows.Forms;

namespace GymFlow.InterfazUsuario
{
    public class FrmRecepcion : Form
    {
        private readonly ClienteServicio _clienteServicio;
        private readonly PagoServicio _pagoServicio;
        private readonly InscripcionServicio _inscripcionServicio;
        private readonly ActividadServicio _actividadServicio;
        private readonly IServiceProvider _serviceProvider;

        private Cliente _clienteCargado;
        private CuotaCliente _cuotaClienteCargada;

        private TextBox txtBuscarDni;
        private Button btnBuscar;
        private Label lblNombreCliente;
        private Label lblEstadoCuota;
        
        private Label lblActividadTitulo;
        private ComboBox cmbActividades;
        private Button btnInscribir;
        private Button btnCuentaCorriente;
        private Button btnCobro;
        private Button btnDebitoMensual;
        private Button btnNotaCredito;

        public FrmRecepcion(ClienteServicio clienteServicio, PagoServicio pagoServicio, InscripcionServicio inscripcionServicio, ActividadServicio actividadServicio, IServiceProvider serviceProvider)
        {
            _clienteServicio = clienteServicio;
            _pagoServicio = pagoServicio;
            _inscripcionServicio = inscripcionServicio;
            _actividadServicio = actividadServicio;
            _serviceProvider = serviceProvider;

            InitializeComponent();
            CargarActividades();
        }

        private void InitializeComponent()
        {
            this.txtBuscarDni = new TextBox();
            this.btnBuscar = new Button();
            this.lblNombreCliente = new Label();
            this.lblEstadoCuota = new Label();

            this.lblActividadTitulo = new Label();
            this.cmbActividades = new ComboBox();
            this.btnInscribir = new Button();
            this.btnCuentaCorriente = new Button();
            this.btnCobro = new Button();
            this.btnDebitoMensual = new Button();
            this.btnNotaCredito = new Button();

            this.txtBuscarDni.Location = new Point(20, 20);
            this.txtBuscarDni.Size = new Size(150, 20);

            this.btnBuscar.Location = new Point(180, 18);
            this.btnBuscar.Size = new Size(80, 25);
            this.btnBuscar.Text = "Buscar";
            this.btnBuscar.Click += new EventHandler(this.btnBuscar_Click);

            this.lblNombreCliente.Location = new Point(20, 55);
            this.lblNombreCliente.Size = new Size(480, 20);
            this.lblNombreCliente.Text = string.Empty;
            this.lblNombreCliente.Font = new Font(this.Font, FontStyle.Bold);

            this.lblEstadoCuota.Location = new Point(20, 80);
            this.lblEstadoCuota.Size = new Size(480, 20);
            this.lblEstadoCuota.Text = string.Empty;

            this.lblActividadTitulo.Location = new Point(20, 115);
            this.lblActividadTitulo.Size = new Size(60, 20);
            this.lblActividadTitulo.Text = "Actividad:";

            this.cmbActividades.Location = new Point(80, 112);
            this.cmbActividades.Size = new Size(240, 21);
            this.cmbActividades.DropDownStyle = ComboBoxStyle.DropDownList;

            this.btnInscribir.Location = new Point(330, 110);
            this.btnInscribir.Size = new Size(90, 25);
            this.btnInscribir.Text = "Inscribir";
            this.btnInscribir.Click += new EventHandler(this.btnInscribir_Click);

            this.btnCuentaCorriente.Location = new Point(20, 170);
            this.btnCuentaCorriente.Size = new Size(115, 30);
            this.btnCuentaCorriente.Text = "Cuenta corriente";
            this.btnCuentaCorriente.Click += new EventHandler(this.btnCuentaCorriente_Click);

            this.btnCobro.Location = new Point(145, 170);
            this.btnCobro.Size = new Size(115, 30);
            this.btnCobro.Text = "Registrar cobro";
            this.btnCobro.Click += new EventHandler(this.btnCobro_Click);

            this.btnDebitoMensual.Location = new Point(270, 170);
            this.btnDebitoMensual.Size = new Size(115, 30);
            this.btnDebitoMensual.Text = "Débito mensual";
            this.btnDebitoMensual.Click += new EventHandler(this.btnDebitoMensual_Click);

            this.btnNotaCredito.Location = new Point(395, 170);
            this.btnNotaCredito.Size = new Size(115, 30);
            this.btnNotaCredito.Text = "Nota de crédito";
            this.btnNotaCredito.Click += new EventHandler(this.btnNotaCredito_Click);

            this.Text = "Recepción";
            this.Size = new Size(545, 260);
            this.FormBorderStyle = FormBorderStyle.FixedDialog;
            this.MaximizeBox = false;
            this.StartPosition = FormStartPosition.CenterParent;

            this.Controls.Add(this.txtBuscarDni);
            this.Controls.Add(this.btnBuscar);
            this.Controls.Add(this.lblNombreCliente);
            this.Controls.Add(this.lblEstadoCuota);

            this.Controls.Add(this.lblActividadTitulo);
            this.Controls.Add(this.cmbActividades);
            this.Controls.Add(this.btnInscribir);
            this.Controls.Add(this.btnCuentaCorriente);
            this.Controls.Add(this.btnCobro);
            this.Controls.Add(this.btnDebitoMensual);
            this.Controls.Add(this.btnNotaCredito);
        }

        private void CargarActividades()
        {
            var actividades = _actividadServicio.GetAllActividades()
                .Select(a => new { a.Id, Nombre = a.Nombre + " (Cupo: " + a.CupoMaximo + ")" })
                .ToList();

            this.cmbActividades.DataSource = actividades;
            this.cmbActividades.DisplayMember = "Nombre";
            this.cmbActividades.ValueMember = "Id";
        }

        private void btnBuscar_Click(object sender, EventArgs e)
        {
            int dni;

            if (!int.TryParse(this.txtBuscarDni.Text, out dni))
            {
                MessageBox.Show("Ingrese un DNI válido.", "Recepción", MessageBoxButtons.OK, MessageBoxIcon.Warning);
                return;
            }

            _clienteCargado = _clienteServicio.GetClienteByDNI(dni);
            RefrescarCliente();

            if (_clienteCargado == null)
            {
                MessageBox.Show("No se encontró ningún cliente con ese DNI.", "Recepción", MessageBoxButtons.OK, MessageBoxIcon.Information);
            }
        }

        private void RefrescarCliente()
        {
            if (_clienteCargado != null)
            {
                this.lblNombreCliente.Text = "Cliente: " + _clienteCargado.Apellido + " " + _clienteCargado.Nombre + " (DNI " + _clienteCargado.Dni + ")";
                _cuotaClienteCargada = _clienteServicio.GetCuotaActivaByCliente(_clienteCargado.Id);

                if (_cuotaClienteCargada != null)
                {
                    if (_cuotaClienteCargada.FechaVencimiento.HasValue)
                    {
                        string vigencia = _cuotaClienteCargada.FechaVencimiento.Value >= DateTime.Today ? "ACTIVO" : "VENCIDO";
                        this.lblEstadoCuota.Text = "Cuota: " + vigencia + " (Vence: " + _cuotaClienteCargada.FechaVencimiento.Value.ToString("dd/MM/yyyy") + ")";
                    }
                    else
                    {
                        this.lblEstadoCuota.Text = "Cuota: Pendiente de pago";
                    }
                }
                else
                {
                    this.lblEstadoCuota.Text = "Cuota: Sin cuota asignada";
                }
            }
            else
            {
                this.lblNombreCliente.Text = string.Empty;
                this.lblEstadoCuota.Text = string.Empty;
                _cuotaClienteCargada = null;
            }
        }

        private void btnInscribir_Click(object sender, EventArgs e)
        {
            if (_clienteCargado == null)
            {
                MessageBox.Show("Primero busque un cliente por DNI.", "Recepción", MessageBoxButtons.OK, MessageBoxIcon.Information);
                return;
            }

            if (this.cmbActividades.SelectedValue == null)
            {
                MessageBox.Show("Seleccione una actividad.", "Recepción", MessageBoxButtons.OK, MessageBoxIcon.Information);
                return;
            }

            int actividadId = (int)this.cmbActividades.SelectedValue;

            try
            {
                if (_inscripcionServicio.InscribirCliente(_clienteCargado.Id, actividadId))
                {
                    MessageBox.Show("El cliente fue inscripto correctamente.", "Recepción", MessageBoxButtons.OK, MessageBoxIcon.Information);
                    CargarActividades();
                }
                else
                {
                    MessageBox.Show("No se pudo inscribir. Verifique que la cuota esté al día, que haya cupo disponible y que el cliente no esté ya inscripto.", "Recepción", MessageBoxButtons.OK, MessageBoxIcon.Warning);
                }
            }
            catch (Exception ex)
            {
                System.Diagnostics.Debug.WriteLine(ex.ToString());
                MessageBox.Show("Ocurrió un error al inscribir al cliente. Intente nuevamente.", "Recepción", MessageBoxButtons.OK, MessageBoxIcon.Error);
            }
        }

        private void btnCuentaCorriente_Click(object sender, EventArgs e)
        {
            using (FrmCuentaCorriente formulario = _serviceProvider.GetRequiredService<FrmCuentaCorriente>())
            {
                formulario.ShowDialog(this);
            }
        }

        private void btnCobro_Click(object sender, EventArgs e)
        {
            using (FrmCobro formulario = _serviceProvider.GetRequiredService<FrmCobro>())
            {
                if (_clienteCargado != null)
                {
                    formulario.CargarClientePorDni(_clienteCargado.Dni);
                }

                formulario.ShowDialog(this);
                RefrescarCliente();
            }
        }

        private void btnDebitoMensual_Click(object sender, EventArgs e)
        {
            using (FrmDebitoMensual formulario = _serviceProvider.GetRequiredService<FrmDebitoMensual>())
            {
                formulario.ShowDialog(this);
                RefrescarCliente();
            }
        }

        private void btnNotaCredito_Click(object sender, EventArgs e)
        {
            using (FrmNotaCredito formulario = _serviceProvider.GetRequiredService<FrmNotaCredito>())
            {
                formulario.ShowDialog(this);
            }
        }
    }
}
