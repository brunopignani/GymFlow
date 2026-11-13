using GymFlow.Aplicacion;
using GymFlow.Dominio;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Drawing;
using System.Linq;
using System.Windows.Forms;

namespace GymFlow.InterfazUsuario
{
    public class FrmClientes : Form
    {
        private readonly ClienteServicio _clienteServicio;

        private List<EstadoCliente> _estadosClientes;

        private ComboBox cmbFiltro;
        private Label lblResumen;
        private DataGridView dgvClientes;
        private Label lblDni;
        private TextBox txtDni;
        private Label lblNombre;
        private TextBox txtNombre;
        private Label lblApellido;
        private TextBox txtApellido;
        private Label lblTelefono;
        private TextBox txtTelefono;
        private ErrorProvider errorProvider;

        private Button btnAlta;
        private Button btnModificar;
        private Button btnBaja;
        private Button btnLimpiar;

        public FrmClientes(ClienteServicio clienteServicio)
        {
            _clienteServicio = clienteServicio;
            _estadosClientes = new List<EstadoCliente>();

            InitializeComponent();
            CargarClientes();
        }

        private void InitializeComponent()
        {
            this.cmbFiltro = new ComboBox();
            this.lblResumen = new Label();
            this.dgvClientes = new DataGridView();
            this.lblDni = new Label();
            this.txtDni = new TextBox();
            this.lblNombre = new Label();
            this.txtNombre = new TextBox();
            this.lblApellido = new Label();
            this.txtApellido = new TextBox();
            this.lblTelefono = new Label();
            this.txtTelefono = new TextBox();
            this.errorProvider = new ErrorProvider(this);

            this.btnAlta = new Button();
            this.btnModificar = new Button();
            this.btnBaja = new Button();
            this.btnLimpiar = new Button();

            // Filtro y resumen
            this.cmbFiltro.Location = new Point(20, 20);
            this.cmbFiltro.Size = new Size(160, 21);
            this.cmbFiltro.DropDownStyle = ComboBoxStyle.DropDownList;
            this.cmbFiltro.Items.Add("Todos");
            this.cmbFiltro.Items.Add(EstadosCliente.Activo);
            this.cmbFiltro.Items.Add(EstadosCliente.Vencido);
            this.cmbFiltro.Items.Add(EstadosCliente.Pendiente);
            this.cmbFiltro.Items.Add(EstadosCliente.SinCuota);
            this.cmbFiltro.SelectedIndex = 0;
            this.cmbFiltro.SelectedIndexChanged += new EventHandler(this.cmbFiltro_SelectedIndexChanged);

            this.lblResumen.Location = new Point(195, 23);
            this.lblResumen.Size = new Size(480, 20);
            this.lblResumen.Text = string.Empty;

            // Grilla
            this.dgvClientes.Location = new Point(20, 48);
            this.dgvClientes.Size = new Size(700, 220);
            this.dgvClientes.ReadOnly = true;
            this.dgvClientes.AllowUserToAddRows = false;
            this.dgvClientes.RowHeadersVisible = false;
            this.dgvClientes.SelectionMode = DataGridViewSelectionMode.FullRowSelect;
            this.dgvClientes.AutoSizeColumnsMode = DataGridViewAutoSizeColumnsMode.Fill;
            this.dgvClientes.Columns.Add("colDni", "DNI");
            this.dgvClientes.Columns.Add("colApellido", "Apellido");
            this.dgvClientes.Columns.Add("colNombre", "Nombre");
            this.dgvClientes.Columns.Add("colTelefono", "Teléfono");
            this.dgvClientes.Columns.Add("colEstado", "Estado cuota");
            this.dgvClientes.Columns.Add("colVencimiento", "Vencimiento");
            this.dgvClientes.SelectionChanged += new EventHandler(this.dgvClientes_SelectionChanged);

            AgregarCampo(this.lblDni, this.txtDni, "DNI:", 20, 285);
            AgregarCampo(this.lblNombre, this.txtNombre, "Nombre:", 20, 315);
            AgregarCampo(this.lblApellido, this.txtApellido, "Apellido:", 380, 285);
            AgregarCampo(this.lblTelefono, this.txtTelefono, "Teléfono:", 380, 315);

            this.btnAlta.Location = new Point(20, 350);
            this.btnAlta.Size = new Size(100, 30);
            this.btnAlta.Text = "Alta";
            this.btnAlta.Click += new EventHandler(this.btnAlta_Click);

            this.btnModificar.Location = new Point(130, 350);
            this.btnModificar.Size = new Size(100, 30);
            this.btnModificar.Text = "Modificar";
            this.btnModificar.Click += new EventHandler(this.btnModificar_Click);

            this.btnBaja.Location = new Point(240, 350);
            this.btnBaja.Size = new Size(100, 30);
            this.btnBaja.Text = "Baja";
            this.btnBaja.Click += new EventHandler(this.btnBaja_Click);

            this.btnLimpiar.Location = new Point(350, 350);
            this.btnLimpiar.Size = new Size(100, 30);
            this.btnLimpiar.Text = "Limpiar";
            this.btnLimpiar.Click += new EventHandler(this.btnLimpiar_Click);

            this.Text = "Gestionar clientes";
            this.Size = new Size(760, 440);
            this.FormBorderStyle = FormBorderStyle.FixedDialog;
            this.MaximizeBox = false;
            this.StartPosition = FormStartPosition.CenterParent;

            this.Controls.Add(this.cmbFiltro);
            this.Controls.Add(this.lblResumen);
            this.Controls.Add(this.dgvClientes);
            this.Controls.Add(this.btnAlta);
            this.Controls.Add(this.btnModificar);
            this.Controls.Add(this.btnBaja);
            this.Controls.Add(this.btnLimpiar);
        }

