using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Net.Http;
using System.Threading.Tasks;
using CrptoTrade.Assets;
using Newtonsoft.Json.Linq;
using WebSocket4Net;

namespace CrptoTrade.Trading
{
    public interface IExchange
    {
        MinHeap<Quote> AskHeap(CryptoCurrency currency);
        MaxHeap<Quote> BidHeap(CryptoCurrency currency);
    }

    public class FakeExchange : GdaxExchange
    {
        private readonly Random _delay = new Random();
        public FakeExchange(HttpClient authClient, int id, int initialCapex = 16384)
            : base(authClient, id, initialCapex)
        {
        }

        protected override async Task<TradeInfo> TradeAsync(decimal size, decimal price, string ticker, bool buySide)
        {
            var sw = Stopwatch.StartNew();
            //we sleep for a while to simulate
            await Task.Delay(TimeSpan.FromMilliseconds(_delay.NextDouble() * _delay.Next(1, _delay.Next(5, 10))))
                .ConfigureAwait(false);
            sw.Stop();
            return new TradeInfo
            {
                Initial = size,
                Remains = 0,
                TradeSize = size,
                DollarValue = size * price,
                Quote = price,
                MsTime = ((int)(sw.Elapsed.TotalMilliseconds * 1000))/1000.0m
            };
        }
    }

    public class GdaxExchange : IExchange
    {
        private const decimal MinSize = 0.01m;
        private const decimal StepSize = 0.00000001m; //Satoshi
        private readonly HttpClient _authClient;
        private readonly int _id;
        private readonly Dictionary<CryptoCurrency, GdaxTrader> _heapLookup;
        private readonly QuoteSocket[] _sockets;

        public GdaxExchange(HttpClient authClient, int id, int initialCapex = 16384)
        {
            _authClient = authClient;
            _id = id;
            var btcTrader = new GdaxTrader(this, "BTC-USD", MinSize, StepSize, id, "Gdax BTC", initialCapex);
            var ethTrader = new GdaxTrader(this, "ETH-USD", MinSize, StepSize, id, "Gdax ETH", initialCapex);
            var ltcTrader = new GdaxTrader(this, "LTC-USD", MinSize, StepSize, id, "Gdax LTC", initialCapex);
            _heapLookup = new Dictionary<CryptoCurrency, GdaxTrader>
            {
                {CryptoCurrency.Btc, btcTrader},
                {CryptoCurrency.Eth, ethTrader},
                {CryptoCurrency.Ltc, ltcTrader}
            };
            _sockets = new[]
            {
                new QuoteSocket("wss://ws-feed.gdax.com", @"{""type"": ""subscribe"",""product_ids"": [""BTC-USD""]}",
                    btcTrader).Start(),
                new QuoteSocket("wss://ws-feed.gdax.com", @"{""type"": ""subscribe"",""product_ids"": [""ETH-USD""]}",
                    ethTrader).Start(),
                new QuoteSocket("wss://ws-feed.gdax.com", @"{""type"": ""subscribe"",""product_ids"": [""LTC-USD""]}",
                    ltcTrader).Start()
            };
        }

        public MinHeap<Quote> AskHeap(CryptoCurrency currency)
        {
            //if some exchange does NOT have given currency, no issue
            //we give empty heap... rest Heap of Heaps will do!
            return _heapLookup.TryGetValue(currency, out GdaxTrader trader)
                ? trader.AskHeap
                : new MinHeap<Quote>(_id, Quote.Equality, 0);
        }

        public MaxHeap<Quote> BidHeap(CryptoCurrency currency)
        {
            //if some exchange does NOT have given currency, no issue
            //we give empty heap... rest Heap of Heaps will do!
            return _heapLookup.TryGetValue(currency, out GdaxTrader trader)
                ? trader.BidHeap
                : new MaxHeap<Quote>(_id, Quote.Equality, 0);
        }

