using System;
using System.Collections.Generic;
using System.Data;
using System.Diagnostics;

namespace Security.Portfolio
{
    public class PortfolioAdapterYSS: IPortfolioDataAdapter
    {
        #region 估值表字段
        public const string C_GZB_ColName_FDate = "FDATE";              //日期
        public const string C_GZB_ColName_FKmbm = "FKMBM";              //科目编码
        public const string C_GZB_ColName_FKmmc = "FKMMC";              //科目名称
        public const string C_GZB_ColName_FHqjg = "FHQJG";              //行情价格
        public const string C_GZB_ColName_FZqsl = "FZQSL";              //证券数量
        public const string C_GZB_ColName_FZqcb = "FZQCB";              //证券成本
        public const string C_GZB_ColName_FZqsz = "FZQSZ";              //证券市值
        public const string C_GZB_ColName_FGz_zz = "FGZ_ZZ";            //估值增值
        public const string C_GZB_ColName_FSz_Jz_bl = "FSZ_JZ_BL";      //市值占基金净值比例(以%的字符串形式表示)
        public const string C_GZB_ColName_FCb_Jz_bl = "FCB_JZ_BL";      //成本占基金净值比例(以%的字符串形式表示)
        #endregion

        #region 会计科目
        public const string C_GZB_KMBM_ACCTYPE_COST = "01"; //成本
        public const string C_GZB_KMBM_ACCTYPE_VALA = "99"; //增值

        #region 1级科目-4位数字
        public const string C_GZB_KMBM_Bond = "1103";       //债券投资
        public const string C_GZB_KMBM_Fund = "1105";       //基金投资
        public const string C_GZB_KMBM_Equity = "1102";     //股票投资
        public const string C_GZB_KMBM_Warrant = "1106";    //权证投资

        public const string C_GZB_KMBM_TheRepo = "2202";    //卖出回购证券款  ：正回购
        public const string C_GZB_KMBM_RevRepo = "1202";    //买入返售金融资产：逆回购

        public const string C_GZB_KMBM_Deposit = "1002";    //银行存款
        public const string C_GZB_KMBM_CurrentDeposit = "100201";
        public const string C_GZB_KMBM_TimeDeposit = "100202";
        public const string C_GZB_KMBM_Clearing = "1021";   //清算备付金
        public const string C_GZB_KMBM_Margin = "1031";     //结算保证金
        public const string C_GZB_KMBM_Settlement = "3003"; //证券清算款

        public const string C_GZB_KMBM_Interest = "1204";   //应收利息
        public const string C_GZB_KMBM_Dividend = "1203";   //应收股利
        #endregion

        #region 2级科目-6位数字
        //股票，债券，权证
        public const string C_GZB_KMBM_Equity_SH_AG = "110201"; //上交所A股
        public const string C_GZB_KMBM_Equity_SH_XG = "110203"; //上交所新股
        public const string C_GZB_KMBM_Equity_SH_ZF = "110204"; //上交所增发
        public const string C_GZB_KMBM_Equity_SH_PG = "110205"; //上交所配股
        public const string C_GZB_KMBM_Equity_SH_FG = "110206"; //上交所非公开新股
        public const string C_GZB_KMBM_Bond_SH_GZ = "110311"; //上交所国债
        public const string C_GZB_KMBM_Bond_SH_ZZ = "110312"; //上交所可转债
        public const string C_GZB_KMBM_Bond_SH_QZ = "110313"; //上交所企业债
        public const string C_GZB_KMBM_Bond_SH_PT = "110321"; //上交所固定收益平台国债
        public const string C_GZB_KMBM_Bond_SH_UZ = "110372"; //上交所未上市可转债
        public const string C_GZB_KMBM_Bond_SH_UQ = "110373"; //上交所未上市企业债
        public const string C_GZB_KMBM_Warrant_SH_EU = "110611"; //上交所欧式认购权证
        public const string C_GZB_KMBM_Warrant_SH_UU = "110641"; //上交所未上市欧式认购权证
        public const string C_GZB_KMBM_ETP_SH = "110501";        //上交所基金
        public const string C_GZB_KMBM_Equity_SZ_AG = "110231"; //深交所A股
        public const string C_GZB_KMBM_Equity_SZ_XG = "110233"; //深交所新股
        public const string C_GZB_KMBM_Equity_SZ_ZF = "110234"; //深交所新股增发
        public const string C_GZB_KMBM_Equity_SZ_PG = "110235"; //深交所配股
        public const string C_GZB_KMBM_Equity_SZ_CYB = "110237"; //深交所创业板股票
        public const string C_GZB_KMBM_Equity_SZ_CYN = "110241"; //深交所创业板新股
        public const string C_GZB_KMBM_Bond_SZ_GZ = "110331"; //深交所国债
        public const string C_GZB_KMBM_Bond_SZ_ZZ = "110332"; //深交所可转债
        public const string C_GZB_KMBM_Bond_SZ_QZ = "110333"; //深交所企业债券
        public const string C_GZB_KMBM_Bond_SZ_PP = "110376"; //深交所私募债
        public const string C_GZB_KMBM_Bond_SZ_UZ = "110378"; //深交所未上市可转债
        public const string C_GZB_KMBM_Bond_SZ_UQ = "110379"; //深交所未上市企业债
        public const string C_GZB_KMBM_Warrant_SZ_EU = "110613"; //深交所欧式认购权证
        public const string C_GZB_KMBM_Warrant_SZ_BM = "110621"; //深圳百慕大认购权证
        public const string C_GZB_KMBM_ETP_SZ = "110502";        //深交所基金
        public const string C_GZB_KMBM_Bond_IB_GZ = "110351"; //银行间国债
        public const string C_GZB_KMBM_Bond_IB_QZ = "110353"; //银行间企业债
        public const string C_GZB_KMBM_Bond_IB_JR = "110354"; //银行间金融债
        public const string C_GZB_KMBM_Bond_IB_YP = "110355"; //银行间央行票据
        public const string C_GZB_KMBM_Bond_IB_ZC = "110369"; //银行间政策性金融债        
        public const string C_GZB_KMBM_Bond_IB_DR= "110357";  //银行间短期融资券    

