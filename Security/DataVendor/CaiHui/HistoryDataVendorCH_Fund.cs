using System;
using System.Collections.Generic;
using System.Data;
using System.Diagnostics;

namespace Security
{
    public partial class HistoryDataVendorCH
    {
        #region 基金信息
        public override FundGroup GetMutualFunds(AFundCategory category)
        {
            try
            {
                //读取数据库
                this.LoadMutualFundInfo();

                //以Group输出
                FundGroup mfg = new FundGroup();

                foreach (DataRow oRow in _FundInfo.Tables[0].Rows)
                {
                    MutualFund f = new MutualFund(oRow[C_ColName_Code].ToString());
                    this.LoadMutualFundInfo(f);
                    
                    if (category == null ||
                        (
                            (f.Category.AssetCategory == category.AssetCategory || category.AssetCategory == FundAssetCategory.Undefined) &&
                            (f.Category.InvestmentCategory == category.InvestmentCategory || category.InvestmentCategory == FundInvestmentCategory.Undefined) &&
                            (f.Category.OperationCategory == category.OperationCategory || category.OperationCategory == FundOperationCategory.Undefined) &&
                            (f.Category.StructureCategory == category.StructureCategory || category.StructureCategory == FundStructureCategory.Undefined)
                         )
                       )
                        mfg.Add(f);
                }

                return mfg;
            }
            catch (Exception ex)
            {
                throw ex;
            }
        }