        private void AgregarCampo(Label etiqueta, TextBox cuadro, string texto, int x, int y)
        {
            etiqueta.Location = new Point(x, y + 3);
            etiqueta.Size = new Size(70, 20);
            etiqueta.Text = texto;

            cuadro.Location = new Point(x + 80, y);
            cuadro.Size = new Size(240, 20);

            this.Controls.Add(etiqueta);
            this.Controls.Add(cuadro);
        }

        private void CargarClientes()
        {
            try
            {
                _estadosClientes = _clienteServicio.GetEstadoClientes();
            }
            catch (Exception ex)
            {
                Debug.WriteLine(ex.ToString());
                MessageBox.Show("No se pudo cargar la lista de clientes.", "Gestionar clientes", MessageBoxButtons.OK, MessageBoxIcon.Error);
                _estadosClientes = new List<EstadoCliente>();
            }

            AplicarFiltro();
        }

        private void AplicarFiltro()
        {
            this.dgvClientes.Rows.Clear();
            LimpiarCampos();

            string filtro = this.cmbFiltro.SelectedItem as string;

            List<EstadoCliente> mostrar = filtro == "Todos"
                ? _estadosClientes
                : _estadosClientes.Where(e => e.Estado == filtro).ToList();

            foreach (EstadoCliente estado in mostrar)
            {
                Cliente c = estado.Cliente;
                string estadoTexto = estado.Estado;
                string vencimiento;

                if (estadoTexto == EstadosCliente.Pendiente)
                {
                    vencimiento = "Pendiente";
                }
                else if (estadoTexto == EstadosCliente.SinCuota)
                {
                    vencimiento = "-";
                }
                else
                {
                    vencimiento = estado.CuotaCliente.FechaVencimiento.Value.ToString("dd/MM/yyyy");
                }

                this.dgvClientes.Rows.Add(c.Dni, c.Apellido, c.Nombre, c.Telefono, estadoTexto, vencimiento);
            }

            this.lblResumen.Text = "Mostrando " + mostrar.Count + " de " + _estadosClientes.Count + " clientes";
        }

