using GymFlow.Aplicacion;
using GymFlow.Dominio;
using System;
using System.Diagnostics;
using System.Drawing;
using System.Windows.Forms;
using System.ComponentModel;
using System.Globalization;

namespace GymFlow.InterfazUsuario
{
    public class FrmCobro : Form
    {
        private static readonly CultureInfo CulturaMonetaria = CultureInfo.GetCultureInfo("es-AR");

        private readonly CobroFacade _cobroFacade;

        private Cliente _clienteCargado;
        private ResumenCobro _resumenCobroActual;

        private Label lblDni;
        private TextBox txtDni;
        private Button btnBuscar;
        private Label lblCliente;
        private Label lblEmpleado;
        private Label lblImporteTitulo;
        private TextBox txtImporte;
        private Label lblSaldo;
        private Label lblPrecioCuota;
        private Label lblMinimo;
        private Label lblMetodoPagoTitulo;
        private ComboBox cmbMetodoPago;
        private Button btnRegistrar;
        private ErrorProvider errorProvider;

        public FrmCobro(CobroFacade cobroFacade)
        {
            _cobroFacade = cobroFacade;

            InitializeComponent();
        }

        private void InitializeComponent()
        {
            this.lblDni = new Label();
            this.txtDni = new TextBox();
            this.btnBuscar = new Button();
            this.lblCliente = new Label();
            this.lblEmpleado = new Label();
            this.lblImporteTitulo = new Label();
            this.txtImporte = new TextBox();
            this.lblSaldo = new Label();
            this.lblPrecioCuota = new Label();
            this.lblMinimo = new Label();
            this.lblMetodoPagoTitulo = new Label();
            this.cmbMetodoPago = new ComboBox();
            this.btnRegistrar = new Button();
            this.errorProvider = new ErrorProvider(this);

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
            this.lblCliente.Size = new Size(380, 20);
            this.lblCliente.Text = string.Empty;

            this.lblEmpleado.Location = new Point(20, 85);
            this.lblEmpleado.Size = new Size(380, 20);
            this.lblEmpleado.Text = "Empleado que cobra: Nº " + SesionActual.Instancia.EmpleadoId;

            this.lblSaldo.Location = new Point(20, 115);
            this.lblSaldo.Size = new Size(380, 20);
            this.lblPrecioCuota.Location = new Point(20, 137);
            this.lblPrecioCuota.Size = new Size(380, 20);
            this.lblMinimo.Location = new Point(20, 159);
            this.lblMinimo.Size = new Size(380, 20);

            this.lblImporteTitulo.Location = new Point(20, 190);
            this.lblImporteTitulo.Size = new Size(120, 20);
            this.lblImporteTitulo.Text = "Importe:";

            this.txtImporte.Location = new Point(150, 187);
            this.txtImporte.Size = new Size(150, 20);
            this.txtImporte.Validating += new CancelEventHandler(this.txtImporte_Validating);
            this.txtImporte.TextChanged += new EventHandler(this.txtImporte_TextChanged);

            this.lblMetodoPagoTitulo.Location = new Point(20, 225);
            this.lblMetodoPagoTitulo.Size = new Size(120, 20);
            this.lblMetodoPagoTitulo.Text = "Método de pago:";

            this.cmbMetodoPago.Location = new Point(150, 222);
            this.cmbMetodoPago.Size = new Size(200, 21);
            this.cmbMetodoPago.DropDownStyle = ComboBoxStyle.DropDownList;
            foreach (string medio in _cobroFacade.ObtenerMediosPagoValidos())
            {
                this.cmbMetodoPago.Items.Add(medio);
            }
            if (this.cmbMetodoPago.Items.Count > 0)
            {
                this.cmbMetodoPago.SelectedIndex = 0;
            }

            this.btnRegistrar.Location = new Point(20, 260);
            this.btnRegistrar.Size = new Size(380, 32);
            this.btnRegistrar.Text = "Registrar cobro";
            this.btnRegistrar.Enabled = false;
            this.btnRegistrar.Click += new EventHandler(this.btnRegistrar_Click);

            this.Text = "Registrar cobro";
            this.Size = new Size(440, 345);
            this.FormBorderStyle = FormBorderStyle.FixedDialog;
            this.MaximizeBox = false;
            this.StartPosition = FormStartPosition.CenterParent;
            this.AcceptButton = this.btnBuscar;

            this.Controls.Add(this.lblDni);
            this.Controls.Add(this.txtDni);
            this.Controls.Add(this.btnBuscar);
            this.Controls.Add(this.lblCliente);
            this.Controls.Add(this.lblEmpleado);
            this.Controls.Add(this.lblSaldo);
            this.Controls.Add(this.lblPrecioCuota);
            this.Controls.Add(this.lblMinimo);
            this.Controls.Add(this.lblImporteTitulo);
            this.Controls.Add(this.txtImporte);
            this.Controls.Add(this.lblMetodoPagoTitulo);
            this.Controls.Add(this.cmbMetodoPago);
            this.Controls.Add(this.btnRegistrar);
        }