        private DataSet _FundSubscribeRate, _FundRedeemRate;
        private void LoadMutualFundInfo()
        {
            try
            {
                if (_FundInfo == null)
                {
                    string sql = @"SELECT A.Symbol AS " + C_ColName_Code + @"
                                    , A.Name AS " + C_ColName_Name + @"
                                    , A.listdate AS " + C_ColName_ListedDate + @"
                                    , B.listdate AS " + C_ColName_RelistedDate + @"
                                    , A.Delistdate AS " + C_ColName_DelistedDate + @"
                                    , B.GSFundType AS " + C_ColName_GSFundType + @"
                                    , B.Tdate AS " + C_ColName_TradeDate + @"
                                    , C.Symbol_Comp AS " + C_ColName_ParentCode + @"
                                    , C.Sname_Comp AS " + C_ColName_ParentName + @"
                                    , D.Fundclassinfo1 AS " + C_ColName_StructureType + @"
                                    , D.FundClassInfo3 AS " + C_ColName_AllowPairTrans + @"
                                    , E.IFundOS1 AS " + C_ColName_PairRatio + @"
                                    , F.Fund_Benchmark1 AS " + C_ColName_BenchmarkText + @"
                                    , G.Weight AS " + C_ColName_PrimaryBenchmarkWeight + @"
                                    , H.Compcode AS " + C_ColName_PrimaryBenchmarkCode + @"
                                    , A.FType AS  " + C_ColName_FundType + @"
                                        FROM(
                                          SELECT Symbol, Ofprofile3 as name, OFProfile8 as listdate, 'OF' as FundType, null as Delistdate, OFProfile5 as FType
                                          FROM OFProfile
                                          UNION
                                          SELECT Symbol, Cfprofile3 as name, CFProfile8 as listdate, 'CF' as FundType, CFProfile10 as Delistdate, '' as FType
                                          FROM CFProfile
                                        ) A
                                        LEFT JOIN (
                                              SELECT AA.Symbol, AA.listdate, AA.Operation, AA.Tdate, AA.CompanyName
                                              , BB.Columnvalue as GSFundType
                                              FROM GSFRating AA 
                                              LEFT JOIN DATADICT_COLUMNVALUE BB
                                                   ON AA.Stype=BB.Columncode AND BB.Cid = '25672'
                                              WHERE TDATE=(SELECT MAX(TDATE) FROM GSFRating WHERE AA.SYMBOL=SYMBOL)
                                        ) B
                                          ON A.Symbol = B.SYMBOL
                                        LEFT JOIN CURFSCODE C
                                          ON A.Symbol = C.SYMBOL
                                        LEFT JOIN FundClassInfo D
                                          ON A.Symbol = D.SYMBOL
                                        LEFT JOIN IFundOS E
                                          ON A.Symbol=E.Symbol
                                        LEFT JOIN FUND_BENCHMARK F
                                             ON A.Symbol=F.Symbol AND F.Enabled=0
                                        LEFT JOIN (
                                          Select * From FundBIW AA 
                                          Where Changedate = (select max(changedate) from FundBIW group by symbol having symbol = AA.symbol) 
                                          And weight>60
                                        ) G
                                             ON A.Symbol=G.Symbol
                                        LEFT JOIN FUNDBDY_C H
                                             ON G.ICode = H.WCode
                                        ";

                    //获得所有基金的信息做缓存
                    _FundInfo = base.DBInstance.ExecuteSQL(sql);

                    //获得申赎费率
                    sql = @"SELECT Symbol AS " + C_ColName_Code + @"
                            , CHGRF8  AS " + C_ColName_FundFeeRate + @"
                            FROM CHGRF WHERE CHGRF3 in ('0501','0505') AND CHGRF4 = '02' AND (CHGRF9 = 0 OR CHGRF1 = 1) ORDER BY Symbol, DeclareDate desc, BeginDate desc, CHGRF1";
                    _FundSubscribeRate = base.DBInstance.ExecuteSQL(sql);
                    sql = @"SELECT Symbol AS " + C_ColName_Code + @"
                            , CHGRF8  AS " + C_ColName_FundFeeRate + @"
                            FROM CHGRF WHERE CHGRF3 in ('0601','0604') AND CHGRF4 = '02' AND (CHGRF9 = 0 OR CHGRF1 = 1) ORDER BY Symbol, DeclareDate desc, BeginDate desc, CHGRF1";
                    _FundRedeemRate = base.DBInstance.ExecuteSQL(sql);
                }
            }
            catch (Exception ex)
            {
                throw ex;
            }
        }
        public override void LoadMutualFundInfo(MutualFund f)
        {
            try
            {
                //读取数据库
                this.LoadMutualFundInfo();

                //更新数据
                DataRow[] rows = _FundInfo.Tables[0].Select(C_ColName_Code + "='" + f.Code + "'");
                if (rows.Length >= 1)
                {
                    f.DataSource = this.DataSource;
                    f.Category.DataSource = this.DataSource;

                    DataRow row = rows[0];
                    f.Name = row[C_ColName_Name].ToString();
                    f.FoundedDate = DataManager.ConvertToDate(row[C_ColName_ListedDate]);
                    f.MaturityDate = DataManager.ConvertToDate(row[C_ColName_DelistedDate]);
                    f.Category.SetupCategory(row[C_ColName_GSFundType].ToString(), 3);

                    string ftype = row[C_ColName_FundType].ToString();
                    switch (ftype.ToUpper())
                    {
                        case "LOF":
                            f.IsLOF = true;
                            break;
                        case "ETF":
                            f.IsETF = true;
                            break;
                        case "FOF":
                            f.IsFOF = true;
                            break;
                        default:
                            break;
                    }

                    //比较基准
                    f.BenchmarkDescription = row[C_ColName_BenchmarkText].ToString();
                    if (row[C_ColName_PrimaryBenchmarkCode] != DBNull.Value && row[C_ColName_PrimaryBenchmarkWeight] != null && row[C_ColName_PrimaryBenchmarkWeight].ToString().Trim().Length >0)
                    {
                        string indexcode = row[C_ColName_PrimaryBenchmarkCode].ToString();
                        double weight = DataManager.ConvertToDouble(row[C_ColName_PrimaryBenchmarkWeight]) / 100;

                        if (indexcode.Length == 6)
                            f.AddBenchmark(indexcode, weight);
                    }

                    //申购赎回费率
                    DataRow[] rowsSubRate = _FundSubscribeRate.Tables[0].Select(C_ColName_Code + "='" + f.Code + "'");
                    DataRow[] rowsRedRate = _FundRedeemRate.Tables[0].Select(C_ColName_Code + "='" + f.Code + "'");

                    if (rowsSubRate != null && rowsSubRate.Length > 0 && rowsSubRate[0][C_ColName_FundFeeRate] != DBNull.Value)
                        f.MaxSubscribeRate = DataManager.ConvertToDouble(rowsSubRate[0][C_ColName_FundFeeRate]) / 100;

                    if (rowsRedRate != null && rowsRedRate.Length > 0 && rowsRedRate[0][C_ColName_FundFeeRate] != DBNull.Value)
                        f.MaxRedeemRate = DataManager.ConvertToDouble(rowsRedRate[0][C_ColName_FundFeeRate]) / 100;
                    else
                    { }

                    #region 子基金信息
                    if (row[C_ColName_ParentCode] == DBNull.Value)
                    {
                        //普通基金
                        f.IsSOF = false;
                        f.Category.StructureCategory = FundStructureCategory.Parent;
                    }
                    else
                    {
                        if (row[C_ColName_ParentCode].ToString() == row[C_ColName_Code].ToString())
                        {
                            //该基金是母基金
                            f.Category.StructureCategory = FundStructureCategory.Parent;

                            //寻找子基金: 
                            //  注意:     000387没有母基金
                            //            160806历史上有两个分级代码,当前有两个新的分级代码,共4个子基金
                            string gsftdate = row[C_ColName_TradeDate].ToString();
                            string strfilter ="";
                            if (gsftdate.Length > 0)
                                strfilter = C_ColName_ParentCode + "='" + f.Code + "' AND "
                                    + C_ColName_TradeDate + "='" + gsftdate + "' AND "
                                    + C_ColName_Code + " <> " + C_ColName_ParentCode;
                            else
                                strfilter = C_ColName_ParentCode + "='" + f.Code + "' AND "
                                    + C_ColName_Code + " <> " + C_ColName_ParentCode;

                            DataRow[] rowssubfunds = _FundInfo.Tables[0].Select(strfilter);

                            if (rowssubfunds.Length == 0)
                            {
                                MessageManager.GetInstance().AddMessage(MessageType.Warning, Message.C_Msg_MF8, f.Code);
                            }
                            else
                            {
                                if (f.SubFundsCode == null)
                                    f.SubFundsCode = new List<string>();
                                else
                                    f.SubFundsCode.Clear();

                                //区分母基金：分级基金或伞形基金
                                if (row[C_ColName_StructureType] == DBNull.Value)
                                {
                                    //伞形基金: 货币债券基金A,B,C
                                    f.IsSOF = false;
                                    foreach (DataRow rowsub in rowssubfunds)
                                    {
                                        string code = rowsub[C_ColName_Code].ToString();
                                        f.SubFundsCode.Add(code);
                                    }
                                }
                                else
                                {
                                    //分级基金
                                    f.IsSOF = true;

                                    //增加分级基金属性：是否可以配对转换
                                    if (f.GetType() == typeof(StructuredFund))
                                    {
                                        if (row[C_ColName_AllowPairTrans] != DBNull.Value)
                                            ((StructuredFund)f).IsArbitrageEnabled = (row[C_ColName_AllowPairTrans].ToString() == "1");
                                    }

                                    //增加子基金属性
                                    //  注: 000387: 该分级基金没有母基金
                                    if (rowssubfunds.Length >= 2)
                                    {
                                        foreach (DataRow rowsub in rowssubfunds)
                                        {
                                            string code = rowsub[C_ColName_Code].ToString();
                                            f.SubFundsCode.Add(code);

                                            if (f.GetType() == typeof(StructuredFund))
                                            {
                                                int iStructureFlag = DataManager.ConvertToInt(rowsub[C_ColName_StructureType]);

                                                //增加子基金
                                                switch (iStructureFlag)
                                                {
                                                    case 2:     //A份额
                                                        ((StructuredFund)f).AddSubFund(code, true);
                                                        //配对比例
                                                        if (rowsub[C_ColName_PairRatio] != DBNull.Value)
                                                            ((StructuredFund)f).ShareARatio = DataManager.ConvertToDouble(rowsub[C_ColName_PairRatio]) / 100;
                                                        else
                                                            MessageManager.GetInstance().AddMessage(MessageType.Error, Message.C_Msg_MF9, code);
                                                        break;
                                                    case 3:     //B份额
                                                        ((StructuredFund)f).AddSubFund(code, false);
                                                        //配对比例
                                                        if (rowsub[C_ColName_PairRatio] != DBNull.Value)
                                                            ((StructuredFund)f).ShareARatio = 1 - DataManager.ConvertToDouble(rowsub[C_ColName_PairRatio]) / 100;
                                                        else
                                                            MessageManager.GetInstance().AddMessage(MessageType.Error, Message.C_Msg_MF9, code);
                                                        break;
                                                    default:
                                                        break;
                                                }
                                            }
                                        }
                                    }
                                    else
                                    {
                                        MessageManager.GetInstance().AddMessage(MessageType.Warning, Message.C_Msg_MF8, f.Code);
                                    }
                                }                     
                            }
                        }
                        else
                        {
                            //该基金是子基金
                            f.Category.StructureCategory = FundStructureCategory.Child;
                            f.ParentFundCode = row[C_ColName_ParentCode].ToString();
                        }
                    }
                    #endregion
                }
                else
                {
                    MessageManager.GetInstance().AddMessage(MessageType.Warning, Message.C_Msg_MF5, f.Code);
                }
            }
            catch (Exception ex)
            {
                throw ex;
            }
        }
        #endregion

