using System.Collections.Generic;
using System;
using System.Diagnostics;

namespace Security
{
    public class FundGroup: ASecurityGroup
    {
        #region 基础方法
        public FundGroup() : base(typeof(MutualFund)) { }
        public FundGroup(System.Type classtype) : base(classtype) { }
        public override void LoadData(DataInfoType type)
        {
            if (this.SecurityList == null || this.SecurityList.Count == 0)
                return;

            switch (type)
            {
                case DataInfoType.SecurityInfo:
                    base.LoadSecurityInfo();
                    break;

                //获得实时价格数据
                case DataInfoType.RealtimeTradePrice:
                    DataManager.GetRealtimeDataVendor().LoadTradePrice(this);
                    break;

                case DataInfoType.RealtimeFundNAV:
                    DataManager.GetRealtimeDataVendor().LoadNetAssetValue(this);
                    break;

                //获得交易日数据：以上证指数代替
                case DataInfoType.HistoryTradePrice:                    
                    base.LoadBaseIndex();
                    DataManager.GetHistoryDataVendor().LoadMutualFundPrice(this, true);
                    break;
                    
                case DataInfoType.HistoryFundNAV:
                    base.LoadBaseIndex();
                    DataManager.GetHistoryDataVendor().LoadMutualFundNAV(this);
                    break;

                case DataInfoType.HistoryReport:
                    base.LoadBaseIndex();
                    DataManager.GetHistoryDataVendor().LoadMutualFundReport(this);
                    break;

                case DataInfoType.SpotTradePrice:
                case DataInfoType.HistoryDividend:
                case DataInfoType.HistoryBondValue:
                case DataInfoType.IndexComponents:
                    break;

                default:
                    MessageManager.GetInstance().AddMessage(MessageType.Information, Message.C_Msg_GE1, type.ToString());
                    break;
            }

            //加载基金比较基准指数
            if (type == DataInfoType.HistoryTradePrice || type == DataInfoType.RealtimeTradePrice || type == DataInfoType.SecurityInfo)
            {
                IndexGroup ig = new IndexGroup();
                ig.SetDatePeriod(this.TimeSeriesStart, this.TimeSeriesEnd);
                foreach (MutualFund f in this.SecurityList)
                {
                    if (f.PrimaryBenchmarkIndex != null)
                        ig.Add(f.PrimaryBenchmarkIndex, true);
                }

                if (ig.SecurityList != null && ig.SecurityList.Count > 0)
                    ig.LoadData(type);
            }

            //分级基金加载子基金数据
            if (SecurityClass == typeof(StructuredFund))
            {
                FundGroup fg = new FundGroup();
                fg.SetDatePeriod(this.TimeSeriesStart, this.TimeSeriesEnd);
                foreach (StructuredFund sf in this.SecurityList)
                {
                    if (sf.ShareA == null || sf.ShareB == null)
                        continue;

                    fg.Add(sf.ShareA);
                    fg.Add(sf.ShareB);                    
                }
                fg.LoadData(type);
            }
        }
        #endregion

        #region 扩展属性
        private MutualFund WholeFund;
        #endregion

