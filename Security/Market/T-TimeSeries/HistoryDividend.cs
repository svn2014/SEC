using System;
using System.Diagnostics;

namespace Security
{
    public class HistoryDividend: AHistoryTimeSeries
    {
        #region 基础方法
        public HistoryDividend(string code, DateTime start, DateTime end) : base(code, start, end) { }
        public override void Adjust(){/*不作调整*/}
        public override void Calculate() {/*不作计算*/}
        #endregion
    }
}
