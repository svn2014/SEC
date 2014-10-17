using System;
using Security.Portfolio;

namespace Security
{
    public enum SecurityType
    {
        Equity,
        Bond,
        Index,
        Fund,
        Group,
        TheRepo,    //正回购
        RevRepo,    //逆回购
        Deposit,    //存款
        Warrant,
        Future,
        Other
    }

    public enum SecExchange
    {
        SHE,    //上交所
        SZE,    //深交所
        IBM,    //银行间
        FFE,    //金融期货交易所
        DFE,    //大连商品期货交易所
        ZFE,    //郑州商品期货交易所
        SFE,    //上海期货交易所
        OTC     //场外市场
    }

    public enum EquityBoardType
    {
        Main,           //主板
        SmallMedium,    //中小板
        StartUp,        //创业板
        Other
    }

    public enum DataInfoType
    {
        //通用
        SecurityInfo,
        HistoryTradePrice,
        SpotTradePrice,
        HistoryReport,

        //股票
        HistoryDividend,

        //基金
        HistoryFundNAV,
        
        //债券
        HistoryBondValue,   //债券交易不活跃，使用估值；
        
        //指数
        IndexComponents,

        //实时
        RealtimeTradePrice,
        RealtimeFundNAV
    }

    public abstract class ASecurity : ADatePeriod
    {
        public string DataSource;
        public string Code = "";
        public string Name = "";
        public SecurityType Type;
        public SecExchange Exchange;
        public DateTime ListedDate;
        public DateTime DelistedDate;
        public AHistoryTimeSeries HistoryTradePrice;
        public ARealTimeSeries RealtimeTradePrice;
        public PositionInfo Position = new PositionInfo();  //持仓

        public override void SetDatePeriod(DateTime start, DateTime end)
        {
            base.SetDatePeriod(start, end);
            this.BuildHistoryObjects(this.Code, this.TimeSeriesStart, this.TimeSeriesEnd);
        }

        public abstract void LoadData(DataInfoType type);

        protected abstract void BuildHistoryObjects(string code, DateTime start, DateTime end);

        public ASecurity(string code)
        {
            this.Code = code;
            this.BuildSecurity();
        }
        public ASecurity(string code, DateTime start, DateTime end)
        {
            this.Code = code;
            this.BuildSecurity();
            this.SetDatePeriod(start, end);
        }
        protected abstract void BuildSecurity();

        #region 辅助函数
        public static SecExchange GetSecurityExchange(SecurityType type, string code)
        {
            if (code == null || code.Length == 0)
            {
                MessageManager.GetInstance().AddMessage(MessageType.Warning, Message.C_Msg_GE3, "(null)");
                return SecExchange.OTC;
            }

            string code1 = code.Substring(0, 1);
            string code2 = code.Substring(0, 2);
            string code3 = code.Substring(0, 3);            

            switch (type)
            {
                case SecurityType.Equity:
                    switch (code1)
	                {
                        case "6":   //上交所A股
                        case "9":   //上交所B股
                            return SecExchange.SHE;

                        case "0":   //深交所A股
                        case "3":   //深交所创业板
                        case "2":   //深交所B股
                            return SecExchange.SZE;

                        default:
                            break;
	                }
                    break;

                case SecurityType.Index:
                    switch (code1)
	                {
                        case "0":   //上交所指数
                            return SecExchange.SHE;

                        case "3":   //深交所指数
                            return SecExchange.SZE;

                        default:    //非交易所指数
                            return SecExchange.OTC;
	                }

                case SecurityType.Fund:
                    switch (code1)
                    {
                        case "5":   //上交所基金
                            return SecExchange.SHE;

                        case "1":   //深交所基金
                            return SecExchange.SZE;

                        default:    //非交易所基金
                            return SecExchange.OTC;
                    }

                case SecurityType.Group:
                    return SecExchange.OTC;

                default:
                    break;
            }

            throw new NotImplementedException(code);
        }
        public static ASecurity CreateSecurity(SecurityType type, string code, SecExchange exchange)
        {
            switch (type)
            {
                case SecurityType.Equity:
                    return new Equity(code, exchange);
                case SecurityType.Bond:
                    return new Bond(code, exchange);
                case SecurityType.Index:
                    return new Index(code);
                case SecurityType.Future:
                    return new Future(code, exchange);
                default:
                    return null;
            }
        }
        #endregion
    }
}
