using GymFlow.Aplicacion;
using GymFlow.Dominio;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Drawing;
using System.Windows.Forms;

namespace GymFlow.InterfazUsuario
{
    public class FrmClientesPorActividad : Form
    {
        private readonly ActividadServicio _actividadServicio;
        private readonly InscripcionServicio _inscripcionServicio;

        private Label lblActividad;
        private ComboBox cmbActividades;
        private DataGridView dgvInscriptos;
        private Label lblMensaje;

        public FrmClientesPorActividad(ActividadServicio actividadServicio, InscripcionServicio inscripcionServicio)
        {
            _actividadServicio = actividadServicio;
            _inscripcionServicio = inscripcionServicio;

            InitializeComponent();
            CargarActividades();
        }

        private void InitializeComponent()
        {
            this.lblActividad = new Label();
            this.cmbActividades = new ComboBox();
            this.dgvInscriptos = new DataGridView();
            this.lblMensaje = new Label();

            this.lblActividad.Location = new Point(20, 23);
            this.lblActividad.Size = new Size(70, 20);
            this.lblActividad.Text = "Actividad:";

            this.cmbActividades.Location = new Point(100, 20);
            this.cmbActividades.Size = new Size(250, 21);
            this.cmbActividades.DropDownStyle = ComboBoxStyle.DropDownList;
            this.cmbActividades.SelectedIndexChanged += new EventHandler(this.cmbActividades_SelectedIndexChanged);

            this.dgvInscriptos.Location = new Point(20, 60);
            this.dgvInscriptos.Size = new Size(560, 280);
            this.dgvInscriptos.ReadOnly = true;
            this.dgvInscriptos.AllowUserToAddRows = false;
            this.dgvInscriptos.RowHeadersVisible = false;
            this.dgvInscriptos.SelectionMode = DataGridViewSelectionMode.FullRowSelect;
            this.dgvInscriptos.AutoSizeColumnsMode = DataGridViewAutoSizeColumnsMode.Fill;
            this.dgvInscriptos.Columns.Add("colDni", "DNI");
            this.dgvInscriptos.Columns.Add("colApellido", "Apellido");
            this.dgvInscriptos.Columns.Add("colNombre", "Nombre");
            this.dgvInscriptos.Columns.Add("colFecha", "Fecha de inscripción");

            this.lblMensaje.Location = new Point(20, 350);
            this.lblMensaje.Size = new Size(560, 20);
            this.lblMensaje.Text = string.Empty;

            this.Text = "Clientes por actividad";
            this.Size = new Size(620, 430);
            this.FormBorderStyle = FormBorderStyle.FixedDialog;
            this.MaximizeBox = false;
            this.StartPosition = FormStartPosition.CenterParent;

            this.Controls.Add(this.lblActividad);
            this.Controls.Add(this.cmbActividades);
            this.Controls.Add(this.dgvInscriptos);
            this.Controls.Add(this.lblMensaje);
        }

        private void CargarActividades()
        {
            try
            {
                this.cmbActividades.DataSource = _actividadServicio.GetAllActividades();
                this.cmbActividades.DisplayMember = "Nombre";
                this.cmbActividades.ValueMember = "Id";
            }
            catch (Exception ex)
            {
                Debug.WriteLine(ex.ToString());
                MessageBox.Show("No se pudo cargar la lista de actividades.", "Clientes por actividad", MessageBoxButtons.OK, MessageBoxIcon.Error);
            }
        }

        private void cmbActividades_SelectedIndexChanged(object sender, EventArgs e)
        {
            this.dgvInscriptos.Rows.Clear();
            this.lblMensaje.Text = string.Empty;

            Actividad actividad = this.cmbActividades.SelectedItem as Actividad;

            if (actividad == null)
            {
                return;
            }

            try
            {
                List<Inscripcion> inscripciones = _inscripcionServicio.GetInscripcionesByActividad(actividad.Id);

                foreach (Inscripcion inscripcion in inscripciones)
                {
                    this.dgvInscriptos.Rows.Add(
                        inscripcion.Cliente.Dni,
                        inscripcion.Cliente.Apellido,
                        inscripcion.Cliente.Nombre,
                        inscripcion.Fecha.ToString("dd/MM/yyyy"));
                }

                this.lblMensaje.Text = inscripciones.Count == 0
                    ? "La actividad no tiene clientes inscriptos."
                    : "Clientes inscriptos: " + inscripciones.Count;
            }
            catch (Exception ex)
            {
                Debug.WriteLine(ex.ToString());
                MessageBox.Show("No se pudieron consultar los inscriptos.", "Clientes por actividad", MessageBoxButtons.OK, MessageBoxIcon.Error);
            }
        }
    }
}
