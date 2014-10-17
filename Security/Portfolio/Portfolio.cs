using System;
using System.Collections.Generic;
using System.Data;

namespace Security.Portfolio
{
    public class Portfolio
    {
        public static Portfolio Create(List<PositionInfo> positionlist)
        {
            //==================================================
            //当估值表数据重复时
            //      SecurityGroup.Add函数可以自动将多余的数据过滤
            //==================================================
            if (positionlist == null || positionlist.Count == 0)
                return null;

            Portfolio portfolio = new Portfolio();
            portfolio.ReportDate = positionlist[0].TradingDay;
            portfolio.IsDataLoaded = true;
            foreach (PositionInfo p in positionlist)
            {
                switch (p.SecType)
                {
                    case SecurityType.Equity:
                        Equity e = new Equity(p.Code);
                        e.Name = p.Name;
                        e.Position = p;
                        portfolio.EquityHoldings.Add(e);    //重复数据会被自动过滤
                        break;
                    case SecurityType.Bond:
                        Bond b = new Bond(p.Code, p.Exchange);
                        b.Name = p.Name;
                        b.Position = p;
                        portfolio.BondHoldings.Add(b);
                        break;
                    case SecurityType.Fund:
                        MutualFund f = new MutualFund(p.Code);
                        f.Name = p.Name;
                        f.Position = p;
                        portfolio.FundHoldings.Add(f);
                        break;
                    case SecurityType.Deposit:
                        Deposit d = new Deposit(p.Code);
                        d.Name = p.Name;
                        d.Position = p;
                        if (p.Code == Deposit.C_Code_CurrentDeposit || p.Code == Deposit.C_Code_ClearSettlement || p.Code == Deposit.C_Code_ClearDeposit)
                            portfolio.CashHoldings.Add(d);
                        else if (p.Code == Deposit.C_Code_TimeDeposit)
                            portfolio.DepositHoldings.Add(d);
                        else
                            break;
                        break;
                    case SecurityType.RevRepo:  //逆回购
                        Repo r = new Repo(p.Code, SecurityType.RevRepo);
                        r.Name = p.Name;
                        r.Position = p;
                        portfolio.RevRepoHoldings.Add(r);
                        break;
                    case SecurityType.TheRepo:  //正回购
                        Repo t = new Repo(p.Code, SecurityType.TheRepo);
                        t.Name = p.Name;
                        t.Position = p;
                        portfolio.TheRepoHoldings.Add(t);
                        break;
                    default:
                        continue;
                }
            }

            //读取证券基础信息
            foreach (ASecurityGroup g in portfolio.SecurityGroupList)
            {
                g.LoadData(DataInfoType.SecurityInfo);
            }

            return portfolio;
        }

        //资产组
        public ASecurityGroup BondHoldings = new BondGroup();
        public ASecurityGroup EquityHoldings = new EquityGroup();
        public ASecurityGroup FundHoldings = new FundGroup();
        public ASecurityGroup DepositHoldings = new DepositGroup();
        public ASecurityGroup CashHoldings = new DepositGroup();
        public ASecurityGroup RevRepoHoldings = new RepoGroup();
        public ASecurityGroup TheRepoHoldings = new RepoGroup();
        public List<ASecurityGroup> SecurityGroupList = new List<ASecurityGroup>();
        public List<ASecurityGroup> PositionGroupList = new List<ASecurityGroup>();
        public List<ASecurityGroup> AllGroupList = new List<ASecurityGroup>();

        //组合指标
        public DateTime ReportDate = DataManager.C_Null_Date;
        public bool IsDataLoaded = true;
        public double Shares;
        public double NetAssetValue, UnitNetAssetValue, AccumUnitNetAssetValue;
        public double SubscribeRecievables, RedeemPayables, DividendRecievables;
        public double CashAvailable;
        public PositionInfo Position = new PositionInfo();
        
        //货币基金指标
        public double AverageMaturityDays;
        public double ValueDeviationPct;
        public double ReturnOn10000;
        public double YieldIn7Days;
        
        public Portfolio()
        {
            //证券=股票+债券+基金
            this.SecurityGroupList.Add(this.BondHoldings);
            this.SecurityGroupList.Add(this.EquityHoldings);
            this.SecurityGroupList.Add(this.FundHoldings);

            //持仓=股票+债券+基金+正回购+逆回购
            this.PositionGroupList.Add(this.BondHoldings);
            this.PositionGroupList.Add(this.EquityHoldings);
            this.PositionGroupList.Add(this.FundHoldings);
            this.PositionGroupList.Add(this.TheRepoHoldings);
            this.PositionGroupList.Add(this.RevRepoHoldings);

            //全部
            this.AllGroupList.Add(this.BondHoldings);
            this.AllGroupList.Add(this.EquityHoldings);
            this.AllGroupList.Add(this.FundHoldings);
            this.AllGroupList.Add(this.RevRepoHoldings);
            this.AllGroupList.Add(this.TheRepoHoldings);
            this.AllGroupList.Add(this.DepositHoldings);
            this.AllGroupList.Add(this.CashHoldings);
        }
        public ASecurityGroup GetSecurityGroup(SecurityType type)
        {
            switch (type)
            {
                case SecurityType.Equity:
                    return this.EquityHoldings;
                case SecurityType.Bond:
                    return this.BondHoldings;
                case SecurityType.Fund:
                    return this.FundHoldings;
                case SecurityType.TheRepo:
                    return this.TheRepoHoldings;
                case SecurityType.RevRepo:
                    return this.RevRepoHoldings;
                case SecurityType.Deposit:
                    return this.DepositHoldings;
                default:
                    return null;
            }
        }
        
