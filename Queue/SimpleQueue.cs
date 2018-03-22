using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace OKOSW.Queue
{
    /// <summary>
    /// Implements simple thread safe FIFO queue template
    /// </summary>
    public class SimpleQueue<T>
    {
        //Constants
        //Attributes
        private Object _lock;
        private int _capacity;
        private T[] _queue;
        private int _enqueueOffset;
        private int _dequeueOffset;
        private int _count;

        //Constructor
        public SimpleQueue(int capacity)
        {
            _lock = new Object();
            _capacity = capacity;
            _queue = new T[_capacity];
            _enqueueOffset = 0;
            _dequeueOffset = 0;
            _count = 0;
            return;
        }

        //Properties
        /// <summary>
        /// Count of enqueued items
        /// </summary>
        public int Count
        {
            get
            {
                lock(_lock)
                {
                    return _count;
                }
            }
        }

        /// <summary>
        /// Is queue full?
        /// </summary>
        public bool Full
        {
            get
            {
                lock (_lock)
                {
                    return (_count == _capacity);
                }
            }
        }

        //Methods
        /// <summary>
        /// Adds new item into the queue
        /// </summary>
        /// <param name="item">Item to be added</param>
        /// <returns>False if queue is full, True if success</returns>
        public bool Enqueue(T item)
        {
            lock(_lock)
            {
                if (_count < _capacity)
                {
                    _queue[_enqueueOffset] = item;
                    ++_enqueueOffset;
                    if (_enqueueOffset == _capacity)
                    {
                        _enqueueOffset = 0;
                    }
                    ++_count;
                    return true;
                }
            }
            return false;
        }

        /// <summary>
        /// Dequeues item (FIFO order)
        /// </summary>
        /// <returns>Default(T) if queue is empty, Item if success.</returns>
        public T Dequeue()
        {
            lock(_lock)
            {
                if (_count > 0)
                {
                    T item = _queue[_dequeueOffset];
                    ++_dequeueOffset;
                    if (_dequeueOffset == _capacity)
                    {
                        _dequeueOffset = 0;
                    }
                    --_count;
                    return item;
                }
            }
            return default(T);
        }
    }//SimpleQueue
}//Namespace
