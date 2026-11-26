using GymFlow.Aplicacion;
using GymFlow.Dominio;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Drawing;
using System.Linq;
using System.Windows.Forms;

namespace GymFlow.InterfazUsuario
{
    public class FrmActividades : Form
    {
        private readonly ActividadServicio _actividadServicio;
        private readonly InscripcionServicio _inscripcionServicio;

        private List<Actividad> _actividades;
        private List<Actividad> _actividadesMostradas;
        private List<Inscripcion> _inscripcionesActuales;
        private List<Inscripcion> _inscripcionesMostradas;

        private Label lblFiltroNombreAct;
        private TextBox txtFiltroNombreAct;
        private Label lblFiltroCapacidad;
        private TextBox txtFiltroCapacidad;
        private Button btnLimpiarFiltrosAct;
        private DataGridView dgvActividades;
        private Label lblNombre;
        private TextBox txtNombre;
        private Label lblCupo;
        private TextBox txtCupo;
        private Button btnAlta;
        private Button btnModificar;
        private Button btnEliminar;
        private Button btnLimpiar;

        private Label lblInscriptosTitulo;
        private Label lblFiltroInsDni;
        private TextBox txtFiltroInsDni;
        private Label lblFiltroInsNombre;
        private TextBox txtFiltroInsNombre;
        private Button btnLimpiarFiltrosIns;
        private DataGridView dgvInscriptos;
        private Label lblInscriptosResumen;
        private Button btnDesinscribir;

        public FrmActividades(ActividadServicio actividadServicio, InscripcionServicio inscripcionServicio)
        {
            _actividadServicio = actividadServicio;
            _inscripcionServicio = inscripcionServicio;
            _actividades = new List<Actividad>();
            _actividadesMostradas = new List<Actividad>();
            _inscripcionesActuales = new List<Inscripcion>();
            _inscripcionesMostradas = new List<Inscripcion>();

            InitializeComponent();
            CargarActividades();
        }

