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
            this.cmbActividades.Size = new Size(280, 21);
            this.cmbActividades.DropDownStyle = ComboBoxStyle.DropDownList;

            this.btnInscribir.Location = new Point(370, 110);
            this.btnInscribir.Size = new Size(90, 25);
            this.btnInscribir.Text = "Inscribir";
            this.btnInscribir.Click += new EventHandler(this.btnInscribir_Click);

            this.Text = "Recepción";
            this.Size = new Size(500, 200);
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
        }

        private void CargarActividades()
        {
            var actividades = _actividadServicio.GetAllActividades()
                .Select(a =>
                {
                    int inscriptos = _inscripcionServicio.GetInscripcionesByActividad(a.Id).Count;
                    int disponibles = a.CapacidadMaxima - inscriptos;
                    return new
                    {
                        a.Id,
                        Nombre = a.Nombre + " " + inscriptos + "/" + a.CapacidadMaxima + " (" + disponibles + " disponibles)"
                    };
                })
                .ToList();

            this.cmbActividades.DataSource = actividades;
            this.cmbActividades.DisplayMember = "Nombre";
            this.cmbActividades.ValueMember = "Id";
        }

        private void btnBuscar_Click(object sender, EventArgs e)
        {
            int dni;

            if (!ValidacionesDatos.TryParseDni(this.txtBuscarDni.Text, out dni))
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

                string estado = ClienteServicio.DeterminarEstadoCuota(_cuotaClienteCargada);

                if (estado == EstadosCliente.Activo)
                {
                    this.lblEstadoCuota.Text = "Cuota: ACTIVO (Vence: " + _cuotaClienteCargada.FechaVencimiento.Value.ToString("dd/MM/yyyy") + ")";
                }
                else if (estado == EstadosCliente.Vencido)
                {
                    this.lblEstadoCuota.Text = "Cuota: VENCIDO (Venció: " + _cuotaClienteCargada.FechaVencimiento.Value.ToString("dd/MM/yyyy") + ")";
                }
                else if (estado == EstadosCliente.Pendiente)
                {
                    this.lblEstadoCuota.Text = "Cuota: PENDIENTE (Pendiente de pago)";
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
                if (EsConflictoDeConcurrencia(ex))
                {
                    MessageBox.Show("La inscripción no pudo completarse debido a una contienda simultánea de cupos con otra operación. Por favor, intente nuevamente.", "Recepción", MessageBoxButtons.OK, MessageBoxIcon.Warning);
                }
                else
                {
                    MessageBox.Show("Ocurrió un error al inscribir al cliente. Intente nuevamente.", "Recepción", MessageBoxButtons.OK, MessageBoxIcon.Error);
                }
            }
        }

        private bool EsConflictoDeConcurrencia(Exception ex)
        {
            Exception actual = ex;
            while (actual != null)
            {
                if (actual.Message.IndexOf("deadlock", StringComparison.OrdinalIgnoreCase) >= 0 ||
                    actual.Message.IndexOf("1205", StringComparison.OrdinalIgnoreCase) >= 0)
                {
                    return true;
                }
                actual = actual.InnerException;
            }
            return false;
        }
    }
}
