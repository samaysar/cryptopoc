using System;
using System.Net.Http;
using System.Threading.Tasks;
using System.Web.Http;
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
    }
}