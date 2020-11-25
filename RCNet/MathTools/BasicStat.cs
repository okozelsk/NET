using RCNet.Extensions;
using System;
using System.Collections.Generic;

namespace RCNet.MathTools
{
    /// <summary>
    /// Implements the simple and thread safe (if required) statistics.
    /// </summary>
    [Serializable]
    public class BasicStat
    {
        //Enums
        /// <summary>
        /// Available outputs
        /// </summary>
        public enum OutputFeature
        {
            /// <summary>
            /// Sum of values
            /// </summary>
            Sum,
            /// <summary>
            /// Sum of negative values
            /// </summary>
            NegSum,
            /// <summary>
            /// Sum of positive values
            /// </summary>
            PosSum,
            /// <summary>
            /// Sum of squared values
            /// </summary>
            SumOfSquares,
            /// <summary>
            /// Min value
            /// </summary>
            Min,
            /// <summary>
            /// Max value
            /// </summary>
            Max,
            /// <summary>
            /// The center value between min and max
            /// </summary>
            Mid,
            /// <summary>
            /// Span between min and max
            /// </summary>
            Span,
            /// <summary>
            /// Arithmetic average
            /// </summary>
            ArithAvg,
            /// <summary>
            /// Mean of the squared values
            /// </summary>
            MeanSquare,
            /// <summary>
            /// Root of the mean of the squared values
            /// </summary>
            RootMeanSquare,
            /// <summary>
            /// The variance of the values
            /// </summary>
            Variance,
            /// <summary>
            /// The standard deviation of the values
            /// </summary>
            StdDev,
            /// <summary>
            /// The span multiplicated by standard deviation of the values
            /// </summary>
            SpanDev
        }

        //Attributes
        //Locker to ensure thread safe behaviour
        private readonly Object _lock;
        private readonly bool _threadSafe;
        //Base values
        private double _sum;
        private double _negSum;
        private double _posSum;
        private double _sumOfSquares;
        private double _min;
        private double _max;
        private int _numOfSamples;
        private int _numOfNonzeroSamples;
        //Precomputed properties
        private bool _recompute;
        private double _arithAvg;
        private double _meanSquare;
        private double _rootMeanSquare;
        private double _variance;
        private double _stdDev;
        private double _spanDev;

        //Constructors
        /// <summary>
        /// Creates an uninitialized instance
        /// </summary>
        /// <param name="threadSafe">Specifies whether to create thread safe instance</param>
        public BasicStat(bool threadSafe = false)
        {
            _threadSafe = threadSafe;
            if (_threadSafe)
            {
                _lock = new Object();
            }
            else
            {
                _lock = null;
            }
            Reset();
            return;
        }

        /// <summary>
        /// Creates an instance and loads given sample values
        /// </summary>
        /// <param name="sampleValueCollection">Values to be pushed as the samples</param>
        /// <param name="threadSafe">Specifies whether to create thread safe instance</param>
        public BasicStat(IEnumerable<double> sampleValueCollection, bool threadSafe = false)
            : this(threadSafe)
        {
            SetSampleValues(sampleValueCollection);
            return;
        }

        /// <summary>
        /// Creates an instance and loads given sample values
        /// </summary>
        /// <param name="sampleValueCollection">Values to be pushed as the samples</param>
        /// <param name="threadSafe">Specifies whether to create thread safe instance</param>
        public BasicStat(IEnumerable<long> sampleValueCollection, bool threadSafe = false)
            : this(threadSafe)
        {
            SetSampleValues(sampleValueCollection);
            return;
        }

        /// <summary>
        /// Creates an instance and loads given sample values
        /// </summary>
        /// <param name="sampleValueCollection">Values to be pushed as the samples</param>
        /// <param name="threadSafe">Specifies whether to create thread safe instance</param>
        public BasicStat(IEnumerable<ulong> sampleValueCollection, bool threadSafe = false)
            : this(threadSafe)
        {
            SetSampleValues(sampleValueCollection);
            return;
        }

        /// <summary>
        /// Creates an instance and loads given sample values
        /// </summary>
        /// <param name="sampleValueCollection">Values to be pushed as the samples</param>
        /// <param name="threadSafe">Specifies whether to create thread safe instance</param>
        public BasicStat(IEnumerable<int> sampleValueCollection, bool threadSafe = false)
            : this(threadSafe)
        {
            SetSampleValues(sampleValueCollection);
            return;
        }

        /// <summary>
        /// Creates an instance and loads given sample values
        /// </summary>
        /// <param name="sampleValueCollection">Values to be pushed as the samples</param>
        /// <param name="threadSafe">Specifies whether to create thread safe instance</param>
        public BasicStat(IEnumerable<uint> sampleValueCollection, bool threadSafe = false)
            : this(threadSafe)
        {
            SetSampleValues(sampleValueCollection);
            return;
        }

