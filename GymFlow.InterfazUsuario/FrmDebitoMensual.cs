using GymFlow.Aplicacion;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Drawing;
using System.Linq;
using System.Windows.Forms;

namespace GymFlow.InterfazUsuario
{
    public class FrmDebitoMensual : Form
    {
        private readonly DebitoMensualServicio _debitoMensualServicio;

        private List<DetalleDebitoMensual> _detallesActuales;

        private Label lblPeriodo;
        private Label lblDescripcion;
        private Button btnEmitir;
        private Label lblResumen;
        private Label lblDetalleTitulo;
        private Label lblFiltroResultado;
        private ComboBox cmbFiltroResultado;
        private Label lblFiltroDni;
        private TextBox txtFiltroDni;
        private Label lblFiltroCliente;
        private TextBox txtFiltroCliente;
        private Button btnLimpiarFiltros;
        private DataGridView dgvDetalle;

        public FrmDebitoMensual(DebitoMensualServicio debitoMensualServicio)
        {
            _debitoMensualServicio = debitoMensualServicio;
            _detallesActuales = new List<DetalleDebitoMensual>();

            InitializeComponent();
        }

        private void InitializeComponent()
        {
            this.lblPeriodo = new Label();
            this.lblDescripcion = new Label();
            this.btnEmitir = new Button();
            this.lblResumen = new Label();
            this.lblDetalleTitulo = new Label();
            this.lblFiltroResultado = new Label();
            this.cmbFiltroResultado = new ComboBox();
            this.lblFiltroDni = new Label();
            this.txtFiltroDni = new TextBox();
            this.lblFiltroCliente = new Label();
            this.txtFiltroCliente = new TextBox();
            this.btnLimpiarFiltros = new Button();
            this.dgvDetalle = new DataGridView();

            this.lblPeriodo.Location = new Point(20, 20);
            this.lblPeriodo.Size = new Size(760, 20);
            this.lblPeriodo.Text = "Período de débito: " + DateTime.Today.ToString("MMMM yyyy");
            this.lblPeriodo.Font = new Font(this.lblPeriodo.Font, FontStyle.Bold);

            this.lblDescripcion.Location = new Point(20, 48);
            this.lblDescripcion.Size = new Size(760, 42);
            this.lblDescripcion.Text = "Se emite una nota de venta por la cuota de cada cliente con cuenta corriente y cuota ya activada (activa o vencida). Los clientes pendientes de primera activación se informan como no procesados y los débitos ya emitidos del período se omiten.";

            this.btnEmitir.Location = new Point(20, 98);
            this.btnEmitir.Size = new Size(220, 32);
            this.btnEmitir.Text = "Emitir débitos del período";
            this.btnEmitir.Click += new EventHandler(this.btnEmitir_Click);

            this.lblResumen.Location = new Point(20, 142);
            this.lblResumen.Size = new Size(760, 20);
            this.lblResumen.Text = string.Empty;

            this.lblDetalleTitulo.Location = new Point(20, 168);
            this.lblDetalleTitulo.Size = new Size(760, 20);
            this.lblDetalleTitulo.Text = string.Empty;

            // Filtros de grilla
            this.lblFiltroResultado.Location = new Point(20, 198);
            this.lblFiltroResultado.Size = new Size(65, 20);
            this.lblFiltroResultado.Text = "Resultado:";

            this.cmbFiltroResultado.Location = new Point(88, 195);
            this.cmbFiltroResultado.Size = new Size(130, 21);
            this.cmbFiltroResultado.DropDownStyle = ComboBoxStyle.DropDownList;
            this.cmbFiltroResultado.Items.Add("Todos");
            this.cmbFiltroResultado.Items.Add("Emitido");
            this.cmbFiltroResultado.Items.Add("Ya emitido");
            this.cmbFiltroResultado.Items.Add("No procesado");
            this.cmbFiltroResultado.SelectedIndex = 0;
            this.cmbFiltroResultado.SelectedIndexChanged += new EventHandler(this.Filtros_Changed);

            this.lblFiltroDni.Location = new Point(232, 198);
            this.lblFiltroDni.Size = new Size(32, 20);
            this.lblFiltroDni.Text = "DNI:";

            this.txtFiltroDni.Location = new Point(268, 195);
            this.txtFiltroDni.Size = new Size(120, 20);
            this.txtFiltroDni.TextChanged += new EventHandler(this.Filtros_Changed);

            this.lblFiltroCliente.Location = new Point(402, 198);
            this.lblFiltroCliente.Size = new Size(48, 20);
            this.lblFiltroCliente.Text = "Cliente:";

            this.txtFiltroCliente.Location = new Point(454, 195);
            this.txtFiltroCliente.Size = new Size(200, 20);
            this.txtFiltroCliente.TextChanged += new EventHandler(this.Filtros_Changed);

            this.btnLimpiarFiltros.Location = new Point(668, 193);
            this.btnLimpiarFiltros.Size = new Size(112, 25);
            this.btnLimpiarFiltros.Text = "Limpiar filtros";
            this.btnLimpiarFiltros.Click += new EventHandler(this.btnLimpiarFiltros_Click);

            this.dgvDetalle.Location = new Point(20, 228);
            this.dgvDetalle.Size = new Size(760, 230);
            this.dgvDetalle.ReadOnly = true;
            this.dgvDetalle.AllowUserToAddRows = false;
            this.dgvDetalle.RowHeadersVisible = false;
            this.dgvDetalle.AutoSizeColumnsMode = DataGridViewAutoSizeColumnsMode.Fill;
            this.dgvDetalle.Columns.Add("colNombre", "Cliente");
            this.dgvDetalle.Columns.Add("colDni", "DNI");
            this.dgvDetalle.Columns.Add("colResultado", "Resultado");
            this.dgvDetalle.Columns.Add("colComprobante", "Comprobante");
            this.dgvDetalle.Columns.Add("colMotivo", "Motivo");

            this.Text = "Débito mensual";
            this.Size = new Size(820, 520);
            this.FormBorderStyle = FormBorderStyle.FixedDialog;
            this.MaximizeBox = false;
            this.StartPosition = FormStartPosition.CenterParent;

            this.Controls.Add(this.lblPeriodo);
            this.Controls.Add(this.lblDescripcion);
            this.Controls.Add(this.btnEmitir);
            this.Controls.Add(this.lblResumen);
            this.Controls.Add(this.lblDetalleTitulo);
            this.Controls.Add(this.lblFiltroResultado);
            this.Controls.Add(this.cmbFiltroResultado);
            this.Controls.Add(this.lblFiltroDni);
            this.Controls.Add(this.txtFiltroDni);
            this.Controls.Add(this.lblFiltroCliente);
            this.Controls.Add(this.txtFiltroCliente);
            this.Controls.Add(this.btnLimpiarFiltros);
            this.Controls.Add(this.dgvDetalle);
        }

