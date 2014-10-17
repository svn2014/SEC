
namespace Security
{
    public class RealtimeTradePrice : ARealTimeSeries
    {        
        public bool IsTrading = true;

        public double Preclose = 0;
        public double Open = 0;
        public double Low = 0;
        public double High = 0;
        public double Close = 0;
        public double Volume = 0;
        public double Amount = 0;
        
        public double BuyPrice1 = 0;
        public double BuyPrice2 = 0;
        public double BuyPrice3 = 0;
        public double BuyPrice4 = 0;
        public double BuyPrice5 = 0;
        public double BuyVolume1 = 0;
        public double BuyVolume2 = 0;
        public double BuyVolume3 = 0;
        public double BuyVolume4 = 0;
        public double BuyVolume5 = 0;

        public double SellPrice1 = 0;
        public double SellPrice2 = 0;
        public double SellPrice3 = 0;
        public double SellPrice4 = 0;
        public double SellPrice5 = 0;
        public double SellVolume1 = 0;
        public double SellVolume2 = 0;
        public double SellVolume3 = 0;
        public double SellVolume4 = 0;
        public double SellVolume5 = 0;

        public double SettlePrice = 0;
        public double PresettlePrice = 0;
        public double Holding = 0;
    }
}
