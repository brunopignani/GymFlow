using GymFlow.Aplicacion;
using GymFlow.Dominio;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Drawing;
using System.Windows.Forms;

namespace GymFlow.InterfazUsuario
{
    public class FrmDesinscripcion : Form
    {
        private readonly ClienteServicio _clienteServicio;
        private readonly InscripcionServicio _inscripcionServicio;

        private Cliente _clienteCargado;
        private List<Inscripcion> _inscripciones;

        private Label lblDni;
        private TextBox txtDni;
        private Button btnBuscar;
        private Label lblCliente;
        private DataGridView dgvInscripciones;
        private Button btnDesinscribir;
        private Label lblMensaje;

        public FrmDesinscripcion(ClienteServicio clienteServicio, InscripcionServicio inscripcionServicio)
        {
            _clienteServicio = clienteServicio;
            _inscripcionServicio = inscripcionServicio;
            _inscripciones = new List<Inscripcion>();

            InitializeComponent();
        }

        private void InitializeComponent()
        {
            this.lblDni = new Label();
            this.txtDni = new TextBox();
            this.btnBuscar = new Button();
            this.lblCliente = new Label();
            this.dgvInscripciones = new DataGridView();
            this.btnDesinscribir = new Button();
            this.lblMensaje = new Label();

            this.lblDni.Location = new Point(20, 23);
            this.lblDni.Size = new Size(40, 20);
            this.lblDni.Text = "DNI:";

            this.txtDni.Location = new Point(70, 20);
            this.txtDni.Size = new Size(120, 20);

            this.btnBuscar.Location = new Point(200, 18);
            this.btnBuscar.Size = new Size(80, 25);
            this.btnBuscar.Text = "Buscar";
            this.btnBuscar.Click += new EventHandler(this.btnBuscar_Click);

            this.lblCliente.Location = new Point(20, 60);
            this.lblCliente.Size = new Size(520, 20);
            this.lblCliente.Text = string.Empty;

            this.dgvInscripciones.Location = new Point(20, 90);
            this.dgvInscripciones.Size = new Size(520, 180);
            this.dgvInscripciones.ReadOnly = true;
            this.dgvInscripciones.AllowUserToAddRows = false;
            this.dgvInscripciones.RowHeadersVisible = false;
            this.dgvInscripciones.SelectionMode = DataGridViewSelectionMode.FullRowSelect;
            this.dgvInscripciones.AutoSizeColumnsMode = DataGridViewAutoSizeColumnsMode.Fill;
            this.dgvInscripciones.Columns.Add("colActividad", "Actividad");
            this.dgvInscripciones.Columns.Add("colFecha", "Fecha de inscripción");

            this.btnDesinscribir.Location = new Point(20, 285);
            this.btnDesinscribir.Size = new Size(190, 30);
            this.btnDesinscribir.Text = "Desinscribir de la actividad";
            this.btnDesinscribir.Enabled = false;
            this.btnDesinscribir.Click += new EventHandler(this.btnDesinscribir_Click);

            this.lblMensaje.Location = new Point(20, 325);
            this.lblMensaje.Size = new Size(520, 20);
            this.lblMensaje.Text = string.Empty;

            this.Text = "Desinscribir cliente";
            this.Size = new Size(580, 410);
            this.FormBorderStyle = FormBorderStyle.FixedDialog;
            this.MaximizeBox = false;
            this.StartPosition = FormStartPosition.CenterParent;
            this.AcceptButton = this.btnBuscar;

            this.Controls.Add(this.lblDni);
            this.Controls.Add(this.txtDni);
            this.Controls.Add(this.btnBuscar);
            this.Controls.Add(this.lblCliente);
            this.Controls.Add(this.dgvInscripciones);
            this.Controls.Add(this.btnDesinscribir);
            this.Controls.Add(this.lblMensaje);
        }

        private void btnBuscar_Click(object sender, EventArgs e)
        {
            _clienteCargado = null;
            this.lblCliente.Text = string.Empty;
            this.lblMensaje.Text = string.Empty;

            int dni;

            if (!int.TryParse(this.txtDni.Text, out dni))
            {
                MessageBox.Show("Ingrese un DNI válido.", "Desinscribir cliente", MessageBoxButtons.OK, MessageBoxIcon.Warning);
                CargarInscripciones();
                return;
            }

            try
            {
                Cliente cliente = _clienteServicio.GetClienteByDNI(dni);

                if (cliente == null)
                {
                    this.lblMensaje.Text = "No existe un cliente con ese DNI.";
                    CargarInscripciones();
                    return;
                }

                _clienteCargado = cliente;
                this.lblCliente.Text = cliente.Apellido + " " + cliente.Nombre + " (DNI " + cliente.Dni + ")";
                CargarInscripciones();
            }
            catch (Exception ex)
            {
                Debug.WriteLine(ex.ToString());
                MessageBox.Show("No se pudo consultar el cliente. Intente nuevamente.", "Desinscribir cliente", MessageBoxButtons.OK, MessageBoxIcon.Error);
            }
        }

        private void CargarInscripciones()
        {
            this.dgvInscripciones.Rows.Clear();
            _inscripciones = new List<Inscripcion>();
            this.btnDesinscribir.Enabled = false;

            if (_clienteCargado == null)
            {
                return;
            }

            _inscripciones = _inscripcionServicio.GetInscripcionesByCliente(_clienteCargado.Id);

            foreach (Inscripcion inscripcion in _inscripciones)
            {
                this.dgvInscripciones.Rows.Add(inscripcion.Actividad.Nombre, inscripcion.Fecha.ToString("dd/MM/yyyy"));
            }

            if (_inscripciones.Count == 0)
            {
                this.lblMensaje.Text = "El cliente no está inscripto en ninguna actividad.";
            }

            this.btnDesinscribir.Enabled = _inscripciones.Count > 0;
        }

        private void btnDesinscribir_Click(object sender, EventArgs e)
        {
            if (_clienteCargado == null || this.dgvInscripciones.CurrentRow == null)
            {
                return;
            }

            int indice = this.dgvInscripciones.CurrentRow.Index;

            if (indice < 0 || indice >= _inscripciones.Count)
            {
                MessageBox.Show("Seleccione una inscripción de la lista.", "Desinscribir cliente", MessageBoxButtons.OK, MessageBoxIcon.Information);
                return;
            }

            Inscripcion inscripcion = _inscripciones[indice];
            string pregunta = "¿Desinscribir a " + this.lblCliente.Text + " de " + inscripcion.Actividad.Nombre + "?";

            if (MessageBox.Show(pregunta, "Desinscribir cliente", MessageBoxButtons.YesNo, MessageBoxIcon.Question) != DialogResult.Yes)
            {
                return;
            }

            this.btnDesinscribir.Enabled = false;
            this.Cursor = Cursors.WaitCursor;

            try
            {
                if (!_inscripcionServicio.DesinscribirCliente(_clienteCargado.Id, inscripcion.ActividadId))
                {
                    MessageBox.Show("No se pudo desinscribir al cliente.", "Desinscribir cliente", MessageBoxButtons.OK, MessageBoxIcon.Warning);
                    return;
                }

                this.lblMensaje.Text = "El cliente fue desinscripto y se liberó el cupo.";
            }
            catch (Exception ex)
            {
                Debug.WriteLine(ex.ToString());
                MessageBox.Show("Ocurrió un error al desinscribir. Verifique la inscripción antes de reintentar.", "Desinscribir cliente", MessageBoxButtons.OK, MessageBoxIcon.Error);
            }
            finally
            {
                this.Cursor = Cursors.Default;
                CargarInscripciones();
            }
        }
    }
}
