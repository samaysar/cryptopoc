using System.Threading.Tasks;
using CrptoTrade.Assets;

namespace CrptoTrade.Trading
{
    public interface IBuyer
    {
        //returns unfilled size
        Task<TradeInfo> BuyAsync(decimal size, decimal price);
    }

    public interface ISeller
    {
        //returns unfilled size
        Task<TradeInfo> SellAsync(decimal size, decimal price);
    }

    public interface ITrader : IBuyer, ISeller
    {

    }

    public abstract class CurrencyTrader : ITrader
    {
        protected readonly MinHeap<Quote> _askHeap;
        protected readonly MaxHeap<Quote> _bidHeap;

        protected CurrencyTrader(int id, string heapName, int initialCapex = 16*1024)
        {
            _askHeap = new MinHeap<Quote>(id, Quote.Equality, initialCapex, heapName);
            _bidHeap = new MaxHeap<Quote>(id, Quote.Equality, initialCapex, heapName);
        }

        public abstract void HandleQuote(string quoteMessage);
        public abstract Task<TradeInfo> BuyAsync(decimal size, decimal price);
        public abstract Task<TradeInfo> SellAsync(decimal size, decimal price);
    }
}