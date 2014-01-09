using System;
using System.Collections.Generic;
using System.Text;

namespace Cecer.Utils
{
    internal class RollingCollection<T>
    {
        /// <summary>
        /// 
        /// </summary>
        private readonly T[] _values;
        private int _startIndex;

        internal int CurrentSize 
        {
            get;
            private set;
        }

        internal RollingCollection(int size)
        {
            _startIndex = 0;
            CurrentSize = 0;
            _values = new T[size];
        }

        internal T this[int index]
        {
            get
            {
                if (CurrentSize <= index)
                    return default(T);

                if (CurrentSize < _values.Length)
                    return _values[CurrentSize - index - 1];
                return _values[(_startIndex + index) % 100];
            }
        }

        internal void Add(T value)
        {
            if (CurrentSize < _values.Length)
            {
                _values[CurrentSize++] = value;
            }
            else
            {
                _values[_startIndex] = value;
                _startIndex = (_startIndex + 1) % 100;
            }
        }
    }

}