        //need to make it protected virtual to have easy FAKE xchg
        protected virtual async Task<TradeInfo> TradeAsync(decimal size, decimal price, string ticker, bool buySide)
        {
            //{
            //    "id": "68e6a28f-ae28-4788-8d4f-5ab4e5e5ae08",
            //    "size": "1.00000000",
            //    "product_id": "BTC-USD",
            //    "side": "buy",
            //    "stp": "dc",
            //    "funds": "9.9750623400000000",
            //    "specified_funds": "10.0000000000000000",
            //    "type": "market",
            //    "post_only": false,
            //    "created_at": "2016-12-08T20:09:05.508883Z",
            //    "done_at": "2016-12-08T20:09:05.527Z",
            //    "done_reason": "filled",
            //    "fill_fees": "0.0249376391550000",
            //    "filled_size": "0.01291771",
            //    "executed_value": "9.9750556620000000",
            //    "status": "done",
            //    "settled": true
            //}

            string id;
            decimal traded;
            var sw = Stopwatch.StartNew();
            var trdData =
                $@"{{""size"": ""{size}"",""price"": ""{price}"",""side"": ""{GetSide(buySide)}"",""product_id"": ""{ticker}"",""time_in_force"":""IOC""}}";
            using (var response = await _authClient.PostAsync("/orders", new StringContent(trdData))
                .ConfigureAwait(false))
            {
                response.EnsureSuccessStatusCode();
                var responseData = JObject.Parse(await response.Content.ReadAsStringAsync().ConfigureAwait(false));
                id = responseData.GetValue("id").Value<string>();
            }
            using (var response = await _authClient.GetAsync("/orders/" + id).ConfigureAwait(false))
            {
                response.EnsureSuccessStatusCode();
                var responseData = JObject.Parse(await response.Content.ReadAsStringAsync().ConfigureAwait(false));
                traded = responseData.GetValue("filled_size").Value<string>().ToDecimal();
            }
            sw.Stop();
            return new TradeInfo
            {
                Initial = size,
                Remains = size - traded,
                TradeSize = traded,
                DollarValue = traded * price,
                MsTime = ((int)(sw.Elapsed.TotalMilliseconds * 1000)) / 1000.0m
            };
        }

        private static string GetSide(bool buySide)
        {
            return buySide ? "buy" : "sell";
        }

        private class QuoteSocket
        {
            private readonly string _subscribeData;
            private readonly CurrencyTrader _quoteHandler;
            private readonly WebSocket _socket;

            public QuoteSocket(string socketUrl, string subscribeData,
                CurrencyTrader quoteHandler)
            {
                _subscribeData = subscribeData;
                _quoteHandler = quoteHandler;
                _socket = new WebSocket(socketUrl);
            }

            public QuoteSocket Start()
            {
                _socket.Opened += (sender, eventArgs) =>
                {
                    _socket.Send(_subscribeData);
                };
                _socket.Error += (sender, eventArgs) =>
                {
                    Console.Out.WriteLine("ERROR:" + eventArgs.Exception);
                };
                _socket.Closed += (sender, eventArgs) =>
                {
                    Console.Out.WriteLine($"Closing socket on subcription: {_subscribeData}");
                };
                _socket.MessageReceived += (sender, eventArgs) =>
                {
                    //todo: we dont sync order book...!
                    _quoteHandler.HandleQuote(eventArgs.Message);
                };
                _socket.Open();
                return this;
            }

            public void Close()
            {
                _socket.Close();
            }
        }

        private class GdaxTrader : CurrencyTrader
        {
            private readonly GdaxExchange _xchg;
            private readonly string _ticker;
            private readonly decimal _minSize;
            private readonly decimal _stepSize;

            public GdaxTrader(GdaxExchange xchg, string ticker, decimal minSize, decimal stepSize, int id,
                string heapName, int initialCapex = 16384) : base(id, heapName, initialCapex)
            {
                _xchg = xchg;
                _ticker = ticker;
                _minSize = minSize;
                _stepSize = stepSize;
            }

