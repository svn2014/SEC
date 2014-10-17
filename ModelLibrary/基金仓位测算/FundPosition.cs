using System;
using System.Collections;
using System.Collections.Generic;
using System.Data;
using System.Diagnostics;
using MathWorks.MATLAB.NET.Arrays;
using MatlabModelFundPosition;
using Security;

namespace ModelLib
{
    /// <summary>
    /// 基金仓位测算模型
    /// </summary>
    public class ModelFundPosition
    {
        #region 枚举
        enum IndexType
        { 
            SWIndustry, //申万一级行业指数
            ZXIndustry, //中信一级行业指数
            JCIndustry, //巨潮1000行业指数

            ZZSize,     //中证规模指数
            JCSize,     //巨潮规模指数
            SWProfit,   //申万盈利指数
            JCStyle,    //巨潮风格指数
        }

        public const string C_PARA_StartDate = "START";
        public const string C_PARA_EndDate = "END";
        public const string C_PARA_Model = "MODEL";

        private const string C_MSG_WrongPara = "错误的模型参数";
        private const string C_MSG_OutputError = "输出错误";
        #endregion

        #region 接口
        public void Run()
        {
            foreach (int m in this.Model)
            {
                switch (m)
                {
                    case 1: ;
                        this.RumModel1();
                        break;
                    case 2:
                        this.RumModel2();
                        break;
                    default:
                        break;
                }
            }
        }
        public void SetParameters(Hashtable ht)
        {
            try
            {
                if (ht.Contains(C_PARA_StartDate))
                    this.StartDate = Convert.ToDateTime(ht[C_PARA_StartDate]);
                if (ht.Contains(C_PARA_EndDate))
                    this.EndDate = Convert.ToDateTime(ht[C_PARA_EndDate]);
                if (ht.Contains(C_PARA_Model))
                    this.Model = new int[] { Convert.ToInt16(ht[C_PARA_Model])};
            }
            catch (Exception ex)
            {
                throw new Exception(C_MSG_WrongPara, ex);
            }             
        }
        public DataSet GetResult()
        {
            try
            {
                if (this.OutputSeriesList == null || this.OutputSeriesList.Count == 0)
                    return null;

                //加入指数
                Index szzs = new Index("000001");
                szzs.SetDatePeriod(this.StartDate, this.EndDate);
                szzs.LoadData(DataInfoType.HistoryTradePrice);

                #region 建立表
                DataTable dtEquity = new DataTable();
                dtEquity.TableName = "EquityFund";
                dtEquity.Columns.Add(new DataColumn("Date",Type.GetType("System.DateTime")));
                dtEquity.Columns.Add(new DataColumn("Index", Type.GetType("System.Double")));
                dtEquity.Columns.Add(new DataColumn("Value1", Type.GetType("System.Double")));
                //dtEquity.Columns.Add(new DataColumn("RSquare1", Type.GetType("System.Double")));
                dtEquity.Columns.Add(new DataColumn("Value2", Type.GetType("System.Double")));
                //dtEquity.Columns.Add(new DataColumn("RSquare2", Type.GetType("System.Double")));
                dtEquity.Columns.Add(new DataColumn("Avg", Type.GetType("System.Double")));
                dtEquity.Columns.Add(new DataColumn("MovingAvg", Type.GetType("System.Double")));
                dtEquity.Columns.Add(new DataColumn("ReportValue", Type.GetType("System.Double")));
                //dtEquity.Columns.Add(new DataColumn("TotalNetAsset", Type.GetType("System.Double")));
                //dtEquity.Columns.Add(new DataColumn("TotalCashAsset", Type.GetType("System.Double")));

                DataTable dtHybrid = dtEquity.Copy();
                dtHybrid.TableName = "HybridFund";
                #endregion

                int size = this.OutputSeriesList[0].OutputList.Count;

                #region 原始值
                for (int i = 0; i < size; i++)
                {
                    DataRow rEquity = dtEquity.NewRow();
                    DataRow rHybrid = dtHybrid.NewRow();
                    foreach (OutputSeries os in this.OutputSeriesList)
	                {
                        DataRow r = null;
                        if (os.Category == FundAssetCategory.Equity)
                            r = rEquity;
                        else if (os.Category == FundAssetCategory.Hybrid)
                            r = rHybrid;
                        else
                            continue;

                        r["Date"] = os.OutputList[i].TradeDate;
                        if (os.Model == 1)
                        {
                            r["Value1"] = os.OutputList[i].Position;
                            //r["RSquare1"] = os.OutputList[i].RSquare;
                        }
                        else if (os.Model == 2)
                        {
                            r["Value2"] = os.OutputList[i].Position;
                            //r["RSquare2"] = os.OutputList[i].RSquare;
                        }

                        //Debug.Write(os.Category); 
                        //Debug.Write("\t"); 
                        //Debug.Write(os.OutputList[i].TradeDate.ToString("yyyy-MM-dd"));
                        //Debug.Write("\t"); 
                        //for (int x = 0; x < os.OutputList[i].SubPostion.Length; x++)
                        //{
                        //    Debug.Write(os.OutputList[i].SubPostion[x, 0]);
                        //    Debug.Write("\t");
                        //}
                        //Debug.WriteLine("");
	                }
                    dtEquity.Rows.Add(rEquity);
                    dtHybrid.Rows.Add(rHybrid);
                }
                #endregion

                #region 衍生值
                int MA = 5;
                for (int i = 0; i < dtEquity.Rows.Count; i++)
                {
                    MutualFund mf = null;
                    DataTable dt = null;
                    DataRow r = null;
                    double maxEquityPosition = 0.95;
                    for (int k = 0; k < 2; k++)
                    {
                        if (k == 0)
                        {
                            r = dtEquity.Rows[i];
                            dt = dtEquity;
                            mf = this.WholeFundEquity;
                            maxEquityPosition = 0.95;
                        }
                        else
                        {
                            r = dtHybrid.Rows[i];
                            dt = dtHybrid;
                            mf = this.WholeFundHybrid;
                            maxEquityPosition = 0.75;
                        }

                        if (r["Value1"] != DBNull.Value && r["Value2"] != DBNull.Value)
                        {
                            //大盘指数
                            DateTime tradedate = Convert.ToDateTime(r["Date"]);
                            AHistoryTimeItem idxPx = szzs.HistoryTradePrice.AdjustedTimeSeries.Find(delegate(AHistoryTimeItem s) { return s.TradeDate == tradedate; });
                            if (idxPx != null)
                                //r["Index"] = ((ExchangeTradingPrice)idxPx).Close;
                                r["Index"] = ((HistoryItemTradingPrice)idxPx).Close;

                            //季报仓位
                            //  季报通常比日历日期晚20天
                            //  AdjustedTimeSeries是倒序排列的
                            int idx = mf.HistoryAssetReport.AdjustedTimeSeries.FindLastIndex(delegate(AHistoryTimeItem s) { return s.TradeDate >= tradedate.AddDays(20); });
                            AHistoryTimeItem mfrpt = null;
                            if (idx >= 0)
                                mfrpt = mf.HistoryAssetReport.AdjustedTimeSeries[idx];
                            else
                                mfrpt = mf.HistoryAssetReport.AdjustedTimeSeries[0];

                            r["Avg"] = (Convert.ToDouble(r["Value1"]) + Convert.ToDouble(r["Value2"])) / 2;

                            if (mfrpt != null)
                            {
                                //MutualFundReport rpt = (MutualFundReport)mfrpt;
                                HistoryItemMutualFundReport rpt = (HistoryItemMutualFundReport)mfrpt;
                                r["ReportValue"] = (rpt.TotalNetAsset == 0) ? 0 : (rpt.TotalEquityAsset / rpt.TotalNetAsset);
                                //r["TotalNetAsset"] = rpt.TotalNetAsset;
                                //r["TotalCashAsset"] = rpt.TotalNetAsset * (maxEquityPosition - Convert.ToDouble(r["Avg"]));
                            }                            

                            double mavalue = 0;
                            for (int j = 0; j < MA; j++)
                            {
                                if (i - j >= 0)
                                    mavalue += Convert.ToDouble(dt.Rows[i - j]["Avg"]);
                                else
                                    break;
                            }

                            r["MovingAvg"] = mavalue / Math.Min(MA, i + 1);
                        }
                    }
                }
                #endregion

                DataSet ds = new DataSet();
                ds.Tables.Add(dtEquity);
                ds.Tables.Add(dtHybrid);
                return ds;
            }
            catch (Exception ex)
            {
                throw new Exception(C_MSG_OutputError, ex);
            }
        }
        #endregion