        private double getSellPrice(ASecurity previousS, DateTime tradeday, Portfolio dataInfo)
        {
            //回购按1元计算
            if (previousS.Type == SecurityType.RevRepo || previousS.Type == SecurityType.TheRepo)
                return 1;

            //如果当期没有价格,则按上期市值计算
            double sellPx = previousS.Position.MarketValue / previousS.Position.Quantity;

            //如果价格数据存在，则按平均价格计算
            ASecurityGroup priceG = dataInfo.GetSecurityGroup(previousS.Type);
            if (dataInfo != null && priceG != null && priceG.SecurityList != null)
            {
                ASecurity priceS = priceG.SecurityList.Find(delegate(ASecurity s) { return s.Code == previousS.Code; });
                if (priceS != null && priceS.HistoryTradePrice != null && priceS.HistoryTradePrice.OriginalTimeSeries != null)
                {
                    //必须使用未复权的价格
                    AHistoryTimeItem priceitem = priceS.HistoryTradePrice.OriginalTimeSeries.Find(delegate(AHistoryTimeItem i) { return i.TradeDate == this.ReportDate; });
                    if (priceitem != null)
                        sellPx = ((HistoryItemTradingPrice)priceitem).Average;
                }
            }
            else
            {
                throw new Exception();
            }

            //返回估算价格
            return sellPx;
        }
        private double getAdjustedCost(double accountcost, double accruedinterest, ASecurity s)
        {
            if (s.Type != SecurityType.Bond)
                return accountcost;

            //仅对债券成本作调整
            //  （未上市的债券不作调整，由于应计利息不多，调整了影响也不大）
            //估值表记录的会计成本 = 交易成本 + 应收利息 * 20% (提前把税计入成本)
            //  则：交易成本 = 债券的会计成本 - 应收利息 * 20%
            //      估值表中的应收利息为税后，所以要先恢复到税前(除以0.8)再计算利息税
            return accountcost - accruedinterest / 0.8 * 0.2;
        }
        public void Calculate(Portfolio previousPortfolio, ref Portfolio dataInfo)
        { 
            //计算个券和组合收益率
            //dataInfo中保存有交易价格和分红信息，不用每次重新加载，节约时间；
            #region 个券
            #region 买入，增持，减持
            foreach (ASecurityGroup currentG in this.PositionGroupList)
            {
                if (currentG.SecurityList !=null && currentG.SecurityList.Count>0)
                {
                    ASecurityGroup previousG = previousPortfolio.GetSecurityGroup(currentG.SecurityList[0].Type);
                    foreach (ASecurity currentS in currentG.SecurityList)
                    {
                        try
                        {
                            ASecurity previousS = null;
                            if (previousG.SecurityList != null)
                                previousS = previousG.SecurityList.Find(delegate(ASecurity s) { return s.Code == currentS.Code; });

                            if (previousS == null || previousS.Position.Quantity == 0)
                            {
                                #region 买入
                                //=========================
                                //  收益 = 今日市值 - 今日成本
                                //  成本 = 今日成本
                                //  利息 = 0[理论上计头不计尾，但由于无法获得数据，设为0]
                                //=========================
                                currentS.Position.CurrentInterest = 0;  //理论上：当日应计利息=持有市值*票面利率/365
                                currentS.Position.RealizedReturn = 0;
                                currentS.Position.CurrentTotalCost = this.getAdjustedCost(currentS.Position.Cost, currentS.Position.AccruedInterest, currentS);
                                currentS.Position.UnrealizedReturn = currentS.Position.MarketValue - currentS.Position.CurrentTotalCost;
                                #endregion

                                //交易记录
                                currentS.Position.Trade(TradeAction.Buy, currentS.Position.Quantity, currentS.Position.Cost / currentS.Position.Quantity);
                                //记录开仓时间
                                currentS.Position.DateIn = this.ReportDate;
                            }
                            else
                            { 
                                if (currentS.Position.Quantity > previousS.Position.Quantity)
                                {
                                    #region 增持: TODO-需要考虑送股等因素造成的股本变化，若除权日正好遇到交易则容易出错
                                    //=========================
                                    //增持(新买入股份会分摊成本均价) 
                                    //  收益 = (今日市值  / 今日股份数 * 昨日股份数 - 昨日市值) 
                                    //      + (今日市值  / 今日股份数 - 今日买入价) * 今日买入股份
                                    //  成本 = 昨日市值 + 成本增加 = 昨日市值 + 今日买入价 * 今日买入股份
                                    //  利息 = (昨日对应部分今日应计利息 - 昨日应计利息) + 0[今日新买部分]
                                    //=========================
                                    currentS.Position.CurrentInterest = currentS.Position.AccruedInterest / currentS.Position.Quantity * previousS.Position.Quantity - previousS.Position.AccruedInterest;
                                    if (currentS.Position.CurrentInterest < 0)
                                        currentS.Position.CurrentInterest = currentS.Position.AccruedInterest;  //若当日应计利息 < 0 说明当日为付息日

                                    double buyPx = (currentS.Position.Cost - previousS.Position.Cost) / (currentS.Position.Quantity - previousS.Position.Quantity);
                                    currentS.Position.RealizedReturn = 0;
                                    currentS.Position.UnrealizedReturn = (currentS.Position.MarketValue / currentS.Position.Quantity * previousS.Position.Quantity - previousS.Position.MarketValue)
                                                            + (currentS.Position.MarketValue / currentS.Position.Quantity - buyPx) * (currentS.Position.Quantity - previousS.Position.Quantity);
                                    double tradecost = this.getAdjustedCost(
                                                            (currentS.Position.Cost - previousS.Position.Cost), 
                                                            currentS.Position.AccruedInterest * (1 - previousS.Position.Quantity / currentS.Position.Quantity),
                                                            currentS
                                                            );
                                    currentS.Position.CurrentTotalCost = previousS.Position.MarketValue + tradecost;
                                    #endregion

                                    //交易记录
                                    currentS.Position.Trade(TradeAction.Increase, currentS.Position.Quantity - previousS.Position.Quantity, buyPx);
                                }
                                else if (currentS.Position.Quantity < previousS.Position.Quantity)
                                {
                                    #region 减持: TODO-需要考虑送股等因素造成的股本变化，若除权日正好遇到交易则容易出错
                                    //=========================
                                    //减持(卖出股份形成实现收益计入银行存款)
                                    //  收益 = (今日均价 - 昨日收盘价) * 今日卖出股份                //实现
                                    //       + (今日市值 - 昨日市值 / 昨日股份数 * 今日股份数)       //浮动
                                    //  成本 = 昨日市值
                                    //  利息 = (今日应计利息 - 未出售部分昨日应计利息) + 0(今日卖出部分)
                                    //=========================
                                    double sellPx = this.getSellPrice(previousS, this.ReportDate, dataInfo);
                                    //currentS.Position.CurrentDividend 此前已经根据股权登记日调整完毕
                                    currentS.Position.CurrentInterest = currentS.Position.AccruedInterest - previousS.Position.AccruedInterest / previousS.Position.Quantity * currentS.Position.Quantity;
                                    if (currentS.Position.CurrentInterest < 0)
                                        currentS.Position.CurrentInterest = currentS.Position.AccruedInterest;  //若当日应计利息 < 0 说明当日为付息日
                                    currentS.Position.RealizedReturn = (sellPx - previousS.Position.MarketValue / previousS.Position.Quantity) * (previousS.Position.Quantity - currentS.Position.Quantity);
                                    currentS.Position.UnrealizedReturn = currentS.Position.MarketValue - previousS.Position.MarketValue / previousS.Position.Quantity * currentS.Position.Quantity;                                    
                                    currentS.Position.CurrentTotalCost = previousS.Position.MarketValue;
                                    #endregion

                                    //交易记录
                                    currentS.Position.Trade(TradeAction.Decrease, previousS.Position.Quantity - currentS.Position.Quantity, sellPx);
                                }
                                else
                                {
                                    #region 无变动
                                    //=========================
                                    //  收益 = 今日市值 - 昨日市值
                                    //  成本 = 昨日市值
                                    //  利息 = 今日应计利息 - 昨日应计利息
                                    //=========================
                                    currentS.Position.CurrentInterest = currentS.Position.AccruedInterest - previousS.Position.AccruedInterest;
                                    if (currentS.Position.CurrentInterest < 0)
                                        currentS.Position.CurrentInterest = currentS.Position.AccruedInterest;  //若当日应计利息 < 0 说明当日为付息日
                                    currentS.Position.RealizedReturn = 0;
                                    currentS.Position.UnrealizedReturn = currentS.Position.MarketValue - previousS.Position.MarketValue;
                                    currentS.Position.CurrentTotalCost = previousS.Position.MarketValue;
                                    #endregion                                
                                }

                                //记录开仓时间
                                currentS.Position.DateIn = previousS.Position.DateIn;
                            }

                            //收益率计算
                            currentS.Position.CurrentTotalReturn = currentS.Position.RealizedReturn + currentS.Position.UnrealizedReturn + currentS.Position.CurrentInterest + currentS.Position.CurrentDividend;
                            currentS.Position.CurrentYield = currentS.Position.CurrentTotalReturn / currentS.Position.CurrentTotalCost;

                            if (previousS != null)
                            {
                                currentS.Position.AccumulatedReturn = previousS.Position.AccumulatedReturn + currentS.Position.CurrentTotalReturn;
                                currentS.Position.AccumulatedYieldIndex = previousS.Position.AccumulatedYieldIndex * (1 + currentS.Position.CurrentYield);
                            }
                            else
                            {
                                currentS.Position.AccumulatedReturn = currentS.Position.CurrentTotalReturn;
                                currentS.Position.AccumulatedYieldIndex = (1 + currentS.Position.CurrentYield);
                            }
                        }
                        catch (Exception ex)
                        {
                            throw ex;
                        }
                    }
                }
            }
            #endregion

            #region 卖出
            foreach (ASecurityGroup previousG in previousPortfolio.PositionGroupList)
            {
                if (previousG.SecurityList != null && previousG.SecurityList.Count > 0)
                {
                    ASecurityGroup currentG = this.GetSecurityGroup(previousG.SecurityList[0].Type);
                    foreach (ASecurity previousS in previousG.SecurityList)
                    {
                        try
                        {
                            ASecurity currentS = null;
                            if (currentG.SecurityList != null)
                                currentS = currentG.SecurityList.Find(delegate(ASecurity s) { return s.Code == previousS.Code; });

                            if (currentS == null)
                            {
                                object[] paras = new object[] { previousS.Code, previousS.Exchange };
                                currentS = (ASecurity)Activator.CreateInstance(previousS.GetType(), paras);
                                currentS.Position.Code = currentS.Code;
                                currentS.Position.Name = currentS.Name = previousS.Name;
                                currentS.Position.SecType = currentS.Type;
                                currentS.Position.TradingDay = this.ReportDate;
                                currentS.Position.DateIn = previousS.Position.DateIn;

                                if (previousS.Position.Quantity == 0)
                                {
                                    //如果前期数量为0，说明早已卖出，只需记录结果
                                    currentS.Position = (PositionInfo)previousS.Position.Clone();
                                    currentS.Position.CurrentDividend = 0;
                                    currentS.Position.CurrentInterest = 0;
                                    currentS.Position.CurrentTotalCost = 0;
                                    currentS.Position.CurrentTotalReturn = 0;
                                    currentS.Position.CurrentYield = 0;
                                    currentS.Position.Transactions = new List<TransactionInfo>();
                                    currentS.Position.DateOut = previousS.Position.DateOut;
                                }
                                else
                                {
                                    #region 卖出
                                    double sellPx = this.getSellPrice(previousS, this.ReportDate, dataInfo);
                                    currentS.Position.CurrentInterest = 0;  //卖出不计利息
                                    //currentS.Position.CurrentDividend   此前已经根据股权登记日调整完毕
                                    currentS.Position.RealizedReturn = sellPx * previousS.Position.Quantity - previousS.Position.MarketValue;
                                    currentS.Position.UnrealizedReturn = 0;
                                    currentS.Position.CurrentTotalReturn = currentS.Position.RealizedReturn + currentS.Position.UnrealizedReturn
                                                                        + currentS.Position.CurrentInterest + currentS.Position.CurrentDividend;
                                    currentS.Position.AccumulatedReturn = previousS.Position.AccumulatedReturn + currentS.Position.CurrentTotalReturn;
                                    currentS.Position.CurrentTotalCost = previousS.Position.MarketValue;
                                    currentS.Position.CurrentYield = currentS.Position.CurrentTotalReturn / currentS.Position.CurrentTotalCost;
                                    currentS.Position.AccumulatedYieldIndex = previousS.Position.AccumulatedYieldIndex * (1 + currentS.Position.CurrentYield);
                                    #endregion

                                    //交易记录
                                    currentS.Position.Trade(TradeAction.Sell, previousS.Position.Quantity, sellPx);
                                    //记录平仓时间
                                    currentS.Position.DateOut = this.ReportDate;
                                }

                                currentG.Add(currentS);
                            }
                        }
                        catch (Exception ex)
                        {
                            throw ex;
                        }
                    }
                }
            }
            #endregion
            #endregion

            #region 股票：行业分组
            if (this.EquityHoldings != null && this.EquityHoldings.SecurityList != null && this.EquityHoldings.SecurityList.Count > 0)
            {
                EquityGroup egP = (EquityGroup)this.EquityHoldings;
                egP.IndustryClassify();

                //更新行业收益率
                foreach (EquityGroup eg in egP.IndustryList)
                {
                    EquityGroup feg = ((EquityGroup)previousPortfolio.EquityHoldings).IndustryList.Find(delegate(EquityGroup teg) { return teg.Name == eg.Name; });
                    if (feg != null)
                    {
                        //注：AccumulatedReturn已经计算过了，不要重复计算
                        eg.Position.AccumulatedYieldIndex *= feg.Position.AccumulatedYieldIndex;
                    }
                }
            }
            #endregion

            #region 投资组合
            this.Position = new PositionInfo();//初始化
            foreach (ASecurityGroup sg in this.AllGroupList)
            {
                if (sg.SecurityList == null || sg.SecurityList.Count == 0)
                    continue;

                //计算证券组的持仓与收益率
                sg.Calculate();
                ASecurityGroup g = previousPortfolio.GetSecurityGroup(sg.GetSecurityType());
                if (g != null)
                    sg.Position.AccumulatedYieldIndex *= g.Position.AccumulatedYieldIndex;

                //计算投资组合的持仓与收益率
                this.Position.Sum(sg.Position);
            }

            //加入应收股利合计（如果此前未作个股股利统计，则此处需要加入，无论加或不加并不对组合收益率的计算产生影响）
            //this.Position.CurrentTotalReturn += this.DividendRecievables;

            //复权单位净值增长率计算方式
            //Growth=UnitNAV(T)/(UnitNAV(T)-AccumUnitNAV(T)+AccumUnitNAV(T-1))-1;
            //注：AccumulatedReturn已经计算过了，不要重复计算
            this.Position.CurrentYield = this.UnitNetAssetValue / (this.UnitNetAssetValue - this.AccumUnitNetAssetValue + previousPortfolio.AccumUnitNetAssetValue) - 1;
            this.Position.AccumulatedYieldIndex = previousPortfolio.Position.AccumulatedYieldIndex * (1 + this.Position.CurrentYield);
            #endregion
        }

