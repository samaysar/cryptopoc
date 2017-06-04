using System;
using System.Collections.Generic;

namespace CrptoTrade.Assets
{
    public class Quote : IComparable<Quote>
    {
        //we go low on GC
        public static readonly IEqualityComparer<Quote> Equality = new QuoteEquality();

        //can we set it somewhr as const value?
        private readonly double _minsize;
        private double _units;
        public readonly double Price;
        public readonly string Id;

        public bool CanRemove => _units.Equals(0);

        public Quote(double price, double units, string id, double minsize)
        {
            Price = price;
            _units = units;
            Id = id;
            _minsize = minsize;
        }

        public double ReduceValue(double tradeValue)
        {
            var tradableUnits = Math.Min(((int) ((tradeValue / Price) / _minsize)) * _minsize, _units);
            _units -= tradableUnits;
            return tradeValue - (tradableUnits * Price);
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