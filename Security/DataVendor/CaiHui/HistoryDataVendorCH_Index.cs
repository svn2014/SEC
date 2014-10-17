using System;
using System.Data;

namespace Security
{
    public partial class HistoryDataVendorCH
    {
        #region 指数信息
        public override void LoadIndexInfo(Index i)
        {
            try
            {
                if (_IndexInfo == null)
                {
                    string sql = @"SELECT Symbol AS " + C_ColName_Code + @"
                                    , Iname AS " + C_ColName_Name + @"
                                    , Bdate AS " + C_ColName_IndexBaseDate + @"
                                    , Stopdate AS " + C_ColName_IndexStopDate + @"
                                    , Iprofile7 AS " + C_ColName_IndexCategory + @"
                                    , StockNum AS " + C_ColName_IndexComponentCount + @"
                                    , Exchange AS " + C_ColName_Exchange + @"
                                    FROM IPROFILE A
                                    WHERE Status = -1 AND currency='CNY'
                                    AND Exchange IN ('CNSE00','CNSESH','CNSESZ')";

                    //获得所有指数的信息做缓存
                    _IndexInfo = base.DBInstance.ExecuteSQL(sql);
                }

                //更新数据
                DataRow[] rows = _IndexInfo.Tables[0].Select(C_ColName_Code + "='" + i.Code + "'");
                if (rows.Length >= 1)
                {
                    DataRow row = rows[0];
                    i.Name = row[C_ColName_Name].ToString();
                    i.ListedDate = DataManager.ConvertToDate(row[C_ColName_IndexBaseDate]);
                    i.DelistedDate = DataManager.ConvertToDate(row[C_ColName_IndexStopDate]);
                    i.Category = row[C_ColName_IndexCategory].ToString();
                    i.Exchange = this.GetExchange(row[C_ColName_Exchange].ToString());
                    i.DataSource = this.DataSource;
                }
                else
                {
                    MessageManager.GetInstance().AddMessage(MessageType.Warning, Message.C_Msg_ID5, i.Code);
                }
            }
            catch (Exception ex)
            {
                throw ex;
            }
        }
        #endregion

        #region 指数价格
        public override void LoadIndexPrice(ASecurityGroup g, bool IsLoadAll)
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
                string sql = @"SELECT A.TDate AS " + C_ColName_TradeDate + @"
                            , A.Exchange AS " + C_ColName_Exchange + @"
                            , A.Symbol AS " + C_ColName_Code + @"
                            , A.SName AS " + C_ColName_Name + @"
                            , A.LClose AS " + C_ColName_PreClose + @"
                            , A.TOpen AS " + C_ColName_Open + @"
                            , A.TClose AS " + C_ColName_Close + @"
                            , A.HIGH AS " + C_ColName_High + @"
                            , A.LOW AS " + C_ColName_Low + @"
                            , A.VOTURNOVER AS " + C_ColName_Volume + @"
                            , A.VATURNOVER AS " + C_ColName_Amount + @"
                        FROM CIHDQUOTE A WHERE 1=1 ";
                sql += " AND SYMBOL IN (" + base.getCodeListString(g.SecurityCodes) + ") ";
                sql += this.getDateRestrictSQL("Tdate", g, IsLoadAll);
                sql += " ORDER BY Symbol, Tdate Desc";
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

        #region 指数成份
        public override void LoadIndexComponents(ASecurityGroup g)
        {
            //===============================
            //批量读取多个指数的数据，速度更快
            //  此处只读取月度的免费成份数据
            //===============================
            try
            {
                //如果没有持仓则退出
                if (g.SecurityList == null || g.SecurityCodes.Count == 0)
                    return;

                DateTime start = g.TimeSeriesEnd.AddDays(-31);
                if (start > g.TimeSeriesStart)
                    start = g.TimeSeriesStart;

                //读数据: 按时间降序排列
                string sql = @"Select TDate AS " + C_ColName_TradeDate + @"
                                ,ISymbol AS " + C_ColName_IndexCode + @"
                                ,IExchange AS " + C_ColName_IndexExchange + @"
                                ,IName AS " + C_ColName_IndexName + @"
                                ,Symbol AS " + C_ColName_Code + @"
                                ,SName AS " + C_ColName_Name + @"
                                ,Exchange AS " + C_ColName_Exchange + @"
                                ,Weighing AS " + C_ColName_IndexComponentWeight + @"
                                ,STYPE AS " + C_ColName_SecurityType + @"
                               From issweight_month 
                               Where 1=1 ";
                sql += " AND ISYMBOL IN (" + base.getCodeListString(g.SecurityCodes) + ") ";
                sql += " AND Tdate >= " + this.DBInstance.ConvertToSQLDate(start) + " AND Tdate <= " + this.DBInstance.ConvertToSQLDate(g.TimeSeriesEnd);
                sql += " ORDER BY ISymbol, Tdate desc, Weighing desc";
                DataSet ds = base.DBInstance.ExecuteSQL(sql);

                //更新数据
                foreach (Index idx in g.SecurityList)
                {
                    this.updateComponents(ds, idx);
                }
            }
            catch (Exception ex)
            {
                throw ex;
            }
        }
        private void updateComponents(DataSet ds, Index idx)
        {
            if (idx.Components != null)
                idx.Components.Clear();

            DataRow[] rows = ds.Tables[0].Select(C_ColName_IndexCode + "='" + idx.Code + "'");
            if (rows.Length > 0)
            {
                #region 类别
                string stype = rows[0][C_ColName_SecurityType].ToString().ToUpper();                
                SecurityType type = SecurityType.Other;
                if (stype.Substring(0, 1) == "E")
                {
                    type = SecurityType.Equity;
                }
                else if (stype.Substring(0, 1) == "B")
                {
                    type = SecurityType.Bond;
                }
                else if (stype.Substring(0, 1) == "F")
                {
                    type = SecurityType.Fund;
                }
                else
                {
                    throw new Exception(Message.C_Msg_ID1);
                }
                #endregion

                ASecurityGroup components = null;
                foreach (DataRow row in rows)
                {                    
                    string code = row[C_ColName_Code].ToString();
                    string name = row[C_ColName_Name].ToString();
                    SecExchange exchange = this.GetExchange(row[C_ColName_Exchange].ToString());
                    double weight = DataManager.ConvertToDouble( row[C_ColName_IndexComponentWeight]);
                    DateTime reportdate = DataManager.ConvertToDate(row[C_ColName_TradeDate]);

                    if (components == null || components.ReportDate != reportdate)
                        components = idx.Components.Find(delegate(ASecurityGroup g) { return g.ReportDate == reportdate; });

                    if (components == null)
                    {
                        components = ASecurityGroup.CreateGroup(type);
                        components.ReportDate = reportdate;
                        idx.Components.Add(components);
                    }

                    ASecurity s = ASecurity.CreateSecurity(type, code, exchange);
                    s.Name = name;
                    s.Position.MarketValuePct = weight / 100;
                    components.Add(s);
                }
            }
        }
        #endregion
    }
}
