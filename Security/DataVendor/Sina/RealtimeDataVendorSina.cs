using System;
using System.Collections;
using System.Net;

namespace Security
{
    public class RealtimeDataVendorSina: ARealtimeDataVendor
    {
        #region 扩展方法
        public RealtimeDataVendorSina()
        {
            base.DataSource = "新浪";

            _webclient = new WebClient();
            _webclient.Credentials = CredentialCache.DefaultCredentials;
        }

        private string getExchangeTradedCodes(ASecurityGroup g)
        {
            string codestring = "";

            foreach (ASecurity s in g.SecurityList)
            {
                string prefix = "";

                switch (s.Exchange)
                {
                    case SecExchange.SHE:
                        prefix = "sh";
                        break;
                    case SecExchange.SZE:
                        prefix = "sz";
                        break;
                    case SecExchange.FFE:
                        prefix = "CFF_";
                        break;
                    case SecExchange.SFE:
                    case SecExchange.ZFE:
                    case SecExchange.DFE:
                        prefix = "";
                        break;
                    default:
                        break;
                }

                if (!codestring.Contains(s.Code))
                    codestring += prefix + s.Code.ToUpper() + ",";
            }

            return codestring.Substring(0, codestring.Length - 1);
        }

        private const string C_BenchmarkIndex = "000906";
        private string getFundCodes(ASecurityGroup g)
        {
            string codestring = "";

            foreach (ASecurity s in g.SecurityList)
            {
                //加入基金代码
                string prefix = "f_";
                if (!codestring.Contains(s.Code))
                    codestring += prefix + s.Code + ",";

                //股票型基金加入比较基准代码
                if (((MutualFund)s).PrimaryBenchmarkIndex != null && ((MutualFund)s).Category.AssetCategory == FundAssetCategory.Equity)
                {
                    switch (((MutualFund)s).PrimaryBenchmarkIndex.Exchange)
                    {
                        case SecExchange.SHE:
                            prefix = "sh";
                            break;
                        case SecExchange.SZE:
                            prefix = "sz";
                            break;
                        default:
                            break;
                    }
                    if (!codestring.Contains(((MutualFund)s).PrimaryBenchmarkIndex.Code))
                        codestring += prefix + ((MutualFund)s).PrimaryBenchmarkIndex.Code + ",";
                }                
            }

            //主动股票基金的比较基准
            if (!codestring.Contains(C_BenchmarkIndex))
                codestring += "sh" + C_BenchmarkIndex + ",";

            return codestring.Substring(0, codestring.Length - 1);
        }
        #endregion

        private WebClient _webclient;
        public override void LoadTradePrice(ASecurityGroup g)
        {
            try
            {
                //如果没有持仓则退出
                if (g.SecurityList == null || g.SecurityCodes.Count == 0)
                    return;

                //读数据
                string url = @"http://hq.sinajs.cn/list=" + this.getExchangeTradedCodes(g);
                string hq = _webclient.DownloadString(url);

                //更新数据
                this.updateTradePrice(g, hq);
            }
            catch (Exception ex)
            {
                throw ex;
            }
        }

