using System;
using System.Collections.Generic;

namespace Security
{
    public enum FundOperationCategory
    {
        Undefined,
        OpenEnd,
        CloseEnd
    }

    public enum FundAssetCategory
    {
        Undefined,
        Equity,
        Hybrid,
        Bond,
        Monetory,
        QDII,
        Other
    }

    public enum FundInvestmentCategory
    {
        Undefined,
        Active,
        Passive
    }

    public enum FundStructureCategory
    {
        Undefined,        
        Parent,
        Child
    }

    public class MutualFund : ASecurity
    {
        #region 基础方法
        public MutualFund(string code) : base(code) { }
        public MutualFund(string code, DateTime start, DateTime end) : base(code, start, end) { }
        public MutualFund(string code, SecExchange exchange) : base(code) { }
        protected override void BuildSecurity()
        {
            base.Exchange = SecExchange.OTC;
            base.Type = SecurityType.Fund;            
            this.Category = AFundCategory.GetFundCategory(FundCategoryType.GalaxySecurity);
        }
        protected override void BuildHistoryObjects(string code, DateTime start, DateTime end)
        {
            if (this.HistoryTradeNAV == null)
                this.HistoryTradeNAV = new HistoryNetAssetValue(code, start, end);
            this.HistoryTradeNAV.SetDatePeriod(start, end);

            if (base.HistoryTradePrice == null)
                base.HistoryTradePrice = new HistoryTradePrice(code, start, end);
            base.HistoryTradePrice.SetDatePeriod(start, end);

            if (this.HistoryAssetReport == null)
                this.HistoryAssetReport = new HistoryFundReport(code, start, end);
            this.HistoryAssetReport.SetDatePeriod(start, end);
        }
        public override void LoadData(DataInfoType type)
        {
            FundGroup mfg = null;
            try
            {
                switch (type)
                {
                    case DataInfoType.SecurityInfo:
                        DataManager.GetHistoryDataVendor().LoadMutualFundInfo(this);
                        break;

                    //获得交易日数据
                    case DataInfoType.HistoryFundNAV:
                        if (this.Category.AssetCategory != FundAssetCategory.Monetory)
                        {
                            mfg = new FundGroup();
                            mfg.SetDatePeriod(this.TimeSeriesStart, this.TimeSeriesEnd);
                            mfg.Add(this.Code);
                            mfg.LoadData(DataInfoType.HistoryFundNAV);
                            this.HistoryTradeNAV = ((MutualFund)mfg.SecurityList[0]).HistoryTradeNAV;
                        }
                        break;

                    case DataInfoType.HistoryTradePrice:
                        mfg = new FundGroup();
                        mfg.SetDatePeriod(this.TimeSeriesStart, this.TimeSeriesEnd);
                        mfg.Add(this.Code);
                        mfg.LoadData(type);
                        this.HistoryTradePrice = mfg.SecurityList[0].HistoryTradePrice;
                        break;
                        
                    case DataInfoType.HistoryReport:
                        mfg = new FundGroup();
                        mfg.SetDatePeriod(this.TimeSeriesStart, this.TimeSeriesEnd);
                        mfg.Add(this.Code);
                        mfg.LoadData(DataInfoType.HistoryReport);
                        this.HistoryAssetReport = ((MutualFund)mfg.SecurityList[0]).HistoryAssetReport;
                        break;

                    //获得实时价格数据
                    case DataInfoType.RealtimeTradePrice:
                        mfg = new FundGroup(this.GetType());
                        mfg.Add(this.Code);
                        mfg.LoadData(DataInfoType.RealtimeTradePrice);
                        this.RealtimeTradePrice = mfg.SecurityList[0].RealtimeTradePrice;
                        break;

                    case DataInfoType.RealtimeFundNAV:
                        mfg = new FundGroup();
                        mfg.Add(this.Code);
                        mfg.LoadData(DataInfoType.RealtimeFundNAV);
                        this.RealtimeNAV = ((MutualFund)mfg.SecurityList[0]).RealtimeNAV;
                        break;

                    default:
                        MessageManager.GetInstance().AddMessage(MessageType.Information, Message.C_Msg_GE1, type.ToString());
                        return;
                }

                //比较基准
                //this.PrimaryBenchmarkIndex.LoadData(type);

            }
            catch (Exception ex)
            {
                throw new Exception("读取基金数据时出错！", ex); ;
            }
        }
        #endregion

        #region 扩展属性
        public AFundCategory Category;
        public DateTime FoundedDate;
        public DateTime MaturityDate;
        public HistoryNetAssetValue HistoryTradeNAV;
        public RealtimeNetAssetValue RealtimeNAV;
        public HistoryFundReport HistoryAssetReport;

        public double MaxSubscribeRate = 0.015; //最大申购费率
        public double MaxRedeemRate = 0.005;    //最大赎回费率

        public string BenchmarkDescription = "";
        public Index PrimaryBenchmarkIndex = null;
        public double PrimaryBenchmarkWeight = 0.95;

        public bool IsFOF = false;              //Fund of Fund
        public bool IsETF = false;              //Exchange Listed Fund
        public bool IsLOF = false;              //Listed Open Fund
        public bool IsSOF = false;              //Structured Open Fund 分级基金
        public string ParentFundCode = "";
        public List<string> SubFundsCode;
        #endregion

        #region 扩展方法
        public void AddBenchmark(string code, double weight)
        {
            this.PrimaryBenchmarkIndex = new Index(code);
            this.PrimaryBenchmarkIndex.SetDatePeriod(this.TimeSeriesStart, this.TimeSeriesEnd);
            this.PrimaryBenchmarkWeight = weight;
        }
        #endregion
    }
}
