using System;
using System.Runtime.CompilerServices;

namespace RCNet.Queue
{
    /// <summary>
    /// Implements a simple FIFO queue template
    /// </summary>
    [Serializable]
    public class SimpleQueue<T>
    {
        //Constants
        //Attributes
        private readonly int _capacity;
        private readonly T[] _queue;
        private int _enqueueOffset;
        private int _dequeueOffset;

        //Constructor
        /// <summary>
        /// Instantiate a simple queue
        /// </summary>
        /// <param name="capacity">Maximum capacity of the queue</param>
        public SimpleQueue(int capacity)
        {
            _capacity = capacity;
            _queue = new T[_capacity];
            Reset();
            return;
        }

        //Properties
        /// <summary>
        /// Number of enqueued items
        /// </summary>
        public int Count { get; private set; }

        /// <summary>
        /// Is the queue full?
        /// </summary>
        public bool Full { get { return (Count == _capacity); } }

        //Methods
        /// <summary>
        /// Adds a new item into the queue
        /// </summary>
        /// <param name="item">Item to be added</param>
        /// <returns>False if queue is full, True if success</returns>
        public bool Enqueue(T item)
        {
            if (Count < _capacity)
            {
                _queue[_enqueueOffset] = item;
                ++_enqueueOffset;
                if (_enqueueOffset == _capacity)
                {
                    _enqueueOffset = 0;
                }
                ++Count;
                return true;
            }
            return false;
        }

        /// <summary>
        /// Dequeues the item (FIFO order)
        /// </summary>
        /// <returns>The item if success or the Default(T) if queue is empty.</returns>
        public T Dequeue()
        {
            if (Count > 0)
            {
                T item = _queue[_dequeueOffset];
                ++_dequeueOffset;
                if (_dequeueOffset == _capacity)
                {
                    _dequeueOffset = 0;
                }
                --Count;
                return item;
            }
            return default(T);
        }

        /// <summary>
        /// Resets queue to its initial state
        /// </summary>
        public void Reset()
        {
            _enqueueOffset = 0;
            _dequeueOffset = 0;
            Count = 0;
            return;
        }

    }//SimpleQueue

}//Namespace

