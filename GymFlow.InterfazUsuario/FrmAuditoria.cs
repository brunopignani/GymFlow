using GymFlow.Aplicacion;
using GymFlow.Dominio;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Text.Json;
using System.Windows.Forms;

namespace GymFlow.InterfazUsuario
{
    public class FrmAuditoria : Form
    {
        private readonly AuditoriaServicio _auditoriaServicio;
        private List<AuditoriaSesion> _todasLasSesiones = new List<AuditoriaSesion>();
        private List<AuditoriaCliente> _todosLosClientes = new List<AuditoriaCliente>();
        private List<AuditoriaCliente> _clientesMostrados = new List<AuditoriaCliente>();
        private bool _actualizandoFiltros;

        private TabControl tabAuditoria;
        private TabPage tabSesiones;
        private TabPage tabClientes;

        // Controles pestaña Sesiones
        private Label lblSesDesde;
        private DateTimePicker dtpSesDesde;
        private Label lblSesHasta;
        private DateTimePicker dtpSesHasta;
        private Label lblSesUsuario;
        private TextBox txtSesUsuario;
        private Label lblSesEvento;
        private ComboBox cmbSesEvento;
        private Button btnSesActualizar;
        private Button btnSesLimpiar;
        private Label lblSesResumen;
        private DataGridView dgvSesiones;

        // Controles pestaña Clientes
        private Label lblCliDesde;
        private DateTimePicker dtpCliDesde;
        private Label lblCliHasta;
        private DateTimePicker dtpCliHasta;
        private Label lblCliUsuario;
        private TextBox txtCliUsuario;
        private Label lblCliOperacion;
        private ComboBox cmbCliOperacion;
        private Label lblCliDni;
        private TextBox txtCliDni;
        private Button btnCliActualizar;
        private Button btnCliLimpiar;
        private Label lblCliResumen;
        private DataGridView dgvClientes;
        private Label lblCliDetalle;
        private TextBox txtCliDetalle;

        public FrmAuditoria(AuditoriaServicio auditoriaServicio)
        {
            _auditoriaServicio = auditoriaServicio;

            InitializeComponent();
            CargarSesiones();
            CargarClientes();
        }

        private void InitializeComponent()
        {
            this.tabAuditoria = new TabControl();
            this.tabSesiones = new TabPage("Sesiones (Login / Logout)");
            this.tabClientes = new TabPage("Cambios en Clientes");

            InicializarPestanaSesiones();
            InicializarPestanaClientes();

            this.tabAuditoria.Location = new Point(12, 12);
            this.tabAuditoria.Size = new Size(920, 560);
            this.tabAuditoria.TabPages.Add(this.tabSesiones);
            this.tabAuditoria.TabPages.Add(this.tabClientes);

            this.Text = "Auditoría del sistema";
            this.Size = new Size(960, 625);
            this.FormBorderStyle = FormBorderStyle.FixedDialog;
            this.MaximizeBox = false;
            this.StartPosition = FormStartPosition.CenterParent;

            this.Controls.Add(this.tabAuditoria);
        }

