using System;
using System.Collections.Generic;

namespace Security
{
    public class HistoryTradePrice: AHistoryTimeSeries
    {
        #region 基础方法
        public HistoryTradePrice(string code, DateTime start, DateTime end) : base(code, start, end) { }
        public override void Calculate()
        {
            //====================
            //计算涨跌幅: 时间序列全部降序排列
            //====================
            if (this.AdjustedTimeSeries == null || this.AdjustedTimeSeries.Count == 0)
                return;

            double px0 = 1, px1 = 1;
            DateTime dt0 = DateTime.Today, dt1 = DateTime.Today;
            for (int i = 0; i < this.AdjustedTimeSeries.Count; i++)
            {
                HistoryItemTradingPrice item = (HistoryItemTradingPrice)this.AdjustedTimeSeries[i];
                if ((i + 1) < this.AdjustedTimeSeries.Count)
                    item.UpAndDown.KLineDay1 = item.Close / ((HistoryItemTradingPrice)this.AdjustedTimeSeries[i + 1]).Close - 1;
                if ((i + 2) < this.AdjustedTimeSeries.Count)
                    item.UpAndDown.KLineDay2 = item.Close / ((HistoryItemTradingPrice)this.AdjustedTimeSeries[i + 2]).Close - 1;
                if ((i + 3) < this.AdjustedTimeSeries.Count)
                    item.UpAndDown.KLineDay3 = item.Close / ((HistoryItemTradingPrice)this.AdjustedTimeSeries[i + 3]).Close - 1;
                if ((i + 4) < this.AdjustedTimeSeries.Count)
                    item.UpAndDown.KLineDay4 = item.Close / ((HistoryItemTradingPrice)this.AdjustedTimeSeries[i + 4]).Close - 1;
                if ((i + 5) < this.AdjustedTimeSeries.Count )
                    item.UpAndDown.KLineDay5 = item.Close / ((HistoryItemTradingPrice)this.AdjustedTimeSeries[i + 5]).Close - 1;
                if ((i + 10) < this.AdjustedTimeSeries.Count)
                    item.UpAndDown.KLineDay10 = item.Close / ((HistoryItemTradingPrice)this.AdjustedTimeSeries[i + 10]).Close - 1;
                if ((i + 20) < this.AdjustedTimeSeries.Count)
                    item.UpAndDown.KLineDay20 = item.Close / ((HistoryItemTradingPrice)this.AdjustedTimeSeries[i + 20]).Close - 1;

                //期末值
                if (i == 0)
                {
                    px1 = item.Close;
                    dt1 = item.TradeDate;
                }
                //期初值
                if (item.TradeDate >= base.TimeSeriesStart)
                {
                    px0 = item.Close;
                    dt0 = item.TradeDate;
                }
            }

            //持有期收益率
            base.HoldingPeriodInfo = px0.ToString("N4") + "[" + dt0.ToString("yyyy-MM-dd") + "]-" + px1.ToString("N4") + "[" + dt1.ToString("yyyy-MM-dd") + "]";
            base.HoldingPeriodReturn = px1 / px0 - 1;
        }
        #endregion
    }
}