        public const string C_GZB_KMBM_Fund_OTC = "110504";  //基金投资

        //回购
        public const string C_GZB_KMBM_RevRepo_SH = "120201"; //上交所回购
        public const string C_GZB_KMBM_RevRepo_SZ = "120202"; //深交所回购
        public const string C_GZB_KMBM_RevRepo_IB = "120203"; //银行间回购
        public const string C_GZB_KMBM_TheRepo_SH = "220201"; //上交所回购
        public const string C_GZB_KMBM_TheRepo_SZ = "220202"; //深交所回购
        public const string C_GZB_KMBM_TheRepo_IB = "220203"; //银行间回购

        //利息
        public const string C_GZB_KMBM_Interest_Deposit = "120401";     //应收银行存款利息
        public const string C_GZB_KMBM_Interest_ClearDeposit = "120402";    //应收清算备付金利息
        public const string C_GZB_KMBM_Interest_ClearMargin = "120403";      //应收结算保证金利息
        public const string C_GZB_KMBM_Interest_TA = "120404";          //应收TA申购款利息
        public const string C_GZB_KMBM_Interest_Bond = "120410";        //应收债券利息
        //public const string C_GZB_KMBM_Interest_Repo = "120491";        //应收买入返售利息
        #endregion
        
        #region 3级科目-8位数字
        public const string C_GZB_KMBM_DepositInterest_Current = "12040101";//应收活期存款利息
        public const string C_GZB_KMBM_DepositInterest_Time = "12040102";   //应收定期存款利息
        //债券利息
        public const string C_GZB_KMBM_BondInterest_SH_Govn = "12041011";   //应收上交所国债利息
        public const string C_GZB_KMBM_BondInterest_SH_CB = "12041012";     //应收上交所可转债利息
        public const string C_GZB_KMBM_BondInterest_SH_Corp = "12041013";   //应收上交所企业债利息
        public const string C_GZB_KMBM_BondInterest_SH_GovnFP = "12041021"; //应收上交所固定平台国债利息
        public const string C_GZB_KMBM_BondInterest_SH_CBUL = "12041072";   //应收上交所未上市可转债利息
        public const string C_GZB_KMBM_BondInterest_SH_CorpUL = "12041073"; //应收上交所未上市企业债利息
        public const string C_GZB_KMBM_BondInterest_SZ_Govn = "12041031";   //应收深交所国债利息
        public const string C_GZB_KMBM_BondInterest_SZ_CB = "12041032";     //应收深交所可转债利息
        public const string C_GZB_KMBM_BondInterest_SZ_Corp = "12041033";   //应收深交所企业债利息
        public const string C_GZB_KMBM_BondInterest_SZ_CorpUL = "12041079";	//应收深交所未上市企业债利息
        public const string C_GZB_KMBM_BondInterest_IB_Govn = "12041051";   //应收银行间国债利息
        public const string C_GZB_KMBM_BondInterest_IB_Corp = "12041053";   //应收银行间企业债利息
        public const string C_GZB_KMBM_BondInterest_IB_Fina = "12041054";   //应收银行间普通金融债券利息
        public const string C_GZB_KMBM_BondInterest_IB_Cent = "12041055";   //应收银行间央行票据利息
        public const string C_GZB_KMBM_BondInterest_IB_Poli = "12041069";   //应收银行间政策性金融债利息
        public const string C_GZB_KMBM_BondInterest_IB_Comm = "12041057";   //应收银行间短期融资券利息
        //回购利息        
        //12049111	应收上交所质押式买入返售利息
        //12049112	应收银行间买断式买入利息
        //12049151	应收银行间质押式回购利息
        #endregion
        #endregion