        public void CargarClientePorDni(int dni)
        {
            this.txtDni.Text = dni.ToString();
            BuscarPorDni(dni);
        }

        private void btnBuscar_Click(object sender, EventArgs e)
        {
            int dni;

            if (!ValidacionesDatos.TryParseDni(this.txtDni.Text, out dni))
            {
                MessageBox.Show("Ingrese un DNI válido.", "Registrar cobro", MessageBoxButtons.OK, MessageBoxIcon.Warning);
                return;
            }

            BuscarPorDni(dni);
        }

        private void BuscarPorDni(int dni)
        {
            _clienteCargado = null;
            this.lblCliente.Text = string.Empty;
            this.btnRegistrar.Enabled = false;
            this.lblSaldo.Text = string.Empty;
            this.lblPrecioCuota.Text = string.Empty;
            this.lblMinimo.Text = string.Empty;

            try
            {
                ResultadoConsultaCobro consulta = _cobroFacade.BuscarClienteParaCobro(dni);

                if (consulta == null || !consulta.ClienteEncontrado || consulta.Cliente == null)
                {
                    MessageBox.Show("No existe un cliente con ese DNI.", "Registrar cobro", MessageBoxButtons.OK, MessageBoxIcon.Information);
                    return;
                }

                _clienteCargado = consulta.Cliente;
                this.lblCliente.Text = consulta.Cliente.Apellido + " " + consulta.Cliente.Nombre + " (DNI " + consulta.Cliente.Dni + ")";
                MostrarResumenCobro(consulta.Resumen);
            }
            catch (Exception ex)
            {
                Debug.WriteLine(ex.ToString());
                MessageBox.Show("No se pudo buscar el cliente. Intente nuevamente.", "Registrar cobro", MessageBoxButtons.OK, MessageBoxIcon.Error);
            }
        }

        private void MostrarResumenCobro(ResumenCobro resumen)
        {
            this.errorProvider.Clear();

            if (resumen == null)
            {
                this._resumenCobroActual = null;
                this.lblSaldo.Text = "Saldo: no disponible";
                this.lblPrecioCuota.Text = "Precio de cuota: no disponible";
                this.lblMinimo.Text = "Mínimo exigible: no disponible";
                this.btnRegistrar.Enabled = false;
                this.txtImporte.Enabled = false;
                this.txtImporte.Text = string.Empty;
                return;
            }

            this._resumenCobroActual = resumen;
            this.lblSaldo.Text = "Saldo: " + resumen.Saldo.ToString("C2", CulturaMonetaria);
            this.lblPrecioCuota.Text = "Precio de cuota: " + resumen.PrecioCuota.ToString("C2", CulturaMonetaria);

            if (resumen.TieneInconsistenciaContable)
            {
                this.lblMinimo.Text = "Mínimo exigible: Inconsistencia contable";
                this.btnRegistrar.Enabled = false;
                this.txtImporte.Enabled = false;
                this.txtImporte.Text = string.Empty;
                this.errorProvider.SetError(this.lblSaldo, DescribirBloqueo(resumen));
                return;
            }

            this.lblMinimo.Text = "Mínimo exigible: " + resumen.MinimoExigible.ToString("C2", CulturaMonetaria);

            if (!resumen.PermiteRegistrarCobro)
            {
                this.btnRegistrar.Enabled = false;
                this.txtImporte.Enabled = false;
                this.txtImporte.Text = string.Empty;
                this.errorProvider.SetError(this.lblMinimo, DescribirBloqueo(resumen));
                return;
            }

            this.txtImporte.Enabled = true;
            this.btnRegistrar.Enabled = true;
            this.txtImporte.Text = resumen.MinimoExigible.ToString("C2", CulturaMonetaria);
            this.txtImporte.SelectAll();
        }

