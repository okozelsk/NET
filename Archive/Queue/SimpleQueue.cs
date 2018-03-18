using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace OKOSW.Queue
{
    public class SimpleQueue<T>
    {
        //Constants
        //Attributes
        private Object m_lock;
        private int m_capacity;
        private T[] m_queue;
        private int m_enqueueOffset;
        private int m_dequeueOffset;
        private int m_count;
        private bool m_feedingStopped;

        //Constructor
        public SimpleQueue(int capacity)
        {
            m_lock = new Object();
            m_capacity = capacity;
            m_queue = new T[m_capacity];
            m_enqueueOffset = 0;
            m_dequeueOffset = 0;
            m_count = 0;
            m_feedingStopped = false;
            return;
        }

        //Properties
        public bool FeedingStopped
        {
            get
            {
                lock (m_lock)
                {
                    return m_feedingStopped;
                }
            }

            set
            {
                lock (m_lock)
                {
                    m_feedingStopped = value;
                }
            }
        }

        public int Count
        {
            get
            {
                lock(m_lock)
                {
                    return m_count;
                }
            }
        }

        public bool Full
        {
            get
            {
                lock (m_lock)
                {
                    return (m_count == m_capacity);
                }
            }
        }

        //Methods
        public bool Enqueue(T item)
        {
            lock(m_lock)
            {
                if(m_feedingStopped)
                {
                    throw new ApplicationException("Can´t engueue. Queue feeding was stopped.");
                }
                if (m_count < m_capacity)
                {
                    m_queue[m_enqueueOffset] = item;
                    ++m_enqueueOffset;
                    if (m_enqueueOffset == m_capacity)
                    {
                        m_enqueueOffset = 0;
                    }
                    ++m_count;
                    return true;
                }
            }
            return false;
        }

        public T Dequeue()
        {
            lock(m_lock)
            {
                if (m_count > 0)
                {
                    T item = m_queue[m_dequeueOffset];
                    ++m_dequeueOffset;
                    if (m_dequeueOffset == m_capacity)
                    {
                        m_dequeueOffset = 0;
                    }
                    --m_count;
                    return item;
                }
            }
            return default(T);
        }
    }//SimpleQueue
}//Namespace
