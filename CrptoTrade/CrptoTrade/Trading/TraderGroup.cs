using System.Linq;
using System.Threading.Tasks;
using CrptoTrade.Assets;

namespace CrptoTrade.Trading
{
    public class TraderGroup
    {
        private readonly AskHeap _askHeap;
        private readonly BidHeap _bidHeap;

        public TraderGroup(CryptoCurrency currency, IExchange[] xchgs)
        {
            _askHeap = new AskHeap(xchgs.Select(x => x.AskHeap(currency)).ToList());
            _bidHeap = new BidHeap(xchgs.Select(x => x.BidHeap(currency)).ToList());
        }

        public Task TradeAsync(BuyPosition buy)
        {
            return Task.CompletedTask;
        }
    }
}