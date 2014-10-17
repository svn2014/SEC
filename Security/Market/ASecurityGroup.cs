using System;
using System.Collections.Generic;
using System.Diagnostics;

namespace Security
{
    public abstract class ASecurityGroup: ASecurity
    {
        #region 基础方法
        public ASecurityGroup() : base("") { }
        public ASecurityGroup(Type type) : base("") { this.SecurityClass = type; this.Name = this.SecurityClass.ToString(); }
        public ASecurityGroup(string code) : base(code) { }
        public ASecurityGroup(string code, DateTime start, DateTime end) : base(code, start, end) { }
        protected override void BuildSecurity()
        {
            base.Exchange = SecExchange.OTC;
            base.Type = SecurityType.Group;            
        }
        protected override void BuildHistoryObjects(string code, DateTime start, DateTime end)
        {
            //调整持仓个股的时间区间
            if (this.SecurityList != null && this.SecurityList.Count > 0)
            {
                foreach (ASecurity s in this.SecurityList)
                {
                    s.SetDatePeriod(start, end);
                }
            }
        }        
        #endregion

        #region 扩展属性
        public List<ASecurity> SecurityList;
        public List<string> SecurityCodes;
        protected System.Type SecurityClass;
        #endregion

        #region 扩展方法
        #region Add/Remove Security
        public void Add(string code)
        {
            List<string> codelist = new List<string>();
            codelist.Add(code);
            Add(codelist);
        }
        public void Add(List<string> codelist)
        {
            if (codelist == null || codelist.Count == 0)
                return;

            foreach (string code in codelist)
            {
                if (!IsHolding(code))
                {
                    string[] para= new string[]{code};
                    ASecurity s = (ASecurity)Activator.CreateInstance(SecurityClass, para);
                    this.Add(s);
                }
            }
        }
        public void Add(ASecurity s)
        {
            if (!IsHolding(s.Code))
            {
                //调整各股的时间区间同组合一致
                s.SetDatePeriod(this.TimeSeriesStart, this.TimeSeriesEnd);

                this.SecurityCodes.Add(s.Code);
                this.SecurityList.Add(s);
            }
        }
        public void Add(ASecurity s, bool ignoreholdings)
        {
            if (!IsHolding(s.Code) || ignoreholdings)
            {
                //调整各股的时间区间同组合一致
                s.SetDatePeriod(this.TimeSeriesStart, this.TimeSeriesEnd);

                this.SecurityCodes.Add(s.Code);
                this.SecurityList.Add(s);
            }
        }
        public void Add(ASecurityGroup g)
        {
            foreach (ASecurity s in g.SecurityList)
                this.Add(s);
        }
        public void Clear()
        {
            this.SecurityCodes.Clear();
            this.SecurityList.Clear();
        }
        public ASecurity GetSecurity(string code)
        {
            return SecurityList.Find(delegate(ASecurity s) { return s.Code == code; });
        }
        public SecurityType GetSecurityType()
        {
            if (this.SecurityList == null || this.SecurityList.Count == 0)
                return SecurityType.Other;
            else
                return this.SecurityList[0].Type;
        }
        #endregion

        protected bool IsHolding(string code)
        {
            if (code.Length == 0)
                return true;

            if (this.SecurityCodes == null)
            {
                this.SecurityList = new List<ASecurity>();
                this.SecurityCodes = new List<string>();
                return false;
            }

            int idx = this.SecurityCodes.FindIndex(delegate(string s) { return s == code; });
            return idx >= 0;
        }
        protected void LoadSecurityInfo()
        {
            if (this.SecurityList == null || this.SecurityList.Count == 0)
                return;
            else
            {
                foreach (ASecurity s in this.SecurityList)
                {
                    s.LoadData(DataInfoType.SecurityInfo);
                    this.DataSource = s.DataSource;
                }
            }
        }
        
        protected void LoadBaseIndex()
        {
            if (this.HistoryTradePrice == null)
            {
                //获得交易日数据：以上证指数代替
                IndexGroup ig = new IndexGroup();
                ig.SetDatePeriod(this.TimeSeriesStart, this.TimeSeriesEnd);
                ig.Add("000001");
                ig.LoadData(DataInfoType.HistoryTradePrice);
                this.HistoryTradePrice = ig.SecurityList[0].HistoryTradePrice;
            }
        }

        public virtual void Calculate()
        {
            if (this.SecurityList == null || this.SecurityList.Count == 0)
                return;

            //初始化：避免重复计算
            this.Position = new Portfolio.PositionInfo();

            //计算证券组的信息
            double yield = 0;
            foreach (ASecurity s in this.SecurityList)
            {
                if (s.Position == null)
                    continue;

                //证券信息
                this.Position.SecType = s.Type;
                this.Position.TradingDay = s.Position.TradingDay;
                //估值信息
                this.Position.Sum(s.Position);

                //加权收益率
                yield += s.Position.MarketValuePct * (s.Position.AccumulatedYieldIndex - 1);
            }

            if (this.Position.Quantity == 0 && this.Position.MarketValuePct > 0)
            {
                this.Position.CurrentYield = yield / this.Position.MarketValuePct;
            }
            else
            {
                this.Position.CurrentYield = (this.Position.CurrentTotalCost != 0) ? this.Position.CurrentTotalReturn / this.Position.CurrentTotalCost : 0;
            }
            this.Position.AccumulatedYieldIndex *= 1 + this.Position.CurrentYield;
        }
        #endregion

        #region 辅助函数
        public static ASecurityGroup CreateGroup(SecurityType type)
        {
            switch (type)
            {
                case SecurityType.Equity:
                    return new EquityGroup();
                case SecurityType.Bond:
                    return new BondGroup();
                case SecurityType.Index:
                    return new IndexGroup();
                case SecurityType.Fund:
                    return new FundGroup();
                case SecurityType.Deposit:
                    return new DepositGroup();
                case SecurityType.Future:
                    return new FutureGroup();
                default:
                    return null;
            }
        }
        #endregion

        public void Print()
        {
            foreach (ASecurity s in this.SecurityList)
            {
                Debug.Write(s.Position.TradingDay.ToString("yyyy-MM-dd") + ",");
                Debug.Write(s.Code + ",");
                Debug.Write(s.Name + ",");
                Debug.Write(((Equity)s).Industry1 + ",");
                Debug.Write(s.Position.Quantity + ",");
                Debug.Write(s.Position.Cost + ",");
                Debug.Write(s.Position.MarketValue + ",");
                Debug.Write(s.Position.MarketValuePct + ",");
                Debug.Write(s.Position.CurrentYield + ",");
                Debug.WriteLine(s.Position.AccumulatedYieldIndex + ",");
            }
        }
    }
}