        private void InicializarPestanaSesiones()
        {
            this.lblSesDesde = new Label { Location = new Point(15, 18), Size = new Size(45, 20), Text = "Desde:" };
            this.dtpSesDesde = new DateTimePicker
            {
                Location = new Point(65, 14),
                Size = new Size(125, 21),
                Format = DateTimePickerFormat.Short,
                ShowCheckBox = true,
                Checked = false
            };
            this.dtpSesDesde.ValueChanged += new EventHandler(this.FiltrosSesiones_Changed);

            this.lblSesHasta = new Label { Location = new Point(205, 18), Size = new Size(45, 20), Text = "Hasta:" };
            this.dtpSesHasta = new DateTimePicker
            {
                Location = new Point(252, 14),
                Size = new Size(125, 21),
                Format = DateTimePickerFormat.Short,
                ShowCheckBox = true,
                Checked = false
            };
            this.dtpSesHasta.ValueChanged += new EventHandler(this.FiltrosSesiones_Changed);

            this.lblSesUsuario = new Label { Location = new Point(392, 18), Size = new Size(52, 20), Text = "Usuario:" };
            this.txtSesUsuario = new TextBox { Location = new Point(448, 14), Size = new Size(125, 21) };
            this.txtSesUsuario.TextChanged += new EventHandler(this.FiltrosSesiones_Changed);

            this.lblSesEvento = new Label { Location = new Point(583, 18), Size = new Size(48, 20), Text = "Evento:" };
            this.cmbSesEvento = new ComboBox
            {
                Location = new Point(634, 14),
                Size = new Size(100, 21),
                DropDownStyle = ComboBoxStyle.DropDownList
            };
            this.cmbSesEvento.Items.Add("Todos");
            this.cmbSesEvento.Items.Add(EventosAuditoriaSesion.Login);
            this.cmbSesEvento.Items.Add(EventosAuditoriaSesion.Logout);
            this.cmbSesEvento.SelectedIndex = 0;
            this.cmbSesEvento.SelectedIndexChanged += new EventHandler(this.FiltrosSesiones_Changed);

            this.btnSesActualizar = new Button { Location = new Point(744, 12), Size = new Size(78, 25), Text = "Actualizar" };
            this.btnSesActualizar.Click += new EventHandler(this.btnSesActualizar_Click);

            this.btnSesLimpiar = new Button { Location = new Point(828, 12), Size = new Size(70, 25), Text = "Limpiar" };
            this.btnSesLimpiar.Click += new EventHandler(this.btnSesLimpiar_Click);

            this.lblSesResumen = new Label { Location = new Point(15, 48), Size = new Size(880, 20), Text = string.Empty };

            this.dgvSesiones = new DataGridView
            {
                Location = new Point(15, 72),
                Size = new Size(883, 445),
                ReadOnly = true,
                AllowUserToAddRows = false,
                AllowUserToDeleteRows = false,
                RowHeadersVisible = false,
                SelectionMode = DataGridViewSelectionMode.FullRowSelect,
                MultiSelect = false,
                AutoSizeColumnsMode = DataGridViewAutoSizeColumnsMode.Fill
            };
            this.dgvSesiones.Columns.Add("colSesFecha", "Fecha y hora");
            this.dgvSesiones.Columns.Add("colSesUsuario", "Usuario");
            this.dgvSesiones.Columns.Add("colSesEvento", "Evento");

            this.tabSesiones.Controls.Add(this.lblSesDesde);
            this.tabSesiones.Controls.Add(this.dtpSesDesde);
            this.tabSesiones.Controls.Add(this.lblSesHasta);
            this.tabSesiones.Controls.Add(this.dtpSesHasta);
            this.tabSesiones.Controls.Add(this.lblSesUsuario);
            this.tabSesiones.Controls.Add(this.txtSesUsuario);
            this.tabSesiones.Controls.Add(this.lblSesEvento);
            this.tabSesiones.Controls.Add(this.cmbSesEvento);
            this.tabSesiones.Controls.Add(this.btnSesActualizar);
            this.tabSesiones.Controls.Add(this.btnSesLimpiar);
            this.tabSesiones.Controls.Add(this.lblSesResumen);
            this.tabSesiones.Controls.Add(this.dgvSesiones);
        }

