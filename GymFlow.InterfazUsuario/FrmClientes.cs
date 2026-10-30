using GymFlow.Aplicacion;
using GymFlow.Dominio;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Drawing;
using System.Windows.Forms;

namespace GymFlow.InterfazUsuario
{
    public class FrmClientes : Form
    {
        private readonly ClienteServicio _clienteServicio;

        private List<Cliente> _clientes;

        private DataGridView dgvClientes;
        private Label lblDni;
        private TextBox txtDni;
        private Label lblNombre;
        private TextBox txtNombre;
        private Label lblApellido;
        private TextBox txtApellido;
        private Label lblTelefono;
        private TextBox txtTelefono;


        private Button btnAlta;
        private Button btnModificar;
        private Button btnBaja;
        private Button btnLimpiar;

        public FrmClientes(ClienteServicio clienteServicio)
        {
            _clienteServicio = clienteServicio;
            _clientes = new List<Cliente>();

            InitializeComponent();
            CargarClientes();
        }

        private void InitializeComponent()
        {
            this.dgvClientes = new DataGridView();
            this.lblDni = new Label();
            this.txtDni = new TextBox();
            this.lblNombre = new Label();
            this.txtNombre = new TextBox();
            this.lblApellido = new Label();
            this.txtApellido = new TextBox();
            this.lblTelefono = new Label();
            this.txtTelefono = new TextBox();


            this.btnAlta = new Button();
            this.btnModificar = new Button();
            this.btnBaja = new Button();
            this.btnLimpiar = new Button();

            this.dgvClientes.Location = new Point(20, 20);
            this.dgvClientes.Size = new Size(640, 220);
            this.dgvClientes.ReadOnly = true;
            this.dgvClientes.AllowUserToAddRows = false;
            this.dgvClientes.RowHeadersVisible = false;
            this.dgvClientes.SelectionMode = DataGridViewSelectionMode.FullRowSelect;
            this.dgvClientes.AutoSizeColumnsMode = DataGridViewAutoSizeColumnsMode.Fill;
            this.dgvClientes.Columns.Add("colDni", "DNI");
            this.dgvClientes.Columns.Add("colApellido", "Apellido");
            this.dgvClientes.Columns.Add("colNombre", "Nombre");
            this.dgvClientes.Columns.Add("colTelefono", "Teléfono");
            this.dgvClientes.SelectionChanged += new EventHandler(this.dgvClientes_SelectionChanged);

            AgregarCampo(this.lblDni, this.txtDni, "DNI:", 20, 255);
            AgregarCampo(this.lblNombre, this.txtNombre, "Nombre:", 20, 285);
            AgregarCampo(this.lblApellido, this.txtApellido, "Apellido:", 340, 255);
            AgregarCampo(this.lblTelefono, this.txtTelefono, "Teléfono:", 340, 285);

            this.btnAlta.Location = new Point(20, 320);
            this.btnAlta.Size = new Size(100, 30);
            this.btnAlta.Text = "Alta";
            this.btnAlta.Click += new EventHandler(this.btnAlta_Click);

            this.btnModificar.Location = new Point(130, 320);
            this.btnModificar.Size = new Size(100, 30);
            this.btnModificar.Text = "Modificar";
            this.btnModificar.Click += new EventHandler(this.btnModificar_Click);

            this.btnBaja.Location = new Point(240, 320);
            this.btnBaja.Size = new Size(100, 30);
            this.btnBaja.Text = "Baja";
            this.btnBaja.Click += new EventHandler(this.btnBaja_Click);

            this.btnLimpiar.Location = new Point(350, 320);
            this.btnLimpiar.Size = new Size(100, 30);
            this.btnLimpiar.Text = "Limpiar";
            this.btnLimpiar.Click += new EventHandler(this.btnLimpiar_Click);

            this.Text = "Gestionar clientes";
            this.Size = new Size(700, 415);
            this.FormBorderStyle = FormBorderStyle.FixedDialog;
            this.MaximizeBox = false;
            this.StartPosition = FormStartPosition.CenterParent;

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
            cuadro.Size = new Size(190, 20);

            this.Controls.Add(etiqueta);
            this.Controls.Add(cuadro);
        }

        private void CargarClientes()
        {
            this.dgvClientes.Rows.Clear();
            LimpiarCampos();

            try
            {
                _clientes = _clienteServicio.GetAllClientes();

                foreach (Cliente cliente in _clientes)
                {
                    this.dgvClientes.Rows.Add(cliente.Dni, cliente.Apellido, cliente.Nombre, cliente.Telefono);
                }
            }
            catch (Exception ex)
            {
                Debug.WriteLine(ex.ToString());
                MessageBox.Show("No se pudo cargar la lista de clientes.", "Gestionar clientes", MessageBoxButtons.OK, MessageBoxIcon.Error);
            }
        }

        private void LimpiarCampos()
        {
            this.txtDni.Text = string.Empty;
            this.txtNombre.Text = string.Empty;
            this.txtApellido.Text = string.Empty;
            this.txtTelefono.Text = string.Empty;
        }

        private Cliente ClienteSeleccionado()
        {
            if (this.dgvClientes.CurrentRow == null)
            {
                return null;
            }

            int indice = this.dgvClientes.CurrentRow.Index;

            if (indice < 0 || indice >= _clientes.Count)
            {
                return null;
            }

            return _clientes[indice];
        }

        private bool LeerCampos(Cliente cliente)
        {
            int dni;

            if (!int.TryParse(this.txtDni.Text, out dni))
            {
                MessageBox.Show("Ingrese un DNI válido.", "Gestionar clientes", MessageBoxButtons.OK, MessageBoxIcon.Warning);
                return false;
            }

            cliente.Dni = dni;
            cliente.Nombre = this.txtNombre.Text.Trim();
            cliente.Apellido = this.txtApellido.Text.Trim();
            cliente.Telefono = this.txtTelefono.Text.Trim();
            return true;
        }

        private void dgvClientes_SelectionChanged(object sender, EventArgs e)
        {
            Cliente cliente = ClienteSeleccionado();

            if (cliente == null)
            {
                return;
            }

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
            Cliente seleccionado = ClienteSeleccionado();

            if (seleccionado == null)
            {
                MessageBox.Show("Seleccione un cliente de la lista.", "Gestionar clientes", MessageBoxButtons.OK, MessageBoxIcon.Information);
                return;
            }

            Cliente copia = new Cliente();
            copia.Id = seleccionado.Id;
            copia.UsuarioId = seleccionado.UsuarioId;

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
            Cliente seleccionado = ClienteSeleccionado();

            if (seleccionado == null)
            {
                MessageBox.Show("Seleccione un cliente de la lista.", "Gestionar clientes", MessageBoxButtons.OK, MessageBoxIcon.Information);
                return;
            }

            string pregunta = "¿Dar de baja a " + seleccionado.Apellido + " " + seleccionado.Nombre + " (DNI " + seleccionado.Dni + ")?";

            if (MessageBox.Show(pregunta, "Gestionar clientes", MessageBoxButtons.YesNo, MessageBoxIcon.Question) != DialogResult.Yes)
            {
                return;
            }

            try
            {
                if (!_clienteServicio.DeleteCliente(seleccionado.Id))
                {
                    MessageBox.Show("No se puede dar de baja a un cliente que tiene cuotas, inscripciones o movimientos en su cuenta corriente.", "Gestionar clientes", MessageBoxButtons.OK, MessageBoxIcon.Warning);
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
