using GymFlow.Aplicacion;
using GymFlow.Dominio;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Drawing;
using System.Windows.Forms;

namespace GymFlow.InterfazUsuario
{
    public class FrmActividades : Form
    {
        private readonly ActividadServicio _actividadServicio;

        private List<Actividad> _actividades;

        private DataGridView dgvActividades;
        private Label lblNombre;
        private TextBox txtNombre;
        private Label lblCupo;
        private TextBox txtCupo;
        private Button btnAlta;
        private Button btnModificar;
        private Button btnEliminar;
        private Button btnLimpiar;

        public FrmActividades(ActividadServicio actividadServicio)
        {
            _actividadServicio = actividadServicio;
            _actividades = new List<Actividad>();

            InitializeComponent();
            CargarActividades();
        }

        private void InitializeComponent()
        {
            this.dgvActividades = new DataGridView();
            this.lblNombre = new Label();
            this.txtNombre = new TextBox();
            this.lblCupo = new Label();
            this.txtCupo = new TextBox();
            this.btnAlta = new Button();
            this.btnModificar = new Button();
            this.btnEliminar = new Button();
            this.btnLimpiar = new Button();

            this.dgvActividades.Location = new Point(20, 20);
            this.dgvActividades.Size = new Size(540, 200);
            this.dgvActividades.ReadOnly = true;
            this.dgvActividades.AllowUserToAddRows = false;
            this.dgvActividades.RowHeadersVisible = false;
            this.dgvActividades.SelectionMode = DataGridViewSelectionMode.FullRowSelect;
            this.dgvActividades.AutoSizeColumnsMode = DataGridViewAutoSizeColumnsMode.Fill;
            this.dgvActividades.Columns.Add("colNombre", "Actividad");
            this.dgvActividades.Columns.Add("colCupo", "Cupo");
            this.dgvActividades.SelectionChanged += new EventHandler(this.dgvActividades_SelectionChanged);

            this.lblNombre.Location = new Point(20, 243);
            this.lblNombre.Size = new Size(60, 20);
            this.lblNombre.Text = "Nombre:";
            this.txtNombre.Location = new Point(90, 240);
            this.txtNombre.Size = new Size(220, 20);

            this.lblCupo.Location = new Point(330, 243);
            this.lblCupo.Size = new Size(50, 20);
            this.lblCupo.Text = "Cupo:";
            this.txtCupo.Location = new Point(385, 240);
            this.txtCupo.Size = new Size(60, 20);

            this.btnAlta.Location = new Point(20, 285);
            this.btnAlta.Size = new Size(100, 30);
            this.btnAlta.Text = "Alta";
            this.btnAlta.Click += new EventHandler(this.btnAlta_Click);

            this.btnModificar.Location = new Point(130, 285);
            this.btnModificar.Size = new Size(100, 30);
            this.btnModificar.Text = "Modificar";
            this.btnModificar.Click += new EventHandler(this.btnModificar_Click);

            this.btnEliminar.Location = new Point(240, 285);
            this.btnEliminar.Size = new Size(100, 30);
            this.btnEliminar.Text = "Eliminar";
            this.btnEliminar.Click += new EventHandler(this.btnEliminar_Click);

            this.btnLimpiar.Location = new Point(350, 285);
            this.btnLimpiar.Size = new Size(100, 30);
            this.btnLimpiar.Text = "Limpiar";
            this.btnLimpiar.Click += new EventHandler(this.btnLimpiar_Click);

            this.Text = "Gestionar actividades";
            this.Size = new Size(600, 380);
            this.FormBorderStyle = FormBorderStyle.FixedDialog;
            this.MaximizeBox = false;
            this.StartPosition = FormStartPosition.CenterParent;

            this.Controls.Add(this.dgvActividades);
            this.Controls.Add(this.lblNombre);
            this.Controls.Add(this.txtNombre);
            this.Controls.Add(this.lblCupo);
            this.Controls.Add(this.txtCupo);
            this.Controls.Add(this.btnAlta);
            this.Controls.Add(this.btnModificar);
            this.Controls.Add(this.btnEliminar);
            this.Controls.Add(this.btnLimpiar);
        }

        private void CargarActividades()
        {
            this.dgvActividades.Rows.Clear();
            LimpiarCampos();

            try
            {
                _actividades = _actividadServicio.GetAllActividades();

                foreach (Actividad actividad in _actividades)
                {
                    this.dgvActividades.Rows.Add(actividad.Nombre, actividad.CupoMaximo);
                }
            }
            catch (Exception ex)
            {
                Debug.WriteLine(ex.ToString());
                MessageBox.Show("No se pudo cargar la lista de actividades.", "Gestionar actividades", MessageBoxButtons.OK, MessageBoxIcon.Error);
            }
        }