        private void InicializarPestanaClientes()
        {
            this.lblCliDesde = new Label { Location = new Point(15, 18), Size = new Size(45, 20), Text = "Desde:" };
            this.dtpCliDesde = new DateTimePicker
            {
                Location = new Point(65, 14),
                Size = new Size(120, 21),
                Format = DateTimePickerFormat.Short,
                ShowCheckBox = true,
                Checked = false
            };
            this.dtpCliDesde.ValueChanged += new EventHandler(this.FiltrosClientes_Changed);

            this.lblCliHasta = new Label { Location = new Point(195, 18), Size = new Size(45, 20), Text = "Hasta:" };
            this.dtpCliHasta = new DateTimePicker
            {
                Location = new Point(242, 14),
                Size = new Size(120, 21),
                Format = DateTimePickerFormat.Short,
                ShowCheckBox = true,
                Checked = false
            };
            this.dtpCliHasta.ValueChanged += new EventHandler(this.FiltrosClientes_Changed);

            this.lblCliUsuario = new Label { Location = new Point(372, 18), Size = new Size(52, 20), Text = "Usuario:" };
            this.txtCliUsuario = new TextBox { Location = new Point(428, 14), Size = new Size(110, 21) };
            this.txtCliUsuario.TextChanged += new EventHandler(this.FiltrosClientes_Changed);

            this.lblCliOperacion = new Label { Location = new Point(548, 18), Size = new Size(64, 20), Text = "Operación:" };
            this.cmbCliOperacion = new ComboBox
            {
                Location = new Point(615, 14),
                Size = new Size(110, 21),
                DropDownStyle = ComboBoxStyle.DropDownList
            };
            this.cmbCliOperacion.Items.Add("Todas");
            this.cmbCliOperacion.Items.Add("Alta");
            this.cmbCliOperacion.Items.Add("Modificación");
            this.cmbCliOperacion.Items.Add("Baja");
            this.cmbCliOperacion.SelectedIndex = 0;
            this.cmbCliOperacion.SelectedIndexChanged += new EventHandler(this.FiltrosClientes_Changed);

            this.lblCliDni = new Label { Location = new Point(15, 48), Size = new Size(70, 20), Text = "DNI Cliente:" };
            this.txtCliDni = new TextBox { Location = new Point(90, 44), Size = new Size(120, 21) };
            this.txtCliDni.TextChanged += new EventHandler(this.FiltrosClientes_Changed);

            this.btnCliActualizar = new Button { Location = new Point(744, 12), Size = new Size(78, 25), Text = "Actualizar" };
            this.btnCliActualizar.Click += new EventHandler(this.btnCliActualizar_Click);

            this.btnCliLimpiar = new Button { Location = new Point(828, 12), Size = new Size(70, 25), Text = "Limpiar" };
            this.btnCliLimpiar.Click += new EventHandler(this.btnCliLimpiar_Click);

            this.lblCliResumen = new Label { Location = new Point(230, 48), Size = new Size(665, 20), Text = string.Empty };

            this.dgvClientes = new DataGridView
            {
                Location = new Point(15, 76),
                Size = new Size(883, 285),
                ReadOnly = true,
                AllowUserToAddRows = false,
                AllowUserToDeleteRows = false,
                RowHeadersVisible = false,
                SelectionMode = DataGridViewSelectionMode.FullRowSelect,
                MultiSelect = false,
                AutoSizeColumnsMode = DataGridViewAutoSizeColumnsMode.Fill
            };
            this.dgvClientes.Columns.Add("colCliFecha", "Fecha y hora");
            this.dgvClientes.Columns.Add("colCliUsuario", "Usuario");
            this.dgvClientes.Columns.Add("colCliOperacion", "Operación");
            this.dgvClientes.Columns.Add("colCliId", "ID Cliente");
            this.dgvClientes.Columns.Add("colCliDni", "DNI");
            this.dgvClientes.SelectionChanged += new EventHandler(this.dgvClientes_SelectionChanged);

            this.lblCliDetalle = new Label
            {
                Location = new Point(15, 370),
                Size = new Size(400, 20),
                Text = "Detalle del registro seleccionado (solo lectura):",
                Font = new Font(this.Font, FontStyle.Bold)
            };

            this.txtCliDetalle = new TextBox
            {
                Location = new Point(15, 392),
                Size = new Size(883, 125),
                ReadOnly = true,
                Multiline = true,
                ScrollBars = ScrollBars.Vertical,
                BackColor = SystemColors.Window
            };

            this.tabClientes.Controls.Add(this.lblCliDesde);
            this.tabClientes.Controls.Add(this.dtpCliDesde);
            this.tabClientes.Controls.Add(this.lblCliHasta);
            this.tabClientes.Controls.Add(this.dtpCliHasta);
            this.tabClientes.Controls.Add(this.lblCliUsuario);
            this.tabClientes.Controls.Add(this.txtCliUsuario);
            this.tabClientes.Controls.Add(this.lblCliOperacion);
            this.tabClientes.Controls.Add(this.cmbCliOperacion);
            this.tabClientes.Controls.Add(this.lblCliDni);
            this.tabClientes.Controls.Add(this.txtCliDni);
            this.tabClientes.Controls.Add(this.btnCliActualizar);
            this.tabClientes.Controls.Add(this.btnCliLimpiar);
            this.tabClientes.Controls.Add(this.lblCliResumen);
            this.tabClientes.Controls.Add(this.dgvClientes);
            this.tabClientes.Controls.Add(this.lblCliDetalle);
            this.tabClientes.Controls.Add(this.txtCliDetalle);
        }

