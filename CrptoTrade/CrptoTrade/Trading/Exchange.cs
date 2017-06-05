using System.Collections.Generic;
using System.Net.Http;
using System.Threading.Tasks;
using CrptoTrade.Assets;
using Newtonsoft.Json.Linq;

namespace CrptoTrade.Trading
{
    public class GdaxExchange
    {
        private readonly int _id;

        private readonly Dictionary<CryptoCurrency, CurrencyTrader> _heapLookup;

        public GdaxExchange(HttpClient client, int id, int initialCapex = 16384)
        {
            _id = id;
            _heapLookup = new Dictionary<CryptoCurrency, CurrencyTrader>
            {
                {CryptoCurrency.Btc, new BtcTrader(client, id, initialCapex)},
                {CryptoCurrency.Ltc, new LtcTrader(client, id, initialCapex)},
                {CryptoCurrency.Eth, new EthTrader(client, id, initialCapex)}
            };
        }

        // More refactoring is possible as ONLY currency symbol is changing and XCHG... Later!!!
        private class BtcTrader : CurrencyTrader
        {
            private readonly HttpClient _client;

            public BtcTrader(HttpClient client, int id, int initialCapex = 16384) : base(id, initialCapex)
            {
                _client = client;
            }

            public override Task BuyAsync(decimal size, decimal price)
            {
                throw new System.NotImplementedException();
            }

            public override Task SellAsync(decimal size, decimal price)
            {
                throw new System.NotImplementedException();
            }

            //true for bid, false for ask
            public override bool ToQuote(JObject jsonQuote, out Quote quote)
            {
                throw new System.NotImplementedException();
            }

            public override bool IsValid(JObject jsonQuote)
            {
                throw new System.NotImplementedException();
            }
        }

        private class EthTrader : CurrencyTrader
        {
            private readonly HttpClient _client;

            public EthTrader(HttpClient client, int id, int initialCapex = 16384) : base(id, initialCapex)
            {
                _client = client;
            }

            public override Task BuyAsync(decimal size, decimal price)
            {
                throw new System.NotImplementedException();
            }

            public override Task SellAsync(decimal size, decimal price)
            {
                throw new System.NotImplementedException();
            }

            //true for bid, false for ask
            public override bool ToQuote(JObject jsonQuote, out Quote quote)
            {
                throw new System.NotImplementedException();
            }

            public override bool IsValid(JObject jsonQuote)
            {
                throw new System.NotImplementedException();
            }
        }

        private class LtcTrader : CurrencyTrader
        {
            private readonly HttpClient _client;

            public LtcTrader(HttpClient client, int id, int initialCapex = 16384) : base(id, initialCapex)
            {
                _client = client;
            }

            public override Task BuyAsync(decimal size, decimal price)
            {
                throw new System.NotImplementedException();
            }

            public override Task SellAsync(decimal size, decimal price)
            {
                throw new System.NotImplementedException();
            }

            //true for bid, false for ask
            public override bool ToQuote(JObject jsonQuote, out Quote quote)
            {
                throw new System.NotImplementedException();
            }

            public override bool IsValid(JObject jsonQuote)
            {
                throw new System.NotImplementedException();
            }
        }
    }
}