        /// <summary>
        /// Creates an instance and loads given sample values
        /// </summary>
        /// <param name="sampleValueCollection">Values to be pushed as the samples</param>
        /// <param name="threadSafe">Specifies whether to create thread safe instance</param>
        public BasicStat(IEnumerable<byte> sampleValueCollection, bool threadSafe = false)
            : this(threadSafe)
        {
            SetSampleValues(sampleValueCollection);
            return;
        }

        /// <summary>
        /// The copy constructor
        /// </summary>
        /// <param name="source">Source instance</param>
        /// <param name="threadSafe">Specifies whether to create thread safe instance</param>
        public BasicStat(BasicStat source, bool threadSafe = false)
            : this(threadSafe)
        {
            CopyFrom(source);
            return;
        }

        //Properties
        /// <summary>
        /// Number of sample values affected in the statistics
        /// </summary>
        public int NumOfSamples
        {
            get
            {
                if (_threadSafe)
                {
                    lock (_lock)
                    {
                        return _numOfSamples;
                    }
                }
                else
                {
                    return _numOfSamples;
                }
            }
        }

        /// <summary>
        /// Number of nonzero sample values affected in the statistics
        /// </summary>
        public int NumOfNonzeroSamples
        {
            get
            {
                if (_threadSafe)
                {
                    lock (_lock)
                    {
                        return _numOfNonzeroSamples;
                    }
                }
                else
                {
                    return _numOfNonzeroSamples;
                }
            }
        }

        /// <summary>
        /// Sum of the sample values
        /// </summary>
        public double Sum
        {
            get
            {
                if (_threadSafe)
                {
                    lock (_lock)
                    {
                        return _sum;
                    }
                }
                else
                {
                    return _sum;
                }
            }
        }

        /// <summary>
        /// Sum of the negative sample values
        /// </summary>
        public double NegSum
        {
            get
            {
                if (_threadSafe)
                {
                    lock (_lock)
                    {
                        return _negSum;
                    }
                }
                else
                {
                    return _negSum;
                }
            }
        }

        /// <summary>
        /// Sum of the positive sample values
        /// </summary>
        public double PosSum
        {
            get
            {
                if (_threadSafe)
                {
                    lock (_lock)
                    {
                        return _posSum;
                    }
                }
                else
                {
                    return _posSum;
                }
            }
        }

        /// <summary>
        /// Sum of the squared sample values
        /// </summary>
        public double SumOfSquares
        {
            get
            {
                if (_threadSafe)
                {
                    lock (_lock)
                    {
                        return _sumOfSquares;
                    }
                }
                else
                {
                    return _sumOfSquares;
                }
            }
        }

        /// <summary>
        /// Min sample value
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
        /// Max sample value
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
        /// Mid = Min + ((Max-Min)/2)
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
        /// Span = (Max-Min)
        /// </summary>
        public double Span
        {
            get
            {
                if (_threadSafe)
                {
                    lock (_lock)
                    {
                        return (_max - _min);
                    }
                }
                else
                {
                    return (_max - _min);
                }
            }
        }

        /// <summary>
        /// Mean of the sample values
        /// </summary>
        public double ArithAvg
        {
            get
            {
                if (_threadSafe)
                {
                    lock (_lock)
                    {
                        Recompute();
                        return _arithAvg;
                    }
                }
                else
                {
                    Recompute();
                    return _arithAvg;
                }
            }
        }

        /// <summary>
        /// Mean of the squared sample values
        /// </summary>
        public double MeanSquare
        {
            get
            {
                if (_threadSafe)
                {
                    lock (_lock)
                    {
                        Recompute();
                        return _meanSquare;
                    }
                }
                else
                {
                    Recompute();
                    return _meanSquare;
                }
            }
        }

        /// <summary>
        /// Root of the mean of the squared sample values
        /// </summary>
        public double RootMeanSquare
        {
            get
            {
                if (_threadSafe)
                {
                    lock (_lock)
                    {
                        Recompute();
                        return _rootMeanSquare;
                    }
                }
                else
                {
                    Recompute();
                    return _rootMeanSquare;
                }
            }
        }

        /// <summary>
        /// The variance of the sample values
        /// </summary>
        public double Variance
        {
            get
            {
                if (_threadSafe)
                {
                    lock (_lock)
                    {
                        Recompute();
                        return _variance;
                    }
                }
                else
                {
                    Recompute();
                    return _variance;
                }
            }
        }

        /// <summary>
        /// The standard deviation of the sample values
        /// </summary>
        public double StdDev
        {
            get
            {
                if (_threadSafe)
                {
                    lock (_lock)
                    {
                        Recompute();
                        return _stdDev;
                    }
                }
                else
                {
                    Recompute();
                    return _stdDev;
                }
            }
        }

