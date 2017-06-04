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
    public class HeapsTest
    {
        [Test]
        [TestCase(true, 256 * 1024, false, 0)] //256k
        [TestCase(false, 256 * 1024, false, 0)] //256k
        [TestCase(true, 16 * 1024 * 1024, false, 0)] //16M
        [TestCase(false, 16 * 1024 * 1024, false, 0)] //16M
        [TestCase(true, 256 * 1024, true, 2)] //256k
        [TestCase(false, 256 * 1024, true, 2)] //256k
        [TestCase(true, 16 * 1024 * 1024, true, 8)] //16M
        [TestCase(false, 16 * 1024 * 1024, true, 8)] //16M
        public void Heap_Latency(bool maxHeap, int size, bool measureExtract, int extractCon)
        {
            var arr = new Wrapper[size];
            Populate(arr);
            var h = CreateHeap(arr, maxHeap);
            if (measureExtract)
            {
                MeasureExtractTime(h, extractCon);
            }
            else
            {
                MakeAsserts(arr, h, maxHeap);
            }
            Assert.False(h.TryGetRoot(out Wrapper val));
        }

        private static void MakeAsserts(Wrapper[] arr, AbsHeap<Wrapper> h, bool maxHeap)
        {
            arr = maxHeap
                    ? arr.OrderByDescending(x => x.Value).ToArray()
                    : arr.OrderBy(x => x.Value).ToArray();
            for (var i = 0; i < arr.Length;)
            {
                Assert.True(h.TryGetRoot(out Wrapper val) && arr[i++].Value.Equals(val.Value));
            }
        }

        private static void MeasureExtractTime(AbsHeap<Wrapper> h, int con)
        {
            var sw = Stopwatch.StartNew();
            Parallel.For(0, con, i =>
            {
                while (h.TryGetRoot(out Wrapper val))
                {
                }
            });
            sw.Stop();
            Console.Out.WriteLine($"Extract Time:{sw.Elapsed.TotalMilliseconds}");
        }

        private static void Populate(Wrapper[] arr)
        {
            var ran = new Random();
            Parallel.For(0, arr.Length, new ParallelOptions
            {
                MaxDegreeOfParallelism = Environment.ProcessorCount
            }, i =>
            {
                arr[i] = new Wrapper
                {
                    Value = ran.Next(),
                    Id = Guid.NewGuid().ToString("N")
                };
            });
        }

        private static AbsHeap<Wrapper> CreateHeap(IReadOnlyList<Wrapper> arr, bool max = true)
        {
            var h = max
                ? (AbsHeap<Wrapper>) new MaxHeap<Wrapper>(0, new WrapperEquality(), arr.Count)
                : new MinHeap<Wrapper>(0, new WrapperEquality(), arr.Count);
            var sw = Stopwatch.StartNew();
            for (var i = 0; i < arr.Count;)
            {
                h.Insert(arr[i++]);
            }
            sw.Stop();
            Console.Out.WriteLine($"Insert Time:{sw.Elapsed.TotalMilliseconds} for {arr.Count}");
            return h;
        }


        private class Wrapper : IComparable<Wrapper>
        {
            public int Value;
            public string Id;

            public int CompareTo(Wrapper other)
            {
                return ReferenceEquals(null, other)
                    ? 1
                    : (ReferenceEquals(this, other) ? 0 : Value.CompareTo(other.Value));
            }
        }

        private class WrapperEquality : IEqualityComparer<Wrapper>
        {
            public bool Equals(Wrapper x, Wrapper y)
            {
                return x.Id.Equals(y.Id);
            }

            public int GetHashCode(Wrapper obj)
            {
                return obj.Id.GetHashCode();
            }
        }
    }
}