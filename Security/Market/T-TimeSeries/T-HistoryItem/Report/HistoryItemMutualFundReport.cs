using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace Security
{
    public class HistoryItemMutualFundReport : AHistoryItemReport
    {
        #region 扩展属性
        public double TotalShare = 0;           //单位：份

        public double TotalNetAsset = 0;        //单位：元
        public double TotalEquityAsset = 0;     //单位：元
        public double TotalBondAsset = 0;       //单位：元

        //注：TotalBondAsset = PureBondAsset + ConvertableBondAsset
        public double PureBondAsset = 0;        //单位：元
        public double ConvertableBondAsset = 0; //单位：元
        #endregion
    }
}
