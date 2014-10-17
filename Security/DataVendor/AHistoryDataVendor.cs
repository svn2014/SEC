using System;
using System.Collections.Generic;
using System.Data;

namespace Security
{
    public abstract class AHistoryDataVendor
    {
        protected ADBInstance DBInstance;
        protected string DataSource = "未知数据源";

        #region 字段常量：所有不同数据库替换为相同的字段名
        #region 通用 
        public const string C_ColName_Order = "ORDER";
        public const string C_ColName_Type = "TYPE";
        public const string C_ColName_TradeAction = "TRADEACTION";
        public const string C_ColName_Price = "PRICE";
        public const string C_ColName_Quantity = "QUANTITY";
        public const string C_ColName_Cost = "COST";
        public const string C_ColName_MarketValue = "MARKETVALUE";
        public const string C_ColName_MarketValuePct = "MARKETVALUEPCT";
        public const string C_ColName_HoldingPeriodReturn = "HOLDPERIODRET";
        public const string C_ColName_HoldingPeriodYield = "HOLDPERIODYIELD";
        public const string C_ColName_DateIn = "DATEIN";
        public const string C_ColName_DateOut = "DATEOUT";
        #endregion

        #region 股票
        public const string C_ColName_Code = "CODE";
        public const string C_ColName_Name = "NAME";
        public const string C_ColName_Industry1 = "INDUSTRY1";
        public const string C_ColName_Industry2 = "INDUSTRY2";
        public const string C_ColName_Industry3 = "INDUSTRY3";
        public const string C_ColName_IndustryIndex = "INDUSTRYINDEX";
        public const string C_ColName_Exchange = "EXCHANGE";
        public const string C_ColName_ListedDate = "LISTEDDATE";
        public const string C_ColName_RelistedDate = "RELISTEDDATE";
        public const string C_ColName_DelistedDate = "DELISTEDDATE";
        public const string C_ColName_TradeDate = "TRADEDATE";
        public const string C_ColName_PreClose = "PRECLOSE";
        public const string C_ColName_Open = "OPEN";
        public const string C_ColName_High = "HIGH";
        public const string C_ColName_Low = "LOW";
        public const string C_ColName_Close = "CLOSE";
        public const string C_ColName_AdjustedClose = "ADJUSTEDCLOSE";
        public const string C_ColName_Volume = "VOLUME";
        public const string C_ColName_Amount = "AMOUNT";
        public const string C_ColName_Average = "AVERAGE";

        public const string C_ColName_ExDividendDate = "EXDIVIDENDDATE";  //除息日
        public const string C_ColName_RegisterDate = "REGISTERDATE";    //股权登记日
        public const string C_ColName_DividendBeforeTax = "DIVIDENDBFTAX";
        public const string C_ColName_DividendAfterTax = "DIVIDENDAFTAX";
        #endregion

        #region 债券
        public const string C_ColName_CompanyCode = "COMPANYCODE";
        public const string C_ColName_CodeSH = "CODESH";
        public const string C_ColName_CodeSZ = "CODESZ";
        public const string C_ColName_CodeIB = "CODEIB";
        public const string C_ColName_ListedDateIB = "LISTEDDATEIB";
        public const string C_ColName_ListedDateSH = "LISTEDDATESH";
        public const string C_ColName_ListedDateSZ = "LISTEDDATESZ";
        public const string C_ColName_BondType = "BONDTYPE";                 //国债，央票，企业债...
        public const string C_ColName_IsCorpBond = "ISCORP";     //区分企业债和公司债的标志
        public const string C_ColName_IntStartDate = "INTSTARTDATE";    //起息日
        public const string C_ColName_IssueDate = "ISSUEDATE";
        public const string C_ColName_Issuer = "ISSUER";
        public const string C_ColName_IssuerStockCode = "ISSUERSTOCKCODE";
        public const string C_ColName_ConvertStockCode = "CONVERTSTOCKCODE";
        public const string C_ColName_InterestType = "INTERESTTYPE";
        public const string C_ColName_IntAccruType = "INTACCRUTYPE";
        public const string C_ColName_IntPaymentDate = "INTPAYMENTDATE";
        public const string C_ColName_IntPaymentFreq = "INTPAYMENTFREQ";
        public const string C_ColName_IntNorminalRate = "NORMINALINTRATE";   //票面利率
        public const string C_ColName_IntFloatRateBase = "FLOATBASERATE";
        public const string C_ColName_IntFloatRateSpread = "FLOATRATESPREAD";
        public const string C_ColName_CreditRate = "BONDCREDITRATE";
        public const string C_ColName_EmbededOption = "EMBEDEDOPTION";
        public const string C_ColName_Callable = "CALLABLE";
        public const string C_ColName_Putable = "PUTABLE";
        public const string C_ColName_Exchangable = "EXCHANGABLE";
        public const string C_ColName_Splitable = "SPLITABLE";
        public const string C_ColName_CreditInnerEnhance = "INNERCREDITENHANCE";
        public const string C_ColName_CreditOuterEnhance = "OUTERCREDITENHANCE";
        public const string C_ColName_IssueTerm = "TERM";                         //发行年限
        public const string C_ColName_Currency = "CURRENCY";

