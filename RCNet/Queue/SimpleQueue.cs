using System;
using System.Runtime.CompilerServices;

namespace RCNet.Queue
{
    /// <summary>
    /// Implements a simple FIFO queue template.
    /// Supports access to enqueued elements so it can be also used as the "sliding window"
    /// </summary>
    [Serializable]
    public class SimpleQueue<T>
    {
        //Attribute properties
        /// <summary>
        /// Maximum capacity of the queue
        /// </summary>
        public int Capacity { get; private set; }

        /// <summary>
        /// Number of elements in the queue
        /// </summary>
        public int Count { get; private set; }

        //Attributes
        private T[] _queueBuffer;
        private int _enqueueIndex;
        private int _dequeueIndex;

        //Constructor
        /// <summary>
        /// Instantiate empty queue
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
        /// Indicates full queue
        /// </summary>
        public bool Full { get { return (Count == Capacity); } }

        //Methods
        /// <summary>
        /// Resets queue to its initial state
        /// </summary>
        public void Reset()
        {
            _enqueueIndex = 0;
            _dequeueIndex = 0;
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
        /// If element exists then it can be reused in immediately following Enqueue call.
        /// </summary>
        public T GetElementOnEnqueuePosition()
        {
            return _queueBuffer[_enqueueIndex];
        }

        /// <summary>
        /// Returns zero based index within the queue buffer related to logical position 0..(Count-1) following specified logical order.
        /// </summary>
        /// <param name="logicalPos">Logical position 0..(Count-1)</param>
        /// <param name="latestFirst">Specifies logical order (latest..oldest or vice versa)</param>
        /// <returns>Non-negative index</returns>
        private int GetElementIndex(int logicalPos, bool latestFirst = false)
        {
            if (logicalPos < 0 || logicalPos >= Count)
            {
                throw new IndexOutOfRangeException($"Specified logicalPos {logicalPos} is outside of the range 0..{Count - 1}");
            }
            else if (latestFirst)
            {
                int bufferOffset = (_enqueueIndex - logicalPos) - 1;
                if (bufferOffset < 0) bufferOffset += Count;
                return bufferOffset;
            }
            else
            {
                int bufferOffset = (_dequeueIndex + logicalPos);
                if (bufferOffset >= Count) bufferOffset -= Count;
                return bufferOffset;
            }
        }

        /// <summary>
        /// Returns enqueued element at the zero based logical position 0..(Count-1) respecting desired logical order of elements.
        /// </summary>
        /// <param name="logicalPos">Logical position 0..(Count-1)</param>
        /// <param name="latestFirst">Specifies logical order (latest..oldest or oldest..latest)</param>
        /// <returns>Element at the logical position</returns>
        public T GetElementAt(int logicalPos, bool latestFirst = false)
        {
            return _queueBuffer[GetElementIndex(logicalPos, latestFirst)];
        }

        /// <summary>
        /// Sets element at the zero based logical position 0..(Count-1) respecting desired logical order of elements.
        /// </summary>
        /// <param name="elem">Element to be set at specified logical position 0..(Count-1)</param>
        /// <param name="logicalPos">Logical position 0..(Count-1)</param>
        /// <param name="latestFirst">Specifies logical order (latest..oldest or oldest..latest)</param>
        /// <returns>Replaced element at the logical position</returns>
        public T SetElementAt(T elem, int logicalPos, bool latestFirst = false)
        {
            int index = GetElementIndex(logicalPos, latestFirst);
            T orgItem = _queueBuffer[index];
            _queueBuffer[index] = elem;
            return orgItem;
        }

        /// <summary>
        /// Adds element into the queue
        /// </summary>
        /// <param name="elem">Element to be added</param>
        /// <param name="autoDequeue">Specifies if to atomatically dequeue when queue is full</param>
        /// <returns>True if success, False if queue is full</returns>
        public bool Enqueue(T elem, bool autoDequeue = false)
        {
            //Atomatically dequeue when queue is full and parameter autoDequeue is true
            if (Count == Capacity && autoDequeue)
            {
                Dequeue();
            }
            //Enqueue when there is a free capacity
            if (Count < Capacity)
            {
                _queueBuffer[_enqueueIndex] = elem;
                ++_enqueueIndex;
                if (_enqueueIndex == Capacity)
                {
                    _enqueueIndex = 0;
                }
                ++Count;
                return true;
            }
            return false;
        }

        /// <summary>
        /// Picks up next element from the queue (FIFO order)
        /// </summary>
        /// <returns>Dequeued eklement</returns>
        public T Dequeue()
        {
            if (Count > 0)
            {
                T elem = _queueBuffer[_dequeueIndex];
                ++_dequeueIndex;
                if (_dequeueIndex == Capacity)
                {
                    _dequeueIndex = 0;
                }
                --Count;
                return elem;
            }
            else
            {
                throw new Exception("Queue is empty.");
            }
        }

        /// <summary>
        /// Returns shallow copy of this queue
        /// </summary>
        /// <returns></returns>
        public SimpleQueue<T> ShallowClone()
        {
            SimpleQueue<T> newQueue = new SimpleQueue<T>(Capacity);
            _queueBuffer.CopyTo(newQueue._queueBuffer, 0);
            newQueue._enqueueIndex = _enqueueIndex;
            newQueue._dequeueIndex = _dequeueIndex;
            return newQueue;
        }

    }//SimpleQueue

}//Namespace

