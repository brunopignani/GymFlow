using GymFlow.Aplicacion;
using System;
using System.Drawing;
using System.Windows.Forms;

namespace GymFlow.InterfazUsuario
{
    public class FrmAutoingreso : Form
    {
        private readonly ClienteServicio _clienteServicio;

        private TextBox txtDni;
        private Button btnValidar;
        private Label lblResultado;

        public FrmAutoingreso(ClienteServicio clienteServicio)
        {
            _clienteServicio = clienteServicio;
            InitializeComponent();
        }

        private void InitializeComponent()
        {
            this.txtDni = new TextBox();
            this.btnValidar = new Button();
            this.lblResultado = new Label();

            this.txtDni.Location = new Point(20, 20);
            this.txtDni.Size = new Size(200, 20);

            this.btnValidar.Location = new Point(20, 60);
            this.btnValidar.Size = new Size(100, 30);
            this.btnValidar.Text = "Validar";
            this.btnValidar.Click += new EventHandler(this.btnValidar_Click);

            this.lblResultado.Location = new Point(20, 110);
            this.lblResultado.Size = new Size(200, 30);
            this.lblResultado.Text = string.Empty;
            this.lblResultado.Font = new Font(this.lblResultado.Font, FontStyle.Bold);

            this.Text = "Autoingreso";
            this.Size = new Size(280, 200);

            this.Controls.Add(this.txtDni);
            this.Controls.Add(this.btnValidar);
            this.Controls.Add(this.lblResultado);
        }

        private void btnValidar_Click(object sender, EventArgs e)
        {
            int dni;

            if (!int.TryParse(this.txtDni.Text, out dni))
            {
                MessageBox.Show("Ingrese un DNI válido.", "Autoingreso", MessageBoxButtons.OK, MessageBoxIcon.Warning);
                return;
            }

            bool resultado = _clienteServicio.ValidarAutoingreso(dni);

            if (resultado)
            {
                this.lblResultado.Text = "ACCESO PERMITIDO";
                this.lblResultado.ForeColor = Color.Green;
            }
            else
            {
                this.lblResultado.Text = "ACCESO DENEGADO";
                this.lblResultado.ForeColor = Color.Red;
            }
        }
    }
}