        private void Filtros_Changed(object sender, EventArgs e)
        {
            AplicarFiltroDetalle();
        }

        private void btnLimpiarFiltros_Click(object sender, EventArgs e)
        {
            this.cmbFiltroResultado.SelectedIndex = 0;
            this.txtFiltroDni.Text = string.Empty;
            this.txtFiltroCliente.Text = string.Empty;
        }

        private void AplicarFiltroDetalle()
        {
            this.dgvDetalle.Rows.Clear();

            string filtroResultado = this.cmbFiltroResultado.SelectedItem as string;
            string filtroDni = this.txtFiltroDni.Text.Trim();
            string filtroCliente = this.txtFiltroCliente.Text.Trim();

            IEnumerable<DetalleDebitoMensual> consulta = _detallesActuales;

            if (!string.IsNullOrEmpty(filtroResultado) && filtroResultado != "Todos")
            {
                consulta = consulta.Where(d => string.Equals(d.Resultado, filtroResultado, StringComparison.OrdinalIgnoreCase));
            }

            if (filtroDni.Length > 0)
            {
                consulta = consulta.Where(d => d.Dni.ToString().Contains(filtroDni));
            }

            if (filtroCliente.Length > 0)
            {
                consulta = consulta.Where(d => d.Nombre != null && d.Nombre.IndexOf(filtroCliente, StringComparison.OrdinalIgnoreCase) >= 0);
            }

            List<DetalleDebitoMensual> mostrar = consulta.ToList();

            foreach (DetalleDebitoMensual detalle in mostrar)
            {
                this.dgvDetalle.Rows.Add(
                    detalle.Nombre,
                    detalle.Dni,
                    detalle.Resultado,
                    string.IsNullOrWhiteSpace(detalle.NumeroComprobante) ? "—" : detalle.NumeroComprobante,
                    string.IsNullOrWhiteSpace(detalle.Motivo) ? "—" : detalle.Motivo);
            }

            if (_detallesActuales.Count == 0)
            {
                this.lblDetalleTitulo.Text = string.Empty;
            }
            else
            {
                this.lblDetalleTitulo.Text = "Detalle de clientes procesados (mostrando " + mostrar.Count + " de " + _detallesActuales.Count + "):";
            }
        }

        private void btnEmitir_Click(object sender, EventArgs e)
        {
            string pregunta = "¿Emitir los débitos mensuales del período " + DateTime.Today.ToString("MMMM yyyy") + "?";

            if (MessageBox.Show(pregunta, "Débito mensual", MessageBoxButtons.YesNo, MessageBoxIcon.Question) != DialogResult.Yes)
            {
                return;
            }

            this.btnEmitir.Enabled = false;
            this.Cursor = Cursors.WaitCursor;
            this._detallesActuales = new List<DetalleDebitoMensual>();
            this.dgvDetalle.Rows.Clear();
            this.lblResumen.Text = string.Empty;
            this.lblDetalleTitulo.Text = string.Empty;

            bool habilitarBoton = true;

            try
            {
                ResultadoDebitoMensual resultado = _debitoMensualServicio.EmitirDebitosMensuales();

                this.lblResumen.Text = "Emitidos: " + resultado.Emitidos
                    + "  |  Ya emitidos en el período: " + resultado.YaEmitidos
                    + "  |  No procesados: " + resultado.NoProcesados.Count;

                this._detallesActuales = resultado.Detalles ?? new List<DetalleDebitoMensual>();

                if (this._detallesActuales.Count == 0)
                {
                    this.lblDetalleTitulo.Text = "Sin clientes para procesar.";
                }
                else
                {
                    AplicarFiltroDetalle();
                }
            }
            catch (Exception ex)
            {
                Debug.WriteLine(ex.ToString());
                MessageBox.Show("Ocurrió un error al emitir los débitos. Revise la cuenta de los clientes antes de reintentar.", "Débito mensual", MessageBoxButtons.OK, MessageBoxIcon.Error);
            }
            finally
            {
                this.Cursor = Cursors.Default;
                this.btnEmitir.Enabled = habilitarBoton;
            }
        }
    }
}
