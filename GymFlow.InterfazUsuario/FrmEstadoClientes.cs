using GymFlow.Aplicacion;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Drawing;
using System.Windows.Forms;

namespace GymFlow.InterfazUsuario
{
    public class FrmEstadoClientes : Form
    {
        private readonly ClienteServicio _clienteServicio;

        private Label lblFiltro;
        private ComboBox cmbFiltro;
        private DataGridView dgvClientes;
        private Label lblResumen;

        public FrmEstadoClientes(ClienteServicio clienteServicio)
        {
            _clienteServicio = clienteServicio;

            InitializeComponent();
            CargarClientes();
        }

        private void InitializeComponent()
        {
            this.lblFiltro = new Label();
            this.cmbFiltro = new ComboBox();
            this.dgvClientes = new DataGridView();
            this.lblResumen = new Label();

            this.lblFiltro.Location = new Point(20, 23);
            this.lblFiltro.Size = new Size(50, 20);
            this.lblFiltro.Text = "Mostrar:";

            this.cmbFiltro.Location = new Point(80, 20);
            this.cmbFiltro.Size = new Size(200, 21);
            this.cmbFiltro.DropDownStyle = ComboBoxStyle.DropDownList;
            this.cmbFiltro.Items.Add("Todos");
            this.cmbFiltro.Items.Add("Activos (cuota vigente)");
            this.cmbFiltro.Items.Add("Vencidos (cuota vencida)");
            this.cmbFiltro.SelectedIndex = 0;
            this.cmbFiltro.SelectedIndexChanged += new EventHandler(this.cmbFiltro_SelectedIndexChanged);

            this.dgvClientes.Location = new Point(20, 60);
            this.dgvClientes.Size = new Size(620, 300);
            this.dgvClientes.ReadOnly = true;
            this.dgvClientes.AllowUserToAddRows = false;
            this.dgvClientes.RowHeadersVisible = false;
            this.dgvClientes.SelectionMode = DataGridViewSelectionMode.FullRowSelect;
            this.dgvClientes.AutoSizeColumnsMode = DataGridViewAutoSizeColumnsMode.Fill;
            this.dgvClientes.Columns.Add("colDni", "DNI");
            this.dgvClientes.Columns.Add("colApellido", "Apellido");
            this.dgvClientes.Columns.Add("colNombre", "Nombre");
            this.dgvClientes.Columns.Add("colVencimiento", "Vencimiento");
            this.dgvClientes.Columns.Add("colEstado", "Estado");

            this.lblResumen.Location = new Point(20, 370);
            this.lblResumen.Size = new Size(620, 20);
            this.lblResumen.Text = string.Empty;

            this.Text = "Estado de clientes";
            this.Size = new Size(680, 450);
            this.FormBorderStyle = FormBorderStyle.FixedDialog;
            this.MaximizeBox = false;
            this.StartPosition = FormStartPosition.CenterParent;

            this.Controls.Add(this.lblFiltro);
            this.Controls.Add(this.cmbFiltro);
            this.Controls.Add(this.dgvClientes);
            this.Controls.Add(this.lblResumen);
        }

        private void CargarClientes()
        {
            this.dgvClientes.Rows.Clear();
            this.lblResumen.Text = string.Empty;

            try
            {
                List<EstadoCliente> estados = _clienteServicio.GetEstadoClientes();
                int filtro = this.cmbFiltro.SelectedIndex;
                int mostrados = 0;

                foreach (EstadoCliente estado in estados)
                {
                    bool mostrar = filtro == 0
                        || (filtro == 1 && estado.Vigente)
                        || (filtro == 2 && estado.TieneCuota && !estado.Vigente);

                    if (!mostrar)
                    {
                        continue;
                    }

                    this.dgvClientes.Rows.Add(
                        estado.Cliente.Dni,
                        estado.Cliente.Apellido,
                        estado.Cliente.Nombre,
                        estado.TieneCuota ? (estado.CuotaCliente.FechaVencimiento.HasValue ? estado.CuotaCliente.FechaVencimiento.Value.ToString("dd/MM/yyyy") : "Pendiente") : "-",
                        DescribirEstado(estado));

                    mostrados = mostrados + 1;
                }

                this.lblResumen.Text = mostrados == 0 ? "No hay clientes para mostrar." : "Clientes mostrados: " + mostrados;
            }
            catch (Exception ex)
            {
                Debug.WriteLine(ex.ToString());
                MessageBox.Show("No se pudo consultar el estado de los clientes.", "Estado de clientes", MessageBoxButtons.OK, MessageBoxIcon.Error);
            }
        }

        private string DescribirEstado(EstadoCliente estado)
        {
            if (!estado.TieneCuota)
            {
                return "Sin cuota";
            }

            return estado.Vigente ? "Activo" : "Vencido";
        }

        private void cmbFiltro_SelectedIndexChanged(object sender, EventArgs e)
        {
            CargarClientes();
        }
    }
}