        private double Shares;
        private double NetAssetValue, UnitNetAssetValue, AccumUnitNetAssetValue;
        private double SubscribeRecievables, RedeemPayables, DividendRecievables;
        private double CashAvailable;
        private double AverageMaturityDays;
        private double ValueDeviationPct;
        private double ReturnOn10000;
        private double YieldIn7Days;
        private List<PositionInfo> InterestList = new List<PositionInfo>();
                
        private void clearPortfolioData()
        {
            this.Shares = 0;
            this.NetAssetValue = this.UnitNetAssetValue = this.AccumUnitNetAssetValue = 0;
            this.SubscribeRecievables = this.RedeemPayables = 0;
            this.CashAvailable = 0;

            this.AverageMaturityDays = 0;
            this.ValueDeviationPct = 0;
            this.ReturnOn10000 = 0;
            this.YieldIn7Days = 0;
        }

        private void saveInterest(PositionInfo p)
        {
            try
            {
                string level1code, level2code;            
                level1code = p.Code.Substring(0, 4);
                if (level1code != C_GZB_KMBM_Interest)
                    return;

                if (p.Code.Length >= 6)
                    level2code = p.Code.Substring(0, 6);
                else
                    return;

                PositionInfo pInt = new PositionInfo();
                switch (level2code)
                {
                    case C_GZB_KMBM_Interest_ClearDeposit:  //清算备付金利息
                        if (p.Code.Length != 6)
                            break;
                        pInt.Code = Deposit.C_Code_ClearDeposit;
                        break;
                    case C_GZB_KMBM_Interest_ClearMargin:    //结算保证金利息
                        if (p.Code.Length != 6)
                            break;
                        pInt.Code = Deposit.C_Code_ClearMargin;
                        break;
                    case C_GZB_KMBM_Interest_Deposit:   //存款利息
                        //存款利息代码有8位
                        if (p.Code.Length != 8)
                            break;

                        switch (p.Code)
	                    {
                            case C_GZB_KMBM_DepositInterest_Current:    //活期存款
                                pInt.Code = Deposit.C_Code_CurrentDeposit;
                                break;
                            case C_GZB_KMBM_DepositInterest_Time:       //定期存款
                                pInt.Code = Deposit.C_Code_TimeDeposit;
                                break;
                            default:
                                MessageManager.GetInstance().AddMessage(MessageType.Warning, Message.C_Msg_GE4, p.Name);
                                throw new Exception(Message.C_Msg_GE4);
	                    }
                        break;                
                    case C_GZB_KMBM_Interest_Bond:      //债券利息
                        //债券利息代码有14位
                        if (p.Code.Length != 14)
                            break;
                        pInt.Code = p.Code.Substring(8, 6);
                        pInt.Code = reverseCode(pInt.Code);
                        pInt.Exchange = this.getExchange(p.Code.Substring(0, 8), p.Name);
                        break;
                    default:
                        //应收TA申购款利息
                        //应收买入返售利息
                        //  应收银行间质押式回购利息
                        break;
                }

                if (pInt.Code != null && pInt.Code.Length > 0)
                {
                    pInt.TradingDay = p.TradingDay;
                    pInt.Name = p.Name;
                    pInt.MarketValue = p.MarketValue;
                    this.InterestList.Add(pInt);
                }
            }
            catch (Exception ex)
            {
                throw ex;
            }
        }

