using System;
using System.Net.Http;
using System.Threading.Tasks;
using System.Web.Http;
using CrptoTrade.Assets;
using CrptoTrade.Trading;

namespace CrptoTrade
{
    public enum CryptoCurrency
    {
        Btc = 0,
        Eth,
        Ltc
    }

    public class BuyRequest
    {
        public CryptoCurrency Currency { get; set; }
        public decimal DollarAmount { get; set; }
    }

    public class SellRequest
    {
        public CryptoCurrency Currency { get; set; }
        public decimal CurrencyAmount { get; set; }
    }

    [RoutePrefix("trade")]
    public class TradeController : ApiController
    {
        private readonly TradingFactory _factory;

        public TradeController(TradingFactory factory)
        {
            _factory = factory;
        }

        [HttpPost]
        [Route("buy")]
        public Task<HttpResponseMessage> Buy([FromBody] BuyRequest request)
        {
            throw new NotImplementedException();
        }

        [HttpPost]
        [Route("sell")]
        public Task<HttpResponseMessage> Sell([FromBody] SellRequest request)
        {
            throw new NotImplementedException();
        }

        [HttpGet]
        [Route("fakebuy")]
        public Task<TradeResponse> FakeBuy([FromUri] int currency, [FromUri] decimal dollarAmount)
        {
            if (currency < 0 || currency > 2)
            {
                return Task.FromResult(new TradeResponse
                {
                    Error = $"Currency Value invalid, Use 0,1,2"
                });
            }
            var currencyEnum = (CryptoCurrency) currency;
            return _factory.GetTrader(currencyEnum).TradeAsync(new BuyPosition(dollarAmount));

        }

        [HttpGet]
        [Route("fakesell")]
        public Task<TradeResponse> FakeSell([FromUri] int currency, [FromUri] decimal currencyAmount)
        {
            if (currency < 0 || currency > 2)
            {
                return Task.FromResult(new TradeResponse
                {
                    Error = $"Currency Value invalid, Use 0,1,2"
                });
            }
            var currencyEnum = (CryptoCurrency) currency;
            return _factory.GetTrader(currencyEnum).TradeAsync(new SellPosition(currencyAmount));
        }
    }
}