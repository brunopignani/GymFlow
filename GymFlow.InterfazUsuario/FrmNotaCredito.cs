using GymFlow.Aplicacion;
using GymFlow.Dominio;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Drawing;
using System.Windows.Forms;

namespace GymFlow.InterfazUsuario
{
    public class FrmNotaCredito : Form
    {
        private readonly ClienteServicio _clienteServicio;
        private readonly CuentaCorrienteServicio _cuentaCorrienteServicio;
        private readonly NotaCreditoServicio _notaCreditoServicio;

        private Cliente _clienteCargado;

        private Label lblDni;
        private TextBox txtDni;
        private Button btnBuscar;
        private Label lblCliente;
        private Label lblComprobanteTitulo;
        private ComboBox cmbComprobantes;
        private Label lblDetalleComprobante;
        private Label lblImporteTitulo;
        private TextBox txtImporte;
        private Label lblMotivoTitulo;
        private TextBox txtMotivo;
        private Button btnEmitir;

        public FrmNotaCredito(ClienteServicio clienteServicio, CuentaCorrienteServicio cuentaCorrienteServicio, NotaCreditoServicio notaCreditoServicio)
        {
            _clienteServicio = clienteServicio;
            _cuentaCorrienteServicio = cuentaCorrienteServicio;
            _notaCreditoServicio = notaCreditoServicio;

            InitializeComponent();
        }

        private void InitializeComponent()
        {
            this.lblDni = new Label();
            this.txtDni = new TextBox();
            this.btnBuscar = new Button();
            this.lblCliente = new Label();
            this.lblComprobanteTitulo = new Label();
            this.cmbComprobantes = new ComboBox();
            this.lblDetalleComprobante = new Label();
            this.lblImporteTitulo = new Label();
            this.txtImporte = new TextBox();
            this.lblMotivoTitulo = new Label();
            this.txtMotivo = new TextBox();
            this.btnEmitir = new Button();

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
            this.lblCliente.Text = string.Empty;

            this.lblComprobanteTitulo.Location = new Point(20, 98);
            this.lblComprobanteTitulo.Size = new Size(100, 20);
            this.lblComprobanteTitulo.Text = "Nota de venta:";

            this.cmbComprobantes.Location = new Point(130, 95);
            this.cmbComprobantes.Size = new Size(250, 21);
            this.cmbComprobantes.DropDownStyle = ComboBoxStyle.DropDownList;
            this.cmbComprobantes.SelectedIndexChanged += new EventHandler(this.cmbComprobantes_SelectedIndexChanged);

            this.lblDetalleComprobante.Location = new Point(20, 128);
            this.lblDetalleComprobante.Size = new Size(460, 20);
            this.lblDetalleComprobante.Text = string.Empty;

            this.lblImporteTitulo.Location = new Point(20, 163);
            this.lblImporteTitulo.Size = new Size(100, 20);
            this.lblImporteTitulo.Text = "Importe:";

            this.txtImporte.Location = new Point(130, 160);
            this.txtImporte.Size = new Size(120, 20);

            this.lblMotivoTitulo.Location = new Point(20, 198);
            this.lblMotivoTitulo.Size = new Size(100, 20);
            this.lblMotivoTitulo.Text = "Motivo:";

            this.txtMotivo.Location = new Point(130, 195);
            this.txtMotivo.Size = new Size(330, 20);

            this.btnEmitir.Location = new Point(20, 245);
            this.btnEmitir.Size = new Size(190, 32);
            this.btnEmitir.Text = "Emitir nota de crédito";
            this.btnEmitir.Enabled = false;
            this.btnEmitir.Click += new EventHandler(this.btnEmitir_Click);

            this.Text = "Emitir nota de crédito";
            this.Size = new Size(500, 340);
            this.FormBorderStyle = FormBorderStyle.FixedDialog;
            this.MaximizeBox = false;
            this.StartPosition = FormStartPosition.CenterParent;
            this.AcceptButton = this.btnBuscar;

            this.Controls.Add(this.lblDni);
            this.Controls.Add(this.txtDni);
            this.Controls.Add(this.btnBuscar);
            this.Controls.Add(this.lblCliente);
            this.Controls.Add(this.lblComprobanteTitulo);
            this.Controls.Add(this.cmbComprobantes);
            this.Controls.Add(this.lblDetalleComprobante);
            this.Controls.Add(this.lblImporteTitulo);
            this.Controls.Add(this.txtImporte);
            this.Controls.Add(this.lblMotivoTitulo);
            this.Controls.Add(this.txtMotivo);
            this.Controls.Add(this.btnEmitir);
        }

