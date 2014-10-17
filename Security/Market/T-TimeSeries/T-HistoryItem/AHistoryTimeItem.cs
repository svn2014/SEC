using System;
using System.Diagnostics;

namespace Security
{
    public class PriceUpAndDown : ICloneable
    {
        //涨跌幅
        public Nullable<double> KLineDay1 = null;
        public Nullable<double> KLineDay2 = null;
        public Nullable<double> KLineDay3 = null;
        public Nullable<double> KLineDay4 = null;
        public Nullable<double> KLineDay5 = null;
        public Nullable<double> KLineDay10 = null;
        public Nullable<double> KLineDay20 = null;

        public object Clone()
        {
            return this.MemberwiseClone();
        }
    }

    public abstract class AHistoryTimeItem : ICloneable
    {
        public bool IsOutsideSamplePeriod = false;  //样本外数据
        public DateTime TradeDate;
        public bool IsTrading = true;
        public PriceUpAndDown UpAndDown = new PriceUpAndDown();

        public virtual void Adjust() { }
        public object Clone()
        {
            object obj = this.MemberwiseClone();
            AHistoryTimeItem item = (AHistoryTimeItem)obj;
            item.UpAndDown = (PriceUpAndDown)this.UpAndDown.Clone();
            return item;
        }
    }
}
