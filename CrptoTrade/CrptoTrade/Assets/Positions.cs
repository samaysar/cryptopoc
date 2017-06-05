using System;
using System.Threading.Tasks;
using CrptoTrade.Trading;

namespace CrptoTrade.Assets
{
    public class BuyPosition
    {
        private decimal _value;
        public IBuyer LastBuyer;
        public decimal LastSize;
        public decimal LastPrice;

        public decimal Current => _value;

        public BuyPosition(decimal value)
        {
            _value = value;
        }

        public void AddValue(decimal toadd)
        {
            _value += toadd;
        }

        public decimal AdjustWithQuote(Quote quote, IBuyer quoteBuyer)
        {
            LastSize = 0;
            if (_value >= quote.MinDollar)
            {
                LastSize = quote.MinSize;
                var quoteSatoshi = decimal.Floor((quote.QuoteSize - quote.MinSize) / quote.Stepsize);
                if (quoteSatoshi > 0)
                {
                    var extraSatoshi = decimal.Floor((_value - quote.MinDollar) / (quote.QuotePrice * quote.Stepsize));
                    var satoshiSize = Math.Min(extraSatoshi, quoteSatoshi) * quote.Stepsize;
                    LastSize += satoshiSize;
                }
                _value -= (LastSize * quote.QuotePrice);
            }
            LastPrice = quote.QuotePrice;
            LastBuyer = quoteBuyer;
            return LastSize;
        }
    }

    public class SellPosition
    {
        private decimal _totalSize;
        public ISeller LastSeller;
        public decimal LastSize;
        public decimal LastPrice;

        public decimal Current => _totalSize;

        public SellPosition(decimal totalSize)
        {
            _totalSize = totalSize;
        }

        public void AddSize(decimal toadd)
        {
            _totalSize += toadd;
        }

        public decimal AdjustWithQuote(Quote quote, ISeller quoteSeller)
        {
            LastSize = 0;
            if (_totalSize >= quote.MinSize)
            {
                LastSize = quote.MinSize;
                var quoteSatoshi = decimal.Floor((quote.QuoteSize - quote.MinSize) / quote.Stepsize);
                if (quoteSatoshi > 0)
                {
                    var extraSatoshi = decimal.Floor((_totalSize - quote.MinSize) / quote.Stepsize);
                    var satoshiSize = Math.Min(extraSatoshi, quoteSatoshi) * quote.Stepsize;
                    LastSize += satoshiSize;
                }
                _totalSize -= LastSize;
            }
            LastPrice = quote.QuotePrice;
            LastSeller = quoteSeller;
            return LastSize;
        }
    }
}