        #region 基金净值
        public override void LoadMutualFundNAV(ASecurityGroup g)
        {
            //===============================
            //批量读取多个股票的数据，速度更快
            //===============================
            try
            {
                //如果没有持仓则退出
                if (g.SecurityList == null || g.SecurityCodes.Count == 0)
                    return;

                //读数据: 按时间降序排列
                string sql = @"SELECT Symbol AS " + C_ColName_Code + @"
                                , TDate AS " + C_ColName_TradeDate + @"
                                , NAV AS " + C_ColName_UnitNAV + @"
                                , NAV1 AS " + C_ColName_AccumUnitNAV + @"
                                , NAV2 AS " + C_ColName_AdjustedUnitNAV + @"
                                FROM DERNAV WHERE 1=1 ";
                sql += " AND SYMBOL IN (" + base.getCodeListString(g.SecurityCodes) + ")";
                sql += " AND TDate >= '" + g.TimeSeriesStartExtended.ToString("yyyyMMdd") + "' "
                     + " AND TDate <= '" + g.TimeSeriesEnd.ToString("yyyyMMdd") + "' ";
                sql += " ORDER BY Symbol, TDate Desc";

                DataSet ds = base.DBInstance.ExecuteSQL(sql);

                //更新数据
                foreach (MutualFund f in g.SecurityList)
                {
                    this.updateFundNAV(ds, f.HistoryTradeNAV);
                }
            }
            catch (Exception ex)
            {
                throw ex;
            }
        }
        private void updateFundNAV(DataSet ds, HistoryNetAssetValue navs)
        {
            DataRow[] rows = ds.Tables[0].Select(C_ColName_Code + "='" + navs.Code + "'");
            if (rows.Length > 0)
            {
                //基本信息
                navs.DataSource = this.DataSource;
                navs.OriginalTimeSeries.Clear();

                foreach (DataRow row in rows)
                {
                    HistoryItemNetAssetValue nav = new HistoryItemNetAssetValue();
                    nav.TradeDate = DataManager.ConvertToDate(row[C_ColName_TradeDate]);
                    nav.UnitNAV = DataManager.ConvertToDouble(row[C_ColName_UnitNAV]);
                    nav.AccumUnitNAV = DataManager.ConvertToDouble(row[C_ColName_AccumUnitNAV]);
                    nav.AdjustedUnitNAV = DataManager.ConvertToDouble(row[C_ColName_AdjustedUnitNAV]);
                    //复权系数
                    navs.OriginalTimeSeries.Add(nav);
                }

                //交易日校验,复权并计算涨跌幅
                navs.Adjust();
            }
            else
            {
                MessageManager.GetInstance().AddMessage(MessageType.Warning, Message.C_Msg_MF4, navs.Code);
            }
        }
        #endregion

