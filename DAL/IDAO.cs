using System;
using System.Collections.Generic;
using System.Data;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace DAL
{
    /// Contrato del Facade de acceso a datos. Define las operaciones disponibles
    /// para hablar con la base de datos, sin exponer detalles de ADO.NET.
    internal interface IDAO
    {
        DataSet ExecuteDataSetStoredProcedure(string Nombre, Dictionary<string, object> Parameters = null, CommandType TypeCommand = CommandType.StoredProcedure);
        void ExecuteNonQueryStoredProcedure(string Nombre, Dictionary<string, object> Parameters = null, CommandType TypeCommand = CommandType.StoredProcedure);
        object ExecuteScalarStoredProcedure(string Nombre, Dictionary<string, object> Parameters = null, CommandType TypeCommand = CommandType.StoredProcedure);
    }
}
