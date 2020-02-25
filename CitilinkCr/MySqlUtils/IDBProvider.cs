using System;
using System.Data;

namespace MySqlUtils
{
    public interface IDbProvider
    {
        string ToSqlParam(object param);
        DataTable ProcedureByName(string procedure, params object[] par);

        DataSet ProcedureDsByName(string procedure, params object[] par);

        DataTable GetDataTable(string sqlQuery);

        int Execute(string query);

    }
}
