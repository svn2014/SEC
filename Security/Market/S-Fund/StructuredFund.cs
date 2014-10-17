
using System;
using System.Collections.Generic;
namespace Security
{
    public class StructuredFund : MutualFund
    {
        #region 基础方法
        public StructuredFund(string code) : base(code) { }
        public StructuredFund(string code, DateTime start, DateTime end) : base(code, start, end) { }
        #endregion

        #region 扩展属性
        public bool IsArbitrageEnabled = false; //是否允许分拆合并套利;
        public ListedFund ShareA = null;        //A份额
        public ListedFund ShareB = null;        //B份额
        public double ShareARatio = 0.5;
        #endregion

        #region 扩展方法
        public void AddSubFund(string code, bool isfundA)
        {
            if (isfundA)
            {
                this.ShareA = new ListedFund(code);
                this.ShareA.SetDatePeriod(this.TimeSeriesStart, this.TimeSeriesEnd);
            }
            else
            {
                this.ShareB = new ListedFund(code);
                this.ShareB.SetDatePeriod(this.TimeSeriesStart, this.TimeSeriesEnd);
            }
        }
        #endregion
    }
}
