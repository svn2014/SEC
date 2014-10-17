using System;
using System.Diagnostics;

namespace Security
{
    public class HistoryFundReport: AHistoryTimeSeries
    {
        #region 基础方法
        public HistoryFundReport(string code, DateTime start, DateTime end) : base(code, start, end) { }
        #endregion
    }
}
