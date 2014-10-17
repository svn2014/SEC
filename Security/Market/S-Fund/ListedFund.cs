using System;

namespace Security
{
    public class ListedFund: MutualFund
    {
        //场内交易基金
        //  ETF
        //  LOF
        //  分级子基金

        #region 基础方法
        public ListedFund(string code) : base(code) { }
        public ListedFund(string code, SecExchange exchange) : base(code) { base.Exchange = exchange; }
        public ListedFund(string code, DateTime start, DateTime end) : base(code, start, end) { }
        protected override void BuildSecurity()
        {
            base.BuildSecurity();
            base.Exchange = ASecurity.GetSecurityExchange(SecurityType.Fund, this.Code);
        }
        #endregion
    }
}
