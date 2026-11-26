using GymFlow.Abstracciones;
using Microsoft.Data.SqlClient;
using System;
using System.Collections.Generic;
using System.Data;
using System.IO;
using System.Runtime.ExceptionServices;
using System.Text.RegularExpressions;

namespace GymFlow.Repositorio
{
    public class RespaldoRepositorio : IRespaldoRepositorio
    {
        private static readonly Regex RegexNombreBaseValido = new Regex("^[A-Za-z0-9_]+$", RegexOptions.Compiled);

        private readonly string _nombreBase;
        private readonly string _cadenaConexionMaster;

        public RespaldoRepositorio(string cadenaConexion)
        {
            if (string.IsNullOrWhiteSpace(cadenaConexion))
            {
                throw new ArgumentException("La cadena de conexión no puede estar vacía.", nameof(cadenaConexion));
            }

            SqlConnectionStringBuilder builder = new SqlConnectionStringBuilder(cadenaConexion);
            string nombreBase = builder.InitialCatalog;

            if (string.IsNullOrWhiteSpace(nombreBase))
            {
                throw new ArgumentException("La cadena de conexión no especifica una base de datos inicial (InitialCatalog).", nameof(cadenaConexion));
            }

            if (!RegexNombreBaseValido.IsMatch(nombreBase))
            {
                throw new ArgumentException($"El nombre de base de datos '{nombreBase}' no es válido.", nameof(cadenaConexion));
            }

            _nombreBase = nombreBase;

            SqlConnectionStringBuilder masterBuilder = new SqlConnectionStringBuilder(cadenaConexion)
            {
                InitialCatalog = "master"
            };
            _cadenaConexionMaster = masterBuilder.ConnectionString;
        }

        public string ObtenerNombreBaseActual()
        {
            return _nombreBase;
        }

        public string ObtenerRutaPorDefecto()
        {
            using (SqlConnection conexion = new SqlConnection(_cadenaConexionMaster))
            {
                conexion.Open();

                using (SqlCommand comando = new SqlCommand("SELECT CAST(SERVERPROPERTY('InstanceDefaultBackupPath') AS NVARCHAR(4000))", conexion))
                {
                    object resultado = comando.ExecuteScalar();
                    if (resultado == null || resultado == DBNull.Value)
                    {
                        return string.Empty;
                    }

                    string ruta = Convert.ToString(resultado)?.Trim();
                    return string.IsNullOrWhiteSpace(ruta) ? string.Empty : ruta;
                }
            }
        }

        public void GenerarRespaldo(string rutaArchivo)
        {
            if (string.IsNullOrWhiteSpace(rutaArchivo))
            {
                throw new ArgumentException("Debe especificar una ruta de archivo válida para generar el resguardo.", nameof(rutaArchivo));
            }

            try
            {
                using (SqlConnection conexion = new SqlConnection(_cadenaConexionMaster))
                {
                    conexion.Open();

                    string sql = $"BACKUP DATABASE [{_nombreBase}] TO DISK = @ruta WITH INIT, CHECKSUM, STATS = 10";
                    using (SqlCommand comando = new SqlCommand(sql, conexion))
                    {
                        comando.CommandTimeout = 0;
                        comando.Parameters.Add(new SqlParameter("@ruta", SqlDbType.NVarChar, 4000) { Value = rutaArchivo });
                        comando.ExecuteNonQuery();
                    }
                }
            }
            catch (SqlException ex) when (EsErrorAccesoDenegadoSO(ex))
            {
                throw new InvalidOperationException(
                    "SQL Server no tiene permisos de escritura en la carpeta seleccionada. " +
                    "Verifique que la cuenta del servicio SQL Server (NT SERVICE\\MSSQL$SQLEXPRESS) tenga permisos de escritura sobre la carpeta destino o seleccione otra ubicación.",
                    ex);
            }
        }