        /// <summary>
        /// SpanDev = (Span * StdDev)
        /// </summary>
        public double SpanDev
        {
            get
            {
                if (_threadSafe)
                {
                    lock (_lock)
                    {
                        Recompute();
                        return _spanDev;
                    }
                }
                else
                {
                    Recompute();
                    return _spanDev;
                }
            }
        }

        //Methods
        /// <summary>
        /// Resets the statistics
        /// </summary>
        public void Reset()
        {
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
            _sum = 0;
            _negSum = 0;
            _posSum = 0;
            _sumOfSquares = 0;
            _min = 0;
            _max = 0;
            _numOfSamples = 0;
            _numOfNonzeroSamples = 0;
            _recompute = true;
            Recompute();
            return;
        }

        /// <summary>
        /// Copies all internal attribute values from the source instance into this instance
        /// </summary>
        /// <param name="source">The source instance</param>
        public void CopyFrom(BasicStat source)
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

        /// <summary>
        /// Creates the deep copy of this instance
        /// </summary>
        public BasicStat DeepClone()
        {
            if (_threadSafe)
            {
                lock (_lock)
                {
                    return new BasicStat(this, _threadSafe);
                }
            }
            else
            {
                return new BasicStat(this, _threadSafe);
            }
        }

        private void CopyFromInternal(BasicStat source)
        {
            _sum = source._sum;
            _posSum = source._posSum;
            _negSum = source._negSum;
            _sumOfSquares = source._sumOfSquares;
            _min = source._min;
            _max = source._max;
            _numOfSamples = source._numOfSamples;
            _numOfNonzeroSamples = source._numOfNonzeroSamples;
            _recompute = source._recompute;
            _arithAvg = source._arithAvg;
            _rootMeanSquare = source._rootMeanSquare;
            _variance = source._variance;
            _stdDev = source._stdDev;
            _spanDev = source._spanDev;
            return;
        }

        /// <summary>
        /// Recomputes the statistics (if necessary)
        /// </summary>
        private void Recompute()
        {
            if (_recompute)
            {
                if (_numOfSamples > 0)
                {
                    _arithAvg = _sum / (double)(_numOfSamples);
                    _meanSquare = _sumOfSquares / (double)(_numOfSamples);
                    _rootMeanSquare = Math.Sqrt(_meanSquare);
                    _variance = (_sumOfSquares / (double)(_numOfSamples)) - _arithAvg.Power(2);
                    _stdDev = (_variance > 0) ? _stdDev = Math.Sqrt(_variance) : 0;
                    _spanDev = (_max - _min) * _stdDev;
                }
                else
                {
                    _arithAvg = 0;
                    _meanSquare = 0;
                    _rootMeanSquare = 0;
                    _variance = 0;
                    _stdDev = 0;
                    _spanDev = 0;
                }
                _recompute = false;
            }
            return;
        }

        /// <summary>
        /// Function computes ArithAvg, Variance and StdDev for next hypothetical sample value.
        /// Function does not change the instance, it is a simulation only.
        /// </summary>
        /// <param name="simSampleValue">Next hypothetical sample value</param>
        /// <param name="simArithAvg">Simulated ArithAvg</param>
        /// <param name="simVariance">Simulated Variance</param>
        /// <param name="simStdDev">Simulated StdDev</param>
        public void SimulateNext(double simSampleValue, out double simArithAvg, out double simVariance, out double simStdDev)
        {
            if (_threadSafe)
            {
                lock (_lock)
                {
                    SimulateNextInternal(simSampleValue, out simArithAvg, out simVariance, out simStdDev);
                }
            }
            else
            {
                SimulateNextInternal(simSampleValue, out simArithAvg, out simVariance, out simStdDev);
            }
            return;
        }

        private void SimulateNextInternal(double simSampleValue, out double simArithAvg, out double simVariance, out double simStdDev)
        {
            Recompute();
            simArithAvg = (_sum + simSampleValue) / (double)(_numOfSamples + 1);
            simVariance = ((_sumOfSquares + simSampleValue.Power(2)) / (double)(_numOfSamples + 1)) - simArithAvg.Power(2);
            simStdDev = Math.Sqrt(simVariance);
            return;
        }


        /// <summary>
        /// Affects the sample value
        /// </summary>
        public void AddSampleValue(double value)
        {
            if (_threadSafe)
            {
                lock (_lock)
                {
                    AddSampleValueInternal(value);
                }
            }
            else
            {
                AddSampleValueInternal(value);
            }
            return;
        }

