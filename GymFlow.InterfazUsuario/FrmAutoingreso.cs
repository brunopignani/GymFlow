using GymFlow.Aplicacion;
using System;
using System.Drawing;
using System.Windows.Forms;

namespace GymFlow.InterfazUsuario
{
    public class FrmAutoingreso : Form
    {
        private readonly ClienteServicio _clienteServicio;

        private Label lblDniTitulo;
        private TextBox txtDni;
        private Button btnValidar;
        private Label lblResultado;

        private Label lblNombre;
        private Label lblDniInfo;
        private Label lblEstadoCuota;
        private Label lblFechaVencimiento;
        private Label lblActividades;

        public FrmAutoingreso(ClienteServicio clienteServicio)
        {
            _clienteServicio = clienteServicio;
            InitializeComponent();
        }

        private void InitializeComponent()
        {
            this.lblDniTitulo = new Label();
            this.txtDni = new TextBox();
            this.btnValidar = new Button();
            this.lblResultado = new Label();

            this.lblNombre = new Label();
            this.lblDniInfo = new Label();
            this.lblEstadoCuota = new Label();
            this.lblFechaVencimiento = new Label();
            this.lblActividades = new Label();

            // Entrada DNI
            this.lblDniTitulo.Location = new Point(20, 23);
            this.lblDniTitulo.Size = new Size(40, 20);
            this.lblDniTitulo.Text = "DNI:";

            this.txtDni.Location = new Point(65, 20);
            this.txtDni.Size = new Size(160, 20);

            this.btnValidar.Location = new Point(235, 18);
            this.btnValidar.Size = new Size(90, 25);
            this.btnValidar.Text = "Validar";
            this.btnValidar.Click += new EventHandler(this.btnValidar_Click);

            // Banner destacado de resultado
            this.lblResultado.Location = new Point(20, 58);
            this.lblResultado.Size = new Size(420, 32);
            this.lblResultado.Text = string.Empty;
            this.lblResultado.Font = new Font(this.Font.FontFamily, 14, FontStyle.Bold);
            this.lblResultado.TextAlign = ContentAlignment.MiddleLeft;

            // Datos del socio
            this.lblNombre.Location = new Point(20, 100);
            this.lblNombre.Size = new Size(420, 20);
            this.lblNombre.Text = string.Empty;

            this.lblDniInfo.Location = new Point(20, 125);
            this.lblDniInfo.Size = new Size(420, 20);
            this.lblDniInfo.Text = string.Empty;

            this.lblEstadoCuota.Location = new Point(20, 150);
            this.lblEstadoCuota.Size = new Size(420, 20);
            this.lblEstadoCuota.Text = string.Empty;

            this.lblFechaVencimiento.Location = new Point(20, 175);
            this.lblFechaVencimiento.Size = new Size(420, 20);
            this.lblFechaVencimiento.Text = string.Empty;

            this.lblActividades.Location = new Point(20, 200);
            this.lblActividades.Size = new Size(420, 45);
            this.lblActividades.Text = string.Empty;

            // Form properties
            this.Text = "Autoingreso";
            this.Size = new Size(480, 290);
            this.StartPosition = FormStartPosition.CenterParent;
            this.FormBorderStyle = FormBorderStyle.FixedDialog;
            this.MaximizeBox = false;
            this.MinimizeBox = false;
            this.AcceptButton = this.btnValidar;

            this.Controls.Add(this.lblDniTitulo);
            this.Controls.Add(this.txtDni);
            this.Controls.Add(this.btnValidar);
            this.Controls.Add(this.lblResultado);
            this.Controls.Add(this.lblNombre);
            this.Controls.Add(this.lblDniInfo);
            this.Controls.Add(this.lblEstadoCuota);
            this.Controls.Add(this.lblFechaVencimiento);
            this.Controls.Add(this.lblActividades);
        }

        private void btnValidar_Click(object sender, EventArgs e)
        {
            int dni;

            if (!ValidacionesDatos.TryParseDni(this.txtDni.Text, out dni))
            {
                MessageBox.Show("Ingrese un DNI válido.", "Autoingreso", MessageBoxButtons.OK, MessageBoxIcon.Warning);
                return;
            }

            ResultadoAutoingreso resultado = _clienteServicio.ValidarAutoingreso(dni);

            if (!resultado.SocioEncontrado)
            {
                this.lblResultado.Text = "Socio no encontrado";
                this.lblResultado.ForeColor = Color.Red;
                LimpiarDetalle();
                this.txtDni.SelectAll();
                this.txtDni.Focus();
                return;
            }

            // Datos del cliente
            this.lblNombre.Text = "Nombre y apellido: " + resultado.NombreCompleto;
            this.lblDniInfo.Text = "DNI: " + resultado.Dni;
            this.lblEstadoCuota.Text = "Estado de la cuota: " + FormatearEstado(resultado.EstadoCuota);
            this.lblFechaVencimiento.Text = "Fecha de vencimiento: " + (resultado.FechaVencimiento.HasValue
                ? resultado.FechaVencimiento.Value.ToString("dd/MM/yyyy")
                : "No registra");
            this.lblActividades.Text = "Actividades: " + (resultado.Actividades != null && resultado.Actividades.Count > 0
                ? string.Join(", ", resultado.Actividades)
                : "No registra actividades");

            // Resultado claro y destacado
            if (resultado.AccesoPermitido)
            {
                this.lblResultado.Text = "ACCESO PERMITIDO";
                this.lblResultado.ForeColor = Color.Green;
            }
            else
            {
                this.lblResultado.Text = "ACCESO DENEGADO";
                this.lblResultado.ForeColor = Color.Red;
            }

            this.txtDni.SelectAll();
            this.txtDni.Focus();
        }

        private static string FormatearEstado(string estado)
        {
            if (estado == EstadosCliente.Activo)
            {
                return "Activa";
            }

            if (estado == EstadosCliente.Pendiente)
            {
                return "Pendiente";
            }

            if (estado == EstadosCliente.Vencido)
            {
                return "Vencida";
            }

            return "Sin cuota";
        }

        private void LimpiarDetalle()
        {
            this.lblNombre.Text = string.Empty;
            this.lblDniInfo.Text = string.Empty;
            this.lblEstadoCuota.Text = string.Empty;
            this.lblFechaVencimiento.Text = string.Empty;
            this.lblActividades.Text = string.Empty;
        }
    }
}
