using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace OKOSW.MathTools
{
    /// <summary>
    /// Implements simple thread safe class representing interval.
    /// </summary>
    [Serializable]
    public class Interval
    {
        //Constants
        /// <summary>
        /// Supported interval types
        /// </summary>
        public enum IntervalType
        {
            LeftClosedRightClosed,
            LeftClosedRightOpen,
            LeftOpenRightClosed,
            LeftOpenRightOpen
        }

        //Attributes
        //Locker to ensure thread safe behaviour
        private Object _lock = new Object();
        //Initialization indicator
        private bool _initialized;
        //Interval borders
        private double _min;
        private double _max;

        //Constructor
        /// <summary>
        /// Construct an interval
        /// </summary>
        public Interval()
        {
            Reset();
            return;
        }

        /// <summary>
        /// Construct an interval
        /// </summary>
        /// <param name="min">Left value</param>
        /// <param name="max">Right value</param>
        public Interval(double min, double max)
        {
            Set(min, max);
            return;
        }

        /// <summary>
        /// Construct an interval
        /// </summary>
        /// <param name="values">Collection of values from which are determined interval's min and max borders</param>
        public Interval (IEnumerable<double> values)
        {
            Reset();
            foreach (double value in values)
            {
                Adjust(value);
            }
            return;
        }

        /// <summary>
        /// Construct an interval as a copy of source
        /// </summary>
        /// <param name="source">Source interval</param>
        public Interval(Interval source)
        {
            CopyFrom(source);
            return;
        }

        //Properties
        /// <summary>
        /// Is interval properly initialized and ready to use?
        /// </summary>
        public bool Initialized
        {
            get
            {
                lock (_lock)
                {
                    return _initialized;
                }
            }
        }

        /// <summary>
        /// Left border of the interval
        /// </summary>
        public double Min
        {
            get
            {
                lock (_lock)
                {
                    return _min;
                }
            }
        }

        /// <summary>
        /// Right border of the interval
        /// </summary>
        public double Max
        {
            get
            {
                lock (_lock)
                {
                    return _max;
                }
            }
        }

        /// <summary>
        /// Middle value of the interval (min + (max-min)/2)
        /// </summary>
        public double Mid
        {
            get
            {
                lock (_lock)
                {
                    return _min + ((_max - _min) / 2d);
                }
            }
        }

        /// <summary>
        /// Min and Max span (max-min)
        /// </summary>
        public double Span
        {
            get
            {
                lock (_lock)
                {
                    return _max - _min;
                }
            }
        }

        //Methods
        /// <summary>
        /// Resets interval to uninitialized state
        /// </summary>
        public void Reset()
        {
            lock(_lock)
            {
                _initialized = false;
                _min = 0;
                _max = 0;
            }
            return;
        }

        /// <summary>
        /// Creates shallow copy of this interval
        /// </summary>
        /// <returns></returns>
        public Interval Clone()
        {
            lock (_lock)
            {
                Interval clone = new Interval(this);
                return clone;
            }
        }

        /// <summary>
        /// Sets this internals as a copy of the source
        /// </summary>
        /// <param name="source"></param>
        public void CopyFrom(Interval source)
        {
            lock (_lock)
            {
                _min = source._min;
                _max = source._max;
                _initialized = source._initialized;
            }
            return;
        }

        /// <summary>
        /// Sets interval border values min and max. It is not necessary to care about border order, function evaluates Min and Max by itself.
        /// </summary>
        /// <param name="border1">First border value</param>
        /// <param name="border2">Second border value</param>
        public void Set(double border1, double border2)
        {
            lock (_lock)
            {
                _min = Math.Min(border1, border2);
                _max = Math.Max(border1, border2);
                _initialized = true;
            }
            return;
        }

        /// <summary>
        /// Adjusts min and max according to sample value.
        /// </summary>
        /// <param name="sampleValue">Sample value</param>
        public void Adjust(double sampleValue)
        {
            lock (_lock)
            {
                if (!_initialized)
                {
                    _min = sampleValue;
                    _max = sampleValue;
                    _initialized = true;
                }
                else
                {
                    if (sampleValue < _min)
                    {
                        _min = sampleValue;
                    }
                    else if (sampleValue > _max)
                    {
                        _max = sampleValue;
                    }
                }
            }
            return;
        }

        /// <summary>
        /// Adjusts min and max according to sample valus in given collection.
        /// </summary>
        /// <param name="sampleValuesCollection">Collection of sample values</param>
        public void Adjust(IEnumerable<double> sampleValuesCollection)
        {
            foreach(double value in sampleValuesCollection)
            {
                Adjust(value);
            }
            return;
        }

        /// <summary>
        /// Adjusts min and max according to sample valus in given collection.
        /// </summary>
        /// <param name="sampleValuesCollection">Collection of sample values</param>
        public void Adjust(IEnumerable<long> sampleValuesCollection)
        {
            foreach (long value in sampleValuesCollection)
            {
                Adjust(value);
            }
            return;
        }

        /// <summary>
        /// Adjusts min and max according to sample valus in given collection.
        /// </summary>
        /// <param name="sampleValuesCollection">Collection of sample values</param>
        public void Adjust(IEnumerable<ulong> sampleValuesCollection)
        {
            foreach (ulong value in sampleValuesCollection)
            {
                Adjust(value);
            }
            return;
        }

        /// <summary>
        /// Adjusts min and max according to sample valus in given collection.
        /// </summary>
        /// <param name="sampleValuesCollection">Collection of sample values</param>
        public void Adjust(IEnumerable<int> sampleValuesCollection)
        {
            foreach (int value in sampleValuesCollection)
            {
                Adjust(value);
            }
            return;
        }

        /// <summary>
        /// Adjusts min and max according to sample valus in given collection.
        /// </summary>
        /// <param name="sampleValuesCollection">Collection of sample values</param>
        public void Adjust(IEnumerable<uint> sampleValuesCollection)
        {
            foreach (uint value in sampleValuesCollection)
            {
                Adjust(value);
            }
            return;
        }

        /// <summary>
        /// Adjusts min and max according to sample valus in given collection.
        /// </summary>
        /// <param name="sampleValuesCollection">Collection of sample values</param>
        public void Adjust(IEnumerable<sbyte> sampleValuesCollection)
        {
            foreach (sbyte value in sampleValuesCollection)
            {
                Adjust(value);
            }
            return;
        }

        /// <summary>
        /// Adjusts min and max according to sample valus in given collection.
        /// </summary>
        /// <param name="sampleValuesCollection">Collection of sample values</param>
        public void Adjust(IEnumerable<byte> sampleValuesCollection)
        {
            foreach (byte value in sampleValuesCollection)
            {
                Adjust(value);
            }
            return;
        }

        /// <summary>
        /// Adjusts min and max to represent an unification of given intervals
        /// </summary>
        /// <param name="intervalsCollection">Collection of intervals</param>
        public void Adjust(IEnumerable<Interval> intervalsCollection)
        {
            foreach (Interval item in intervalsCollection)
            {
                Adjust(item.Min);
                Adjust(item.Max);
            }
            return;
        }

        /// <summary>
        /// Tests if given value belongs to this interval
        /// </summary>
        /// <param name="value">Value to be tested</param>
        /// <param name="intervalType">Type of this interval to be considered</param>
        public bool BelongsTo(double value, IntervalType intervalType = IntervalType.LeftClosedRightClosed)
        {
            lock (_lock)
            {
                switch (intervalType)
                {
                    case IntervalType.LeftClosedRightClosed:
                        return (value >= _min && value <= _max);
                    case IntervalType.LeftClosedRightOpen:
                        return (value >= _min && value < _max);
                    case IntervalType.LeftOpenRightClosed:
                        return (value > _min && value <= _max);
                    case IntervalType.LeftOpenRightOpen:
                        return (value > _min && value < _max);
                    default:
                        break;
                }
            }
            return false;
        }

    }//Interval
}//Namespace