        private void btnBuscar_Click(object sender, EventArgs e)
        {
            LimpiarCliente();

            int dni;

            if (!int.TryParse(this.txtDni.Text, out dni))
            {
                MessageBox.Show("Ingrese un DNI válido.", "Nota de crédito", MessageBoxButtons.OK, MessageBoxIcon.Warning);
                return;
            }

            this.Cursor = Cursors.WaitCursor;

            try
            {
                Cliente cliente = _clienteServicio.GetClienteByDNI(dni);

                if (cliente == null)
                {
                    MessageBox.Show("No existe un cliente con ese DNI.", "Nota de crédito", MessageBoxButtons.OK, MessageBoxIcon.Information);
                    return;
                }

                ResumenCuentaCorriente resumen = _cuentaCorrienteServicio.ConsultarCuentaCorriente(cliente.Id);

                if (resumen == null)
                {
                    MessageBox.Show("El cliente no tiene cuenta corriente.", "Nota de crédito", MessageBoxButtons.OK, MessageBoxIcon.Information);
                    return;
                }

                List<NotaVenta> notasVenta = new List<NotaVenta>();

                foreach (Movimiento movimiento in resumen.Movimientos)
                {
                    NotaVenta notaVenta = movimiento.Comprobante as NotaVenta;

                    if (notaVenta != null)
                    {
                        notasVenta.Add(notaVenta);
                    }
                }

                _clienteCargado = cliente;
                this.lblCliente.Text = cliente.Apellido + " " + cliente.Nombre + " (DNI " + cliente.Dni + ")";

                if (notasVenta.Count == 0)
                {
                    MessageBox.Show("El cliente no tiene notas de venta para acreditar.", "Nota de crédito", MessageBoxButtons.OK, MessageBoxIcon.Information);
                    return;
                }

                this.cmbComprobantes.DataSource = notasVenta;
                this.cmbComprobantes.DisplayMember = "Numero";
                this.cmbComprobantes.ValueMember = "Id";
                this.btnEmitir.Enabled = true;
            }
            catch (Exception ex)
            {
                Debug.WriteLine(ex.ToString());
                LimpiarCliente();
                MessageBox.Show("No se pudo consultar la cuenta del cliente. Intente nuevamente.", "Nota de crédito", MessageBoxButtons.OK, MessageBoxIcon.Error);
            }
            finally
            {
                this.Cursor = Cursors.Default;
            }
        }

        private void cmbComprobantes_SelectedIndexChanged(object sender, EventArgs e)
        {
            NotaVenta notaVenta = this.cmbComprobantes.SelectedItem as NotaVenta;

            if (notaVenta == null)
            {
                this.lblDetalleComprobante.Text = string.Empty;
                return;
            }

            this.lblDetalleComprobante.Text = "Emitida el " + notaVenta.FechaEmision.ToString("dd/MM/yyyy") + " - Total: " + notaVenta.Total.ToString("N2");
        }

        private void btnEmitir_Click(object sender, EventArgs e)
        {
            NotaVenta notaVenta = this.cmbComprobantes.SelectedItem as NotaVenta;

            if (_clienteCargado == null || notaVenta == null)
            {
                return;
            }

            decimal importe;

            if (!decimal.TryParse(this.txtImporte.Text, out importe) || importe <= 0)
            {
                MessageBox.Show("Ingrese un importe mayor a cero.", "Nota de crédito", MessageBoxButtons.OK, MessageBoxIcon.Warning);
                this.txtImporte.Focus();
                return;
            }

            string motivo = this.txtMotivo.Text.Trim();

            if (motivo.Length == 0)
            {
                MessageBox.Show("Ingrese el motivo de la nota de crédito.", "Nota de crédito", MessageBoxButtons.OK, MessageBoxIcon.Warning);
                this.txtMotivo.Focus();
                return;
            }

            string pregunta = "Se emitirá una nota de crédito por " + importe.ToString("N2")
                + " sobre la nota de venta " + notaVenta.Numero + " (total " + notaVenta.Total.ToString("N2") + ")" + Environment.NewLine
                + "Cliente: " + this.lblCliente.Text + Environment.NewLine
                + "Motivo: " + motivo + Environment.NewLine + Environment.NewLine
                + "¿Confirma la operación?";

            if (MessageBox.Show(pregunta, "Nota de crédito", MessageBoxButtons.YesNo, MessageBoxIcon.Question) != DialogResult.Yes)
            {
                return;
            }

            this.btnEmitir.Enabled = false;
            this.Cursor = Cursors.WaitCursor;

            try
            {
                bool emitida = _notaCreditoServicio.EmitirNotaCredito(_clienteCargado.Id, notaVenta.Id, importe, motivo);

                if (!emitida)
                {
                    MessageBox.Show(
                        "No se pudo emitir la nota de crédito. Verifique que el importe no supere el total de la nota de venta y que el tipo de movimiento CREDITO esté cargado.",
                        "Nota de crédito", MessageBoxButtons.OK, MessageBoxIcon.Warning);
                    return;
                }

                MessageBox.Show("La nota de crédito se emitió correctamente.", "Nota de crédito", MessageBoxButtons.OK, MessageBoxIcon.Information);

                this.txtImporte.Text = string.Empty;
                this.txtMotivo.Text = string.Empty;
            }
            catch (Exception ex)
            {
                Debug.WriteLine(ex.ToString());
                MessageBox.Show("Ocurrió un error al emitir la nota de crédito. Verifique la cuenta corriente antes de reintentar.", "Nota de crédito", MessageBoxButtons.OK, MessageBoxIcon.Error);
            }
            finally
            {
                this.Cursor = Cursors.Default;
                this.btnEmitir.Enabled = true;
            }
        }

        private void LimpiarCliente()
        {
            _clienteCargado = null;
            this.lblCliente.Text = string.Empty;
            this.lblDetalleComprobante.Text = string.Empty;
            this.cmbComprobantes.DataSource = null;
            this.btnEmitir.Enabled = false;
        }
    }
}
