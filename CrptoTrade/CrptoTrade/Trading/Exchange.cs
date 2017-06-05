using System;
using System.Collections.Generic;
using System.Net.Http;
using System.Threading.Tasks;
using CrptoTrade.Assets;
using Newtonsoft.Json.Linq;
using WebSocket4Net;

namespace CrptoTrade.Trading
{
    public class GdaxExchange
    {
        private readonly int _id;

        private readonly Dictionary<CryptoCurrency, CurrencyTrader> _heapLookup;

        public GdaxExchange(HttpClient client, int id, int initialCapex = 16384)
        {
            _id = id;
            var btcTrader = new BtcTrader(client, id, initialCapex);
            _heapLookup = new Dictionary<CryptoCurrency, CurrencyTrader>
            {
                {CryptoCurrency.Btc, btcTrader},
                //{CryptoCurrency.Ltc, new LtcTrader(client, id, initialCapex)},
                //{CryptoCurrency.Eth, new EthTrader(client, id, initialCapex)}
            };
            var websocket = new WebSocket("wss://ws-feed.gdax.com");
            //""ETH-USD"",
            //""ETH-BTC""
            websocket.Opened += (sender, eventArgs) =>
            {
                websocket.Send(@"{
                ""type"": ""subscribe"",
                ""product_ids"": [
                    ""BTC-USD"",
                ]
            }");
            };
            websocket.Error += (sender, eventArgs) =>
            {
                Console.Out.WriteLine("ERROR:" + eventArgs.Exception);
            };
            websocket.Closed += (sender, eventArgs) =>
            {
                
            };
            websocket.MessageReceived += (sender, eventArgs) =>
            {
                btcTrader.HandleQuote(eventArgs.Message);
            };
            websocket.Open();

        }

        // More refactoring is possible as ONLY currency symbol is changing and XCHG... Later!!!
        private class BtcTrader : CurrencyTrader
        {
            private const decimal MinSize = 0.01m;
            private const decimal StepSize = 0.00000001m;
            private const decimal MaxSize = 10000;
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

            public override void HandleQuote(string quoteMessage)
            {
                //{
                //    "type":"received",
	               // "order_id":"48406ee0-55dc-4933-8de7-d8a5b2ee146e",
	               // "order_type":"limit",
	               // "size":"0.99000000",
	               // "price":"2581.11000000",
	               // "side":"buy",
	               // "client_oid":"4b5118a8-75b8-4029-ad90-0bc5e028ccc7",
	               // "product_id":"BTC-USD",
	               // "sequence":3226912131,
	               // "time":"2017-06-05T12:40:41.192000Z"
                //}
                
            }
        }

        //private class EthTrader : CurrencyTrader
        //{
        //    private readonly HttpClient _client;

        //    public EthTrader(HttpClient client, int id, int initialCapex = 16384) : base(id, initialCapex)
        //    {
        //        _client = client;
        //    }

        //    public override Task BuyAsync(decimal size, decimal price)
        //    {
        //        throw new System.NotImplementedException();
        //    }

        //    public override Task SellAsync(decimal size, decimal price)
        //    {
        //        throw new System.NotImplementedException();
        //    }

        //    //true for bid, false for ask
        //    public override bool ToQuote(JObject jsonQuote, out Quote quote)
        //    {
        //        throw new System.NotImplementedException();
        //    }

        //    public override bool IsValid(JObject jsonQuote)
        //    {
        //        throw new System.NotImplementedException();
        //    }
        //}

        //private class LtcTrader : CurrencyTrader
        //{
        //    private readonly HttpClient _client;

        //    public LtcTrader(HttpClient client, int id, int initialCapex = 16384) : base(id, initialCapex)
        //    {
        //        _client = client;
        //    }

        //    public override Task BuyAsync(decimal size, decimal price)
        //    {
        //        throw new System.NotImplementedException();
        //    }

        //    public override Task SellAsync(decimal size, decimal price)
        //    {
        //        throw new System.NotImplementedException();
        //    }

        //    //true for bid, false for ask
        //    public override bool ToQuote(JObject jsonQuote, out Quote quote)
        //    {
        //        throw new System.NotImplementedException();
        //    }

        //    public override bool IsValid(JObject jsonQuote)
        //    {
        //        throw new System.NotImplementedException();
        //    }
        //}
    }
}