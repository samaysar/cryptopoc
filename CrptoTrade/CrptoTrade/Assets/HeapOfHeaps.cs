using System;
using System.Collections.Generic;

namespace CrptoTrade.Assets
{
    //it's a min heap of min heaps
    //Idea is to create 1 heap per Exchange (GDAX etc)
    //then create a heap on all Exchange heaps
    //Benefit is that all exchange quotes can be heapified without taking
    //locks on a global Heap (in case we merge all data in a single global heap)
    //
    //With the help of RootChanged event, we reHeapify this heap
    //which is comparatively VERY SMALL, thus locking time is LOW!
    //
    //Though, while extracting quotes, those individual exchange
    //heap may change thier Root value and we wont be able to catch
    //it, as event based re-heapification will occur after,
    //but thats an opportunity cost... which can occur on the order
    //itself so there is anyway no guarantee!
    //
    // --------------
    //Now we create Ask heap as "Min Heap On Min Heaps" so that we can buy
    //on cheaper quotes
    // --------------
    //And Bid heap as "Max Heap On Max Heaps" so that we can sell
    //on costliest quotes

    public class AskHeap : AbsHeap<MinHeap<Quote>>
    {
        private readonly MinHeap<Quote>[] _minHeaps;

        public AskHeap(IReadOnlyCollection<MinHeap<Quote>> minHeaps)
            : base(0, AbsHeap<Quote>.Equality, minHeaps.Count)
        {
            _minHeaps = new MinHeap<Quote>[minHeaps.Count];
            foreach (var minHeap in minHeaps)
            {
                _minHeaps[minHeap.Id] = minHeap;
                Insert(minHeap);
                minHeap.RootChanged += OnMinChanged;
            }
        }

        public void FindSizeAndPrice(TradePosition position)
        {
            //going to return true, when Trade position consumes
            //all the units of the Quote, so the Min will change
            //thus requirement of reheapification.
            if (TryPeekOrExtract(heap =>
            {
                return heap.TryPeekOrGetMin(quote =>
                {
                    quote.AdjustBuyPosition(position);
                    return quote.IsZero;
                }, out Quote outQuote);
            }, false, out MinHeap<Quote> affectedHeap))
            {
                //we set 3rd param false so that Heap does NOT change
                //and if return value is TRUE it means our quote heap
                //was modified!
                OnMinChanged(affectedHeap);
            }
        }

        private void OnMinChanged(int id)
        {
            OnMinChanged(_minHeaps[id]);
        }

        private void OnMinChanged(MinHeap<Quote> heap)
        {
            Reheapify(heap);
        }

        protected override bool StopBubbleUp(MinHeap<Quote> parent, MinHeap<Quote> child)
        {
            return parent.CompareTo(child) < 1;
        }

        protected override bool MustBubbleUp(MinHeap<Quote> child, MinHeap<Quote> parent)
        {
            return child.CompareTo(parent) < 0;
        }
    }

    public class BidHeap : AbsHeap<MaxHeap<Quote>>
    {
        private readonly MaxHeap<Quote>[] _maxHeaps;

        public BidHeap(IReadOnlyCollection<MaxHeap<Quote>> maxHeaps)
            : base(0, AbsHeap<Quote>.Equality, maxHeaps.Count)
        {
            _maxHeaps = new MaxHeap<Quote>[maxHeaps.Count];
            foreach (var maxHeap in maxHeaps)
            {
                _maxHeaps[maxHeap.Id] = maxHeap;
                Insert(maxHeap);
                maxHeap.RootChanged += OnMaxChanged;
            }
        }

        public void FindPriceAdjustSize(TradePosition position)
        {
            //going to return true, when Trade position consumes
            //all the units of the Quote, so the Min will change
            //thus requirement of reheapification.
            if (TryPeekOrExtract(heap =>
            {
                return heap.TryPeekOrGetMax(quote =>
                {
                    quote.AdjustSellPosition(position);
                    return quote.IsZero;
                }, out Quote nouse);
            }, false, out MaxHeap<Quote> affectedHeap))
            {
                //we set 3rd param false so that Heap does NOT change
                //and if return value is TRUE it means our quote heap
                //was modified!
                OnMaxChanged(affectedHeap);
            }
        }

        private void OnMaxChanged(int id)
        {
            OnMaxChanged(_maxHeaps[id]);
        }

        private void OnMaxChanged(MaxHeap<Quote> heap)
        {
            Reheapify(heap);
        }

        protected override bool StopBubbleUp(MaxHeap<Quote> parent, MaxHeap<Quote> child)
        {
            return parent.CompareTo(child) > -1;
        }

        protected override bool MustBubbleUp(MaxHeap<Quote> child, MaxHeap<Quote> parent)
        {
            return child.CompareTo(parent) > 0;
        }
    }
}