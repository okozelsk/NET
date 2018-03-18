using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace OKOSW.MathTools
{
    /// <summary>
    /// Implements simple thread safe interval.
    /// </summary>
    [Serializable]
    public class Interval
    {
        //Constants
        public const int TYPE_MIN_CLOSED_MAX_CLOSED = 0;
        public const int TYPE_MIN_CLOSED_MAX_OPEN = 1;
        public const int TYPE_MIN_OPEN_MAX_CLOSED = 2;
        public const int TYPE_MIN_OPEN_MAX_OPEN = 3;

        //Attributes
        //Locker to ensure thread safe behaviour
        private Object m_lock = new Object();
        //Initialization indicator
        private bool m_initialized;
        //Interval borders
        private double m_min;
        private double m_max;

        //Constructor
        public Interval()
        {
            Reset();
            return;
        }

        public Interval(double min, double max)
        {
            Set(min, max);
            return;
        }

        public Interval (IEnumerable<double> values)
        {
            Reset();
            foreach (double value in values)
            {
                Adjust(value);
            }
            return;
        }

        public Interval(Interval sourceInterval)
        {
            Set(sourceInterval);
            return;
        }

        //Properties
        public bool Initialized
        {
            get
            {
                lock (m_lock)
                {
                    return m_initialized;
                }
            }
        }

        public double Min
        {
            get
            {
                lock (m_lock)
                {
                    return m_min;
                }
            }
        }

        public double Max
        {
            get
            {
                lock (m_lock)
                {
                    return m_max;
                }
            }
        }

        public double Mid
        {
            get
            {
                lock (m_lock)
                {
                    return m_min + ((m_max - m_min) / 2d);
                }
            }
        }

        public double Span
        {
            get
            {
                lock (m_lock)
                {
                    return m_max - m_min;
                }
            }
        }

        //Methods
        public void Reset()
        {
            lock(m_lock)
            {
                m_initialized = false;
                m_min = 0;
                m_max = 0;
            }
            return;
        }

        public Interval Clone()
        {
            lock (m_lock)
            {
                Interval clone = new Interval(this);
                return clone;
            }
        }

        public void Set(double border1, double border2)
        {
            lock (m_lock)
            {
                m_min = Math.Min(border1, border2);
                m_max = Math.Max(border1, border2);
                m_initialized = true;
            }
            return;
        }

        public void Set(Interval sourceInterval)
        {
            lock (m_lock)
            {
                m_min = sourceInterval.m_min;
                m_max = sourceInterval.m_max;
                m_initialized = sourceInterval.m_initialized;
            }
            return;
        }

        public void Adjust(double value)
        {
            lock (m_lock)
            {
                if (!m_initialized)
                {
                    m_min = value;
                    m_max = value;
                    m_initialized = true;
                }
                else
                {
                    if (value < m_min)
                    {
                        m_min = value;
                    }
                    else if (value > m_max)
                    {
                        m_max = value;
                    }
                }
            }
            return;
        }

        public void Adjust(IEnumerable<double> values)
        {
            foreach(double value in values)
            {
                Adjust(value);
            }
            return;
        }

        public void Adjust(IEnumerable<long> values)
        {
            foreach (long value in values)
            {
                Adjust(value);
            }
            return;
        }

        public void Adjust(IEnumerable<ulong> values)
        {
            foreach (ulong value in values)
            {
                Adjust(value);
            }
            return;
        }

        public void Adjust(IEnumerable<int> values)
        {
            foreach (int value in values)
            {
                Adjust(value);
            }
            return;
        }

        public void Adjust(IEnumerable<uint> values)
        {
            foreach (uint value in values)
            {
                Adjust(value);
            }
            return;
        }

        public void Adjust(IEnumerable<sbyte> values)
        {
            foreach (sbyte value in values)
            {
                Adjust(value);
            }
            return;
        }

        public void Adjust(IEnumerable<byte> values)
        {
            foreach (byte value in values)
            {
                Adjust(value);
            }
            return;
        }

        public void Adjust(IEnumerable<Interval> values)
        {
            foreach (Interval item in values)
            {
                Adjust(item.Min);
                Adjust(item.Max);
            }
            return;
        }

        public bool Belongs(double value, int intervalType = TYPE_MIN_CLOSED_MAX_CLOSED)
        {
            lock (m_lock)
            {
                switch (intervalType)
                {
                    case TYPE_MIN_CLOSED_MAX_CLOSED:
                        return (value >= m_min && value <= m_max);
                    case TYPE_MIN_CLOSED_MAX_OPEN:
                        return (value >= m_min && value < m_max);
                    case TYPE_MIN_OPEN_MAX_CLOSED:
                        return (value > m_min && value <= m_max);
                    case TYPE_MIN_OPEN_MAX_OPEN:
                        return (value > m_min && value < m_max);
                    default:
                        break;
                }
            }
            return false;
        }

    }//Interval
}//Namespace