        private void LimpiarCampos()
        {
            this.txtDni.Text = string.Empty;
            this.txtNombre.Text = string.Empty;
            this.txtApellido.Text = string.Empty;
            this.txtTelefono.Text = string.Empty;
        }

        private EstadoCliente EstadoSeleccionado()
        {
            if (this.dgvClientes.CurrentRow == null)
            {
                return null;
            }

            int indice = this.dgvClientes.CurrentRow.Index;
            string filtro = this.cmbFiltro.SelectedItem as string;

            List<EstadoCliente> mostrar = filtro == "Todos"
                ? _estadosClientes
                : _estadosClientes.Where(e => e.Estado == filtro).ToList();

            if (indice < 0 || indice >= mostrar.Count)
            {
                return null;
            }

            return mostrar[indice];
        }

        private bool LeerCampos(Cliente cliente)
        {
            int dni;
            this.errorProvider.Clear();

            if (!ValidacionesDatos.TryParseDni(this.txtDni.Text, out dni))
            {
                this.errorProvider.SetError(this.txtDni, "Ingrese solo 7 u 8 dígitos, entre 1.000.000 y 99.999.999.");
            }

            if (!ValidacionesDatos.EsNombreValido(this.txtNombre.Text))
            {
                this.errorProvider.SetError(this.txtNombre, "Ingrese un nombre de hasta 100 caracteres.");
            }

            if (!ValidacionesDatos.EsNombreValido(this.txtApellido.Text))
            {
                this.errorProvider.SetError(this.txtApellido, "Ingrese un apellido de hasta 100 caracteres.");
            }

            if (!ValidacionesDatos.EsTelefonoValido(this.txtTelefono.Text))
            {
                this.errorProvider.SetError(this.txtTelefono, "Ingrese un teléfono válido: al menos 7 dígitos y hasta 30 caracteres.");
            }

            if (this.errorProvider.GetError(this.txtDni).Length > 0
                || this.errorProvider.GetError(this.txtNombre).Length > 0
                || this.errorProvider.GetError(this.txtApellido).Length > 0
                || this.errorProvider.GetError(this.txtTelefono).Length > 0)
            {
                return false;
            }

            cliente.Dni = dni;
            cliente.Nombre = this.txtNombre.Text.Trim();
            cliente.Apellido = this.txtApellido.Text.Trim();
            cliente.Telefono = this.txtTelefono.Text.Trim();
            return true;
        }

        private void cmbFiltro_SelectedIndexChanged(object sender, EventArgs e)
        {
            AplicarFiltro();
        }

        private void dgvClientes_SelectionChanged(object sender, EventArgs e)
        {
            EstadoCliente estado = EstadoSeleccionado();

            if (estado == null)
            {
                return;
            }

            Cliente cliente = estado.Cliente;
            this.txtDni.Text = cliente.Dni.ToString();
            this.txtNombre.Text = cliente.Nombre;
            this.txtApellido.Text = cliente.Apellido;
            this.txtTelefono.Text = cliente.Telefono;
        }

        private void btnAlta_Click(object sender, EventArgs e)
        {
            Cliente nuevo = new Cliente();

            if (!LeerCampos(nuevo))
            {
                return;
            }

            try
            {
                if (!_clienteServicio.AddCliente(nuevo))
                {
                    MessageBox.Show("No se pudo dar de alta. Complete DNI, nombre y apellido, y verifique que el DNI no esté repetido.", "Gestionar clientes", MessageBoxButtons.OK, MessageBoxIcon.Warning);
                    return;
                }

                CargarClientes();
            }
            catch (Exception ex)
            {
                Debug.WriteLine(ex.ToString());
                MessageBox.Show("Ocurrió un error al guardar el cliente.", "Gestionar clientes", MessageBoxButtons.OK, MessageBoxIcon.Error);
            }
        }

