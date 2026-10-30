using GymFlow.Aplicacion;
using System;
using System.Diagnostics;
using System.Drawing;
using System.Windows.Forms;

namespace GymFlow.InterfazUsuario
{
    public class FrmCambiarClave : Form
    {
        private readonly UsuarioServicio _usuarioServicio;

        private Label lblActual;
        private TextBox txtActual;
        private Label lblNueva;
        private TextBox txtNueva;
        private Label lblConfirmar;
        private TextBox txtConfirmar;
        private Button btnCambiar;

        public FrmCambiarClave(UsuarioServicio usuarioServicio)
        {
            _usuarioServicio = usuarioServicio;

            InitializeComponent();
        }

        private void InitializeComponent()
        {
            this.lblActual = new Label();
            this.txtActual = new TextBox();
            this.lblNueva = new Label();
            this.txtNueva = new TextBox();
            this.lblConfirmar = new Label();
            this.txtConfirmar = new TextBox();
            this.btnCambiar = new Button();

            this.lblActual.Location = new Point(20, 23);
            this.lblActual.Size = new Size(120, 20);
            this.lblActual.Text = "Clave actual:";
            this.txtActual.Location = new Point(150, 20);
            this.txtActual.Size = new Size(180, 20);
            this.txtActual.UseSystemPasswordChar = true;

            this.lblNueva.Location = new Point(20, 58);
            this.lblNueva.Size = new Size(120, 20);
            this.lblNueva.Text = "Clave nueva:";
            this.txtNueva.Location = new Point(150, 55);
            this.txtNueva.Size = new Size(180, 20);
            this.txtNueva.UseSystemPasswordChar = true;

            this.lblConfirmar.Location = new Point(20, 93);
            this.lblConfirmar.Size = new Size(120, 20);
            this.lblConfirmar.Text = "Repetir clave nueva:";
            this.txtConfirmar.Location = new Point(150, 90);
            this.txtConfirmar.Size = new Size(180, 20);
            this.txtConfirmar.UseSystemPasswordChar = true;

            this.btnCambiar.Location = new Point(150, 130);
            this.btnCambiar.Size = new Size(120, 30);
            this.btnCambiar.Text = "Cambiar clave";
            this.btnCambiar.Click += new EventHandler(this.btnCambiar_Click);

            this.Text = "Cambiar contraseña";
            this.Size = new Size(380, 230);
            this.FormBorderStyle = FormBorderStyle.FixedDialog;
            this.MaximizeBox = false;
            this.StartPosition = FormStartPosition.CenterParent;

            this.Controls.Add(this.lblActual);
            this.Controls.Add(this.txtActual);
            this.Controls.Add(this.lblNueva);
            this.Controls.Add(this.txtNueva);
            this.Controls.Add(this.lblConfirmar);
            this.Controls.Add(this.txtConfirmar);
            this.Controls.Add(this.btnCambiar);
        }

        private void btnCambiar_Click(object sender, EventArgs e)
        {
            if (this.txtNueva.Text != this.txtConfirmar.Text)
            {
                MessageBox.Show("La clave nueva y su repetición no coinciden.", "Cambiar contraseña", MessageBoxButtons.OK, MessageBoxIcon.Warning);
                return;
            }

            this.btnCambiar.Enabled = false;

            try
            {
                bool cambiada = _usuarioServicio.CambiarClave(SesionActual.UsuarioId, this.txtActual.Text, this.txtNueva.Text);

                if (!cambiada)
                {
                    MessageBox.Show("No se pudo cambiar la clave. Verifique la clave actual y que la nueva no esté vacía ni sea igual a la actual.", "Cambiar contraseña", MessageBoxButtons.OK, MessageBoxIcon.Warning);
                    return;
                }

                MessageBox.Show("La clave se cambió correctamente.", "Cambiar contraseña", MessageBoxButtons.OK, MessageBoxIcon.Information);
                this.DialogResult = DialogResult.OK;
            }
            catch (Exception ex)
            {
                Debug.WriteLine(ex.ToString());
                MessageBox.Show("Ocurrió un error al cambiar la clave. Intente nuevamente.", "Cambiar contraseña", MessageBoxButtons.OK, MessageBoxIcon.Error);
            }
            finally
            {
                this.btnCambiar.Enabled = true;
            }
        }
    }
}
