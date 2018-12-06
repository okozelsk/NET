using System;
using System.Collections.Generic;

namespace RCNet.MathTools
{
    /// <summary>
    /// Implements a simple thread safe (if required) class representing interval.
    /// </summary>
    [Serializable]
    public class Interval
    {
        //Constants
        /// <summary>
        /// Interval types
        /// </summary>
        public enum IntervalType
        {
            /// <summary>
            /// Left closed - Right closed
            /// </summary>
            LeftClosedRightClosed,
            /// <summary>
            /// Left closed - Right open
            /// </summary>
            LeftClosedRightOpen,
            /// <summary>
            /// Left open - Right closed
            /// </summary>
            LeftOpenRightClosed,
            /// <summary>
            /// Left open - Right open
            /// </summary>
            LeftOpenRightOpen
        }//IntervalType

        //Attributes
        //Locker to ensure thread safe behaviour
        private Object _lock = new Object();
        private readonly bool _threadSafe;
        //Initialization indicator
        private bool _initialized;
        //Interval borders
        private double _min;
        private double _max;

        //Constructor
        /// <summary>
        /// Construct an interval
        /// </summary>
        /// <param name="threadSafe">Specifies if to create thread safe instance</param>
        public Interval(bool threadSafe = false)
        {
            _threadSafe = threadSafe;
            Reset();
            return;
        }

        /// <summary>
        /// Construct an interval
        /// </summary>
        /// <param name="min">Left value</param>
        /// <param name="max">Right value</param>
        /// <param name="threadSafe">Specifies if to create thread safe instance</param>
        public Interval(double min, double max, bool threadSafe = false)
            :this(threadSafe)
        {
            Set(min, max);
            return;
        }

        /// <summary>
        /// Constructs an interval
        /// </summary>
        /// <param name="values">Collection of the values from which are determined interval's min and max borders</param>
        /// <param name="threadSafe">Specifies if to create thread safe instance</param>
        public Interval(IEnumerable<double> values, bool threadSafe = false)
            : this(threadSafe)
        {
            foreach (double value in values)
            {
                Adjust(value);
            }
            return;
        }

        /// <summary>
        /// Constructs an interval as a copy of source instance
        /// </summary>
        /// <param name="source">Source instance</param>
        /// <param name="threadSafe">Specifies if to create thread safe instance</param>
        public Interval(Interval source, bool threadSafe = false)
            : this(threadSafe)
        {
            CopyFrom(source);
            return;
        }

        //Properties
        /// <summary>
        /// Is this interval properly initialized and ready to use?
        /// </summary>
        public bool Initialized
        {
            get
            {
                if(_threadSafe)
                {
                    lock (_lock)
                    {
                        return _initialized;
                    }
                }
                else
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
                if (_threadSafe)
                {
                    lock (_lock)
                    {
                        return _min;
                    }
                }
                else
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
                if (_threadSafe)
                {
                    lock (_lock)
                    {
                        return _max;
                    }
                }
                else
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
                if (_threadSafe)
                {
                    lock (_lock)
                    {
                        return _min + ((_max - _min) / 2d);
                    }
                }
                else
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
                if (_threadSafe)
                {
                    lock (_lock)
                    {
                        return _max - _min;
                    }
                }
                else
                {
                    return _max - _min;
                }
            }
        }

        //Methods
        /// <summary>
        /// See the base.
        /// </summary>
        public override bool Equals(object obj)
        {
            if (obj == null) return false;
            Interval cmpInterval = obj as Interval;
            lock (_lock)
            {
                return (_min == cmpInterval._min && _max == cmpInterval._max);
            }
        }

        /// <summary>
        /// See the base.
        /// </summary>
        public override int GetHashCode()
        {
            unchecked
            {
                int hash = 3;
                hash = hash * 7 + _min.GetHashCode();
                hash = hash * 7 + _max.GetHashCode();
                return hash;
            }
        }

        /// <summary>
        /// Resets this interval to the uninitialized state
        /// </summary>
        public void Reset()
        {
            if(_threadSafe)
            {
                lock (_lock)
                {
                    ResetInternal();
                }
            }
            else
            {
                ResetInternal();
            }
            return;
        }

        private void ResetInternal()
        {
            _initialized = false;
            _min = 0;
            _max = 0;
            return;
        }

        /// <summary>
        /// Creates a deep copy of this interval
        /// </summary>
        /// <returns></returns>
        public Interval DeepClone()
        {
            if (_threadSafe)
            {
                lock (_lock)
                {
                    return new Interval(this);
                }
            }
            else
            {
                return new Interval(this);
            }
        }

        /// <summary>
        /// Copies all internal attribute values from the source instance into this instance
        /// </summary>
        /// <param name="source">Source instance</param>
        public void CopyFrom(Interval source)
        {
            if (_threadSafe)
            {
                lock (source._lock)
                {
                    CopyFromInternal(source);
                }
            }
            else
            {
                CopyFromInternal(source);
            }
            return;
        }