        private void btnModificar_Click(object sender, EventArgs e)
        {
            EstadoCliente estado = EstadoSeleccionado();

            if (estado == null)
            {
                MessageBox.Show("Seleccione un cliente de la lista.", "Gestionar clientes", MessageBoxButtons.OK, MessageBoxIcon.Information);
                return;
            }

            Cliente copia = new Cliente();
            copia.Id = estado.Cliente.Id;
            copia.UsuarioId = estado.Cliente.UsuarioId;

            if (!LeerCampos(copia))
            {
                return;
            }

            try
            {
                if (!_clienteServicio.UpdateCliente(copia))
                {
                    MessageBox.Show("No se pudo modificar. Complete DNI, nombre y apellido, y verifique que el DNI no pertenezca a otro cliente.", "Gestionar clientes", MessageBoxButtons.OK, MessageBoxIcon.Warning);
                    return;
                }

                CargarClientes();
            }
            catch (Exception ex)
            {
                Debug.WriteLine(ex.ToString());
                MessageBox.Show("Ocurrió un error al modificar el cliente.", "Gestionar clientes", MessageBoxButtons.OK, MessageBoxIcon.Error);
            }
        }

        private void btnBaja_Click(object sender, EventArgs e)
        {
            EstadoCliente estado = EstadoSeleccionado();

            if (estado == null)
            {
                MessageBox.Show("Seleccione un cliente de la lista.", "Gestionar clientes", MessageBoxButtons.OK, MessageBoxIcon.Information);
                return;
            }

            Cliente seleccionado = estado.Cliente;
            string pregunta = "¿Dar de baja a " + seleccionado.Apellido + " " + seleccionado.Nombre + " (DNI " + seleccionado.Dni + ")?";

            if (MessageBox.Show(pregunta, "Gestionar clientes", MessageBoxButtons.YesNo, MessageBoxIcon.Question) != DialogResult.Yes)
            {
                return;
            }

            try
            {
                ResultadoBajaCliente resultado = _clienteServicio.DeleteClienteConResultado(seleccionado.Id);

                if (resultado != ResultadoBajaCliente.Eliminado)
                {
                    string mensaje = resultado == ResultadoBajaCliente.ClienteInexistente
                        ? "El cliente ya no existe."
                        : resultado == ResultadoBajaCliente.CuotasNoIniciales
                            ? "La baja está bloqueada porque el cliente tiene cuotas vencidas o activadas."
                            : resultado == ResultadoBajaCliente.Inscripciones
                                ? "La baja está bloqueada porque el cliente tiene inscripciones."
                                : resultado == ResultadoBajaCliente.CuentaCorrienteInexistente
                                    ? "La baja está bloqueada porque el cliente no tiene cuenta corriente."
                                    : resultado == ResultadoBajaCliente.Movimientos
                                        ? "La baja está bloqueada porque la cuenta corriente tiene movimientos."
                                        : resultado == ResultadoBajaCliente.Comprobantes
                                            ? "La baja está bloqueada porque el cliente tiene comprobantes financieros."
                                            : resultado == ResultadoBajaCliente.Pagos
                                                ? "La baja está bloqueada porque el cliente tiene pagos registrados."
                                                : "La baja está bloqueada porque el cliente tiene un usuario asociado.";

                    MessageBox.Show(mensaje, "Gestionar clientes", MessageBoxButtons.OK, MessageBoxIcon.Warning);
                    return;
                }

                CargarClientes();
            }
            catch (Exception ex)
            {
                Debug.WriteLine(ex.ToString());
                MessageBox.Show("Ocurrió un error al dar de baja el cliente.", "Gestionar clientes", MessageBoxButtons.OK, MessageBoxIcon.Error);
            }
        }

        private void btnLimpiar_Click(object sender, EventArgs e)
        {
            LimpiarCampos();
        }
    }
}
