using System.Collections.Generic;

namespace CrptoTrade.Trading
{
    public class TradingFactory
    {
        private readonly Dictionary<CryptoCurrency, TraderGroup> _lookup;

        public TradingFactory(IExchange[] xchgs)
        {
            _lookup = new Dictionary<CryptoCurrency, TraderGroup>
            {
                [CryptoCurrency.Btc] = new TraderGroup(CryptoCurrency.Btc, xchgs),
                [CryptoCurrency.Eth] = new TraderGroup(CryptoCurrency.Eth, xchgs),
                [CryptoCurrency.Ltc] = new TraderGroup(CryptoCurrency.Ltc, xchgs)
            };
        }

        public TraderGroup GetTrader(CryptoCurrency currency)
        {
            return _lookup[currency];
        }
    }
}