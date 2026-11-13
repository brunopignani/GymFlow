using GymFlow.Aplicacion;
using System;
using System.Diagnostics;
using System.Drawing;
using System.Windows.Forms;

namespace GymFlow.InterfazUsuario
{
    public class FrmDebitoMensual : Form
    {
        private readonly DebitoMensualServicio _debitoMensualServicio;

        private Label lblPeriodo;
        private Label lblDescripcion;
        private Button btnEmitir;
        private Label lblResumen;
        private Label lblDetalleTitulo;
        private DataGridView dgvDetalle;

        public FrmDebitoMensual(DebitoMensualServicio debitoMensualServicio)
        {
            _debitoMensualServicio = debitoMensualServicio;

            InitializeComponent();
        }

        private void InitializeComponent()
        {
            this.lblPeriodo = new Label();
            this.lblDescripcion = new Label();
            this.btnEmitir = new Button();
            this.lblResumen = new Label();
            this.lblDetalleTitulo = new Label();
            this.dgvDetalle = new DataGridView();

            this.lblPeriodo.Location = new Point(20, 20);
            this.lblPeriodo.Size = new Size(460, 20);
            this.lblPeriodo.Text = "Período de débito: " + DateTime.Today.ToString("MMMM yyyy");
            this.lblPeriodo.Font = new Font(this.lblPeriodo.Font, FontStyle.Bold);

            this.lblDescripcion.Location = new Point(20, 50);
            this.lblDescripcion.Size = new Size(460, 50);
            this.lblDescripcion.Text = "Se emite una nota de venta por la cuota de cada cliente que tenga cuenta corriente y cuota asignada. Si el débito del período ya fue emitido para un cliente, se omite.";

            this.btnEmitir.Location = new Point(20, 110);
            this.btnEmitir.Size = new Size(220, 32);
            this.btnEmitir.Text = "Emitir débitos del período";
            this.btnEmitir.Click += new EventHandler(this.btnEmitir_Click);

            this.lblResumen.Location = new Point(20, 160);
            this.lblResumen.Size = new Size(460, 20);
            this.lblResumen.Text = string.Empty;

            this.lblDetalleTitulo.Location = new Point(20, 190);
            this.lblDetalleTitulo.Size = new Size(700, 20);
            this.lblDetalleTitulo.Text = string.Empty;

            this.dgvDetalle.Location = new Point(20, 215);
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
            this.Size = new Size(820, 510);
            this.FormBorderStyle = FormBorderStyle.FixedDialog;
            this.MaximizeBox = false;
            this.StartPosition = FormStartPosition.CenterParent;

            this.Controls.Add(this.lblPeriodo);
            this.Controls.Add(this.lblDescripcion);
            this.Controls.Add(this.btnEmitir);
            this.Controls.Add(this.lblResumen);
            this.Controls.Add(this.lblDetalleTitulo);
            this.Controls.Add(this.dgvDetalle);
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

                foreach (DetalleDebitoMensual detalle in resultado.Detalles)
                {
                    this.dgvDetalle.Rows.Add(
                        detalle.Nombre,
                        detalle.Dni,
                        detalle.Resultado,
                        string.IsNullOrWhiteSpace(detalle.NumeroComprobante) ? "—" : detalle.NumeroComprobante,
                        string.IsNullOrWhiteSpace(detalle.Motivo) ? "—" : detalle.Motivo);
                }

                this.lblDetalleTitulo.Text = resultado.Detalles.Count == 0
                    ? "Sin clientes para procesar."
                    : "Detalle de clientes procesados:";
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