        private void LimpiarCampos()
        {
            this.txtNombre.Text = string.Empty;
            this.txtCupo.Text = string.Empty;
        }

        private Actividad ActividadSeleccionada()
        {
            if (this.dgvActividades.CurrentRow == null)
            {
                return null;
            }

            int indice = this.dgvActividades.CurrentRow.Index;

            if (indice < 0 || indice >= _actividades.Count)
            {
                return null;
            }

            return _actividades[indice];
        }

        private bool LeerCampos(Actividad actividad)
        {
            int cupo;

            if (!int.TryParse(this.txtCupo.Text, out cupo))
            {
                MessageBox.Show("Ingrese un cupo numérico.", "Gestionar actividades", MessageBoxButtons.OK, MessageBoxIcon.Warning);
                return false;
            }

            actividad.Nombre = this.txtNombre.Text.Trim();
            actividad.CupoMaximo = cupo;
            return true;
        }

        private void dgvActividades_SelectionChanged(object sender, EventArgs e)
        {
            Actividad actividad = ActividadSeleccionada();

            if (actividad == null)
            {
                return;
            }

            this.txtNombre.Text = actividad.Nombre;
            this.txtCupo.Text = actividad.CupoMaximo.ToString();
        }

        private void btnAlta_Click(object sender, EventArgs e)
        {
            Actividad nueva = new Actividad();

            if (!LeerCampos(nueva))
            {
                return;
            }

            try
            {
                if (!_actividadServicio.AddActividad(nueva))
                {
                    MessageBox.Show("No se pudo dar de alta. Ingrese un nombre y un cupo entre 1 y " + ActividadServicio.CupoMaximoPermitido + ".", "Gestionar actividades", MessageBoxButtons.OK, MessageBoxIcon.Warning);
                    return;
                }

                CargarActividades();
            }
            catch (Exception ex)
            {
                Debug.WriteLine(ex.ToString());
                MessageBox.Show("Ocurrió un error al guardar la actividad.", "Gestionar actividades", MessageBoxButtons.OK, MessageBoxIcon.Error);
            }
        }

        private void btnModificar_Click(object sender, EventArgs e)
        {
            Actividad seleccionada = ActividadSeleccionada();

            if (seleccionada == null)
            {
                MessageBox.Show("Seleccione una actividad de la lista.", "Gestionar actividades", MessageBoxButtons.OK, MessageBoxIcon.Information);
                return;
            }

            Actividad copia = new Actividad();
            copia.Id = seleccionada.Id;

            if (!LeerCampos(copia))
            {
                return;
            }

            try
            {
                if (!_actividadServicio.UpdateActividad(copia))
                {
                    MessageBox.Show("No se pudo modificar. Ingrese un nombre y un cupo entre 0 y " + ActividadServicio.CupoMaximoPermitido + ".", "Gestionar actividades", MessageBoxButtons.OK, MessageBoxIcon.Warning);
                    return;
                }

                CargarActividades();
            }
            catch (Exception ex)
            {
                Debug.WriteLine(ex.ToString());
                MessageBox.Show("Ocurrió un error al modificar la actividad.", "Gestionar actividades", MessageBoxButtons.OK, MessageBoxIcon.Error);
            }
        }

        private void btnEliminar_Click(object sender, EventArgs e)
        {
            Actividad seleccionada = ActividadSeleccionada();

            if (seleccionada == null)
            {
                MessageBox.Show("Seleccione una actividad de la lista.", "Gestionar actividades", MessageBoxButtons.OK, MessageBoxIcon.Information);
                return;
            }

            if (MessageBox.Show("¿Eliminar la actividad " + seleccionada.Nombre + "?", "Gestionar actividades", MessageBoxButtons.YesNo, MessageBoxIcon.Question) != DialogResult.Yes)
            {
                return;
            }

            try
            {
                if (!_actividadServicio.DeleteActividad(seleccionada.Id))
                {
                    MessageBox.Show("No se puede eliminar una actividad que tiene clientes inscriptos.", "Gestionar actividades", MessageBoxButtons.OK, MessageBoxIcon.Warning);
                    return;
                }

                CargarActividades();
            }
            catch (Exception ex)
            {
                Debug.WriteLine(ex.ToString());
                MessageBox.Show("Ocurrió un error al eliminar la actividad.", "Gestionar actividades", MessageBoxButtons.OK, MessageBoxIcon.Error);
            }
        }

        private void btnLimpiar_Click(object sender, EventArgs e)
        {
            LimpiarCampos();
        }
    }
}
