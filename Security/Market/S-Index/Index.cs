using System;
using System.Diagnostics;
using System.Collections.Generic;

namespace Security
{
    public class Index: ASecurity
    {
        #region 基础方法
        public Index(string code) : base(code) { }
        public Index(string code, DateTime start, DateTime end) : base(code, start, end) { }
        protected override void BuildSecurity()
        {
            base.Exchange = ASecurity.GetSecurityExchange(SecurityType.Index, this.Code);
            base.Type = SecurityType.Index;
        }
        protected override void BuildHistoryObjects(string code, DateTime start, DateTime end)
        {
            if (base.HistoryTradePrice == null)
                base.HistoryTradePrice = new HistoryTradePrice(code, start, end);
            base.HistoryTradePrice.SetDatePeriod(start, end);
        }
        public override void LoadData(DataInfoType type)
        {
            IndexGroup ig;
            switch (type)
            {
                case DataInfoType.SecurityInfo:
                    DataManager.GetHistoryDataVendor().LoadIndexInfo(this);
                    break;

                case DataInfoType.HistoryTradePrice:
                    ig = new IndexGroup();
                    ig.SetDatePeriod(this.TimeSeriesStart, this.TimeSeriesEnd);
                    ig.Add(this.Code);
                    ig.LoadData(type);
                    this.HistoryTradePrice = ig.SecurityList[0].HistoryTradePrice;
                    this.Position.CurrentYield = this.HistoryTradePrice.HoldingPeriodReturn.Value;
                    this.Position.AccumulatedYieldIndex = 1 + this.Position.CurrentYield;
                    break;

                case DataInfoType.RealtimeTradePrice:
                    ig = new IndexGroup();
                    ig.Add(this.Code);
                    ig.LoadData(DataInfoType.RealtimeTradePrice);
                    this.RealtimeTradePrice = ig.SecurityList[0].RealtimeTradePrice;
                    break;

                case DataInfoType.IndexComponents:
                    ig = new IndexGroup();
                    ig.SetDatePeriod(this.TimeSeriesStart, this.TimeSeriesEnd);
                    ig.Add(this.Code);
                    ig.LoadData(DataInfoType.IndexComponents);
                    this.Components = ((Index)ig.SecurityList[0]).Components;
                    break;

                default:
                    MessageManager.GetInstance().AddMessage(MessageType.Information, Message.C_Msg_GE1, type.ToString());
                    return;
            }
        }
        #endregion

        #region 扩展属性
        public string Category;
        public List<ASecurityGroup> Components = new List<ASecurityGroup>();    //按交易日降序排列
        #endregion 

        #region 扩展方法
        public void ComponentsCalculate()
        {
            if (this.Components.Count == 0)
                return;

            //Components已按交易日降序排列
            //首期（时间上的第一期）
            ASecurityGroup g = this.Components[this.Components.Count - 1];
            if (this.Components.Count == 1)
                this.ComponentsCalculate(g, this.TimeSeriesStart, this.TimeSeriesEnd);
            else if (this.TimeSeriesStart < g.ReportDate)
                this.ComponentsCalculate(g, this.TimeSeriesStart, g.ReportDate);            

            //中间
            for (int i = this.Components.Count - 2; i >= 0; i--)
            {
                //计算个股区间收益率
                if (i == 0)
                    this.ComponentsCalculate(this.Components[i], this.Components[i + 1].ReportDate, this.TimeSeriesEnd);    //末期
                else
                    this.ComponentsCalculate(this.Components[i], this.Components[i + 1].ReportDate, this.Components[i].ReportDate);
                
                //计算行业累计收益率
                if (this.Components[i].GetSecurityType() == SecurityType.Equity)
                {
                    foreach (ASecurityGroup industry in ((EquityGroup)this.Components[i]).IndustryList)
                    {
                        EquityGroup findind = ((EquityGroup)this.Components[i + 1]).IndustryList.Find(delegate(EquityGroup tg) { return tg.Name == industry.Name; });
                        
                        if (findind == null)
                            continue;

                        industry.Position.AccumulatedYieldIndex *= findind.Position.AccumulatedYieldIndex;
                    }
                }

                //计算全部累计收益率
                this.Components[i].Position.AccumulatedYieldIndex *= this.Components[i + 1].Position.AccumulatedYieldIndex;
            } 
        }
        private void ComponentsCalculate(ASecurityGroup g, DateTime start, DateTime end)
        {
            if (g == null || g.SecurityList == null || g.SecurityList.Count == 0)
                return;

            try
            {
                g.SetDatePeriod(start, end);
                g.LoadData(DataInfoType.SpotTradePrice);

                //将区间回报赋值给Position
                foreach (ASecurity s in g.SecurityList)
                {
                    if (s.HistoryTradePrice.HoldingPeriodReturn != null)
                    {
                        s.Position.CurrentYield = s.HistoryTradePrice.HoldingPeriodReturn.Value;
                        s.Position.AccumulatedYieldIndex = s.HistoryTradePrice.HoldingPeriodReturn.Value + 1;
                    }
                    else
                    {
                        MessageManager.GetInstance().AddMessage(MessageType.Warning, Message.C_Msg_GE5, s.Name + "(" + s.Code + ")");
                    }
                }

                if (g.GetSecurityType() == SecurityType.Equity)
                    ((EquityGroup)g).IndustryClassify();
                g.Calculate();
            }
            catch (Exception ex)
            {
                throw ex;
            }            
        }
        public ASecurityGroup GetLatestComponents()
        {
            if (this.Components.Count == 0)
                return null;

            //[0]=最新的成份股
            return this.Components[0];
        }
        #endregion
    }
}
