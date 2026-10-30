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
        private Label lblNoProcesadosTitulo;
        private ListBox lstNoProcesados;

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
            this.lblNoProcesadosTitulo = new Label();
            this.lstNoProcesados = new ListBox();

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

            this.lblNoProcesadosTitulo.Location = new Point(20, 190);
            this.lblNoProcesadosTitulo.Size = new Size(460, 20);
            this.lblNoProcesadosTitulo.Text = string.Empty;

            this.lstNoProcesados.Location = new Point(20, 215);
            this.lstNoProcesados.Size = new Size(460, 150);

            this.Text = "Débito mensual";
            this.Size = new Size(520, 430);
            this.FormBorderStyle = FormBorderStyle.FixedDialog;
            this.MaximizeBox = false;
            this.StartPosition = FormStartPosition.CenterParent;

            this.Controls.Add(this.lblPeriodo);
            this.Controls.Add(this.lblDescripcion);
            this.Controls.Add(this.btnEmitir);
            this.Controls.Add(this.lblResumen);
            this.Controls.Add(this.lblNoProcesadosTitulo);
            this.Controls.Add(this.lstNoProcesados);
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
            this.lstNoProcesados.Items.Clear();
            this.lblResumen.Text = string.Empty;
            this.lblNoProcesadosTitulo.Text = string.Empty;

            bool habilitarBoton = true;

            try
            {
                ResultadoDebitoMensual resultado = _debitoMensualServicio.EmitirDebitosMensuales();

                this.lblResumen.Text = "Emitidos: " + resultado.Emitidos
                    + "  |  Ya emitidos en el período: " + resultado.YaEmitidos
                    + "  |  No procesados: " + resultado.NoProcesados.Count;

                if (resultado.NoProcesados.Count > 0)
                {
                    this.lblNoProcesadosTitulo.Text = "Operaciones no procesadas:";

                    foreach (string mensaje in resultado.NoProcesados)
                    {
                        this.lstNoProcesados.Items.Add(mensaje);
                    }
                }
                else
                {
                    habilitarBoton = false;
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
