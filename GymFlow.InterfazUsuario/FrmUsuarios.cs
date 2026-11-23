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
    public class FrmUsuarios : Form
    {
        private readonly UsuarioServicio _usuarioServicio;

        private List<Empleado> _empleados;

        private DataGridView dgvUsuarios;
        private Label lblDni;
        private TextBox txtDni;
        private Label lblNombre;
        private TextBox txtNombre;
        private Label lblApellido;
        private TextBox txtApellido;
        private Label lblTelefono;
        private TextBox txtTelefono;
        private Label lblUsuario;
        private TextBox txtUsuario;
        private Label lblClave;
        private TextBox txtClave;
        private Label lblConfirmar;
        private TextBox txtConfirmar;
        private Button btnAlta;
        private Button btnModificar;
        private Button btnBaja;
        private Button btnLimpiar;
        private ErrorProvider errorProvider;

        public FrmUsuarios(UsuarioServicio usuarioServicio)
        {
            _usuarioServicio = usuarioServicio;
            _empleados = new List<Empleado>();

            InitializeComponent();
            CargarUsuarios();
        }

        private void InitializeComponent()
        {
            this.dgvUsuarios = new DataGridView();
            this.lblDni = new Label();
            this.txtDni = new TextBox();
            this.lblNombre = new Label();
            this.txtNombre = new TextBox();
            this.lblApellido = new Label();
            this.txtApellido = new TextBox();
            this.lblTelefono = new Label();
            this.txtTelefono = new TextBox();
            this.lblUsuario = new Label();
            this.txtUsuario = new TextBox();
            this.lblClave = new Label();
            this.txtClave = new TextBox();
            this.lblConfirmar = new Label();
            this.txtConfirmar = new TextBox();
            this.btnAlta = new Button();
            this.btnModificar = new Button();
            this.btnBaja = new Button();
            this.btnLimpiar = new Button();
            this.errorProvider = new ErrorProvider(this);

            this.dgvUsuarios.Location = new Point(20, 20);
            this.dgvUsuarios.Size = new Size(640, 200);
            this.dgvUsuarios.ReadOnly = true;
            this.dgvUsuarios.AllowUserToAddRows = false;
            this.dgvUsuarios.RowHeadersVisible = false;
            this.dgvUsuarios.SelectionMode = DataGridViewSelectionMode.FullRowSelect;
            this.dgvUsuarios.AutoSizeColumnsMode = DataGridViewAutoSizeColumnsMode.Fill;
            this.dgvUsuarios.Columns.Add("colUsuario", "Usuario");
            this.dgvUsuarios.Columns.Add("colEstado", "Estado");
            this.dgvUsuarios.Columns.Add("colDni", "DNI");
            this.dgvUsuarios.Columns.Add("colApellido", "Apellido");
            this.dgvUsuarios.Columns.Add("colNombre", "Nombre");
            this.dgvUsuarios.Columns.Add("colTelefono", "Teléfono");
            this.dgvUsuarios.SelectionChanged += new EventHandler(this.dgvUsuarios_SelectionChanged);

            AgregarCampo(this.lblDni, this.txtDni, "DNI:", 20, 240);
            AgregarCampo(this.lblNombre, this.txtNombre, "Nombre:", 20, 270);
            AgregarCampo(this.lblApellido, this.txtApellido, "Apellido:", 340, 240);
            AgregarCampo(this.lblTelefono, this.txtTelefono, "Teléfono:", 340, 270);
            AgregarCampo(this.lblUsuario, this.txtUsuario, "Usuario:", 20, 300);
            AgregarCampo(this.lblClave, this.txtClave, "Nueva contraseña (opcional):", 20, 330, 165, 145);
            AgregarCampo(this.lblConfirmar, this.txtConfirmar, "Confirmar nueva contraseña:", 345, 330, 165, 150);

            this.txtClave.UseSystemPasswordChar = true;
            this.txtConfirmar.UseSystemPasswordChar = true;

            this.btnAlta.Location = new Point(20, 375);
            this.btnAlta.Size = new Size(100, 30);
            this.btnAlta.Text = "Alta";
            this.btnAlta.Click += new EventHandler(this.btnAlta_Click);

            this.btnModificar.Location = new Point(130, 375);
            this.btnModificar.Size = new Size(100, 30);
            this.btnModificar.Text = "Modificar";
            this.btnModificar.Click += new EventHandler(this.btnModificar_Click);

            this.btnBaja.Location = new Point(240, 375);
            this.btnBaja.Size = new Size(100, 30);
            this.btnBaja.Text = "Baja";
            this.btnBaja.Click += new EventHandler(this.btnBaja_Click);

            this.btnLimpiar.Location = new Point(350, 375);
            this.btnLimpiar.Size = new Size(100, 30);
            this.btnLimpiar.Text = "Limpiar";
            this.btnLimpiar.Click += new EventHandler(this.btnLimpiar_Click);

            this.Text = "Gestionar usuarios";
            this.Size = new Size(700, 465);
            this.FormBorderStyle = FormBorderStyle.FixedDialog;
            this.MaximizeBox = false;
            this.StartPosition = FormStartPosition.CenterParent;

            this.Controls.Add(this.dgvUsuarios);
            this.Controls.Add(this.btnAlta);
            this.Controls.Add(this.btnModificar);
            this.Controls.Add(this.btnBaja);
            this.Controls.Add(this.btnLimpiar);
        }

        private void AgregarCampo(Label etiqueta, TextBox cuadro, string texto, int x, int y, int anchoEtiqueta = 80, int anchoCuadro = 195)
        {
            etiqueta.Location = new Point(x, y + 3);
            etiqueta.Size = new Size(anchoEtiqueta, 20);
            etiqueta.Text = texto;

            cuadro.Location = new Point(x + anchoEtiqueta + 5, y);
            cuadro.Size = new Size(anchoCuadro, 20);

            this.Controls.Add(etiqueta);
            this.Controls.Add(cuadro);
        }

        private void CargarUsuarios(int? seleccionarEmpleadoId = null)
        {
            this.dgvUsuarios.Rows.Clear();
            LimpiarCampos();

            try
            {
                _empleados = _usuarioServicio.GetEmpleados()
                    .OrderByDescending(e => e.Id)
                    .ToList();

                foreach (Empleado empleado in _empleados)
                {
                    this.dgvUsuarios.Rows.Add(
                        empleado.Usuario == null ? string.Empty : empleado.Usuario.Nombre,
                        empleado.Usuario == null ? "Sin usuario" : empleado.Usuario.Estado,
                        empleado.Dni,
                        empleado.Apellido,
                        empleado.Nombre,
                        empleado.Telefono);
                }

                if (seleccionarEmpleadoId.HasValue)
                {
                    int indice = _empleados.FindIndex(e => e.Id == seleccionarEmpleadoId.Value);
                    if (indice >= 0 && indice < this.dgvUsuarios.Rows.Count)
                    {
                        this.dgvUsuarios.ClearSelection();
                        this.dgvUsuarios.Rows[indice].Selected = true;
                        this.dgvUsuarios.CurrentCell = this.dgvUsuarios.Rows[indice].Cells[0];
                        this.dgvUsuarios.FirstDisplayedScrollingRowIndex = indice;
                    }
                }
            }
            catch (Exception ex)
            {
                Debug.WriteLine(ex.ToString());
                MessageBox.Show("No se pudo cargar la lista de usuarios.", "Gestionar usuarios", MessageBoxButtons.OK, MessageBoxIcon.Error);
            }
        }

        private void LimpiarCampos()
        {
            this.errorProvider.Clear();
            this.txtDni.Text = string.Empty;
            this.txtNombre.Text = string.Empty;
            this.txtApellido.Text = string.Empty;
            this.txtTelefono.Text = string.Empty;
            this.txtUsuario.Text = string.Empty;
            this.txtClave.Text = string.Empty;
            this.txtConfirmar.Text = string.Empty;
        }

        private Empleado EmpleadoSeleccionado()
        {
            if (this.dgvUsuarios.CurrentRow == null)
            {
                return null;
            }

            int indice = this.dgvUsuarios.CurrentRow.Index;

            if (indice < 0 || indice >= _empleados.Count)
            {
                return null;
            }

            return _empleados[indice];
        }

        private void dgvUsuarios_SelectionChanged(object sender, EventArgs e)
        {
            Empleado empleado = EmpleadoSeleccionado();

            if (empleado == null)
            {
                return;
            }

            this.errorProvider.Clear();
            this.txtDni.Text = empleado.Dni.ToString();
            this.txtNombre.Text = empleado.Nombre;
            this.txtApellido.Text = empleado.Apellido;
            this.txtTelefono.Text = empleado.Telefono;
            this.txtUsuario.Text = empleado.Usuario == null ? string.Empty : empleado.Usuario.Nombre;
            this.txtClave.Text = string.Empty;
            this.txtConfirmar.Text = string.Empty;
        }

        private void btnLimpiar_Click(object sender, EventArgs e)
        {
            this.dgvUsuarios.ClearSelection();
            this.dgvUsuarios.CurrentCell = null;
            LimpiarCampos();
            this.txtDni.Focus();
        }

        private bool ValidarCamposComunes(out int dni)
        {
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

            if (!ValidacionesDatos.EsNombreUsuarioValido(this.txtUsuario.Text))
            {
                this.errorProvider.SetError(this.txtUsuario, "Ingrese un usuario de hasta 50 caracteres.");
            }

            return this.errorProvider.GetError(this.txtDni).Length == 0
                && this.errorProvider.GetError(this.txtNombre).Length == 0
                && this.errorProvider.GetError(this.txtApellido).Length == 0
                && this.errorProvider.GetError(this.txtTelefono).Length == 0
                && this.errorProvider.GetError(this.txtUsuario).Length == 0;
        }

        private void btnAlta_Click(object sender, EventArgs e)
        {
            if (!ValidarCamposComunes(out int dni))
            {
                return;
            }

            string clave = this.txtClave.Text;
            string confirmar = this.txtConfirmar.Text;

            if (string.IsNullOrWhiteSpace(clave))
            {
                MessageBox.Show("Ingrese una contraseña.", "Gestionar usuarios", MessageBoxButtons.OK, MessageBoxIcon.Warning);
                return;
            }

            if (clave != confirmar)
            {
                MessageBox.Show("Las contraseñas no coinciden.", "Gestionar usuarios", MessageBoxButtons.OK, MessageBoxIcon.Warning);
                return;
            }

            try
            {
                bool registrado = _usuarioServicio.RegistrarUsuario(
                    dni,
                    this.txtNombre.Text,
                    this.txtApellido.Text,
                    this.txtTelefono.Text,
                    this.txtUsuario.Text,
                    clave);

                if (!registrado)
                {
                    MessageBox.Show("No se pudo registrar. Verifique que los datos estén completos y que el DNI y el nombre de usuario no estén registrados en el sistema.", "Gestionar usuarios", MessageBoxButtons.OK, MessageBoxIcon.Warning);
                    return;
                }

                CargarUsuarios();
            }
            catch (Exception ex)
            {
                Debug.WriteLine(ex.ToString());
                if (ConcurrenciaUI.EsConflictoDeUnicidad(ex))
                {
                    MessageBox.Show("Ya existe una persona registrada con ese DNI.", "Gestionar usuarios", MessageBoxButtons.OK, MessageBoxIcon.Warning);
                    return;
                }

                MessageBox.Show("Ocurrió un error al registrar el usuario.", "Gestionar usuarios", MessageBoxButtons.OK, MessageBoxIcon.Error);
            }
        }

        private void btnModificar_Click(object sender, EventArgs e)
        {
            Empleado empleado = EmpleadoSeleccionado();

            if (empleado == null)
            {
                MessageBox.Show("Seleccione un usuario de la lista.", "Gestionar usuarios", MessageBoxButtons.OK, MessageBoxIcon.Information);
                return;
            }

            if (!ValidarCamposComunes(out int dni))
            {
                return;
            }

            string clave = this.txtClave.Text;
            string confirmar = this.txtConfirmar.Text;
            string claveParaActualizar = null;

            if (clave.Length > 0 || confirmar.Length > 0)
            {
                if (string.IsNullOrWhiteSpace(clave))
                {
                    MessageBox.Show("Ingrese una contraseña válida.", "Gestionar usuarios", MessageBoxButtons.OK, MessageBoxIcon.Warning);
                    return;
                }

                if (clave != confirmar)
                {
                    MessageBox.Show("Las contraseñas no coinciden.", "Gestionar usuarios", MessageBoxButtons.OK, MessageBoxIcon.Warning);
                    return;
                }

                claveParaActualizar = clave;
            }

            try
            {
                bool modificado = _usuarioServicio.ModificarUsuario(
                    empleado.Id,
                    dni,
                    this.txtNombre.Text,
                    this.txtApellido.Text,
                    this.txtTelefono.Text,
                    this.txtUsuario.Text,
                    claveParaActualizar);

                if (!modificado)
                {
                    MessageBox.Show("No se pudo guardar. Verifique que los datos estén completos, que el DNI y el nombre de usuario no pertenezcan a otra persona y que el empleado tenga usuario.", "Gestionar usuarios", MessageBoxButtons.OK, MessageBoxIcon.Warning);
                    return;
                }

                CargarUsuarios(empleado.Id);
                MessageBox.Show("El usuario se modificó correctamente.", "Gestionar usuarios", MessageBoxButtons.OK, MessageBoxIcon.Information);
            }
            catch (Exception ex)
            {
                Debug.WriteLine(ex.ToString());
                if (ConcurrenciaUI.EsConflictoDeUnicidad(ex))
                {
                    MessageBox.Show("Ya existe otra persona registrada con ese DNI.", "Gestionar usuarios", MessageBoxButtons.OK, MessageBoxIcon.Warning);
                    return;
                }

                MessageBox.Show("Ocurrió un error al guardar los cambios.", "Gestionar usuarios", MessageBoxButtons.OK, MessageBoxIcon.Error);
            }
        }

        private void btnBaja_Click(object sender, EventArgs e)
        {
            Empleado empleado = EmpleadoSeleccionado();

            if (empleado == null)
            {
                MessageBox.Show("Seleccione un usuario de la lista.", "Gestionar usuarios", MessageBoxButtons.OK, MessageBoxIcon.Information);
                return;
            }

            string pregunta = "¿Dar de baja a " + empleado.Apellido + " " + empleado.Nombre + " (DNI " + empleado.Dni + ")? No podrá volver a iniciar sesión.";

            if (MessageBox.Show(pregunta, "Gestionar usuarios", MessageBoxButtons.YesNo, MessageBoxIcon.Question) != DialogResult.Yes)
            {
                return;
            }

            try
            {
                if (!_usuarioServicio.DarDeBajaUsuario(empleado.Id))
                {
                    MessageBox.Show("No se pudo dar de baja: el empleado no tiene un usuario asociado.", "Gestionar usuarios", MessageBoxButtons.OK, MessageBoxIcon.Warning);
                    return;
                }

                CargarUsuarios();
            }
            catch (Exception ex)
            {
                Debug.WriteLine(ex.ToString());
                MessageBox.Show("Ocurrió un error al dar de baja el usuario.", "Gestionar usuarios", MessageBoxButtons.OK, MessageBoxIcon.Error);
            }
        }
    }
}