        #region 参数
        private DateTime StartDate = DateTime.Today.AddDays(-30);
        private DateTime EndDate = DateTime.Today.AddDays(-1);
        private int[] Model = new int[] { 1, 2 };
        private List<DateTime> TradeDayList = new List<DateTime>();
        private List<OutputSeries> OutputSeriesList = new List<OutputSeries>();
        private MutualFund WholeFundEquity = null;
        private MutualFund WholeFundHybrid = null;
        #endregion

        #region 方法
        public ModelFundPosition()
        {
            //this.Name = "基金仓位测算模型";
            //this.Description = "使用申万行业指数和中证规模指数测算基金仓位。";
            //this.Version = "1.0.20130502";
        }

        private MWNumericArray GetFundTimeSeries(int yieldspan, FundAssetCategory category)
        {
            MutualFund WholeFund;
            switch (category)
            {
                case FundAssetCategory.Equity:
                    WholeFund = this.WholeFundEquity;    //股票型
                    break;
                case FundAssetCategory.Hybrid:
                    WholeFund = this.WholeFundHybrid;    //混合型
                    break;
                default:
                    WholeFund = this.WholeFundEquity;    //股票型
                    break;
            }

            if (WholeFund == null)
            {
                FundSelector mfs = new FundSelector();
                FundGroup mfg= null;
                switch (category)
                {
                    case FundAssetCategory.Equity:
                        mfg = mfs.GetActiveEquityFunds();    //股票型
                        break;
                    case FundAssetCategory.Hybrid:
                        mfg = mfs.GetActiveHybridFunds();    //混合型
                        break;
                    default:
                        mfg = mfs.GetActiveEquityFunds();    //股票型
                        break;
                }
                                
                mfg.SetDatePeriod(this.StartDate, this.EndDate);
                mfg.LoadData(DataInfoType.SecurityInfo);
                mfg.LoadData(DataInfoType.HistoryFundNAV);
                mfg.LoadData(DataInfoType.HistoryReport);
                WholeFund = mfg.GetWholeFund();
                WholeFund.Category.AssetCategory = category;

                switch (category)
                {
                    case FundAssetCategory.Equity:
                        this.WholeFundEquity = WholeFund;    //股票型
                        break;
                    case FundAssetCategory.Hybrid:
                        this.WholeFundHybrid = WholeFund;    //混合型
                        break;
                    default:
                        this.WholeFundEquity = WholeFund;    //股票型
                        break;
                }
            }

            int rows = WholeFund.HistoryTradeNAV.AdjustedTimeSeries.Count;
            double[,] aryf = new double[rows,1];
            for (int i = 0; i < rows; i++)
            {
                //从降序排列转换为升序排列
                Nullable<double> d = null;
                switch (yieldspan)
                {
                    case 1:
                        d = WholeFund.HistoryTradeNAV.AdjustedTimeSeries[i].UpAndDown.KLineDay1;
                        break;
                    case 2:
                        d = WholeFund.HistoryTradeNAV.AdjustedTimeSeries[i].UpAndDown.KLineDay2;
                        break;
                    case 3:
                        d = WholeFund.HistoryTradeNAV.AdjustedTimeSeries[i].UpAndDown.KLineDay3;
                        break;
                    case 4:
                        d = WholeFund.HistoryTradeNAV.AdjustedTimeSeries[i].UpAndDown.KLineDay4;
                        break;
                    case 5:
                        d = WholeFund.HistoryTradeNAV.AdjustedTimeSeries[i].UpAndDown.KLineDay5;
                        break;
                    default:
                        d = WholeFund.HistoryTradeNAV.AdjustedTimeSeries[i].UpAndDown.KLineDay1;
                        break;
                }
                        
                if (d == null)
                    aryf[rows - i - 1,0] = 0;
                else
                    aryf[rows - i - 1,0] = d.Value;

                TradeDayList.Add(WholeFund.HistoryTradeNAV.AdjustedTimeSeries[rows - i - 1].TradeDate);
            }

            MWNumericArray y = aryf;
            return y;
        }
        private MWNumericArray GetIndexTimeSeries(IndexType type, int yieldspan)
        {
            IndexSelector ids = new IndexSelector();
            IndexGroup idgSWI = null; ;

            switch (type)
            {
                case IndexType.SWIndustry:
                    idgSWI = ids.GetSWIndustryIndex();
                    break;
                case IndexType.ZXIndustry:
                    idgSWI = ids.GetZXIndustryIndex();
                    break;
                case IndexType.JCIndustry:
                    idgSWI = ids.GetJCIndustryIndex();
                    break;
                case IndexType.ZZSize:
                    idgSWI = ids.GetZZSizeIndex();
                    break;
                case IndexType.JCSize:
                    idgSWI = ids.GetJCSizeIndex();
                    break;
                case IndexType.SWProfit:
                    idgSWI = ids.GetSWProfitIndex();
                    break;
                case IndexType.JCStyle:
                    idgSWI = ids.GetJCStyleIndex();
                    break;
                default:
                    throw new Exception("未知的指数类型");
            }            
            
            idgSWI.SetDatePeriod(this.StartDate, this.EndDate);
            idgSWI.LoadData(DataInfoType.SecurityInfo);
            idgSWI.LoadData(DataInfoType.HistoryTradePrice);

            int rows = idgSWI.SecurityList[0].HistoryTradePrice.AdjustedTimeSeries.Count;
            int cols = idgSWI.SecurityList.Count;
            double[,] aryi = new double[rows, cols];
            for (int irow = 0; irow < rows; irow++)
            {
                for (int icol = 0; icol < cols; icol++)
                {
                    //从降序排列转换为升序排列
                    Nullable<double> d = null;
                    switch (yieldspan)
                    {
                        case 1:
                            d = idgSWI.SecurityList[icol].HistoryTradePrice.AdjustedTimeSeries[irow].UpAndDown.KLineDay1;
                            break;
                        case 2:
                            d = idgSWI.SecurityList[icol].HistoryTradePrice.AdjustedTimeSeries[irow].UpAndDown.KLineDay2;
                            break;
                        case 3:
                            d = idgSWI.SecurityList[icol].HistoryTradePrice.AdjustedTimeSeries[irow].UpAndDown.KLineDay3;
                            break;
                        case 4:
                            d = idgSWI.SecurityList[icol].HistoryTradePrice.AdjustedTimeSeries[irow].UpAndDown.KLineDay4;
                            break;
                        case 5:
                            d = idgSWI.SecurityList[icol].HistoryTradePrice.AdjustedTimeSeries[irow].UpAndDown.KLineDay5;
                            break;
                        default:
                            d = idgSWI.SecurityList[icol].HistoryTradePrice.AdjustedTimeSeries[irow].UpAndDown.KLineDay1;
                            break;
                    }

                    if (d == null)
                        aryi[rows - irow - 1, icol] = 0;
                    else
                        aryi[rows - irow - 1, icol] = d.Value;
                }
            }

            MWNumericArray x = aryi;
            return x;
        }

