using System.Collections.Generic;

namespace CrptoTrade.Trading
{
    public class TradingFactory
    {
        private readonly Dictionary<CryptoCurrency, TraderGroup> _lookup;

        public TradingFactory()
        {
            _lookup = new Dictionary<CryptoCurrency, TraderGroup>
            {
                [CryptoCurrency.Btc] = new TraderGroup(CryptoCurrency.Btc),
                [CryptoCurrency.Eth] = new TraderGroup(CryptoCurrency.Eth),
                [CryptoCurrency.Ltc] = new TraderGroup(CryptoCurrency.Ltc)
            };
        }

        public TraderGroup GetTrader(CryptoCurrency currency)
        {
            return _lookup[currency];
        }
    }
}