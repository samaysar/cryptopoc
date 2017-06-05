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

        //can we set it somewhr as const value?
        public readonly decimal Minsize;
        public readonly decimal Price;
        public readonly string Id;

        public int Units { get; private set; }

        public bool Invalid => Units < 1;

        public Quote(decimal priceOfMinSize, decimal totalSize, string id, decimal minSize,
            ITrader quoteTrader)
        {
            Price = priceOfMinSize;
            Units = (int) (totalSize / minSize);
            Id = id;
            Minsize = minSize;
            _quoteTrader = quoteTrader;
        }

        public void AdjustBuyPosition(BuyPosition position)
        {
            Units -= position.AdjustWithQuote(this, _quoteTrader);
        }

        public void AdjustSellPosition(SellPosition position)
        {
            Units -= position.AdjustWithQuote(this, _quoteTrader);
        }

        public int CompareTo(Quote other)
        {
            //we DONT check for ref equality, its OK as price is READONLY
            return ReferenceEquals(null, other)
                ? 1
                : Price.CompareTo(other.Price);
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