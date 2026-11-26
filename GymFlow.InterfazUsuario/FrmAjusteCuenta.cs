using GymFlow.Aplicacion;
using GymFlow.Dominio;
using System;
using System.Diagnostics;
using System.Drawing;
using System.Globalization;
using System.Windows.Forms;

namespace GymFlow.InterfazUsuario
{
    public class FrmAjusteCuenta : Form
    {
        private static readonly CultureInfo CulturaMonetaria = CultureInfo.GetCultureInfo("es-AR");

        private readonly ClienteServicio _clienteServicio;
        private readonly CuentaCorrienteServicio _cuentaCorrienteServicio;
        private readonly AjusteCuentaServicio _ajusteCuentaServicio;

        private Cliente _clienteCargado;

        private Label lblDni;
        private TextBox txtDni;
        private Button btnBuscar;
        private Label lblCliente;
        private Label lblSaldo;
        private Label lblTipoTitulo;
        private ComboBox cmbTipo;
        private Label lblImporteTitulo;
        private TextBox txtImporte;
        private Label lblMotivoTitulo;
        private TextBox txtMotivo;
        private Label lblEmpleado;
        private Button btnRegistrar;

        public FrmAjusteCuenta(ClienteServicio clienteServicio, CuentaCorrienteServicio cuentaCorrienteServicio, AjusteCuentaServicio ajusteCuentaServicio)
        {
            _clienteServicio = clienteServicio;
            _cuentaCorrienteServicio = cuentaCorrienteServicio;
            _ajusteCuentaServicio = ajusteCuentaServicio;

            InitializeComponent();
        }

        private void InitializeComponent()
        {
            this.lblDni = new Label();
            this.txtDni = new TextBox();
            this.btnBuscar = new Button();
            this.lblCliente = new Label();
            this.lblSaldo = new Label();
            this.lblTipoTitulo = new Label();
            this.cmbTipo = new ComboBox();
            this.lblImporteTitulo = new Label();
            this.txtImporte = new TextBox();
            this.lblMotivoTitulo = new Label();
            this.txtMotivo = new TextBox();
            this.lblEmpleado = new Label();
            this.btnRegistrar = new Button();

            this.lblDni.Location = new Point(20, 23);
            this.lblDni.Size = new Size(40, 20);
            this.lblDni.Text = "DNI:";

            this.txtDni.Location = new Point(70, 20);
            this.txtDni.Size = new Size(120, 20);

            this.btnBuscar.Location = new Point(200, 18);
            this.btnBuscar.Size = new Size(80, 25);
            this.btnBuscar.Text = "Buscar";
            this.btnBuscar.Click += new EventHandler(this.btnBuscar_Click);

            this.lblCliente.Location = new Point(20, 60);
            this.lblCliente.Size = new Size(460, 20);

            this.lblSaldo.Location = new Point(20, 82);
            this.lblSaldo.Size = new Size(460, 20);

            this.lblTipoTitulo.Location = new Point(20, 118);
            this.lblTipoTitulo.Size = new Size(100, 20);
            this.lblTipoTitulo.Text = "Tipo de ajuste:";

            this.cmbTipo.Location = new Point(130, 115);
            this.cmbTipo.Size = new Size(200, 21);
            this.cmbTipo.DropDownStyle = ComboBoxStyle.DropDownList;
            this.cmbTipo.Items.Add(NombresTipoMovimiento.Debito);
            this.cmbTipo.Items.Add(NombresTipoMovimiento.Credito);
            this.cmbTipo.SelectedIndex = 0;

            this.lblImporteTitulo.Location = new Point(20, 153);
            this.lblImporteTitulo.Size = new Size(100, 20);
            this.lblImporteTitulo.Text = "Importe:";

            this.txtImporte.Location = new Point(130, 150);
            this.txtImporte.Size = new Size(120, 20);

            this.lblMotivoTitulo.Location = new Point(20, 188);
            this.lblMotivoTitulo.Size = new Size(100, 20);
            this.lblMotivoTitulo.Text = "Motivo:";

            this.txtMotivo.Location = new Point(130, 185);
            this.txtMotivo.Size = new Size(330, 20);
            this.txtMotivo.MaxLength = AjusteCuentaServicio.LongitudMaximaMotivo;
            this.txtMotivo.PlaceholderText = "Describa el motivo del ajuste (obligatorio)";

            this.lblEmpleado.Location = new Point(20, 223);
            this.lblEmpleado.Size = new Size(440, 20);
            this.lblEmpleado.Text = "Empleado responsable: Nº " + SesionActual.Instancia.EmpleadoId;

            this.btnRegistrar.Location = new Point(20, 255);
            this.btnRegistrar.Size = new Size(190, 32);
            this.btnRegistrar.Text = "Registrar ajuste";
            this.btnRegistrar.Enabled = false;
            this.btnRegistrar.Click += new EventHandler(this.btnRegistrar_Click);

            this.Text = "Ajuste de cuenta";
            this.Size = new Size(500, 350);
            this.FormBorderStyle = FormBorderStyle.FixedDialog;
            this.MaximizeBox = false;
            this.StartPosition = FormStartPosition.CenterParent;
            this.AcceptButton = this.btnBuscar;

            this.Controls.Add(this.lblDni);
            this.Controls.Add(this.txtDni);
            this.Controls.Add(this.btnBuscar);
            this.Controls.Add(this.lblCliente);
            this.Controls.Add(this.lblSaldo);
            this.Controls.Add(this.lblTipoTitulo);
            this.Controls.Add(this.cmbTipo);
            this.Controls.Add(this.lblImporteTitulo);
            this.Controls.Add(this.txtImporte);
            this.Controls.Add(this.lblMotivoTitulo);
            this.Controls.Add(this.txtMotivo);
            this.Controls.Add(this.lblEmpleado);
            this.Controls.Add(this.btnRegistrar);
        }

