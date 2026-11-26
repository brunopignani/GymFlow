using GymFlow.Aplicacion;
using GymFlow.Dominio;
using System;
using System.Diagnostics;
using System.Drawing;
using System.Globalization;
using System.Windows.Forms;

namespace GymFlow.InterfazUsuario
{
    public class FrmPrecioCuota : Form
    {
        private readonly CuotaServicio _cuotaServicio;

        private Label lblPrecioActualTitulo;
        private Label lblPrecioActualValor;
        private Label lblNuevoPrecio;
        private TextBox txtNuevoPrecio;
        private Button btnGuardar;
        private Button btnCancelar;
        private ErrorProvider errorProvider;

        public FrmPrecioCuota(CuotaServicio cuotaServicio)
        {
            _cuotaServicio = cuotaServicio;
            InitializeComponent();
            CargarPrecioActual();
        }

        private void InitializeComponent()
        {
            this.lblPrecioActualTitulo = new Label();
            this.lblPrecioActualValor = new Label();
            this.lblNuevoPrecio = new Label();
            this.txtNuevoPrecio = new TextBox();
            this.btnGuardar = new Button();
            this.btnCancelar = new Button();
            this.errorProvider = new ErrorProvider(this);

            this.lblPrecioActualTitulo.Location = new Point(20, 22);
            this.lblPrecioActualTitulo.Size = new Size(125, 20);
            this.lblPrecioActualTitulo.Text = "Precio actual (Mensual):";

            this.lblPrecioActualValor.Location = new Point(150, 22);
            this.lblPrecioActualValor.Size = new Size(160, 20);
            this.lblPrecioActualValor.Font = new Font(this.lblPrecioActualValor.Font, FontStyle.Bold);

            this.lblNuevoPrecio.Location = new Point(20, 58);
            this.lblNuevoPrecio.Size = new Size(125, 20);
            this.lblNuevoPrecio.Text = "Nuevo precio ($):";

            this.txtNuevoPrecio.Location = new Point(150, 55);
            this.txtNuevoPrecio.Size = new Size(160, 20);

            this.btnGuardar.Location = new Point(110, 100);
            this.btnGuardar.Size = new Size(95, 30);
            this.btnGuardar.Text = "Guardar";
            this.btnGuardar.Click += new EventHandler(this.btnGuardar_Click);

            this.btnCancelar.Location = new Point(215, 100);
            this.btnCancelar.Size = new Size(95, 30);
            this.btnCancelar.Text = "Cancelar";
            this.btnCancelar.DialogResult = DialogResult.Cancel;

            this.Text = "Precio de cuota mensual";
            this.Size = new Size(350, 185);
            this.FormBorderStyle = FormBorderStyle.FixedDialog;
            this.MaximizeBox = false;
            this.StartPosition = FormStartPosition.CenterParent;
            this.AcceptButton = this.btnGuardar;
            this.CancelButton = this.btnCancelar;

            this.Controls.Add(this.lblPrecioActualTitulo);
            this.Controls.Add(this.lblPrecioActualValor);
            this.Controls.Add(this.lblNuevoPrecio);
            this.Controls.Add(this.txtNuevoPrecio);
            this.Controls.Add(this.btnGuardar);
            this.Controls.Add(this.btnCancelar);
        }

        private void CargarPrecioActual()
        {
            try
            {
                Cuota cuota = _cuotaServicio.ObtenerCuotaMensual();
                if (cuota == null)
                {
                    this.lblPrecioActualValor.Text = "No configurada";
                    this.btnGuardar.Enabled = false;
                    return;
                }

                this.lblPrecioActualValor.Text = "$ " + cuota.Precio.ToString("N2");
                this.txtNuevoPrecio.Text = cuota.Precio.ToString("0.00", CultureInfo.CurrentCulture);
            }
            catch (Exception ex)
            {
                Debug.WriteLine(ex.ToString());
                MessageBox.Show("No se pudo obtener el precio actual de la cuota mensual.", "Precio de cuota mensual", MessageBoxButtons.OK, MessageBoxIcon.Error);
                this.btnGuardar.Enabled = false;
            }
        }

        private void btnGuardar_Click(object sender, EventArgs e)
        {
            this.errorProvider.Clear();

            string texto = (this.txtNuevoPrecio.Text ?? string.Empty).Trim();
            if (!decimal.TryParse(texto, NumberStyles.Number, CultureInfo.CurrentCulture, out decimal nuevoPrecio) || nuevoPrecio <= 0)
            {
                this.errorProvider.SetError(this.txtNuevoPrecio, "Ingrese un importe válido mayor a cero.");
                return;
            }

            try
            {
                if (!_cuotaServicio.ActualizarPrecioMensual(nuevoPrecio))
                {
                    MessageBox.Show("No se pudo actualizar el precio. Verifique que posea permisos y que el importe sea válido.", "Precio de cuota mensual", MessageBoxButtons.OK, MessageBoxIcon.Warning);
                    return;
                }

                MessageBox.Show("Precio de la cuota mensual actualizado correctamente.", "Precio de cuota mensual", MessageBoxButtons.OK, MessageBoxIcon.Information);
                this.DialogResult = DialogResult.OK;
            }
            catch (Exception ex)
            {
                Debug.WriteLine(ex.ToString());
                MessageBox.Show("Ocurrió un error al actualizar el precio de la cuota mensual.", "Precio de cuota mensual", MessageBoxButtons.OK, MessageBoxIcon.Error);
            }
        }
    }
}
