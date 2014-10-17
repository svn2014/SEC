using System;

namespace Security
{
//    public partial class HistoryDataVendorCSV : AHistoryDataVendor
//    {
//        #region 常量

//        #endregion

//        #region 日历数据
//        public override void LoadTradingDate(AHistoryTimeSeries ts)
//        {
//            try
//            {
////                //数据已经存在
////                if (_TradingDays == null || _TimeSeriesStart != ts.TimeSeriesStartExtended || _TimeSeriesEnd != ts.TimeSeriesEnd)
////                {
////                    _TimeSeriesStart = ts.TimeSeriesStartExtended;
////                    _TimeSeriesEnd = ts.TimeSeriesEnd;

////                    string sql = @"SELECT Tdate
////                                FROM TRADEDATE 
////                                WHERE Exchange = 'CNSESH' 
////                                AND TDate>='" + _TimeSeriesStart.ToString("yyyyMMdd")
////                               + @"' AND TDate<='" + _TimeSeriesEnd.ToString("yyyyMMdd") + "' ORDER BY TDate Desc";

////                    _TradingDays = base.DBInstance.ExecuteSQL(sql);
////                }

////                //更新数据
////                ts.TradingDates.Clear();

////                if (_TradingDays == null || _TradingDays.Tables.Count == 0 || _TradingDays.Tables[0].Rows.Count == 0)
////                    return;

////                foreach (DataRow row in _TradingDays.Tables[0].Rows)
////                {
////                    DateTime tradedate = DataManager.ConvertToDate(row[C_ColName_TradeDate]);
////                    ts.TradingDates.Add(tradedate);
////                }
//            }
//            catch (Exception ex)
//            {
//                throw ex;
//            }
//        }
//        #endregion

//        #region 扩展方法
//        public HistoryDataVendorCSV(ADBInstance instance)
//        {
//            base.DBInstance = instance;
//            base.DataSource = "文件系统";
//        }
//        #endregion

//        public override void LoadEquityInfo(Equity e)
//        {
//            throw new NotImplementedException();
//        }

//        public override void LoadEquityPrice(ASecurityGroup g)
//        {
//            throw new NotImplementedException();
//        }

//        public override FundGroup GetMutualFunds(AFundCategory category)
//        {
//            throw new NotImplementedException();
//        }

//        public override void LoadMutualFundInfo(MutualFund f)
//        {
//            throw new NotImplementedException();
//        }

//        public override void LoadMutualFundNAV(ASecurityGroup g)
//        {
//            throw new NotImplementedException();
//        }

//        public override void LoadMutualFundPrice(ASecurityGroup g)
//        {
//            throw new NotImplementedException();
//        }

//        public override void LoadMutualFundReport(ASecurityGroup g)
//        {
//            throw new NotImplementedException();
//        }

//        public override void LoadIndexInfo(Index i)
//        {
//            throw new NotImplementedException();
//        }

//        public override void LoadIndexPrice(ASecurityGroup g)
//        {
//            throw new NotImplementedException();
//        }
//    }
}
