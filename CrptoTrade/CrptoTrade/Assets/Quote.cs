﻿using System;
using System.Collections.Generic;

namespace CrptoTrade.Assets
{
    public class Quote : IComparable<Quote>
    {
        //we go low on GC
        public static readonly IEqualityComparer<Quote> Equality = new QuoteEquality();

        //can we set it somewhr as const value?
        private readonly double _minsize;
        private int _units;
        public readonly double Price;
        public readonly string Id;

        public bool IsZero => _units == 0;

        public Quote(double priceOfMinSize, double totalSize, string id, double minSize)
        {
            Price = priceOfMinSize;
            _units = (int) (totalSize / minSize);
            Id = id;
            _minsize = minSize;
        }

        public void AdjustBuyPosition(TradePosition position)
        {
            var tradableUnits = Math.Min((int) (position.Value / Price), _units);
            _units -= tradableUnits;
            position.Size = tradableUnits * _minsize;
            position.Value -= (tradableUnits * Price);
            position.Price = Price;
        }

        public void AdjustSellPosition(TradePosition position)
        {
            var tradableUnits = Math.Min((int) (position.Size / _minsize), _units);
            _units -= tradableUnits;
            position.Size -= (tradableUnits * _minsize);
            position.Value += (tradableUnits * Price);
            position.Price = Price;
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