using System.Threading.Tasks;
using CrptoTrade.Assets;

namespace CrptoTrade.Trading
{
    public interface IBuyer
    {
        //returns unfilled size
        Task<decimal> BuyAsync(decimal size, decimal price);
    }

    public interface ISeller
    {
        //returns unfilled size
        Task<decimal> SellAsync(decimal size, decimal price);
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
        public abstract Task<decimal> BuyAsync(decimal size, decimal price);
        public abstract Task<decimal> SellAsync(decimal size, decimal price);
    }
}