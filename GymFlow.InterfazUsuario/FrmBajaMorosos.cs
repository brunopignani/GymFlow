using GymFlow.Aplicacion;
using System;
using System.Diagnostics;
using System.Drawing;
using System.Windows.Forms;

namespace GymFlow.InterfazUsuario
{
    public class FrmBajaMorosos : Form
    {
        private readonly InscripcionServicio _inscripcionServicio;

        private Label lblDescripcion;
        private Button btnEjecutar;
        private Label lblResumen;
        private Label lblNoProcesadosTitulo;
        private ListBox lstNoProcesados;

        public FrmBajaMorosos(InscripcionServicio inscripcionServicio)
        {
            _inscripcionServicio = inscripcionServicio;

            InitializeComponent();
        }

        private void InitializeComponent()
        {
            this.lblDescripcion = new Label();
            this.btnEjecutar = new Button();
            this.lblResumen = new Label();
            this.lblNoProcesadosTitulo = new Label();
            this.lstNoProcesados = new ListBox();

            this.lblDescripcion.Location = new Point(20, 20);
            this.lblDescripcion.Size = new Size(460, 60);
            this.lblDescripcion.Text = "Da de baja las inscripciones de los clientes cuya cuota está vencida y libera el cupo de cada actividad. Los clientes no se eliminan.";

            this.btnEjecutar.Location = new Point(20, 90);
            this.btnEjecutar.Size = new Size(260, 32);
            this.btnEjecutar.Text = "Dar de baja inscripciones de morosos";
            this.btnEjecutar.Click += new EventHandler(this.btnEjecutar_Click);

            this.lblResumen.Location = new Point(20, 140);
            this.lblResumen.Size = new Size(460, 20);
            this.lblResumen.Text = string.Empty;

            this.lblNoProcesadosTitulo.Location = new Point(20, 170);
            this.lblNoProcesadosTitulo.Size = new Size(460, 20);
            this.lblNoProcesadosTitulo.Text = string.Empty;

            this.lstNoProcesados.Location = new Point(20, 195);
            this.lstNoProcesados.Size = new Size(460, 130);

            this.Text = "Baja de inscripciones por morosidad";
            this.Size = new Size(520, 400);
            this.FormBorderStyle = FormBorderStyle.FixedDialog;
            this.MaximizeBox = false;
            this.StartPosition = FormStartPosition.CenterParent;

            this.Controls.Add(this.lblDescripcion);
            this.Controls.Add(this.btnEjecutar);
            this.Controls.Add(this.lblResumen);
            this.Controls.Add(this.lblNoProcesadosTitulo);
            this.Controls.Add(this.lstNoProcesados);
        }

        private void btnEjecutar_Click(object sender, EventArgs e)
        {
            string pregunta = "Se darán de baja las inscripciones de todos los clientes con cuota vencida. ¿Desea continuar?";

            if (MessageBox.Show(pregunta, "Baja por morosidad", MessageBoxButtons.YesNo, MessageBoxIcon.Question) != DialogResult.Yes)
            {
                return;
            }

            this.btnEjecutar.Enabled = false;
            this.Cursor = Cursors.WaitCursor;
            this.lstNoProcesados.Items.Clear();
            this.lblResumen.Text = string.Empty;
            this.lblNoProcesadosTitulo.Text = string.Empty;

            try
            {
                ResultadoBajaMorosos resultado = _inscripcionServicio.DarDeBajaInscripcionesDeMorosos();

                this.lblResumen.Text = "Clientes afectados: " + resultado.ClientesAfectados
                    + "  |  Inscripciones dadas de baja: " + resultado.InscripcionesEliminadas
                    + "  |  No procesados: " + resultado.NoProcesados.Count;

                if (resultado.NoProcesados.Count > 0)
                {
                    this.lblNoProcesadosTitulo.Text = "Clientes no procesados:";

                    foreach (string mensaje in resultado.NoProcesados)
                    {
                        this.lstNoProcesados.Items.Add(mensaje);
                    }
                }
            }
            catch (Exception ex)
            {
                Debug.WriteLine(ex.ToString());
                MessageBox.Show("Ocurrió un error durante el proceso. Revise las inscripciones antes de reintentar.", "Baja por morosidad", MessageBoxButtons.OK, MessageBoxIcon.Error);
            }
            finally
            {
                this.Cursor = Cursors.Default;
                this.btnEjecutar.Enabled = true;
            }
        }
    }
}