        #region 扩展方法
        public MutualFund GetWholeFund()
        {
            if (base.SecurityList == null || base.SecurityList.Count == 0)
                return null;

            WholeFund = new MutualFund("");
            WholeFund.Code = "";
            WholeFund.Name = "基金组中包含的所有基金组成的整体";
            WholeFund.SetDatePeriod(base.TimeSeriesStart, base.TimeSeriesEnd);
            WholeFund.DataSource = this.DataSource;
            WholeFund.Category = ((MutualFund)base.SecurityList[0]).Category;
            WholeFund.FoundedDate = this.TimeSeriesStart;
            WholeFund.MaturityDate = this.TimeSeriesEnd;

            #region 构造整体基金的累计净值序列
            //==========================
            //计算公式：
            //  NAVw = Sum(NAVi * SHAREi)/Sum(SHAREi)
            //==========================
            try
            {
                WholeFund.HistoryTradeNAV.DataSource = this.DataSource;
                WholeFund.HistoryTradeNAV.ExchangeTradingDates = base.HistoryTradePrice.ExchangeTradingDates;
                //WholeFund.HistoryTradeNAV.InsideSampleLength = base.HistoryTradePrice.InsideSampleLength;
                WholeFund.HistoryTradeNAV.AdjustedTimeSeries = new List<AHistoryTimeItem>();
                WholeFund.HistoryAssetReport.AdjustedTimeSeries = new List<AHistoryTimeItem>();

                //计算净值
                for (int i = 0; i < WholeFund.HistoryTradeNAV.ExchangeTradingDates.Count; i++)
                {
                    HistoryItemNetAssetValue NAVw = new HistoryItemNetAssetValue();
                    NAVw.TradeDate = WholeFund.HistoryTradeNAV.ExchangeTradingDates[i];
                    NAVw.IsTrading = true;
                    NAVw.IsOutsideSamplePeriod = base.HistoryTradePrice.AdjustedTimeSeries[i].IsOutsideSamplePeriod;

                    HistoryItemMutualFundReport RPTw = new HistoryItemMutualFundReport();
                    RPTw.TradeDate = NAVw.TradeDate;
                    RPTw.ReportDate = DataManager.C_Null_Date;
                    RPTw.IsTrading = true;
                    RPTw.IsOutsideSamplePeriod = base.HistoryTradePrice.AdjustedTimeSeries[i].IsOutsideSamplePeriod;

                    foreach (MutualFund f in base.SecurityList)
                    {
                        if (f.HistoryTradeNAV.AdjustedTimeSeries == null || f.HistoryAssetReport.AdjustedTimeSeries == null
                            || i >= f.HistoryTradeNAV.AdjustedTimeSeries.Count || i >= f.HistoryAssetReport.AdjustedTimeSeries.Count)
                        {
                            continue;
                        }
                        
                        //成立不足30天的去除
                        if (f.FoundedDate.AddDays(30) > WholeFund.HistoryTradeNAV.ExchangeTradingDates[i])
                            continue;

                        //计算净值 和 资产配置
                        HistoryItemNetAssetValue NAVi = (HistoryItemNetAssetValue)f.HistoryTradeNAV.AdjustedTimeSeries[i];
                        HistoryItemMutualFundReport RPTi = (HistoryItemMutualFundReport)f.HistoryAssetReport.AdjustedTimeSeries[i];
                        NAVw.UnitNAV += NAVi.UnitNAV * RPTi.TotalShare;
                        RPTw.TotalShare += RPTi.TotalShare;

                        if (RPTw.ReportDate < RPTi.ReportDate)
                            RPTw.ReportDate = RPTi.ReportDate;

                        RPTw.TotalEquityAsset += RPTi.TotalEquityAsset;
                        RPTw.TotalBondAsset += RPTi.TotalBondAsset;
                        RPTw.TotalNetAsset += RPTi.TotalNetAsset;
                        RPTw.PureBondAsset += RPTi.PureBondAsset;
                        RPTw.ConvertableBondAsset += RPTi.ConvertableBondAsset;
                    }

                    NAVw.UnitNAV = NAVw.UnitNAV / RPTw.TotalShare;
                    NAVw.AccumUnitNAV = NAVw.UnitNAV;
                    NAVw.AdjustedUnitNAV = NAVw.UnitNAV;
                    WholeFund.HistoryTradeNAV.AdjustedTimeSeries.Add(NAVw);
                    WholeFund.HistoryAssetReport.AdjustedTimeSeries.Add(RPTw);
                }

                //计算净值收益率
                WholeFund.HistoryTradeNAV.Calculate();
                WholeFund.HistoryAssetReport.Calculate();
            }
            catch (Exception ex)
            {
                throw new Exception(Message.C_Msg_MF11, ex);
            }
            #endregion

            return WholeFund;
        }
        #endregion
    }
}
