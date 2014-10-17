using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Data;

namespace Security
{
    public partial class HistoryDataVendorCH
    {
        #region 期货信息
        public override void LoadFutureInfo(Future f)
        {
            try
            {
                if (_FutureContactInfo == null)
                {
                    string sqlContract = @" Select Symbol AS " + C_ColName_Code + @"
                                    ,Exchange AS " + C_ColName_Exchange + @"
                                    ,SName AS " + C_ColName_Name + @"
                                    ,BeginDate AS " + C_ColName_ListedDate + @"
                                    ,EndDate AS " + C_ColName_DelistedDate + @"
                                    ,DeliveryDay AS " + C_ColName_DeliveryDate + @"
                                    From FuturesCTInfo 
                                    Where Exchange in ('CNFEDC','CNFESF','CNFEZC','CNFF00')
                                    ";

                    string sqlFuture = @"SELECT Exchange AS " + C_ColName_Exchange + @"
                                    ,ISymbol AS " + C_ColName_Code + @"
                                    ,FuturesInfo1 AS " + C_ColName_CommodityName + @"
                                    ,FuturesInfo2 AS " + C_ColName_CommodityUnit + @"
                                    ,FuturesInfo3 AS " + C_ColName_QuoteUnit + @"
                                    ,FUTURESINFO17 AS " + C_ColName_UnitsPerContract + @"
                                    ,FuturesInfo4 AS " + C_ColName_TickPrice + @"
                                    ,FuturesInfo5 AS " + C_ColName_PriceLimitPct + @"
                                    ,FuturesInfo6 AS " + C_ColName_DeliveryMonth + @"
                                    ,FuturesInfo8 AS " + C_ColName_LastTradingDate + @"
                                    ,FuturesInfo9 AS " + C_ColName_LastDeliveryDate + @"
                                    ,FuturesInfo12 AS " + C_ColName_MinimumTradingMargin + @"
                                    ,FuturesInfo23 AS " + C_ColName_ClearingPrice + @"
                                    ,FuturesInfo24 AS " + C_ColName_DeliveryPrice + @"
                                    FROM FuturesInfo
                                    WHERE Exchange IN ('CNFEDC','CNFESF','CNFEZC','CNFF00')
                                    ";

                    //获得所有期货的信息做缓存
                    _FutureContactInfo = base.DBInstance.ExecuteSQL(sqlContract);
                    _FutureInfo = base.DBInstance.ExecuteSQL(sqlFuture);
                }

                //更新数据
                DataRow[] rContract = _FutureContactInfo.Tables[0].Select(C_ColName_Code + "='" + f.Code + "'");
                string commoditycode = (f.Code.Length <= 4) ? f.Code : f.Code.Substring(0, f.Code.Length - 4);
                DataRow[] rFuture = _FutureInfo.Tables[0].Select(C_ColName_Code + "='" + commoditycode + "'");

                if (rContract.Length >= 1)
                {
                    DataRow row = rContract[0];
                    f.Name = row[C_ColName_Name].ToString();
                    f.ListedDate = DataManager.ConvertToDate(row[C_ColName_ListedDate]);
                    f.DelistedDate = DataManager.ConvertToDate(row[C_ColName_DelistedDate]);
                    f.DeliveryDate = DataManager.ConvertToDate(row[C_ColName_DeliveryDate]);
                    f.Exchange = this.GetExchange(row[C_ColName_Exchange].ToString());

                    if (rFuture.Length >=1)
                    {
                        row = rFuture[0];
                        f.CommodityCode = row[C_ColName_Code].ToString();
                        f.CommodityName = row[C_ColName_CommodityName].ToString();
                        f.CommodityUnit = row[C_ColName_CommodityUnit].ToString();
                        f.QuoteUnit = row[C_ColName_QuoteUnit].ToString();
                        f.TickPrice = row[C_ColName_TickPrice].ToString();
                        f.PriceLimitPct = row[C_ColName_PriceLimitPct].ToString();
                        f.DeliveryMonth = row[C_ColName_DeliveryMonth].ToString();
                        f.LastTradingDate = row[C_ColName_LastTradingDate].ToString(); 
                        f.LastDeliveryDate = row[C_ColName_LastDeliveryDate].ToString();
                        f.MinimumTradingMargin = row[C_ColName_MinimumTradingMargin].ToString();
                        f.ClearingPrice = row[C_ColName_ClearingPrice].ToString();
                        f.DeliveryPrice = row[C_ColName_DeliveryPrice].ToString();
                        f.UnitsPerContract = row[C_ColName_UnitsPerContract].ToString();
                    }

                    f.DataSource = this.DataSource;
                }
                else
                {
                    MessageManager.GetInstance().AddMessage(MessageType.Warning, Message.C_Msg_EQ5, f.Code);
                }
            }
            catch (Exception ex)
            {
                throw ex;
            }
        }
        #endregion
    }
}