        private void updateInterest(ref List<PositionInfo> positionlist)
        {
            foreach (PositionInfo p in positionlist)
            {
                try
                {
                    if (p.Code == Deposit.C_Code_ClearDeposit
                    || p.Code == Deposit.C_Code_ClearMargin
                    || p.Code == Deposit.C_Code_CurrentDeposit
                    || p.Code == Deposit.C_Code_TimeDeposit
                    )
                    {
                        PositionInfo pInt = this.InterestList.Find(delegate(PositionInfo pi) { return (pi.Code == p.Code && pi.TradingDay == p.TradingDay); });
                        if (pInt != null)
                            p.AccruedInterest = pInt.MarketValue;
                    }
                    else
                    {
                        PositionInfo pInt = this.InterestList.Find(delegate(PositionInfo pi) { return (pi.Code == p.Code && pi.Exchange == p.Exchange && pi.TradingDay == p.TradingDay); });
                        if (pInt != null)
                            p.AccruedInterest = pInt.MarketValue;
                    }
                }
                catch (Exception ex)
                {
                    throw ex;
                }
            }
        }

        private List<PositionInfo> loadPositionData(DataTable dtGZB, DateTime reportdate)
        {
            try
            {
                this.clearPortfolioData();
                List<PositionInfo> rawdatalist = this.loadFundAccountData(dtGZB, reportdate);
                if (rawdatalist == null || rawdatalist.Count == 0)
                    return null;

                string level1code;
                string depositcode = "";
                List<PositionInfo> positionlist = new List<PositionInfo>();                
                foreach (PositionInfo item in rawdatalist)
                {
                    #region 组合指标
                    if (item.Code.Contains("601实收"))
                    {
                        this.Shares = item.MarketValue;
                    }
                    else if (item.Code.Contains("701基金资产净值"))
                    {
                        this.NetAssetValue = item.MarketValue;
                    }
                    else if (item.Code.Contains("702基金单位净值"))
                    {
                        this.UnitNetAssetValue = item.MarketValue;
                        if (this.AccumUnitNetAssetValue == 0)
                            this.AccumUnitNetAssetValue = this.UnitNetAssetValue;
                    }
                    else if (item.Code.Contains("905累计单位净值"))
                    {
                        this.AccumUnitNetAssetValue = DataManager.ConvertToDouble(item.Name);
                    }
                    else if (item.Code.Contains("804偏离度"))  //货币基金专用
                    {
                        this.ValueDeviationPct = DataManager.ConvertToDouble(item.Name);
                    }
                    else if (item.Code.Contains("908投资组合平均到期日"))  //货币基金专用
                    {
                        this.AverageMaturityDays = DataManager.ConvertToDouble(item.Name);
                    }
                    else if (item.Code.Contains("907基金七日收益率"))  //货币基金专用
                    {
                        this.YieldIn7Days = DataManager.ConvertToDouble(item.Name);
                    }
                    else if (item.Code.Contains("902每万份基金收益"))  //货币基金专用
                    {
                        this.ReturnOn10000 = DataManager.ConvertToDouble(item.Name);
                    }
                    else if (item.Code.Contains("512应收申购基金款"))
                    {
                        this.SubscribeRecievables = item.MarketValue;
                    }
                    else if (item.Code.Contains("513应付赎回基金款"))
                    {
                        this.RedeemPayables = item.MarketValue;
                    }
                    else if (item.Code.Contains("520今日可用头寸"))
                    {
                        this.CashAvailable = item.MarketValue;
                    }
                    else if (item.Code == C_GZB_KMBM_Dividend)
                    {
                        //应收股利无细分统计
                        this.DividendRecievables = item.MarketValue;
                    }
                    #endregion

                    #region 持仓证券
                    SecurityType secType;
                    level1code = item.Code.Substring(0, 4);
                    #region 证券类别
                    switch (level1code)
                    {
                        case C_GZB_KMBM_TheRepo:
                            secType = SecurityType.TheRepo;
                            break;

                        case C_GZB_KMBM_RevRepo:
                            secType = SecurityType.RevRepo;
                            break;

                        case C_GZB_KMBM_Bond:
                            secType = SecurityType.Bond;
                            break;

                        case C_GZB_KMBM_Fund:
                            secType = SecurityType.Fund;
                            break;

                        case C_GZB_KMBM_Equity:
                            secType = SecurityType.Equity;
                            break;

                        case C_GZB_KMBM_Warrant:
                            secType = SecurityType.Warrant;
                            break;

                        case C_GZB_KMBM_Deposit:
                            if (item.Code.Length == 6)
                            {
                                secType = SecurityType.Deposit;
                                switch (item.Code)
	                            {
                                    case C_GZB_KMBM_CurrentDeposit:
                                        depositcode = Deposit.C_Code_CurrentDeposit;
                                        break;
                                    case C_GZB_KMBM_TimeDeposit:
                                        depositcode = Deposit.C_Code_TimeDeposit;
                                        break;
                                    default:
                                        break;
	                            }
                            }
                            else
                            {
                                secType = SecurityType.Other;
                                continue;
                            }
                            break;
                        case C_GZB_KMBM_Clearing:
                            if (item.Code.Length == 4)
                            {
                                depositcode = Deposit.C_Code_ClearDeposit;
                                secType = SecurityType.Deposit;
                            }
                            else
                            {
                                secType = SecurityType.Other;
                                continue;
                            }
                            break;
                        case C_GZB_KMBM_Margin:
                            if (item.Code.Length == 4)
                            {
                                depositcode = Deposit.C_Code_ClearMargin;
                                secType = SecurityType.Deposit;
                            }
                            else
                            {
                                secType = SecurityType.Other;
                                continue;
                            }
                            break;
                        case C_GZB_KMBM_Settlement:
                            if (item.Code.Length == 4)
                            {
                                depositcode = Deposit.C_Code_ClearSettlement;
                                secType = SecurityType.Deposit;
                            }
                            else
                            {
                                secType = SecurityType.Other;
                                continue;
                            }
                            break;
                        case C_GZB_KMBM_Interest:   //应收利息
                            this.saveInterest(item);
                            continue;
                        default:
                            secType = SecurityType.Other;
                            continue;
                    }
                    #endregion

                    PositionInfo p = new PositionInfo();
                    p.SecType = secType;
                    p.Name = item.Name;
                    p.TradingDay = item.TradingDay;
                    p.CurrentPrice = item.CurrentPrice;
                    p.ItemCode = item.Code;

                    switch (secType)
                    {
                        case SecurityType.Deposit:
                            p.Code = depositcode;
                            p.Exchange = SecExchange.OTC;
                            break;
                        case SecurityType.RevRepo:
                        case SecurityType.TheRepo:
                            #region 回购
                            if (item.Code.Length == 14)
                            {
                                p.Code = item.Code.Substring(8, 6);
                                p.Exchange = this.getExchange(item.Code.Substring(0, 6), item.Name);
                            }
                            else
                                continue;
                            #endregion
                            break;
                        case SecurityType.Bond:
                        case SecurityType.Fund:
                        case SecurityType.Equity:
                        case SecurityType.Warrant:
                            #region 股债基权
                            //股票债券基金权证明细：仅需要成本项
                            //  01=成本；99=增值；
                            if (item.Code.Length == 14 && item.Code.Substring(6, 2) == C_GZB_KMBM_ACCTYPE_COST)
                            {
                                p.Code = item.Code.Substring(8, 6);
                                p.Exchange = this.getExchange(item.Code.Substring(0, 6), item.Name);

                                //关于增发配股：
                                //  增发配股的票会设立单独的科目，其成本为配股价，其市值为除权后的市场价*数量
                                //  计算中将合并相关的成本市值数据
                                PositionInfo fp = positionlist.Find(delegate(PositionInfo tp) { return tp.Code == p.Code && tp.Exchange == p.Exchange && tp.ItemCode != p.ItemCode; });
                                if (fp != null)
                                    p = fp; //项目已存在，多个项目和并计算
                            }
                            else
                                continue;
                            #endregion
                            break;
                        default:
                            break;
                    }

                    //赋值
                    p.Cost += item.Cost;
                    p.Quantity += item.Quantity;
                    p.MarketValue += item.MarketValue;
                    p.ValueAdded += item.ValueAdded;
                    p.CostPct += item.CostPct;
                    p.MarketValuePct += item.MarketValuePct;                    

                    //回购的Quantity缺少数据
                    if (p.SecType == SecurityType.TheRepo || p.SecType == SecurityType.RevRepo)
                    {
                        p.Quantity = p.MarketValue;
                    }

                    //债券代码反变换
                    if (p.SecType == SecurityType.Bond)
                    {
                        p.Code = this.reverseCode(p.Code);
                    }

                    positionlist.Add(p);
                    #endregion
                }

                //获得持仓明细后更新利息数据
                this.updateInterest(ref positionlist);

                return positionlist;
            }
            catch (Exception ex)
            {
                MessageManager.GetInstance().AddMessage(MessageType.Error, Message.C_Msg_PD3, "");
                throw new Exception(Message.C_Msg_PD3 + ":" + ex.Message, ex);
            }
        }