        private void updateTradePrice(ASecurityGroup g, string hq)
        {
            //未找到数据
            if (hq.Length < 50)
                return;

            //数据解析
            string[] aryhq = hq.Split(";".ToCharArray());//最后又一个空字串
            Hashtable ht = new Hashtable();
            for (int i = 0; i < aryhq.Length - 1; i++)
            {
                if (aryhq[i].ToString().Length < 50)
                    continue;

                //商品期货：var hq_str_OI1311=
                //股票：var hq_str_sh600000=
                //股指期货：var hq_str_CFF_TF1312=
                int idx1 = aryhq[i].LastIndexOf("_");
                int idx2 = aryhq[i].IndexOf("=");
                string code = aryhq[i].Substring(idx1 + 1, idx2 - idx1);

                if (code.Substring(0, 2).ToLower() == "sh" || code.Substring(0, 2).ToLower() == "sz")
                    code = code.Substring(2, 6);

                ht.Add(code, aryhq[i]);
            }

            //对象分配
            foreach (ASecurity s in g.SecurityList)
            {
                if (ht.Contains(s.Code))
                {
                    if (s.RealtimeTradePrice == null)
                        s.RealtimeTradePrice = new RealtimeTradePrice();

                    s.RealtimeTradePrice.Code = s.Code;
                    s.RealtimeTradePrice.DataSource = base.DataSource;

                    string data = ht[s.Code].ToString();
                    string[] arydata = data.Split(",\"".ToCharArray());

                    switch (s.Type)
                    {
                        case SecurityType.Equity:
                        case SecurityType.Index:
                        case SecurityType.Fund:
                            this.updateEquityTradePrice(s, arydata);
                            break;
                        case SecurityType.Future:
                            switch (s.Exchange)
                            {
                                case SecExchange.FFE:
                                    this.updateFinancialFutureTradePrice(s, arydata);
                                    break;
                                case SecExchange.DFE:
                                case SecExchange.ZFE:
                                case SecExchange.SFE:
                                    this.updateCommodityFutureTradePrice(s, arydata);
                                    break;
                                default:
                                    break;
                            }
                            break;
                        default:
                            break;
                    }                    
                }
            }
        }
        private void updateEquityTradePrice(ASecurity s, string[] arydata)
        {
            RealtimeTradePrice p = (RealtimeTradePrice)s.RealtimeTradePrice;
            p.Name = arydata[1].ToString().Replace(" ", "");

            //价格数据
            p.Open = DataManager.ConvertToDouble(arydata[2]);
            p.Preclose = DataManager.ConvertToDouble(arydata[3]);
            p.Close = DataManager.ConvertToDouble(arydata[4]);
            p.High = DataManager.ConvertToDouble(arydata[5]);
            p.Low = DataManager.ConvertToDouble(arydata[6]);

            //成交额
            p.Volume = DataManager.ConvertToDouble(arydata[9]);  //股
            p.Amount = DataManager.ConvertToDouble(arydata[10]);  //元

            //买：挂单量价，涨停=0
            p.BuyVolume1 = DataManager.ConvertToDouble(arydata[11]);
            p.BuyPrice1 = DataManager.ConvertToDouble(arydata[12]);
            p.BuyVolume2 = DataManager.ConvertToDouble(arydata[13]);
            p.BuyPrice2 = DataManager.ConvertToDouble(arydata[14]);
            p.BuyVolume3 = DataManager.ConvertToDouble(arydata[15]);
            p.BuyPrice3 = DataManager.ConvertToDouble(arydata[16]);
            p.BuyVolume4 = DataManager.ConvertToDouble(arydata[17]);
            p.BuyPrice4 = DataManager.ConvertToDouble(arydata[18]);
            p.BuyVolume5 = DataManager.ConvertToDouble(arydata[19]);
            p.BuyPrice5 = DataManager.ConvertToDouble(arydata[20]);

            //卖：挂单量价，涨停=0
            p.SellVolume1 = DataManager.ConvertToDouble(arydata[21]);
            p.SellPrice1 = DataManager.ConvertToDouble(arydata[22]);
            p.SellVolume2 = DataManager.ConvertToDouble(arydata[23]);
            p.SellPrice2 = DataManager.ConvertToDouble(arydata[24]);
            p.SellVolume3 = DataManager.ConvertToDouble(arydata[25]);
            p.SellPrice3 = DataManager.ConvertToDouble(arydata[26]);
            p.SellVolume4 = DataManager.ConvertToDouble(arydata[27]);
            p.SellPrice4 = DataManager.ConvertToDouble(arydata[28]);
            p.SellVolume5 = DataManager.ConvertToDouble(arydata[29]);
            p.SellPrice5 = DataManager.ConvertToDouble(arydata[30]);

            p.TimeStamp = Convert.ToDateTime(arydata[31].ToString() + " " + arydata[32].ToString());
            p.IsTrading = (arydata[33].ToString() == "00" ? true : false);

            //数据调整
            //1) 没有成交的证券价格同昨收盘
            if (p.Amount == 0)
            {
                double preclose = p.Preclose;
                p.Open = preclose;
                p.Close = preclose;
                p.High = preclose;
                p.Low = preclose;
            }

            //2) 上交所指数成交量单位手，调整为股
            if (s.Exchange == SecExchange.SHE && s.Type == SecurityType.Index)
            {
                p.Volume *= 100;
            }
        }
        private void updateCommodityFutureTradePrice(ASecurity s, string[] arydata)
        {
            //RealtimeTradePrice p = (RealtimeTradePrice)s.RealtimeTradePrice;
            //p.Name = arydata[1].ToString().Replace(" ", "");

            ////价格数据
            //p.Open = DataManager.ConvertToDouble(arydata[2]);
            //p.High = DataManager.ConvertToDouble(arydata[3]);
            //p.Low = DataManager.ConvertToDouble(arydata[4]);
            //p.Preclose = DataManager.ConvertToDouble(arydata[5]);
            //p.BuyPrice1 = DataManager.ConvertToDouble(arydata[6]);
            //p.SellPrice1 = DataManager.ConvertToDouble(arydata[7]);
            //p.Close = DataManager.ConvertToDouble(arydata[8]);
            //p.SettlePrice = DataManager.ConvertToDouble(arydata[9]);
            //p.PresettlePrice = DataManager.ConvertToDouble(arydata[10]);
            //p.BuyVolume1 = DataManager.ConvertToDouble(arydata[11]);
            //p.SellVolume1 = DataManager.ConvertToDouble(arydata[12]);
            //p.Holding = DataManager.ConvertToDouble(arydata[13]);
            //p.Volume = DataManager.ConvertToDouble(arydata[14]);  //份
            
            //p.TimeStamp = Convert.ToDateTime(arydata[17].ToString());
            //p.IsTrading = (p.TimeStamp == DateTime.Today ? true : false);

            ////数据调整
            ////1) 没有成交的证券价格同昨收盘
            //if (p.Amount == 0)
            //{
            //    double preclose = p.Preclose;
            //    p.Open = preclose;
            //    p.Close = preclose;
            //    p.High = preclose;
            //    p.Low = preclose;
            //}
        }
        private void updateFinancialFutureTradePrice(ASecurity s, string[] arydata)
        {
            //RealtimeTradePrice p = (RealtimeTradePrice)s.RealtimeTradePrice;
            //p.Name = arydata[1].ToString().Replace(" ", "");

            ////价格数据
            //p.Open = DataManager.ConvertToDouble(arydata[0]);
            //p.High = DataManager.ConvertToDouble(arydata[1]);
            //p.Low = DataManager.ConvertToDouble(arydata[2]);
            //p.Close = DataManager.ConvertToDouble(arydata[3]);
            //p.Volume = DataManager.ConvertToDouble(arydata[4]);  //份
            //p.Amount = DataManager.ConvertToDouble(arydata[5]);  //元
            //p.Holding = DataManager.ConvertToDouble(arydata[6]);
            ////new
            //p.SettlePrice = DataManager.ConvertToDouble(arydata[8]);
            //p.Preclose = DataManager.ConvertToDouble(arydata[13]);
            //p.PresettlePrice = DataManager.ConvertToDouble(arydata[14]);

            ////p.BuyPrice1 = DataManager.ConvertToDouble(arydata[6]);
            ////p.SellPrice1 = DataManager.ConvertToDouble(arydata[7]);
            ////p.BuyVolume1 = DataManager.ConvertToDouble(arydata[11]);
            ////p.SellVolume1 = DataManager.ConvertToDouble(arydata[12]);

            //p.TimeStamp = Convert.ToDateTime(arydata[36].ToString() + " " + arydata[37].ToString());
            //p.IsTrading = (arydata[38].ToString() == "0" ? true : false);

            ////数据调整
            ////1) 没有成交的证券价格同昨收盘
            //if (p.Amount == 0)
            //{
            //    double preclose = p.Preclose;
            //    p.Open = preclose;
            //    p.Close = preclose;
            //    p.High = preclose;
            //    p.Low = preclose;
            //}
        }
        public override void LoadNetAssetValue(ASecurityGroup g)
        {
            try
            {
                //如果没有持仓则退出
                if (g.SecurityList == null || g.SecurityCodes.Count == 0)
                    return;

                //读数据
                string url = @"http://hq.sinajs.cn/list=" + this.getFundCodes(g);
                string hq = _webclient.DownloadString(url);

                //更新数据
                this.updateFundNAV(g, hq);
            }
            catch (Exception ex)
            {
                throw ex;
            }
        }

