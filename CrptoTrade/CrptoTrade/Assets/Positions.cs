using System;

namespace CrptoTrade.Assets
{
    public class BuyPosition
    {
        private readonly object _syncRoot = new object();
        private decimal _value;
        public decimal LastSize;
        public decimal LastPrice;

        public BuyPosition(decimal value)
        {
            _value = value;
        }

        public void AddValue(decimal toadd)
        {
            lock (_syncRoot)
            {
                _value += toadd;
            }
        }

        public int AdjustWithQuote(Quote quote)
        {
            int tradableUnits;
            lock (_syncRoot)
            {
                tradableUnits = Math.Min((int) (_value / quote.Price), quote.Units);
                _value -= (tradableUnits * quote.Price);
            }            
            LastSize = tradableUnits * quote.Minsize;
            LastPrice = quote.Price;
            return tradableUnits;
        }
    }

    public class SellPosition
    {
        private readonly object _syncRoot = new object();
        private decimal _totalSize;
        public decimal LastSize;
        public decimal LastPrice;

        public SellPosition(decimal totalSize)
        {
            _totalSize = totalSize;
        }

        public void AddSize(decimal toadd)
        {
            lock (_syncRoot)
            {
                _totalSize += toadd;
            }
        }

        public int AdjustWithQuote(Quote quote)
        {
            int tradableUnits;
            lock (_syncRoot)
            {
                tradableUnits = Math.Min((int) (_totalSize / quote.Minsize), quote.Units);
                LastSize = tradableUnits * quote.Minsize;
                _totalSize -= LastSize;
            }
            LastPrice = quote.Price;
            return tradableUnits;
        }
    }
}