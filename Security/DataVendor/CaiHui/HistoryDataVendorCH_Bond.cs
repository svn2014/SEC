using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Data;
using System.Diagnostics;

namespace Security
{
    public partial class HistoryDataVendorCH : AHistoryDataVendor
    {
        #region 债券信息
        public override void LoadBondInfo(Bond b)
        {
            try
            {
                if (_BondInfo == null)
                {
                    #region SQL
                    string sql = @"SELECT  Abname AS " + C_ColName_Name + @",
                                    BONDDT10 AS " + C_ColName_CompanyCode + @",
                                    I.Industrychg4 AS " + C_ColName_Industry3 + @",
                                    H.Listdate AS " + C_ColName_ListedDateSH + @",
                                    Z.Listdate AS " + C_ColName_ListedDateSZ + @",
                                    BONDDT16 AS " + C_ColName_CodeSH + @",
                                    BONDDT17 AS " + C_ColName_CodeSZ + @",
                                    BONDDT47 AS " + C_ColName_CodeIB + @",
                                    BONDDT2 AS " + C_ColName_BondType + @", 
                                    BONDDT71 AS " + C_ColName_IsCorpBond + @",
                                    BONDDT19 AS " + C_ColName_IssueDate + @",
                                    BONDDT15 AS " + C_ColName_ListedDateIB + @", 
                                    BONDDT21 AS " + C_ColName_IntStartDate + @",
                                    BONDDT20 AS " + C_ColName_DelistedDate + @", 
                                    BONDDT7 AS " + C_ColName_InterestType + @",
                                    BONDDT23 AS " + C_ColName_IntAccruType + @",
                                    BONDDT28 AS " + C_ColName_IntNorminalRate + @",
                                    D.Columnvalue AS " + C_ColName_IntFloatRateBase + @",
                                    BONDDT30 AS " + C_ColName_IntFloatRateSpread + @",
                                    BONDDT50 AS " + C_ColName_IntPaymentDate + @",
                                    BONDDT27 AS " + C_ColName_IntPaymentFreq + @",
                                    BONDDT33 AS " + C_ColName_CreditRate + @",
                                    BONDDT40 AS " + C_ColName_EmbededOption + @",
                                    BONDDT34 AS " + C_ColName_Callable + @",
                                    BONDDT37 AS " + C_ColName_Putable + @",
                                    BONDDT57 AS " + C_ColName_Exchangable + @",
                                    BONDDT65 AS " + C_ColName_Splitable + @",
                                    BONDDT9 AS " + C_ColName_Issuer + @",
                                    C.Symbol AS " + C_ColName_IssuerStockCode + @",
                                    BONDDT18 AS " + C_ColName_ConvertStockCode + @",
                                    BONDDT62 AS " + C_ColName_CreditInnerEnhance + @",
                                    BONDDT63 AS " + C_ColName_CreditOuterEnhance + @",
                                    BONDDT11 AS " + C_ColName_Currency + @",
                                    BONDDT3 AS " + C_ColName_IssueTerm + @"
                                    FROM BONDDT B 
                                    LEFT JOIN (SELECT Symbol,SName,CompanyCode FROM SecurityCode WHERE SType IN ('EQA','EQB')) C
                                        ON B.BONDDT10=C.CompanyCode
                                    LEFT JOIN (Select * From SecurityCode where exchange = 'CNSESH') H
                                        ON B.BONDDT16=H.Symbol  
                                    LEFT JOIN (Select * From SecurityCode where exchange = 'CNSESZ') Z
                                        ON B.BONDDT17=Z.Symbol
                                    LEFT JOIN (SELECT * FROM DATADICT_COLUMNVALUE WHERE CID = 3591) D
                                    ON BONDDT46 = D.Columncode
                                    LEFT JOIN (
                                         SELECT A.Companycode, A.Industrychg4 
                                          FROM IndustryChg A
                                          INNER JOIN 
                                          (
                                                Select CompanyCode, IndustryChg3, MAX(PublishDate) AS PublishDate
                                                From IndustryChg 
                                                WHERE IndustryChg3 = '证监会行业分类(2012)'
                                                GROUP BY CompanyCode, IndustryChg3
                                          ) B
                                          ON A.Companycode = B.CompanyCode AND A.PublishDate = B.PublishDate AND A.IndustryChg3 = B.IndustryChg3
                                    ) I
                                    ON B.BONDDT10 = I.CompanyCode
                                    ";
                    #endregion
                    //获得所有股票的信息做缓存
                    _BondInfo = base.DBInstance.ExecuteSQL(sql);
                }

                #region SearchKey
                //更新数据
                string searchstring = "";
                if (b.Code.Trim().Length > 0)
                {
                    switch (b.Exchange)
                    {
                        case SecExchange.SHE:
                            searchstring = C_ColName_CodeSH + "='" + b.Code + "'";
                            break;
                        case SecExchange.SZE:
                            searchstring = C_ColName_CodeSZ + "='" + b.Code + "'";
                            break;
                        case SecExchange.IBM:
                            searchstring = C_ColName_CodeIB + "='" + b.Code + "'";
                            break;
                        default:
                            searchstring = C_ColName_Name + "='" + b.Name + "'";
                            break;
                    }
                }
                else
                {
                    searchstring = C_ColName_Name + "='" + b.Name + "'";
                }
                #endregion

                DataRow[] rows = _BondInfo.Tables[0].Select(searchstring);
                if (rows.Length >= 1)
                {
                    DataRow row = rows[0];
                    //基础信息
                    b.CompanyCode = row[C_ColName_CompanyCode].ToString(); 
                    b.Name = row[C_ColName_Name].ToString();
                    b.Industry = row[C_ColName_Industry3].ToString();
                    b.BondCodeSH = row[C_ColName_CodeSH].ToString();
                    b.BondCodeSZ = row[C_ColName_CodeSZ].ToString();
                    b.BondCodeIB = row[C_ColName_CodeIB].ToString();
                    //b.Code: 此为输入的代码，不可赋值

                    b.IssueDate = DataManager.ConvertToDate(row[C_ColName_IssueDate]);
                    b.Issuer = row[C_ColName_Issuer].ToString();
                    b.IssuerStockCode = row[C_ColName_IssuerStockCode].ToString();
                    b.IssueTerm = DataManager.ConvertToInt(row[C_ColName_IssueTerm]);
                    #region 债券分类
                    string subtype = row[C_ColName_BondType].ToString();
                        
                    switch (subtype)
                    {
                        case "01":  //01 国债 
                            b.SubType = BondType.Treasury;
                            break;
                        case "02":  //02 普通金融债 
                            b.SubType = BondType.Financial;
                            break;
                        case "03":  //03 普通企业债 
                            b.SubType = BondType.Enterprise;
                            string strIsCorp = row[C_ColName_IsCorpBond].ToString();
                            if (strIsCorp != "0")
                                b.SubType = BondType.Corporate; //公司债
                            break;
                        case "04":  //04 可转换企业债 
                            b.SubType = BondType.Convertable;
                            break;
                        case "06":  //06 可交换公司债券 
                            b.SubType = BondType.Exchangable;
                            break;
                        case "05":  //05 地方政府债券
                            b.SubType = BondType.Municipal;
                            break;
                        case "08":  //08 政策性金融债 
                            b.SubType = BondType.SpecialFinancial;
                            break;
                        case "10":  //10 政府支持机构债 
                        case "13":  //13 国际开发机构债券 
                            b.SubType = BondType.SponsedAgency;
                            break;
                        case "11":  //11 央行票据 
                            b.SubType = BondType.Central;
                            break;
                        case "14":  //14 信贷资产证券化
                        case "15":  //15 住房抵押贷款证券化 
                        case "16":  //16 券商专项资产管理 
                        case "17":  //17 汽车抵押贷款证券化 
                        case "18":  //18 资产支持票据 
                            b.SubType = BondType.AssetBacked;
                            break;
                        default:
                            b.SubType = BondType.Other;
                            break;
                    }
                    #endregion
                    #region 上市日期
                    b.ListedDateIB = DataManager.ConvertToDate(row[C_ColName_ListedDateIB]);    //银行间市场上市时间
                    if (b.BondCodeSH.Length > 0)    //交易所市场上市时间 或 银行间
                        b.ListedDate = DataManager.ConvertToDate(row[C_ColName_ListedDateSH]);
                    else if (b.BondCodeSZ.Length > 0)
                        b.ListedDate = DataManager.ConvertToDate(row[C_ColName_ListedDateSZ]);
                    else
                        b.ListedDate = b.ListedDateIB;
                    #endregion
                    b.MaturityDate = DataManager.ConvertToDate(row[C_ColName_DelistedDate]);
                    b.DelistedDate = b.MaturityDate;
                    b.Currency = row[C_ColName_Currency].ToString();
                    b.DataSource = this.DataSource;

                    //利息
                    #region 计息方法
                    string intaccrutype = row[C_ColName_IntAccruType].ToString();                    
                    switch (intaccrutype)
                    {
                        case "":          //null 贴现
                            b.IntAccruType = InterestType.Discount;
                            break;
                        case "零息":      //00 零息 
                            b.IntAccruType = InterestType.Zero;
                            break;
                        case "固定利率":  //10 固定利率 
                            b.IntAccruType = InterestType.Fixed;
                            break;
                        case "浮动利率":  //11 浮动利率 
                            b.IntAccruType = InterestType.Float;
                            break;
                        case "递进利率":  //12 递进利率
                            b.IntAccruType = InterestType.Stepped;
                            break;
                        default:         //13 含权债利率 
                            b.IntAccruType = InterestType.Other;
                            break;
                    }
                    #endregion
                    b.IntNominalRate = DataManager.ConvertToDouble(row[C_ColName_IntNorminalRate]) / 100;
                    b.IntFloatRateBase = row[C_ColName_IntFloatRateBase].ToString();
                    b.IntFloatRateSpread = DataManager.ConvertToDouble(row[C_ColName_IntFloatRateSpread]);
                    #region 浮动利率基准类型
                    if (b.IntAccruType == InterestType.Float)
                    {
                        if (b.IntFloatRateBase.Contains("活期存款"))
                            b.IntFloatRateBaseType = BaseRateType.CurrentDeposit;
                        else if (b.IntFloatRateBase.Contains("协定存款"))
                            b.IntFloatRateBaseType = BaseRateType.AgreementDeposit;
                        else if (b.IntFloatRateBase.Contains("通知存款"))
                            b.IntFloatRateBaseType = BaseRateType.CallDeposit;
                        else if (b.IntFloatRateBase.ToUpper().Contains("SHIBOR"))
                            b.IntFloatRateBaseType = BaseRateType.SHIBOR;
                        else if (b.IntFloatRateBase.ToUpper().Contains("LIBOR"))
                            b.IntFloatRateBaseType = BaseRateType.LIBOR;
                        else if (b.IntFloatRateBase.Contains("回购"))
                            b.IntFloatRateBaseType = BaseRateType.Repo;
                        else if (b.IntFloatRateBase.Contains("贷款"))
                            b.IntFloatRateBaseType = BaseRateType.LoanRate;
                        else if (b.IntFloatRateBase.Contains("存款"))
                        {
                            switch (b.IntFloatRateBase)
                            {
                                case "一个月存款":
                                case "三个月存款":
                                case "半年存款":
                                case "一年存款":
                                case "二年存款":
                                case "三年存款":
                                case "五年存款":
                                case "八年及以上存款":
                                    b.IntFloatRateBaseType = BaseRateType.TimeDeposit;
                                    break;
                                default:
                                    b.IntFloatRateBaseType = BaseRateType.Other;
                                    break;
                            }
                        }
                        else
                        {
                            b.IntFloatRateBaseType = BaseRateType.Other;
                        }
                    }
                    #endregion

                    b.IntStartDate = DataManager.ConvertToDate(row[C_ColName_IntStartDate]);                    
                    b.IntPaymentFreq = DataManager.ConvertToInt(row[C_ColName_IntPaymentFreq]);
                    b.IntPaymentDates = row[C_ColName_IntPaymentDate].ToString();
                    #region 利息支付日期
                    if (b.IntPaymentFreq == 0)
                    {
                        //IntPaymentDate = null
                        b.IntNextPaymentDate = b.MaturityDate;
                    }
                    else if (b.IntPaymentFreq == 1)
                    {
                        //IntPaymentDate = "10-28" ==> 默认本年：2013-10-28
                        b.IntNextPaymentDate = DataManager.ConvertToDate(b.IntPaymentDates);
                        if (b.IntNextPaymentDate < DateTime.Today)
                            b.IntNextPaymentDate = b.IntNextPaymentDate.AddYears(1);
                    }
                    else
                    {
                        //IntPaymentDate = "02-23,05-23,08-23,11-23"
                        string[] arydates = b.IntPaymentDates.Split(",".ToCharArray());
                        DateTime tmpdate;

                        b.IntNextPaymentDate = DateTime.Today.AddYears(1);
                        foreach (string strdate in arydates)
                        {
                            tmpdate = DataManager.ConvertToDate(strdate);
                            if (tmpdate < DateTime.Today)
                                tmpdate = tmpdate.AddYears(1);

                            if(tmpdate < b.IntNextPaymentDate)
                                b.IntNextPaymentDate = tmpdate;
                        }
                    }
                    #endregion

                    //评级和增信
                    int icie = DataManager.ConvertToInt(row[C_ColName_CreditInnerEnhance]);
                    int ocie = DataManager.ConvertToInt(row[C_ColName_CreditOuterEnhance]);
                    b.CreditInnerEnhance = (icie > 0) ? true : false;
                    b.CreditOuterEnhance = (ocie > 0) ? true : false;
                    b.CreditRates = row[C_ColName_CreditRate].ToString();   //可能存在多个评级同列
                    b.CreditRate = b.CreditRates;
                    if (b.CreditRate.Contains(","))
                    {
                        b.CreditRate = b.CreditRate.Split(",".ToCharArray())[0];
                    }

                    //内置期权
                    b.ConvertStockCode = row[C_ColName_ConvertStockCode].ToString();
                    string strembededoption = row[C_ColName_EmbededOption].ToString();
                    b.HasEmbededOption = (strembededoption == "0000") ? false : true;   //可赎回，可回售，可交换，可分离
                    strembededoption = row[C_ColName_Callable].ToString();
                    b.IsCallable = (strembededoption == "0") ? false : true;
                    strembededoption = row[C_ColName_Putable].ToString();
                    b.IsPutable = (strembededoption == "0") ? false : true;
                    strembededoption = row[C_ColName_Exchangable].ToString();
                    b.IsExchangable = (strembededoption == "0") ? false : true;
                    strembededoption = row[C_ColName_Splitable].ToString();
                    b.IsSplitable = (strembededoption == "0") ? false : true;
                }
                else
                {
                    MessageManager.GetInstance().AddMessage(MessageType.Warning, Message.C_Msg_BD5, b.Code);
                }
            }
            catch (Exception ex)
            {
                throw ex;
            }
        }
        #endregion

