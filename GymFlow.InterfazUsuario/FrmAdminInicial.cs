using GymFlow.Aplicacion;
using System;
using System.Diagnostics;
using System.Drawing;
using System.Windows.Forms;

namespace GymFlow.InterfazUsuario
{
    public class FrmAdminInicial : Form
    {
        private readonly UsuarioServicio _usuarioServicio;

        private Label lblTitulo;
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
        private Button btnCrear;
        private Button btnCancelar;
        private ErrorProvider errorProvider;

        public FrmAdminInicial(UsuarioServicio usuarioServicio)
        {
            _usuarioServicio = usuarioServicio;
            InitializeComponent();
        }

        private void InitializeComponent()
        {
            this.lblTitulo = new Label();
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
            this.btnCrear = new Button();
            this.btnCancelar = new Button();
            this.errorProvider = new ErrorProvider(this);

            this.lblTitulo.Location = new Point(20, 15);
            this.lblTitulo.Size = new Size(340, 35);
            this.lblTitulo.Text = "No existe un administrador activo.\nComplete los datos para crear el administrador inicial:";
            this.lblTitulo.Font = new Font(this.lblTitulo.Font, FontStyle.Bold);

            AgregarCampo(this.lblDni, this.txtDni, "DNI:", 20, 60);
            AgregarCampo(this.lblNombre, this.txtNombre, "Nombre:", 20, 92);
            AgregarCampo(this.lblApellido, this.txtApellido, "Apellido:", 20, 124);
            AgregarCampo(this.lblTelefono, this.txtTelefono, "Teléfono:", 20, 156);
            AgregarCampo(this.lblUsuario, this.txtUsuario, "Usuario:", 20, 188);
            AgregarCampo(this.lblClave, this.txtClave, "Clave:", 20, 220);
            AgregarCampo(this.lblConfirmar, this.txtConfirmar, "Confirmar clave:", 20, 252);

            this.txtClave.UseSystemPasswordChar = true;
            this.txtConfirmar.UseSystemPasswordChar = true;

            this.btnCrear.Location = new Point(140, 295);
            this.btnCrear.Size = new Size(105, 30);
            this.btnCrear.Text = "Crear";
            this.btnCrear.Click += new EventHandler(this.btnCrear_Click);

            this.btnCancelar.Location = new Point(255, 295);
            this.btnCancelar.Size = new Size(105, 30);
            this.btnCancelar.Text = "Cancelar";
            this.btnCancelar.DialogResult = DialogResult.Cancel;

            this.Text = "GymFlow - Administrador inicial";
            this.Size = new Size(400, 380);
            this.FormBorderStyle = FormBorderStyle.FixedDialog;
            this.MaximizeBox = false;
            this.MinimizeBox = false;
            this.StartPosition = FormStartPosition.CenterScreen;
            this.AcceptButton = this.btnCrear;
            this.CancelButton = this.btnCancelar;

            this.Controls.Add(this.lblTitulo);
            this.Controls.Add(this.btnCrear);
            this.Controls.Add(this.btnCancelar);
        }

        private void AgregarCampo(Label etiqueta, TextBox cuadro, string texto, int x, int y)
        {
            etiqueta.Location = new Point(x, y + 3);
            etiqueta.Size = new Size(115, 20);
            etiqueta.Text = texto;

            cuadro.Location = new Point(x + 120, y);
            cuadro.Size = new Size(220, 20);

            this.Controls.Add(etiqueta);
            this.Controls.Add(cuadro);
        }

        private bool ValidarCampos(out int dni)
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

            if (string.IsNullOrWhiteSpace(this.txtClave.Text))
            {
                this.errorProvider.SetError(this.txtClave, "Ingrese una clave.");
            }

            if (this.txtClave.Text != this.txtConfirmar.Text)
            {
                this.errorProvider.SetError(this.txtConfirmar, "Las claves no coinciden.");
            }

            return this.errorProvider.GetError(this.txtDni).Length == 0
                && this.errorProvider.GetError(this.txtNombre).Length == 0
                && this.errorProvider.GetError(this.txtApellido).Length == 0
                && this.errorProvider.GetError(this.txtTelefono).Length == 0
                && this.errorProvider.GetError(this.txtUsuario).Length == 0
                && this.errorProvider.GetError(this.txtClave).Length == 0
                && this.errorProvider.GetError(this.txtConfirmar).Length == 0;
        }

        private void btnCrear_Click(object sender, EventArgs e)
        {
            if (!ValidarCampos(out int dni))
            {
                return;
            }

            this.btnCrear.Enabled = false;

            try
            {
                bool creado = _usuarioServicio.CrearAdministradorInicial(
                    dni,
                    this.txtNombre.Text,
                    this.txtApellido.Text,
                    this.txtTelefono.Text,
                    this.txtUsuario.Text,
                    this.txtClave.Text);

                if (!creado)
                {
                    MessageBox.Show(
                        "No se pudo crear el administrador inicial. Verifique que los datos no estén duplicados y que no exista ya un administrador activo.",
                        "Administrador inicial",
                        MessageBoxButtons.OK,
                        MessageBoxIcon.Warning);
                    return;
                }

                MessageBox.Show(
                    "Administrador inicial creado correctamente. Ahora puede iniciar sesión.",
                    "Administrador inicial",
                    MessageBoxButtons.OK,
                    MessageBoxIcon.Information);
                this.DialogResult = DialogResult.OK;
            }
            catch (Exception ex)
            {
                Debug.WriteLine(ex.ToString());
                if (ConcurrenciaUI.EsConflictoDeUnicidad(ex))
                {
                    MessageBox.Show("Ya existe una persona o usuario registrado con esos datos.", "Administrador inicial", MessageBoxButtons.OK, MessageBoxIcon.Warning);
                    return;
                }

                MessageBox.Show("Ocurrió un error al crear el administrador inicial.", "Administrador inicial", MessageBoxButtons.OK, MessageBoxIcon.Error);
            }
            finally
            {
                this.btnCrear.Enabled = true;
            }
        }
    }
}