        #region 数据输出
        public DataTable GetPositionTable(SecurityType type)
        {
            DataTable dt = null;
            switch (type)
            {
                case SecurityType.Equity:
                    dt = this.GetEquityTable();
                    break;
                case SecurityType.Bond:
                    dt = this.GetBondTable();
                    break;
                case SecurityType.Fund:
                    dt = this.GetFundTable();
                    break;
                case SecurityType.TheRepo:
                    dt = this.GetTheRepoTable();
                    break;
                case SecurityType.RevRepo:
                    dt = this.GetRevRepoTable();
                    break;
                case SecurityType.Deposit:
                    dt = this.GetDepositTable();
                    break;
                default:
                    dt = this.GetPortfolioIndicator();
                    break;
            }

            return dt;
        }

        private DataTable _EquityTable;
        public DataTable GetEquityTable()
        {
            if (_EquityTable == null)
            {
                _EquityTable = new DataTable();
                _EquityTable.Columns.Add(AHistoryDataVendor.C_ColName_Code, typeof(string));
                _EquityTable.Columns.Add(AHistoryDataVendor.C_ColName_Name, typeof(string));
                _EquityTable.Columns.Add(AHistoryDataVendor.C_ColName_Industry1, typeof(string));
                _EquityTable.Columns.Add(AHistoryDataVendor.C_ColName_Industry2, typeof(string));
                _EquityTable.Columns.Add(AHistoryDataVendor.C_ColName_Industry3, typeof(string));
                _EquityTable.Columns.Add(AHistoryDataVendor.C_ColName_PreClose, typeof(double));
                _EquityTable.Columns.Add(AHistoryDataVendor.C_ColName_Quantity, typeof(double));
                _EquityTable.Columns.Add(AHistoryDataVendor.C_ColName_Cost, typeof(double));
                _EquityTable.Columns.Add(AHistoryDataVendor.C_ColName_MarketValue, typeof(double));
                _EquityTable.Columns.Add(AHistoryDataVendor.C_ColName_MarketValuePct, typeof(double));
                _EquityTable.Columns.Add(AHistoryDataVendor.C_ColName_HoldingPeriodReturn, typeof(double));
                _EquityTable.Columns.Add(AHistoryDataVendor.C_ColName_HoldingPeriodYield, typeof(double));
                _EquityTable.Columns.Add(AHistoryDataVendor.C_ColName_DateIn, typeof(string));
                _EquityTable.Columns.Add(AHistoryDataVendor.C_ColName_DateOut, typeof(string));
            }

            _EquityTable.Rows.Clear();
            if (this.EquityHoldings.SecurityList != null)
            {
                this.EquityHoldings.LoadData(DataInfoType.SecurityInfo);
                foreach (ASecurity s in this.EquityHoldings.SecurityList)
                {
                    DataRow r = _EquityTable.NewRow();
                    r[AHistoryDataVendor.C_ColName_Code] = s.Code;
                    r[AHistoryDataVendor.C_ColName_Name] = s.Name;
                    r[AHistoryDataVendor.C_ColName_Industry1] = ((Equity)s).Industry1;
                    r[AHistoryDataVendor.C_ColName_Industry2] = ((Equity)s).Industry2;
                    r[AHistoryDataVendor.C_ColName_Industry3] = ((Equity)s).Industry3;
                    r[AHistoryDataVendor.C_ColName_PreClose] = s.Position.CurrentPrice;
                    r[AHistoryDataVendor.C_ColName_Quantity] = s.Position.Quantity / 10000;         //万股
                    r[AHistoryDataVendor.C_ColName_Cost] = s.Position.Cost / 10000;                 //万元
                    r[AHistoryDataVendor.C_ColName_MarketValue] = s.Position.MarketValue / 10000;   //万元
                    r[AHistoryDataVendor.C_ColName_MarketValuePct] = s.Position.MarketValuePct;

                    r[AHistoryDataVendor.C_ColName_HoldingPeriodReturn] = s.Position.AccumulatedReturn / 10000; //万元
                    r[AHistoryDataVendor.C_ColName_HoldingPeriodYield] = s.Position.AccumulatedYieldIndex - 1;

                    //买入卖出时间
                    r[AHistoryDataVendor.C_ColName_DateIn] = (s.Position.DateIn > DataManager.C_Null_Date) ? s.Position.DateIn.ToString("yyyy-MM-dd") : "";
                    r[AHistoryDataVendor.C_ColName_DateOut] = (s.Position.DateOut > DataManager.C_Null_Date) ? s.Position.DateOut.ToString("yyyy-MM-dd") : "";

                    _EquityTable.Rows.Add(r);
                }
            }

            return _EquityTable;
        }

