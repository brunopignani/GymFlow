using GymFlow.Aplicacion;
using GymFlow.Dominio;
using Microsoft.Extensions.DependencyInjection;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Drawing;
using System.Windows.Forms;

namespace GymFlow.InterfazUsuario
{
    public class FrmUsuarios : Form
    {
        private readonly UsuarioServicio _usuarioServicio;
        private readonly IServiceProvider _serviceProvider;

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
        private Button btnNuevo;
        private Button btnModificar;
        private Button btnBaja;

        public FrmUsuarios(UsuarioServicio usuarioServicio, IServiceProvider serviceProvider)
        {
            _usuarioServicio = usuarioServicio;
            _serviceProvider = serviceProvider;
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
            this.btnNuevo = new Button();
            this.btnModificar = new Button();
            this.btnBaja = new Button();

            this.dgvUsuarios.Location = new Point(20, 20);
            this.dgvUsuarios.Size = new Size(640, 220);
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

            AgregarCampo(this.lblDni, this.txtDni, "DNI:", 20, 260);
            AgregarCampo(this.lblNombre, this.txtNombre, "Nombre:", 20, 295);
            AgregarCampo(this.lblApellido, this.txtApellido, "Apellido:", 340, 260);
            AgregarCampo(this.lblTelefono, this.txtTelefono, "Teléfono:", 340, 295);
            AgregarCampo(this.lblUsuario, this.txtUsuario, "Usuario:", 20, 330);

            this.btnNuevo.Location = new Point(20, 375);
            this.btnNuevo.Size = new Size(140, 30);
            this.btnNuevo.Text = "Nuevo usuario";
            this.btnNuevo.Click += new EventHandler(this.btnNuevo_Click);

            this.btnModificar.Location = new Point(170, 375);
            this.btnModificar.Size = new Size(140, 30);
            this.btnModificar.Text = "Guardar cambios";
            this.btnModificar.Click += new EventHandler(this.btnModificar_Click);

            this.btnBaja.Location = new Point(320, 375);
            this.btnBaja.Size = new Size(140, 30);
            this.btnBaja.Text = "Dar de baja";
            this.btnBaja.Click += new EventHandler(this.btnBaja_Click);

            this.Text = "Gestionar usuarios";
            this.Size = new Size(700, 470);
            this.FormBorderStyle = FormBorderStyle.FixedDialog;
            this.MaximizeBox = false;
            this.StartPosition = FormStartPosition.CenterParent;

            this.Controls.Add(this.dgvUsuarios);
            this.Controls.Add(this.btnNuevo);
            this.Controls.Add(this.btnModificar);
            this.Controls.Add(this.btnBaja);
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

        private void CargarUsuarios()
        {
            this.dgvUsuarios.Rows.Clear();
            LimpiarCampos();

            try
            {
                _empleados = _usuarioServicio.GetEmpleados();

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
            }
            catch (Exception ex)
            {
                Debug.WriteLine(ex.ToString());
                MessageBox.Show("No se pudo cargar la lista de usuarios.", "Gestionar usuarios", MessageBoxButtons.OK, MessageBoxIcon.Error);
            }
        }

        private void LimpiarCampos()
        {
            this.txtDni.Text = string.Empty;
            this.txtNombre.Text = string.Empty;
            this.txtApellido.Text = string.Empty;
            this.txtTelefono.Text = string.Empty;
            this.txtUsuario.Text = string.Empty;
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

            this.txtDni.Text = empleado.Dni.ToString();
            this.txtNombre.Text = empleado.Nombre;
            this.txtApellido.Text = empleado.Apellido;
            this.txtTelefono.Text = empleado.Telefono;
            this.txtUsuario.Text = empleado.Usuario == null ? string.Empty : empleado.Usuario.Nombre;
        }

        private void btnNuevo_Click(object sender, EventArgs e)
        {
            using (FrmRegistro formulario = _serviceProvider.GetRequiredService<FrmRegistro>())
            {
                formulario.ShowDialog(this);
            }

            CargarUsuarios();
        }

        private void btnModificar_Click(object sender, EventArgs e)
        {
            Empleado empleado = EmpleadoSeleccionado();

            if (empleado == null)
            {
                MessageBox.Show("Seleccione un usuario de la lista.", "Gestionar usuarios", MessageBoxButtons.OK, MessageBoxIcon.Information);
                return;
            }

            int dni;

            if (!int.TryParse(this.txtDni.Text, out dni))
            {
                MessageBox.Show("Ingrese un DNI válido.", "Gestionar usuarios", MessageBoxButtons.OK, MessageBoxIcon.Warning);
                return;
            }

            try
            {
                bool modificado = _usuarioServicio.ModificarUsuario(empleado.Id, dni, this.txtNombre.Text, this.txtApellido.Text, this.txtTelefono.Text, this.txtUsuario.Text);

                if (!modificado)
                {
                    MessageBox.Show("No se pudo guardar. Verifique que los datos estén completos, que el DNI y el nombre de usuario no estén repetidos y que el empleado tenga usuario.", "Gestionar usuarios", MessageBoxButtons.OK, MessageBoxIcon.Warning);
                    return;
                }

                CargarUsuarios();
            }
            catch (Exception ex)
            {
                Debug.WriteLine(ex.ToString());
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

            string pregunta = "¿Dar de baja a " + empleado.Apellido + " " + empleado.Nombre + "? No podrá volver a iniciar sesión.";

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