        private void InitializeComponent()
        {
            this.lblFiltroNombreAct = new Label();
            this.txtFiltroNombreAct = new TextBox();
            this.lblFiltroCapacidad = new Label();
            this.txtFiltroCapacidad = new TextBox();
            this.btnLimpiarFiltrosAct = new Button();
            this.dgvActividades = new DataGridView();
            this.lblNombre = new Label();
            this.txtNombre = new TextBox();
            this.lblCupo = new Label();
            this.txtCupo = new TextBox();
            this.btnAlta = new Button();
            this.btnModificar = new Button();
            this.btnEliminar = new Button();
            this.btnLimpiar = new Button();

            this.lblInscriptosTitulo = new Label();
            this.lblFiltroInsDni = new Label();
            this.txtFiltroInsDni = new TextBox();
            this.lblFiltroInsNombre = new Label();
            this.txtFiltroInsNombre = new TextBox();
            this.btnLimpiarFiltrosIns = new Button();
            this.dgvInscriptos = new DataGridView();
            this.lblInscriptosResumen = new Label();
            this.btnDesinscribir = new Button();

            // Filtros de actividades
            this.lblFiltroNombreAct.Location = new Point(20, 21);
            this.lblFiltroNombreAct.Size = new Size(60, 20);
            this.lblFiltroNombreAct.Text = "Actividad:";

            this.txtFiltroNombreAct.Location = new Point(85, 18);
            this.txtFiltroNombreAct.Size = new Size(170, 20);
            this.txtFiltroNombreAct.TextChanged += new EventHandler(this.FiltrosActividades_Changed);

            this.lblFiltroCapacidad.Location = new Point(270, 21);
            this.lblFiltroCapacidad.Size = new Size(95, 20);
            this.lblFiltroCapacidad.Text = "Capacidad mín.:";

            this.txtFiltroCapacidad.Location = new Point(370, 18);
            this.txtFiltroCapacidad.Size = new Size(60, 20);
            this.txtFiltroCapacidad.TextChanged += new EventHandler(this.FiltrosActividades_Changed);

            this.btnLimpiarFiltrosAct.Location = new Point(480, 16);
            this.btnLimpiarFiltrosAct.Size = new Size(120, 25);
            this.btnLimpiarFiltrosAct.Text = "Limpiar filtros";
            this.btnLimpiarFiltrosAct.Click += new EventHandler(this.btnLimpiarFiltrosAct_Click);

            // Panel superior: lista de actividades
            this.dgvActividades.Location = new Point(20, 48);
            this.dgvActividades.Size = new Size(580, 165);
            this.dgvActividades.ReadOnly = true;
            this.dgvActividades.AllowUserToAddRows = false;
            this.dgvActividades.RowHeadersVisible = false;
            this.dgvActividades.SelectionMode = DataGridViewSelectionMode.FullRowSelect;
            this.dgvActividades.AutoSizeColumnsMode = DataGridViewAutoSizeColumnsMode.Fill;
            this.dgvActividades.Columns.Add("colNombre", "Actividad");
            this.dgvActividades.Columns.Add("colCapacidad", "Capacidad");
            this.dgvActividades.SelectionChanged += new EventHandler(this.dgvActividades_SelectionChanged);

            // Campos de edición
            this.lblNombre.Location = new Point(20, 231);
            this.lblNombre.Size = new Size(60, 20);
            this.lblNombre.Text = "Nombre:";
            this.txtNombre.Location = new Point(90, 228);
            this.txtNombre.Size = new Size(220, 20);

            this.lblCupo.Location = new Point(330, 231);
            this.lblCupo.Size = new Size(65, 20);
            this.lblCupo.Text = "Capacidad:";
            this.txtCupo.Location = new Point(400, 228);
            this.txtCupo.Size = new Size(60, 20);

            // Botones de gestión de actividades
            this.btnAlta.Location = new Point(20, 263);
            this.btnAlta.Size = new Size(100, 30);
            this.btnAlta.Text = "Alta";
            this.btnAlta.Click += new EventHandler(this.btnAlta_Click);

            this.btnModificar.Location = new Point(130, 263);
            this.btnModificar.Size = new Size(100, 30);
            this.btnModificar.Text = "Modificar";
            this.btnModificar.Click += new EventHandler(this.btnModificar_Click);

            this.btnEliminar.Location = new Point(240, 263);
            this.btnEliminar.Size = new Size(100, 30);
            this.btnEliminar.Text = "Eliminar";
            this.btnEliminar.Click += new EventHandler(this.btnEliminar_Click);

            this.btnLimpiar.Location = new Point(350, 263);
            this.btnLimpiar.Size = new Size(100, 30);
            this.btnLimpiar.Text = "Limpiar";
            this.btnLimpiar.Click += new EventHandler(this.btnLimpiar_Click);

            // Panel inferior: inscriptos
            this.lblInscriptosTitulo.Location = new Point(20, 310);
            this.lblInscriptosTitulo.Size = new Size(580, 20);
            this.lblInscriptosTitulo.Text = "Socios inscriptos en la actividad seleccionada:";
            this.lblInscriptosTitulo.Font = new Font(this.Font, FontStyle.Bold);

            // Filtros de inscriptos
            this.lblFiltroInsDni.Location = new Point(20, 339);
            this.lblFiltroInsDni.Size = new Size(32, 20);
            this.lblFiltroInsDni.Text = "DNI:";

            this.txtFiltroInsDni.Location = new Point(56, 336);
            this.txtFiltroInsDni.Size = new Size(110, 20);
            this.txtFiltroInsDni.TextChanged += new EventHandler(this.FiltrosInscriptos_Changed);

            this.lblFiltroInsNombre.Location = new Point(180, 339);
            this.lblFiltroInsNombre.Size = new Size(100, 20);
            this.lblFiltroInsNombre.Text = "Nombre/Apellido:";

            this.txtFiltroInsNombre.Location = new Point(285, 336);
            this.txtFiltroInsNombre.Size = new Size(180, 20);
            this.txtFiltroInsNombre.TextChanged += new EventHandler(this.FiltrosInscriptos_Changed);

            this.btnLimpiarFiltrosIns.Location = new Point(480, 334);
            this.btnLimpiarFiltrosIns.Size = new Size(120, 25);
            this.btnLimpiarFiltrosIns.Text = "Limpiar filtros";
            this.btnLimpiarFiltrosIns.Click += new EventHandler(this.btnLimpiarFiltrosIns_Click);

            this.dgvInscriptos.Location = new Point(20, 366);
            this.dgvInscriptos.Size = new Size(580, 165);
            this.dgvInscriptos.ReadOnly = true;
            this.dgvInscriptos.AllowUserToAddRows = false;
            this.dgvInscriptos.RowHeadersVisible = false;
            this.dgvInscriptos.SelectionMode = DataGridViewSelectionMode.FullRowSelect;
            this.dgvInscriptos.AutoSizeColumnsMode = DataGridViewAutoSizeColumnsMode.Fill;
            this.dgvInscriptos.Columns.Add("colDni", "DNI");
            this.dgvInscriptos.Columns.Add("colApellido", "Apellido");
            this.dgvInscriptos.Columns.Add("colNombreIns", "Nombre");
            this.dgvInscriptos.Columns.Add("colFechaIns", "Fecha inscripción");

            this.lblInscriptosResumen.Location = new Point(20, 542);
            this.lblInscriptosResumen.Size = new Size(430, 20);
            this.lblInscriptosResumen.Text = string.Empty;

            this.btnDesinscribir.Location = new Point(470, 538);
            this.btnDesinscribir.Size = new Size(130, 28);
            this.btnDesinscribir.Text = "Desinscribir socio";
            this.btnDesinscribir.Click += new EventHandler(this.btnDesinscribir_Click);

            this.Text = "Gestionar actividades";
            this.Size = new Size(640, 620);
            this.FormBorderStyle = FormBorderStyle.FixedDialog;
            this.MaximizeBox = false;
            this.StartPosition = FormStartPosition.CenterParent;

            this.Controls.Add(this.lblFiltroNombreAct);
            this.Controls.Add(this.txtFiltroNombreAct);
            this.Controls.Add(this.lblFiltroCapacidad);
            this.Controls.Add(this.txtFiltroCapacidad);
            this.Controls.Add(this.btnLimpiarFiltrosAct);
            this.Controls.Add(this.dgvActividades);
            this.Controls.Add(this.lblNombre);
            this.Controls.Add(this.txtNombre);
            this.Controls.Add(this.lblCupo);
            this.Controls.Add(this.txtCupo);
            this.Controls.Add(this.btnAlta);
            this.Controls.Add(this.btnModificar);
            this.Controls.Add(this.btnEliminar);
            this.Controls.Add(this.btnLimpiar);
            this.Controls.Add(this.lblInscriptosTitulo);
            this.Controls.Add(this.lblFiltroInsDni);
            this.Controls.Add(this.txtFiltroInsDni);
            this.Controls.Add(this.lblFiltroInsNombre);
            this.Controls.Add(this.txtFiltroInsNombre);
            this.Controls.Add(this.btnLimpiarFiltrosIns);
            this.Controls.Add(this.dgvInscriptos);
            this.Controls.Add(this.lblInscriptosResumen);
            this.Controls.Add(this.btnDesinscribir);
        }

