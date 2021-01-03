using RCNet.Extensions;
using System;
using System.Collections.Generic;

namespace RCNet.MathTools
{
    /// <summary>
    /// Implements an interval.
    /// </summary>
    [Serializable]
    public class Interval
    {
        //Static members
        /// <summary>
        /// An interval of range -1...1
        /// </summary>
        public static readonly Interval IntN1P1 = new Interval(-1d, 1d, false, true);
        /// <summary>
        /// An interval of range 0...1
        /// </summary>
        public static readonly Interval IntZP1 = new Interval(0d, 1d, false, true);
        /// <summary>
        /// An interval of range 0...positive infiniti
        /// </summary>
        public static readonly Interval IntZPI = new Interval(0d, double.PositiveInfinity.Bound(), false, true);
        /// <summary>
        /// An interval of range negative infiniti...positive infiniti
        /// </summary>
        public static readonly Interval IntNIPI = new Interval(double.NegativeInfinity.Bound(), double.PositiveInfinity.Bound(), false, true);

        //Constants
        //Enumerations
        /// <summary>
        /// Interval type.
        /// </summary>
        public enum IntervalType
        {
            /// <summary>
            /// Left closed - Right closed.
            /// </summary>
            LeftClosedRightClosed,
            /// <summary>
            /// Left closed - Right open.
            /// </summary>
            LeftClosedRightOpen,
            /// <summary>
            /// Left open - Right closed.
            /// </summary>
            LeftOpenRightClosed,
            /// <summary>
            /// Left open - Right open.
            /// </summary>
            LeftOpenRightOpen
        }//IntervalType

        //Attribute properties
        /// <summary>
        /// Indicates whether the content can be modified or is unmodifiable.
        /// </summary>
        public bool Unmodifiable { get; private set; }

        //Attributes
        //Locking
        private readonly Object _lock = new Object();
        private readonly bool _threadSafe;
        //The borders
        private double _min;
        private double _max;
        private bool _initialized;

        //Constructor
        /// <summary>
        /// Creates an uninitialized instance.
        /// </summary>
        /// <param name="threadSafe">Specifies whether to create a thread safe instance.</param>
        public Interval(bool threadSafe = false)
        {
            Unmodifiable = false;
            _threadSafe = threadSafe;
            Reset();
            return;
        }

        /// <summary>
        /// Creates an initialized instance.
        /// </summary>
        /// <param name="min">The left boder value.</param>
        /// <param name="max">The right boder value.</param>
        /// <param name="threadSafe">Specifies whether to create a thread safe instance.</param>
        /// <param name="unmodifiable">Indicates whether the content can be modified or is unmodifiable.</param>
        public Interval(double min, double max, bool threadSafe = false, bool unmodifiable = false)
            : this(threadSafe)
        {
            Set(min, max);
            Unmodifiable = unmodifiable;
            return;
        }

        /// <summary>
        /// Creates an initialized instance.
        /// </summary>
        /// <param name="sampleCollection">The samples from which are determined interval's min and max border values.</param>
        /// <param name="threadSafe">Specifies whether to create a thread safe instance.</param>
        /// <param name="unmodifiable">Indicates whether the content can be modified or is unmodifiable.</param>
        public Interval(IEnumerable<double> sampleCollection, bool threadSafe = false, bool unmodifiable = false)
            : this(threadSafe)
        {
            foreach (double sample in sampleCollection)
            {
                Adjust(sample);
            }
            Unmodifiable = unmodifiable;
            return;
        }

        /// <summary>
        /// The copy constructor.
        /// </summary>
        /// <param name="source">The source instance.</param>
        /// <param name="threadSafe">Specifies whether to create a thread safe instance.</param>
        public Interval(Interval source, bool threadSafe = false)
            : this(threadSafe)
        {
            CopyFrom(source);
            return;
        }

        //Properties
        /// <summary>
        /// Gets the left border value of the interval.
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
        /// Gets the right border value of the interval.
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
        /// Gets the middle value of the interval (min + (max-min)/2).
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
        /// Gets the span of the Min and Max (Max - Min).
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
        /// Checks the content can be modified.
        /// </summary>
        private void CheckModifiable()
        {
            if (Unmodifiable)
            {
                throw new InvalidOperationException("Content can not be modified.");
            }
            return;
        }

        /// <summary>
        /// Sets the content unmodifiable. This is an irreversible operation.
        /// </summary>
        public void SetUnmodifiable()
        {
            if (_threadSafe)
            {
                lock (_lock)
                {
                    Unmodifiable = true;
                }
            }
            else
            {
                Unmodifiable = false;
            }
            return;
        }