        #region 债券估值
        public override void LoadBondValue(ASecurityGroup g)
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
                string sql = @"SELECT TDate AS " + C_ColName_TradeDate + @"
                                , Exchange AS " + C_ColName_Exchange + @"
                                , Symbol AS " + C_ColName_Code + @"
                                , ClearPrice AS " + C_ColName_CleanPrice + @"
                                , DirtyPrice AS " + C_ColName_DirtyPrice + @"
                                , Accruedinterest AS " + C_ColName_AccruedInterest + @"
                                , TermToMaturity AS " + C_ColName_TermToMaturity + @"
                                , Yield AS " + C_ColName_Yield + @"
                                , Duration_mod AS " + C_ColName_ModifiedDuration + @"
                                , Convexity AS " + C_ColName_Convexity + @"
                                , Duration_dis AS " + C_ColName_Duration_dis + @"
                                , Convexity_dis AS " + C_ColName_Convexity_dis + @"
                                , Duration_int AS " + C_ColName_Duration_int + @"
                                , Convexity_int AS " + C_ColName_Convexity_int + @"
                                , DirtyPrice_day AS " + C_ColName_DirtyPrice_day + @"
                                , AccruedInterest_day AS " + C_ColName_AccruedInterest_day + @"
                                , BasisPoint_val AS " + C_ColName_BasisPoint_val + @"
                                , RemainingPrinc AS " + C_ColName_RemainingPrinc + @"
                                FROM BESTIMATE
                                WHERE DataSource =1 ";