        private List<PositionInfo> loadFundAccountData(DataTable dtGZB, DateTime reportdate)
        {
            DataRow[] selectedRows = dtGZB.Select(C_GZB_ColName_FDate + "='" + reportdate.ToString("yyyy-MM-dd") + "'");

            //==================================================================================================
            //字段名
            //FDate, FKmbm, FKmmc, FHqjg, FZqsl, FZqcb, FZqsz, Gz_zz, FSz_Jz_bl, FCb_Jz_bl
            //日期,编码，名称，价格，数量，成本，市值，增值，市值-净值比例，成本-净值比例
            //==================================================================================================
            List<PositionInfo> accountlist = new List<PositionInfo>();

            try
            {
                accountlist.Clear();

                if (selectedRows != null && selectedRows.Length > 0)
                {
                    foreach (DataRow oRow in selectedRows)
                    {
                        PositionInfo gzb = new PositionInfo();
                        string pct = "";

                        foreach (DataColumn oCol in dtGZB.Columns)
                        {
                            if (oRow[oCol.ColumnName] == DBNull.Value)
                                continue;

                            string colNameAdjusted = oCol.ColumnName.ToUpper();
                            switch (colNameAdjusted)
                            {
                                case C_GZB_ColName_FDate:
                                    gzb.TradingDay = Convert.ToDateTime(oRow[oCol.ColumnName]);
                                    break;

                                case C_GZB_ColName_FKmbm:
                                    gzb.Code = oRow[oCol.ColumnName].ToString();
                                    break;

                                case C_GZB_ColName_FKmmc:
                                    gzb.Name = oRow[oCol.ColumnName].ToString();
                                    break;

                                case C_GZB_ColName_FZqsl:
                                    gzb.Quantity = Convert.ToDouble(oRow[oCol.ColumnName]);
                                    break;

                                case C_GZB_ColName_FZqcb:
                                    gzb.Cost = Convert.ToDouble(oRow[oCol.ColumnName]);
                                    break;

                                case C_GZB_ColName_FZqsz:
                                    gzb.MarketValue = Convert.ToDouble(oRow[oCol.ColumnName]);
                                    break;

                                case C_GZB_ColName_FGz_zz:
                                    gzb.ValueAdded = Convert.ToDouble(oRow[oCol.ColumnName]);
                                    break;

                                case C_GZB_ColName_FHqjg:
                                    gzb.CurrentPrice = Convert.ToDouble(oRow[oCol.ColumnName]);
                                    break;

                                case C_GZB_ColName_FCb_Jz_bl:
                                    //比例：原值="15.23%",新值="0.1523"                                    
                                    if (oRow[oCol.ColumnName] != DBNull.Value)
                                    {
                                        pct = oRow[oCol.ColumnName].ToString();
                                        pct = pct.Substring(0, pct.Length - 1);
                                        gzb.CostPct = Convert.ToDouble(pct) / 100;
                                    }
                                    break;

                                case C_GZB_ColName_FSz_Jz_bl:
                                    //比例：原值="15.23%",新值="0.1523"   
                                    if (oRow[oCol.ColumnName] != DBNull.Value)
                                    {
                                        pct = oRow[oCol.ColumnName].ToString();
                                        pct = pct.Substring(0, pct.Length - 1);
                                        gzb.MarketValuePct = Convert.ToDouble(pct) / 100;
                                    }
                                    break;

                                default:
                                    break;
                            }
                        }

                        accountlist.Add(gzb);
                    }
                }

                return accountlist;
            }
            catch (Exception ex)
            {
                MessageManager.GetInstance().AddMessage(MessageType.Error, Message.C_Msg_PD1, "");
                throw new Exception(Message.C_Msg_PD1, ex);
            }
        }

