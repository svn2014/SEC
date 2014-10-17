using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Security.Portfolio;

namespace Security
{
    public class BondGroup : ASecurityGroup
    {
        #region 基础方法
        public BondGroup() : base(typeof(Bond)) { }
        public override void LoadData(DataInfoType type)
        {
            if (this.SecurityList == null || this.SecurityList.Count == 0)
                return;

            switch (type)
            {
                case DataInfoType.SecurityInfo:
                    this.LoadSecurityInfo();
                    break;

                case DataInfoType.HistoryBondValue:
                    base.LoadBaseIndex();                                           //获得交易日数据：以上证指数代替 
                    DataManager.GetHistoryDataVendor().LoadBondValue(this);         //获得历史估值数据
                    break;

                case DataInfoType.SpotTradePrice:
                    base.LoadBaseIndex();                                           //获得交易日数据：以上证指数代替                    
                    DataManager.GetHistoryDataVendor().LoadBondPrice(this,false);    //获得历史价格数据
                    break;

                case DataInfoType.HistoryTradePrice:
                    base.LoadBaseIndex();                                           //获得交易日数据：以上证指数代替                    
                    DataManager.GetHistoryDataVendor().LoadBondPrice(this, true);    //获得历史价格数据
                    break;

                case DataInfoType.HistoryReport:
                case DataInfoType.HistoryDividend:
                case DataInfoType.HistoryFundNAV:
                case DataInfoType.IndexComponents:
                case DataInfoType.RealtimeTradePrice:
                case DataInfoType.RealtimeFundNAV:
                    break;

                default:
                    MessageManager.GetInstance().AddMessage(MessageType.Information, Message.C_Msg_GE1, type.ToString());
                    return;
            }
        }
        #endregion

        #region 扩展方法
        public double GetAverageMaturity()
        {
            if (this.SecurityList == null || this.SecurityList.Count == 0)
                return 0;

            double weightedavg = 0, avg =0;
            foreach (Bond b in this.SecurityList)
            {
                int remainingdays = (b.MaturityDate - DateTime.Today).Days;

                weightedavg += remainingdays * b.Position.MarketValuePct;
                avg += remainingdays / this.SecurityList.Count;
            }

            if (weightedavg == 0)
                return avg;
            else
                return weightedavg;
        }

        public class CompanyView
        {
            public string CompanyCode;
            public string CompanyName;
            public int CompanyNumber = 0;
            public PositionInfo TotalPosition = new PositionInfo();
            public List<PositionInfo> RelatedPositionList = new List<PositionInfo>();
        }

        public List<CompanyView> GetCompanyPositions()
        { 
            //计算同一公司发行的债券合计持仓
            if (this.SecurityList == null || this.SecurityList.Count == 0)
                return null;

            List<CompanyView> cvlist = new List<CompanyView>();
            foreach (Bond b in this.SecurityList)
            {
                CompanyView ofind = cvlist.Find(delegate(CompanyView c) { return c.CompanyCode == b.CompanyCode; });
                if (ofind == null)
                {
                    ofind = new CompanyView();
                    ofind.CompanyCode = b.CompanyCode;
                    ofind.CompanyName = b.Issuer;
                    cvlist.Add(ofind);
                }

                ofind.TotalPosition.Cost += b.Position.Cost;
                ofind.TotalPosition.CostPct += b.Position.Cost;
                ofind.TotalPosition.MarketValue += b.Position.MarketValue;
                ofind.TotalPosition.MarketValuePct += b.Position.MarketValuePct;
                ofind.TotalPosition.ValueAdded += b.Position.ValueAdded;
                ofind.CompanyNumber++;
                ofind.RelatedPositionList.Add(b.Position);
            }

            return cvlist;
        }
        #endregion
    }
}
