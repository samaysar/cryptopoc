using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Threading.Tasks;
using CrptoTrade.Assets;
using NUnit.Framework;

namespace CryptoTrade.Tests
{
    [TestFixture]
    public class BidAskHeapsTest
    {
        [Test]
        public void Ask_Heap_Latency()
        {
            const int size = 1024 * 1024;
            var arr1 = new Quote[128 * 1024];
            Populate(arr1);
            var arr2 = new Quote[(1024 - 129) * 1024 - 1];
            Populate(arr2);
            var arr3 = new Quote[1025];
            Populate(arr3);

            var quote1 = new Quote(0.5m, 1, Guid.NewGuid().ToString(), 1, null);
            var quote2 = new Quote(0.6m, 1, Guid.NewGuid().ToString(), 1, null);
            var quote3 = new Quote(0.7m, 1, Guid.NewGuid().ToString(), 1, null);

            var fullArray = new Quote[size + 3];
            Array.Copy(arr1, 0, fullArray, 0, arr1.Length);
            Array.Copy(arr2, 0, fullArray, arr1.Length, arr2.Length);
            Array.Copy(arr3, 0, fullArray, arr1.Length + arr2.Length, arr3.Length);
            fullArray[fullArray.Length - 3] = quote1;
            fullArray[fullArray.Length - 2] = quote2;
            fullArray[fullArray.Length - 1] = quote3;

            fullArray = fullArray.OrderBy(x => x.Price).ToArray();

            var heaps = new[]
            {
                CreateMinHeap(arr1, 0),
                CreateMinHeap(arr2, 1),
                CreateMinHeap(arr3, 2)
            };
            var askHeap = new AskHeap(heaps);
            heaps[0].InsertVal(quote1);
            heaps[1].InsertVal(quote2);
            heaps[2].InsertVal(quote3);

            var position = new BuyPosition(fullArray.Sum(x => x.Price));
            var sw = Stopwatch.StartNew();
            Parallel.For(0, 1, i =>
            {
                var cnt = 0;
                do
                {
                    //position.LastSize = 0;
                    askHeap.FindSizeAndPrice(position);
                    //Assert.True(fullArray[cnt].Price.Equals(position.LastPrice));
                } while (++cnt < fullArray.Length);

                //position.LastSize = 0;
                //askHeap.FindSizeAndPrice(position);
                //Assert.True(position.LastSize.Equals(0));
            });
            sw.Stop();
            Console.Out.WriteLine($"AskHeap Extract Time:{sw.Elapsed.TotalMilliseconds}");
        }

        [Test]
        public void Bid_Heap_Latency()
        {
            const int size = 1024 * 1024;
            var arr1 = new Quote[128 * 1024];
            Populate(arr1);
            var arr2 = new Quote[(1024 - 129) * 1024 - 1];
            Populate(arr2);
            var arr3 = new Quote[1025];
            Populate(arr3);

            var quote1 = new Quote(0.5m, 1, Guid.NewGuid().ToString(), 1, null);
            var quote2 = new Quote(0.6m, 1, Guid.NewGuid().ToString(), 1, null);
            var quote3 = new Quote(0.7m, 1, Guid.NewGuid().ToString(), 1, null);

            var fullArray = new Quote[size + 3];
            Array.Copy(arr1, 0, fullArray, 0, arr1.Length);
            Array.Copy(arr2, 0, fullArray, arr1.Length, arr2.Length);
            Array.Copy(arr3, 0, fullArray, arr1.Length + arr2.Length, arr3.Length);
            fullArray[fullArray.Length - 3] = quote1;
            fullArray[fullArray.Length - 2] = quote2;
            fullArray[fullArray.Length - 1] = quote3;

            fullArray = fullArray.OrderByDescending(x => x.Price).ToArray();

            var heaps = new[]
            {
                CreateMaxHeap(arr1, 0),
                CreateMaxHeap(arr2, 1),
                CreateMaxHeap(arr3, 2)
            };
            var askHeap = new BidHeap(heaps);
            heaps[0].InsertVal(quote1);
            heaps[1].InsertVal(quote2);
            heaps[2].InsertVal(quote3);

            var position = new SellPosition(fullArray.Length);
            var sw = Stopwatch.StartNew();
            Parallel.For(0, 1, i =>
            {
                var cnt = 0;
                do
                {
                    //position.LastSize = 0;
                    askHeap.FindPriceAdjustSize(position);
                    //Assert.True(fullArray[cnt].Price.Equals(position.LastPrice));
                } while (++cnt < fullArray.Length);

                //position.LastSize = 0;
                //askHeap.FindPriceAdjustSize(position);
                //Assert.True(position.LastSize.Equals(0));
            });
            sw.Stop();
            Console.Out.WriteLine($"AskHeap Extract Time:{sw.Elapsed.TotalMilliseconds}");
        }

        private static void Populate(Quote[] arr)
        {
            var ran = new Random();
            Parallel.For(0, arr.Length, new ParallelOptions
            {
                MaxDegreeOfParallelism = Environment.ProcessorCount
            }, i =>
            {
                arr[i] = new Quote((new decimal((int) (ran.NextDouble() / 0.00000001)) * 0.00000001m) + 1, 1,
                    Guid.NewGuid().ToString(), 1, null);
            });
        }

        private static MinHeap<Quote> CreateMinHeap(IReadOnlyList<Quote> arr, int id)
        {
            var h = new MinHeap<Quote>(id, Quote.Equality, arr.Count);
            var sw = Stopwatch.StartNew();
            for (var i = 0; i < arr.Count;)
            {
                h.InsertVal(arr[i++]);
            }
            sw.Stop();
            Console.Out.WriteLine($"MinHeap Insert Time:{sw.Elapsed.TotalMilliseconds} for {arr.Count}");
            return h;
        }

        private static MaxHeap<Quote> CreateMaxHeap(IReadOnlyList<Quote> arr, int id)
        {
            var h = new MaxHeap<Quote>(id, Quote.Equality, arr.Count);
            var sw = Stopwatch.StartNew();
            for (var i = 0; i < arr.Count;)
            {
                h.InsertVal(arr[i++]);
            }
            sw.Stop();
            Console.Out.WriteLine($"MaxHeap Insert Time:{sw.Elapsed.TotalMilliseconds} for {arr.Count}");
            return h;
        }
    }
}