        private DataTable _BondTable;
        public DataTable GetBondTable()
        {
            if (_BondTable == null)
            {
                _BondTable = new DataTable();
                _BondTable.Columns.Add(AHistoryDataVendor.C_ColName_Code, typeof(string));
                _BondTable.Columns.Add(AHistoryDataVendor.C_ColName_Name, typeof(string));
                _BondTable.Columns.Add(AHistoryDataVendor.C_ColName_Industry3, typeof(string));
                _BondTable.Columns.Add(AHistoryDataVendor.C_ColName_Cost, typeof(double));
                _BondTable.Columns.Add(AHistoryDataVendor.C_ColName_MarketValue, typeof(double));
                _BondTable.Columns.Add(AHistoryDataVendor.C_ColName_MarketValuePct, typeof(double));
                _BondTable.Columns.Add(AHistoryDataVendor.C_ColName_IntNorminalRate, typeof(double));
                _BondTable.Columns.Add(AHistoryDataVendor.C_ColName_CreditRate, typeof(string));
                _BondTable.Columns.Add(AHistoryDataVendor.C_ColName_IntPaymentDate, typeof(DateTime));
                _BondTable.Columns.Add(AHistoryDataVendor.C_ColName_DelistedDate, typeof(DateTime));
                _BondTable.Columns.Add(AHistoryDataVendor.C_ColName_TermToMaturity, typeof(double));
                _BondTable.Columns.Add(AHistoryDataVendor.C_ColName_EmbededOption, typeof(string));                
                _BondTable.Columns.Add(AHistoryDataVendor.C_ColName_IssuerStockCode, typeof(string));
                _BondTable.Columns.Add(AHistoryDataVendor.C_ColName_PreClose, typeof(double));
                _BondTable.Columns.Add(AHistoryDataVendor.C_ColName_Quantity, typeof(double));
                _BondTable.Columns.Add(AHistoryDataVendor.C_ColName_ValueDeviation, typeof(double));
                _BondTable.Columns.Add(AHistoryDataVendor.C_ColName_HoldingPeriodReturn, typeof(double));
                _BondTable.Columns.Add(AHistoryDataVendor.C_ColName_HoldingPeriodYield, typeof(double));
                _BondTable.Columns.Add(AHistoryDataVendor.C_ColName_DateIn, typeof(string));
                _BondTable.Columns.Add(AHistoryDataVendor.C_ColName_DateOut, typeof(string));
            }

            if (this.BondHoldings.SecurityList != null)
            {
                this.BondHoldings.LoadData(DataInfoType.SecurityInfo);
                foreach (ASecurity s in this.BondHoldings.SecurityList)
                {
                    DataRow r = _BondTable.NewRow();
                    r[AHistoryDataVendor.C_ColName_Code] = s.Code;
                    r[AHistoryDataVendor.C_ColName_Name] = s.Name;
                    r[AHistoryDataVendor.C_ColName_Industry3] = ((Bond)s).Industry;
                    r[AHistoryDataVendor.C_ColName_Cost] = s.Position.Cost / 10000;               //万元
                    r[AHistoryDataVendor.C_ColName_MarketValue] = s.Position.MarketValue / 10000; //万元
                    r[AHistoryDataVendor.C_ColName_MarketValuePct] = s.Position.MarketValuePct;
                    r[AHistoryDataVendor.C_ColName_IntNorminalRate] = ((Bond)s).IntNominalRate;
                    r[AHistoryDataVendor.C_ColName_CreditRate] = ((Bond)s).CreditRate;
                    r[AHistoryDataVendor.C_ColName_IntPaymentDate] = ((Bond)s).IntNextPaymentDate;
                    r[AHistoryDataVendor.C_ColName_DelistedDate] = ((Bond)s).MaturityDate;
                    r[AHistoryDataVendor.C_ColName_TermToMaturity] = (((Bond)s).MaturityDate - DateTime.Today).Days / 365.0;
                    r[AHistoryDataVendor.C_ColName_EmbededOption] = ((Bond)s).HasEmbededOption ? "Y" : "";
                    r[AHistoryDataVendor.C_ColName_IssuerStockCode] = ((Bond)s).IssuerStockCode;
                    r[AHistoryDataVendor.C_ColName_PreClose] = s.Position.CurrentPrice;
                    r[AHistoryDataVendor.C_ColName_Quantity] = s.Position.Quantity / 10000; //万份

                    r[AHistoryDataVendor.C_ColName_HoldingPeriodReturn] = s.Position.AccumulatedReturn / 10000; //万元
                    r[AHistoryDataVendor.C_ColName_HoldingPeriodYield] = s.Position.AccumulatedYieldIndex - 1;

                    //买入卖出时间
                    r[AHistoryDataVendor.C_ColName_DateIn] = (s.Position.DateIn > DataManager.C_Null_Date) ? s.Position.DateIn.ToString("yyyy-MM-dd") : "";
                    r[AHistoryDataVendor.C_ColName_DateOut] = (s.Position.DateOut > DataManager.C_Null_Date) ? s.Position.DateOut.ToString("yyyy-MM-dd") : "";

                    //估值偏离
                    double basevalue = s.Position.MarketValue / s.Position.Quantity;
                    double estimatevalue = 0;
                    if (((Bond)s).HistoryBondIntrinsicValue.AdjustedTimeSeries != null)
                        estimatevalue = ((HistoryItemBondValue)((Bond)s).HistoryBondIntrinsicValue.AdjustedTimeSeries[0]).ClearPrice;
                    double deviation = Math.Abs(estimatevalue / basevalue - 1);
                    r[AHistoryDataVendor.C_ColName_ValueDeviation] = deviation;

                    _BondTable.Rows.Add(r);
                }
            }

            return _BondTable;
        }

