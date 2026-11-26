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
        private List<EstadoCliente> _estadosMostrados;

        private Label lblFiltroEstado;
        private ComboBox cmbFiltro;
        private Label lblFiltroDni;
        private TextBox txtFiltroDni;
        private Label lblFiltroNombre;
        private TextBox txtFiltroNombre;
        private Label lblFiltroApellido;
        private TextBox txtFiltroApellido;
        private Button btnLimpiarFiltros;
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
            _estadosMostrados = new List<EstadoCliente>();

            InitializeComponent();
            CargarClientes();
        }

        private void InitializeComponent()
        {
            this.lblFiltroEstado = new Label();
            this.cmbFiltro = new ComboBox();
            this.lblFiltroDni = new Label();
            this.txtFiltroDni = new TextBox();
            this.lblFiltroNombre = new Label();
            this.txtFiltroNombre = new TextBox();
            this.lblFiltroApellido = new Label();
            this.txtFiltroApellido = new TextBox();
            this.btnLimpiarFiltros = new Button();
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

            // Filtros y resumen
            this.lblFiltroEstado.Location = new Point(20, 21);
            this.lblFiltroEstado.Size = new Size(50, 20);
            this.lblFiltroEstado.Text = "Estado:";

            this.cmbFiltro.Location = new Point(75, 18);
            this.cmbFiltro.Size = new Size(130, 21);
            this.cmbFiltro.DropDownStyle = ComboBoxStyle.DropDownList;
            this.cmbFiltro.Items.Add("Todos");
            this.cmbFiltro.Items.Add(EstadosCliente.Activo);
            this.cmbFiltro.Items.Add(EstadosCliente.Vencido);
            this.cmbFiltro.Items.Add(EstadosCliente.Pendiente);
            this.cmbFiltro.Items.Add(EstadosCliente.SinCuota);
            this.cmbFiltro.SelectedIndex = 0;
            this.cmbFiltro.SelectedIndexChanged += new EventHandler(this.Filtros_Changed);

            this.lblFiltroDni.Location = new Point(220, 21);
            this.lblFiltroDni.Size = new Size(35, 20);
            this.lblFiltroDni.Text = "DNI:";

            this.txtFiltroDni.Location = new Point(260, 18);
            this.txtFiltroDni.Size = new Size(130, 20);
            this.txtFiltroDni.TextChanged += new EventHandler(this.Filtros_Changed);

            this.btnLimpiarFiltros.Location = new Point(608, 16);
            this.btnLimpiarFiltros.Size = new Size(112, 25);
            this.btnLimpiarFiltros.Text = "Limpiar filtros";
            this.btnLimpiarFiltros.Click += new EventHandler(this.btnLimpiarFiltros_Click);

            this.lblFiltroNombre.Location = new Point(20, 51);
            this.lblFiltroNombre.Size = new Size(55, 20);
            this.lblFiltroNombre.Text = "Nombre:";

            this.txtFiltroNombre.Location = new Point(75, 48);
            this.txtFiltroNombre.Size = new Size(180, 20);
            this.txtFiltroNombre.TextChanged += new EventHandler(this.Filtros_Changed);

            this.lblFiltroApellido.Location = new Point(275, 51);
            this.lblFiltroApellido.Size = new Size(55, 20);
            this.lblFiltroApellido.Text = "Apellido:";

            this.txtFiltroApellido.Location = new Point(335, 48);
            this.txtFiltroApellido.Size = new Size(180, 20);
            this.txtFiltroApellido.TextChanged += new EventHandler(this.Filtros_Changed);

            this.lblResumen.Location = new Point(20, 78);
            this.lblResumen.Size = new Size(700, 20);
            this.lblResumen.Text = string.Empty;

            // Grilla
            this.dgvClientes.Location = new Point(20, 102);
            this.dgvClientes.Size = new Size(700, 215);
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

            AgregarCampo(this.lblDni, this.txtDni, "DNI:", 20, 332);
            AgregarCampo(this.lblNombre, this.txtNombre, "Nombre:", 20, 362);
            AgregarCampo(this.lblApellido, this.txtApellido, "Apellido:", 380, 332);
            AgregarCampo(this.lblTelefono, this.txtTelefono, "Teléfono:", 380, 362);

            this.btnAlta.Location = new Point(20, 398);
            this.btnAlta.Size = new Size(100, 30);
            this.btnAlta.Text = "Alta";
            this.btnAlta.Click += new EventHandler(this.btnAlta_Click);

            this.btnModificar.Location = new Point(130, 398);
            this.btnModificar.Size = new Size(100, 30);
            this.btnModificar.Text = "Modificar";
            this.btnModificar.Click += new EventHandler(this.btnModificar_Click);

            this.btnBaja.Location = new Point(240, 398);
            this.btnBaja.Size = new Size(100, 30);
            this.btnBaja.Text = "Baja";
            this.btnBaja.Click += new EventHandler(this.btnBaja_Click);

            this.btnLimpiar.Location = new Point(350, 398);
            this.btnLimpiar.Size = new Size(100, 30);
            this.btnLimpiar.Text = "Limpiar";
            this.btnLimpiar.Click += new EventHandler(this.btnLimpiar_Click);

            this.Text = "Gestionar clientes";
            this.Size = new Size(760, 490);
            this.FormBorderStyle = FormBorderStyle.FixedDialog;
            this.MaximizeBox = false;
            this.StartPosition = FormStartPosition.CenterParent;

            this.Controls.Add(this.lblFiltroEstado);
            this.Controls.Add(this.cmbFiltro);
            this.Controls.Add(this.lblFiltroDni);
            this.Controls.Add(this.txtFiltroDni);
            this.Controls.Add(this.lblFiltroNombre);
            this.Controls.Add(this.txtFiltroNombre);
            this.Controls.Add(this.lblFiltroApellido);
            this.Controls.Add(this.txtFiltroApellido);
            this.Controls.Add(this.btnLimpiarFiltros);
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

            string filtroEstado = this.cmbFiltro.SelectedItem as string;
            string filtroDni = this.txtFiltroDni.Text.Trim();
            string filtroNombre = this.txtFiltroNombre.Text.Trim();
            string filtroApellido = this.txtFiltroApellido.Text.Trim();

            IEnumerable<EstadoCliente> consulta = _estadosClientes;

            if (!string.IsNullOrEmpty(filtroEstado) && filtroEstado != "Todos")
            {
                consulta = consulta.Where(e => e.Estado == filtroEstado);
            }

            if (filtroDni.Length > 0)
            {
                consulta = consulta.Where(e => e.Cliente.Dni.ToString().Contains(filtroDni));
            }

            if (filtroNombre.Length > 0)
            {
                consulta = consulta.Where(e => e.Cliente.Nombre != null
                    && e.Cliente.Nombre.IndexOf(filtroNombre, StringComparison.OrdinalIgnoreCase) >= 0);
            }

            if (filtroApellido.Length > 0)
            {
                consulta = consulta.Where(e => e.Cliente.Apellido != null
                    && e.Cliente.Apellido.IndexOf(filtroApellido, StringComparison.OrdinalIgnoreCase) >= 0);
            }

            _estadosMostrados = consulta
                .OrderByDescending(e => e.Cliente.Id)
                .ToList();

            foreach (EstadoCliente estado in _estadosMostrados)
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

            this.lblResumen.Text = "Mostrando " + _estadosMostrados.Count + " de " + _estadosClientes.Count + " clientes";
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

            if (indice < 0 || indice >= _estadosMostrados.Count)
            {
                return null;
            }

            return _estadosMostrados[indice];
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

        private void Filtros_Changed(object sender, EventArgs e)
        {
            AplicarFiltro();
        }

        private void btnLimpiarFiltros_Click(object sender, EventArgs e)
        {
            this.cmbFiltro.SelectedIndex = 0;
            this.txtFiltroDni.Text = string.Empty;
            this.txtFiltroNombre.Text = string.Empty;
            this.txtFiltroApellido.Text = string.Empty;
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
                    MessageBox.Show("No se pudo dar de alta. Complete DNI, nombre y apellido, y verifique que el DNI no esté registrado en clientes ni en empleados.", "Gestionar clientes", MessageBoxButtons.OK, MessageBoxIcon.Warning);
                    return;
                }

                CargarClientes();
            }
            catch (Exception ex)
            {
                Debug.WriteLine(ex.ToString());
                if (ConcurrenciaUI.EsConflictoDeUnicidad(ex))
                {
                    MessageBox.Show("Ya existe una persona registrada con ese DNI.", "Gestionar clientes", MessageBoxButtons.OK, MessageBoxIcon.Warning);
                    return;
                }

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
                    MessageBox.Show("No se pudo modificar. Complete DNI, nombre y apellido, y verifique que el DNI no pertenezca a otra persona.", "Gestionar clientes", MessageBoxButtons.OK, MessageBoxIcon.Warning);
                    return;
                }

                CargarClientes();
            }
            catch (Exception ex)
            {
                Debug.WriteLine(ex.ToString());
                if (ConcurrenciaUI.EsConflictoDeUnicidad(ex))
                {
                    MessageBox.Show("Ya existe otra persona registrada con ese DNI.", "Gestionar clientes", MessageBoxButtons.OK, MessageBoxIcon.Warning);
                    return;
                }

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