        private void CargarSesiones()
        {
            try
            {
                _todasLasSesiones = _auditoriaServicio.ConsultarSesiones(null, null, null, null);
            }
            catch (Exception ex)
            {
                Debug.WriteLine(ex.ToString());
                MessageBox.Show("No se pudo consultar la auditoría de sesiones.", "Auditoría", MessageBoxButtons.OK, MessageBoxIcon.Error);
                _todasLasSesiones = new List<AuditoriaSesion>();
            }

            AplicarFiltroSesiones();
        }

        private void CargarClientes()
        {
            try
            {
                _todosLosClientes = _auditoriaServicio.ConsultarClientes(null, null, null, null, null);
            }
            catch (Exception ex)
            {
                Debug.WriteLine(ex.ToString());
                MessageBox.Show("No se pudo consultar la auditoría de clientes.", "Auditoría", MessageBoxButtons.OK, MessageBoxIcon.Error);
                _todosLosClientes = new List<AuditoriaCliente>();
            }

            AplicarFiltroClientes();
        }

        private void AplicarFiltroSesiones()
        {
            DateTime? desde = this.dtpSesDesde.Checked ? this.dtpSesDesde.Value.Date : (DateTime?)null;
            DateTime? hasta = this.dtpSesHasta.Checked ? this.dtpSesHasta.Value.Date.AddDays(1).AddTicks(-1) : (DateTime?)null;

            this.dgvSesiones.Rows.Clear();

            if (desde.HasValue && hasta.HasValue && desde.Value > hasta.Value)
            {
                this.lblSesResumen.Text = "Mostrando 0 de " + _todasLasSesiones.Count + " sesiones (rango de fechas inválido).";
                return;
            }

            string filtroUsuario = (this.txtSesUsuario.Text ?? string.Empty).Trim();
            string eventoSeleccionado = this.cmbSesEvento.SelectedItem as string;
            string filtroEvento = string.Equals(eventoSeleccionado, "Todos", StringComparison.OrdinalIgnoreCase)
                ? null
                : eventoSeleccionado;

            IEnumerable<AuditoriaSesion> consulta = _todasLasSesiones;

            if (desde.HasValue)
            {
                consulta = consulta.Where(a => a.FechaHora >= desde.Value);
            }

            if (hasta.HasValue)
            {
                consulta = consulta.Where(a => a.FechaHora <= hasta.Value);
            }

            if (filtroUsuario.Length > 0)
            {
                consulta = consulta.Where(a => a.NombreUsuario != null
                    && a.NombreUsuario.IndexOf(filtroUsuario, StringComparison.OrdinalIgnoreCase) >= 0);
            }

            if (!string.IsNullOrWhiteSpace(filtroEvento))
            {
                consulta = consulta.Where(a => string.Equals(a.TipoEvento, filtroEvento, StringComparison.OrdinalIgnoreCase));
            }

            List<AuditoriaSesion> filtrados = consulta.ToList();

            foreach (AuditoriaSesion registro in filtrados)
            {
                this.dgvSesiones.Rows.Add(
                    registro.FechaHora.ToString("dd/MM/yyyy HH:mm:ss"),
                    registro.NombreUsuario,
                    registro.TipoEvento);
            }

            this.lblSesResumen.Text = "Mostrando " + filtrados.Count + " de " + _todasLasSesiones.Count + " sesiones.";
        }

