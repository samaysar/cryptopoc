using System.Collections.Generic;
using System.Threading.Tasks;
using CrptoTrade.Assets;
using Microsoft.Practices.ObjectBuilder2;

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

    public abstract class Trader : ITrader
    {
        private readonly MinHeap<Quote> _askHeap;
        private readonly MaxHeap<Quote> _bidHeap;

        protected Trader(int id, int initialCapex = 16*1024)
        {
            _askHeap = new MinHeap<Quote>(id, Quote.Equality, initialCapex);
            _bidHeap = new MaxHeap<Quote>(id, Quote.Equality, initialCapex);
        }

        protected void FillAsk(Quote quote)
        {
            _askHeap.InsertVal(quote);
        }

        protected void FillBid(Quote quote)
        {
            _bidHeap.InsertVal(quote);
        }

        public abstract Task BuyAsync(decimal size, decimal price);
        public abstract Task SellAsync(decimal size, decimal price);
    }
}