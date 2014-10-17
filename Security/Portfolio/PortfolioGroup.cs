using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace Security.Portfolio
{
    public class PortfolioGroup
    {
        public Portfolio MergedPortfolio = new Portfolio();
        public List<Portfolio> Portfolios = new List<Portfolio>();
        public List<DateTime> ExchangeTradingDays;
        public List<DateTime> InnerBankTradingDays;
        
        public void Add(Portfolio p)
        {
            this.Portfolios.Add(p);

            //合并所有交易过的证券
            this.merge(p);
        }

        private void merge(Portfolio newportfolio)
        {
            foreach(ASecurityGroup g in newportfolio.SecurityGroupList)
            {
                if (g != null && g.SecurityList != null && g.SecurityList.Count > 0)
                {
                    foreach (ASecurity s in g.SecurityList)
                    {
                        try
                        {
                            ASecurityGroup sg = MergedPortfolio.GetSecurityGroup(s.Type);
                            if (sg != null)
                                sg.Add(s);
                        }
                        catch (Exception ex)
                        {                            
                            throw ex;
                        }
                    }
                }
            }
        }

        public bool IsDataComplete()
        {
            //检查数据完整性：每个交易日都有估值表
            bool result = true;
            try
            {
                if (this.ExchangeTradingDays.Count != this.Portfolios.Count)
                {
                    MessageManager.GetInstance().AddMessage(MessageType.Error, Message.C_Msg_PD4, "");
                    result = false;
                }

                foreach (Portfolio p in this.Portfolios)
                {
                    if (p.IsDataLoaded == false)
                    {
                        MessageManager.GetInstance().AddMessage(MessageType.Error, Message.C_Msg_PD4, p.ReportDate.ToString("yyyy-MM-dd"));
                        result = false;
                    }
                }

                return result;
            }
            catch (Exception ex)
            {
                MessageManager.GetInstance().AddMessage(MessageType.Error, ex.Message, "");
            }

            return false;
        }

        public void UpdateDividend()
        {
            if (this.MergedPortfolio.EquityHoldings.SecurityList == null || this.MergedPortfolio.EquityHoldings.SecurityList.Count == 0)
                return;

            foreach (ASecurity s in this.MergedPortfolio.EquityHoldings.SecurityList)
            {
                HistoryDividend hd = ((Equity)s).HistoryDividends;
                if (hd == null || hd.OriginalTimeSeries == null || hd.OriginalTimeSeries.Count == 0)
                    continue;

                foreach (HistoryItemDividend htd in hd.OriginalTimeSeries)
                {
                    //除息日持仓数据
                    Portfolio exP = this.Portfolios.Find(delegate(Portfolio p) { return p.ReportDate == htd.ExcludeDate; });
                    if (exP == null)
                        continue;

                    //股权登记日持仓数据
                    Portfolio regP = this.Portfolios.Find(delegate(Portfolio p) { return p.ReportDate == htd.RegisterDate; });
                    if (regP == null)
                    {
                        //登记日在统计区间之外，以第一个交易组合代替
                        if (htd.RegisterDate < this.ExchangeTradingDays[0])
                            regP = this.Portfolios[0];
                        else
                            throw new Exception();
                    }

                    //记录股利
                    ASecurity exS = exP.EquityHoldings.SecurityList.Find(delegate(ASecurity ts) { return ts.Code == s.Code; });
                    if (exS == null)
                        continue;   //分红时尚未购入

                    ASecurity regS = regP.EquityHoldings.SecurityList.Find(delegate(ASecurity ts) { return ts.Code == s.Code; });
                    if (regS == null)
                        regS = exS;

                    //股息[除息日]=持有量[登记日] / 10 * 税前红利[每10股派x元]
                    exS.Position.CurrentDividend = regS.Position.Quantity / 10 * htd.DividendBeforeTax;
                }
            }
        }

        public Portfolio GetLatestPortfolio()
        {
            //按时间顺序排列的
            if (this.Portfolios.Count > 0)
                return this.Portfolios[this.Portfolios.Count - 1];
            else
                return null;
        }
        public Portfolio GetEarliestPortfolio()
        {
            //按时间顺序排列的
            if (this.Portfolios.Count > 0)
                return this.Portfolios[0];
            else
                return null;
        }
    }
}
