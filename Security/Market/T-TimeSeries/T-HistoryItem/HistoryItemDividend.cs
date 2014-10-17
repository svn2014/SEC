
using System;
namespace Security
{
    public class HistoryItemDividend : AHistoryTimeItem
    {
        #region 扩展属性
        public DateTime RegisterDate;   //登记日
        public DateTime ExcludeDate;    //除息日
        public double DividendBeforeTax;//税前红利, x元/10股
        #endregion
    }
}
