using RCNet.Extensions;
using System;
using System.Collections.Generic;

namespace RCNet.MathTools
{
    /// <summary>
    /// Implements the basic statistics of sample data.
    /// </summary>
    [Serializable]
    public class BasicStat
    {
        //Enums
        /// <summary>
        /// The statistical figure.
        /// </summary>
        public enum StatisticalFigure
        {
            /// <summary>
            /// The sum of all samples.
            /// </summary>
            Sum,
            /// <summary>
            /// The sum of negative samples.
            /// </summary>
            NegSum,
            /// <summary>
            /// The sum of positive samples.
            /// </summary>
            PosSum,
            /// <summary>
            /// The sum of squared samples.
            /// </summary>
            SumOfSquares,
            /// <summary>
            /// The min sample.
            /// </summary>
            Min,
            /// <summary>
            /// The max sample.
            /// </summary>
            Max,
            /// <summary>
            /// The center value between the Min and Max.
            /// </summary>
            Mid,
            /// <summary>
            /// The span of the Min and Max (Max - Min).
            /// </summary>
            Span,
            /// <summary>
            /// The arithmetic average.
            /// </summary>
            ArithAvg,
            /// <summary>
            /// The mean of the squared samples.
            /// </summary>
            MeanSquare,
            /// <summary>
            /// The root of the mean of the squared samples.
            /// </summary>
            RootMeanSquare,
            /// <summary>
            /// The variance of the samples.
            /// </summary>
            Variance,
            /// <summary>
            /// The standard deviation of the samples.
            /// </summary>
            StdDev,
            /// <summary>
            /// The span multiplied by the standard deviation of the samples.
            /// </summary>
            SpanDev
        }

        //Attributes
        //Locking for the thread safe behaviour
        private readonly Object _lock;
        private readonly bool _threadSafe;
        //Cumulative
        private double _sum;
        private double _negSum;
        private double _posSum;
        private double _sumOfSquares;
        private double _min;
        private double _max;
        private int _numOfSamples;
        private int _numOfNonzeroSamples;
        //Precomputed
        private double _arithAvg;
        private double _meanSquare;
        private double _rootMeanSquare;
        private double _variance;
        private double _stdDev;
        private double _spanDev;
        //Recomputation request indicatior
        private bool _recompute;

        //Constructors
        /// <summary>
        /// Creates an uninitialized instance.
        /// </summary>
        /// <param name="threadSafe">Specifies whether to create a thread safe instance.</param>
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
        /// Creates an initialized instance.
        /// </summary>
        /// <param name="sampleCollection">The samples to be processed.</param>
        /// <param name="threadSafe">Specifies whether to create a thread safe instance.</param>
        public BasicStat(IEnumerable<double> sampleCollection, bool threadSafe = false)
            : this(threadSafe)
        {
            SetSampleValues(sampleCollection);
            return;
        }

        /// <summary>
        /// Creates an initialized instance.
        /// </summary>
        /// <param name="sampleCollection">The samples to be processed.</param>
        /// <param name="threadSafe">Specifies whether to create a thread safe instance.</param>
        public BasicStat(IEnumerable<long> sampleCollection, bool threadSafe = false)
            : this(threadSafe)
        {
            SetSampleValues(sampleCollection);
            return;
        }

        /// <summary>
        /// Creates an initialized instance.
        /// </summary>
        /// <param name="sampleCollection">The samples to be processed.</param>
        /// <param name="threadSafe">Specifies whether to create a thread safe instance.</param>
        public BasicStat(IEnumerable<ulong> sampleCollection, bool threadSafe = false)
            : this(threadSafe)
        {
            SetSampleValues(sampleCollection);
            return;
        }

        /// <summary>
        /// Creates an initialized instance.
        /// </summary>
        /// <param name="sampleCollection">The samples to be processed.</param>
        /// <param name="threadSafe">Specifies whether to create a thread safe instance.</param>
        public BasicStat(IEnumerable<int> sampleCollection, bool threadSafe = false)
            : this(threadSafe)
        {
            SetSampleValues(sampleCollection);
            return;
        }