        #region 基金价格
        public override void LoadMutualFundPrice(ASecurityGroup g, bool IsLoadAll)
        {
            try
            {
                //如果没有持仓则退出
                if (g.SecurityList == null || g.SecurityCodes.Count == 0)
                    return;

                //读数据: 按时间降序排列
                string sql = @"SELECT A.TDate AS " + C_ColName_TradeDate + @"
                                , A.Symbol AS " + C_ColName_Code + @"
                                , A.SName AS " + C_ColName_Name + @"
                                , A.Exchange AS " + C_ColName_Exchange + @"
                                , A.LClose AS " + C_ColName_PreClose + @"
                                , A.TOpen AS " + C_ColName_Open + @"
                                , A.TClose AS " + C_ColName_Close + @"
                                , A.HIGH AS " + C_ColName_High + @"
                                , A.LOW AS " + C_ColName_Low + @"
                                , A.VOTURNOVER AS " + C_ColName_Volume + @"
                                , A.VATURNOVER AS " + C_ColName_Amount + @"
                                , C.LTDR*B.TClose AS " + C_ColName_AdjustedClose + @"
                                FROM FHDQUOTE A
                                LEFT JOIN (
	                                SELECT * FROM FHDQUOTE AA WHERE TDATE=(SELECT MIN(TDATE) FROM FHDQUOTE WHERE AA.SYMBOL=SYMBOL AND TOPEN>0)
                                )B
                                        ON A.Symbol = B.Symbol
                                LEFT JOIN Se_dreturn C
                                        ON A.Symbol = C.Symbol AND A.Tdate=C.Tdate AND C.dtype=6
                                WHERE 1=1 ";
                sql += " AND A.SYMBOL IN (" + base.getCodeListString(g.SecurityCodes) + ")";
                sql += this.getDateRestrictSQL("A.Tdate", g, IsLoadAll);
                sql += " ORDER BY A.Symbol, Tdate Desc";
                DataSet ds = base.DBInstance.ExecuteSQL(sql);

                //更新数据
                this.updateTradePrice(ds, g, IsLoadAll);
            }
            catch (Exception ex)
            {
                throw ex;
            }
        }
        #endregion

