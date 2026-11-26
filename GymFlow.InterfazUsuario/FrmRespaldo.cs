using GymFlow.Aplicacion;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Drawing;
using System.IO;
using System.Linq;
using System.Windows.Forms;

namespace GymFlow.InterfazUsuario
{
    public class FrmRespaldo : Form
    {
        private readonly RespaldoServicio _respaldoServicio;
        private string _nombreBaseActual = string.Empty;

        private Label lblBaseTitulo;
        private Label lblBaseValor;
        private GroupBox grpGenerar;
        private Label lblDescripcionGenerar;
        private Button btnGenerarRespaldo;
        private GroupBox grpRestaurar;
        private Label lblDescripcionRestaurar;
        private Button btnRestaurarRespaldo;
        private Button btnCerrar;

        public FrmRespaldo(RespaldoServicio respaldoServicio)
        {
            _respaldoServicio = respaldoServicio;
            InitializeComponent();
            CargarBaseActual();
        }

        private void InitializeComponent()
        {
            this.lblBaseTitulo = new Label();
            this.lblBaseValor = new Label();
            this.grpGenerar = new GroupBox();
            this.lblDescripcionGenerar = new Label();
            this.btnGenerarRespaldo = new Button();
            this.grpRestaurar = new GroupBox();
            this.lblDescripcionRestaurar = new Label();
            this.btnRestaurarRespaldo = new Button();
            this.btnCerrar = new Button();

            this.lblBaseTitulo.Location = new Point(20, 20);
            this.lblBaseTitulo.Size = new Size(145, 20);
            this.lblBaseTitulo.Text = "Base de datos activa:";

            this.lblBaseValor.Location = new Point(165, 20);
            this.lblBaseValor.Size = new Size(320, 20);
            this.lblBaseValor.Font = new Font(this.lblBaseValor.Font, FontStyle.Bold);

            // Bloque 1 — Generar resguardo
            this.grpGenerar.Location = new Point(20, 52);
            this.grpGenerar.Size = new Size(465, 115);
            this.grpGenerar.Text = "Generar resguardo";

            this.lblDescripcionGenerar.Location = new Point(15, 25);
            this.lblDescripcionGenerar.Size = new Size(435, 38);
            this.lblDescripcionGenerar.Text =
                "Genera un resguardo completo (.bak) de la base de datos activa con verificación de integridad (CHECKSUM).";

            this.btnGenerarRespaldo.Location = new Point(15, 70);
            this.btnGenerarRespaldo.Size = new Size(190, 30);
            this.btnGenerarRespaldo.Text = "Generar resguardo...";
            this.btnGenerarRespaldo.Click += new EventHandler(this.btnGenerarRespaldo_Click);

            this.grpGenerar.Controls.Add(this.lblDescripcionGenerar);
            this.grpGenerar.Controls.Add(this.btnGenerarRespaldo);

            // Bloque 2 — Restaurar resguardo
            this.grpRestaurar.Location = new Point(20, 180);
            this.grpRestaurar.Size = new Size(465, 130);
            this.grpRestaurar.Text = "Restaurar resguardo";

            this.lblDescripcionRestaurar.Location = new Point(15, 25);
            this.lblDescripcionRestaurar.Size = new Size(435, 52);
            this.lblDescripcionRestaurar.ForeColor = Color.DarkRed;
            this.lblDescripcionRestaurar.Text =
                "Atención: restaurar un archivo .bak sobrescribirá todos los datos actuales de la base de datos activa. " +
                "Al finalizar correctamente, la sesión se cerrará para recargar el sistema.";

            this.btnRestaurarRespaldo.Location = new Point(15, 85);
            this.btnRestaurarRespaldo.Size = new Size(190, 30);
            this.btnRestaurarRespaldo.Text = "Restaurar resguardo...";
            this.btnRestaurarRespaldo.Click += new EventHandler(this.btnRestaurarRespaldo_Click);

            this.grpRestaurar.Controls.Add(this.lblDescripcionRestaurar);
            this.grpRestaurar.Controls.Add(this.btnRestaurarRespaldo);

            this.btnCerrar.Location = new Point(390, 325);
            this.btnCerrar.Size = new Size(95, 30);
            this.btnCerrar.Text = "Cerrar";
            this.btnCerrar.DialogResult = DialogResult.Cancel;

            this.Text = "Resguardo y restauración de base de datos";
            this.ClientSize = new Size(505, 370);
            this.FormBorderStyle = FormBorderStyle.FixedDialog;
            this.MaximizeBox = false;
            this.StartPosition = FormStartPosition.CenterParent;
            this.CancelButton = this.btnCerrar;

            this.Controls.Add(this.lblBaseTitulo);
            this.Controls.Add(this.lblBaseValor);
            this.Controls.Add(this.grpGenerar);
            this.Controls.Add(this.grpRestaurar);
            this.Controls.Add(this.btnCerrar);
        }

