using System.Collections.Generic;
using System.Data;
using System;

namespace Security.Portfolio
{
    public enum TradeAction
    { 
        Undefined,
        Buy,
        Sell,
        Increase,
        Decrease,
        Long,
        Short
    }

    public class TransactionInfo
    {
        public DateTime TradingDay;
        public string Code;
        public string Name;
        public TradeAction Action = TradeAction.Undefined;
        public double Quantity = 0;
        public double Price = 0;

        public TransactionInfo(DateTime tradingday, string code, string name, TradeAction action, double quantity, double price)
        {
            this.TradingDay = tradingday;
            this.Code = code;
            this.Name = name;
            this.Action = action;
            this.Quantity = quantity;
            this.Price = price;
        }
    }

    public class PositionInfo:ICloneable
    {
        //估值表持仓信息
        public string ItemCode;
        public string Code;
        public string Name;
        public DateTime TradingDay;
        public double Quantity = 0;
        public double Cost = 0;
        public double MarketValue = 0;
        public double ValueAdded = 0;
        public double CostPct = 0;          //Cost / NAV
        public double MarketValuePct = 0;   //MV   / NAV
        public double CurrentPrice = 0;
        public SecurityType SecType = SecurityType.Other;
        public SecExchange Exchange = SecExchange.OTC;
        public double AccruedInterest = 0;

        //衍生数据
        public double CurrentDividend = 0;      //估值表不提供细分得股利收入需要衍生提供
        public double CurrentInterest = 0;      //当日利息
        public double RealizedReturn = 0;       //当日实现损益
        public double UnrealizedReturn = 0;     //当日浮动损益
        public double CurrentTotalReturn = 0;   //当日总收益=实现损益+浮动损益+利息+股利
        public double CurrentTotalCost = 0;
        public double AccumulatedReturn = 0;    //用于统计个股损益的数值

        //收益率
        public double CurrentYield = 0;         //当日收益率
        public double AccumulatedYieldIndex = 1;//累计收益率的指数

        //交易记录: 买入时间和卖出时间
        public DateTime DateIn =DataManager.C_Null_Date, DateOut = DataManager.C_Null_Date;
        public List<TransactionInfo> Transactions = new List<TransactionInfo>();

        public object Clone()
        {
            return this.MemberwiseClone();
        }

        public void Sum(PositionInfo p)
        {
            this.Quantity += p.Quantity;
            this.Cost += p.Cost;
            this.MarketValue += p.MarketValue;
            this.ValueAdded += p.ValueAdded;
            this.CostPct += p.CostPct;
            this.MarketValuePct += p.MarketValuePct;
            this.AccruedInterest += p.AccruedInterest;
            this.CurrentDividend += p.CurrentDividend;
            this.CurrentInterest += p.CurrentInterest;
            this.RealizedReturn += p.RealizedReturn;
            this.UnrealizedReturn += p.UnrealizedReturn;
            this.CurrentTotalCost += p.CurrentTotalCost;
            this.CurrentTotalReturn += p.CurrentTotalReturn;
            this.AccumulatedReturn += p.AccumulatedReturn;
        }

        public void Trade(TradeAction action, double quantity, double price)
        {
            TransactionInfo t = new TransactionInfo(this.TradingDay, this.Code, this.Name, action, quantity, price);
            this.Transactions.Add(t);

            if (action == TradeAction.Buy)
            {
                this.DateIn = this.TradingDay;    
            }
            else if (action == TradeAction.Sell)
            {
                this.DateOut = this.TradingDay;
            }
        }
    }

    public interface IPortfolioDataAdapter
    {
        PortfolioGroup BuildPortfolios(DataTable dtGZB, DateTime start, DateTime end);
    }

    public enum PortfolioDataAdapterType
    {
        YSS     //赢时胜
    }

    public class PortfolioDataAdaptorFactory
    {
        public static IPortfolioDataAdapter GetAdapter(PortfolioDataAdapterType type)
        {
            switch (type)
            {
                case PortfolioDataAdapterType.YSS:
                    return new PortfolioAdapterYSS();
                default:
                    return null;
            }
        }
    }
}
