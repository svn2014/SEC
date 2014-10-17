using System;
using System.Data;
using System.Collections.Generic;

namespace Security
{
    public partial class HistoryDataVendorCH
    {
        #region 股票信息
        public override void LoadEquityInfo(Equity e)
        {
            try
            {
                if (_EquityInfo == null)
                {
                    //对于2011年的申万行业分类：B.Style = '707',E.Typecode = '3'
                    //对于2014年的申万行业分类：B.Style = '741',E.Typecode = '7'
                    string style = "741", typecode = "7";
                    string sql = @"SELECT A.SYMBOL AS " + C_ColName_Code + @"
                            , A.Exchange AS " + C_ColName_Exchange + @"
                            , A.SNAME AS " + C_ColName_Name + @"
                            , A.LISTDATE AS " + C_ColName_ListedDate + @"
                            , A.ENDDATE AS " + C_ColName_DelistedDate + @"
                            , B.Cindustry2 AS " + C_ColName_Industry1 + @"
                            , C.Cindustry2 AS " + C_ColName_Industry2 + @"
                            , D.Cindustry2 AS " + C_ColName_Industry3 + @"
                            , E.Symbol AS " + C_ColName_IndustryIndex + @"
                            FROM SECURITYCODE A
                            LEFT JOIN CINDUSTRY B
                                 ON A.Companycode = B.Companycode AND B.Style = '" + style + @"' AND substr(B.StyleCode,3,4)='0000'
                            LEFT JOIN CINDUSTRY C
                                 ON A.Companycode = C.Companycode AND C.Style = '" + style + @"' AND substr(C.StyleCode,3,4)!='0000' AND substr(C.StyleCode,5,2)='00'
                            LEFT JOIN CINDUSTRY D
                                 ON A.Companycode = D.Companycode AND D.Style = '" + style + @"' AND substr(D.StyleCode,3,4)!='0000' AND substr(D.StyleCode,5,2)!='00'
                            LEFT JOIN Iindex_Comp E
                                 ON B.Stylecode = E.Industrycode AND E.Typecode = '" + typecode + @"'
                            WHERE STYPE = 'EQA' ";

                    //获得所有股票的信息做缓存
                    _EquityInfo = base.DBInstance.ExecuteSQL(sql);
                }

                //更新数据
                DataRow[] rows = _EquityInfo.Tables[0].Select(C_ColName_Code + "='" + e.Code + "'");
                if (rows.Length >= 1)
                {
                    DataRow row = rows[0];
                    e.Name = row[C_ColName_Name].ToString();
                    e.ListedDate = DataManager.ConvertToDate(row[C_ColName_ListedDate]);
                    e.DelistedDate = DataManager.ConvertToDate(row[C_ColName_DelistedDate]);
                    e.Exchange = this.GetExchange(row[C_ColName_Exchange].ToString());
                    
                    e.Industry1 = row[C_ColName_Industry1].ToString();
                    e.Industry2 = row[C_ColName_Industry2].ToString();
                    e.Industry3 = row[C_ColName_Industry3].ToString();
                    e.IndustryIndex = row[C_ColName_IndustryIndex].ToString();

                    e.DataSource = this.DataSource;
                }
                else
                {
                    MessageManager.GetInstance().AddMessage(MessageType.Warning, Message.C_Msg_EQ5, e.Code);
                }
            }
            catch (Exception ex)
            {
                throw ex;
            }
        }
        #endregion