        private void btnBuscar_Click(object sender, EventArgs e)
        {
            LimpiarCliente();

            int dni;

            if (!ValidacionesDatos.TryParseDni(this.txtDni.Text, out dni))
            {
                MessageBox.Show("Ingrese un DNI válido.", "Ajuste de cuenta", MessageBoxButtons.OK, MessageBoxIcon.Warning);
                return;
            }

            this.Cursor = Cursors.WaitCursor;

            try
            {
                Cliente cliente = _clienteServicio.GetClienteByDNI(dni);

                if (cliente == null)
                {
                    MessageBox.Show("No existe un cliente con ese DNI.", "Ajuste de cuenta", MessageBoxButtons.OK, MessageBoxIcon.Information);
                    return;
                }

                _clienteCargado = cliente;
                this.lblCliente.Text = cliente.Apellido + " " + cliente.Nombre + " (DNI " + cliente.Dni + ")";

                if (!MostrarSaldo())
                {
                    LimpiarCliente();
                    MessageBox.Show("El cliente no tiene cuenta corriente.", "Ajuste de cuenta", MessageBoxButtons.OK, MessageBoxIcon.Information);
                    return;
                }

                this.btnRegistrar.Enabled = true;
            }
            catch (Exception ex)
            {
                Debug.WriteLine(ex.ToString());
                LimpiarCliente();
                MessageBox.Show("No se pudo consultar la cuenta del cliente. Intente nuevamente.", "Ajuste de cuenta", MessageBoxButtons.OK, MessageBoxIcon.Error);
            }
            finally
            {
                this.Cursor = Cursors.Default;
            }
        }

        private bool MostrarSaldo()
        {
            ResumenCuentaCorriente resumen = _cuentaCorrienteServicio.ConsultarCuentaCorriente(_clienteCargado.Id);

            if (resumen == null)
            {
                return false;
            }

            this.lblSaldo.Text = "Saldo (débitos - créditos): " + resumen.Saldo.ToString("C2", CulturaMonetaria);
            return true;
        }

        private void btnRegistrar_Click(object sender, EventArgs e)
        {
            if (_clienteCargado == null || this.cmbTipo.SelectedItem == null)
            {
                return;
            }

            string tipo = this.cmbTipo.SelectedItem.ToString();
            decimal importe;

            if (!decimal.TryParse(this.txtImporte.Text, NumberStyles.Currency, CulturaMonetaria, out importe) || !AjusteCuentaServicio.EsImporteValido(importe))
            {
                MessageBox.Show("Ingrese un importe mayor a cero con hasta dos decimales.", "Ajuste de cuenta", MessageBoxButtons.OK, MessageBoxIcon.Warning);
                this.txtImporte.Focus();
                return;
            }

            string motivo = this.txtMotivo.Text.Trim();

            if (motivo.Length == 0)
            {
                MessageBox.Show("Ingrese el motivo del ajuste.", "Ajuste de cuenta", MessageBoxButtons.OK, MessageBoxIcon.Warning);
                this.txtMotivo.Focus();
                return;
            }

            string pregunta = "Se registrará un ajuste de tipo " + tipo + " por " + importe.ToString("C2", CulturaMonetaria) + Environment.NewLine
                + "Cliente: " + this.lblCliente.Text + Environment.NewLine
                + "Motivo: " + motivo + Environment.NewLine
                + "Empleado responsable: Nº " + SesionActual.Instancia.EmpleadoId + Environment.NewLine + Environment.NewLine
                + "El ajuste no podrá editarse ni eliminarse. ¿Confirma la operación?";

            if (MessageBox.Show(pregunta, "Ajuste de cuenta", MessageBoxButtons.YesNo, MessageBoxIcon.Question) != DialogResult.Yes)
            {
                return;
            }

            this.btnRegistrar.Enabled = false;
            this.Cursor = Cursors.WaitCursor;

            try
            {
                bool registrado = _ajusteCuentaServicio.RegistrarAjuste(_clienteCargado.Id, SesionActual.Instancia.EmpleadoId, tipo, importe, motivo);

                if (!registrado)
                {
                    MessageBox.Show(
                        "No se pudo registrar el ajuste. Verifique el importe, el motivo, que el empleado exista y que los tipos de movimiento estén cargados.",
                        "Ajuste de cuenta", MessageBoxButtons.OK, MessageBoxIcon.Warning);
                    return;
                }

                MessageBox.Show("El ajuste se registró correctamente.", "Ajuste de cuenta", MessageBoxButtons.OK, MessageBoxIcon.Information);

                this.txtImporte.Text = string.Empty;
                this.txtMotivo.Text = string.Empty;
                MostrarSaldo();
            }
            catch (Exception ex)
            {
                Debug.WriteLine(ex.ToString());
                MessageBox.Show("Ocurrió un error al registrar el ajuste. Verifique la cuenta corriente antes de reintentar.", "Ajuste de cuenta", MessageBoxButtons.OK, MessageBoxIcon.Error);
            }
            finally
            {
                this.Cursor = Cursors.Default;
                this.btnRegistrar.Enabled = _clienteCargado != null;
            }
        }

        private void LimpiarCliente()
        {
            _clienteCargado = null;
            this.lblCliente.Text = string.Empty;
            this.lblSaldo.Text = string.Empty;
            this.btnRegistrar.Enabled = false;
        }
    }
}