        private static string DescribirBloqueo(ResumenCobro resumen)
        {
            if (resumen == null)
            {
                return "No se pudo determinar el estado de cobro del cliente. Verifique que tenga cuenta corriente y cuota asignada.";
            }

            switch (resumen.MotivoBloqueo)
            {
                case MotivoBloqueoCobro.InconsistenciaContable:
                    return "La cuenta presenta inconsistencias en los movimientos del período. Operaciones bloqueadas.";
                case MotivoBloqueoCobro.CuotaVigente:
                    return "La cuota está vigente. No se admiten cobros anticipados.";
                case MotivoBloqueoCobro.SaldoAFavorCubreCuota:
                    return "El saldo a favor ya cubre el mínimo exigible. No hay importe a cobrar.";
                case MotivoBloqueoCobro.EstadoCuotaNoCobrable:
                    return "El estado de la cuota (" + resumen.EstadoCuota + ") no admite cobros.";
                default:
                    return string.Empty;
            }
        }

        private void btnRegistrar_Click(object sender, EventArgs e)
        {
            if (_clienteCargado == null)
            {
                return;
            }

            decimal importe;
            this.errorProvider.Clear();

            if (!TryParseImporte(this.txtImporte.Text, out importe) || importe <= 0)
            {
                this.errorProvider.SetError(this.txtImporte, "Ingrese un importe numérico mayor a cero.");
                this.txtImporte.Focus();
                return;
            }

            if (this._resumenCobroActual == null || !this._resumenCobroActual.PermiteRegistrarCobro)
            {
                this.errorProvider.SetError(this.txtImporte, DescribirBloqueo(this._resumenCobroActual));
                return;
            }

            if (importe != this._resumenCobroActual.MinimoExigible)
            {
                this.errorProvider.SetError(this.txtImporte, "El importe debe ser exactamente igual al mínimo exigible de " + this._resumenCobroActual.MinimoExigible.ToString("C2", CulturaMonetaria) + ".");
                this.txtImporte.Focus();
                return;
            }

            string metodoPago = this.cmbMetodoPago.SelectedItem != null ? this.cmbMetodoPago.SelectedItem.ToString() : string.Empty;

            if (metodoPago.Length == 0)
            {
                this.errorProvider.SetError(this.cmbMetodoPago, "Seleccione un método de pago permitido.");
                this.cmbMetodoPago.Focus();
                return;
            }

            string pregunta = "Importe a cobrar: " + importe.ToString("C2", CulturaMonetaria) + Environment.NewLine
                + "Mínimo exigible: " + this._resumenCobroActual.MinimoExigible.ToString("C2", CulturaMonetaria) + Environment.NewLine
                + "Cliente: " + this.lblCliente.Text + Environment.NewLine
                + "Medio de pago: " + metodoPago + Environment.NewLine + Environment.NewLine
                + "¿Registrar el cobro?";

            if (MessageBox.Show(pregunta, "Registrar cobro", MessageBoxButtons.YesNo, MessageBoxIcon.Question) != DialogResult.Yes)
            {
                return;
            }

            this.btnRegistrar.Enabled = false;
            this.Cursor = Cursors.WaitCursor;

            try
            {
                ResultadoRegistroCobro resultado = _cobroFacade.RegistrarCobro(_clienteCargado.Id, SesionActual.Instancia.EmpleadoId, importe, metodoPago);

                if (!resultado.Registrado)
                {
                    ResumenCobro resumen = resultado.ResumenActualizado;
                    string mensaje = resumen != null && !resumen.PermiteRegistrarCobro
                        ? DescribirBloqueo(resumen)
                        : "No se pudo registrar el cobro. Verifique que el importe sea exactamente el mínimo exigible, que el cliente tenga cuenta corriente y cuota asignada, que el empleado exista y que los tipos de movimiento estén cargados.";
                    MessageBox.Show(
                        mensaje,
                        "Registrar cobro", MessageBoxButtons.OK, MessageBoxIcon.Warning);
                    return;
                }

                MostrarRecibo(resultado.Recibo);

                this.txtImporte.Text = string.Empty;
                MostrarResumenCobro(resultado.ResumenActualizado);
            }
            catch (Exception ex)
            {
                Debug.WriteLine(ex.ToString());
                MessageBox.Show("Ocurrió un error al registrar el cobro. Verifique la cuenta corriente antes de reintentar.", "Registrar cobro", MessageBoxButtons.OK, MessageBoxIcon.Error);
            }
            finally
            {
                this.Cursor = Cursors.Default;
                ActualizarEstadoImporte();
            }
        }