        public const string C_ColName_CleanPrice = "CLEANPRICE";
        public const string C_ColName_DirtyPrice = "DIRTYPRICE";
        public const string C_ColName_AccruedInterest = "ACCRUEDINT";
        public const string C_ColName_Yield = "YIELD";
        public const string C_ColName_TermToMaturity = "TERMTOMATURITY";
        public const string C_ColName_ModifiedDuration = "DURATION_MOD";
        public const string C_ColName_Convexity = "CONVEXITY";
        public const string C_ColName_Duration_dis = "DURATION_DIS";     //利差久期
        public const string C_ColName_Convexity_dis = "CONVEXITY_DIS";   //利差凸性
        public const string C_ColName_Duration_int = "DURATION_INT";     //利率久期
        public const string C_ColName_Convexity_int = "CONVEXITY_INT";   //利率凸性
        public const string C_ColName_DirtyPrice_day = "DIRTYPRICE_DAY"; //当日全价
        public const string C_ColName_AccruedInterest_day = "ACCRUEDINT_DAY";    //当日应计利息   
        public const string C_ColName_BasisPoint_val = "BASEPOINTVAL";
        public const string C_ColName_RemainingPrinc = "REMAININGPRINC"; //剩余本金
        public const string C_ColName_ValueDeviation = "VALUEDEVIATION";    //估值偏离

        public const string C_ColName_DirtyPrice_Preclose = "DIRTYPRECLOSE";    //全价：前收
        public const string C_ColName_DirtyPrice_Open = "DIRTYOPEN";
        public const string C_ColName_DirtyPrice_Close = "DIRTYCLOSE";
        public const string C_ColName_DirtyPrice_High = "DIRTYHIGH";
        public const string C_ColName_DirtyPrice_Low = "DIRTYLOW";
        #endregion

        #region 基金
        public const string C_ColName_GSFundType = "GSFUNDTYPE";             //银河基金分类
        public const string C_ColName_FundType = "FUNDTYPE";                 //LOF, ETF, FOF, 联接，开基, 封基
        public const string C_ColName_ParentCode = "PARENTCODE";
        public const string C_ColName_ParentName = "PARENTNAME";
        public const string C_ColName_BenchmarkText = "BENCHMARKTEXT";
        public const string C_ColName_PrimaryBenchmarkCode = "BENCHMARKCODE";
        public const string C_ColName_PrimaryBenchmarkWeight = "BENCHMARKWEIGHT";
        public const string C_ColName_UnitNAV = "UNITNAV";
        public const string C_ColName_AccumUnitNAV = "ACCMNAV";
        public const string C_ColName_AdjustedUnitNAV = "ADJNAV";
        public const string C_ColName_StructureType = "STRUCTURETYPE";       //分级基金类型：母基金，A/B份额
        public const string C_ColName_AllowPairTrans = "ALLOWPAIRTRANS";     //是否可以配对转换
        public const string C_ColName_PairRatio = "PAIRRATIO";               //配对比例
        public const string C_ColName_FundFeeRate = "FUNDFEERATE";
        public const string C_ColName_PublishDate = "PUBLISHDATE";
        public const string C_ColName_ReportDate = "REPORTDATE";
        public const string C_ColName_DeclareDate = "DECLAREDATE";
        public const string C_ColName_FundShare = "TOTALSHARE";
        public const string C_ColName_FundNetAsset = "NETASSET";
        public const string C_ColName_FundEquityAsset = "EQUITYASSET";
        public const string C_ColName_FundBondAsset = "BONDASSET";
        public const string C_ColName_FundCashAsset = "CASHASSET";
        public const string C_ColName_FundCBAsset = "CBASSET";
        #endregion

