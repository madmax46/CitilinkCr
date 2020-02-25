using MySql.Data.MySqlClient;
using System;
using System.Collections.Generic;
using System.Data;
using System.Data.Common;
using System.Linq;
using System.Text;

namespace MySqlUtils
{
    public class MySqlWrap : IDbProvider
    {
        private string _connectionStr;

        public string ConnectionStr { get => _connectionStr; private set => _connectionStr = value; }
        public MySqlConfig ConnectConfig { get; private set; }
        public MySqlWrap(MySqlConfig config)
        {
            ConnectConfig = config;
            ConnectionStr = ConnectConfig.CreateConnectionString();
        }
        public DataTable ProcedureByName(string procedure, params object[] par)
        {
            string parPart = string.Join(",", par.Select(r => ToMySqlParameters(r)).ToArray());
            string sql = string.Format("CALL {0}({1})", procedure, parPart);
            using (MySqlConnection connect = new MySqlConnection(ConnectionStr))
            {
                connect.Open();
                using (MySqlDataAdapter adapter = new MySqlDataAdapter(sql, connect))
                {
                    DataTable dataTable = new DataTable();
                    adapter.Fill(dataTable);
                    return dataTable;
                }
            }
        }

        public DataSet ProcedureDsByName(string procedure, params object[] par)
        {
            string parPart = string.Join(",", par.Select(r => string.Format("{0}", Convert.ToString(r))).ToArray());
            string sql = string.Format("CALL {0}({1})", procedure, parPart);
            using (MySqlConnection connect = new MySqlConnection(ConnectionStr))
            {
                connect.Open();
                using (MySqlDataAdapter adapter = new MySqlDataAdapter(sql, connect))
                {
                    DataSet dataset = new DataSet();
                    adapter.Fill(dataset);
                    return dataset;
                }
            }
        }


        public DataTable GetDataTable(string sqlQuery)
        {
            using (MySqlConnection connect = new MySqlConnection(ConnectionStr))
            {
                connect.Open();
                using (MySqlDataAdapter adapter = new MySqlDataAdapter(sqlQuery, connect))
                {
                    DataTable dataTable = new DataTable();
                    adapter.Fill(dataTable);
                    return dataTable;
                }
            }
        }
        public int Execute(string query)
        {
            using (MySqlConnection connect = new MySqlConnection(ConnectionStr))
            {
                connect.Open();
                using (MySqlCommand com = new MySqlCommand(query, connect))
                {
                    return com.ExecuteNonQuery();
                }
            }
        }

        public string ToSqlParam(object param)
        {
            return ToMySqlParameters(param);
        }

        public static string ToMySqlParameters(object param)
        {
            if (param is double || param is decimal || param is float)
                return ToMySqlParameters(param.ToString().Replace(',', '.'));

            if (param is int || param is long)
                return param.ToString();

            if (param is Enum)
                return ToMySqlParameters(Convert.ToInt32(param));

            if (param is DateTime)
                return ToMySqlParameters(((DateTime)param).ToString("yyyy-MM-dd HH:mm:ss"));

            if (param is TimeSpan)
            {
                var ts = (TimeSpan)param;
                return ToMySqlParameters($"{Convert.ToInt32(ts.TotalHours)}:{ts.Minutes}:{ts.Seconds}");
            }

            if (param as string != null)
            {
                string retVal = Convert.ToString(param);
                retVal = retVal.Replace("\'", "'").Replace("'", "\'");
                return string.Format("'{0}'", retVal);
            }

            if (param == null)
                return "NULL";

            if (param is bool)
                return Convert.ToBoolean(param) == true ? ToMySqlParameters(1) : ToMySqlParameters(0);

            return ToMySqlParameters(param.ToString());
        }

    }
}