        private void AddSampleValueInternal(double value)
        {
            _sum += value;
            if (value < 0)
            {
                _negSum += value;
            }
            else
            {
                _posSum += value;
            }
            _sumOfSquares += value.Power(2);
            if (_numOfSamples == 0)
            {
                _min = _max = value;
            }
            else
            {
                if (value < _min) _min = value;
                else if (value > _max) _max = value;
            }
            ++_numOfSamples;
            if (value != 0)
            {
                ++_numOfNonzeroSamples;
            }
            _recompute = true;
            return;
        }

        /// <summary>
        /// Affects sample values from the collection
        /// </summary>
        public void AddSampleValues(IEnumerable<double> sampleValueCollection)
        {
            foreach (double value in sampleValueCollection)
            {
                AddSampleValue(value);
            }
            return;
        }

        /// <summary>
        /// Affects sample values from the collection
        /// </summary>
        public void AddSampleValues(IEnumerable<long> sampleValueCollection)
        {
            foreach (long value in sampleValueCollection)
            {
                AddSampleValue(value);
            }
            return;
        }

        /// <summary>
        /// Affects sample values from the collection
        /// </summary>
        public void AddSampleValues(IEnumerable<ulong> sampleValueCollection)
        {
            foreach (ulong value in sampleValueCollection)
            {
                AddSampleValue(value);
            }
            return;
        }

        /// <summary>
        /// Affects sample values from the collection
        /// </summary>
        public void AddSampleValues(IEnumerable<int> sampleValueCollection)
        {
            foreach (int value in sampleValueCollection)
            {
                AddSampleValue(value);
            }
            return;
        }

        /// <summary>
        /// Affects sample values from the collection
        /// </summary>
        public void AddSampleValues(IEnumerable<uint> sampleValueCollection)
        {
            foreach (uint value in sampleValueCollection)
            {
                AddSampleValue(value);
            }
            return;
        }

        /// <summary>
        /// Affects sample values from the collection
        /// </summary>
        public void AddSampleValues(IEnumerable<byte> sampleValueCollection)
        {
            foreach (byte value in sampleValueCollection)
            {
                AddSampleValue(value);
            }
            return;
        }

        /// <summary>
        /// Resets the statistics and then affects sample values from the collection
        /// </summary>
        public void SetSampleValues(IEnumerable<double> sampleValueCollection)
        {
            Reset();
            AddSampleValues(sampleValueCollection);
            return;
        }

        /// <summary>
        /// Resets the statistics and then affects sample values from the collection
        /// </summary>
        public void SetSampleValues(IEnumerable<long> sampleValueCollection)
        {
            Reset();
            AddSampleValues(sampleValueCollection);
            return;
        }

        /// <summary>
        /// Resets the statistics and then affects sample values from the collection
        /// </summary>
        public void SetSampleValues(IEnumerable<ulong> sampleValueCollection)
        {
            Reset();
            AddSampleValues(sampleValueCollection);
            return;
        }

        /// <summary>
        /// Resets the statistics and then affects sample values from the collection
        /// </summary>
        public void SetSampleValues(IEnumerable<int> sampleValueCollection)
        {
            Reset();
            AddSampleValues(sampleValueCollection);
            return;
        }

        /// <summary>
        /// Resets the statistics and then affects sample values from the collection
        /// </summary>
        public void SetSampleValues(IEnumerable<uint> sampleValueCollection)
        {
            Reset();
            AddSampleValues(sampleValueCollection);
            return;
        }

        /// <summary>
        /// Resets the statistics and then affects sample values from the collection
        /// </summary>
        public void SetSampleValues(IEnumerable<byte> sampleValueCollection)
        {
            Reset();
            AddSampleValues(sampleValueCollection);
            return;
        }

        /// <summary>
        /// Gets the statistical feature
        /// </summary>
        /// <param name="feature">Requiered statistical feature</param>
        public double Get(OutputFeature feature)
        {
            switch(feature)
            {
                case OutputFeature.Sum: return Sum;
                case OutputFeature.NegSum: return NegSum;
                case OutputFeature.PosSum: return PosSum;
                case OutputFeature.SumOfSquares: return SumOfSquares;
                case OutputFeature.Min: return Min;
                case OutputFeature.Max: return Max;
                case OutputFeature.Mid: return Mid;
                case OutputFeature.Span: return Span;
                case OutputFeature.ArithAvg: return ArithAvg;
                case OutputFeature.MeanSquare: return MeanSquare;
                case OutputFeature.RootMeanSquare: return RootMeanSquare;
                case OutputFeature.Variance: return Variance;
                case OutputFeature.StdDev: return StdDev;
                case OutputFeature.SpanDev: return SpanDev;
                default: return 0d;
            }
        }

    }//BasicStat

}//Namespace
