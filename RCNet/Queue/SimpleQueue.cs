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
        //Attribute properties
        /// <summary>
        /// Number of enqueued items
        /// </summary>
        public int Capacity { get; private set; }

        /// <summary>
        /// Number of enqueued items
        /// </summary>
        public int Count { get; private set; }

        //Attributes
        private T[] _queueBuffer;
        private int _enqueueOffset;
        private int _dequeueOffset;

        //Constructor
        /// <summary>
        /// Instantiate a simple queue
        /// </summary>
        /// <param name="capacity">Maximum capacity of the queue</param>
        public SimpleQueue(int capacity)
        {
            Capacity = capacity;
            _queueBuffer = new T[Capacity];
            Reset();
            return;
        }

        //Properties
        /// <summary>
        /// Is the queue full?
        /// </summary>
        public bool Full { get { return (Count == Capacity); } }

        //Methods
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

        /// <summary>
        /// Resets queue and changes the queue capacity.
        /// </summary>
        /// <param name="newCapacity">New capacity of the queue</param>
        /// <param name="forceShrink">Determines whether to reallocate queue buffer even if new capacity is smaller than queue buffer size</param>
        public void Resize(int newCapacity, bool forceShrink = false)
        {
            Reset();
            if(forceShrink || newCapacity > _queueBuffer.Length)
            {
                _queueBuffer = new T[newCapacity];
            }
            Capacity = newCapacity;
            return;
        }

        /// <summary>
        /// Returns element from queue buffer on the next enqueue position.
        /// If element exists then can be resused in immediately following Enqueue call.
        /// </summary>
        public T GetElementOnEnqueuePosition()
        {
            return _queueBuffer[_enqueueOffset];
        }

        /// <summary>
        /// Adds a new item into the queue
        /// </summary>
        /// <param name="item">Item to be added</param>
        /// <returns>False if queue is full, True if success</returns>
        public bool Enqueue(T item)
        {
            if (Count < Capacity)
            {
                _queueBuffer[_enqueueOffset] = item;
                ++_enqueueOffset;
                if (_enqueueOffset == Capacity)
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
                T item = _queueBuffer[_dequeueOffset];
                ++_dequeueOffset;
                if (_dequeueOffset == Capacity)
                {
                    _dequeueOffset = 0;
                }
                --Count;
                return item;
            }
            return default(T);
        }

    }//SimpleQueue

}//Namespace

