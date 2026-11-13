using GymFlow.Aplicacion;
using System;
using System.Diagnostics;
using System.Drawing;
using System.Windows.Forms;

namespace GymFlow.InterfazUsuario
{
    public class FrmRegistro : Form
    {
        private readonly UsuarioServicio _usuarioServicio;

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
        private Button btnRegistrar;
        private ErrorProvider errorProvider;

        public FrmRegistro(UsuarioServicio usuarioServicio)
        {
            _usuarioServicio = usuarioServicio;

            InitializeComponent();
        }

        private void InitializeComponent()
        {
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
            this.btnRegistrar = new Button();
            this.errorProvider = new ErrorProvider(this);

            AgregarFila(this.lblDni, this.txtDni, "DNI:", 20);
            AgregarFila(this.lblNombre, this.txtNombre, "Nombre:", 55);
            AgregarFila(this.lblApellido, this.txtApellido, "Apellido:", 90);
            AgregarFila(this.lblTelefono, this.txtTelefono, "Teléfono:", 125);
            AgregarFila(this.lblUsuario, this.txtUsuario, "Usuario:", 175);
            AgregarFila(this.lblClave, this.txtClave, "Clave:", 210);
            AgregarFila(this.lblConfirmar, this.txtConfirmar, "Repetir clave:", 245);
            this.txtClave.UseSystemPasswordChar = true;
            this.txtConfirmar.UseSystemPasswordChar = true;

            this.btnRegistrar.Location = new Point(140, 295);
            this.btnRegistrar.Size = new Size(120, 30);
            this.btnRegistrar.Text = "Registrar";
            this.btnRegistrar.Click += new EventHandler(this.btnRegistrar_Click);

            this.Text = "Registro de usuario";
            this.Size = new Size(400, 395);
            this.FormBorderStyle = FormBorderStyle.FixedDialog;
            this.MaximizeBox = false;
            this.StartPosition = FormStartPosition.CenterParent;

            this.Controls.Add(this.btnRegistrar);
        }

        private void AgregarFila(Label etiqueta, TextBox cuadro, string texto, int y)
        {
            etiqueta.Location = new Point(20, y + 3);
            etiqueta.Size = new Size(110, 20);
            etiqueta.Text = texto;

            cuadro.Location = new Point(140, y);
            cuadro.Size = new Size(220, 20);

            this.Controls.Add(etiqueta);
            this.Controls.Add(cuadro);
        }

        private void btnRegistrar_Click(object sender, EventArgs e)
        {
            int dni;
            this.errorProvider.Clear();

            if (!ValidacionesDatos.TryParseDni(this.txtDni.Text, out dni))
            {
                this.errorProvider.SetError(this.txtDni, "Ingrese solo 7 u 8 dígitos válidos.");
            }

            if (!ValidacionesDatos.EsNombreValido(this.txtNombre.Text))
            {
                this.errorProvider.SetError(this.txtNombre, "Ingrese un nombre obligatorio de hasta 100 caracteres.");
            }

            if (!ValidacionesDatos.EsNombreValido(this.txtApellido.Text))
            {
                this.errorProvider.SetError(this.txtApellido, "Ingrese un apellido obligatorio de hasta 100 caracteres.");
            }

            if (!ValidacionesDatos.EsTelefonoValido(this.txtTelefono.Text))
            {
                this.errorProvider.SetError(this.txtTelefono, "Ingrese un teléfono obligatorio y válido.");
            }

            if (!ValidacionesDatos.EsNombreUsuarioValido(this.txtUsuario.Text))
            {
                this.errorProvider.SetError(this.txtUsuario, "Ingrese un usuario obligatorio de hasta 50 caracteres.");
            }

            if (this.errorProvider.GetError(this.txtDni).Length > 0
                || this.errorProvider.GetError(this.txtNombre).Length > 0
                || this.errorProvider.GetError(this.txtApellido).Length > 0
                || this.errorProvider.GetError(this.txtTelefono).Length > 0
                || this.errorProvider.GetError(this.txtUsuario).Length > 0)
            {
                return;
            }

            if (this.txtClave.Text != this.txtConfirmar.Text)
            {
                MessageBox.Show("Las claves no coinciden.", "Registro", MessageBoxButtons.OK, MessageBoxIcon.Warning);
                return;
            }

            this.btnRegistrar.Enabled = false;

            try
            {
                bool registrado = _usuarioServicio.RegistrarUsuario(dni, this.txtNombre.Text, this.txtApellido.Text, this.txtTelefono.Text, this.txtUsuario.Text, this.txtClave.Text);

                if (!registrado)
                {
                    MessageBox.Show("No se pudo registrar. Complete todos los datos obligatorios y verifique que el DNI y el nombre de usuario no estén repetidos.", "Registro", MessageBoxButtons.OK, MessageBoxIcon.Warning);
                    return;
                }

                MessageBox.Show("Usuario registrado correctamente.", "Registro", MessageBoxButtons.OK, MessageBoxIcon.Information);
                this.DialogResult = DialogResult.OK;
            }
            catch (Exception ex)
            {
                Debug.WriteLine(ex.ToString());
                string detalle = ex.InnerException != null ? ex.Message + Environment.NewLine + ex.InnerException.Message : ex.Message;
                MessageBox.Show("Ocurrió un error al registrar el usuario:" + Environment.NewLine + detalle, "Registro", MessageBoxButtons.OK, MessageBoxIcon.Error);
            }
            finally
            {
                this.btnRegistrar.Enabled = true;
            }
        }
    }
}