        private string reverseCode(string code)
        {
            //====================================================
            //银行间代码 转 6位估值表代码 [赢时胜方案,可能不准确]
            //1）中债登7位：年份(2)|类别或机构(2)|发行期数(3)
            //      年份(2): 
            //      机构(2): 不变(2)
            //      期数(3): 
            //2) 上清所9位：类别(2)|年份(2)|机构(2)|发行期数(3)
            //      类别(2): 转变(1)，01-25 >> A-Z 或 10-35 >> A-Z 
            //      年份(2): 转变(1)，10-35 >> A-Z
            //      机构(2): 不变(2)
            //      期数(3): 转变(2)，001-099 >> 01-99；10x-35x >> Ax-Zx
            //
            //反编码：变通方案：10年以后，年份都会转变为字母
            //1）第1、2位都是字母，首字母A=1，次字母A=10
            //2) 仅第2位是字母，A=10
            //====================================================
            string category, year, num, newcode;
            char[] codearray = code.ToUpper().ToCharArray();
            char c1 = codearray[0];
            char c2 = codearray[1];
            char c4 = codearray[4];

            category = c1.ToString();
            year = c2.ToString();
            newcode = code;

            if (c1 >= 'A')
            {
                if (c2 >= 'A')
                {
                    //前两位都是字母: 9位代码
                    //  首字母：A=1，次字母：A=10
                    category = (c1 - 'A' + 1).ToString().PadLeft(2, '0');
                    year = (c2 - 'A' + 10).ToString().PadLeft(2, '0');

                    if (c4 >= 'A')
                        num = (c2 - 'A' + 10).ToString() + code.Substring(5);
                    else
                        num = '0' + code.Substring(4);

                    newcode = category + year + code.Substring(2, 2) + num;
                }
                else
                { 
                    //仅第一位是字母：7位代码
                    //  字母：A=10
                    year = (c1 - 'A' + 10).ToString().PadLeft(2, '0');
                    newcode = year + code.Substring(1);
                }
            }
            else
            {
                if (c2 >= 'A')
                {
                    //仅第二位是字母: 9位代码
                    //  字母：A=10
                    category = "0" + c1.ToString();
                    year = (c2 - 'A' + 10).ToString().PadLeft(2, '0');
                    if (c4 >= 'A')
                        num = (c2 - 'A' + 10).ToString() + code.Substring(5);
                    else
                        num = '0' + code.Substring(4);

                    newcode = category + year + code.Substring(2, 2) + num;
                }
                else
                {
                    //无需转换
                }
            }

            return newcode;
        }

