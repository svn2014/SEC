using System;
using System.Data;
using System.Data.Common;

namespace Security
{
    public abstract class ADBInstance
    {
        protected DbConnection _DBConnection = null;
        protected DbCommand _DBCommand = null;

        public abstract DataSet ExecuteSQL(string sql);
        public abstract void UpdateDB(string sql, DataTable dt);
        public abstract string ConvertToSQLDate(DateTime date);
        public abstract string ConvertToSQLDate(string fieldname);

        public void Close()
        {
            if (_DBConnection.State != ConnectionState.Closed)
            {
                try
                {
                    _DBConnection.Close();
                }
                catch (Exception ex)
                {
                    throw new Exception(Message.C_Msg_DB1, ex);
                }
            }
        }
    }
}