        /// <summary>
        /// Creates an initialized instance.
        /// </summary>
        /// <param name="sampleCollection">The samples to be processed.</param>
        /// <param name="threadSafe">Specifies whether to create a thread safe instance.</param>
        public BasicStat(IEnumerable<uint> sampleCollection, bool threadSafe = false)
            : this(threadSafe)
        {
            SetSampleValues(sampleCollection);
            return;
        }

        /// <summary>
        /// Creates an initialized instance.
        /// </summary>
        /// <param name="sampleCollection">The samples to be processed.</param>
        /// <param name="threadSafe">Specifies whether to create a thread safe instance.</param>
        public BasicStat(IEnumerable<byte> sampleCollection, bool threadSafe = false)
            : this(threadSafe)
        {
            SetSampleValues(sampleCollection);
            return;
        }

        /// <summary>
        /// The copy constructor.
        /// </summary>
        /// <param name="source">The source instance.</param>
        /// <param name="threadSafe">Specifies whether to create a thread safe instance.</param>
        public BasicStat(BasicStat source, bool threadSafe = false)
            : this(threadSafe)
        {
            CopyFrom(source);
            return;
        }

        //Properties
        /// <summary>
        /// Gets the number of the samples.
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
        /// Gets the number of the nonzero samples.
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
        /// Gets the sum of the samples.
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
        /// Gets the sum of the negative samples.
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
        /// Gets the sum of the positive samples.
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
        /// Gets the sum of the squared samples.
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
        /// Gets the min sample.
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
        /// Gets the max sample.
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
        /// Gets the center value between the Min and Max.
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
        /// Gets the arithmetic average of the samples.
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
        /// Gets the mean of the squared samples.
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
        /// Gets the root of the mean of the squared samples.
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
        /// Gets the variance of the samples.
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
        /// Gets the standard deviation of the samples.
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
        /// Gets the span multiplied by the standard deviation of the samples.
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
        /// Resets the statistics.
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
        /// Copies the data from a source instance.
        /// </summary>
        /// <param name="source">The source instance.</param>
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
        /// Creates the deep copy of this instance.
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
        /// Recomputes the statistics (if necessary).
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
        /// Computes ArithAvg, Variance and StdDev considering the next hypothetical sample.
        /// </summary>
        /// <remarks>
        /// It is a simulation only.
        /// </remarks>
        /// <param name="simSampleValue">Next hypothetical sample value</param>
        /// <param name="simArithAvg">The resulting arithmetical average.</param>
        /// <param name="simVariance">The resulting variance.</param>
        /// <param name="simStdDev">The resulting standard seviation.</param>
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
        /// Adds the new sample.
        /// </summary>
        public void AddSample(double value)
        {
            if (_threadSafe)
            {
                lock (_lock)
                {
                    AddSampleInternal(value);
                }
            }
            else
            {
                AddSampleInternal(value);
            }
            return;
        }

        private void AddSampleInternal(double value)
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
        /// Adds the samples.
        /// </summary>
        /// <param name="sampleCollection">The samples.</param>
        public void AddSamples(IEnumerable<double> sampleCollection)
        {
            foreach (double value in sampleCollection)
            {
                AddSample(value);
            }
            return;
        }

        /// <summary>
        /// Adds the samples.
        /// </summary>
        /// <param name="sampleCollection">The samples.</param>
        public void AddSampleValues(IEnumerable<long> sampleCollection)
        {
            foreach (long value in sampleCollection)
            {
                AddSample(value);
            }
            return;
        }

        /// <summary>
        /// Adds the samples.
        /// </summary>
        /// <param name="sampleCollection">The samples.</param>
        public void AddSampleValues(IEnumerable<ulong> sampleCollection)
        {
            foreach (ulong value in sampleCollection)
            {
                AddSample(value);
            }
            return;
        }

