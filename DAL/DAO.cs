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
        public DataSet ExecuteDataSetStoredProcedure(string Nombre, Dictionary<string, object> Parameters = null, CommandType TypeCommand = CommandType.StoredProcedure)
        {
            try
            {
                SqlCommand Command = new SqlCommand(Nombre, Con);
                Command.CommandType = TypeCommand;
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
