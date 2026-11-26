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
        private sealed class ItemSeleccion
        {
            public int Id { get; set; }
            public string Texto { get; set; }
            public override string ToString() => Texto;
        }

        private readonly UsuarioServicio _usuarioServicio;

        private List<Empleado> _todosLosEmpleados = new List<Empleado>();
        private List<Empleado> _empleadosFiltrados = new List<Empleado>();
        private List<Grupo> _gruposDisponibles = new List<Grupo>();
        private List<Accion> _accionesDisponibles = new List<Accion>();

        private Label lblFiltroTexto;
        private TextBox txtFiltroTexto;
        private Label lblFiltroEstado;
        private ComboBox cboFiltroEstado;
        private Label lblFiltroGrupo;
        private ComboBox cboFiltroGrupo;

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
        private Button btnReactivar;
        private Button btnResetearClave;
        private Button btnLimpiar;

        private Label lblGrupos;
        private CheckedListBox clbGrupos;
        private Label lblAccionesDirectas;
        private CheckedListBox clbAccionesDirectas;
        private Button btnGuardarPermisos;
        private ErrorProvider errorProvider;

        public FrmUsuarios(UsuarioServicio usuarioServicio)
        {
            _usuarioServicio = usuarioServicio;

            InitializeComponent();
            CargarCatalogosSeguridad();
            CargarUsuarios();
        }

        private void InitializeComponent()
        {
            this.lblFiltroTexto = new Label();
            this.txtFiltroTexto = new TextBox();
            this.lblFiltroEstado = new Label();
            this.cboFiltroEstado = new ComboBox();
            this.lblFiltroGrupo = new Label();
            this.cboFiltroGrupo = new ComboBox();

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
            this.btnReactivar = new Button();
            this.btnResetearClave = new Button();
            this.btnLimpiar = new Button();
            this.lblGrupos = new Label();
            this.clbGrupos = new CheckedListBox();
            this.lblAccionesDirectas = new Label();
            this.clbAccionesDirectas = new CheckedListBox();
            this.btnGuardarPermisos = new Button();
            this.errorProvider = new ErrorProvider(this);

            this.lblFiltroTexto.Location = new Point(20, 18);
            this.lblFiltroTexto.Size = new Size(105, 20);
            this.lblFiltroTexto.Text = "Nombre / Usuario:";

            this.txtFiltroTexto.Location = new Point(130, 15);
            this.txtFiltroTexto.Size = new Size(185, 20);
            this.txtFiltroTexto.TextChanged += new EventHandler(this.Filtros_Changed);

            this.lblFiltroEstado.Location = new Point(330, 18);
            this.lblFiltroEstado.Size = new Size(48, 20);
            this.lblFiltroEstado.Text = "Estado:";

            this.cboFiltroEstado.Location = new Point(382, 15);
            this.cboFiltroEstado.Size = new Size(105, 21);
            this.cboFiltroEstado.DropDownStyle = ComboBoxStyle.DropDownList;
            this.cboFiltroEstado.Items.AddRange(new object[] { "Todos", EstadosUsuario.Activo, EstadosUsuario.Inactivo });
            this.cboFiltroEstado.SelectedIndex = 0;
            this.cboFiltroEstado.SelectedIndexChanged += new EventHandler(this.Filtros_Changed);

            this.lblFiltroGrupo.Location = new Point(505, 18);
            this.lblFiltroGrupo.Size = new Size(45, 20);
            this.lblFiltroGrupo.Text = "Grupo:";

            this.cboFiltroGrupo.Location = new Point(555, 15);
            this.cboFiltroGrupo.Size = new Size(185, 21);
            this.cboFiltroGrupo.DropDownStyle = ComboBoxStyle.DropDownList;
            this.cboFiltroGrupo.SelectedIndexChanged += new EventHandler(this.Filtros_Changed);

            this.dgvUsuarios.Location = new Point(20, 48);
            this.dgvUsuarios.Size = new Size(720, 175);
            this.dgvUsuarios.ReadOnly = true;
            this.dgvUsuarios.AllowUserToAddRows = false;
            this.dgvUsuarios.RowHeadersVisible = false;
            this.dgvUsuarios.SelectionMode = DataGridViewSelectionMode.FullRowSelect;
            this.dgvUsuarios.MultiSelect = false;
            this.dgvUsuarios.AutoSizeColumnsMode = DataGridViewAutoSizeColumnsMode.Fill;
            this.dgvUsuarios.Columns.Add("colUsuario", "Usuario");
            this.dgvUsuarios.Columns.Add("colEstado", "Estado");
            this.dgvUsuarios.Columns.Add("colGrupos", "Grupos");
            this.dgvUsuarios.Columns.Add("colDni", "DNI");
            this.dgvUsuarios.Columns.Add("colApellido", "Apellido");
            this.dgvUsuarios.Columns.Add("colNombre", "Nombre");
            this.dgvUsuarios.Columns.Add("colTelefono", "Teléfono");
            this.dgvUsuarios.SelectionChanged += new EventHandler(this.dgvUsuarios_SelectionChanged);

            AgregarCampo(this.lblDni, this.txtDni, "DNI:", 20, 238);
            AgregarCampo(this.lblNombre, this.txtNombre, "Nombre:", 20, 268);
            AgregarCampo(this.lblApellido, this.txtApellido, "Apellido:", 370, 238);
            AgregarCampo(this.lblTelefono, this.txtTelefono, "Teléfono:", 370, 268);
            AgregarCampo(this.lblUsuario, this.txtUsuario, "Usuario:", 20, 298);
            AgregarCampo(this.lblClave, this.txtClave, "Nueva contraseña (opcional):", 20, 328, 165, 170);
            AgregarCampo(this.lblConfirmar, this.txtConfirmar, "Confirmar nueva contraseña:", 375, 328, 165, 175);

            this.txtClave.UseSystemPasswordChar = true;
            this.txtConfirmar.UseSystemPasswordChar = true;

            this.btnAlta.Location = new Point(20, 368);
            this.btnAlta.Size = new Size(90, 28);
            this.btnAlta.Text = "Alta";
            this.btnAlta.Click += new EventHandler(this.btnAlta_Click);

            this.btnModificar.Location = new Point(120, 368);
            this.btnModificar.Size = new Size(95, 28);
            this.btnModificar.Text = "Modificar";
            this.btnModificar.Click += new EventHandler(this.btnModificar_Click);

            this.btnBaja.Location = new Point(225, 368);
            this.btnBaja.Size = new Size(95, 28);
            this.btnBaja.Text = "Baja";
            this.btnBaja.Click += new EventHandler(this.btnBaja_Click);

            this.btnReactivar.Location = new Point(225, 368);
            this.btnReactivar.Size = new Size(95, 28);
            this.btnReactivar.Text = "Reactivar";
            this.btnReactivar.Visible = false;
            this.btnReactivar.Click += new EventHandler(this.btnReactivar_Click);

            this.btnResetearClave.Location = new Point(330, 368);
            this.btnResetearClave.Size = new Size(120, 28);
            this.btnResetearClave.Text = "Resetear clave";
            this.btnResetearClave.Click += new EventHandler(this.btnResetearClave_Click);

            this.btnLimpiar.Location = new Point(460, 368);
            this.btnLimpiar.Size = new Size(90, 28);
            this.btnLimpiar.Text = "Limpiar";
            this.btnLimpiar.Click += new EventHandler(this.btnLimpiar_Click);

            this.lblGrupos.Location = new Point(20, 410);
            this.lblGrupos.Size = new Size(260, 20);
            this.lblGrupos.Text = "Grupos asignados:";

            this.clbGrupos.Location = new Point(20, 432);
            this.clbGrupos.Size = new Size(270, 145);
            this.clbGrupos.CheckOnClick = true;

            this.lblAccionesDirectas.Location = new Point(310, 410);
            this.lblAccionesDirectas.Size = new Size(430, 20);
            this.lblAccionesDirectas.Text = "Acciones directas asignadas:";

            this.clbAccionesDirectas.Location = new Point(310, 432);
            this.clbAccionesDirectas.Size = new Size(430, 145);
            this.clbAccionesDirectas.CheckOnClick = true;

            this.btnGuardarPermisos.Location = new Point(560, 588);
            this.btnGuardarPermisos.Size = new Size(180, 30);
            this.btnGuardarPermisos.Text = "Guardar permisos del usuario";
            this.btnGuardarPermisos.Click += new EventHandler(this.btnGuardarPermisos_Click);

            this.Text = "Gestionar usuarios";
            this.Size = new Size(775, 668);
            this.FormBorderStyle = FormBorderStyle.FixedDialog;
            this.MaximizeBox = false;
            this.StartPosition = FormStartPosition.CenterParent;

            this.Controls.Add(this.lblFiltroTexto);
            this.Controls.Add(this.txtFiltroTexto);
            this.Controls.Add(this.lblFiltroEstado);
            this.Controls.Add(this.cboFiltroEstado);
            this.Controls.Add(this.lblFiltroGrupo);
            this.Controls.Add(this.cboFiltroGrupo);
            this.Controls.Add(this.dgvUsuarios);
            this.Controls.Add(this.btnAlta);
            this.Controls.Add(this.btnModificar);
            this.Controls.Add(this.btnBaja);
            this.Controls.Add(this.btnReactivar);
            this.Controls.Add(this.btnResetearClave);
            this.Controls.Add(this.btnLimpiar);
            this.Controls.Add(this.lblGrupos);
            this.Controls.Add(this.clbGrupos);
            this.Controls.Add(this.lblAccionesDirectas);
            this.Controls.Add(this.clbAccionesDirectas);
            this.Controls.Add(this.btnGuardarPermisos);
        }

        private void AgregarCampo(Label etiqueta, TextBox cuadro, string texto, int x, int y, int anchoEtiqueta = 80, int anchoCuadro = 230)
        {
            etiqueta.Location = new Point(x, y + 3);
            etiqueta.Size = new Size(anchoEtiqueta, 20);
            etiqueta.Text = texto;

            cuadro.Location = new Point(x + anchoEtiqueta + 5, y);
            cuadro.Size = new Size(anchoCuadro, 20);

            this.Controls.Add(etiqueta);
            this.Controls.Add(cuadro);
        }

        private void CargarCatalogosSeguridad()
        {
            try
            {
                _gruposDisponibles = _usuarioServicio.GetGruposDisponibles()
                    .OrderBy(g => g.Nombre)
                    .ToList();
                _accionesDisponibles = _usuarioServicio.GetAccionesDisponibles()
                    .OrderBy(a => a.Formulario == null || a.Formulario.Modulo == null ? string.Empty : a.Formulario.Modulo.Nombre)
                    .ThenBy(a => a.Formulario == null ? string.Empty : a.Formulario.Nombre)
                    .ThenBy(a => a.Nombre)
                    .ToList();

                this.cboFiltroGrupo.SelectedIndexChanged -= this.Filtros_Changed;
                this.cboFiltroGrupo.Items.Clear();
                this.cboFiltroGrupo.Items.Add(new ItemSeleccion { Id = 0, Texto = "Todos" });
                this.clbGrupos.Items.Clear();

                foreach (Grupo grupo in _gruposDisponibles)
                {
                    this.cboFiltroGrupo.Items.Add(new ItemSeleccion { Id = grupo.Id, Texto = grupo.Nombre });
                    this.clbGrupos.Items.Add(new ItemSeleccion
                    {
                        Id = grupo.Id,
                        Texto = grupo.Nombre + " (" + grupo.Estado + ")"
                    }, false);
                }

                this.cboFiltroGrupo.SelectedIndex = 0;
                this.cboFiltroGrupo.SelectedIndexChanged += this.Filtros_Changed;

                this.clbAccionesDirectas.Items.Clear();
                foreach (Accion accion in _accionesDisponibles)
                {
                    string modulo = accion.Formulario?.Modulo?.Nombre ?? string.Empty;
                    string formulario = accion.Formulario?.Nombre ?? string.Empty;
                    string prefijo = string.IsNullOrEmpty(modulo) ? string.Empty : "[" + modulo + " > " + formulario + "] ";

                    this.clbAccionesDirectas.Items.Add(new ItemSeleccion
                    {
                        Id = accion.Id,
                        Texto = prefijo + accion.Nombre
                    }, false);
                }
            }
            catch (Exception ex)
            {
                Debug.WriteLine(ex.ToString());
                MessageBox.Show("No se pudieron cargar los grupos y acciones disponibles.", "Gestionar usuarios", MessageBoxButtons.OK, MessageBoxIcon.Error);
            }
        }

        private void CargarUsuarios(int? seleccionarEmpleadoId = null)
        {
            try
            {
                _todosLosEmpleados = _usuarioServicio.GetEmpleados()
                    .OrderByDescending(e => e.Id)
                    .ToList();

                AplicarFiltros(seleccionarEmpleadoId);
            }
            catch (Exception ex)
            {
                Debug.WriteLine(ex.ToString());
                MessageBox.Show("No se pudo cargar la lista de usuarios.", "Gestionar usuarios", MessageBoxButtons.OK, MessageBoxIcon.Error);
            }
        }

        private void Filtros_Changed(object sender, EventArgs e)
        {
            Empleado actual = EmpleadoSeleccionado();
            AplicarFiltros(actual == null ? (int?)null : actual.Id);
        }

        private void AplicarFiltros(int? seleccionarEmpleadoId = null)
        {
            string texto = (this.txtFiltroTexto.Text ?? string.Empty).Trim();
            string estado = this.cboFiltroEstado.SelectedItem as string ?? "Todos";
            ItemSeleccion grupoFiltro = this.cboFiltroGrupo.SelectedItem as ItemSeleccion;
            int grupoIdFiltro = grupoFiltro == null ? 0 : grupoFiltro.Id;

            IEnumerable<Empleado> consulta = _todosLosEmpleados;

            if (texto.Length > 0)
            {
                consulta = consulta.Where(e =>
                    (e.Nombre != null && e.Nombre.IndexOf(texto, StringComparison.OrdinalIgnoreCase) >= 0)
                    || (e.Apellido != null && e.Apellido.IndexOf(texto, StringComparison.OrdinalIgnoreCase) >= 0)
                    || (e.Usuario != null && e.Usuario.Nombre != null && e.Usuario.Nombre.IndexOf(texto, StringComparison.OrdinalIgnoreCase) >= 0));
            }

            if (estado != "Todos")
            {
                consulta = consulta.Where(e =>
                    e.Usuario != null && string.Equals(e.Usuario.Estado, estado, StringComparison.OrdinalIgnoreCase));
            }

            if (grupoIdFiltro > 0)
            {
                consulta = consulta.Where(e =>
                    e.Usuario != null && e.Usuario.Grupos.Any(g => g.Id == grupoIdFiltro));
            }

            _empleadosFiltrados = consulta.ToList();

            this.dgvUsuarios.SelectionChanged -= this.dgvUsuarios_SelectionChanged;
            this.dgvUsuarios.Rows.Clear();

            foreach (Empleado empleado in _empleadosFiltrados)
            {
                string gruposTexto = empleado.Usuario == null || empleado.Usuario.Grupos.Count == 0
                    ? string.Empty
                    : string.Join(", ", empleado.Usuario.Grupos.Select(g => g.Nombre).OrderBy(n => n));

                this.dgvUsuarios.Rows.Add(
                    empleado.Usuario == null ? string.Empty : empleado.Usuario.Nombre,
                    empleado.Usuario == null ? "Sin usuario" : empleado.Usuario.Estado,
                    gruposTexto,
                    empleado.Dni,
                    empleado.Apellido,
                    empleado.Nombre,
                    empleado.Telefono);
            }

            this.dgvUsuarios.ClearSelection();
            this.dgvUsuarios.CurrentCell = null;
            this.dgvUsuarios.SelectionChanged += this.dgvUsuarios_SelectionChanged;

            if (seleccionarEmpleadoId.HasValue)
            {
                int indice = _empleadosFiltrados.FindIndex(e => e.Id == seleccionarEmpleadoId.Value);
                if (indice >= 0 && indice < this.dgvUsuarios.Rows.Count)
                {
                    this.dgvUsuarios.Rows[indice].Selected = true;
                    this.dgvUsuarios.CurrentCell = this.dgvUsuarios.Rows[indice].Cells[0];
                    this.dgvUsuarios.FirstDisplayedScrollingRowIndex = indice;
                    MostrarEmpleadoSeleccionado();
                    return;
                }
            }

            LimpiarCampos();
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
            LimpiarCheckBoxes(this.clbGrupos);
            LimpiarCheckBoxes(this.clbAccionesDirectas);
            ActualizarBotonesEstadoUsuario(null);
        }

        private void ActualizarBotonesEstadoUsuario(Empleado empleado)
        {
            if (empleado == null || empleado.Usuario == null)
            {
                this.btnBaja.Visible = true;
                this.btnBaja.Enabled = false;
                this.btnReactivar.Visible = false;
                this.btnReactivar.Enabled = false;
                return;
            }

            bool estaInactivo = string.Equals(empleado.Usuario.Estado, EstadosUsuario.Inactivo, StringComparison.OrdinalIgnoreCase);

            this.btnBaja.Visible = !estaInactivo;
            this.btnBaja.Enabled = !estaInactivo;
            this.btnReactivar.Visible = estaInactivo;
            this.btnReactivar.Enabled = estaInactivo;
        }

        private static void LimpiarCheckBoxes(CheckedListBox lista)
        {
            for (int i = 0; i < lista.Items.Count; i++)
            {
                lista.SetItemChecked(i, false);
            }
        }

        private Empleado EmpleadoSeleccionado()
        {
            if (this.dgvUsuarios.CurrentRow == null)
            {
                return null;
            }

            int indice = this.dgvUsuarios.CurrentRow.Index;

            if (indice < 0 || indice >= _empleadosFiltrados.Count)
            {
                return null;
            }

            return _empleadosFiltrados[indice];
        }

        private void dgvUsuarios_SelectionChanged(object sender, EventArgs e)
        {
            MostrarEmpleadoSeleccionado();
        }

        private void MostrarEmpleadoSeleccionado()
        {
            Empleado empleado = EmpleadoSeleccionado();

            if (empleado == null)
            {
                LimpiarCampos();
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

            HashSet<int> gruposUsuario = empleado.Usuario == null
                ? new HashSet<int>()
                : new HashSet<int>(empleado.Usuario.Grupos.Select(g => g.Id));

            for (int i = 0; i < this.clbGrupos.Items.Count; i++)
            {
                ItemSeleccion item = (ItemSeleccion)this.clbGrupos.Items[i];
                this.clbGrupos.SetItemChecked(i, gruposUsuario.Contains(item.Id));
            }

            HashSet<int> accionesUsuario = empleado.Usuario == null
                ? new HashSet<int>()
                : new HashSet<int>(empleado.Usuario.AccionesDirectas.Select(a => a.Id));

            for (int i = 0; i < this.clbAccionesDirectas.Items.Count; i++)
            {
                ItemSeleccion item = (ItemSeleccion)this.clbAccionesDirectas.Items[i];
                this.clbAccionesDirectas.SetItemChecked(i, accionesUsuario.Contains(item.Id));
            }

            ActualizarBotonesEstadoUsuario(empleado);
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

                CargarUsuarios(empleado.Id);
            }
            catch (Exception ex)
            {
                Debug.WriteLine(ex.ToString());
                MessageBox.Show("Ocurrió un error al dar de baja el usuario.", "Gestionar usuarios", MessageBoxButtons.OK, MessageBoxIcon.Error);
            }
        }

        private void btnReactivar_Click(object sender, EventArgs e)
        {
            Empleado empleado = EmpleadoSeleccionado();

            if (empleado == null)
            {
                MessageBox.Show("Seleccione un usuario de la lista.", "Gestionar usuarios", MessageBoxButtons.OK, MessageBoxIcon.Information);
                return;
            }

            string pregunta = "¿Reactivar a " + empleado.Apellido + " " + empleado.Nombre + " (DNI " + empleado.Dni + ")?";
            if (MessageBox.Show(pregunta, "Gestionar usuarios", MessageBoxButtons.YesNo, MessageBoxIcon.Question) != DialogResult.Yes)
            {
                return;
            }

            try
            {
                if (!_usuarioServicio.ReactivarUsuario(empleado.Id))
                {
                    MessageBox.Show("No se pudo reactivar el usuario.", "Gestionar usuarios", MessageBoxButtons.OK, MessageBoxIcon.Warning);
                    return;
                }

                CargarUsuarios(empleado.Id);
                MessageBox.Show("El usuario fue reactivado correctamente.", "Gestionar usuarios", MessageBoxButtons.OK, MessageBoxIcon.Information);
            }
            catch (Exception ex)
            {
                Debug.WriteLine(ex.ToString());
                MessageBox.Show("Ocurrió un error al reactivar el usuario.", "Gestionar usuarios", MessageBoxButtons.OK, MessageBoxIcon.Error);
            }
        }

        private void btnResetearClave_Click(object sender, EventArgs e)
        {
            Empleado empleado = EmpleadoSeleccionado();

            if (empleado == null)
            {
                MessageBox.Show("Seleccione un usuario de la lista.", "Gestionar usuarios", MessageBoxButtons.OK, MessageBoxIcon.Information);
                return;
            }

            string pregunta = "¿Generar una nueva clave temporal para " + empleado.Apellido + " " + empleado.Nombre + "? La clave anterior dejará de funcionar.";
            if (MessageBox.Show(pregunta, "Resetear clave", MessageBoxButtons.YesNo, MessageBoxIcon.Question) != DialogResult.Yes)
            {
                return;
            }

            try
            {
                string claveTemporal = _usuarioServicio.ResetearClave(empleado.Id);
                if (claveTemporal == null)
                {
                    MessageBox.Show("No se pudo resetear la clave del usuario.", "Resetear clave", MessageBoxButtons.OK, MessageBoxIcon.Warning);
                    return;
                }

                bool copiada = false;
                try
                {
                    Clipboard.SetText(claveTemporal);
                    copiada = true;
                }
                catch (Exception ex)
                {
                    Debug.WriteLine(ex.ToString());
                }

                string mensaje = copiada
                    ? "Clave temporal generada (se muestra una sola vez y fue copiada al portapapeles):\n\n" + claveTemporal
                    : "Clave temporal generada (se muestra una sola vez; no se pudo copiar al portapapeles):\n\n" + claveTemporal;

                MessageBox.Show(
                    mensaje,
                    "Resetear clave",
                    MessageBoxButtons.OK,
                    MessageBoxIcon.Information);
            }
            catch (Exception ex)
            {
                Debug.WriteLine(ex.ToString());
                MessageBox.Show("Ocurrió un error al resetear la clave.", "Resetear clave", MessageBoxButtons.OK, MessageBoxIcon.Error);
            }
        }

        private void btnGuardarPermisos_Click(object sender, EventArgs e)
        {
            Empleado empleado = EmpleadoSeleccionado();

            if (empleado == null || empleado.Usuario == null)
            {
                MessageBox.Show("Seleccione un usuario de la lista.", "Gestionar usuarios", MessageBoxButtons.OK, MessageBoxIcon.Information);
                return;
            }

            List<int> grupoIds = this.clbGrupos.CheckedItems
                .OfType<ItemSeleccion>()
                .Select(i => i.Id)
                .ToList();

            List<int> accionDirectaIds = this.clbAccionesDirectas.CheckedItems
                .OfType<ItemSeleccion>()
                .Select(i => i.Id)
                .ToList();

            try
            {
                if (!_usuarioServicio.AsignarPermisosUsuario(empleado.Id, grupoIds, accionDirectaIds))
                {
                    MessageBox.Show("No se pudieron asignar los permisos al usuario.", "Gestionar usuarios", MessageBoxButtons.OK, MessageBoxIcon.Warning);
                    return;
                }

                if (empleado.UsuarioId.HasValue && empleado.UsuarioId.Value == SesionActual.Instancia.UsuarioId)
                {
                    IReadOnlySet<string> permisos = _usuarioServicio.ObtenerPermisosEfectivos(SesionActual.Instancia.UsuarioId);
                    SesionActual.Instancia.ActualizarPermisos(permisos);
                }

                CargarUsuarios(empleado.Id);
                MessageBox.Show("Permisos del usuario actualizados correctamente.", "Gestionar usuarios", MessageBoxButtons.OK, MessageBoxIcon.Information);
            }
            catch (Exception ex)
            {
                Debug.WriteLine(ex.ToString());
                MessageBox.Show("Ocurrió un error al guardar los permisos del usuario.", "Gestionar usuarios", MessageBoxButtons.OK, MessageBoxIcon.Error);
            }
        }
    }
}
