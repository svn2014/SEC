using System;
using System.Data;
using System.Collections.Generic;

namespace Security
{
    public partial class HistoryDataVendorCH : AHistoryDataVendor
    {
        #region 日历数据
        public override void LoadTradingDate(AHistoryTimeSeries ts)
        {
            try
            {
                //数据已经存在
                if (_ExchangeTradingDays == null || _TimeSeriesStart != ts.TimeSeriesStartExtended || _TimeSeriesEnd != ts.TimeSeriesEnd)
                {
                    _TimeSeriesStart = ts.TimeSeriesStartExtended;
                    _TimeSeriesEnd = ts.TimeSeriesEnd;

                    string sql = @"SELECT Tdate AS " + C_ColName_TradeDate + @" 
                                FROM TRADEDATE 
                                WHERE Exchange = 'CNSESH' 
                                AND TDate>='" + _TimeSeriesStart.ToString("yyyyMMdd")
                               + @"' AND TDate<='" + _TimeSeriesEnd.ToString("yyyyMMdd") + "' ORDER BY TDate Desc";

                    _ExchangeTradingDays = base.DBInstance.ExecuteSQL(sql);

                    sql = @"SELECT Tdate AS " + C_ColName_TradeDate + @" 
                                FROM TRADEDATE 
                                WHERE Exchange = 'CNIBEX' 
                                AND TDate>='" + _TimeSeriesStart.ToString("yyyyMMdd")
                               + @"' AND TDate<='" + _TimeSeriesEnd.ToString("yyyyMMdd") + "' ORDER BY TDate Desc";

                    _InnerBankTradingDays = base.DBInstance.ExecuteSQL(sql);
                }

                //更新数据: Exchange
                ts.ExchangeTradingDates.Clear();
                if (_ExchangeTradingDays == null || _ExchangeTradingDays.Tables.Count == 0 || _ExchangeTradingDays.Tables[0].Rows.Count == 0)
                    return;

                foreach (DataRow row in _ExchangeTradingDays.Tables[0].Rows)
                {
                    DateTime tradedate = DataManager.ConvertToDate(row[C_ColName_TradeDate]);
                    ts.ExchangeTradingDates.Add(tradedate);
                }

                //更新数据: InnerBank
                ts.InnerBankTradingDates.Clear();
                if (_InnerBankTradingDays == null || _InnerBankTradingDays.Tables.Count == 0 || _InnerBankTradingDays.Tables[0].Rows.Count == 0)
                    return;

                foreach (DataRow row in _InnerBankTradingDays.Tables[0].Rows)
                {
                    DateTime tradedate = DataManager.ConvertToDate(row[C_ColName_TradeDate]);
                    ts.InnerBankTradingDates.Add(tradedate);
                }
            }
            catch (Exception ex)
            {
                throw ex;
            }
        }
        #endregion

        #region 扩展方法
        public HistoryDataVendorCH(ADBInstance instance)
        {
            base.DBInstance = instance;
            base.DataSource = "财汇";
        }
        private SecExchange GetExchange(string exchangename)
        {
            switch (exchangename)
            {
                case "CNSESH":
                    return SecExchange.SHE;
                case "CNSESZ":
                    return SecExchange.SZE; 
                case "CNIBEX":
                    return SecExchange.IBM;
                case "CNFF00":
                    return SecExchange.FFE;
                case "CNFEDC":
                    return SecExchange.DFE;
                case "CNFEZC":
                    return SecExchange.ZFE;
                case "CNFESF":
                    return SecExchange.SFE;
                default:
                    return SecExchange.OTC;
            }
        }
        #endregion
    }
}
