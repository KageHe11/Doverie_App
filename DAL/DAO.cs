using System;
using System.Collections.Generic;
using System.Data;
using System.Data.SqlClient;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Configuration;

namespace DAL
{
    /// Facade sobre ADO.NET. Centraliza y simplifica el acceso a SQL Server
    /// (SqlConnection, SqlCommand, SqlDataAdapter) en tres operaciones básicas.
    /// Las clases DAL específicas (UsuarioDAL, DocumentoDAL, etc.) heredan de esta clase
    /// y solo llaman a estos tres métodos, sin conocer los detalles de ADO.NET.
    public class DAO : IDAO
    {
        protected SqlConnection Con;
        public DAO()
        {
            string ConnectionString = ConfigurationManager.ConnectionStrings["BD_Doverie"].ConnectionString;
            Con = new SqlConnection(ConnectionString);
        }
        /// Tiempo máximo de cada comando, en segundos. Un backup o un restore
        /// tardan bastante más que una consulta.
        public int TiempoEspera { get; set; } = 30;
        /// Nombre de la base de la aplicación (Initial Catalog).
        public string NombreBase
        {
            get { return new SqlConnectionStringBuilder(Con.ConnectionString).InitialCatalog; }
        }
        /// Mismo servidor y credenciales, pero conectado a master: para
        /// restaurar la base de la aplicación sin estar conectado a ella.
        public static DAO Servidor()
        {
            DAO Dao = new DAO();
            SqlConnectionStringBuilder Datos = new SqlConnectionStringBuilder(Dao.Con.ConnectionString) { InitialCatalog = "master" };
            Dao.Con = new SqlConnection(Datos.ConnectionString);
            return Dao;
        }
        /// Descarta las conexiones del pool: después de un restore las que
        /// estaban abiertas quedaron cortadas.
        public static void ReiniciarConexiones()
        {
            SqlConnection.ClearAllPools();
        }
        public DataSet ExecuteDataSetStoredProcedure(string Nombre, Dictionary<string, object> Parameters = null, CommandType TypeCommand = CommandType.StoredProcedure)
        {
            try
            {
                SqlCommand Command = new SqlCommand(Nombre, Con);
                Command.CommandType = TypeCommand;
                Command.CommandTimeout = TiempoEspera;
                if (Parameters != null)
                {
                    foreach (var P in Parameters)
                    {
                        Command.Parameters.AddWithValue(P.Key, P.Value);
                    }
                }
                SqlDataAdapter Da = new SqlDataAdapter(Command);
                DataSet Ds = new DataSet();
                Da.Fill(Ds);
                return Ds;
            }
            catch (Exception)
            {
                throw;
            }
            finally
            {
                if (Con.State != ConnectionState.Closed) { Con.Close(); }
            }
        }

        public void ExecuteNonQueryStoredProcedure(string Nombre, Dictionary<string, object> Parameters = null, CommandType TypeCommand = CommandType.StoredProcedure)
        {
            try
            {
                SqlCommand Command = new SqlCommand(Nombre, Con);
                Command.CommandType = TypeCommand;
                Command.CommandTimeout = TiempoEspera;
                if (Parameters != null)
                {
                    foreach (var P in Parameters)
                    {
                        Command.Parameters.AddWithValue(P.Key, P.Value);
                    }
                }
                Con.Open();
                Command.ExecuteNonQuery();
            }
            catch (Exception)
            {
                throw;
            }
            finally
            {
                if (Con.State != ConnectionState.Closed) { Con.Close(); }
            }
        }

        public object ExecuteScalarStoredProcedure(string Nombre, Dictionary<string, object> Parameters = null, CommandType TypeCommand = CommandType.StoredProcedure)
        {
            try
            {
                SqlCommand Command = new SqlCommand(Nombre, Con);
                Command.CommandType = TypeCommand;
                Command.CommandTimeout = TiempoEspera;
                if (Parameters != null)
                {
                    foreach (var P in Parameters)
                    {
                        Command.Parameters.AddWithValue(P.Key, P.Value);
                    }
                }
                Con.Open();
                object Resultado = Command.ExecuteScalar();
                return Resultado;
            }
            catch (Exception)
            {
                throw;
            }
            finally
            {
                if (Con.State != ConnectionState.Closed) { Con.Close(); }
            }
        }
    }
}
