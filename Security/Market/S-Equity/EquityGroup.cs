using System;
using System.Collections.Generic;

namespace Security
{
    public class EquityGroup : ASecurityGroup
    {
        #region 基础方法
        public EquityGroup() : base(typeof(Equity)) { }
        public override void LoadData(DataInfoType type)
        {
            if (this.SecurityList == null || this.SecurityList.Count == 0)
                return;

            switch (type)
            {
                case DataInfoType.SecurityInfo:
                    this.LoadSecurityInfo();
                    break;

                case DataInfoType.HistoryTradePrice:                    
                    base.LoadBaseIndex();   //获得交易日数据：以上证指数代替                    
                    DataManager.GetHistoryDataVendor().LoadEquityPrice(this,true);//获得历史价格数据
                    break;

                case DataInfoType.SpotTradePrice:
                    base.LoadBaseIndex();   //获得交易日数据：以上证指数代替                    
                    DataManager.GetHistoryDataVendor().LoadEquityPrice(this, false);//获得点价格数据
                    break;

                case DataInfoType.HistoryDividend:
                    base.LoadBaseIndex();   //获得交易日数据：以上证指数代替
                    DataManager.GetHistoryDataVendor().LoadEquityDividend(this);//获得历史红利数据
                    break;

                case DataInfoType.RealtimeTradePrice:
                    DataManager.GetRealtimeDataVendor().LoadTradePrice(this);//获得实时价格数据
                    break;

                case DataInfoType.HistoryReport:
                case DataInfoType.HistoryFundNAV:
                case DataInfoType.HistoryBondValue:
                case DataInfoType.IndexComponents:
                case DataInfoType.RealtimeFundNAV:
                    break;

                default:
                    MessageManager.GetInstance().AddMessage(MessageType.Information, Message.C_Msg_GE1, type.ToString());
                    return;
            }
        }
        #endregion

        #region 扩展属性
        public List<EquityGroup> IndustryList = new List<EquityGroup>();
        #endregion

        #region 扩展方法
        public void IndustryClassify()
        {
            if (this.SecurityList == null || this.SecurityList.Count == 0)
                return;

            //加载行业数据
            this.LoadData(DataInfoType.SecurityInfo);

            //证券行业分类
            this.IndustryList.Clear();
            foreach (ASecurity s in this.SecurityList)
            {
                Equity e = (Equity)s;
                if (e.Industry1 == null || e.Industry1 == "")
                {
                    if (e.Position.Quantity == 0 && e.Position.MarketValuePct == 0)
                        continue;   //已经卖出不再计入行业组合
                    else
                    {
                        MessageManager.GetInstance().AddMessage(MessageType.Warning, Message.C_Msg_EQ3, e.Code + e.Name);
                    }
                }

                EquityGroup sg = this.IndustryList.Find(delegate(EquityGroup teg) { return teg.Name == e.Industry1; });
                if (sg == null)
                {
                    //建立行业分类
                    sg = new EquityGroup();
                    sg.Code = e.IndustryIndex;
                    sg.Name = e.Industry1;
                    this.IndustryList.Add(sg);
                }

                sg.Add(e);
            }

            //持仓及收益率统计
            foreach (EquityGroup eg in this.IndustryList)
            {
                eg.Calculate();

                //调整比例：占股票组合比例
                if (this.Position.MarketValuePct > 0)
                    eg.Position.MarketValuePct /= this.Position.MarketValuePct;
            }
        }
        #endregion
    }
}
