using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Collections.Concurrent;

namespace CogMon.Agent.PerfMon
{
    public class ConcurrentCircularBuffer<T>
    {
        private ConcurrentQueue<T> _q = new ConcurrentQueue<T>();
        private int _maxLen;

        public ConcurrentCircularBuffer(int maxSize)
        {
            _maxLen = maxSize;    
        }

        public ConcurrentCircularBuffer()
            : this(10000)
        {
        }

        public void Enqueue(T val)
        {
            _q.Enqueue(val);
            T t;
            while (_q.Count > _maxLen) _q.TryDequeue(out t);
        }

        public int Count
        {
            get { return _q.Count; }
        }

        public int Capacity
        {
            get { return _maxLen; }
        }

        T[] GetDataAndReset()
        {
            var tq = _q;
            _q = new ConcurrentQueue<T>();
            return tq.ToArray();
        }

        public T Dequeue()
        {
            T t;
            if (_q.TryDequeue(out t)) return t;
            throw new Exception("Empty");
        }
    }
}
