
namespace Security
{
    public class HistoryItemNetAssetValue : AHistoryTimeItem
    {
        #region 扩展属性
        /// <summary>
        /// 单位净值：当前申赎的价格
        /// 复权净值: 将分红加回单位净值，并作为再投资进行复利计算。 **常用于计算收益率**
        /// 累计净值: 将分红加回单位净值，但不作为再投资计算复利。
        /// </summary>
        public double UnitNAV = 0;           //单位净值
        public double AccumUnitNAV = 0;      //累计净值
        public double AdjustedUnitNAV = 0;       //复权净值
        #endregion
    }
}
