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

        private readonly ClienteServicio _clienteServicio;
        private readonly CobroServicio _cobroServicio;
        private readonly CuentaCorrienteServicio _cuentaCorrienteServicio;

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
        private Button btnAplicarSaldo;
        private ErrorProvider errorProvider;

        public FrmCobro(ClienteServicio clienteServicio, CobroServicio cobroServicio, CuentaCorrienteServicio cuentaCorrienteServicio)
        {
            _clienteServicio = clienteServicio;
            _cobroServicio = cobroServicio;
            _cuentaCorrienteServicio = cuentaCorrienteServicio;

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
            this.btnAplicarSaldo = new Button();
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
            this.lblEmpleado.Text = "Empleado que cobra: Nº " + SesionActual.EmpleadoId;

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
            foreach (string medio in CobroServicio.MediosPagoValidos)
            {
                this.cmbMetodoPago.Items.Add(medio);
            }
            if (this.cmbMetodoPago.Items.Count > 0)
            {
                this.cmbMetodoPago.SelectedIndex = 0;
            }

            this.btnRegistrar.Location = new Point(20, 260);
            this.btnRegistrar.Size = new Size(170, 32);
            this.btnRegistrar.Text = "Registrar cobro";
            this.btnRegistrar.Enabled = false;
            this.btnRegistrar.Click += new EventHandler(this.btnRegistrar_Click);

            this.btnAplicarSaldo.Location = new Point(200, 260);
            this.btnAplicarSaldo.Size = new Size(200, 32);
            this.btnAplicarSaldo.Text = "Aplicar saldo a favor";
            this.btnAplicarSaldo.Enabled = false;
            this.btnAplicarSaldo.Click += new EventHandler(this.btnAplicarSaldo_Click);

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
            this.Controls.Add(this.btnAplicarSaldo);
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
            this.btnAplicarSaldo.Enabled = false;
            this.lblSaldo.Text = string.Empty;
            this.lblPrecioCuota.Text = string.Empty;
            this.lblMinimo.Text = string.Empty;

            try
            {
                Cliente cliente = _clienteServicio.GetClienteByDNI(dni);

                if (cliente == null)
                {
                    MessageBox.Show("No existe un cliente con ese DNI.", "Registrar cobro", MessageBoxButtons.OK, MessageBoxIcon.Information);
                    return;
                }

                _clienteCargado = cliente;
                this.lblCliente.Text = cliente.Apellido + " " + cliente.Nombre + " (DNI " + cliente.Dni + ")";
                this.btnRegistrar.Enabled = true;
                MostrarResumenCobro();
            }
            catch (Exception ex)
            {
                Debug.WriteLine(ex.ToString());
                MessageBox.Show("No se pudo buscar el cliente. Intente nuevamente.", "Registrar cobro", MessageBoxButtons.OK, MessageBoxIcon.Error);
            }
        }

        private void MostrarResumenCobro()
        {
            this.errorProvider.Clear();
            ResumenCobro resumen = _cobroServicio.ObtenerResumenCobro(_clienteCargado.Id);

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
            this.lblMinimo.Text = "Mínimo exigible: " + resumen.MinimoExigible.ToString("C2", CulturaMonetaria);
            this.btnRegistrar.Enabled = resumen.MinimoExigible > 0m;
            this.txtImporte.Enabled = resumen.MinimoExigible > 0m;
            this.txtImporte.Text = resumen.MinimoExigible > 0m
                ? resumen.MinimoExigible.ToString("C2", CulturaMonetaria)
                : string.Empty;
            this.txtImporte.SelectAll();
            this.btnAplicarSaldo.Enabled = resumen.TieneNotaVentaDelPeriodo
                ? resumen.Saldo <= 0
                : resumen.Saldo <= -resumen.PrecioCuota;
        }

        private void btnAplicarSaldo_Click(object sender, EventArgs e)
        {
            if (_clienteCargado == null)
            {
                return;
            }

            this.btnAplicarSaldo.Enabled = false;
            this.Cursor = Cursors.WaitCursor;

            try
            {
                ResumenCobro resumen = _resumenCobroActual;

                if (resumen == null)
                {
                    MessageBox.Show("No hay información financiera disponible para aplicar el saldo a favor.", "Registrar cobro", MessageBoxButtons.OK, MessageBoxIcon.Warning);
                    return;
                }

                string detalle = DescribirAplicacionSaldo(resumen);
                if (MessageBox.Show(
                    detalle + Environment.NewLine + Environment.NewLine + "¿Confirma la operación?",
                    "Aplicar saldo a favor", MessageBoxButtons.YesNo, MessageBoxIcon.Question) != DialogResult.Yes)
                {
                    return;
                }

                if (!_cobroServicio.AplicarSaldoAFavor(_clienteCargado.Id))
                {
                    MessageBox.Show(DescribirRechazoAplicacionSaldo(resumen), "Aplicar saldo a favor", MessageBoxButtons.OK, MessageBoxIcon.Warning);
                    MostrarResumenCobro();
                    return;
                }

                MessageBox.Show("Saldo a favor aplicado. La cuota quedó activa durante 30 días.", "Aplicar saldo a favor", MessageBoxButtons.OK, MessageBoxIcon.Information);
                MostrarResumenCobro();
                this.btnRegistrar.Enabled = false;
            }
            catch (Exception ex)
            {
                Debug.WriteLine(ex.ToString());
                MessageBox.Show("Ocurrió un error al aplicar el saldo a favor.", "Registrar cobro", MessageBoxButtons.OK, MessageBoxIcon.Error);
                MostrarResumenCobro();
            }
            finally
            {
                this.Cursor = Cursors.Default;
                if (this._resumenCobroActual != null)
                {
                    this.btnAplicarSaldo.Enabled = this._resumenCobroActual.TieneNotaVentaDelPeriodo
                        ? this._resumenCobroActual.Saldo <= 0
                        : this._resumenCobroActual.Saldo <= -this._resumenCobroActual.PrecioCuota;
                }
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

            if (this._resumenCobroActual != null && importe < this._resumenCobroActual.MinimoExigible)
            {
                this.errorProvider.SetError(this.txtImporte, "El importe no puede ser inferior al mínimo exigible de " + this._resumenCobroActual.MinimoExigible.ToString("C2", CulturaMonetaria) + ".");
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
                bool registrado = _cobroServicio.RegistrarCobro(_clienteCargado.Id, SesionActual.EmpleadoId, importe, metodoPago);

                if (!registrado)
                {
                    ResumenCobro resumen = _cobroServicio.ObtenerResumenCobro(_clienteCargado.Id);
                    string mensaje = resumen != null && resumen.MinimoExigible <= 0m
                        ? "El saldo a favor ya cubre la cuota. Utilice 'Aplicar saldo a favor'."
                        : "No se pudo registrar el cobro. Verifique que el cliente tenga cuenta corriente y cuota asignada, que el empleado exista y que el tipo de movimiento CREDITO esté cargado.";
                    MessageBox.Show(
                        mensaje,
                        "Registrar cobro", MessageBoxButtons.OK, MessageBoxIcon.Warning);
                    return;
                }

                MostrarRecibo();

                this.txtImporte.Text = string.Empty;
                MostrarResumenCobro();
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
                this.txtImporte.Enabled = this.btnRegistrar.Enabled;
            }
        }

        private void MostrarRecibo()
        {
            ReciboPago recibo = null;

            try
            {
                recibo = BuscarUltimoRecibo(_clienteCargado.Id);
            }
            catch (Exception ex)
            {
                Debug.WriteLine(ex.ToString());
            }

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

        private ReciboPago BuscarUltimoRecibo(int clienteId)
        {
            ResumenCuentaCorriente resumen = _cuentaCorrienteServicio.ConsultarCuentaCorriente(clienteId);

            if (resumen == null)
            {
                return null;
            }

            ReciboPago ultimo = null;

            foreach (Movimiento movimiento in resumen.Movimientos)
            {
                ReciboPago recibo = movimiento.Comprobante as ReciboPago;

                if (recibo != null && (ultimo == null || recibo.Id > ultimo.Id))
                {
                    ultimo = recibo;
                }
            }

            return ultimo;
        }

        private void txtImporte_Validating(object sender, CancelEventArgs e)
        {
            e.Cancel = !ActualizarEstadoImporte();
        }

        private string DescribirAplicacionSaldo(ResumenCobro resumen)
        {
            decimal saldoAFavor = Math.Max(0m, -resumen.Saldo);
            string detalle = "Saldo favorable disponible: " + saldoAFavor.ToString("C2", CulturaMonetaria) + Environment.NewLine
                + "Importe de la cuota: " + resumen.PrecioCuota.ToString("C2", CulturaMonetaria) + Environment.NewLine;

            if (resumen.TieneNotaVentaDelPeriodo)
            {
                return detalle
                    + "La Nota de Venta del período ya existe. No se generará otra ni se registrará un movimiento adicional."
                    + Environment.NewLine
                    + "La cuota se activará durante 30 días.";
            }

            decimal restante = saldoAFavor - resumen.PrecioCuota;
            return detalle
                + "Crédito que se utilizará: " + resumen.PrecioCuota.ToString("C2", CulturaMonetaria) + Environment.NewLine
                + "Crédito restante: " + Math.Max(0m, restante).ToString("C2", CulturaMonetaria) + Environment.NewLine
                + "Se generará la Nota de Venta del período y se activará la cuota durante 30 días.";
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

            if (importe < this._resumenCobroActual.MinimoExigible)
            {
                this.errorProvider.SetError(this.txtImporte, "El importe no puede ser inferior al mínimo exigible de " + this._resumenCobroActual.MinimoExigible.ToString("C2", CulturaMonetaria) + ".");
                this.btnRegistrar.Enabled = false;
                return false;
            }

            this.errorProvider.SetError(this.txtImporte, string.Empty);
            this.btnRegistrar.Enabled = this._resumenCobroActual.MinimoExigible > 0m;
            return true;
        }

        private string DescribirRechazoAplicacionSaldo(ResumenCobro resumen)
        {
            if (resumen.MinimoExigible > 0m && resumen.Saldo > -resumen.PrecioCuota)
            {
                return "El saldo favorable disponible no alcanza para cubrir la cuota.";
            }

            return "No se puede aplicar el saldo a favor. La cuota puede estar vigente o la información financiera cambió.";
        }
    }
}
