
namespace Security
{
    public class FundSelector
    {
        public FundGroup GetAllFund()
        {
            return DataManager.GetHistoryDataVendor().GetMutualFunds(null);
        }

        public FundGroup GetActiveEquityFunds()
        {
            AFundCategory c = AFundCategory.GetFundCategory(FundCategoryType.GalaxySecurity);
            c.AssetCategory = FundAssetCategory.Equity;
            c.OperationCategory = FundOperationCategory.OpenEnd;
            c.InvestmentCategory = FundInvestmentCategory.Active;
            c.StructureCategory = FundStructureCategory.Parent;

            return DataManager.GetHistoryDataVendor().GetMutualFunds(c);
        }

        public FundGroup GetActiveHybridFunds()
        {
            AFundCategory c = AFundCategory.GetFundCategory(FundCategoryType.GalaxySecurity);
            c.AssetCategory = FundAssetCategory.Hybrid;
            c.OperationCategory = FundOperationCategory.OpenEnd;
            c.InvestmentCategory = FundInvestmentCategory.Active;
            c.StructureCategory = FundStructureCategory.Parent;

            return DataManager.GetHistoryDataVendor().GetMutualFunds(c);
        }

        public FundGroup GetSOF()
        {
            AFundCategory c = AFundCategory.GetFundCategory(FundCategoryType.GalaxySecurity);
            //c.AssetCategory = FundAssetCategory.Equity;
            //c.InvestmentCategory = FundInvestmentCategory.Passive;
            c.StructureCategory = FundStructureCategory.Parent;

            FundGroup fg = DataManager.GetHistoryDataVendor().GetMutualFunds(c);
            FundGroup sof = new FundGroup(typeof(StructuredFund));

            if (fg.SecurityList != null && fg.SecurityList.Count > 0)
            {
                for (int i = 0; i < fg.SecurityList.Count; i++)
                {
                    if (((MutualFund)fg.SecurityList[i]).IsSOF)
                    {
                        sof.Add(fg.SecurityList[i].Code);
                    }
                }
            }

            return sof;
        }

        public FundGroup GetLOF()
        {
            AFundCategory c = AFundCategory.GetFundCategory(FundCategoryType.GalaxySecurity);
            //c.AssetCategory = FundAssetCategory.Equity;
            //c.InvestmentCategory = FundInvestmentCategory.Passive;
            c.StructureCategory = FundStructureCategory.Parent;

            FundGroup fg = DataManager.GetHistoryDataVendor().GetMutualFunds(c);
            FundGroup lof = new FundGroup(typeof(ListedFund));

            if (fg.SecurityList != null && fg.SecurityList.Count > 0)
            {
                for (int i = 0; i < fg.SecurityList.Count; i++)
                {
                    if (((MutualFund)fg.SecurityList[i]).IsLOF)
                    {
                        lof.Add(fg.SecurityList[i].Code);
                    }
                }
            }

            return lof;
        }
    }
}