        private void FiltrosActividades_Changed(object sender, EventArgs e)
        {
            AplicarFiltroActividades();
        }

        private void btnLimpiarFiltrosAct_Click(object sender, EventArgs e)
        {
            this.txtFiltroNombreAct.Text = string.Empty;
            this.txtFiltroCapacidad.Text = string.Empty;
        }

        private void FiltrosInscriptos_Changed(object sender, EventArgs e)
        {
            AplicarFiltroInscriptos();
        }

        private void btnLimpiarFiltrosIns_Click(object sender, EventArgs e)
        {
            this.txtFiltroInsDni.Text = string.Empty;
            this.txtFiltroInsNombre.Text = string.Empty;
        }

        private void CargarActividades()
        {
            try
            {
                _actividades = _actividadServicio.GetAllActividades();
            }
            catch (Exception ex)
            {
                Debug.WriteLine(ex.ToString());
                MessageBox.Show("No se pudo cargar la lista de actividades.", "Gestionar actividades", MessageBoxButtons.OK, MessageBoxIcon.Error);
                _actividades = new List<Actividad>();
            }

            AplicarFiltroActividades();
        }

        private void AplicarFiltroActividades()
        {
            this.dgvActividades.Rows.Clear();
            LimpiarCampos();
            LimpiarInscriptos();

            string filtroNombre = this.txtFiltroNombreAct.Text.Trim();
            string filtroCapacidad = this.txtFiltroCapacidad.Text.Trim();

            IEnumerable<Actividad> consulta = _actividades;

            if (filtroNombre.Length > 0)
            {
                consulta = consulta.Where(a => a.Nombre != null && a.Nombre.IndexOf(filtroNombre, StringComparison.OrdinalIgnoreCase) >= 0);
            }

            if (filtroCapacidad.Length > 0)
            {
                if (int.TryParse(filtroCapacidad, out int capacidadMinima))
                {
                    consulta = consulta.Where(a => a.CapacidadMaxima >= capacidadMinima);
                }
                else
                {
                    consulta = Enumerable.Empty<Actividad>();
                }
            }

            _actividadesMostradas = consulta.ToList();

            foreach (Actividad actividad in _actividadesMostradas)
            {
                this.dgvActividades.Rows.Add(actividad.Nombre, actividad.CapacidadMaxima);
            }
        }

