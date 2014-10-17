
namespace Security
{
    public class HistoryItemBondValue : AHistoryTimeItem
    {
        #region 扩展属性
        public PriceUpAndDown DirtyPxUpAndDown = new PriceUpAndDown();  //父类中的UpAndDown为CleanPrice涨跌幅

        public double ClearPrice;
        public double DirtyPrice;
        public double AccruedInterest;
        public double TermToMaturity;   //剩余期限
        public double Yield;
        public double ModifiedDuration;     //修正久期
        public double Convexity;
        public double SpreadDuration;     //利差久期
        public double SpreadConvexity;    //利差凸性
        public double InterestDuration;     //利率久期
        public double InterestConvexity;    //利率凸性
        public double BasePointValue;   //基点价值
        public double RemainingPrinc;
        public double CurrDayAccruedInterest;  //当日应计利息
        public double CurrDayDirtyPrice;   //当日全价
        #endregion
    }
}