        /// <summary>
        /// Adds the samples.
        /// </summary>
        /// <param name="sampleCollection">The samples.</param>
        public void AddSampleValues(IEnumerable<int> sampleCollection)
        {
            foreach (int value in sampleCollection)
            {
                AddSample(value);
            }
            return;
        }

        /// <summary>
        /// Adds the samples.
        /// </summary>
        /// <param name="sampleCollection">The samples.</param>
        public void AddSampleValues(IEnumerable<uint> sampleCollection)
        {
            foreach (uint value in sampleCollection)
            {
                AddSample(value);
            }
            return;
        }

        /// <summary>
        /// Adds the samples.
        /// </summary>
        /// <param name="sampleCollection">The samples.</param>
        public void AddSampleValues(IEnumerable<byte> sampleCollection)
        {
            foreach (byte value in sampleCollection)
            {
                AddSample(value);
            }
            return;
        }

        /// <summary>
        /// Resets the statistics and initializes the instance from the new sample set.
        /// </summary>
        /// <param name="sampleCollection">The new sample set.</param>
        public void SetSampleValues(IEnumerable<double> sampleCollection)
        {
            Reset();
            AddSamples(sampleCollection);
            return;
        }

        /// <summary>
        /// Resets the statistics and initializes the instance from the new sample set.
        /// </summary>
        /// <param name="sampleCollection">The new sample set.</param>
        public void SetSampleValues(IEnumerable<long> sampleCollection)
        {
            Reset();
            AddSampleValues(sampleCollection);
            return;
        }

        /// <summary>
        /// Resets the statistics and initializes the instance from the new sample set.
        /// </summary>
        /// <param name="sampleCollection">The new sample set.</param>
        public void SetSampleValues(IEnumerable<ulong> sampleCollection)
        {
            Reset();
            AddSampleValues(sampleCollection);
            return;
        }

        /// <summary>
        /// Resets the statistics and initializes the instance from the new sample set.
        /// </summary>
        /// <param name="sampleCollection">The new sample set.</param>
        public void SetSampleValues(IEnumerable<int> sampleCollection)
        {
            Reset();
            AddSampleValues(sampleCollection);
            return;
        }

        /// <summary>
        /// Resets the statistics and initializes the instance from the new sample set.
        /// </summary>
        /// <param name="sampleCollection">The new sample set.</param>
        public void SetSampleValues(IEnumerable<uint> sampleCollection)
        {
            Reset();
            AddSampleValues(sampleCollection);
            return;
        }

        /// <summary>
        /// Resets the statistics and initializes the instance from the new sample set.
        /// </summary>
        /// <param name="sampleCollection">The new sample set.</param>
        public void SetSampleValues(IEnumerable<byte> sampleCollection)
        {
            Reset();
            AddSampleValues(sampleCollection);
            return;
        }

        /// <summary>
        /// Gets a statistical figure.
        /// </summary>
        /// <param name="figure">The required statistical figure.</param>
        public double Get(StatisticalFigure figure)
        {
            switch (figure)
            {
                case StatisticalFigure.Sum: return Sum;
                case StatisticalFigure.NegSum: return NegSum;
                case StatisticalFigure.PosSum: return PosSum;
                case StatisticalFigure.SumOfSquares: return SumOfSquares;
                case StatisticalFigure.Min: return Min;
                case StatisticalFigure.Max: return Max;
                case StatisticalFigure.Mid: return Mid;
                case StatisticalFigure.Span: return Span;
                case StatisticalFigure.ArithAvg: return ArithAvg;
                case StatisticalFigure.MeanSquare: return MeanSquare;
                case StatisticalFigure.RootMeanSquare: return RootMeanSquare;
                case StatisticalFigure.Variance: return Variance;
                case StatisticalFigure.StdDev: return StdDev;
                case StatisticalFigure.SpanDev: return SpanDev;
                default: return 0d;
            }
        }

    }//BasicStat

}//Namespace
