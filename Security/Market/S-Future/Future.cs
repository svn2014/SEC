using System;

namespace Security
{
    public class Future : ASecurity
    {
        #region 基础方法
        public Future(string code) : base(code) { }
        public Future(string code, SecExchange exchange) : base(code) { base.Exchange = exchange; }
        public Future(string code, DateTime start, DateTime end) : base(code, start, end) { }
        protected override void BuildSecurity()
        {
            base.Type = SecurityType.Future;
        }
        protected override void BuildHistoryObjects(string code, DateTime start, DateTime end)
        {
            if (base.HistoryTradePrice == null)
                base.HistoryTradePrice = new HistoryTradePrice(code, start, end);
            base.HistoryTradePrice.SetDatePeriod(start, end);
        }
        public override void LoadData(DataInfoType type)
        {
            FutureGroup fg;
            switch (type)
            {
                case DataInfoType.SecurityInfo:
                    DataManager.GetHistoryDataVendor().LoadFutureInfo(this);
                    break;

                case DataInfoType.HistoryTradePrice:
                    fg = new FutureGroup();
                    fg.SetDatePeriod(this.TimeSeriesStart, this.TimeSeriesEnd);
                    fg.Add(this.Code);
                    fg.LoadData(DataInfoType.HistoryTradePrice);
                    this.HistoryTradePrice = fg.SecurityList[0].HistoryTradePrice;
                    break;

                case DataInfoType.RealtimeTradePrice:
                    fg = new FutureGroup();
                    fg.Add(this.Code);
                    fg.LoadData(DataInfoType.RealtimeTradePrice);
                    this.RealtimeTradePrice = fg.SecurityList[0].RealtimeTradePrice;
                    break;

                default:
                    MessageManager.GetInstance().AddMessage(MessageType.Information, Message.C_Msg_GE1, type.ToString());
                    return;
            }
        }
        #endregion

        #region 扩展属性
        public DateTime DeliveryDate;
        public string CommodityCode;
        public string CommodityName;
        public string CommodityUnit;
        public string QuoteUnit;
        public string UnitsPerContract;
        public string TickPrice;

        public string PriceLimitPct;
        public string DeliveryMonth;
        public string LastTradingDate;
        public string LastDeliveryDate;
        public string MinimumTradingMargin;
        public string ClearingPrice;
        public string DeliveryPrice;
        #endregion
    }
}