        private void AplicarFiltroClientes()
        {
            DateTime? desde = this.dtpCliDesde.Checked ? this.dtpCliDesde.Value.Date : (DateTime?)null;
            DateTime? hasta = this.dtpCliHasta.Checked ? this.dtpCliHasta.Value.Date.AddDays(1).AddTicks(-1) : (DateTime?)null;

            this.dgvClientes.SelectionChanged -= this.dgvClientes_SelectionChanged;
            this.dgvClientes.Rows.Clear();

            if (desde.HasValue && hasta.HasValue && desde.Value > hasta.Value)
            {
                _clientesMostrados = new List<AuditoriaCliente>();
                this.dgvClientes.SelectionChanged += this.dgvClientes_SelectionChanged;
                MostrarDetalleSeleccionado();
                this.lblCliResumen.Text = "Mostrando 0 de " + _todosLosClientes.Count + " registros (rango de fechas inválido).";
                return;
            }

            string filtroUsuario = (this.txtCliUsuario.Text ?? string.Empty).Trim();
            string filtroOperacion = MapearOperacionFiltro(this.cmbCliOperacion.SelectedItem as string);
            string filtroDni = (this.txtCliDni.Text ?? string.Empty).Trim();

            IEnumerable<AuditoriaCliente> consulta = _todosLosClientes;

            if (desde.HasValue)
            {
                consulta = consulta.Where(a => a.FechaHora >= desde.Value);
            }

            if (hasta.HasValue)
            {
                consulta = consulta.Where(a => a.FechaHora <= hasta.Value);
            }

            if (filtroUsuario.Length > 0)
            {
                consulta = consulta.Where(a => a.NombreUsuario != null
                    && a.NombreUsuario.IndexOf(filtroUsuario, StringComparison.OrdinalIgnoreCase) >= 0);
            }

            if (!string.IsNullOrWhiteSpace(filtroOperacion))
            {
                consulta = consulta.Where(a => string.Equals(a.Operacion, filtroOperacion, StringComparison.OrdinalIgnoreCase));
            }

            if (filtroDni.Length > 0)
            {
                consulta = consulta.Where(a => a.DniCliente.ToString().Contains(filtroDni));
            }

            _clientesMostrados = consulta.ToList();

            foreach (AuditoriaCliente registro in _clientesMostrados)
            {
                this.dgvClientes.Rows.Add(
                    registro.FechaHora.ToString("dd/MM/yyyy HH:mm:ss"),
                    registro.NombreUsuario,
                    FormatearOperacion(registro.Operacion),
                    registro.ClienteId,
                    registro.DniCliente);
            }

            this.dgvClientes.SelectionChanged += this.dgvClientes_SelectionChanged;
            MostrarDetalleSeleccionado();
            this.lblCliResumen.Text = "Mostrando " + _clientesMostrados.Count + " de " + _todosLosClientes.Count + " registros.";
        }