        public void RestaurarRespaldo(string rutaArchivo)
        {
            if (string.IsNullOrWhiteSpace(rutaArchivo))
            {
                throw new ArgumentException("Debe especificar una ruta de archivo válida para restaurar el resguardo.", nameof(rutaArchivo));
            }

            if (!File.Exists(rutaArchivo))
            {
                throw new FileNotFoundException("El archivo de resguardo especificado no existe.", rutaArchivo);
            }

            if (!string.Equals(Path.GetExtension(rutaArchivo), ".bak", StringComparison.OrdinalIgnoreCase))
            {
                throw new ArgumentException("El archivo de resguardo debe tener extensión .bak.", nameof(rutaArchivo));
            }

            int posicion = ValidarYVerificarRespaldo(rutaArchivo);

            SqlConnection.ClearAllPools();

            bool enSingleUser = false;
            Exception errorRestauracion = null;
            Exception errorMultiUser = null;

            try
            {
                using (SqlConnection conexionRestore = new SqlConnection(_cadenaConexionMaster))
                {
                    conexionRestore.Open();

                    try
                    {
                        enSingleUser = true;

                        using (SqlCommand cmdSingle = new SqlCommand($"ALTER DATABASE [{_nombreBase}] SET SINGLE_USER WITH ROLLBACK IMMEDIATE", conexionRestore))
                        {
                            cmdSingle.CommandTimeout = 0;
                            cmdSingle.ExecuteNonQuery();
                        }

                        string sqlRestore = $"RESTORE DATABASE [{_nombreBase}] FROM DISK = @ruta WITH FILE = @posicion, REPLACE";
                        using (SqlCommand cmdRestore = new SqlCommand(sqlRestore, conexionRestore))
                        {
                            cmdRestore.CommandTimeout = 0;
                            cmdRestore.Parameters.Add(new SqlParameter("@ruta", SqlDbType.NVarChar, 4000) { Value = rutaArchivo });
                            cmdRestore.Parameters.Add(new SqlParameter("@posicion", SqlDbType.Int) { Value = posicion });
                            cmdRestore.ExecuteNonQuery();
                        }
                    }
                    catch (Exception ex)
                    {
                        errorRestauracion = ex;
                    }
                    finally
                    {
                        if (enSingleUser)
                        {
                            errorMultiUser = IntentarRestaurarMultiUser(conexionRestore);
                        }
                    }
                }
            }
            finally
            {
                SqlConnection.ClearAllPools();
            }

            if (errorRestauracion != null && errorMultiUser != null)
            {
                throw new InvalidOperationException(
                    "Falló la restauración de la base de datos y no se pudo restablecer el modo MULTI_USER. " +
                    $"La base de datos '{_nombreBase}' podría haber quedado en modo SINGLE_USER. " +
                    $"Error al restablecer MULTI_USER: {errorMultiUser.Message}",
                    errorRestauracion);
            }

            if (errorMultiUser != null)
            {
                throw new InvalidOperationException(
                    "La restauración finalizó, pero no se pudo confirmar el retorno de la base de datos al modo MULTI_USER. " +
                    $"La base de datos '{_nombreBase}' podría haber quedado en modo SINGLE_USER.",
                    errorMultiUser);
            }

            if (errorRestauracion != null)
            {
                if (errorRestauracion is SqlException sqlEx && EsErrorAccesoDenegadoSO(sqlEx))
                {
                    throw new InvalidOperationException(
                        "SQL Server no tiene permisos de lectura sobre el archivo de resguardo seleccionado. " +
                        "Verifique que la cuenta del servicio SQL Server (NT SERVICE\\MSSQL$SQLEXPRESS) pueda acceder a la ubicación del archivo.",
                        sqlEx);
                }

                ExceptionDispatchInfo.Capture(errorRestauracion).Throw();
            }
        }