        /// <inheritdoc/>
        public override bool Equals(object obj)
        {
            if (obj == null) return false;
            Interval cmpInterval = obj as Interval;
            lock (_lock)
            {
                return (_min == cmpInterval._min && _max == cmpInterval._max);
            }
        }

        /// <inheritdoc/>
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
        /// Resets the interval.
        /// </summary>
        public void Reset()
        {
            CheckModifiable();
            if (_threadSafe)
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
            _min = 0d;
            _max = 0d;
            _initialized = false;
            return;
        }

        /// <summary>
        /// Creates a deep copy of this instance.
        /// </summary>
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
        /// Copies all data from the source instance.
        /// </summary>
        /// <param name="source">The source instance.</param>
        public void CopyFrom(Interval source)
        {
            CheckModifiable();
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
            Unmodifiable = source.Unmodifiable;
            return;
        }

        /// <summary>
        /// Sets the border values (min and max).
        /// </summary>
        /// <remarks>
        /// It is not necessary to care about an order, function evaluates what is the Min and Max by itself.
        /// </remarks>
        /// <param name="border1">The border value.</param>
        /// <param name="border2">The border value.</param>
        public void Set(double border1, double border2)
        {
            CheckModifiable();
            if (_threadSafe)
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
        /// Adjusts the min and max border values.
        /// </summary>
        /// <param name="sample">The sample.</param>
        public void Adjust(double sample)
        {
            CheckModifiable();
            if (_threadSafe)
            {
                lock (_lock)
                {
                    AdjustInternal(sample);
                }
            }
            else
            {
                AdjustInternal(sample);
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
        /// Adjusts the min and max borders.
        /// </summary>
        /// <param name="sampleCollection">The samples.</param>
        public void Adjust(IEnumerable<double> sampleCollection)
        {
            CheckModifiable();
            foreach (double sample in sampleCollection)
            {
                Adjust(sample);
            }
            return;
        }

        /// <summary>
        /// Adjusts the min and max borders.
        /// </summary>
        /// <param name="sampleCollection">The samples.</param>
        public void Adjust(IEnumerable<long> sampleCollection)
        {
            CheckModifiable();
            foreach (long sample in sampleCollection)
            {
                Adjust(sample);
            }
            return;
        }

        /// <summary>
        /// Adjusts the min and max borders.
        /// </summary>
        /// <param name="sampleCollection">The samples.</param>
        public void Adjust(IEnumerable<ulong> sampleCollection)
        {
            CheckModifiable();
            foreach (ulong sample in sampleCollection)
            {
                Adjust(sample);
            }
            return;
        }

        /// <summary>
        /// Adjusts the min and max borders.
        /// </summary>
        /// <param name="sampleCollection">The samples.</param>
        public void Adjust(IEnumerable<int> sampleCollection)
        {
            CheckModifiable();
            foreach (int sample in sampleCollection)
            {
                Adjust(sample);
            }
            return;
        }

        /// <summary>
        /// Adjusts the min and max borders.
        /// </summary>
        /// <param name="sampleCollection">The samples.</param>
        public void Adjust(IEnumerable<uint> sampleCollection)
        {
            CheckModifiable();
            foreach (uint sample in sampleCollection)
            {
                Adjust(sample);
            }
            return;
        }

        /// <summary>
        /// Adjusts the min and max borders.
        /// </summary>
        /// <param name="sampleCollection">The samples.</param>
        public void Adjust(IEnumerable<sbyte> sampleCollection)
        {
            CheckModifiable();
            foreach (sbyte sample in sampleCollection)
            {
                Adjust(sample);
            }
            return;
        }

        /// <summary>
        /// Adjusts the min and max borders.
        /// </summary>
        /// <param name="sampleCollection">The samples.</param>
        public void Adjust(IEnumerable<byte> sampleCollection)
        {
            CheckModifiable();
            foreach (byte sample in sampleCollection)
            {
                Adjust(sample);
            }
            return;
        }

        /// <summary>
        /// Evaluates whether the value belongs to interval.
        /// </summary>
        /// <param name="value">Value to be tested</param>
        /// <param name="intervalType">The type of interval to be considered.</param>
        public bool BelongsTo(double value, IntervalType intervalType = IntervalType.LeftClosedRightClosed)
        {
            if (_threadSafe)
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
        /// Rescales the value from another interval to appropriate value from this interval.
        /// </summary>
        /// <param name="value">The value to be rescled.</param>
        /// <param name="valueRange">Value's origin range.</param>
        public double Rescale(double value, Interval valueRange)
        {
            if (_threadSafe)
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

