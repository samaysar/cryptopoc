using System;
using System.Collections.Generic;

namespace CrptoTrade.Assets
{
    public delegate void MinChangeHandler(int id);

    //We need to have IComparable<T> on Bid/Ask price and IEquatable<T> on the identifier, if any (for streaming data REMOVAL purpose)

    public sealed class MinHeap<T> : AbsHeap<T> where T : IComparable<T>
    {
        public MinHeap(int id, IEqualityComparer<T> equalityComparer, int initialCapax = 16 * 1024)
            : base(id, 1, equalityComparer, initialCapax)
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

        public void RemoveVal(T val)
        {
            Remove(val);
        }

        public void InsertVal(T newVal)
        {
            Insert(newVal);
        }

        public bool TryGetMin(out T val)
        {
            return TryPeekOrGetMin(x => true, out val);
        }

        public bool TryPeekOrGetMin(Func<T, bool> predicate, out T val)
        {
            return TryPeekOrExtract(predicate, true, out val);
        }
    }

    public sealed class MaxHeap<T> : AbsHeap<T> where T : IComparable<T>
    {
        public MaxHeap(int id, IEqualityComparer<T> equalityComparer, int initialCapax = 16 * 1024)
            : base(id, -1, equalityComparer, initialCapax)
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

        public void RemoveVal(T val)
        {
            Remove(val);
        }

        public void InsertVal(T newVal)
        {
            Insert(newVal);
        }

        public bool TryGetMax(out T val)
        {
            return TryPeekOrGetMax(x => true, out val);
        }

        public bool TryPeekOrGetMax(Func<T, bool> predicate, out T val)
        {
            return TryPeekOrExtract(predicate, true, out val);
        }
    }

    public abstract class AbsHeap<T> : IComparable<AbsHeap<T>> where T : IComparable<T>
    {
        private readonly int _emptyDefault;
        public static readonly IEqualityComparer<AbsHeap<T>> Equality = new AbsHeapEquality();
        private int _highIndex;
        private readonly object _syncRoot = new object();
        private readonly List<PositionWrapped> _data;

        //PartitionedDictionary ? => Not now!
        private readonly Dictionary<T, PositionWrapped> _positionLookUp;
        public event MinChangeHandler RootChanged = x => { };

        protected AbsHeap(int id, int emptyDefault, IEqualityComparer<T> equalityComparer, int initialCapax = 16 * 1024)
        {
            _emptyDefault = emptyDefault;
            Id = id;
            _data = new List<PositionWrapped>(initialCapax);
            _positionLookUp = new Dictionary<T, PositionWrapped>(initialCapax, equalityComparer);
            _highIndex = -1;
        }

        public int Id { get; }

        protected void Remove(T val)
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
                            _data[position] = new PositionWrapped(_data[_highIndex].Obj, position);
                            _data.RemoveAt(_highIndex);
                            if (position != --_highIndex) Heapify(position);
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

        protected void ReheapifyUnsafe(T val)
        {
            if (_positionLookUp.TryGetValue(val, out PositionWrapped wrapper))
            {
                lock (_syncRoot)
                {
                    try
                    {
                    }
                    finally
                    {

                        var current = wrapper.Position;
                        //we need to do BOTH bubbleUP and DOWN as we dont know whats the new value
                        //but as we know values are continously changing behind the scene
                        //we should perform ONLY one side... so that we can get out of the lock
                        //quickly

                        while (current > 0)
                        {
                            var parent = (current - 1) >> 1;
                            var parentVal = _data[parent];
                            if (StopBubbleUp(parentVal.Obj, val))
                            {
                                break;
                            }
                            _data[current] = new PositionWrapped(parentVal.Obj, current);
                            wrapper.Position = parent;
                            _data[parent] = wrapper;

                            current = parent;
                        }
                        if (current == wrapper.Position) Heapify(current);
                    }
                }
            }
        }

