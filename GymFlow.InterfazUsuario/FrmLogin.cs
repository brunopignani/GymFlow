using GymFlow.Aplicacion;
using GymFlow.Dominio;
using Microsoft.Extensions.DependencyInjection;
using System;
using System.Diagnostics;
using System.Drawing;
using System.Windows.Forms;

namespace GymFlow.InterfazUsuario
{
    public class FrmLogin : Form
    {
        private readonly UsuarioServicio _usuarioServicio;
        private readonly IServiceProvider _serviceProvider;

        private Label lblUsuario;
        private TextBox txtUsuario;
        private Label lblClave;
        private TextBox txtClave;
        private Button btnIngresar;
        private Button btnRegistrarse;

        public FrmLogin(UsuarioServicio usuarioServicio, IServiceProvider serviceProvider)
        {
            _usuarioServicio = usuarioServicio;
            _serviceProvider = serviceProvider;

            InitializeComponent();
        }

        private void InitializeComponent()
        {
            this.lblUsuario = new Label();
            this.txtUsuario = new TextBox();
            this.lblClave = new Label();
            this.txtClave = new TextBox();
            this.btnIngresar = new Button();
            this.btnRegistrarse = new Button();

            this.lblUsuario.Location = new Point(20, 23);
            this.lblUsuario.Size = new Size(70, 20);
            this.lblUsuario.Text = "Usuario:";

            this.txtUsuario.Location = new Point(100, 20);
            this.txtUsuario.Size = new Size(180, 20);

            this.lblClave.Location = new Point(20, 58);
            this.lblClave.Size = new Size(70, 20);
            this.lblClave.Text = "Clave:";

            this.txtClave.Location = new Point(100, 55);
            this.txtClave.Size = new Size(180, 20);
            this.txtClave.UseSystemPasswordChar = true;

            this.btnIngresar.Location = new Point(100, 95);
            this.btnIngresar.Size = new Size(85, 30);
            this.btnIngresar.Text = "Ingresar";
            this.btnIngresar.Click += new EventHandler(this.btnIngresar_Click);

            this.btnRegistrarse.Location = new Point(195, 95);
            this.btnRegistrarse.Size = new Size(85, 30);
            this.btnRegistrarse.Text = "Registrarse";
            this.btnRegistrarse.Click += new EventHandler(this.btnRegistrarse_Click);

            this.Text = "GymFlow - Iniciar sesión";
            this.Size = new Size(320, 200);
            this.FormBorderStyle = FormBorderStyle.FixedDialog;
            this.MaximizeBox = false;
            this.StartPosition = FormStartPosition.CenterScreen;
            this.AcceptButton = this.btnIngresar;

            this.Controls.Add(this.lblUsuario);
            this.Controls.Add(this.txtUsuario);
            this.Controls.Add(this.lblClave);
            this.Controls.Add(this.txtClave);
            this.Controls.Add(this.btnIngresar);
            this.Controls.Add(this.btnRegistrarse);
        }

        private void btnIngresar_Click(object sender, EventArgs e)
        {
            this.btnIngresar.Enabled = false;

            try
            {
                Empleado empleado = _usuarioServicio.Login(this.txtUsuario.Text, this.txtClave.Text);

                if (empleado == null)
                {
                    MessageBox.Show("Usuario o clave incorrectos, o el usuario fue dado de baja.", "Iniciar sesión", MessageBoxButtons.OK, MessageBoxIcon.Warning);
                    this.txtClave.Text = string.Empty;
                    return;
                }

                SesionActual.Iniciar(empleado);
                this.DialogResult = DialogResult.OK;
            }
            catch (Exception ex)
            {
                Debug.WriteLine(ex.ToString());
                string detalle = ex.InnerException != null ? ex.Message + Environment.NewLine + ex.InnerException.Message : ex.Message;
                MessageBox.Show("No se pudo iniciar sesión:" + Environment.NewLine + detalle, "Iniciar sesión", MessageBoxButtons.OK, MessageBoxIcon.Error);
            }
            finally
            {
                this.btnIngresar.Enabled = true;
            }
        }

        private void btnRegistrarse_Click(object sender, EventArgs e)
        {
            using (FrmRegistro formulario = _serviceProvider.GetRequiredService<FrmRegistro>())
            {
                formulario.ShowDialog(this);
            }
        }
    }
}
