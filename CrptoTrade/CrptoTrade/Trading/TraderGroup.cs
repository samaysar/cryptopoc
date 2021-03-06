﻿using System.Collections.Generic;
using System.Diagnostics;
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

        public async Task<TradeResponse> TradeAsync(BuyPosition buy)
        {
            var response = new TradeResponse
            {
                TradeSummary = new List<string>(),
                Initial = $"{buy.Current}"
            };
            var tasks = new List<Task<TradeInfo>>();
            var keepLooping = true;
            var cnt = 0;
            var dollarVal = 0m;
            var tradeSize = 0m;
            var sw = Stopwatch.StartNew();
            while (keepLooping)
            {
                buy.LastSize = 0;
                _askHeap.FindSizeAndPrice(buy);
                while (!buy.LastSize.Equals(0m))
                {
                    tasks.Add(GetBuyTask(buy));
                buy.LastSize = 0;
                    _askHeap.FindSizeAndPrice(buy);
                }
                if (tasks.Count > 0)
                {
                    //We can change this using Interleaved Tasks logic to gain some perf!
                    var finished = await Task.WhenAny(tasks).ConfigureAwait(false);
                    tasks.Remove(finished);
                    var info = await finished.ConfigureAwait(false);
                    response.TradeSummary.Add(info.Summary(++cnt));
                    buy.AddValue(info.Remains);
                    dollarVal += decimal.Round(info.DollarValue, 2);
                    tradeSize += info.TradeSize;
                }
                else
                {
                    keepLooping = false;
                }
            }
            sw.Stop();

            response.Untraded = $"{decimal.Round(buy.Current, 2)}";
            response.TotalValue = $"{decimal.Round(dollarVal, 2)} $";
            response.TotalTraded = $"{tradeSize}";
            response.MsTime = ((int) (sw.Elapsed.TotalMilliseconds * 1000)) / 1000.0m;
            return response;
        }

        private static Task<TradeInfo> GetBuyTask(BuyPosition buy)
        {
            var buyer = buy.LastBuyer;
            var size = buy.LastSize;
            var price = buy.LastPrice;
            return Task.Run(async () => await buyer.BuyAsync(size, price).ConfigureAwait(false));
        }

        private static Task<TradeInfo> GetSellTask(SellPosition sell)
        {
            var seller = sell.LastSeller;
            var size = sell.LastSize;
            var price = sell.LastPrice;
            return Task.Run(async () => await seller.SellAsync(size, price).ConfigureAwait(false));
        }

        public async Task<TradeResponse> TradeAsync(SellPosition sell)
        {
            var response = new TradeResponse
            {
                TradeSummary = new List<string>(),
                Initial = $"{sell.Current}"
            };
            var tasks = new List<Task<TradeInfo>>();
            var keepLooping = true;
            var cnt = 0;
            var dollarVal = 0m;
            var tradeSize = 0m;
            var sw = Stopwatch.StartNew();
            while (keepLooping)
            {
                sell.LastSize = 0;
                _bidHeap.FindPriceAdjustSize(sell);
                while (!sell.LastSize.Equals(0m))
                {
                    tasks.Add(GetSellTask(sell));
                    sell.LastSize = 0;
                    _bidHeap.FindPriceAdjustSize(sell);
                }
                if (tasks.Count > 0)
                {
                    //We can change this using Interleaved Tasks logic to gain some perf!
                    var finished = await Task.WhenAny(tasks).ConfigureAwait(false);
                    tasks.Remove(finished);
                    var info = await finished.ConfigureAwait(false);
                    response.TradeSummary.Add(info.Summary(++cnt));
                    sell.AddSize(info.Remains);
                    dollarVal += info.DollarValue;
                    tradeSize += info.TradeSize;
                }
                else
                {
                    keepLooping = false;
                }
            }
            sw.Stop();

            response.Untraded = $"{decimal.Round(sell.Current, 8)}";
            response.TotalValue = $"{decimal.Round(dollarVal, 2)} $";
            response.TotalTraded = $"{tradeSize}";
            response.MsTime = ((int)(sw.Elapsed.TotalMilliseconds * 1000)) / 1000.0m;
            return response;
        }
    }
}