        protected void Insert(T newVal)
        {
            int current;
            lock (_syncRoot)
            {
                try
                {
                }
                finally
                {
                    var wrapper = new PositionWrapped(newVal, _data.Count);
                    _positionLookUp.Add(newVal, wrapper);
                    _data.Add(wrapper);
                    current = ++_highIndex;
                    while (current > 0)
                    {
                        var parent = (current - 1) >> 1;
                        var parentVal = _data[parent];
                        if (StopBubbleUp(parentVal.Obj, newVal))
                        {
                            break;
                        }
                        _data[current] = new PositionWrapped(parentVal.Obj, current);
                        wrapper.Position = parent;
                        _data[parent] = wrapper;

                        current = parent;
                    }
                }
            }
            //Always outside of LOCK
            if (current == 0)
            {
                RootChanged(Id);
            }
        }

        //returns predicate status
        protected bool TryPeekOrExtract(Func<T, bool> predicate, bool extractIfPredicate, out T val)
        {
            lock (_syncRoot)
            {
                if (_highIndex < 0)
                {
                    val = default(T);
                    return false;
                }
                val = _data[0].Obj;
                if (!predicate(val)) return false;
                if (!extractIfPredicate) return true;
                _data[0] = new PositionWrapped(_data[_highIndex].Obj, 0);
                _positionLookUp.Remove(val);
                _data.RemoveAt(_highIndex--);
                Heapify(0);
            }
            return true;
        }

        protected void HeapifyUnsafe(Func<T, bool> predicate)
        {
            lock (_syncRoot)
            {
                if (predicate(_data[0].Obj))
                {
                    Heapify(0);
                }
            }
        }

        //returns predicate status
        public bool Remove(Func<T, bool> predicate)
        {
            lock (_syncRoot)
            {
                if (_highIndex < 0) return false;
                var val = _data[0].Obj;
                if (!predicate(val)) return false;
                _data[0] = new PositionWrapped(_data[_highIndex].Obj, 0);
                _positionLookUp.Remove(val);
                _data.RemoveAt(_highIndex--);
                if (_highIndex > 0) Heapify(0);
            }
            return true;
        }

        //private void PostExtraction(T extractVal)
        //{
        //    _data[0] = new PositionWrapped(_data[_highIndex].Obj, 0);
        //    _positionLookUp.Remove(extractVal);
        //    _data.RemoveAt(_highIndex--);
        //    Heapify(0);
        //}

        private void Heapify(int startPos)
        {
            try
            {
            }
            finally
            {
                var current = startPos;
                while (true)
                {
                    var l = (current << 1) + 1;
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
                    _data[startPos] = new PositionWrapped(_data[current].Obj, startPos);
                    _data[current] = new PositionWrapped(tmp.Obj, current);
                    startPos = current;
                }
            }
        }

        private int GivenCompareToRoot(T val)
        {
            lock (_syncRoot)
            {
                //we need to compare "GIVEN" value to our value!!!
                return _highIndex < 0 ? -_emptyDefault : val.CompareTo(_data[0].Obj);
            }
        }

        protected abstract bool StopBubbleUp(T parent, T child);
        protected abstract bool MustBubbleUp(T child, T parent);

        public int CompareTo(AbsHeap<T> other)
        {
            if (ReferenceEquals(other, null)) return 1;
            if (ReferenceEquals(this, other)) return 0;
            T myVal;
            lock (_syncRoot)
            {
                if (_highIndex < 0)
                {
                    return _emptyDefault;
                }
                myVal = _data[0].Obj;
            }
            //!!! ALWAYS OUTSIDE OF LOCK... else DEADLOCK can happen
            //other will take the lock inside this call

            //It is OK, if during this time, value changes again => Opportunity COST!
            //But we cannot avoid it... it's the best effort!
            return other.GivenCompareToRoot(myVal);
        }

        private struct PositionWrapped
        {
            public readonly T Obj;
            public int Position;

            public PositionWrapped(T obj, int pos)
            {
                Obj = obj;
                Position = pos;
            }
        }

        private class AbsHeapEquality : IEqualityComparer<AbsHeap<T>>
        {
            public bool Equals(AbsHeap<T> x, AbsHeap<T> y)
            {
                return x.Id == y.Id;
            }

            public int GetHashCode(AbsHeap<T> obj)
            {
                return obj.Id;
            }
        }
    }

    public class MinChangeArgs : EventArgs
    {
        public int Id { get; }

        public MinChangeArgs(int id)
        {
            Id = id;
        }
    }
}