        private void CopyFromInternal(Interval source)
        {
            _min = source._min;
            _max = source._max;
            _initialized = source._initialized;
            return;
        }

        /// <summary>
        /// Sets the border values min and max.
        /// It is not necessary to care about border order, function evaluates the Min and Max by itself.
        /// </summary>
        /// <param name="border1">The first border value</param>
        /// <param name="border2">The second border value</param>
        public void Set(double border1, double border2)
        {
            if(_threadSafe)
            {
                lock (_lock)
                {
                    SetInternal(border1, border2);
                }
            }
            else
            {
                SetInternal(border1, border2);
            }
            return;
        }

        private void SetInternal(double border1, double border2)
        {
            _min = Math.Min(border1, border2);
            _max = Math.Max(border1, border2);
            _initialized = true;
            return;
        }

        /// <summary>
        /// Adjusts the min and max borders according to sample value.
        /// </summary>
        /// <param name="sampleValue">Sample value</param>
        public void Adjust(double sampleValue)
        {
            if(_threadSafe)
            {
                lock (_lock)
                {
                    AdjustInternal(sampleValue);
                }
            }
            else
            {
                AdjustInternal(sampleValue);
            }
            return;
        }

        private void AdjustInternal(double sampleValue)
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
            return;
        }

        /// <summary>
        /// Adjusts the min and max borders according to sample valus in a given collection.
        /// </summary>
        /// <param name="sampleValueCollection">Collection of sample values</param>
        public void Adjust(IEnumerable<double> sampleValueCollection)
        {
            foreach (double value in sampleValueCollection)
            {
                Adjust(value);
            }
            return;
        }

        /// <summary>
        /// Adjusts the min and max borders according to sample valus in a given collection.
        /// </summary>
        /// <param name="sampleValueCollection">Collection of sample values</param>
        public void Adjust(IEnumerable<long> sampleValueCollection)
        {
            foreach (long value in sampleValueCollection)
            {
                Adjust(value);
            }
            return;
        }

        /// <summary>
        /// Adjusts the min and max borders according to sample valus in a given collection.
        /// </summary>
        /// <param name="sampleValueCollection">Collection of sample values</param>
        public void Adjust(IEnumerable<ulong> sampleValueCollection)
        {
            foreach (ulong value in sampleValueCollection)
            {
                Adjust(value);
            }
            return;
        }

        /// <summary>
        /// Adjusts the min and max borders according to sample valus in a given collection.
        /// </summary>
        /// <param name="sampleValueCollection">Collection of sample values</param>
        public void Adjust(IEnumerable<int> sampleValueCollection)
        {
            foreach (int value in sampleValueCollection)
            {
                Adjust(value);
            }
            return;
        }

        /// <summary>
        /// Adjusts the min and max borders according to sample valus in a given collection.
        /// </summary>
        /// <param name="sampleValueCollection">Collection of sample values</param>
        public void Adjust(IEnumerable<uint> sampleValueCollection)
        {
            foreach (uint value in sampleValueCollection)
            {
                Adjust(value);
            }
            return;
        }

        /// <summary>
        /// Adjusts the min and max borders according to sample valus in a given collection.
        /// </summary>
        /// <param name="sampleValueCollection">Collection of sample values</param>
        public void Adjust(IEnumerable<sbyte> sampleValueCollection)
        {
            foreach (sbyte value in sampleValueCollection)
            {
                Adjust(value);
            }
            return;
        }

        /// <summary>
        /// Adjusts the min and max borders according to sample valus in a given collection.
        /// </summary>
        /// <param name="sampleValueCollection">Collection of sample values</param>
        public void Adjust(IEnumerable<byte> sampleValueCollection)
        {
            foreach (byte value in sampleValueCollection)
            {
                Adjust(value);
            }
            return;
        }

        /// <summary>
        /// Tests if given value belongs to this interval
        /// </summary>
        /// <param name="value">Value to be tested</param>
        /// <param name="intervalType">Type of interval to be considered</param>
        public bool BelongsTo(double value, IntervalType intervalType = IntervalType.LeftClosedRightClosed)
        {
            if(_threadSafe)
            {
                lock (_lock)
                {
                    return BelongsToInternal(value, intervalType);
                }
            }
            else
            {
                return BelongsToInternal(value, intervalType);
            }
        }

        private bool BelongsToInternal(double value, IntervalType intervalType = IntervalType.LeftClosedRightClosed)
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
            return false;
        }

        /// <summary>
        /// Rescales given value to this interval
        /// </summary>
        /// <param name="value">Input value</param>
        /// <param name="valueRange">Input value range</param>
        public double Rescale(double value, Interval valueRange)
        {
            if(_threadSafe)
            {
                lock (_lock)
                {
                    return _min + (((value - valueRange._min) / valueRange.Span) * Span);
                }
            }
            else
            {
                return _min + (((value - valueRange._min) / valueRange.Span) * Span);
            }
        }


    }//Interval

}//Namespace