        private int ValidarYVerificarRespaldo(string rutaArchivo)
        {
            try
            {
                using (SqlConnection conexion = new SqlConnection(_cadenaConexionMaster))
                {
                    conexion.Open();

                    List<(int BackupType, string DatabaseName, int Position)> encabezados = new List<(int, string, int)>();

                    using (SqlCommand cmdHeader = new SqlCommand("RESTORE HEADERONLY FROM DISK = @ruta", conexion))
                    {
                        cmdHeader.CommandTimeout = 0;
                        cmdHeader.Parameters.Add(new SqlParameter("@ruta", SqlDbType.NVarChar, 4000) { Value = rutaArchivo });

                        using (SqlDataReader reader = cmdHeader.ExecuteReader())
                        {
                            int ordinalBackupType = reader.GetOrdinal("BackupType");
                            int ordinalDatabaseName = reader.GetOrdinal("DatabaseName");
                            int ordinalPosition = reader.GetOrdinal("Position");

                            while (reader.Read())
                            {
                                int backupType = reader.IsDBNull(ordinalBackupType) ? -1 : Convert.ToInt32(reader.GetValue(ordinalBackupType));
                                string databaseName = reader.IsDBNull(ordinalDatabaseName) ? string.Empty : Convert.ToString(reader.GetValue(ordinalDatabaseName));
                                int position = reader.IsDBNull(ordinalPosition) ? -1 : Convert.ToInt32(reader.GetValue(ordinalPosition));

                                encabezados.Add((backupType, databaseName, position));
                            }
                        }
                    }

                    if (encabezados.Count == 0)
                    {
                        throw new InvalidOperationException("El archivo seleccionado no contiene conjuntos de respaldo válidos.");
                    }

                    if (encabezados.Count > 1)
                    {
                        throw new InvalidOperationException("El archivo seleccionado contiene múltiples conjuntos de respaldo y resulta ambiguo para restaurar.");
                    }

                    var encabezado = encabezados[0];

                    if (encabezado.BackupType != 1)
                    {
                        throw new InvalidOperationException("El archivo seleccionado no corresponde a un respaldo completo de base de datos (BackupType = 1).");
                    }

                    if (!string.Equals(encabezado.DatabaseName, _nombreBase, StringComparison.OrdinalIgnoreCase))
                    {
                        throw new InvalidOperationException(
                            $"El respaldo pertenece a la base de datos '{encabezado.DatabaseName}' y no coincide con la base activa '{_nombreBase}'.");
                    }

                    if (encabezado.Position <= 0)
                    {
                        throw new InvalidOperationException("La posición del conjunto de respaldo en el archivo no es válida.");
                    }

                    using (SqlCommand cmdVerify = new SqlCommand("RESTORE VERIFYONLY FROM DISK = @ruta WITH FILE = @posicion, CHECKSUM", conexion))
                    {
                        cmdVerify.CommandTimeout = 0;
                        cmdVerify.Parameters.Add(new SqlParameter("@ruta", SqlDbType.NVarChar, 4000) { Value = rutaArchivo });
                        cmdVerify.Parameters.Add(new SqlParameter("@posicion", SqlDbType.Int) { Value = encabezado.Position });
                        cmdVerify.ExecuteNonQuery();
                    }

                    return encabezado.Position;
                }
            }
            catch (SqlException ex) when (EsErrorAccesoDenegadoSO(ex))
            {
                throw new InvalidOperationException(
                    "SQL Server no tiene permisos de lectura sobre el archivo de resguardo seleccionado. " +
                    "Verifique que la cuenta del servicio SQL Server (NT SERVICE\\MSSQL$SQLEXPRESS) pueda acceder a la ubicación del archivo.",
                    ex);
            }
        }

        private Exception IntentarRestaurarMultiUser(SqlConnection conexionOriginal)
        {
            string sqlMultiUser = $"ALTER DATABASE [{_nombreBase}] SET MULTI_USER";

            try
            {
                if (conexionOriginal.State != ConnectionState.Open)
                {
                    conexionOriginal.Open();
                }

                using (SqlCommand cmdMulti = new SqlCommand(sqlMultiUser, conexionOriginal))
                {
                    cmdMulti.CommandTimeout = 0;
                    cmdMulti.ExecuteNonQuery();
                }

                return null;
            }
            catch
            {
                try
                {
                    using (SqlConnection conexionReintento = new SqlConnection(_cadenaConexionMaster))
                    {
                        conexionReintento.Open();

                        using (SqlCommand cmdMulti = new SqlCommand(sqlMultiUser, conexionReintento))
                        {
                            cmdMulti.CommandTimeout = 0;
                            cmdMulti.ExecuteNonQuery();
                        }
                    }

                    return null;
                }
                catch (Exception exReintento)
                {
                    return exReintento;
                }
            }
        }

        private static bool EsErrorAccesoDenegadoSO(SqlException ex)
        {
            if (ex == null)
            {
                return false;
            }

            string mensaje = ex.Message ?? string.Empty;
            return mensaje.IndexOf("Operating system error 5", StringComparison.OrdinalIgnoreCase) >= 0
                || mensaje.IndexOf("Error del sistema operativo 5", StringComparison.OrdinalIgnoreCase) >= 0
                || mensaje.IndexOf("Access is denied", StringComparison.OrdinalIgnoreCase) >= 0
                || mensaje.IndexOf("Acceso denegado", StringComparison.OrdinalIgnoreCase) >= 0;
        }
    }
}
