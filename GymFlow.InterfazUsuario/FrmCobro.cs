using GymFlow.Aplicacion;
using GymFlow.Dominio;
using System;
using System.Diagnostics;
using System.Drawing;
using System.Windows.Forms;

namespace GymFlow.InterfazUsuario
{
    public class FrmCobro : Form
    {
        private readonly ClienteServicio _clienteServicio;
        private readonly CobroServicio _cobroServicio;
        private readonly CuentaCorrienteServicio _cuentaCorrienteServicio;

        private Cliente _clienteCargado;

        private Label lblDni;
        private TextBox txtDni;
        private Button btnBuscar;
        private Label lblCliente;
        private Label lblEmpleado;
        private Label lblImporteTitulo;
        private TextBox txtImporte;
        private Label lblMetodoPagoTitulo;
        private ComboBox cmbMetodoPago;
        private Button btnRegistrar;

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
            this.lblMetodoPagoTitulo = new Label();
            this.cmbMetodoPago = new ComboBox();
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
            this.lblCliente.Size = new Size(380, 20);
            this.lblCliente.Text = string.Empty;

            this.lblEmpleado.Location = new Point(20, 85);
            this.lblEmpleado.Size = new Size(380, 20);
            this.lblEmpleado.Text = "Empleado que cobra: Nº " + SesionActual.EmpleadoId;

            this.lblImporteTitulo.Location = new Point(20, 125);
            this.lblImporteTitulo.Size = new Size(120, 20);
            this.lblImporteTitulo.Text = "Importe:";

            this.txtImporte.Location = new Point(150, 122);
            this.txtImporte.Size = new Size(150, 20);

            this.lblMetodoPagoTitulo.Location = new Point(20, 160);
            this.lblMetodoPagoTitulo.Size = new Size(120, 20);
            this.lblMetodoPagoTitulo.Text = "Método de pago:";

            this.cmbMetodoPago.Location = new Point(150, 157);
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

            this.btnRegistrar.Location = new Point(20, 205);
            this.btnRegistrar.Size = new Size(170, 32);
            this.btnRegistrar.Text = "Registrar cobro";
            this.btnRegistrar.Enabled = false;
            this.btnRegistrar.Click += new EventHandler(this.btnRegistrar_Click);

            this.Text = "Registrar cobro";
            this.Size = new Size(440, 320);
            this.FormBorderStyle = FormBorderStyle.FixedDialog;
            this.MaximizeBox = false;
            this.StartPosition = FormStartPosition.CenterParent;
            this.AcceptButton = this.btnBuscar;

            this.Controls.Add(this.lblDni);
            this.Controls.Add(this.txtDni);
            this.Controls.Add(this.btnBuscar);
            this.Controls.Add(this.lblCliente);
            this.Controls.Add(this.lblEmpleado);
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

            if (!int.TryParse(this.txtDni.Text, out dni))
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
            }
            catch (Exception ex)
            {
                Debug.WriteLine(ex.ToString());
                MessageBox.Show("No se pudo buscar el cliente. Intente nuevamente.", "Registrar cobro", MessageBoxButtons.OK, MessageBoxIcon.Error);
            }
        }

        private void btnRegistrar_Click(object sender, EventArgs e)
        {
            if (_clienteCargado == null)
            {
                return;
            }

            decimal importe;

            if (!decimal.TryParse(this.txtImporte.Text, out importe) || importe <= 0)
            {
                MessageBox.Show("Ingrese un importe mayor a cero.", "Registrar cobro", MessageBoxButtons.OK, MessageBoxIcon.Warning);
                this.txtImporte.Focus();
                return;
            }

            string metodoPago = this.cmbMetodoPago.SelectedItem != null ? this.cmbMetodoPago.SelectedItem.ToString() : string.Empty;

            if (metodoPago.Length == 0)
            {
                MessageBox.Show("Seleccione un método de pago válido.", "Registrar cobro", MessageBoxButtons.OK, MessageBoxIcon.Warning);
                this.cmbMetodoPago.Focus();
                return;
            }

            string pregunta = "¿Registrar el cobro de " + importe.ToString("N2") + " a " + this.lblCliente.Text + " (" + metodoPago + ")?";

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
                    MessageBox.Show(
                        "No se pudo registrar el cobro. Verifique que el cliente tenga cuenta corriente y cuota asignada, que el empleado exista y que el tipo de movimiento CREDITO esté cargado.",
                        "Registrar cobro", MessageBoxButtons.OK, MessageBoxIcon.Warning);
                    return;
                }

                MostrarRecibo();

                this.txtImporte.Text = string.Empty;
            }
            catch (Exception ex)
            {
                Debug.WriteLine(ex.ToString());
                MessageBox.Show("Ocurrió un error al registrar el cobro. Verifique la cuenta corriente antes de reintentar.", "Registrar cobro", MessageBoxButtons.OK, MessageBoxIcon.Error);
            }
            finally
            {
                this.Cursor = Cursors.Default;
                this.btnRegistrar.Enabled = true;
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
                + "Importe: " + recibo.Total.ToString("N2") + Environment.NewLine
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
    }
}