        #region 基金报表
        public override void LoadMutualFundReport(ASecurityGroup g)
        {
            try
            {
                //如果没有持仓则退出
                if (g.SecurityList == null || g.SecurityCodes.Count == 0)
                    return;

                //读数据: 
                string sqlShare = @"SELECT Symbol AS " + C_ColName_Code + @"
                                    , Publishdate AS " + C_ColName_PublishDate + @"
                                    , DeclareDate AS " + C_ColName_DeclareDate + @"
                                    , FShare2 AS " + C_ColName_FundShare + @"
                                    FROM FShare  WHERE 1=1 "
                    + " AND SYMBOL IN (" + base.getCodeListString(g.SecurityCodes) + ")"
                    + " AND Declaredate >= " + base.DBInstance.ConvertToSQLDate(g.TimeSeriesStartExtended.AddDays(-100))
                    + " AND Declaredate <= " + base.DBInstance.ConvertToSQLDate(g.TimeSeriesEnd)
                    + " ORDER BY Symbol, PublishDate Desc ";

                string sqlAsset = @"SELECT Symbol AS " + C_ColName_Code + @"
                                    , Publishdate AS " + C_ColName_PublishDate + @"
                                    , ReportDate AS " + C_ColName_ReportDate + @"
                                    , AssetAL1 AS " + C_ColName_FundNetAsset + @"
                                    , AssetAL2 AS " + C_ColName_FundEquityAsset + @"
                                    , AssetAL6 AS " + C_ColName_FundCBAsset + @"
                                    , AssetAL10 AS " + C_ColName_FundCashAsset + @"
                                    , AssetAL33  AS " + C_ColName_FundBondAsset + @"
                                    FROM AssetAL WHERE 1=1 "
                    + " AND SYMBOL IN (" + base.getCodeListString(g.SecurityCodes) + ")"
                    + " AND PublishDate >= " + base.DBInstance.ConvertToSQLDate(g.TimeSeriesStartExtended.AddDays(-100))
                    + " AND PublishDate <= " + base.DBInstance.ConvertToSQLDate(g.TimeSeriesEnd)
                    + " ORDER BY Symbol, Reportdate Desc ";

                DataSet dsShare = base.DBInstance.ExecuteSQL(sqlShare);
                DataSet dsAsset = base.DBInstance.ExecuteSQL(sqlAsset);

                //更新数据
                foreach (MutualFund f in g.SecurityList)
                {
                    this.updateFundReport(dsShare, dsAsset, f.HistoryAssetReport);
                }
            }
            catch (Exception ex)
            {
                throw ex;
            }
        }
        private void updateFundReport(DataRow rowShare, DataRow rowAsset, HistoryFundReport r, bool useAssetReportDate)
        {
            HistoryItemMutualFundReport rpt = new HistoryItemMutualFundReport();
            if (rowShare != null)
            {
                rpt.PublishDate = DataManager.ConvertToDate(rowShare[C_ColName_DeclareDate]);
                rpt.ReportDate = DataManager.ConvertToDate(rowShare[C_ColName_PublishDate]);

                rpt.TotalShare = DataManager.ConvertToDouble(rowShare[C_ColName_FundShare]) * 10000;        //单位：份
            }

            if (rowAsset != null)
            {
                if (useAssetReportDate)
                {
                    rpt.PublishDate = DataManager.ConvertToDate(rowAsset[C_ColName_PublishDate]);
                    rpt.ReportDate = DataManager.ConvertToDate(rowAsset[C_ColName_ReportDate]);
                }

                rpt.TotalNetAsset = DataManager.ConvertToDouble(rowAsset[C_ColName_FundNetAsset]);          //单位：元
                rpt.TotalEquityAsset = DataManager.ConvertToDouble(rowAsset[C_ColName_FundEquityAsset]);    //单位：元
                rpt.TotalBondAsset = DataManager.ConvertToDouble(rowAsset[C_ColName_FundBondAsset]);        //单位：元

                rpt.ConvertableBondAsset = DataManager.ConvertToDouble(rowAsset[C_ColName_FundCBAsset]);    //单位：元
                rpt.PureBondAsset = rpt.TotalBondAsset - rpt.ConvertableBondAsset;                          //单位：元
            }

            rpt.TradeDate = rpt.PublishDate;
            r.OriginalTimeSeries.Add(rpt);
        }
        private void updateFundReport(DataSet dsShare, DataSet dsAsset, HistoryFundReport rpt)
        {
            //基本信息
            rpt.DataSource = this.DataSource;
            rpt.OriginalTimeSeries.Clear();

            string strSymbol = C_ColName_Code + "='" + rpt.Code + "'";
            DataRow[] rowsShare = dsShare.Tables[0].Select(strSymbol);
            DataRow[] rowsAsset = dsAsset.Tables[0].Select(strSymbol);

            //注意: 数据问题：
            //  1) 年报数据公布的较晚，在年报公布之前已有相关信息被披露，那么过时的数据将丢弃
            //  e.g. 166802.OF, 
            //      2013/1/8  公布了2013/1/4  的基金份额数据；[因折算]
            //      2013/1/22 公布了2012/12/31的基金份额数据；[因年报]    --这条因数据过期被丢弃
            //  2) 通常份额的更新数据多于资产配置的
            //  3) 所有序列按照报告日排序，非公告日

            DateTime dShare1, dAsset1;
            DateTime dShare0, dAsset0;
            DataRow rShare, rAsset;
            int iShare = 0, iAsset = 0;
            //按信息公布日期合并数据
            while (true)
            {
                //=======================
                //  准备数据
                //=======================
                //1.基金份额
                if (rowsShare.Length == 0)
                    rShare = null;
                else if (iShare >= rowsShare.Length)
                    rShare = rowsShare[rowsShare.Length - 1];
                else
                    rShare = rowsShare[iShare];

                if (rShare == null)
                    dShare1 = DateTime.Today.AddDays(1);
                else
                    dShare1 = DataManager.ConvertToDate(rShare[C_ColName_DeclareDate]);//本条数据的公告日

                if (iShare >= 1 && iShare < rowsShare.Length)
                {
                    //前一条数据的公告日（报告日更晚的）
                    dShare0 = DataManager.ConvertToDate(rowsShare[iShare - 1][C_ColName_DeclareDate]);
                    if (dShare1 >= dShare0)
                    {
                        //丢弃过期数据
                        iShare++;
                        continue;
                    }
                }

                //2.基金资产
                if (rowsAsset.Length == 0)
                    rAsset = null;
                else if (iAsset >= rowsAsset.Length)
                    rAsset = rowsAsset[rowsAsset.Length - 1];
                else
                    rAsset = rowsAsset[iAsset];

                if (rAsset == null)
                    dAsset1 = DateTime.Today.AddDays(1);
                else
                    dAsset1 = DataManager.ConvertToDate(rAsset[C_ColName_PublishDate]);

                if (iAsset >= 1 && iAsset < rowsAsset.Length)
                {
                    //前一条数据的公告日（报告日更晚的）
                    dAsset0 = DataManager.ConvertToDate(rowsAsset[iAsset - 1][C_ColName_PublishDate]);
                    if (dAsset1 >= dAsset0)
                    {
                        //丢弃过期数据
                        iAsset++;
                        continue;
                    }
                }

                //=======================
                //  合并数据
                //=======================
                if (rShare == null && rAsset == null)
                    break;

                if (dShare1 > dAsset1)
                {
                    if (iShare >= rowsShare.Length)
                    {
                        updateFundReport(null, rAsset, rpt, true);
                        iAsset++;
                    }
                    else
                        updateFundReport(rShare, rAsset, rpt, false);

                    iShare++;
                }
                else if (dShare1 < dAsset1)
                {
                    if (iAsset >= rowsAsset.Length)
                    {
                        updateFundReport(rShare, null, rpt, false);
                        iShare++;
                    }
                    else
                        updateFundReport(rShare, rAsset, rpt, true);

                    iAsset++;
                }
                else
                {
                    updateFundReport(rShare, rAsset, rpt, false);
                    iShare++;
                    iAsset++;
                }

                //=======================
                //  退出条件
                //=======================
                if (iShare >= rowsShare.Length && iAsset >= rowsAsset.Length)
                    break;
            }

            //交易日校验,复权
            rpt.Adjust();
        }
        #endregion
    }
}
