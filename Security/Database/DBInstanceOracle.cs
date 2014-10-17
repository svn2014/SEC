using System;
using System.Data;
using System.Data.OracleClient;

namespace Security
{
    public class DBInstanceOracle : ADBInstance
    {
        public DBInstanceOracle(string connString)
        {
            try
            {
                _DBConnection = new OracleConnection(connString);
                _DBConnection.Open();
            }
            catch (Exception ex)
            {
                throw new Exception(Message.C_Msg_DB2, ex);
            }
        }

        public override DataSet ExecuteSQL(string sql)
        {
            try
            {
                _DBCommand = new OracleCommand();
                _DBCommand.Connection = (OracleConnection)_DBConnection;
                _DBCommand.CommandType = CommandType.Text;
                _DBCommand.CommandText = sql;

                DataSet ds = new DataSet();
                OracleDataAdapter da = new OracleDataAdapter((OracleCommand)_DBCommand);
                OracleCommandBuilder cb = new OracleCommandBuilder(da);

                da.Fill(ds);

                return ds;
            }
            catch (Exception ex)
            {
                throw new Exception(Message.C_Msg_DB3, ex);
            }
        }

        public override void UpdateDB(string sql, DataTable dt)
        {
            try
            {
                OracleDataAdapter da = new OracleDataAdapter(sql, (OracleConnection)_DBConnection);
                OracleCommandBuilder cb = new OracleCommandBuilder(da);
                da.Update(dt);
            }
            catch (Exception ex)
            {
                throw new Exception(Message.C_Msg_DB4, ex);
            }
        }

        public override string ConvertToSQLDate(DateTime date)
        {
            string sql = "to_date('" + date.ToString("yyyyMMdd") + "','yyyyMMdd')";
            return sql;
        }

        public override string ConvertToSQLDate(string fieldname)
        {
            string sql = "to_date(" + fieldname + ",'yyyyMMdd')";
            return sql;
        }
    }
}