        private void CargarBaseActual()
        {
            try
            {
                _nombreBaseActual = _respaldoServicio.ObtenerNombreBaseActual();
                this.lblBaseValor.Text = _nombreBaseActual;
            }
            catch (UnauthorizedAccessException ex)
            {
                this.lblBaseValor.Text = "Sin permiso";
                this.btnGenerarRespaldo.Enabled = false;
                this.btnRestaurarRespaldo.Enabled = false;
                MessageBox.Show(ex.Message, "Resguardo y restauración", MessageBoxButtons.OK, MessageBoxIcon.Warning);
            }
            catch (Exception ex)
            {
                Debug.WriteLine(ex.ToString());
                this.lblBaseValor.Text = "No disponible";
                this.btnGenerarRespaldo.Enabled = false;
                this.btnRestaurarRespaldo.Enabled = false;
                MessageBox.Show(
                    "No se pudo determinar la base de datos activa.\n\n" + ex.Message,
                    "Resguardo y restauración",
                    MessageBoxButtons.OK,
                    MessageBoxIcon.Error);
            }
        }

        private string ObtenerRutaInicialDialogo()
        {
            try
            {
                string rutaPorDefecto = _respaldoServicio.ObtenerRutaPorDefecto();
                if (!string.IsNullOrWhiteSpace(rutaPorDefecto) && Directory.Exists(rutaPorDefecto))
                {
                    return rutaPorDefecto;
                }
            }
            catch (Exception ex)
            {
                Debug.WriteLine(ex.ToString());
            }

            return string.Empty;
        }

        private void btnGenerarRespaldo_Click(object sender, EventArgs e)
        {
            using (SaveFileDialog dialogo = new SaveFileDialog())
            {
                dialogo.Title = "Guardar resguardo de base de datos";
                dialogo.Filter = "Archivos de respaldo (*.bak)|*.bak";
                dialogo.DefaultExt = "bak";
                dialogo.AddExtension = true;
                dialogo.OverwritePrompt = true;
                dialogo.FileName = $"{_nombreBaseActual}_{DateTime.Now:yyyyMMdd_HHmmss}.bak";

                string rutaInicial = ObtenerRutaInicialDialogo();
                if (!string.IsNullOrEmpty(rutaInicial))
                {
                    dialogo.InitialDirectory = rutaInicial;
                }

                if (dialogo.ShowDialog(this) != DialogResult.OK)
                {
                    return;
                }

                EstablecerEstadoOcupado(true);
                try
                {
                    _respaldoServicio.GenerarRespaldo(dialogo.FileName);

                    MessageBox.Show(
                        "El resguardo de la base de datos se generó correctamente en:\n\n" + dialogo.FileName,
                        "Resguardo generado",
                        MessageBoxButtons.OK,
                        MessageBoxIcon.Information);
                }
                catch (UnauthorizedAccessException ex)
                {
                    MessageBox.Show(ex.Message, "Acceso denegado", MessageBoxButtons.OK, MessageBoxIcon.Warning);
                }
                catch (Exception ex)
                {
                    Debug.WriteLine(ex.ToString());
                    MessageBox.Show(
                        "No se pudo generar el resguardo de la base de datos.\n\n" + ex.Message,
                        "Error al generar resguardo",
                        MessageBoxButtons.OK,
                        MessageBoxIcon.Error);
                }
                finally
                {
                    EstablecerEstadoOcupado(false);
                }
            }
        }