        private void MostrarDetalleSeleccionado()
        {
            if (this.dgvClientes.CurrentRow == null)
            {
                this.txtCliDetalle.Text = string.Empty;
                return;
            }

            int indice = this.dgvClientes.CurrentRow.Index;
            if (indice < 0 || indice >= _clientesMostrados.Count)
            {
                this.txtCliDetalle.Text = string.Empty;
                return;
            }

            AuditoriaCliente registro = _clientesMostrados[indice];
            StringBuilder sb = new StringBuilder();
            sb.AppendLine($"Fecha y hora: {registro.FechaHora:dd/MM/yyyy HH:mm:ss}   |   Usuario: {registro.NombreUsuario}   |   Operación: {FormatearOperacion(registro.Operacion)}   |   ID Cliente: {registro.ClienteId}   |   DNI: {registro.DniCliente}");
            sb.AppendLine();
            sb.AppendLine("Valores anteriores: " + FormatearSnapshotJson(registro.ValoresAnteriores));
            sb.AppendLine("Valores nuevos:     " + FormatearSnapshotJson(registro.ValoresNuevos));

            this.txtCliDetalle.Text = sb.ToString();
        }

        private static string MapearOperacionFiltro(string seleccion)
        {
            if (string.IsNullOrWhiteSpace(seleccion) || string.Equals(seleccion, "Todas", StringComparison.OrdinalIgnoreCase))
            {
                return null;
            }

            if (string.Equals(seleccion, "Modificación", StringComparison.OrdinalIgnoreCase))
            {
                return OperacionesAuditoria.Modificacion;
            }

            return seleccion;
        }

        private static string FormatearOperacion(string operacion)
        {
            if (string.Equals(operacion, OperacionesAuditoria.Modificacion, StringComparison.OrdinalIgnoreCase))
            {
                return "Modificación";
            }

            return operacion ?? string.Empty;
        }

        private static string FormatearSnapshotJson(string json)
        {
            if (string.IsNullOrWhiteSpace(json))
            {
                return "(Sin datos)";
            }

            try
            {
                using (JsonDocument doc = JsonDocument.Parse(json))
                {
                    if (doc.RootElement.ValueKind == JsonValueKind.Object)
                    {
                        List<string> partes = new List<string>();
                        foreach (JsonProperty prop in doc.RootElement.EnumerateObject())
                        {
                            partes.Add(prop.Name + ": " + prop.Value.ToString());
                        }
                        return string.Join("  |  ", partes);
                    }
                }
            }
            catch
            {
                // Si el contenido no es JSON estructurado, se muestra tal cual.
            }

            return json;
        }

        private void FiltrosSesiones_Changed(object sender, EventArgs e)
        {
            if (_actualizandoFiltros)
            {
                return;
            }

            AplicarFiltroSesiones();
        }

        private void btnSesActualizar_Click(object sender, EventArgs e)
        {
            CargarSesiones();
        }

        private void btnSesLimpiar_Click(object sender, EventArgs e)
        {
            _actualizandoFiltros = true;
            try
            {
                this.dtpSesDesde.Checked = false;
                this.dtpSesHasta.Checked = false;
                this.txtSesUsuario.Text = string.Empty;
                this.cmbSesEvento.SelectedIndex = 0;
            }
            finally
            {
                _actualizandoFiltros = false;
            }

            AplicarFiltroSesiones();
        }

        private void FiltrosClientes_Changed(object sender, EventArgs e)
        {
            if (_actualizandoFiltros)
            {
                return;
            }

            AplicarFiltroClientes();
        }

        private void btnCliActualizar_Click(object sender, EventArgs e)
        {
            CargarClientes();
        }

        private void btnCliLimpiar_Click(object sender, EventArgs e)
        {
            _actualizandoFiltros = true;
            try
            {
                this.dtpCliDesde.Checked = false;
                this.dtpCliHasta.Checked = false;
                this.txtCliUsuario.Text = string.Empty;
                this.cmbCliOperacion.SelectedIndex = 0;
                this.txtCliDni.Text = string.Empty;
            }
            finally
            {
                _actualizandoFiltros = false;
            }

            AplicarFiltroClientes();
        }

        private void dgvClientes_SelectionChanged(object sender, EventArgs e)
        {
            MostrarDetalleSeleccionado();
        }
    }
}
