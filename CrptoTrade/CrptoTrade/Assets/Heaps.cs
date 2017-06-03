using System;
using System.Collections.Generic;

namespace CrptoTrade.Assets
{
    //We need to have IComparable<T> on Bid/Ask price and IEquatable<T> on the identifier, if any (for streaming data REMOVAL purpose)

    public sealed class MinHeap<T> : AbsHeap<T> where T : IComparable<T>
    {
        public MinHeap(int id, IEqualityComparer<T> equalityComparer, int initialCapax = 1000) : base(id, equalityComparer, initialCapax)
        {
        }

        protected override bool StopBubbleUp(T parent, T child)
        {
            return parent.CompareTo(child) < 1;
        }

        protected override bool MustBubbleUp(T child, T parent)
        {
            return child.CompareTo(parent) < 0;
        }
    }

    public sealed class MaxHeap<T> : AbsHeap<T> where T : IComparable<T>
    {
        public MaxHeap(int id, IEqualityComparer<T> equalityComparer, int initialCapax = 1000) : base(id, equalityComparer, initialCapax)
        {
        }

        protected override bool StopBubbleUp(T parent, T child)
        {
            return parent.CompareTo(child) > -1;
        }

        protected override bool MustBubbleUp(T child, T parent)
        {
            return child.CompareTo(parent) > 0;
        }
    }

    public abstract class AbsHeap<T>
    {
        private readonly int _id;
        private int _highIndex;
        private readonly object _syncRoot = new object();
        private readonly List<PositionWrapped> _data;

        //PartitionedDictionary ? => Not now!
        private readonly Dictionary<T, PositionWrapped> _positionLookUp;

        protected AbsHeap(int id, IEqualityComparer<T> equalityComparer, int initialCapax = 1000)
        {
            _id = id;
            _data = new List<PositionWrapped>(initialCapax);
            _positionLookUp = new Dictionary<T, PositionWrapped>(initialCapax, equalityComparer);
            _highIndex = -1;
        }

        public void Remove(T val)
        {
            lock (_syncRoot)
            {
                try
                {
                }
                finally
                {
                    if (_positionLookUp.TryGetValue(val, out PositionWrapped intervalVal))
                    {
                        var position = intervalVal.Position;
                        if (position != _highIndex)
                        {
                            _data[position] = _data[_highIndex];
                            _data[position].Position = position;
                            _data.RemoveAt(_highIndex);
                            if (position != --_highIndex) MinHeapify(position);
                        }
                        else
                        {
                            _data.RemoveAt(position);
                            _highIndex--;
                        }
                        _positionLookUp.Remove(val);
                    }
                }
            }
        }

        public void Insert(T newVal)
        {
            lock (_syncRoot)
            {
                var wrapper = new PositionWrapped
                {
                    Position = _data.Count,
                    Obj = newVal
                };
                _positionLookUp.Add(newVal, wrapper);
                _data.Add(wrapper);
                var current = ++_highIndex;
                while (current > 0)
                {
                    var parent = (current - 1) >> 2;
                    var parentVal = _data[parent];
                    if (StopBubbleUp(parentVal.Obj, newVal))
                    {
                        break;
                    }
                    _data[current] = _data[parent];
                    _data[current].Position = current;
                    _data[parent] = wrapper;
                    wrapper.Position = parent;

                    current = parent;
                }
            }
        }
        
        public bool TryExtractMin(out T min)
        {
            lock (_syncRoot)
            {
                if (_highIndex < 0)
                {
                    min = default(T);
                    return false;
                }

                min = _data[0].Obj;
                _data[0] = _data[_highIndex];
                _data[0].Position = 0;

                _positionLookUp.Remove(min);
                _data.RemoveAt(_highIndex--);
                MinHeapify(0);
                return true;
            }
        }

        public bool TryPeek(out T val)
        {
            lock (_syncRoot)
            {
                if (_highIndex < 0)
                {
                    val = default(T);
                    return false;
                }
                val = _data[0].Obj;
                return true;
            }
        }

        private void MinHeapify(int startPos)
        {
            try
            {
            }
            finally
            {
                var current = startPos;
                while (true)
                {
                    var l = (current << 2) + 1;
                    var r = l + 1;

                    if (l < _data.Count && MustBubbleUp(_data[l].Obj, _data[current].Obj))
                    {
                        current = l;
                    }
                    if (r < _data.Count && MustBubbleUp(_data[r].Obj, _data[current].Obj))
                    {
                        current = r;
                    }

                    if (current == startPos) break;
                    var tmp = _data[startPos];
                    _data[startPos] = _data[current];
                    _data[startPos].Position = startPos;
                    _data[current] = tmp;
                    _data[current].Position = current;
                    startPos = current;
                }
            }
        }

        protected abstract bool StopBubbleUp(T parent, T child);
        protected abstract bool MustBubbleUp(T child, T parent);

        private class PositionWrapped
        {
            public T Obj { get; set; }
            public int Position { get; set; }
        }
    }
}