using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace Security
{
    public abstract class ARealtimeDataVendor
    {
        protected string DataSource = "未知数据源";
        protected DateTime TradeTimeStart = new DateTime(DateTime.Today.Year, DateTime.Today.Month, DateTime.Today.Day, 9, 30, 0);
        protected DateTime TradeTimeEnd = new DateTime(DateTime.Today.Year, DateTime.Today.Month, DateTime.Today.Day, 15, 0, 0);

        public abstract void LoadTradePrice(ASecurityGroup g);
        public abstract void LoadNetAssetValue(ASecurityGroup g);
    }
}
