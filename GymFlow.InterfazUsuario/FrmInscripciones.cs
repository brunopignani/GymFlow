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
    public class FrmInscripciones : Form
    {
        private class ActividadConCupo
        {
            public Actividad Actividad { get; set; }
            public int Inscriptos { get; set; }
            public int Disponibles { get; set; }
        }

        private readonly ClienteServicio _clienteServicio;
        private readonly InscripcionServicio _inscripcionServicio;
        private readonly ActividadServicio _actividadServicio;

        private Cliente _clienteCargado;
        private CuotaCliente _cuotaClienteCargada;
        private List<ActividadConCupo> _actividades;
        private List<ActividadConCupo> _actividadesMostradas;

        private Label lblDniTitulo;
        private TextBox txtBuscarDni;
        private Button btnBuscar;
        private Label lblNombreCliente;
        private Label lblEstadoCuota;
        private Label lblInscripcionesCliente;

        private Label lblActividadesTitulo;
        private Label lblFiltroActividad;
        private TextBox txtFiltroActividad;
        private CheckBox chkSoloConCupo;
        private Button btnLimpiarFiltros;
        private DataGridView dgvActividades;
        private Button btnInscribir;
        private Label lblResumenCupos;

        public FrmInscripciones(ClienteServicio clienteServicio, InscripcionServicio inscripcionServicio, ActividadServicio actividadServicio)
        {
            _clienteServicio = clienteServicio;
            _inscripcionServicio = inscripcionServicio;
            _actividadServicio = actividadServicio;
            _actividades = new List<ActividadConCupo>();
            _actividadesMostradas = new List<ActividadConCupo>();

            InitializeComponent();
            CargarActividades();
        }

        private void InitializeComponent()
        {
            this.lblDniTitulo = new Label();
            this.txtBuscarDni = new TextBox();
            this.btnBuscar = new Button();
            this.lblNombreCliente = new Label();
            this.lblEstadoCuota = new Label();
            this.lblInscripcionesCliente = new Label();

            this.lblActividadesTitulo = new Label();
            this.lblFiltroActividad = new Label();
            this.txtFiltroActividad = new TextBox();
            this.chkSoloConCupo = new CheckBox();
            this.btnLimpiarFiltros = new Button();
            this.dgvActividades = new DataGridView();
            this.btnInscribir = new Button();
            this.lblResumenCupos = new Label();

            // Sección cliente
            this.lblDniTitulo.Location = new Point(20, 21);
            this.lblDniTitulo.Size = new Size(35, 20);
            this.lblDniTitulo.Text = "DNI:";

            this.txtBuscarDni.Location = new Point(60, 18);
            this.txtBuscarDni.Size = new Size(140, 20);

            this.btnBuscar.Location = new Point(210, 16);
            this.btnBuscar.Size = new Size(80, 25);
            this.btnBuscar.Text = "Buscar";
            this.btnBuscar.Click += new EventHandler(this.btnBuscar_Click);

            this.lblNombreCliente.Location = new Point(20, 50);
            this.lblNombreCliente.Size = new Size(660, 20);
            this.lblNombreCliente.Text = string.Empty;
            this.lblNombreCliente.Font = new Font(this.Font, FontStyle.Bold);

            this.lblEstadoCuota.Location = new Point(20, 72);
            this.lblEstadoCuota.Size = new Size(660, 20);
            this.lblEstadoCuota.Text = string.Empty;

            this.lblInscripcionesCliente.Location = new Point(20, 94);
            this.lblInscripcionesCliente.Size = new Size(660, 20);
            this.lblInscripcionesCliente.Text = string.Empty;

            // Sección actividades y cupos
            this.lblActividadesTitulo.Location = new Point(20, 124);
            this.lblActividadesTitulo.Size = new Size(300, 20);
            this.lblActividadesTitulo.Text = "Actividades y cupos disponibles:";
            this.lblActividadesTitulo.Font = new Font(this.Font, FontStyle.Bold);

            this.lblFiltroActividad.Location = new Point(20, 151);
            this.lblFiltroActividad.Size = new Size(60, 20);
            this.lblFiltroActividad.Text = "Actividad:";

            this.txtFiltroActividad.Location = new Point(85, 148);
            this.txtFiltroActividad.Size = new Size(180, 20);
            this.txtFiltroActividad.TextChanged += new EventHandler(this.FiltrosActividades_Changed);

            this.chkSoloConCupo.Location = new Point(280, 149);
            this.chkSoloConCupo.Size = new Size(165, 20);
            this.chkSoloConCupo.Text = "Solo con cupo disponible";
            this.chkSoloConCupo.CheckedChanged += new EventHandler(this.FiltrosActividades_Changed);

            this.btnLimpiarFiltros.Location = new Point(568, 146);
            this.btnLimpiarFiltros.Size = new Size(112, 25);
            this.btnLimpiarFiltros.Text = "Limpiar filtros";
            this.btnLimpiarFiltros.Click += new EventHandler(this.btnLimpiarFiltros_Click);

            this.dgvActividades.Location = new Point(20, 178);
            this.dgvActividades.Size = new Size(660, 145);
            this.dgvActividades.ReadOnly = true;
            this.dgvActividades.AllowUserToAddRows = false;
            this.dgvActividades.RowHeadersVisible = false;
            this.dgvActividades.SelectionMode = DataGridViewSelectionMode.FullRowSelect;
            this.dgvActividades.AutoSizeColumnsMode = DataGridViewAutoSizeColumnsMode.Fill;
            this.dgvActividades.Columns.Add("colActividad", "Actividad");
            this.dgvActividades.Columns.Add("colCapacidad", "Capacidad");
            this.dgvActividades.Columns.Add("colInscriptos", "Inscriptos");
            this.dgvActividades.Columns.Add("colDisponibles", "Cupos disponibles");
            this.dgvActividades.SelectionChanged += new EventHandler(this.dgvActividades_SelectionChanged);

            this.btnInscribir.Location = new Point(20, 332);
            this.btnInscribir.Size = new Size(210, 28);
            this.btnInscribir.Text = "Inscribir a actividad seleccionada";
            this.btnInscribir.Click += new EventHandler(this.btnInscribir_Click);

            this.lblResumenCupos.Location = new Point(245, 337);
            this.lblResumenCupos.Size = new Size(435, 20);
            this.lblResumenCupos.Text = string.Empty;

            this.Text = "Inscripción a actividades";
            this.ClientSize = new Size(700, 380);
            this.FormBorderStyle = FormBorderStyle.FixedDialog;
            this.MaximizeBox = false;
            this.StartPosition = FormStartPosition.CenterParent;
            this.AcceptButton = this.btnBuscar;

            this.Controls.Add(this.lblDniTitulo);
            this.Controls.Add(this.txtBuscarDni);
            this.Controls.Add(this.btnBuscar);
            this.Controls.Add(this.lblNombreCliente);
            this.Controls.Add(this.lblEstadoCuota);
            this.Controls.Add(this.lblInscripcionesCliente);

            this.Controls.Add(this.lblActividadesTitulo);
            this.Controls.Add(this.lblFiltroActividad);
            this.Controls.Add(this.txtFiltroActividad);
            this.Controls.Add(this.chkSoloConCupo);
            this.Controls.Add(this.btnLimpiarFiltros);
            this.Controls.Add(this.dgvActividades);
            this.Controls.Add(this.btnInscribir);
            this.Controls.Add(this.lblResumenCupos);
        }

        private void FiltrosActividades_Changed(object sender, EventArgs e)
        {
            AplicarFiltroActividades();
        }

        private void btnLimpiarFiltros_Click(object sender, EventArgs e)
        {
            this.txtFiltroActividad.Text = string.Empty;
            this.chkSoloConCupo.Checked = false;
        }

        private void CargarActividades(int? actividadSeleccionadaId = null)
        {
            try
            {
                _actividades = _actividadServicio.GetAllActividades()
                    .Select(a =>
                    {
                        List<Inscripcion> inscripciones = _inscripcionServicio.GetInscripcionesByActividad(a.Id) ?? new List<Inscripcion>();
                        int inscriptos = inscripciones.Count;
                        int disponibles = a.CapacidadMaxima - inscriptos;
                        return new ActividadConCupo
                        {
                            Actividad = a,
                            Inscriptos = inscriptos,
                            Disponibles = disponibles
                        };
                    })
                    .ToList();
            }
            catch (Exception ex)
            {
                Debug.WriteLine(ex.ToString());
                MessageBox.Show("No se pudo cargar la lista de actividades.", "Recepción", MessageBoxButtons.OK, MessageBoxIcon.Error);
                _actividades = new List<ActividadConCupo>();
            }

            AplicarFiltroActividades(actividadSeleccionadaId);
        }

        private void AplicarFiltroActividades(int? actividadSeleccionadaId = null)
        {
            this.dgvActividades.Rows.Clear();

            string filtroNombre = this.txtFiltroActividad.Text.Trim();
            bool soloConCupo = this.chkSoloConCupo.Checked;

            IEnumerable<ActividadConCupo> consulta = _actividades;

            if (filtroNombre.Length > 0)
            {
                consulta = consulta.Where(a => a.Actividad.Nombre != null
                    && a.Actividad.Nombre.IndexOf(filtroNombre, StringComparison.OrdinalIgnoreCase) >= 0);
            }

            if (soloConCupo)
            {
                consulta = consulta.Where(a => a.Disponibles > 0);
            }

            _actividadesMostradas = consulta.ToList();

            int indiceASeleccionar = -1;

            for (int i = 0; i < _actividadesMostradas.Count; i++)
            {
                ActividadConCupo item = _actividadesMostradas[i];
                this.dgvActividades.Rows.Add(
                    item.Actividad.Nombre,
                    item.Actividad.CapacidadMaxima,
                    item.Inscriptos,
                    item.Disponibles);

                if (actividadSeleccionadaId.HasValue && item.Actividad.Id == actividadSeleccionadaId.Value)
                {
                    indiceASeleccionar = i;
                }
            }

            if (indiceASeleccionar >= 0)
            {
                this.dgvActividades.ClearSelection();
                this.dgvActividades.Rows[indiceASeleccionar].Selected = true;
                this.dgvActividades.CurrentCell = this.dgvActividades.Rows[indiceASeleccionar].Cells[0];
            }

            ActualizarResumenActividadSeleccionada();
        }

        private ActividadConCupo ActividadSeleccionada()
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

        private void dgvActividades_SelectionChanged(object sender, EventArgs e)
        {
            ActualizarResumenActividadSeleccionada();
        }

        private void ActualizarResumenActividadSeleccionada()
        {
            ActividadConCupo seleccionada = ActividadSeleccionada();

            if (seleccionada == null)
            {
                this.lblResumenCupos.Text = string.Empty;
                return;
            }

            this.lblResumenCupos.Text = seleccionada.Actividad.Nombre
                + " | Capacidad: " + seleccionada.Actividad.CapacidadMaxima
                + " | Inscriptos: " + seleccionada.Inscriptos
                + " | Disponibles: " + seleccionada.Disponibles;
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

                List<Inscripcion> inscripcionesCliente = _inscripcionServicio.GetInscripcionesByCliente(_clienteCargado.Id) ?? new List<Inscripcion>();
                List<string> nombresActividades = inscripcionesCliente
                    .Where(i => i.Actividad != null && !string.IsNullOrWhiteSpace(i.Actividad.Nombre))
                    .Select(i => i.Actividad.Nombre)
                    .Distinct()
                    .ToList();

                this.lblInscripcionesCliente.Text = nombresActividades.Count > 0
                    ? "Inscripciones actuales del cliente: " + string.Join(", ", nombresActividades)
                    : "Inscripciones actuales del cliente: Ninguna";
            }
            else
            {
                this.lblNombreCliente.Text = string.Empty;
                this.lblEstadoCuota.Text = string.Empty;
                this.lblInscripcionesCliente.Text = string.Empty;
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

            ActividadConCupo seleccionada = ActividadSeleccionada();

            if (seleccionada == null)
            {
                MessageBox.Show("Seleccione una actividad.", "Recepción", MessageBoxButtons.OK, MessageBoxIcon.Information);
                return;
            }

            int actividadId = seleccionada.Actividad.Id;

            try
            {
                if (_inscripcionServicio.InscribirCliente(_clienteCargado.Id, actividadId))
                {
                    MessageBox.Show("El cliente fue inscripto correctamente.", "Recepción", MessageBoxButtons.OK, MessageBoxIcon.Information);
                    CargarActividades(actividadId);
                    RefrescarCliente();
                }
                else
                {
                    MessageBox.Show("No se pudo inscribir. Verifique que la cuota esté al día, que haya cupo disponible y que el cliente no esté ya inscripto.", "Recepción", MessageBoxButtons.OK, MessageBoxIcon.Warning);
                }
            }
            catch (Exception ex)
            {
                Debug.WriteLine(ex.ToString());
                if (ConcurrenciaUI.EsConflictoDeConcurrencia(ex))
                {
                    MessageBox.Show("La inscripción no pudo completarse debido a una contienda simultánea de cupos con otra operación. Por favor, intente nuevamente.", "Recepción", MessageBoxButtons.OK, MessageBoxIcon.Warning);
                }
                else
                {
                    MessageBox.Show("Ocurrió un error al inscribir al cliente. Intente nuevamente.", "Recepción", MessageBoxButtons.OK, MessageBoxIcon.Error);
                }
            }
        }
    }
}