            public MinHeap<Quote> AskHeap => _askHeap;
            public MaxHeap<Quote> BidHeap => _bidHeap;

            public override Task<TradeInfo> BuyAsync(decimal size, decimal price)
            {
                return _xchg.TradeAsync(size, price, _ticker, true);
            }

            public override Task<TradeInfo> SellAsync(decimal size, decimal price)
            {
                return _xchg.TradeAsync(size, price, _ticker, false);
            }

            public override void HandleQuote(string quoteMessage)
            {
                //
                //Implementation based on https://github.com/coinbase/gdax-node/blob/master/lib/orderbook.js
                //
                var jsonQuote = JObject.Parse(quoteMessage);
                var type = jsonQuote.GetValue("type").Value<string>();
                var buyside = jsonQuote.GetValue("side").Value<string>() == "buy";
                switch (type)
                {
                    case "open":
                        if (buyside)
                        {
                            _bidHeap.InsertVal(new Quote(
                                jsonQuote.GetValue("price").Value<string>().ToDecimal(),
                                GetSize(jsonQuote),
                                GetOrderId(jsonQuote),
                                _stepSize,
                                this, _minSize));
                        }
                        else
                        {
                            _askHeap.InsertVal(new Quote(
                                jsonQuote.GetValue("price").Value<string>().ToDecimal(),
                                GetSize(jsonQuote),
                                GetOrderId(jsonQuote),
                                _stepSize,
                                this, _minSize));
                        }
                        break;
                    case "done":
                        if (buyside)
                        {
                            _bidHeap.RemoveVal(new Quote(0, 0, jsonQuote.GetValue("order_id").Value<string>(),
                                0, null, 0));
                        }
                        else
                        {
                            _askHeap.RemoveVal(new Quote(0, 0, jsonQuote.GetValue("order_id").Value<string>(),
                                0, null, 0));
                        }
                        break;
                    case "match":
                        if (buyside)
                        {
                            _bidHeap.Remove(new Quote(0, 0, jsonQuote.GetValue("maker_order_id").Value<string>(),
                                0, null, 0), quote =>
                            {
                                quote.AdjustSize(jsonQuote.GetValue("size").Value<string>().ToDecimal());
                                return quote.Invalid;
                            });
                        }
                        else
                        {
                            _askHeap.Remove(new Quote(0, 0, jsonQuote.GetValue("maker_order_id").Value<string>(),
                                0, null, 0), quote =>
                            {
                                quote.AdjustSize(jsonQuote.GetValue("size").Value<string>().ToDecimal());
                                return quote.Invalid;
                            });
                        }
                        break;
                    case "change":
                        if (buyside)
                        {
                            _bidHeap.Remove(new Quote(0, 0, jsonQuote.GetValue("order_id").Value<string>(),
                                0, null, 0), quote =>
                                {
                                    quote.NewSize(jsonQuote.GetValue("new_size").Value<string>().ToDecimal());
                                    return quote.Invalid;
                                });
                        }
                        else
                        {
                            _askHeap.Remove(new Quote(0, 0, jsonQuote.GetValue("order_id").Value<string>(),
                                0, null, 0), quote =>
                                {
                                    quote.NewSize(jsonQuote.GetValue("new_size").Value<string>().ToDecimal());
                                    return quote.Invalid;
                                });
                        }
                        break;
                    default:
                        break;
                }
            }

            private static decimal GetSize(JObject jsonQuote)
            {
                var size = jsonQuote.GetValue("size")?.Value<string>();
                return string.IsNullOrWhiteSpace(size)
                    ? jsonQuote.GetValue("remaining_size").Value<string>().ToDecimal()
                    : size.ToDecimal();
            }

            private static string GetOrderId(JObject jsonQuote)
            {
                var orderId = jsonQuote.GetValue("order_id")?.Value<string>();
                return string.IsNullOrWhiteSpace(orderId)
                    ? jsonQuote.GetValue("id").Value<string>()
                    : orderId;
            }
        }
    }
}