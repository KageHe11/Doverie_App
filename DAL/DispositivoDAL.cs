using System;
using System.Collections.Generic;
using System.Data;

namespace DAL
{
    /// Dispositivos conocidos de cada Usuario, para detectar inicios de
    /// sesión desde un dispositivo nuevo.
    public class DispositivoDAL
    {
        public bool Existe(int PusuarioId, string Pidentificador)
        {
            DAO Dao = new DAO();
            Dictionary<string, object> Parametros = new Dictionary<string, object>
            {
                { "@UsuarioId", PusuarioId },
                { "@Identificador", Pidentificador }
            };
            return Convert.ToInt32(Dao.ExecuteScalarStoredProcedure("SP_Dispositivo_Existe", Parametros, CommandType.StoredProcedure)) > 0;
        }
        public int Contar(int PusuarioId)
        {
            DAO Dao = new DAO();
            Dictionary<string, object> Parametros = new Dictionary<string, object> { { "@UsuarioId", PusuarioId } };
            return Convert.ToInt32(Dao.ExecuteScalarStoredProcedure("SP_Dispositivo_Contar", Parametros, CommandType.StoredProcedure));
        }
        public void Registrar(int PusuarioId, string Pidentificador, string Pdescripcion)
        {
            DAO Dao = new DAO();
            Dictionary<string, object> Parametros = new Dictionary<string, object>
            {
                { "@UsuarioId", PusuarioId },
                { "@Identificador", Pidentificador },
                { "@Descripcion", (object)Pdescripcion ?? DBNull.Value }
            };
            Dao.ExecuteNonQueryStoredProcedure("SP_Dispositivo_Registrar", Parametros, CommandType.StoredProcedure);
        }
    }
}
