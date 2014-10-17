using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace Security
{
    public enum FundCategoryType
    { 
        GalaxySecurity  //银河证券
    }

    public abstract class AFundCategory: ACategory
    {
        #region 扩展属性
        public FundOperationCategory OperationCategory = FundOperationCategory.Undefined;
        public FundAssetCategory AssetCategory = FundAssetCategory.Undefined;
        public FundInvestmentCategory InvestmentCategory = FundInvestmentCategory.Undefined;
        public FundStructureCategory StructureCategory = FundStructureCategory.Undefined;
        #endregion

        #region 扩展方法
        public abstract void SetupCategory(string text, int level);
        #endregion

        #region 静态方法
        public static AFundCategory GetFundCategory(FundCategoryType type)
        {
            switch (type)
            {
                case FundCategoryType.GalaxySecurity:
                    return new FundCategoryGS();
                default:
                    MessageManager.GetInstance().AddMessage(MessageType.Warning, Message.C_Msg_MF7, type.ToString());
                    return null;
            }
        }
        #endregion
    }
}
