using System;
using System.Collections.Generic;
using System.Collections;

namespace VolumeManager
{
    public class C_RingBuffer<T> : ICollection<T>, IEnumerable<T>, IEnumerable
    {
        private int _Capacity = 0;
        private int _Count = 0;
        private int _Position = 0;
        private T[] _Buffer;
        private DateTime _LastChange;
        private bool _Relative = false;

        public C_RingBuffer(int capacity)
        {
            _Capacity = capacity;
            _Buffer = new T[_Capacity];
            _LastChange = DateTime.Now;
        }

        public int Capacity
        {
            get
            {
                return _Capacity;
            }
        }

        public int Count
        {
            get
            {
                return _Count;
            }
        }

        public bool IsReadOnly
        {
            get
            {
                return false;
            }
        }

        public bool Remove(T item)
        {
            throw new NotImplementedException();
        }

        public void CopyTo(T[] array, int arrayIndex)
        {
            throw new NotImplementedException();
        }

        public void Clear()
        {
            _Buffer = new T[Capacity];
            _Count = 0;
            _LastChange = DateTime.Now;
        }

        public void Add(T item)
        {
            _Buffer[_Position] = item;

            _Position++;
            if (_Count < _Capacity)
                _Count++;

            if (_Position >= _Capacity)
                _Position = 0;

            _LastChange = DateTime.Now;
        }

        public T this[int key]
        {
            get
            {
                if ((key >= 0) && (key <= _Capacity))
                    if (!_Relative)
                        return _Buffer[key];
                    else
                    {
                        if (_Count == _Capacity)
                        {
                            key += _Position;

                            if (key >= _Capacity)
                                key -= _Capacity;
                        }

                        return _Buffer[key];
                    }
                else
                    throw new KeyNotFoundException();
            }
            set
            {
                if ((key >= 0) && (key <= _Capacity))
                    if (!_Relative)
                        _Buffer[key] = value;
                    else
                    {
                        if (_Count == _Capacity)
                        {
                            key += _Position;

                            if (key >= _Capacity)
                                key -= _Capacity;
                        }

                        _Buffer[key] = value;
                    }
                else
                    throw new KeyNotFoundException();
            }
        }

        public bool Contains(T item)
        {
            return IndexOf(item) != -1;
        }

        public int IndexOf(T item)
        {
            for (var _i_ = 0; _i_ < Count; _i_++)
            {
                var _item2_ = _Buffer[_i_];

                if ((item == null) && (_Buffer[_i_] == null))
                    return _i_;

                if ((item != null) && (item.Equals(_item2_)))
                    return _i_;
            }

            return -1;
        }

        public IEnumerator<T> GetEnumerator()
        {
            var _SaveLastChange_ = _LastChange;
            for (var _i_ = 0; _i_ < Count; _i_++)
            {
                if (_SaveLastChange_ != _LastChange)
                    throw new InvalidOperationException("Collection changed");

                yield return this[_i_];
            }
        }

        IEnumerator IEnumerable.GetEnumerator()
        {
            return GetEnumerator();
        }

        public bool RelativeIndex
        {
            get
            {
                return _Relative;
            }
            set
            {
                _Relative = value;
            }
        }
    }
}