        public PortfolioGroup BuildPortfolios(DataTable dtGZB, DateTime start, DateTime end)
        {
            try
            {
                //检查并顺序排序
                List<DateTime> innerbanktradingdays = null;
                List<DateTime> exchangetradingdays = DataManager.GetTradingDays(start, end, ref innerbanktradingdays);

                if (exchangetradingdays == null || exchangetradingdays.Count == 0)
                    return null;

                exchangetradingdays.Sort(); //从小到大排序
                innerbanktradingdays.Sort();
                PortfolioGroup group = new PortfolioGroup();
                group.ExchangeTradingDays = exchangetradingdays;
                group.InnerBankTradingDays = innerbanktradingdays;

                foreach (DateTime d in exchangetradingdays)
                {
                    List<PositionInfo> positionlist = this.loadPositionData(dtGZB, d);
                    Portfolio p = Portfolio.Create(positionlist);

                    if (p != null)
                    {
                        foreach (ASecurityGroup sg in p.AllGroupList)
                        {
                            //计算证券组的持仓与收益率
                            sg.Calculate();
                        }

                        p.Shares = this.Shares;
                        p.NetAssetValue = this.NetAssetValue;
                        p.UnitNetAssetValue = this.UnitNetAssetValue;
                        p.AccumUnitNetAssetValue = this.AccumUnitNetAssetValue;
                        p.AverageMaturityDays = this.AverageMaturityDays;
                        p.ValueDeviationPct = this.ValueDeviationPct;
                        p.ReturnOn10000 = this.ReturnOn10000;
                        p.YieldIn7Days = this.YieldIn7Days;
                        p.SubscribeRecievables = this.SubscribeRecievables;
                        p.RedeemPayables = this.RedeemPayables;
                        p.DividendRecievables = this.DividendRecievables;
                        p.CashAvailable = this.CashAvailable;
                    }
                    else
                    {
                        p = new Portfolio();
                        p.ReportDate = d;
                        p.IsDataLoaded = false;
                    }

                    group.Add(p);
                }

                return group;
            }
            catch (Exception ex)
            {
                throw ex;
            }
        }