        #region 股票价格
        public override void LoadEquityPrice(ASecurityGroup g, bool IsLoadAll)
        {
            //===============================
            //批量读取多个股票的数据，速度更快
            //  IsPointOnly: false=下载时间序列数据，true=下载时间点数据计算收益率
            //===============================
            try
            {
                //如果没有持仓则退出
                if (g.SecurityList == null || g.SecurityCodes.Count == 0)
                    return;

                //读数据: 按时间降序排列
                string sql = @"Select A.Tdate AS " + C_ColName_TradeDate + @"
                                , A.Symbol AS " + C_ColName_Code + @"
                                , A.Exchange AS " + C_ColName_Exchange + @"
                                , A.LCLOSE AS " + C_ColName_PreClose + @"
                                , A.TOPEN AS " + C_ColName_Open + @"
                                , A.HIGH AS " + C_ColName_High + @"
                                , A.Low AS " + C_ColName_Low + @"
                                , A.TCLOSE AS " + C_ColName_Close + @"
                                , A.VOTURNOVER AS " + C_ColName_Volume + @"
                                , A.VATURNOVER AS " + C_ColName_Amount + @"
                                , A.AVGPRICE AS " + C_ColName_Average + @"
                                , B.Price2 AS " + C_ColName_AdjustedClose + @"
                              From SecurityCode S
                              INNER JOIN CHDQUOTE A
                                    ON S.Symbol = A.SYMBOL
                              Left join DERC_EQACHQUOTE_2 B
                                   On A.tdate = to_char(B.tdate,'yyyyMMdd') and A.Symbol=B.symbol and A.Exchange = B.Exchange
                              Where exchange in ('CNSESH','CNSESZ')
                              And Stype = 'EQA' ";
                sql += " AND A.SYMBOL IN (" + base.getCodeListString(g.SecurityCodes) + ") ";
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
        private void updateTradePrice(DataSet ds, ASecurityGroup sg, bool IsLoadAll, double adjustcoefficient = 0, SecExchange defaultExchange = SecExchange.OTC)
        {
            foreach (ASecurity s in sg.SecurityList)
            {
                //初始化
                HistoryTradePrice pxs = (HistoryTradePrice)s.HistoryTradePrice;                
                if (pxs.OriginalTimeSeries == null)
                    pxs.OriginalTimeSeries = new List<AHistoryTimeItem>();
                else
                    pxs.OriginalTimeSeries.Clear();

                //加载数据: ds中时间序列已经倒序排列
                try
                {
                    DataRow[] rows = ds.Tables[0].Select(C_ColName_Code + "='" + pxs.Code + "'");
                    if (rows.Length > 0)
                    {
                        //基本信息
                        pxs.DataSource = this.DataSource;
                        pxs.OriginalTimeSeries.Clear();

                        double price1 = 0, price0 = 0;
                        foreach (DataRow row in rows)
                        {
                            //剔除交易所不一致的情况：重复代码的债券
                            SecExchange exchange = GetExchange(row[C_ColName_Exchange].ToString());
                            if (s.Exchange != exchange)
                                continue;   //同一代码不同交易所的情况

                            HistoryItemTradingPrice px = new HistoryItemTradingPrice();
                            px.TradeDate = DataManager.ConvertToDate(row[C_ColName_TradeDate]);
                            px.PreClose = DataManager.ConvertToDouble(row[C_ColName_PreClose]);
                            px.Close = DataManager.ConvertToDouble(row[C_ColName_Close]);
                            px.High = DataManager.ConvertToDouble(row[C_ColName_High]);
                            px.Low = DataManager.ConvertToDouble(row[C_ColName_Low]);
                            px.Open = DataManager.ConvertToDouble(row[C_ColName_Open]);
                            px.Volume = DataManager.ConvertToDouble(row[C_ColName_Volume]);
                            px.Amount = DataManager.ConvertToDouble(row[C_ColName_Amount]);
                            px.Average = (px.Volume == 0) ? 0 : px.Amount / px.Volume;

                            //判断停牌
                            if (px.Volume == 0 || px.Amount == 0)
                            {
                                px.IsTrading = false;
                                px.Close = px.PreClose;
                                px.Open = px.PreClose;
                                px.High = px.PreClose;
                                px.Low = px.PreClose;
                            }

                            //复权系数
                            if (adjustcoefficient > 0)
                                px.AdjustCoefficient = adjustcoefficient;
                            else
                            {
                                if (ds.Tables[0].Columns.Contains(C_ColName_AdjustedClose))
                                {
                                    double adjustedClose = DataManager.ConvertToDouble(row[C_ColName_AdjustedClose]);
                                    px.AdjustCoefficient = (px.Close == 0 || adjustedClose == 0) ? 1 : adjustedClose / px.Close;
                                }
                                else
                                    px.AdjustCoefficient = 1;
                            }
                            pxs.OriginalTimeSeries.Add(px);

                            //记录点价格
                            if (px.TradeDate >= s.TimeSeriesStart)
                                price0 = px.Close * px.AdjustCoefficient;
                            if (px.TradeDate >= s.TimeSeriesEnd)
                                price1 = px.Close * px.AdjustCoefficient;
                        }

                        //复权
                        if (IsLoadAll)
                            pxs.Adjust();

                        //计算涨跌幅
                        if (price0 * price1 != 0)
                        {
                            //注意：这里不可以对Position对象进行赋值，否则可能引起错误
                            //s.Position.CurrentYield = price1 / price0 - 1;
                            //s.Position.AccumulatedYieldIndex = price1 / price0;
                            pxs.HoldingPeriodReturn = price1 / price0 - 1;
                        }
                        else
                        {
                            //MessageManager.GetInstance().AddMessage(MessageType.Warning, Message.C_Msg_GE5, pxs.Code);
                        }
                    }
                    else
                    {
                        if (defaultExchange == SecExchange.OTC || s.Exchange == defaultExchange)
                            MessageManager.GetInstance().AddMessage(MessageType.Warning, Message.C_Msg_GE5, pxs.Code);
                    }
                }
                catch (Exception ex)
                {
                    throw ex;
                }
            }
        }
        private string getDateRestrictSQL(string colname, ASecurityGroup g, bool IsLoadAll)
        {
            string sql = "";
            if (IsLoadAll)
                //读取区间内所有时间序列
                sql += " AND " + colname + " >= '" + g.TimeSeriesStartExtended.ToString("yyyyMMdd")
                    + "' AND " + colname + " <= '" + g.TimeSeriesEnd.ToString("yyyyMMdd") + "'";
            else
            {
                //仅读取时间起始位置若干数据
                int daysinterval = 5;
                DateTime start, end;
                start = g.TimeSeriesStart.AddDays(-daysinterval); end = g.TimeSeriesStart.AddDays(daysinterval);
                sql += " AND ( " + colname + " >= '" + start.ToString("yyyyMMdd") + "' AND " + colname + " <= '" + end.ToString("yyyyMMdd") + "'";
                start = g.TimeSeriesEnd.AddDays(-daysinterval); end = g.TimeSeriesEnd.AddDays(daysinterval);
                sql += "    OR " + colname + " >= '" + start.ToString("yyyyMMdd") + "' AND " + colname + " <= '" + end.ToString("yyyyMMdd") + "')";

                //sql += " AND " + colname + " IN ('" + g.TimeSeriesStartExtended.ToString("yyyyMMdd") + "','" + g.TimeSeriesEnd.ToString("yyyyMMdd") + "')";
            }
            return sql;
        }
        #endregion

        #region 股票分红
        public override void LoadEquityDividend(ASecurityGroup g)
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
                                ,DisHty3 AS " + C_ColName_DividendBeforeTax + @"
                                ,DisHty4 AS " + C_ColName_DividendAfterTax + @"
                                ,DisHty12 AS " + C_ColName_RegisterDate + @"
                                ,DisHty13 AS " + C_ColName_ExDividendDate + @"
                                FROM DisHty
                               WHERE DisHty2 IN ('111','1F1','211','2F1','311','3F1','411','4F1','511','5F1')";
                sql += " AND SYMBOL IN (" + base.getCodeListString(g.SecurityCodes) + ") ";
                sql += " AND DisHty13 >= " + this.DBInstance.ConvertToSQLDate(g.TimeSeriesStart) + " AND DisHty13 <= " + this.DBInstance.ConvertToSQLDate(g.TimeSeriesEnd);
                sql += " ORDER BY Symbol, DisHty13 Desc";
                DataSet ds = base.DBInstance.ExecuteSQL(sql);

                //更新数据
                foreach (Equity e in g.SecurityList)
                {
                    this.updateDividend(ds, e.HistoryDividends);
                }
            }
            catch (Exception ex)
            {
                throw ex;
            }
        }
        private void updateDividend(DataSet ds, HistoryDividend dividends)
        {
            DataRow[] rows = ds.Tables[0].Select(C_ColName_Code + "='" + dividends.Code + "'");
            if (rows.Length > 0)
            {
                //基本信息
                dividends.DataSource = this.DataSource;
                dividends.OriginalTimeSeries.Clear();

                foreach (DataRow row in rows)
                {
                    HistoryItemDividend d = new HistoryItemDividend();
                    d.RegisterDate = DataManager.ConvertToDate(row[C_ColName_RegisterDate]);
                    d.ExcludeDate = DataManager.ConvertToDate(row[C_ColName_ExDividendDate]);
                    d.DividendBeforeTax = DataManager.ConvertToDouble(row[C_ColName_DividendBeforeTax]);
                    d.TradeDate = d.ExcludeDate;
                    dividends.OriginalTimeSeries.Add(d);
                }
            }
        }
        #endregion
    }
}
