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
        private readonly MinHeap<Quote> _askHeap;
        private readonly MaxHeap<Quote> _bidHeap;
        private readonly BlockingCollection<JObject> _quotes;
        private Task _quotePoulationTask;

        protected CurrencyTrader(int id, int initialCapex = 16*1024)
        {
            _askHeap = new MinHeap<Quote>(id, Quote.Equality, initialCapex);
            _bidHeap = new MaxHeap<Quote>(id, Quote.Equality, initialCapex);
            _quotes = new BlockingCollection<JObject>();
        }

        public void Start(JObject[] initialQuotes)
        {
            initialQuotes.ForEach(Populate);
            _quotePoulationTask = Task.Run(new Action(Populate));
        }

        private void Populate()
        {
            while (_quotes.TryTake(out JObject jsonQuote, Timeout.Infinite))
            {
                if (!IsValid(jsonQuote)) continue;
                Populate(jsonQuote);
            }
        }

        private void Populate(JObject jsonQuote)
        {
            if (ToQuote(jsonQuote, out Quote quote))
            {
                _bidHeap.InsertVal(quote);
            }
            else
            {
                _askHeap.InsertVal(quote);
            }
        }

        public MinHeap<Quote> AskHeap => _askHeap;
        public MaxHeap<Quote> BidHeap => _bidHeap;

        public void PopulateQuote(JObject quote)
        {
            _quotes.Add(quote);
        }

        public void Stop()
        {
            _quotes.CompleteAdding();
        }

        public abstract Task BuyAsync(decimal size, decimal price);
        public abstract Task SellAsync(decimal size, decimal price);
        //true for bid, false for ask
        public abstract bool ToQuote(JObject jsonQuote, out Quote quote);
        public abstract bool IsValid(JObject jsonQuote);
    }
}