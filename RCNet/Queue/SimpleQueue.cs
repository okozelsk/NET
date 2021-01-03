using System;
using System.Runtime.CompilerServices;

namespace RCNet.Queue
{
    /// <summary>
    /// Implements a simple FIFO queue template.
    /// </summary>
    /// <remarks>
    /// Supports access to enqueued elements so it can be also used as the moving data window.
    /// </remarks>
    [Serializable]
    public class SimpleQueue<T>
    {
        //Attribute properties
        /// <summary>
        /// The maximum capacity of the queue.
        /// </summary>
        public int Capacity { get; private set; }

        /// <summary>
        /// The number of elements in the queue.
        /// </summary>
        public int Count { get; private set; }

        //Attributes
        private T[] _queueBuffer;
        private int _enqueueIndex;
        private int _dequeueIndex;

        //Constructor
        /// <summary>
        /// Creates an initialized instance.
        /// </summary>
        /// <param name="capacity">The maximum capacity of the queue.</param>
        public SimpleQueue(int capacity)
        {
            Capacity = capacity;
            _queueBuffer = new T[Capacity];
            Reset();
            return;
        }

        //Properties
        /// <summary>
        /// Indicates the queue is full.
        /// </summary>
        public bool Full { get { return (Count == Capacity); } }

        //Methods
        /// <summary>
        /// Throws the InvalidOperationException.
        /// </summary>
        /// <param name="text">The exception text.</param>
        private void ThrowInvalidOperationException(string text)
        {
            throw new InvalidOperationException(text);
        }

        /// <summary>
        /// Throws the IndexOutOfRangeException.
        /// </summary>
        /// <param name="text">The exception text.</param>
        private void ThrowIndexOutOfRangeException(string text)
        {
            throw new IndexOutOfRangeException(text);
        }

        /// <summary>
        /// Resets the queue to its initial state.
        /// </summary>
        public void Reset()
        {
            _enqueueIndex = 0;
            _dequeueIndex = 0;
            Count = 0;
            return;
        }

        /// <summary>
        /// Resets the queue and changes the queue capacity.
        /// </summary>
        /// <param name="newCapacity">The new maximum capacity of the queue.</param>
        /// <param name="forceShrink">Specifies whether to reallocate queue buffer even if the new capacity is smaller than the current buffer size.</param>
        public void Resize(int newCapacity, bool forceShrink = false)
        {
            Reset();
            if (forceShrink || newCapacity > _queueBuffer.Length)
            {
                _queueBuffer = new T[newCapacity];
            }
            Capacity = newCapacity;
            return;
        }

        /// <summary>
        /// Gets an element from queue buffer at the next enqueue position.
        /// </summary>
        /// <remarks>
        /// If the element exists then it can be reused in immediately following Enqueue call.
        /// </remarks>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public T GetElementAtEnqueuePosition()
        {
            return _queueBuffer[_enqueueIndex];
        }

        /// <summary>
        /// Gets the physical zero-based index within the queue buffer corresponding to a logical position 0..(Count-1) following the specified logical order.
        /// </summary>
        /// <param name="logicalPos">The logical position 0..(Count-1).</param>
        /// <param name="latestFirst">Specifies the logical order (latest..oldest or vice versa).</param>
        /// <returns>The physical zero-based index.</returns>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        private int GetElementIndex(int logicalPos, bool latestFirst = false)
        {
            if (logicalPos < 0 || logicalPos >= Count)
            {
                ThrowIndexOutOfRangeException($"Specified logicalPos {logicalPos} is outside of the range 0..{Count - 1}");
                //This will never happen
                return -1;
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
        /// Gets an enqueued element at the zero-based logical position 0..(Count-1) respecting desired logical order of elements.
        /// </summary>
        /// <param name="logicalPos">The logical position 0..(Count-1).</param>
        /// <param name="latestFirst">Specifies the logical order (latest..oldest or oldest..latest).</param>
        /// <returns>An element at the logical position.</returns>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public T GetElementAt(int logicalPos, bool latestFirst = false)
        {
            return _queueBuffer[GetElementIndex(logicalPos, latestFirst)];
        }

        /// <summary>
        /// Sets an element at the zero-based logical position 0..(Count-1) respecting desired logical order of elements.
        /// </summary>
        /// <param name="elem">An element to be set.</param>
        /// <param name="logicalPos">The logical position 0..(Count-1).</param>
        /// <param name="latestFirst">Specifies the logical order (latest..oldest or oldest..latest).</param>
        /// <returns>The replaced element at the logical position.</returns>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public T SetElementAt(T elem, int logicalPos, bool latestFirst = false)
        {
            int index = GetElementIndex(logicalPos, latestFirst);
            T orgItem = _queueBuffer[index];
            _queueBuffer[index] = elem;
            return orgItem;
        }

        /// <summary>
        /// Adds an element into the queue.
        /// </summary>
        /// <param name="elem">An element to be added.</param>
        /// <param name="autoDequeue">Specifies whether to atomatically dequeue when queue is full.</param>
        /// <returns>True if success, False if queue is full.</returns>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
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
        /// Dequeues an element from the queue (FIFO order).
        /// </summary>
        /// <returns>The dequeued element.</returns>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
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
                ThrowInvalidOperationException($"Queue is empty.");
                //This will never happen
                return default;
            }
        }

        /// <summary>
        /// Creates the shallow copy of this queue.
        /// </summary>
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