        private DataTable _FundTable;
        public DataTable GetFundTable()
        {
            if (_FundTable == null)
            {
                _FundTable = new DataTable();
                _FundTable.Columns.Add(AHistoryDataVendor.C_ColName_Code, typeof(string));
                _FundTable.Columns.Add(AHistoryDataVendor.C_ColName_Name, typeof(string));
                _FundTable.Columns.Add(AHistoryDataVendor.C_ColName_Quantity, typeof(double));
                _FundTable.Columns.Add(AHistoryDataVendor.C_ColName_PreClose, typeof(double));
                _FundTable.Columns.Add(AHistoryDataVendor.C_ColName_UnitNAV, typeof(double));
                _FundTable.Columns.Add(AHistoryDataVendor.C_ColName_Cost, typeof(double));
                _FundTable.Columns.Add(AHistoryDataVendor.C_ColName_MarketValue, typeof(double));
                _FundTable.Columns.Add(AHistoryDataVendor.C_ColName_MarketValuePct, typeof(double));
                _FundTable.Columns.Add(AHistoryDataVendor.C_ColName_DateIn, typeof(string));
                _FundTable.Columns.Add(AHistoryDataVendor.C_ColName_DateOut, typeof(string));
            }

            _FundTable.Rows.Clear();
            if (this.FundHoldings.SecurityList != null)
            {
                this.FundHoldings.LoadData(DataInfoType.SecurityInfo);
                this.FundHoldings.LoadData(DataInfoType.HistoryFundNAV);
                foreach (ASecurity s in this.FundHoldings.SecurityList)
                {
                    DataRow r = _FundTable.NewRow();
                    r[AHistoryDataVendor.C_ColName_Code] = s.Code;
                    r[AHistoryDataVendor.C_ColName_Name] = s.Name;
                    r[AHistoryDataVendor.C_ColName_PreClose] = s.Position.CurrentPrice;
                    r[AHistoryDataVendor.C_ColName_Quantity] = s.Position.Quantity / 10000;         //万股
                    r[AHistoryDataVendor.C_ColName_Cost] = s.Position.Cost / 10000;                 //万元
                    r[AHistoryDataVendor.C_ColName_MarketValue] = s.Position.MarketValue / 10000;   //万元
                    r[AHistoryDataVendor.C_ColName_MarketValuePct] = s.Position.MarketValuePct;
                    
                    //买入卖出时间
                    r[AHistoryDataVendor.C_ColName_DateIn] = (s.Position.DateIn > DataManager.C_Null_Date) ? s.Position.DateIn.ToString("yyyy-MM-dd") : "";
                    r[AHistoryDataVendor.C_ColName_DateOut] = (s.Position.DateOut > DataManager.C_Null_Date) ? s.Position.DateOut.ToString("yyyy-MM-dd") : "";

                    //净值
                    double nav = 1;
                    try
                    {
                        nav = ((HistoryItemNetAssetValue)((MutualFund)s).HistoryTradeNAV.AdjustedTimeSeries[0]).UnitNAV;
                    }
                    catch (Exception)
                    {}
                    r[AHistoryDataVendor.C_ColName_UnitNAV] = nav;

                    _FundTable.Rows.Add(r);
                }
            }

            return _FundTable;
        }