        #region 指数
        protected const string C_ColName_IndexCode = "INDEXCODE";
        protected const string C_ColName_IndexName = "INDEXNAME";
        protected const string C_ColName_IndexExchange = "INDEXEXCHANGE";
        protected const string C_ColName_IndexBaseDate = "BASEDATE";
        protected const string C_ColName_IndexStopDate = "STOPDATE";
        protected const string C_ColName_IndexCategory = "CATEGORY";
        protected const string C_ColName_IndexComponentCount = "COMPONENTCOUNT";
        protected const string C_ColName_IndexComponentWeight = "COMPONENTWEIGHT";
        public const string C_ColName_SecurityType = "SECURITYTYPE";
        #endregion

        #region 期货
        public const string C_ColName_DeliveryDate = "DELIVERYDATE";
        public const string C_ColName_CommodityName = "COMMODITYNAME";
        public const string C_ColName_CommodityUnit = "COMMODITYUNIT";
        public const string C_ColName_QuoteUnit = "UNITPRICE";
        public const string C_ColName_UnitsPerContract = "UNITSPERCONTRACT";
        public const string C_ColName_TickPrice = "TICKPRICE";
        public const string C_ColName_PriceLimitPct = "PRICELIMITPCT";
        public const string C_ColName_DeliveryMonth = "DELIVERYMONTH";
        public const string C_ColName_LastTradingDate = "LASTTRADINGDATE";
        public const string C_ColName_LastDeliveryDate = "LASTDELIVERYDATE";
        public const string C_ColName_MinimumTradingMargin = "MINIMUMTRADINGMARGIN";
        public const string C_ColName_ClearingPrice = "CLEARINGPRICE";
        public const string C_ColName_DeliveryPrice = "DELIVERYPRICE";
        #endregion
        #endregion

        #region 数据缓存
        protected static DateTime _TimeSeriesStart, _TimeSeriesEnd;
        protected static DataSet _ExchangeTradingDays, _InnerBankTradingDays;

        protected static DataSet _EquityInfo;
        protected static DataSet _BondInfo;
        protected static DataSet _FundInfo;
        protected static DataSet _IndexInfo;
        protected static DataSet _FutureInfo, _FutureContactInfo;
        #endregion

        #region 日历数据
        public abstract void LoadTradingDate(AHistoryTimeSeries ts);
        #endregion

        #region 股票方法
        public abstract void LoadEquityInfo(Equity e);
        public abstract void LoadEquityPrice(ASecurityGroup g, bool IsLoadAll);
        public abstract void LoadEquityDividend(ASecurityGroup g);
        #endregion

        #region 债券方法
        public abstract void LoadBondInfo(Bond b);
        public abstract void LoadBondValue(ASecurityGroup g);
        public abstract void LoadBondPrice(ASecurityGroup g, bool IsLoadAll);
        #endregion

        #region 基金方法
        public abstract FundGroup GetMutualFunds(AFundCategory category);
        public abstract void LoadMutualFundInfo(MutualFund f);
        public abstract void LoadMutualFundNAV(ASecurityGroup g);
        public abstract void LoadMutualFundPrice(ASecurityGroup g, bool IsLoadAll);
        public abstract void LoadMutualFundReport(ASecurityGroup g);
        #endregion

        #region 指数方法
        public abstract void LoadIndexInfo(Index i);
        public abstract void LoadIndexPrice(ASecurityGroup g, bool IsLoadAll);
        public abstract void LoadIndexComponents(ASecurityGroup g);
        #endregion

        #region 期货方法
        public abstract void LoadFutureInfo(Future f);
        #endregion

        #region 通用方法
        protected string getCodeListString(List<string> codelist)
        {
            string codestring = "";
            if (codelist != null && codelist.Count > 0)
            {
                foreach (string code in codelist)
                {
                    codestring += "'" + code + "',";
                }

                if (codestring.Length > 0)
                    codestring = codestring.Substring(0, codestring.Length - 1);
            }

            if (codestring.Length == 0)
                codestring = "''";

            return codestring;
        }
        protected string getCodeListString(List<ASecurity> securitylist, SecExchange exchange)
        {
            string codestring = "";
            if (securitylist != null && securitylist.Count > 0)
            {
                foreach (ASecurity s in securitylist)
                {
                    if (s.Exchange == exchange)
                        codestring += "'" + s.Code + "',";
                }

                if (codestring.Length > 0)
                    codestring = codestring.Substring(0, codestring.Length - 1);
            }

            if (codestring.Length == 0)
                codestring = "''";

            return codestring;
        }
        #endregion
    }
}
