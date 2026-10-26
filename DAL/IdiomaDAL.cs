using BE;
using System;
using System.Collections.Generic;
using System.Data;

namespace DAL
{
    public class IdiomaDAL
    {
        static int UltimoId;
        private static int ProximoId()
        {
            DAO Dao = new DAO();
            if (UltimoId == 0) UltimoId = int.Parse(Dao.ExecuteScalarStoredProcedure("SP_Idioma_ObtenerUltimoId", null, CommandType.StoredProcedure).ToString());
            UltimoId += 1;
            return UltimoId;
        }
        public void Registrar(IdiomaBE Pidioma)
        {
            DAO Dao = new DAO();
            Pidioma.Id = ProximoId();
            Dictionary<string, object> Parametros = new Dictionary<string, object>
            {
                { "@Id", Pidioma.Id },
                { "@Codigo", Pidioma.Codigo },
                { "@Nombre", Pidioma.Nombre },
                { "@ArchivoJson", Pidioma.ArchivoJson }
            };
            Dao.ExecuteNonQueryStoredProcedure("SP_Idioma_Registrar", Parametros, CommandType.StoredProcedure);
        }
        public void ActualizarDatos(IdiomaBE Pidioma)
        {
            DAO Dao = new DAO();
            Dictionary<string, object> Parametros = new Dictionary<string, object>
            {
                { "@Id", Pidioma.Id },
                { "@Nombre", Pidioma.Nombre },
                { "@ArchivoJson", Pidioma.ArchivoJson }
            };
            Dao.ExecuteNonQueryStoredProcedure("SP_Idioma_ActualizarDatos", Parametros, CommandType.StoredProcedure);
        }
        public void EstablecerPredeterminado(int Pid)
        {
            DAO Dao = new DAO();
            Dictionary<string, object> Parametros = new Dictionary<string, object> { { "@Id", Pid } };
            Dao.ExecuteNonQueryStoredProcedure("SP_Idioma_EstablecerPredeterminado", Parametros, CommandType.StoredProcedure);
        }
        public void Eliminar(int Pid)
        {
            DAO Dao = new DAO();
            Dictionary<string, object> Parametros = new Dictionary<string, object> { { "@Id", Pid } };
            Dao.ExecuteNonQueryStoredProcedure("SP_Idioma_Eliminar", Parametros, CommandType.StoredProcedure);
        }
        public List<IdiomaBE> Listar()
        {
            DAO Dao = new DAO();
            DataSet Ds = Dao.ExecuteDataSetStoredProcedure("SP_Idioma_Listar", null, CommandType.StoredProcedure);
            List<IdiomaBE> Idiomas = new List<IdiomaBE>();
            foreach (DataRow Fila in Ds.Tables[0].Rows)
            {
                Idiomas.Add(MapearFila(Fila));
            }
            return Idiomas;
        }
        public IdiomaBE ObtenerDatos(int Pid)
        {
            DAO Dao = new DAO();
            Dictionary<string, object> Parametros = new Dictionary<string, object> { { "@Id", Pid } };
            DataSet Ds = Dao.ExecuteDataSetStoredProcedure("SP_Idioma_ObtenerDatos", Parametros, CommandType.StoredProcedure);
            if (Ds.Tables[0].Rows.Count == 0) return null;
            return MapearFila(Ds.Tables[0].Rows[0]);
        }
        public IdiomaBE ObtenerPredeterminado()
        {
            DAO Dao = new DAO();
            DataSet Ds = Dao.ExecuteDataSetStoredProcedure("SP_Idioma_ObtenerPredeterminado", null, CommandType.StoredProcedure);
            if (Ds.Tables[0].Rows.Count == 0) return null;
            return MapearFila(Ds.Tables[0].Rows[0]);
        }
        public bool ExistePorCodigo(string Pcodigo)
        {
            DAO Dao = new DAO();
            Dictionary<string, object> Parametros = new Dictionary<string, object> { { "@Codigo", Pcodigo } };
            object Resultado = Dao.ExecuteScalarStoredProcedure("SP_Idioma_ExistePorCodigo", Parametros, CommandType.StoredProcedure);
            return Convert.ToInt32(Resultado) > 0;
        }
        /// El listado no trae ArchivoJson (ver SP_Idioma_Listar), por eso
        /// la columna se lee solo si viene en la fila.
        private IdiomaBE MapearFila(DataRow Fila)
        {
            return new IdiomaBE
            {
                Id = Convert.ToInt32(Fila["Id"]),
                Codigo = Fila["Codigo"].ToString(),
                Nombre = Fila["Nombre"].ToString(),
                ArchivoJson = Fila.Table.Columns.Contains("ArchivoJson") ? Fila["ArchivoJson"].ToString() : null,
                EsPredeterminado = Convert.ToBoolean(Fila["EsPredeterminado"])
            };
        }
    }
}
