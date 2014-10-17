using System;
using System.Collections.Generic;

namespace Security
{
    //public class EquityIPOInfo
    //{
    //    public string SubCode;              //申购代码
    //    public DateTime BidDateStart;       //询价起始日
    //    public DateTime BidDateEnd;         //询价截止日
    //    public DateTime PurchaseDate;       //申购日
    //    public string PrimaryUnderwriter;   //主承销商
    //    public string Contacts;
    //}

    public class Equity : ASecurity
    {
        #region 基础方法
        public Equity(string code) : base(code) { }
        public Equity(string code, SecExchange exchange) : base(code) { base.Exchange = exchange; }
        public Equity(string code, DateTime start, DateTime end): base(code, start, end) { }
        protected override void BuildSecurity()
        {
            base.Exchange = ASecurity.GetSecurityExchange(SecurityType.Equity,this.Code);
            this.BoardType = getBoardType(base.Code);
            base.Type = SecurityType.Equity;
        }
        protected override void BuildHistoryObjects(string code, DateTime start, DateTime end)
        {
            if (base.HistoryTradePrice == null)
                base.HistoryTradePrice = new HistoryTradePrice(code, start, end);
            base.HistoryTradePrice.SetDatePeriod(start, end);

            if (this.HistoryDividends == null)
                this.HistoryDividends = new HistoryDividend(code, start, end);
            this.HistoryDividends.SetDatePeriod(start, end);
        }
        public override void LoadData(DataInfoType type)
        {
            EquityGroup eg;
            switch (type)
            {
                case DataInfoType.SecurityInfo:
                    DataManager.GetHistoryDataVendor().LoadEquityInfo(this);
                    break;

                case DataInfoType.HistoryTradePrice:
                case DataInfoType.SpotTradePrice:
                    eg = new EquityGroup();
                    eg.SetDatePeriod(this.TimeSeriesStart, this.TimeSeriesEnd);
                    eg.Add(this.Code);
                    eg.LoadData(type);
                    this.HistoryTradePrice = eg.SecurityList[0].HistoryTradePrice;
                    break;

                case DataInfoType.HistoryDividend:
                    eg = new EquityGroup();
                    eg.SetDatePeriod(this.TimeSeriesStart, this.TimeSeriesEnd);
                    eg.Add(this.Code);
                    eg.LoadData(DataInfoType.HistoryDividend);
                    this.HistoryDividends = ((Equity)eg.SecurityList[0]).HistoryDividends;
                    break;

                case DataInfoType.RealtimeTradePrice:
                    eg = new EquityGroup();
                    eg.Add(this.Code);
                    eg.LoadData(DataInfoType.RealtimeTradePrice);
                    this.RealtimeTradePrice = eg.SecurityList[0].RealtimeTradePrice;
                    break;

                default:
                    MessageManager.GetInstance().AddMessage(MessageType.Information, Message.C_Msg_GE1, type.ToString());
                    return;
            }
        }
        #endregion

        #region 扩展属性
        public EquityBoardType BoardType;
        public string Industry1, Industry2, Industry3, IndustryIndex;
        public HistoryDividend HistoryDividends;

        //IPO信息
        //public EquityIPOInfo IPO = new EquityIPOInfo();
        #endregion

        #region 扩展方法
        private EquityBoardType getBoardType(string code)
        {
            if (code == null || code.Length == 0)
                MessageManager.GetInstance().AddMessage(MessageType.Warning, Message.C_Msg_GE3, "(null)");
           
            string code3 = code.Substring(0, 3);
            string code2 = code.Substring(0, 2);

            switch (code2)
            {
                case "60":  //沪市
                    return EquityBoardType.Main;

                default:    //深市
                    switch (code3)
                    {
                        case "000":
                        case "001":
                            return EquityBoardType.Main;
                            
                        case "002":
                            return EquityBoardType.SmallMedium;

                        case "300":
                            return EquityBoardType.StartUp;

                        default:
                            break;
                    }
                    return EquityBoardType.Other;
            }
        }
        #endregion
    }
}