                sql += " AND Tdate >= '" + g.TimeSeriesStartExtended.ToString("yyyyMMdd") + "' AND Tdate <= '" + g.TimeSeriesEnd.ToString("yyyyMMdd") + "'";
                string sqlIB = sql + " AND EXCHANGE = 'CNIBEX' AND SYMBOL IN (" + base.getCodeListString(g.SecurityList, SecExchange.IBM) + ")  ORDER BY Symbol, Tdate Desc";
                string sqlSH = sql + " AND EXCHANGE = 'CNSESH' AND SYMBOL IN (" + base.getCodeListString(g.SecurityList, SecExchange.SHE) + ")  ORDER BY Symbol, Tdate Desc";
                string sqlSZ = sql + " AND EXCHANGE = 'CNSESZ' AND SYMBOL IN (" + base.getCodeListString(g.SecurityList, SecExchange.SZE) + ") ORDER BY Symbol, Tdate Desc";

                DataSet dsIB = base.DBInstance.ExecuteSQL(sqlIB);
                DataSet dsSH = base.DBInstance.ExecuteSQL(sqlSH);
                DataSet dsSZ = base.DBInstance.ExecuteSQL(sqlSZ);

                //更新数据
                foreach (Bond b in g.SecurityList)
                {
                    DataSet ds = null;
                    switch (b.Exchange)
                    {
                        case SecExchange.SHE:
                            ds = dsSH;
                            break;
                        case SecExchange.SZE:
                            ds = dsSZ;
                            break;
                        case SecExchange.IBM:
                            ds = dsIB;
                            break;
                        default:
                            break;
                    }
                    this.updateBondValue(ds, b.HistoryBondIntrinsicValue);
                }
            }
            catch (Exception ex)
            {
                throw ex;
            }
        }
        private void updateBondValue(DataSet ds, HistoryBondValue values)
        {
            if (ds == null)
                return;

            DataRow[] rows = ds.Tables[0].Select(C_ColName_Code + "='" + values.Code + "'");
            if (rows.Length > 0)
            {
                //基本信息
                values.DataSource = this.DataSource;
                values.OriginalTimeSeries.Clear();

                foreach (DataRow row in rows)
                {
                    HistoryItemBondValue val = new HistoryItemBondValue();
                    val.TradeDate = DataManager.ConvertToDate(row[C_ColName_TradeDate]);
                    val.ClearPrice = DataManager.ConvertToDouble(row[C_ColName_CleanPrice]);
                    val.DirtyPrice = DataManager.ConvertToDouble(row[C_ColName_DirtyPrice]);
                    val.AccruedInterest = DataManager.ConvertToDouble(row[C_ColName_AccruedInterest]);
                    val.Yield = DataManager.ConvertToDouble(row[C_ColName_Yield])/100;
                    val.TermToMaturity = DataManager.ConvertToDouble(row[C_ColName_TermToMaturity]);    //年
                    val.ModifiedDuration = DataManager.ConvertToDouble(row[C_ColName_ModifiedDuration]);
                    val.Convexity = DataManager.ConvertToDouble(row[C_ColName_Convexity]);
                    val.SpreadDuration = DataManager.ConvertToDouble(row[C_ColName_Duration_dis]);
                    val.SpreadConvexity = DataManager.ConvertToDouble(row[C_ColName_Convexity_dis]);
                    val.InterestDuration = DataManager.ConvertToDouble(row[C_ColName_Duration_int]);
                    val.InterestConvexity = DataManager.ConvertToDouble(row[C_ColName_Convexity_int]);
                    val.CurrDayDirtyPrice = DataManager.ConvertToDouble(row[C_ColName_DirtyPrice_day]);
                    val.CurrDayAccruedInterest = DataManager.ConvertToDouble(row[C_ColName_AccruedInterest_day]);
                    val.BasePointValue = DataManager.ConvertToDouble(row[C_ColName_BasisPoint_val]);
                    val.RemainingPrinc = DataManager.ConvertToDouble(row[C_ColName_RemainingPrinc]) / 100;

                    values.OriginalTimeSeries.Add(val);
                }

                //交易日校验,复权并计算涨跌幅
                values.Adjust();
            }
            else
            {
                MessageManager.GetInstance().AddMessage(MessageType.Warning, Message.C_Msg_BD4, values.Code);
            }
        }
        #endregion

        #region 债券价格
        public override void LoadBondPrice(ASecurityGroup g, bool IsLoadAll)
        {
            //===============================
            //批量读取多个股票的数据，速度更快
            //===============================
            //注意同一个代码在不同的交易所可能重复
            //  e.g.125089.SH = 12中成债
            //      125089.SZ = 深机转债
            try
            {
                //如果没有持仓则退出
                if (g.SecurityList == null || g.SecurityCodes.Count == 0)
                    return;

                //读数据: 按时间降序排列
                string sql = @"SELECT TDate AS " + C_ColName_TradeDate + @"
                                , Exchange AS " + C_ColName_Exchange + @"
                                , Symbol AS " + C_ColName_Code + @"
                                ,SNAME AS " + C_ColName_Name + @"
                                ,AI AS " + C_ColName_AccruedInterest + @"
                                ,LCClose AS " + C_ColName_PreClose + @"
                                ,COpen AS " + C_ColName_Open + @"
                                ,CClose AS " + C_ColName_Close + @"
                                ,CHigh AS " + C_ColName_High + @"
                                ,CLow AS " + C_ColName_Low + @"
                                ,LDClose AS " + C_ColName_DirtyPrice_Preclose + @"
                                ,DOpen AS " + C_ColName_DirtyPrice_Open + @"
                                ,DClose AS " + C_ColName_DirtyPrice_Close + @"
                                ,DHigh AS " + C_ColName_DirtyPrice_High + @"
                                ,DLow AS " + C_ColName_DirtyPrice_Low + @"
                                ,VOTURNOVER AS " + C_ColName_Volume + @"
                                ,VATURNOVER AS " + C_ColName_Amount + @"
                                FROM BHDQUOTE
                                WHERE 1=1 ";

                //sql += " AND Tdate >= '" + g.TimeSeriesStartExtended.ToString("yyyyMMdd") + "' AND Tdate <= '" + g.TimeSeriesEnd.ToString("yyyyMMdd") + "'";
                sql += this.getDateRestrictSQL("Tdate", g, IsLoadAll);
                string sqlIB = sql + " AND EXCHANGE = 'CNIBEX' AND SYMBOL IN (" + base.getCodeListString(g.SecurityList, SecExchange.IBM) + ")  ORDER BY Symbol, Tdate Desc";
                string sqlSH = sql + " AND EXCHANGE = 'CNSESH' AND SYMBOL IN (" + base.getCodeListString(g.SecurityList, SecExchange.SHE) + ")  ORDER BY Symbol, Tdate Desc";
                string sqlSZ = sql + " AND EXCHANGE = 'CNSESZ' AND SYMBOL IN (" + base.getCodeListString(g.SecurityList, SecExchange.SZE) + ") ORDER BY Symbol, Tdate Desc";

                DataSet dsIB = base.DBInstance.ExecuteSQL(sqlIB);
                DataSet dsSH = base.DBInstance.ExecuteSQL(sqlSH);
                DataSet dsSZ = base.DBInstance.ExecuteSQL(sqlSZ);

                //更新数据
                this.updateTradePrice(dsSH, g, IsLoadAll, 0, SecExchange.SHE);
                this.updateTradePrice(dsSZ, g, IsLoadAll, 0, SecExchange.SZE);
                this.updateTradePrice(dsIB, g, IsLoadAll, 0, SecExchange.IBM);
            }
            catch (Exception ex)
            {
                throw ex;
            }
        }
        #endregion
    }
}
