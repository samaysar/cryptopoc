using System;
using System.Collections.Generic;
using CrptoTrade.Trading;

namespace CrptoTrade.Assets
{
    public class Quote : IComparable<Quote>
    {
        private readonly ITrader _quoteTrader;

        //we go low on GC
        public static readonly IEqualityComparer<Quote> Equality = new QuoteEquality();
        private decimal _totalSize;

        //can we set it somewhr as const value?
        public readonly decimal MinSize;
        public readonly decimal MinDollar;
        public readonly decimal Stepsize;
        public readonly decimal QuotePrice;
        public readonly string Id;

        public decimal QuoteSize => _totalSize;

        public bool Invalid => _totalSize <= 0;

        public Quote(decimal quotePrice, decimal totalSize, string id, decimal stepSize,
            ITrader quoteTrader, decimal minSize = 0.01m)
        {
            Id = id;
            QuotePrice = quotePrice;
            _totalSize = totalSize;
            Stepsize = stepSize;
            MinDollar = minSize * quotePrice;
            MinSize = minSize;
            _quoteTrader = quoteTrader;
        }

        public void AdjustSize(decimal tominus)
        {
            _totalSize -= tominus;
        }

        public void NewSize(decimal newSize)
        {
            _totalSize = newSize;
        }

        public void AdjustBuyPosition(BuyPosition position)
        {
            if (position.AdjustWithQuote(this, _quoteTrader) >= QuoteSize)
            {
                //we reduce the unit ONLY if the buy position can
                //completely settle it
                //becoz the my trade is either going to consume it on the
                //XCHG thus, it would disappear else someone already
                //consumed it, thus DONE notif is anyway on its way in both cases

                //if my trade is partially consuming it and I modify the quantity
                //then I have to take extra precautions on notification when adjusting its size
                //let the notif do its work.
                _totalSize = 0;
            }
        }

        public void AdjustSellPosition(SellPosition position)
        {
            if (position.AdjustWithQuote(this, _quoteTrader) >= QuoteSize)
            {
                //we reduce the unit ONLY if the buy position can
                //completely settle it
                //becoz the my trade is either going to consume it on the
                //XCHG thus, it would disappear else someone already
                //consumed it, thus DONE notif is anyway on its way in both cases

                //if my trade is partially consuming it and I modify the quantity
                //then I have to take extra precautions on notification when adjusting its size
                //let the notif do its work.
                _totalSize = 0;
            }
        }

        public int CompareTo(Quote other)
        {
            //we DONT check for ref equality, its OK as price is READONLY
            return ReferenceEquals(null, other)
                ? 1
                : QuotePrice.CompareTo(other.QuotePrice);
        }

        private class QuoteEquality : IEqualityComparer<Quote>
        {
            public bool Equals(Quote x, Quote y)
            {
                return x.Id.Equals(y.Id, StringComparison.Ordinal);
            }

            public int GetHashCode(Quote obj)
            {
                return obj.Id.GetHashCode();
            }
        }
    }
}