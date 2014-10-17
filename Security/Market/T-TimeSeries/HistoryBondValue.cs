using System;

namespace Security
{
    public class HistoryBondValue : AHistoryTimeSeries
    {
        #region 基础方法
        public HistoryBondValue(string code, DateTime start, DateTime end) : base(code, start, end) { }
        public override void Calculate()
        {
            //====================
            //计算涨跌幅: 时间序列全部降序排列
            //====================
            if (base.AdjustedTimeSeries == null || base.AdjustedTimeSeries.Count == 0)
                return;
            
            double val0 = 1, val1 = 1;
            DateTime dt0 = DateTime.Today, dt1 = DateTime.Today;

            for (int i = 0; i < this.AdjustedTimeSeries.Count; i++)
            {
                HistoryItemBondValue item = (HistoryItemBondValue)this.AdjustedTimeSeries[i];
                if ((i + 1) < this.AdjustedTimeSeries.Count)
                {
                    item.UpAndDown.KLineDay1 = item.ClearPrice / ((HistoryItemBondValue)this.AdjustedTimeSeries[i + 1]).ClearPrice - 1;
                    item.DirtyPxUpAndDown.KLineDay1 = item.DirtyPrice / ((HistoryItemBondValue)this.AdjustedTimeSeries[i + 1]).DirtyPrice - 1;
                }
                if ((i + 2) < this.AdjustedTimeSeries.Count)
                {
                    item.UpAndDown.KLineDay2 = item.ClearPrice / ((HistoryItemBondValue)this.AdjustedTimeSeries[i + 2]).ClearPrice - 1;
                    item.DirtyPxUpAndDown.KLineDay2 = item.DirtyPrice / ((HistoryItemBondValue)this.AdjustedTimeSeries[i + 2]).DirtyPrice - 1;
                }
                if ((i + 3) < this.AdjustedTimeSeries.Count)
                {
                    item.UpAndDown.KLineDay3 = item.ClearPrice / ((HistoryItemBondValue)this.AdjustedTimeSeries[i + 3]).ClearPrice - 1;
                    item.DirtyPxUpAndDown.KLineDay3 = item.DirtyPrice / ((HistoryItemBondValue)this.AdjustedTimeSeries[i + 3]).DirtyPrice - 1;
                }
                if ((i + 4) < this.AdjustedTimeSeries.Count)
                {
                    item.UpAndDown.KLineDay4 = item.ClearPrice / ((HistoryItemBondValue)this.AdjustedTimeSeries[i + 4]).ClearPrice - 1;
                    item.DirtyPxUpAndDown.KLineDay4 = item.DirtyPrice / ((HistoryItemBondValue)this.AdjustedTimeSeries[i + 4]).DirtyPrice - 1;
                }
                if ((i + 5) < this.AdjustedTimeSeries.Count)
                {
                    item.UpAndDown.KLineDay5 = item.ClearPrice / ((HistoryItemBondValue)this.AdjustedTimeSeries[i + 5]).ClearPrice - 1;
                    item.DirtyPxUpAndDown.KLineDay5 = item.DirtyPrice / ((HistoryItemBondValue)this.AdjustedTimeSeries[i + 5]).DirtyPrice - 1;
                }
                if ((i + 10) < this.AdjustedTimeSeries.Count)
                {
                    item.UpAndDown.KLineDay10 = item.ClearPrice / ((HistoryItemBondValue)this.AdjustedTimeSeries[i + 10]).ClearPrice - 1;
                    item.DirtyPxUpAndDown.KLineDay10 = item.DirtyPrice / ((HistoryItemBondValue)this.AdjustedTimeSeries[i + 10]).DirtyPrice - 1;
                }
                if ((i + 20) < this.AdjustedTimeSeries.Count)
                {
                    item.UpAndDown.KLineDay20 = item.ClearPrice / ((HistoryItemBondValue)this.AdjustedTimeSeries[i + 20]).ClearPrice - 1;
                    item.DirtyPxUpAndDown.KLineDay20 = item.DirtyPrice / ((HistoryItemBondValue)this.AdjustedTimeSeries[i + 20]).DirtyPrice - 1;
                }

                //期末值
                if (i == 0)
                {
                    val1 = item.DirtyPrice;
                    dt1 = item.TradeDate;
                }
                //期初值
                if (item.TradeDate >= base.TimeSeriesStart)
                {
                    val0 = item.DirtyPrice;
                    dt0 = item.TradeDate;
                }
            }

            //持有期收益率:全价
            base.HoldingPeriodInfo = "中债估值："+val0.ToString("N4") + "[" + dt0.ToString("yyyy-MM-dd") + "]-" + val1.ToString("N4") + "[" + dt1.ToString("yyyy-MM-dd") + "]";
            base.HoldingPeriodReturn = val1 / val0 - 1;
        }
        #endregion
    }
}