        private void RumModel1()
        {
            //输入参数
            List<InputItem> input = new List<InputItem>();
            
            InputItem item1 = new InputItem();
            ////申万一级行业
            //item1.Type = IndexType.SWIndustry;
            //item1.StepList = new int[] { 26 };
            ////中信一级行业
            //item1.Type = IndexType.ZXIndustry;
            //item1.StepList = new int[] { 36 };
            //巨潮一级行业
            item1.Type = IndexType.JCIndustry;
            item1.StepList = new int[] { 26 };

            item1.CategoryList = new FundAssetCategory[] { FundAssetCategory.Equity, FundAssetCategory.Hybrid };
            item1.YieldSpanList = new int[] { 1 };            
            input.Add(item1);
            this.RunModel(1, input);
        }
        private void RumModel2()
        {
            //输入参数
            List<InputItem> input = new List<InputItem>();
            //中证
            InputItem item1 = new InputItem();
            item1.Type = IndexType.ZZSize;
            item1.CategoryList = new FundAssetCategory[] { FundAssetCategory.Equity, FundAssetCategory.Hybrid };
            item1.YieldSpanList = new int[] { 1 };
            item1.StepList = new int[] { 21 };
            input.Add(item1);
            this.RunModel(2, input);
        }
        private void RunModel(int model, List<InputItem> input)
        {
            MatlabModel mm = new MatlabModel();
            MWNumericArray X0 = new double[] { };

            foreach (InputItem item in input)
            {
                foreach (FundAssetCategory category in item.CategoryList)
                {
                    foreach (int yieldspan in item.YieldSpanList)
                    {
                        foreach (int step in item.StepList)
                        {
                            MWNumericArray Y = this.GetFundTimeSeries(yieldspan, category);
                            MWNumericArray X = this.GetIndexTimeSeries(item.Type, yieldspan);

                            OutputSeries outputseries = new OutputSeries();
                            for (int start = step + 1; start < Y.NumberOfElements; start++)
                            {
                                try
                                {
                                    MWArray[] R = null;

                                    switch (model)
                                    {
                                        case 1:
                                            R = mm.model1(3, Y, X, step, start, X0);
                                            break;
                                        case 2:
                                            R = mm.model2(3, Y, X, step, start, X0);
                                            break;
                                        default:
                                            break;
                                    }

                                    DateTime tradedate = TradeDayList[start];
                                    if (tradedate <= this.EndDate && tradedate >= this.StartDate)
                                    {
                                        OutputItem output = new OutputItem();
                                        output.TradeDate = tradedate;
                                        output.Position = ((MWNumericArray)R[0]).ToScalarDouble();
                                        output.RSquare = ((MWNumericArray)R[2]).ToScalarDouble();
                                        output.SubPostion = (double[,])R[1].ToArray();

                                        outputseries.Category = category;
                                        outputseries.Model = model;
                                        outputseries.Type = item.Type;
                                        outputseries.Step = step;
                                        outputseries.YieldSpan = yieldspan;
                                        outputseries.OutputList.Add(output);
                                    }
                                }
                                catch (Exception ex)
                                {
                                    throw ex;
                                }
                            }
                            this.OutputSeriesList.Add(outputseries);
                        }
                    }
                }
            }

            mm.Dispose();
        }
        #endregion