        private void updateFundNAV(ASecurityGroup g, string hq)
        {
            //未找到数据
            if (hq.Length < 50)
                return;

            //数据解析
            string[] aryhq = hq.Split(";".ToCharArray());//最后又一个空字串
            Hashtable ht = new Hashtable();
            for (int i = 0; i < aryhq.Length - 1; i++)
            {
                if (aryhq[i].ToString().Length < 50)
                    continue;

                string code = aryhq[i].Substring(aryhq[i].IndexOf("=") - 6, 6);
                ht.Add(code, aryhq[i]);
            }

            //对象分配
            foreach (ASecurity s in g.SecurityList)
            {
                try
                {
                    MutualFund f = (MutualFund)s;
                    if (f.RealtimeNAV == null)
                        f.RealtimeNAV = new RealtimeNetAssetValue();

                    string benchmarkindex = C_BenchmarkIndex;
                    if (f.PrimaryBenchmarkIndex != null)
                        benchmarkindex = f.PrimaryBenchmarkIndex.Code;

                    //比较基准涨跌幅
                    double preclose = 0, close = 0;
                    if (ht.Contains(benchmarkindex) && f.Category.AssetCategory == FundAssetCategory.Equity)
                    {
                        string indexdata = ht[benchmarkindex].ToString();
                        string[] aryindexdata = indexdata.Split(",\"".ToCharArray());

                        preclose = DataManager.ConvertToDouble(aryindexdata[3]); //Preclose
                        close = DataManager.ConvertToDouble(aryindexdata[4]);    //Close
                        f.RealtimeNAV.TimeStamp =Convert.ToDateTime(aryindexdata[31].ToString() + " " + aryindexdata[32].ToString());
                        f.RealtimeNAV.IsUpdated = true;
                    }

                    //计算基金的涨跌幅
                    if(ht.Contains(f.Code))
                    {
                        string data = ht[f.Code].ToString();
                        string[] arydata = data.Split(",\"".ToCharArray());

                        f.RealtimeNAV.Code = f.Code;
                        f.RealtimeNAV.DataSource = base.DataSource;
                        f.RealtimeNAV.Name = arydata[1].ToString().Replace(" ", "");

                        if (arydata[2].Trim().Length > 0)
                            f.RealtimeNAV.PreCloseNAV = DataManager.ConvertToDouble(arydata[2]);
                        if (arydata[3].Trim().Length > 0)
                            f.RealtimeNAV.PreCloseAccumNAV = DataManager.ConvertToDouble(arydata[3]);

                        if (f.Category.AssetCategory == FundAssetCategory.Equity && DateTime.Now > base.TradeTimeStart)
                        {
                            //股票型基金 且 开盘前不用调整（净值已经反映了指数涨跌）
                            //  根据指数调整当前净值
                            f.RealtimeNAV.RealtimeUpDown = close / preclose - 1;
                            f.RealtimeNAV.RealtimeNAV = f.RealtimeNAV.PreCloseNAV * (1 + f.RealtimeNAV.RealtimeUpDown * f.PrimaryBenchmarkWeight);
                        }
                        else
                        {
                            f.RealtimeNAV.RealtimeUpDown = 0;
                            f.RealtimeNAV.RealtimeNAV = f.RealtimeNAV.PreCloseNAV;
                        }
                    }
                }
                catch (Exception ex)
                {
                    throw ex;
                }
            }
        }
    }
}
