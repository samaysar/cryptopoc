using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;
using CrptoTrade.Assets;
using Microsoft.Practices.ObjectBuilder2;
using Newtonsoft.Json.Linq;

namespace CrptoTrade.Trading
{
    public interface IBuyer
    {
        Task BuyAsync(decimal size, decimal price);
    }

    public interface ISeller
    {
        Task SellAsync(decimal size, decimal price);
    }

    public interface ITrader : IBuyer, ISeller
    {

    }

    public abstract class CurrencyTrader : ITrader
    {
        protected readonly MinHeap<Quote> _askHeap;
        protected readonly MaxHeap<Quote> _bidHeap;

        protected CurrencyTrader(int id, int initialCapex = 16*1024)
        {
            _askHeap = new MinHeap<Quote>(id, Quote.Equality, initialCapex);
            _bidHeap = new MaxHeap<Quote>(id, Quote.Equality, initialCapex);
        }

        public abstract void HandleQuote(string quoteMessage);
        public abstract Task BuyAsync(decimal size, decimal price);
        public abstract Task SellAsync(decimal size, decimal price);
    }
}