using System;
using System.Collections.Generic;

namespace Security
{
    public enum DatabaseType
    {
        Oracle
    }

    public enum HistoryDataVendor
    {
        CaiHui,
        Sina
    }

    public enum RealtimeDataVendor
    {
        Sina
    }

    public class DataManager
    {
        #region 构建数据
        private static AHistoryDataVendor GetHistoryDataVendor(HistoryDataVendor vendor, ADBInstance dbinstance)
        {
            switch (vendor)
            {
                case HistoryDataVendor.CaiHui:
                    return new HistoryDataVendorCH(dbinstance);

                default:
                    throw new NotImplementedException();
            }
        }
        private static ARealtimeDataVendor GetRealtimeDataVendor(RealtimeDataVendor vendor, ADBInstance dbinstance)
        {
            switch (vendor)
            {
                case RealtimeDataVendor.Sina:
                    return new RealtimeDataVendorSina();

                default:
                    throw new NotImplementedException();
            }
        }
        private static ADBInstance GetDBInstance(DatabaseType type, string conn)
        {
            if (conn == null || conn.Length == 0)
                return null;

            switch (type)
            {
                case DatabaseType.Oracle:
                    return new DBInstanceOracle(conn);
                    
                default:
                    throw new NotImplementedException();
            }
        }

        private static AHistoryDataVendor _HistoryDataVendor;
        private static ARealtimeDataVendor _RealtimeDataVendor;
        public static void Initiate(HistoryDataVendor historyvendor, string historyconnstr, RealtimeDataVendor realtimevendor, string realtimeconnstr, DatabaseType type)
        {
            ADBInstance dbhistory = GetDBInstance(type, historyconnstr);
            ADBInstance dbrealtime = GetDBInstance(type, realtimeconnstr);

            _HistoryDataVendor = GetHistoryDataVendor(historyvendor, dbhistory);
            _RealtimeDataVendor = GetRealtimeDataVendor(realtimevendor, dbrealtime);
        }
        public static AHistoryDataVendor GetHistoryDataVendor()
        {
            if (_HistoryDataVendor == null)
            {
                ADBInstance dbinst = null;
                HistoryDataVendor vendor = HistoryDataVendor.CaiHui;
                string conn = Properties.Settings.Default.HistoryDBConn;
                string strdbtype = Properties.Settings.Default.HistoryDBType;
                string strvendor = Properties.Settings.Default.HistoryDataVendor;

                try
                {
                    dbinst = GetDBInstance((DatabaseType)Enum.Parse(typeof(DatabaseType), strdbtype, true), conn);
                    vendor = (HistoryDataVendor)Enum.Parse(typeof(HistoryDataVendor), strvendor, true);

                    _HistoryDataVendor = GetHistoryDataVendor(vendor, dbinst);
                }
                catch (Exception)
                {
                    throw new Exception(Message.C_Msg_DB5);
                }
            }            

            return _HistoryDataVendor;
        }
        public static ARealtimeDataVendor GetRealtimeDataVendor()
        {
            if (_RealtimeDataVendor == null)
            {
                ADBInstance dbinst = null;
                RealtimeDataVendor vendor = RealtimeDataVendor.Sina;
                string conn = Properties.Settings.Default.RealtimeDBConn;
                string strdbtype = Properties.Settings.Default.RealtimeDBType;
                string strvendor = Properties.Settings.Default.RealtimeDataVendor;

                try
                {
                    dbinst = GetDBInstance((DatabaseType)Enum.Parse(typeof(DatabaseType), strdbtype, true), conn);
                    vendor = (RealtimeDataVendor)Enum.Parse(typeof(RealtimeDataVendor), strvendor, true);

                    _RealtimeDataVendor = GetRealtimeDataVendor(vendor, dbinst);
                }
                catch (Exception)
                {
                    throw new Exception(Message.C_Msg_DB6);
                }
            }

            return _RealtimeDataVendor;
        }
        #endregion

        #region 常用函数
        public static readonly DateTime C_Null_Date = new DateTime(1900, 1, 1);
        public static DateTime ConvertToDate(object obj)
        {
            if (obj == null || obj == DBNull.Value || obj.ToString().Length == 0)
                return C_Null_Date;

            try
            {
                string strDate = obj.ToString();
                if (strDate.Length == 8)
                    return new DateTime(Convert.ToInt16(strDate.Substring(0, 4)),
                                        Convert.ToInt16(strDate.Substring(4, 2)),
                                        Convert.ToInt16(strDate.Substring(6, 2)));
                else
                    return Convert.ToDateTime(obj);
            }
            catch (Exception ex)
            {
                throw ex;
            }                
        }
        public static double ConvertToDouble(object obj)
        {
            if (obj == null || obj == DBNull.Value)
                return 0;

            try
            {
                string strobj = obj.ToString();
                if (strobj.Contains("%"))
                {
                    strobj = strobj.Replace("%", "");
                    return Convert.ToDouble(strobj) / 100;
                }
                else
                    return Convert.ToDouble(obj);
            }
            catch (Exception ex)
            {
                throw ex;
            }
        }
        public static int ConvertToInt(object obj)
        {
            if (obj == null || obj == DBNull.Value)
                return 0;

            try
            {
                return Convert.ToInt32(obj);
            }
            catch (Exception ex)
            {
                throw ex;
            }
        }

        public static List<DateTime> GetTradingDays(DateTime start, DateTime end)
        {
            List<DateTime> innerbanktradingdays = new List<DateTime>();
            return GetTradingDays(start, end, ref innerbanktradingdays);
        }
        public static List<DateTime> GetTradingDays(DateTime start, DateTime end, ref List<DateTime> innerbankTradingDays)
        { 
            Index idx = new Index("000001");
            idx.SetDatePeriod(start, end);
            idx.LoadData(DataInfoType.HistoryTradePrice);

            //银行间交易日
            innerbankTradingDays = idx.HistoryTradePrice.InnerBankTradingDates.FindAll(delegate(DateTime d) { return d >= start; }); ;
            //交易所交易日
            List<DateTime> exchangeTradingDays = idx.HistoryTradePrice.ExchangeTradingDates.FindAll(delegate(DateTime d) { return d >= start; }); ;
            return exchangeTradingDays;
        }
        #endregion
    }
}
