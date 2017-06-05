using System.Collections.Generic;

namespace CrptoTrade.Trading
{
    public class TradeInfo
    {
        public decimal Remains;
        public decimal Initial;
        public decimal TradeSize;
        public decimal DollarValue;
        public int TimeInMicroSec;

        public string Summary(int position)
        {
            return
                $"Trade-{position} => Initial={Initial},Traded={TradeSize},Remain:{Remains},$={DollarValue},MilliSecTime:{TimeInMicroSec / 1000.0m}";
        }
    }

    public class TradeResponse
    {
        public string TotalTradeSize;
        public string TotalDollarValue;
        public string FinalRemains;
        public string Initial;
        public List<string> TradeSummary;
        public decimal TotalTimeInMiiliSec;
        public string Error;
    }
}