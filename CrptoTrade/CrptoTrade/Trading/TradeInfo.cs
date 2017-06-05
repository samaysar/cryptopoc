using System.Collections.Generic;

namespace CrptoTrade.Trading
{
    public class TradeInfo
    {
        public decimal Remains;
        public decimal Initial;
        public decimal TradeSize;
        public decimal Quote;
        public decimal DollarValue;
        public decimal MsTime;

        public string Summary(int position)
        {
            return
                $"Trade-{position} => Initial={Initial},Untraded:{Remains},Traded={TradeSize},Quote={Quote},$={decimal.Round(DollarValue, 2)},MsTime:{MsTime}";
        }
    }

    public class TradeResponse
    {
        public string Initial;
        public string Untraded;
        public string TotalTraded;
        public string TotalValue;
        public decimal MsTime;
        public List<string> TradeSummary;
        public string Error;
    }
}