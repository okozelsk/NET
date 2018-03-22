using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using OKOSW.Extensions;

namespace OKOSW.MathTools
{
    /// <summary>
    /// Implements simple statistics (thread safe).
    /// </summary>
    [Serializable]
    public class BasicStat
    {
        //Attributes
        //Locker to ensure thread safe behaviour
        private Object _lock = new Object();
        //Base values
        private double _sum;
        private double _sumOfPowers;
        private double _min;
        private double _max;
        private int _samplesCount;
        private int _nonzeroSamplesCount;
        //Precomputed properties
        private bool _recompute;
        private double _arithAvg;
        private double _meanSquare;
        private double _rootMeanSquare;
        private double _variance;
        private double _stdDev;
        private double _spanDev;

        //Constructors
        public BasicStat()
        {
            Reset();
            return;
        }

        public BasicStat(IEnumerable<double> values)
        {
            SetSampleValues(values);
            return;
        }

        public BasicStat(IEnumerable<long> values)
        {
            SetSampleValues(values);
            return;
        }

        public BasicStat(IEnumerable<ulong> values)
        {
            SetSampleValues(values);
            return;
        }

        public BasicStat(IEnumerable<int> values)
        {
            SetSampleValues(values);
            return;
        }

        public BasicStat(IEnumerable<uint> values)
        {
            SetSampleValues(values);
            return;
        }

        public BasicStat(IEnumerable<byte> values)
        {
            SetSampleValues(values);
            return;
        }

        public BasicStat(BasicStat source)
        {
            Adopt(source);
            return;
        }

        //Properties
        public int SamplesCount
        {
            get
            {
                lock(_lock)
                {
                    return _samplesCount;
                }
            }
        }

        public int NonzeroSamplesCount
        {
            get
            {
                lock (_lock)
                {
                    return _nonzeroSamplesCount;
                }
            }
        }

        public double Sum
        {
            get
            {
                lock (_lock)
                {
                    return _sum;
                }
            }
        }

        public double SumOfPowers
        {
            get
            {
                lock (_lock)
                {
                    return _sumOfPowers;
                }
            }
        }

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

        public double Span
        {
            get
            {
                lock (_lock)
                {
                    return (_max - _min);
                }
            }
        }

        public double ArithAvg
        {
            get
            {
                lock (_lock)
                {
                    Recompute();
                    return _arithAvg;
                }
            }
        }

        public double MeanSquare
        {
            get
            {
                lock (_lock)
                {
                    Recompute();
                    return _meanSquare;
                }
            }
        }

        public double RootMeanSquare
        {
            get
            {
                lock (_lock)
                {
                    Recompute();
                    return _rootMeanSquare;
                }
            }
        }

        public double Variance
        {
            get
            {
                lock (_lock)
                {
                    Recompute();
                    return _variance;
                }
            }
        }

        public double StdDev
        {
            get
            {
                lock (_lock)
                {
                    Recompute();
                    return _stdDev;
                }
            }
        }

        public double SpanDev
        {
            get
            {
                lock (_lock)
                {
                    Recompute();
                    return _spanDev;
                }
            }
        }

        //Methods
        public void Reset()
        {
            lock(_lock)
            {
                _sum = 0;
                _sumOfPowers = 0;
                _min = 0;
                _max = 0;
                _samplesCount = 0;
                _nonzeroSamplesCount = 0;
                _recompute = true;
                Recompute();
            }
            return;
        }

        public void Adopt(BasicStat source)
        {
            lock(_lock)
            {
                _sum = source._sum;
                _sumOfPowers = source._sumOfPowers;
                _min = source._min;
                _max = source._max;
                _samplesCount = source._samplesCount;
                _nonzeroSamplesCount = source._nonzeroSamplesCount;
                _recompute = source._recompute;
                _arithAvg = source._arithAvg;
                _rootMeanSquare = source._rootMeanSquare;
                _variance = source._variance;
                _stdDev = source._stdDev;
                _spanDev = source._spanDev;
            }
            return;
        }

        private void Recompute()
        {
            if(_recompute)
            {
                if (_samplesCount > 0)
                {
                    _arithAvg = _sum / (double)(_samplesCount);
                    _meanSquare = _sumOfPowers / (double)(_samplesCount);
                    _rootMeanSquare = Math.Sqrt(_meanSquare);
                    _variance = (_sumOfPowers / (double)(_samplesCount)) - _arithAvg.Power(2);
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

        public void AddSampleValue(double value)
        {
            lock(_lock)
            {
                _sum += value;
                _sumOfPowers += value.Power(2);
                if (_samplesCount == 0)
                {
                    _min = _max = value;
                }
                else
                {
                    if (value < _min) _min = value;
                    else if (value > _max) _max = value;
                }
                ++_samplesCount;
                if (value != 0)
                {
                    ++_nonzeroSamplesCount;
                }
                _recompute = true;
            }
            return;
        }

        public void AddSampleValues(IEnumerable<double> values)
        {
            foreach(double value in values)
            {
                AddSampleValue(value);
            }
            return;
        }

        public void AddSampleValues(IEnumerable<long> values)
        {
            foreach (long value in values)
            {
                AddSampleValue(value);
            }
            return;
        }

        public void AddSampleValues(IEnumerable<ulong> values)
        {
            foreach (ulong value in values)
            {
                AddSampleValue(value);
            }
            return;
        }

        public void AddSampleValues(IEnumerable<int> values)
        {
            foreach (int value in values)
            {
                AddSampleValue(value);
            }
            return;
        }

        public void AddSampleValues(IEnumerable<uint> values)
        {
            foreach (uint value in values)
            {
                AddSampleValue(value);
            }
            return;
        }

        public void AddSampleValues(IEnumerable<byte> values)
        {
            foreach (byte value in values)
            {
                AddSampleValue(value);
            }
            return;
        }

        public void SetSampleValues(IEnumerable<double> values)
        {
            Reset();
            AddSampleValues(values);
            return;
        }

        public void SetSampleValues(IEnumerable<long> values)
        {
            Reset();
            AddSampleValues(values);
            return;
        }

        public void SetSampleValues(IEnumerable<ulong> values)
        {
            Reset();
            AddSampleValues(values);
            return;
        }

        public void SetSampleValues(IEnumerable<int> values)
        {
            Reset();
            AddSampleValues(values);
            return;
        }

        public void SetSampleValues(IEnumerable<uint> values)
        {
            Reset();
            AddSampleValues(values);
            return;
        }

        public void SetSampleValues(IEnumerable<byte> values)
        {
            Reset();
            AddSampleValues(values);
            return;
        }

    }
}