        private void btnRestaurarRespaldo_Click(object sender, EventArgs e)
        {
            using (OpenFileDialog dialogo = new OpenFileDialog())
            {
                dialogo.Title = "Seleccionar archivo de resguardo";
                dialogo.Filter = "Archivos de respaldo (*.bak)|*.bak";
                dialogo.CheckFileExists = true;
                dialogo.Multiselect = false;

                string rutaInicial = ObtenerRutaInicialDialogo();
                if (!string.IsNullOrEmpty(rutaInicial))
                {
                    dialogo.InitialDirectory = rutaInicial;
                }

                if (dialogo.ShowDialog(this) != DialogResult.OK)
                {
                    return;
                }

                DialogResult confirmacion = MessageBox.Show(
                    $"Esta operación sobrescribirá todos los datos actuales de la base de datos '{_nombreBaseActual}' con el contenido del archivo:\n\n" +
                    $"{dialogo.FileName}\n\n" +
                    "Los cambios no respaldados se perderán de forma permanente y la sesión se cerrará al finalizar.\n\n" +
                    "¿Desea continuar con la restauración?",
                    "Confirmar restauración de base de datos",
                    MessageBoxButtons.YesNo,
                    MessageBoxIcon.Warning,
                    MessageBoxDefaultButton.Button2);

                if (confirmacion != DialogResult.Yes)
                {
                    return;
                }

                CerrarFormulariosHijosAbiertos();

                EstablecerEstadoOcupado(true);
                try
                {
                    _respaldoServicio.RestaurarRespaldo(dialogo.FileName);
                }
                catch (UnauthorizedAccessException ex)
                {
                    EstablecerEstadoOcupado(false);
                    MessageBox.Show(ex.Message, "Acceso denegado", MessageBoxButtons.OK, MessageBoxIcon.Warning);
                    return;
                }
                catch (Exception ex)
                {
                    EstablecerEstadoOcupado(false);
                    Debug.WriteLine(ex.ToString());

                    if (EsAdvertenciaSingleUser(ex))
                    {
                        string detalle = ex.Message;
                        if (ex.InnerException != null && !string.IsNullOrWhiteSpace(ex.InnerException.Message))
                        {
                            detalle += "\n\nDetalle del error original:\n" + ex.InnerException.Message;
                        }

                        MessageBox.Show(
                            "ADVERTENCIA CRÍTICA: La base de datos podría haber quedado en modo SINGLE_USER.\n\n" +
                            detalle +
                            "\n\nVerifique el estado de la base de datos en SQL Server antes de continuar operando.",
                            "Advertencia crítica - Base en modo SINGLE_USER",
                            MessageBoxButtons.OK,
                            MessageBoxIcon.Warning);
                    }
                    else
                    {
                        MessageBox.Show(
                            "No se pudo restaurar la base de datos.\n\n" + ex.Message,
                            "Error al restaurar resguardo",
                            MessageBoxButtons.OK,
                            MessageBoxIcon.Error);
                    }

                    return;
                }

                EstablecerEstadoOcupado(false);

                MessageBox.Show(
                    "La restauración de la base de datos finalizó correctamente y la base se encuentra disponible en modo multiusuario.\n\n" +
                    "A continuación se cerrará la sesión para reiniciar el contexto de la aplicación.",
                    "Restauración completada",
                    MessageBoxButtons.OK,
                    MessageBoxIcon.Information);

                FrmPrincipal principal = this.Owner as FrmPrincipal
                    ?? Application.OpenForms.OfType<FrmPrincipal>().FirstOrDefault();

                this.Close();

                if (principal != null && !principal.IsDisposed)
                {
                    principal.CerrarSesionPostRestauracion();
                }
            }
        }

        private void CerrarFormulariosHijosAbiertos()
        {
            List<Form> abiertos = Application.OpenForms
                .Cast<Form>()
                .Where(f => f != null && !f.IsDisposed && f != this && !(f is FrmPrincipal))
                .ToList();

            foreach (Form formulario in abiertos)
            {
                formulario.Close();
            }
        }

        private void EstablecerEstadoOcupado(bool ocupado)
        {
            this.UseWaitCursor = ocupado;
            this.btnGenerarRespaldo.Enabled = !ocupado;
            this.btnRestaurarRespaldo.Enabled = !ocupado;
            this.btnCerrar.Enabled = !ocupado;
            Application.DoEvents();
        }

        private static bool EsAdvertenciaSingleUser(Exception ex)
        {
            for (Exception actual = ex; actual != null; actual = actual.InnerException)
            {
                if (!string.IsNullOrEmpty(actual.Message)
                    && actual.Message.IndexOf("SINGLE_USER", StringComparison.OrdinalIgnoreCase) >= 0)
                {
                    return true;
                }
            }

            return false;
        }
    }
}
