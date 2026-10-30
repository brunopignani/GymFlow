using GymFlow.Aplicacion;
using GymFlow.Dominio;
using System;
using System.Diagnostics;
using System.Drawing;
using System.Windows.Forms;

namespace GymFlow.InterfazUsuario
{
    public class FrmCuentaCorriente : Form
    {
        private readonly ClienteServicio _clienteServicio;
        private readonly CuentaCorrienteServicio _cuentaCorrienteServicio;

        private Label lblDni;
        private TextBox txtDni;
        private Button btnBuscar;
        private Label lblCliente;
        private Label lblSaldo;
        private DataGridView dgvMovimientos;
        private Label lblMensaje;

        public FrmCuentaCorriente(ClienteServicio clienteServicio, CuentaCorrienteServicio cuentaCorrienteServicio)
        {
            _clienteServicio = clienteServicio;
            _cuentaCorrienteServicio = cuentaCorrienteServicio;

            InitializeComponent();
        }

        private void InitializeComponent()
        {
            this.lblDni = new Label();
            this.txtDni = new TextBox();
            this.btnBuscar = new Button();
            this.lblCliente = new Label();
            this.lblSaldo = new Label();
            this.dgvMovimientos = new DataGridView();
            this.lblMensaje = new Label();

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
            this.lblCliente.Size = new Size(600, 20);
            this.lblCliente.Text = string.Empty;

            this.lblSaldo.Location = new Point(20, 85);
            this.lblSaldo.Size = new Size(600, 20);
            this.lblSaldo.Text = string.Empty;
            this.lblSaldo.Font = new Font(this.lblSaldo.Font, FontStyle.Bold);

            this.dgvMovimientos.Location = new Point(20, 120);
            this.dgvMovimientos.Size = new Size(600, 260);
            this.dgvMovimientos.ReadOnly = true;
            this.dgvMovimientos.AllowUserToAddRows = false;
            this.dgvMovimientos.RowHeadersVisible = false;
            this.dgvMovimientos.SelectionMode = DataGridViewSelectionMode.FullRowSelect;
            this.dgvMovimientos.AutoSizeColumnsMode = DataGridViewAutoSizeColumnsMode.Fill;
            this.dgvMovimientos.Columns.Add("colFecha", "Fecha");
            this.dgvMovimientos.Columns.Add("colTipo", "Tipo");
            this.dgvMovimientos.Columns.Add("colComprobante", "Comprobante");
            this.dgvMovimientos.Columns.Add("colImporte", "Importe");
            this.dgvMovimientos.Columns["colImporte"].DefaultCellStyle.Alignment = DataGridViewContentAlignment.MiddleRight;

            this.lblMensaje.Location = new Point(20, 390);
            this.lblMensaje.Size = new Size(600, 20);
            this.lblMensaje.Text = string.Empty;

            this.Text = "Cuenta corriente";
            this.Size = new Size(660, 470);
            this.FormBorderStyle = FormBorderStyle.FixedDialog;
            this.MaximizeBox = false;
            this.StartPosition = FormStartPosition.CenterParent;
            this.AcceptButton = this.btnBuscar;

            this.Controls.Add(this.lblDni);
            this.Controls.Add(this.txtDni);
            this.Controls.Add(this.btnBuscar);
            this.Controls.Add(this.lblCliente);
            this.Controls.Add(this.lblSaldo);
            this.Controls.Add(this.dgvMovimientos);
            this.Controls.Add(this.lblMensaje);
        }

        private void btnBuscar_Click(object sender, EventArgs e)
        {
            LimpiarResultados();

            int dni;

            if (!int.TryParse(this.txtDni.Text, out dni))
            {
                MessageBox.Show("Ingrese un DNI válido.", "Cuenta corriente", MessageBoxButtons.OK, MessageBoxIcon.Warning);
                return;
            }

            this.btnBuscar.Enabled = false;
            this.Cursor = Cursors.WaitCursor;

            try
            {
                Cliente cliente = _clienteServicio.GetClienteByDNI(dni);

                if (cliente == null)
                {
                    this.lblMensaje.Text = "No existe un cliente con ese DNI.";
                    return;
                }

                this.lblCliente.Text = cliente.Apellido + " " + cliente.Nombre + " (DNI " + cliente.Dni + ")";

                ResumenCuentaCorriente resumen = _cuentaCorrienteServicio.ConsultarCuentaCorriente(cliente.Id);

                if (resumen == null)
                {
                    this.lblMensaje.Text = "El cliente no tiene cuenta corriente.";
                    return;
                }

                this.lblSaldo.Text = "Saldo (débitos - créditos): " + resumen.Saldo.ToString("N2");

                foreach (Movimiento movimiento in resumen.Movimientos)
                {
                    this.dgvMovimientos.Rows.Add(
                        movimiento.Fecha.ToString("dd/MM/yyyy"),
                        movimiento.TipoMovimiento.Nombre,
                        DescribirComprobante(movimiento.Comprobante),
                        movimiento.Importe.ToString("N2"));
                }

                if (resumen.Movimientos.Count == 0)
                {
                    this.lblMensaje.Text = "La cuenta corriente no tiene movimientos.";
                }
            }
            catch (Exception ex)
            {
                Debug.WriteLine(ex.ToString());
                LimpiarResultados();
                MessageBox.Show("No se pudo consultar la cuenta corriente. Intente nuevamente.", "Cuenta corriente", MessageBoxButtons.OK, MessageBoxIcon.Error);
            }
            finally
            {
                this.Cursor = Cursors.Default;
                this.btnBuscar.Enabled = true;
            }
        }

        private void LimpiarResultados()
        {
            this.lblCliente.Text = string.Empty;
            this.lblSaldo.Text = string.Empty;
            this.lblMensaje.Text = string.Empty;
            this.dgvMovimientos.Rows.Clear();
        }

        private string DescribirComprobante(Comprobante comprobante)
        {
            string tipo = "Comprobante";

            if (comprobante is ReciboPago)
            {
                tipo = "Recibo de pago";
            }
            else if (comprobante is NotaVenta)
            {
                tipo = "Nota de venta";
            }
            else if (comprobante is NotaCredito)
            {
                tipo = "Nota de crédito";
            }

            return tipo + " " + comprobante.Numero;
        }
    }
}