        private void CargarInscriptos()
        {
            this.dgvInscriptos.Rows.Clear();
            _inscripcionesActuales = new List<Inscripcion>();
            _inscripcionesMostradas = new List<Inscripcion>();

            Actividad actividad = ActividadSeleccionada();

            if (actividad == null)
            {
                this.lblInscriptosResumen.Text = string.Empty;
                return;
            }

            try
            {
                _inscripcionesActuales = _inscripcionServicio.GetInscripcionesByActividad(actividad.Id);
                AplicarFiltroInscriptos();
            }
            catch (Exception ex)
            {
                Debug.WriteLine(ex.ToString());
                MessageBox.Show("No se pudo cargar la lista de inscriptos.", "Gestionar actividades", MessageBoxButtons.OK, MessageBoxIcon.Error);
            }
        }

        private void AplicarFiltroInscriptos()
        {
            this.dgvInscriptos.Rows.Clear();

            Actividad actividad = ActividadSeleccionada();

            if (actividad == null)
            {
                _inscripcionesMostradas = new List<Inscripcion>();
                this.lblInscriptosResumen.Text = string.Empty;
                return;
            }

            string filtroDni = this.txtFiltroInsDni.Text.Trim();
            string filtroNombre = this.txtFiltroInsNombre.Text.Trim();

            IEnumerable<Inscripcion> consulta = _inscripcionesActuales;

            if (filtroDni.Length > 0)
            {
                consulta = consulta.Where(i => i.Cliente != null && i.Cliente.Dni.ToString().Contains(filtroDni));
            }

            if (filtroNombre.Length > 0)
            {
                consulta = consulta.Where(i => i.Cliente != null
                    && ((i.Cliente.Nombre != null && i.Cliente.Nombre.IndexOf(filtroNombre, StringComparison.OrdinalIgnoreCase) >= 0)
                        || (i.Cliente.Apellido != null && i.Cliente.Apellido.IndexOf(filtroNombre, StringComparison.OrdinalIgnoreCase) >= 0)));
            }

            _inscripcionesMostradas = consulta.ToList();

            foreach (Inscripcion ins in _inscripcionesMostradas)
            {
                string dni = ins.Cliente != null ? ins.Cliente.Dni.ToString() : string.Empty;
                string apellido = ins.Cliente != null ? ins.Cliente.Apellido : string.Empty;
                string nombre = ins.Cliente != null ? ins.Cliente.Nombre : string.Empty;
                string fecha = ins.Fecha.ToString("dd/MM/yyyy");
                this.dgvInscriptos.Rows.Add(dni, apellido, nombre, fecha);
            }

            int inscriptos = _inscripcionesActuales.Count;
            int disponibles = actividad.CapacidadMaxima - inscriptos;
            this.lblInscriptosResumen.Text = "Mostrando " + _inscripcionesMostradas.Count + " de " + inscriptos + " inscriptos | Capacidad: " + actividad.CapacidadMaxima + " (" + disponibles + " disponibles)";
        }

        private void LimpiarInscriptos()
        {
            this.dgvInscriptos.Rows.Clear();
            _inscripcionesActuales = new List<Inscripcion>();
            _inscripcionesMostradas = new List<Inscripcion>();
            this.lblInscriptosResumen.Text = string.Empty;
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

            if (indice < 0 || indice >= _actividadesMostradas.Count)
            {
                return null;
            }

            return _actividadesMostradas[indice];
        }

