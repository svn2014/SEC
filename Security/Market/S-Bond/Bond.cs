using System;

namespace Security
{
    public enum BondType
    {
        Treasury,           //国债
        Financial,          //普通金融债
        SpecialFinancial,   //政策性金融债
        Municipal,          //地方政府债
        Central,            //央票        
        Corporate,          //公司债        
        Enterprise,         //企业债
        MediumTerm,         //中期票据
        CommercialPaper,    //短期融资券
        Convertable,        //可转债
        Exchangable,        //可交换
        AssetBacked,        //资产支持债券
        SponsedAgency,      //政府支持机构债
        Other
    }

    public enum InterestType
    {
        Discount,   //贴现
        Zero,       //零息：期末支付利息
        Fixed,      //固定利率
        Float,      //浮动利率
        Stepped,    //递进利率 
        Other
    }

    public enum BaseRateType
    { 
        CurrentDeposit, //活期存款
        TimeDeposit,    //定期存款
        CallDeposit,    //通知存款
        AgreementDeposit,   //协议存款
        LoanRate,       //贷款利率
        Repo,
        SHIBOR,
        LIBOR,
        Other
    }
    
    public class Bond : ASecurity
    {
        #region 基础方法
        public Bond(string code, SecExchange exchange, DateTime start, DateTime end) : base(code, start, end) { base.Exchange = exchange; }
        public Bond(string code, SecExchange exchange) : base(code) { base.Exchange = exchange; }
        protected override void BuildSecurity()
        {
            base.Type = SecurityType.Bond;
        }
        protected override void BuildHistoryObjects(string code, DateTime start, DateTime end)
        {
            if (this.HistoryBondIntrinsicValue == null)
                this.HistoryBondIntrinsicValue = new HistoryBondValue(code, start, end);
            this.HistoryBondIntrinsicValue.SetDatePeriod(start, end);

            if (base.HistoryTradePrice == null)
                base.HistoryTradePrice = new HistoryTradePrice(code, start, end);
            base.HistoryTradePrice.SetDatePeriod(start, end);
        }
        public override void LoadData(DataInfoType type)
        {
            BondGroup bg;
            switch (type)
            {
                case DataInfoType.SecurityInfo:
                    DataManager.GetHistoryDataVendor().LoadBondInfo(this);
                    break;

                case DataInfoType.HistoryBondValue:
                    //读取债券估值数据：银行间市场由中债登发送
                    bg = new BondGroup();
                    bg.SetDatePeriod(this.TimeSeriesStart, this.TimeSeriesEnd);
                    bg.Add(this);
                    bg.LoadData(type);
                    this.HistoryBondIntrinsicValue = ((Bond)bg.SecurityList[0]).HistoryBondIntrinsicValue;
                    break;

                case DataInfoType.HistoryTradePrice:
                case DataInfoType.SpotTradePrice:
                    bg = new BondGroup();
                    bg.SetDatePeriod(this.TimeSeriesStart, this.TimeSeriesEnd);
                    bg.Add(this.Code);
                    bg.LoadData(type);
                    this.HistoryTradePrice = bg.SecurityList[0].HistoryTradePrice;
                    break;

                default:
                    MessageManager.GetInstance().AddMessage(MessageType.Information, Message.C_Msg_GE1, type.ToString());
                    return;
            }
        }
        #endregion

        #region 扩展属性
        public HistoryBondValue HistoryBondIntrinsicValue;

        public string CompanyCode, Industry;
        public BondType SubType = BondType.Other;
        public InterestType IntAccruType = InterestType.Other;
        public string BondCodeSH, BondCodeSZ, BondCodeIB;
        public DateTime ListedDateIB;   //在银行间市场上市的时间
        public string Issuer, IssuerStockCode, ConvertStockCode;
        public int IntPaymentFreq;
        public int IssueTerm; //期限
        public double IntNominalRate, IntFloatRateSpread;
        public string IntFloatRateBase;
        public BaseRateType IntFloatRateBaseType = BaseRateType.Other;
        public string CreditRate, CreditRates;
        public bool CreditInnerEnhance, CreditOuterEnhance;
        public bool IsCallable, IsPutable, IsExchangable, IsSplitable, HasEmbededOption;
        public DateTime IntStartDate = DataManager.C_Null_Date;
        public DateTime IssueDate = DataManager.C_Null_Date;
        public DateTime IntNextPaymentDate = DataManager.C_Null_Date;
        public DateTime MaturityDate = DataManager.C_Null_Date;
        public string IntPaymentDates;
        public string Currency;
        #endregion

        #region 扩展方法
        public static int ConvertLongTermCreditRating(string rating)
        {
            switch (rating)
            {
                case "AAA":
                case "AAA(S)": //该短期融资券到期具有极高的还本付息能力，投资者没有风险
                    return 430;
                case "AA+": return 423;
                case "AA": return 422;
                case "AA-": return 421;
                case "A+": return 413;
                case "A": return 412;
                case "A-": return 411;
                case "BBB+": return 333;
                case "BBB": return 332;
                case "BBB-": return 331;
                case "BB+": return 323;
                case "BB": return 322;
                case "BB-": return 321;
                case "B+": return 313;
                case "B": return 312;
                case "B-": return 311;
                case "CCC+": return 233;
                case "CCC": return 232;
                case "CCC-": return 231;
                case "CC": return 220;
                case "C": return 210;
                case "D": return 100;
                default:
                    return -1;
            }
        }
        public static int ConvertShortTermCreditRating(string rating)
        {
            switch (rating)
            {
                case "A-1+":
                case "A-1": 
                    return 430;
                case "A-2": return 411;
                case "A-3": return 332;
                case "B": return 323;
                case "C": return 233;
                default:
                    return -1;
            }
        }
        public static int ConvertCreditRating(string rating)
        { 
            //判断长期/短期评级
            if (rating.Contains("-") && rating.Substring(rating.Length - 1, 1) != "-")
            {
                //短期：仅适用于A-1+,A-1,A-2,A-3
                return ConvertShortTermCreditRating(rating);
            }
            else
            { 
                //长期
                return ConvertLongTermCreditRating(rating);
            }
        }
        #endregion
    }
}
