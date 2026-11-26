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
    public class FrmGrupos : Form
    {
        private sealed class ItemAccion
        {
            public int Id { get; set; }
            public string Texto { get; set; }
            public override string ToString() => Texto;
        }

        private sealed class ItemGrupo
        {
            public int Id { get; set; }
            public string Texto { get; set; }
            public override string ToString() => Texto;
        }

        private readonly GrupoServicio _grupoServicio;
        private readonly UsuarioServicio _usuarioServicio;

        private List<Grupo> _todosLosGrupos = new List<Grupo>();
        private List<Grupo> _gruposFiltrados = new List<Grupo>();
        private List<ItemAccion> _accionesCatalogo = new List<ItemAccion>();

        private Label lblFiltroTexto;
        private TextBox txtFiltroTexto;
        private Label lblFiltroEstado;
        private ComboBox cboFiltroEstado;
        private DataGridView dgvGrupos;

        private Label lblNombre;
        private TextBox txtNombre;
        private Label lblDescripcion;
        private TextBox txtDescripcion;

        private Button btnAlta;
        private Button btnModificar;
        private Button btnCambiarEstado;
        private Button btnEliminar;
        private Button btnLimpiar;

        private Label lblAcciones;
        private CheckedListBox clbAcciones;
        private Label lblSubgrupos;
        private CheckedListBox clbSubgrupos;
        private Button btnGuardarPermisos;
        private ErrorProvider errorProvider;

        public FrmGrupos(GrupoServicio grupoServicio, UsuarioServicio usuarioServicio)
        {
            _grupoServicio = grupoServicio;
            _usuarioServicio = usuarioServicio;

            InitializeComponent();
            CargarCatalogoAcciones();
            CargarGrupos();
        }

        private void InitializeComponent()
        {
            this.lblFiltroTexto = new Label();
            this.txtFiltroTexto = new TextBox();
            this.lblFiltroEstado = new Label();
            this.cboFiltroEstado = new ComboBox();
            this.dgvGrupos = new DataGridView();
            this.lblNombre = new Label();
            this.txtNombre = new TextBox();
            this.lblDescripcion = new Label();
            this.txtDescripcion = new TextBox();
            this.btnAlta = new Button();
            this.btnModificar = new Button();
            this.btnCambiarEstado = new Button();
            this.btnEliminar = new Button();
            this.btnLimpiar = new Button();
            this.lblAcciones = new Label();
            this.clbAcciones = new CheckedListBox();
            this.lblSubgrupos = new Label();
            this.clbSubgrupos = new CheckedListBox();
            this.btnGuardarPermisos = new Button();
            this.errorProvider = new ErrorProvider(this);

            this.lblFiltroTexto.Location = new Point(20, 18);
            this.lblFiltroTexto.Size = new Size(125, 20);
            this.lblFiltroTexto.Text = "Nombre / Descripción:";

            this.txtFiltroTexto.Location = new Point(150, 15);
            this.txtFiltroTexto.Size = new Size(230, 20);
            this.txtFiltroTexto.TextChanged += new EventHandler(this.Filtros_Changed);

            this.lblFiltroEstado.Location = new Point(400, 18);
            this.lblFiltroEstado.Size = new Size(50, 20);
            this.lblFiltroEstado.Text = "Estado:";

            this.cboFiltroEstado.Location = new Point(455, 15);
            this.cboFiltroEstado.Size = new Size(120, 21);
            this.cboFiltroEstado.DropDownStyle = ComboBoxStyle.DropDownList;
            this.cboFiltroEstado.Items.AddRange(new object[] { "Todos", EstadosGrupo.Activo, EstadosGrupo.Inactivo });
            this.cboFiltroEstado.SelectedIndex = 0;
            this.cboFiltroEstado.SelectedIndexChanged += new EventHandler(this.Filtros_Changed);

            this.dgvGrupos.Location = new Point(20, 48);
            this.dgvGrupos.Size = new Size(800, 180);
            this.dgvGrupos.ReadOnly = true;
            this.dgvGrupos.AllowUserToAddRows = false;
            this.dgvGrupos.RowHeadersVisible = false;
            this.dgvGrupos.SelectionMode = DataGridViewSelectionMode.FullRowSelect;
            this.dgvGrupos.MultiSelect = false;
            this.dgvGrupos.AutoSizeColumnsMode = DataGridViewAutoSizeColumnsMode.Fill;
            this.dgvGrupos.Columns.Add("colNombre", "Nombre");
            this.dgvGrupos.Columns.Add("colDescripcion", "Descripción");
            this.dgvGrupos.Columns.Add("colEstado", "Estado");
            this.dgvGrupos.Columns.Add("colAcciones", "Acciones");
            this.dgvGrupos.Columns.Add("colSubgrupos", "Subgrupos");
            this.dgvGrupos.SelectionChanged += new EventHandler(this.dgvGrupos_SelectionChanged);

            this.lblNombre.Location = new Point(20, 243);
            this.lblNombre.Size = new Size(80, 20);
            this.lblNombre.Text = "Nombre:";

            this.txtNombre.Location = new Point(105, 240);
            this.txtNombre.Size = new Size(220, 20);

            this.lblDescripcion.Location = new Point(345, 243);
            this.lblDescripcion.Size = new Size(80, 20);
            this.lblDescripcion.Text = "Descripción:";

            this.txtDescripcion.Location = new Point(430, 240);
            this.txtDescripcion.Size = new Size(390, 20);

            this.btnAlta.Location = new Point(20, 275);
            this.btnAlta.Size = new Size(95, 28);
            this.btnAlta.Text = "Alta";
            this.btnAlta.Click += new EventHandler(this.btnAlta_Click);

            this.btnModificar.Location = new Point(125, 275);
            this.btnModificar.Size = new Size(95, 28);
            this.btnModificar.Text = "Modificar";
            this.btnModificar.Click += new EventHandler(this.btnModificar_Click);

            this.btnCambiarEstado.Location = new Point(230, 275);
            this.btnCambiarEstado.Size = new Size(125, 28);
            this.btnCambiarEstado.Text = "Activar / Desactivar";
            this.btnCambiarEstado.Click += new EventHandler(this.btnCambiarEstado_Click);

            this.btnEliminar.Location = new Point(365, 275);
            this.btnEliminar.Size = new Size(95, 28);
            this.btnEliminar.Text = "Eliminar";
            this.btnEliminar.Click += new EventHandler(this.btnEliminar_Click);

            this.btnLimpiar.Location = new Point(470, 275);
            this.btnLimpiar.Size = new Size(95, 28);
            this.btnLimpiar.Text = "Limpiar";
            this.btnLimpiar.Click += new EventHandler(this.btnLimpiar_Click);

            this.lblAcciones.Location = new Point(20, 318);
            this.lblAcciones.Size = new Size(460, 20);
            this.lblAcciones.Text = "Acciones asignadas ([Módulo > Formulario] Acción):";

            this.clbAcciones.Location = new Point(20, 340);
            this.clbAcciones.Size = new Size(470, 195);
            this.clbAcciones.CheckOnClick = true;

            this.lblSubgrupos.Location = new Point(510, 318);
            this.lblSubgrupos.Size = new Size(310, 20);
            this.lblSubgrupos.Text = "Subgrupos incluidos:";

            this.clbSubgrupos.Location = new Point(510, 340);
            this.clbSubgrupos.Size = new Size(310, 195);
            this.clbSubgrupos.CheckOnClick = true;

            this.btnGuardarPermisos.Location = new Point(640, 545);
            this.btnGuardarPermisos.Size = new Size(180, 30);
            this.btnGuardarPermisos.Text = "Guardar permisos y subgrupos";
            this.btnGuardarPermisos.Click += new EventHandler(this.btnGuardarPermisos_Click);

            this.Text = "Gestionar grupos";
            this.Size = new Size(855, 625);
            this.FormBorderStyle = FormBorderStyle.FixedDialog;
            this.MaximizeBox = false;
            this.StartPosition = FormStartPosition.CenterParent;

            this.Controls.Add(this.lblFiltroTexto);
            this.Controls.Add(this.txtFiltroTexto);
            this.Controls.Add(this.lblFiltroEstado);
            this.Controls.Add(this.cboFiltroEstado);
            this.Controls.Add(this.dgvGrupos);
            this.Controls.Add(this.lblNombre);
            this.Controls.Add(this.txtNombre);
            this.Controls.Add(this.lblDescripcion);
            this.Controls.Add(this.txtDescripcion);
            this.Controls.Add(this.btnAlta);
            this.Controls.Add(this.btnModificar);
            this.Controls.Add(this.btnCambiarEstado);
            this.Controls.Add(this.btnEliminar);
            this.Controls.Add(this.btnLimpiar);
            this.Controls.Add(this.lblAcciones);
            this.Controls.Add(this.clbAcciones);
            this.Controls.Add(this.lblSubgrupos);
            this.Controls.Add(this.clbSubgrupos);
            this.Controls.Add(this.btnGuardarPermisos);
        }

        private void CargarCatalogoAcciones()
        {
            _accionesCatalogo.Clear();
            this.clbAcciones.Items.Clear();

            try
            {
                List<Modulo> modulos = _grupoServicio.GetAllModulosConAcciones();

                foreach (Modulo modulo in modulos.OrderBy(m => m.Nombre))
                {
                    foreach (Formulario formulario in modulo.Formularios.OrderBy(f => f.Nombre))
                    {
                        foreach (Accion accion in formulario.Acciones.OrderBy(a => a.Nombre))
                        {
                            ItemAccion item = new ItemAccion
                            {
                                Id = accion.Id,
                                Texto = "[" + modulo.Nombre + " > " + formulario.Nombre + "] " + accion.Nombre
                            };
                            _accionesCatalogo.Add(item);
                            this.clbAcciones.Items.Add(item, false);
                        }
                    }
                }
            }
            catch (Exception ex)
            {
                Debug.WriteLine(ex.ToString());
                MessageBox.Show("No se pudo cargar el catálogo de acciones.", "Gestionar grupos", MessageBoxButtons.OK, MessageBoxIcon.Error);
            }
        }

        private void CargarGrupos(int? seleccionarGrupoId = null)
        {
            try
            {
                _todosLosGrupos = _grupoServicio.GetAllGrupos()
                    .OrderBy(g => g.Nombre)
                    .ToList();

                AplicarFiltros(seleccionarGrupoId);
            }
            catch (Exception ex)
            {
                Debug.WriteLine(ex.ToString());
                MessageBox.Show("No se pudo cargar la lista de grupos.", "Gestionar grupos", MessageBoxButtons.OK, MessageBoxIcon.Error);
            }
        }

        private void Filtros_Changed(object sender, EventArgs e)
        {
            Grupo actual = GrupoSeleccionado();
            AplicarFiltros(actual == null ? (int?)null : actual.Id);
        }

        private void AplicarFiltros(int? seleccionarGrupoId = null)
        {
            string texto = (this.txtFiltroTexto.Text ?? string.Empty).Trim();
            string estado = this.cboFiltroEstado.SelectedItem as string ?? "Todos";

            IEnumerable<Grupo> consulta = _todosLosGrupos;

            if (texto.Length > 0)
            {
                consulta = consulta.Where(g =>
                    (g.Nombre != null && g.Nombre.IndexOf(texto, StringComparison.OrdinalIgnoreCase) >= 0)
                    || (g.Descripcion != null && g.Descripcion.IndexOf(texto, StringComparison.OrdinalIgnoreCase) >= 0));
            }

            if (estado != "Todos")
            {
                consulta = consulta.Where(g => string.Equals(g.Estado, estado, StringComparison.OrdinalIgnoreCase));
            }

            _gruposFiltrados = consulta.ToList();

            this.dgvGrupos.SelectionChanged -= this.dgvGrupos_SelectionChanged;
            this.dgvGrupos.Rows.Clear();

            foreach (Grupo grupo in _gruposFiltrados)
            {
                this.dgvGrupos.Rows.Add(
                    grupo.Nombre,
                    grupo.Descripcion,
                    grupo.Estado,
                    grupo.Acciones.Count,
                    grupo.Subgrupos.Count);
            }

            this.dgvGrupos.ClearSelection();
            this.dgvGrupos.CurrentCell = null;
            this.dgvGrupos.SelectionChanged += this.dgvGrupos_SelectionChanged;

            if (seleccionarGrupoId.HasValue)
            {
                int indice = _gruposFiltrados.FindIndex(g => g.Id == seleccionarGrupoId.Value);
                if (indice >= 0 && indice < this.dgvGrupos.Rows.Count)
                {
                    this.dgvGrupos.Rows[indice].Selected = true;
                    this.dgvGrupos.CurrentCell = this.dgvGrupos.Rows[indice].Cells[0];
                    MostrarGrupoSeleccionado();
                    return;
                }
            }

            LimpiarCampos();
        }

        private Grupo GrupoSeleccionado()
        {
            if (this.dgvGrupos.CurrentRow == null)
            {
                return null;
            }

            int indice = this.dgvGrupos.CurrentRow.Index;
            if (indice < 0 || indice >= _gruposFiltrados.Count)
            {
                return null;
            }

            return _gruposFiltrados[indice];
        }

        private void dgvGrupos_SelectionChanged(object sender, EventArgs e)
        {
            MostrarGrupoSeleccionado();
        }

        private void MostrarGrupoSeleccionado()
        {
            Grupo grupo = GrupoSeleccionado();
            this.errorProvider.Clear();

            if (grupo == null)
            {
                LimpiarCampos();
                return;
            }

            this.txtNombre.Text = grupo.Nombre;
            this.txtDescripcion.Text = grupo.Descripcion;
            this.btnCambiarEstado.Text = grupo.Estado == EstadosGrupo.Activo ? "Desactivar" : "Activar";

            HashSet<int> accionesIds = new HashSet<int>(grupo.Acciones.Select(a => a.Id));
            for (int i = 0; i < this.clbAcciones.Items.Count; i++)
            {
                ItemAccion item = (ItemAccion)this.clbAcciones.Items[i];
                this.clbAcciones.SetItemChecked(i, accionesIds.Contains(item.Id));
            }

            HashSet<int> subgruposIds = new HashSet<int>(grupo.Subgrupos.Select(s => s.Id));
            this.clbSubgrupos.Items.Clear();
            foreach (Grupo candidato in _todosLosGrupos.Where(g => g.Id != grupo.Id))
            {
                ItemGrupo item = new ItemGrupo
                {
                    Id = candidato.Id,
                    Texto = candidato.Nombre + " (" + candidato.Estado + ")"
                };
                this.clbSubgrupos.Items.Add(item, subgruposIds.Contains(candidato.Id));
            }
        }

        private void LimpiarCampos()
        {
            this.errorProvider.Clear();
            this.txtNombre.Text = string.Empty;
            this.txtDescripcion.Text = string.Empty;
            this.btnCambiarEstado.Text = "Activar / Desactivar";

            for (int i = 0; i < this.clbAcciones.Items.Count; i++)
            {
                this.clbAcciones.SetItemChecked(i, false);
            }

            this.clbSubgrupos.Items.Clear();
        }

        private void btnLimpiar_Click(object sender, EventArgs e)
        {
            this.dgvGrupos.ClearSelection();
            this.dgvGrupos.CurrentCell = null;
            LimpiarCampos();
            this.txtNombre.Focus();
        }

        private bool ValidarCamposBasicos()
        {
            this.errorProvider.Clear();

            if (string.IsNullOrWhiteSpace(this.txtNombre.Text) || this.txtNombre.Text.Trim().Length > 100)
            {
                this.errorProvider.SetError(this.txtNombre, "Ingrese un nombre obligatorio de hasta 100 caracteres.");
            }

            if (this.txtDescripcion.Text.Trim().Length > 250)
            {
                this.errorProvider.SetError(this.txtDescripcion, "La descripción no puede superar los 250 caracteres.");
            }

            return this.errorProvider.GetError(this.txtNombre).Length == 0
                && this.errorProvider.GetError(this.txtDescripcion).Length == 0;
        }

        private void RefrescarPermisosSesion()
        {
            if (SesionActual.UsuarioId > 0)
            {
                IReadOnlySet<string> permisos = _usuarioServicio.ObtenerPermisosEfectivos(SesionActual.UsuarioId);
                SesionActual.ActualizarPermisos(permisos);
            }
        }

        private void btnAlta_Click(object sender, EventArgs e)
        {
            if (!ValidarCamposBasicos())
            {
                return;
            }

            try
            {
                if (!_grupoServicio.AddGrupo(this.txtNombre.Text, this.txtDescripcion.Text))
                {
                    MessageBox.Show("No se pudo crear el grupo. Verifique que el nombre no exista ya.", "Gestionar grupos", MessageBoxButtons.OK, MessageBoxIcon.Warning);
                    return;
                }

                CargarGrupos();
            }
            catch (Exception ex)
            {
                Debug.WriteLine(ex.ToString());
                MessageBox.Show("Ocurrió un error al crear el grupo.", "Gestionar grupos", MessageBoxButtons.OK, MessageBoxIcon.Error);
            }
        }

        private void btnModificar_Click(object sender, EventArgs e)
        {
            Grupo grupo = GrupoSeleccionado();
            if (grupo == null)
            {
                MessageBox.Show("Seleccione un grupo de la lista.", "Gestionar grupos", MessageBoxButtons.OK, MessageBoxIcon.Information);
                return;
            }

            if (!ValidarCamposBasicos())
            {
                return;
            }

            try
            {
                if (!_grupoServicio.UpdateGrupo(grupo.Id, this.txtNombre.Text, this.txtDescripcion.Text))
                {
                    MessageBox.Show("No se pudo modificar el grupo. Verifique que el nombre no pertenezca a otro grupo.", "Gestionar grupos", MessageBoxButtons.OK, MessageBoxIcon.Warning);
                    return;
                }

                RefrescarPermisosSesion();
                CargarGrupos(grupo.Id);
                MessageBox.Show("Grupo modificado correctamente.", "Gestionar grupos", MessageBoxButtons.OK, MessageBoxIcon.Information);
            }
            catch (Exception ex)
            {
                Debug.WriteLine(ex.ToString());
                MessageBox.Show("Ocurrió un error al modificar el grupo.", "Gestionar grupos", MessageBoxButtons.OK, MessageBoxIcon.Error);
            }
        }

        private void btnCambiarEstado_Click(object sender, EventArgs e)
        {
            Grupo grupo = GrupoSeleccionado();
            if (grupo == null)
            {
                MessageBox.Show("Seleccione un grupo de la lista.", "Gestionar grupos", MessageBoxButtons.OK, MessageBoxIcon.Information);
                return;
            }

            bool desactivar = grupo.Estado == EstadosGrupo.Activo;
            string pregunta = (desactivar ? "¿Desactivar" : "¿Activar") + " el grupo '" + grupo.Nombre + "'?";

            if (MessageBox.Show(pregunta, "Gestionar grupos", MessageBoxButtons.YesNo, MessageBoxIcon.Question) != DialogResult.Yes)
            {
                return;
            }

            try
            {
                bool ok = desactivar
                    ? _grupoServicio.DesactivarGrupo(grupo.Id)
                    : _grupoServicio.ActivarGrupo(grupo.Id);

                if (!ok)
                {
                    MessageBox.Show("No se pudo actualizar el estado del grupo.", "Gestionar grupos", MessageBoxButtons.OK, MessageBoxIcon.Warning);
                    return;
                }

                RefrescarPermisosSesion();
                CargarGrupos(grupo.Id);
            }
            catch (Exception ex)
            {
                Debug.WriteLine(ex.ToString());
                MessageBox.Show("Ocurrió un error al cambiar el estado del grupo.", "Gestionar grupos", MessageBoxButtons.OK, MessageBoxIcon.Error);
            }
        }

        private void btnEliminar_Click(object sender, EventArgs e)
        {
            Grupo grupo = GrupoSeleccionado();
            if (grupo == null)
            {
                MessageBox.Show("Seleccione un grupo de la lista.", "Gestionar grupos", MessageBoxButtons.OK, MessageBoxIcon.Information);
                return;
            }

            if (MessageBox.Show("¿Eliminar el grupo '" + grupo.Nombre + "'?", "Gestionar grupos", MessageBoxButtons.YesNo, MessageBoxIcon.Question) != DialogResult.Yes)
            {
                return;
            }

            try
            {
                if (!_grupoServicio.EliminarGrupo(grupo.Id))
                {
                    MessageBox.Show(
                        "No se puede eliminar el grupo porque tiene usuarios asignados, es subgrupo de otro grupo o posee subgrupos hijos.",
                        "Gestionar grupos",
                        MessageBoxButtons.OK,
                        MessageBoxIcon.Warning);
                    return;
                }

                RefrescarPermisosSesion();
                CargarGrupos();
            }
            catch (Exception ex)
            {
                Debug.WriteLine(ex.ToString());
                MessageBox.Show("Ocurrió un error al eliminar el grupo.", "Gestionar grupos", MessageBoxButtons.OK, MessageBoxIcon.Error);
            }
        }

        private void btnGuardarPermisos_Click(object sender, EventArgs e)
        {
            Grupo grupo = GrupoSeleccionado();
            if (grupo == null)
            {
                MessageBox.Show("Seleccione un grupo de la lista.", "Gestionar grupos", MessageBoxButtons.OK, MessageBoxIcon.Information);
                return;
            }

            List<int> accionIds = this.clbAcciones.CheckedItems
                .OfType<ItemAccion>()
                .Select(i => i.Id)
                .ToList();

            List<int> subgrupoIds = this.clbSubgrupos.CheckedItems
                .OfType<ItemGrupo>()
                .Select(i => i.Id)
                .ToList();

            try
            {
                if (!_grupoServicio.AsignarSubgrupos(grupo.Id, subgrupoIds))
                {
                    MessageBox.Show(
                        "No se pudieron asignar los subgrupos. Verifique que la jerarquía seleccionada no forme un ciclo.",
                        "Gestionar grupos",
                        MessageBoxButtons.OK,
                        MessageBoxIcon.Warning);
                    return;
                }

                if (!_grupoServicio.AsignarAcciones(grupo.Id, accionIds))
                {
                    MessageBox.Show("No se pudieron asignar las acciones al grupo.", "Gestionar grupos", MessageBoxButtons.OK, MessageBoxIcon.Warning);
                    return;
                }

                RefrescarPermisosSesion();
                CargarGrupos(grupo.Id);
                MessageBox.Show("Permisos y subgrupos actualizados correctamente.", "Gestionar grupos", MessageBoxButtons.OK, MessageBoxIcon.Information);
            }
            catch (Exception ex)
            {
                Debug.WriteLine(ex.ToString());
                MessageBox.Show("Ocurrió un error al guardar permisos y subgrupos.", "Gestionar grupos", MessageBoxButtons.OK, MessageBoxIcon.Error);
            }
        }
    }
}