        private void MostrarRecibo(ReciboPago recibo)
        {
            if (recibo == null)
            {
                MessageBox.Show("El cobro se registró correctamente, pero no se pudo mostrar el recibo.", "Registrar cobro", MessageBoxButtons.OK, MessageBoxIcon.Information);
                return;
            }

            string detalle = "Cobro registrado correctamente." + Environment.NewLine
                + "Recibo: " + recibo.Numero + Environment.NewLine
                + "Fecha: " + recibo.FechaEmision.ToString("dd/MM/yyyy") + Environment.NewLine
                + "Importe: " + recibo.Total.ToString("C2", CulturaMonetaria) + Environment.NewLine
                + "Método de pago: " + recibo.MetodoPago + Environment.NewLine
                + "Empleado: Nº " + recibo.EmpleadoId;

            MessageBox.Show(detalle, "Recibo de pago", MessageBoxButtons.OK, MessageBoxIcon.Information);
        }

        private void txtImporte_Validating(object sender, CancelEventArgs e)
        {
            e.Cancel = !ActualizarEstadoImporte();
        }

        private bool TryParseImporte(string texto, out decimal importe)
        {
            return decimal.TryParse(texto, NumberStyles.Currency, CulturaMonetaria, out importe);
        }

        private void txtImporte_TextChanged(object sender, EventArgs e)
        {
            ActualizarEstadoImporte();
        }

        private bool ActualizarEstadoImporte()
        {
            if (this._resumenCobroActual == null || !this.txtImporte.Enabled)
            {
                this.errorProvider.SetError(this.txtImporte, string.Empty);
                return false;
            }

            decimal importe;
            if (!TryParseImporte(this.txtImporte.Text, out importe) || importe <= 0)
            {
                this.errorProvider.SetError(this.txtImporte, "Ingrese un importe numérico mayor a cero.");
                this.btnRegistrar.Enabled = false;
                return false;
            }

            if (importe != this._resumenCobroActual.MinimoExigible)
            {
                this.errorProvider.SetError(this.txtImporte, "El importe debe ser exactamente igual al mínimo exigible de " + this._resumenCobroActual.MinimoExigible.ToString("C2", CulturaMonetaria) + ".");
                this.btnRegistrar.Enabled = false;
                return false;
            }

            this.errorProvider.SetError(this.txtImporte, string.Empty);
            this.btnRegistrar.Enabled = this._resumenCobroActual.PermiteRegistrarCobro;
            return true;
        }

    }
}