        private DataTable _DepositTable;
        public DataTable GetDepositTable()
        {
            if (_DepositTable == null)
            {
                _DepositTable = new DataTable();
                _DepositTable.Columns.Add(AHistoryDataVendor.C_ColName_Code, typeof(string));
                _DepositTable.Columns.Add(AHistoryDataVendor.C_ColName_Name, typeof(string));
                _DepositTable.Columns.Add(AHistoryDataVendor.C_ColName_MarketValue, typeof(double));
                _DepositTable.Columns.Add(AHistoryDataVendor.C_ColName_MarketValuePct, typeof(double));
                _DepositTable.Columns.Add(AHistoryDataVendor.C_ColName_DateIn, typeof(string));
                _DepositTable.Columns.Add(AHistoryDataVendor.C_ColName_DateOut, typeof(string));
            }

            if (this.DepositHoldings.SecurityList != null)
            {
                foreach (ASecurity s in this.DepositHoldings.SecurityList)
                {
                    DataRow r = _DepositTable.NewRow();
                    r[AHistoryDataVendor.C_ColName_Code] = s.Code;
                    r[AHistoryDataVendor.C_ColName_Name] = s.Name;
                    r[AHistoryDataVendor.C_ColName_MarketValue] = s.Position.MarketValue / 10000; //万元
                    r[AHistoryDataVendor.C_ColName_MarketValuePct] = s.Position.MarketValuePct;

                    //买入卖出时间
                    r[AHistoryDataVendor.C_ColName_DateIn] = (s.Position.DateIn > DataManager.C_Null_Date) ? s.Position.DateIn.ToString("yyyy-MM-dd") : "";
                    r[AHistoryDataVendor.C_ColName_DateOut] = (s.Position.DateOut > DataManager.C_Null_Date) ? s.Position.DateOut.ToString("yyyy-MM-dd") : "";

                    _DepositTable.Rows.Add(r);
                }
            }

            if (this.CashHoldings.SecurityList != null)
            {
                foreach (ASecurity s in this.CashHoldings.SecurityList)
                {
                    DataRow r = _DepositTable.NewRow();
                    r[AHistoryDataVendor.C_ColName_Code] = s.Code;
                    r[AHistoryDataVendor.C_ColName_Name] = s.Name;
                    r[AHistoryDataVendor.C_ColName_MarketValue] = s.Position.MarketValue / 10000; //万元
                    r[AHistoryDataVendor.C_ColName_MarketValuePct] = s.Position.MarketValuePct;
                    _DepositTable.Rows.Add(r);
                }
            }

            return _DepositTable;
        }

