
namespace Security
{
    public class IndexGroup: ASecurityGroup
    {
        #region 基础方法
        public IndexGroup() : base(typeof(Index)) { }
        public override void LoadData(DataInfoType type)
        {
            switch (type)
            {
                case DataInfoType.SecurityInfo:
                    this.LoadSecurityInfo();
                    break;

                //获得历史数据
                case DataInfoType.HistoryTradePrice:
                    DataManager.GetHistoryDataVendor().LoadIndexPrice(this, true);
                    break;

                //加载指数成份
                case DataInfoType.IndexComponents:                    
                    base.LoadBaseIndex();   //获得交易日数据：以上证指数代替
                    DataManager.GetHistoryDataVendor().LoadIndexComponents(this);
                    break;

                //获得实时数据
                case DataInfoType.RealtimeTradePrice:
                    DataManager.GetRealtimeDataVendor().LoadTradePrice(this);
                    break;

                case DataInfoType.SpotTradePrice:
                case DataInfoType.HistoryReport:
                case DataInfoType.HistoryDividend:
                case DataInfoType.HistoryFundNAV:
                case DataInfoType.HistoryBondValue:
                case DataInfoType.RealtimeFundNAV:
                    break;

                default:
                    MessageManager.GetInstance().AddMessage(MessageType.Information, Message.C_Msg_GE1, type.ToString());
                    return;
            }
        }
        public override void Calculate()
        {
            if (this.SecurityList == null || this.SecurityList.Count == 0)
                return;

            foreach (Index idx in this.SecurityList)
            {
                idx.ComponentsCalculate();
            }
        }
        #endregion
    }
}
