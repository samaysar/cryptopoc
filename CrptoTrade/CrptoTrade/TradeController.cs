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
        public async Task<TradeResponse> FakeBuy([FromUri] int currency, [FromUri] decimal dollarAmount)
        {
            if (currency < 0 || currency > 2)
            {
                return new TradeResponse
                {
                    Error = "Currency Value invalid, Use 0,1,2"
                };
            }
            var currencyEnum = (CryptoCurrency) currency;
            var result = await _factory.GetTrader(currencyEnum).TradeAsync(new BuyPosition(dollarAmount))
                .ConfigureAwait(false);
            result.Initial += " $";
            result.Untraded += " $";
            result.TotalTraded += $" {currencyEnum}";

            return result;
        }

        [HttpGet]
        [Route("fakesell")]
        public async Task<TradeResponse> FakeSell([FromUri] int currency, [FromUri] decimal currencyAmount)
        {
            if (currency < 0 || currency > 2)
            {
                return new TradeResponse
                {
                    Error = "Currency Value invalid, Use 0,1,2"
                };
            }
            var currencyEnum = (CryptoCurrency) currency;
            var result = await _factory.GetTrader(currencyEnum).TradeAsync(new SellPosition(currencyAmount))
                .ConfigureAwait(false);
            result.Initial += $" {currencyEnum}";
            result.Untraded += $" {currencyEnum}";
            result.TotalTraded += $" {currencyEnum}";

            return result;
        }
    }
}