        private DataTable _RevRepoTable, _TheRepoTable;
        public DataTable GetRevRepoTable()
        {
            if (_RevRepoTable == null)
            {
                _RevRepoTable = new DataTable();
                _RevRepoTable.Columns.Add(AHistoryDataVendor.C_ColName_Code, typeof(string));
                _RevRepoTable.Columns.Add(AHistoryDataVendor.C_ColName_Name, typeof(string));
                _RevRepoTable.Columns.Add(AHistoryDataVendor.C_ColName_MarketValue, typeof(double));
                _RevRepoTable.Columns.Add(AHistoryDataVendor.C_ColName_MarketValuePct, typeof(double));
                _RevRepoTable.Columns.Add(AHistoryDataVendor.C_ColName_DateIn, typeof(string));
                _RevRepoTable.Columns.Add(AHistoryDataVendor.C_ColName_DateOut, typeof(string));
            }

            if (this.RevRepoHoldings.SecurityList != null)
            {
                foreach (ASecurity s in this.RevRepoHoldings.SecurityList)
                {
                    DataRow r = _RevRepoTable.NewRow();
                    r[AHistoryDataVendor.C_ColName_Code] = s.Code;
                    r[AHistoryDataVendor.C_ColName_Name] = s.Name;
                    r[AHistoryDataVendor.C_ColName_MarketValue] = s.Position.MarketValue / 10000; //万元
                    r[AHistoryDataVendor.C_ColName_MarketValuePct] = s.Position.MarketValuePct;

                    //买入卖出时间
                    r[AHistoryDataVendor.C_ColName_DateIn] = (s.Position.DateIn > DataManager.C_Null_Date) ? s.Position.DateIn.ToString("yyyy-MM-dd") : "";
                    r[AHistoryDataVendor.C_ColName_DateOut] = (s.Position.DateOut > DataManager.C_Null_Date) ? s.Position.DateOut.ToString("yyyy-MM-dd") : "";

                    _RevRepoTable.Rows.Add(r);
                }
            }

            return _RevRepoTable;
        }
        public DataTable GetTheRepoTable()
        {
            if (_TheRepoTable == null)
            {
                _TheRepoTable = new DataTable();
                _TheRepoTable.Columns.Add(AHistoryDataVendor.C_ColName_Code, typeof(string));
                _TheRepoTable.Columns.Add(AHistoryDataVendor.C_ColName_Name, typeof(string));
                _TheRepoTable.Columns.Add(AHistoryDataVendor.C_ColName_MarketValue, typeof(double));
                _TheRepoTable.Columns.Add(AHistoryDataVendor.C_ColName_MarketValuePct, typeof(double));
                _TheRepoTable.Columns.Add(AHistoryDataVendor.C_ColName_DateIn, typeof(string));
                _TheRepoTable.Columns.Add(AHistoryDataVendor.C_ColName_DateOut, typeof(string));
            }

            if (this.TheRepoHoldings.SecurityList != null)
            {
                foreach (ASecurity s in this.TheRepoHoldings.SecurityList)
                {
                    DataRow r = _TheRepoTable.NewRow();
                    r[AHistoryDataVendor.C_ColName_Code] = s.Code;
                    r[AHistoryDataVendor.C_ColName_Name] = s.Name;
                    r[AHistoryDataVendor.C_ColName_MarketValue] = -s.Position.MarketValue / 10000; //万元
                    r[AHistoryDataVendor.C_ColName_MarketValuePct] = -s.Position.MarketValuePct;

                    //买入卖出时间
                    r[AHistoryDataVendor.C_ColName_DateIn] = (s.Position.DateIn > DataManager.C_Null_Date) ? s.Position.DateIn.ToString("yyyy-MM-dd") : "";
                    r[AHistoryDataVendor.C_ColName_DateOut] = (s.Position.DateOut > DataManager.C_Null_Date) ? s.Position.DateOut.ToString("yyyy-MM-dd") : "";

                    _TheRepoTable.Rows.Add(r);
                }
            }

            return _TheRepoTable;
        }

