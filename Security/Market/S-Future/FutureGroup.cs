
namespace Security
{
    public class FutureGroup : ASecurityGroup
    {
        #region 基础方法
        public FutureGroup() : base(typeof(Future)) { }
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
                    //base.LoadBaseIndex();   //获得交易日数据：以上证指数代替
                    //DataManager.GetHistoryDataVendor().LoadFuturePrice(this);//获得历史价格数据
                    break;

                case DataInfoType.RealtimeTradePrice:
                    //DataManager.GetRealtimeDataVendor().LoadTradePrice(this);//获得实时价格数据
                    break;

                default:
                    MessageManager.GetInstance().AddMessage(MessageType.Information, Message.C_Msg_GE1, type.ToString());
                    return;
            }
        }
        #endregion
    }
}