        private SecExchange getExchange(string code, string name)
        {            
            switch (code)
            {
                #region 上交所
                case C_GZB_KMBM_TheRepo_SH:
                case C_GZB_KMBM_RevRepo_SH:
                case C_GZB_KMBM_Equity_SH_AG:           //证券类别-2级代码
                case C_GZB_KMBM_Equity_SH_XG:
                case C_GZB_KMBM_Equity_SH_ZF:
                case C_GZB_KMBM_Equity_SH_PG:
                case C_GZB_KMBM_Equity_SH_FG:
                case C_GZB_KMBM_Bond_SH_GZ:
                case C_GZB_KMBM_Bond_SH_ZZ:
                case C_GZB_KMBM_Bond_SH_QZ:
                case C_GZB_KMBM_Bond_SH_PT:
                case C_GZB_KMBM_Bond_SH_UZ:
                case C_GZB_KMBM_Bond_SH_UQ:
                case C_GZB_KMBM_Warrant_SH_EU:
                case C_GZB_KMBM_Warrant_SH_UU:
                case C_GZB_KMBM_BondInterest_SH_Govn:   //债息类别-3级代码
                case C_GZB_KMBM_BondInterest_SH_CB:
                case C_GZB_KMBM_BondInterest_SH_Corp:
                case C_GZB_KMBM_BondInterest_SH_GovnFP:
                case C_GZB_KMBM_BondInterest_SH_CorpUL:
                case C_GZB_KMBM_BondInterest_SH_CBUL:
                case C_GZB_KMBM_ETP_SH:
                #endregion
                    return SecExchange.SHE;

                #region 深交所
                case C_GZB_KMBM_TheRepo_SZ:
                case C_GZB_KMBM_RevRepo_SZ:
                case C_GZB_KMBM_Equity_SZ_AG:           //证券类别-2级代码
                case C_GZB_KMBM_Equity_SZ_XG:
                case C_GZB_KMBM_Equity_SZ_ZF:
                case C_GZB_KMBM_Equity_SZ_PG:
                case C_GZB_KMBM_Equity_SZ_CYB:
                case C_GZB_KMBM_Equity_SZ_CYN:
                case C_GZB_KMBM_Bond_SZ_GZ:
                case C_GZB_KMBM_Bond_SZ_ZZ:
                case C_GZB_KMBM_Bond_SZ_QZ:
                case C_GZB_KMBM_Bond_SZ_UZ:
                case C_GZB_KMBM_Bond_SZ_UQ:
                case C_GZB_KMBM_Bond_SZ_PP:
                case C_GZB_KMBM_Warrant_SZ_EU:
                case C_GZB_KMBM_Warrant_SZ_BM:
                case C_GZB_KMBM_BondInterest_SZ_Govn:   //债息类别-3级代码
                case C_GZB_KMBM_BondInterest_SZ_CB:
                case C_GZB_KMBM_BondInterest_SZ_Corp:
                case C_GZB_KMBM_BondInterest_SZ_CorpUL:
                case C_GZB_KMBM_ETP_SZ:
                #endregion
                    return SecExchange.SZE;
                
                #region 银行间
                case C_GZB_KMBM_TheRepo_IB:
                case C_GZB_KMBM_RevRepo_IB:
                case C_GZB_KMBM_Bond_IB_GZ:             //证券类别-2级代码
                case C_GZB_KMBM_Bond_IB_QZ:
                case C_GZB_KMBM_Bond_IB_JR:
                case C_GZB_KMBM_Bond_IB_YP:
                case C_GZB_KMBM_Bond_IB_ZC:
                case C_GZB_KMBM_Bond_IB_DR:
                case C_GZB_KMBM_BondInterest_IB_Govn:   //债息类别-3级代码
                case C_GZB_KMBM_BondInterest_IB_Corp:
                case C_GZB_KMBM_BondInterest_IB_Fina:
                case C_GZB_KMBM_BondInterest_IB_Cent:
                case C_GZB_KMBM_BondInterest_IB_Poli:
                case C_GZB_KMBM_BondInterest_IB_Comm:
                #endregion
                    return SecExchange.IBM;

                #region 场外
                case C_GZB_KMBM_Fund_OTC:
                #endregion
                    return SecExchange.OTC;

                default:
                    MessageManager.GetInstance().AddMessage(MessageType.Warning, Message.C_Msg_GE4, name);
                    throw new Exception(Message.C_Msg_GE4);
            }
        }
    }
}