        private DataTable _PortfolioIndicator;
        public DataTable GetPortfolioIndicator()
        {
            if (_PortfolioIndicator == null)
            {
                _PortfolioIndicator = new DataTable();
                _PortfolioIndicator.Columns.Add(AHistoryDataVendor.C_ColName_Name, typeof(string));
                _PortfolioIndicator.Columns.Add(AHistoryDataVendor.C_ColName_MarketValue, typeof(string));
            }

            DataRow r = _PortfolioIndicator.NewRow();
            r[AHistoryDataVendor.C_ColName_Name] = "基金份额/净资产";
            r[AHistoryDataVendor.C_ColName_MarketValue] = (this.Shares/10000).ToString("N2") + "万份/" + (this.NetAssetValue/10000).ToString("N2") + "万元";
            _PortfolioIndicator.Rows.Add(r);

            r = _PortfolioIndicator.NewRow();
            r[AHistoryDataVendor.C_ColName_Name] = "单位/累计净值";
            r[AHistoryDataVendor.C_ColName_MarketValue] = this.UnitNetAssetValue.ToString("N4") + "/" + this.AccumUnitNetAssetValue.ToString("N4");
            _PortfolioIndicator.Rows.Add(r);

            r = _PortfolioIndicator.NewRow();
            r[AHistoryDataVendor.C_ColName_Name] = "申购/两日赎回";
            r[AHistoryDataVendor.C_ColName_MarketValue] = (this.SubscribeRecievables / 10000).ToString("N2") + "万元/" + (-this.RedeemPayables / 10000).ToString("N2") + "万元";
            _PortfolioIndicator.Rows.Add(r);

            r = _PortfolioIndicator.NewRow();
            r[AHistoryDataVendor.C_ColName_Name] = "可用头寸";
            r[AHistoryDataVendor.C_ColName_MarketValue] = (this.CashAvailable/10000).ToString("N2") + "万元";
            _PortfolioIndicator.Rows.Add(r);

            if (this.AverageMaturityDays > 0 || this.YieldIn7Days > 0)
            {
                //货币基金专用
                r = _PortfolioIndicator.NewRow();
                r[AHistoryDataVendor.C_ColName_Name] = "平均剩余期限";
                r[AHistoryDataVendor.C_ColName_MarketValue] = this.AverageMaturityDays.ToString("N0") + "天";
                _PortfolioIndicator.Rows.Add(r);

                r = _PortfolioIndicator.NewRow();
                r[AHistoryDataVendor.C_ColName_Name] = "偏离度";
                r[AHistoryDataVendor.C_ColName_MarketValue] = this.ValueDeviationPct.ToString("P2");
                _PortfolioIndicator.Rows.Add(r);

                r = _PortfolioIndicator.NewRow();
                r[AHistoryDataVendor.C_ColName_Name] = "万份收益";
                r[AHistoryDataVendor.C_ColName_MarketValue] = this.ReturnOn10000.ToString("N4") + "元";
                _PortfolioIndicator.Rows.Add(r);

                r = _PortfolioIndicator.NewRow();
                r[AHistoryDataVendor.C_ColName_Name] = "7日年化";
                r[AHistoryDataVendor.C_ColName_MarketValue] = this.YieldIn7Days.ToString("P2");
                _PortfolioIndicator.Rows.Add(r);
            }

            return _PortfolioIndicator;
        }
        #endregion

        public void Print()
        {
            this.EquityHoldings.Print();
        }
    }
}
