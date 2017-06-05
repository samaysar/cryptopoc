using System;
using System.Threading.Tasks;
using CrptoTrade.Trading;

namespace CrptoTrade.Assets
{
    public class BuyPosition
    {
        private readonly object _syncRoot = new object();
        private IBuyer _lastBuyer;
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

        public decimal AdjustWithQuote(Quote quote, IBuyer quoteBuyer)
        {
            LastSize = 0;
            lock (_syncRoot)
            {
                if (_value >= quote.MinDollar)
                {
                    LastSize = quote.MinSize;
                    var quoteSatoshi = (int)((quote.QuoteSize - quote.MinSize) / quote.Stepsize);
                    if (quoteSatoshi > 0)
                    {
                        var extraSatoshi = (int) ((_value - quote.MinDollar) / (quote.QuotePrice * quote.Stepsize));
                        var satoshiSize = Math.Min(extraSatoshi, quoteSatoshi) * quote.Stepsize;
                        LastSize += satoshiSize;
                    }
                    _value -= (LastSize * quote.QuotePrice);
                }
            }            
            LastPrice = quote.QuotePrice;
            _lastBuyer = quoteBuyer;
            return LastSize;
        }

        public Task ExecuteLastAsync()
        {
            return _lastBuyer.BuyAsync(LastSize, LastPrice);
        }
    }

    public class SellPosition
    {
        private readonly object _syncRoot = new object();
        private decimal _totalSize;
        private ISeller _lastSeller;
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

        public decimal AdjustWithQuote(Quote quote, ISeller quoteSeller)
        {
            LastSize = 0;
            lock (_syncRoot)
            {
                if (_totalSize >= quote.MinSize)
                {
                    LastSize = quote.MinSize;
                    var quoteSatoshi = (int)((quote.QuoteSize - quote.MinSize) / quote.Stepsize);
                    if (quoteSatoshi > 0)
                    {
                        var extraSatoshi = (int) ((_totalSize - quote.MinSize) / quote.Stepsize);
                        var satoshiSize = Math.Min(extraSatoshi, quoteSatoshi) * quote.Stepsize;
                        LastSize += satoshiSize;
                    }
                    _totalSize -= LastSize;
                }
            }
            LastPrice = quote.QuotePrice;
            _lastSeller = quoteSeller;
            return LastSize;
        }

        public Task ExecuteLastAsync()
        {
            return _lastSeller.SellAsync(LastSize, LastPrice);
        }
    }
}