        #region 结构
        class InputItem
        {
            public IndexType Type;
            public FundAssetCategory[] CategoryList = new FundAssetCategory[] { FundAssetCategory.Equity, FundAssetCategory.Hybrid };
            public int[] YieldSpanList;
            public int[] StepList;
        }
        class OutputItem
        {
            public DateTime TradeDate = new DateTime(1900, 1, 1);
            public double Position;
            public double RSquare;
            public double[,] SubPostion;
        }
        class OutputSeries
        {
            public FundAssetCategory Category = FundAssetCategory.Equity;
            public int Model;
            public IndexType Type;
            public int Step;
            public int YieldSpan;
            public List<OutputItem> OutputList = new List<OutputItem>();

            public void DebugPrint()
            {
                Debug.WriteLine("模型\t指数\t步长\t跨期\t日期\t仓位\tRsq\t");

                for (int i = 0; i < this.OutputList.Count; i++)
                {
                    Debug.Write(this.Model + "\t");
                    Debug.Write(this.Type.ToString() + "\t");
                    Debug.Write(this.Step + "\t");
                    Debug.Write(this.YieldSpan + "\t");
                    Debug.Write(this.OutputList[i].TradeDate.ToString("yyyy-MM-dd") + "\t");
                    Debug.Write(this.OutputList[i].Position + "\t");
                    Debug.Write(this.OutputList[i].RSquare + "\t");
                    Debug.WriteLine("");
                }
            }
        }
        #endregion
    }
}