        private bool LeerCampos(Actividad actividad)
        {
            int cupo;

            if (!int.TryParse(this.txtCupo.Text, out cupo))
            {
                MessageBox.Show("Ingrese una capacidad numérica.", "Gestionar actividades", MessageBoxButtons.OK, MessageBoxIcon.Warning);
                return false;
            }

            actividad.Nombre = this.txtNombre.Text.Trim();
            actividad.CapacidadMaxima = cupo;
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
            this.txtCupo.Text = actividad.CapacidadMaxima.ToString();

            CargarInscriptos();
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
                    MessageBox.Show("No se pudo dar de alta. Ingrese un nombre y una capacidad entre 1 y " + ActividadServicio.CapacidadMaximaPermitida + ".", "Gestionar actividades", MessageBoxButtons.OK, MessageBoxIcon.Warning);
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
                    MessageBox.Show("No se pudo modificar. Verifique que el nombre no esté vacío, que la capacidad esté entre 1 y " + ActividadServicio.CapacidadMaximaPermitida + " y que no sea menor a la cantidad de socios inscriptos actualmente.", "Gestionar actividades", MessageBoxButtons.OK, MessageBoxIcon.Warning);
                    return;
                }

                CargarActividades();
            }
            catch (Exception ex)
            {
                Debug.WriteLine(ex.ToString());
                if (ConcurrenciaUI.EsConflictoDeConcurrencia(ex))
                {
                    MessageBox.Show("La modificación no pudo completarse debido a una contienda simultánea con otra operación. Intente nuevamente.", "Gestionar actividades", MessageBoxButtons.OK, MessageBoxIcon.Warning);
                }
                else
                {
                    MessageBox.Show("Ocurrió un error al modificar la actividad.", "Gestionar actividades", MessageBoxButtons.OK, MessageBoxIcon.Error);
                }
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

        private void btnDesinscribir_Click(object sender, EventArgs e)
        {
            Actividad actividad = ActividadSeleccionada();

            if (actividad == null)
            {
                MessageBox.Show("Seleccione una actividad de la lista.", "Gestionar actividades", MessageBoxButtons.OK, MessageBoxIcon.Information);
                return;
            }

            if (this.dgvInscriptos.CurrentRow == null)
            {
                MessageBox.Show("Seleccione un socio inscripto.", "Gestionar actividades", MessageBoxButtons.OK, MessageBoxIcon.Information);
                return;
            }

            int indice = this.dgvInscriptos.CurrentRow.Index;

            if (indice < 0 || indice >= _inscripcionesMostradas.Count)
            {
                return;
            }

            Inscripcion inscripcion = _inscripcionesMostradas[indice];
            string nombreCliente = inscripcion.Cliente != null
                ? inscripcion.Cliente.Apellido + " " + inscripcion.Cliente.Nombre
                : "el socio seleccionado";

            if (MessageBox.Show("¿Desinscribir a " + nombreCliente + " de " + actividad.Nombre + "?", "Gestionar actividades", MessageBoxButtons.YesNo, MessageBoxIcon.Question) != DialogResult.Yes)
            {
                return;
            }

            try
            {
                if (!_inscripcionServicio.DesinscribirCliente(inscripcion.ClienteId, actividad.Id))
                {
                    MessageBox.Show("No se pudo desinscribir al socio.", "Gestionar actividades", MessageBoxButtons.OK, MessageBoxIcon.Warning);
                    return;
                }

                CargarActividades();
            }
            catch (Exception ex)
            {
                Debug.WriteLine(ex.ToString());
                if (ConcurrenciaUI.EsConflictoDeConcurrencia(ex))
                {
                    MessageBox.Show("La desinscripción no pudo completarse debido a una contienda simultánea con otra operación. Intente nuevamente.", "Gestionar actividades", MessageBoxButtons.OK, MessageBoxIcon.Warning);
                }
                else
                {
                    MessageBox.Show("Ocurrió un error al desinscribir al socio.", "Gestionar actividades", MessageBoxButtons.OK, MessageBoxIcon.Error);
                }
            }
        }
    }
}
