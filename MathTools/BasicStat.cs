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
        private Object m_lock = new Object();
        //Base values
        private double m_sum;
        private double m_sumOfPowers;
        private double m_min;
        private double m_max;
        private int m_samplesCount;
        private int m_nonzeroSamplesCount;
        //Precomputed properties
        private bool m_recompute;
        private double m_arithAvg;
        private double m_meanSquare;
        private double m_rootMeanSquare;
        private double m_variance;
        private double m_stdDev;
        private double m_spanDev;

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
                lock(m_lock)
                {
                    return m_samplesCount;
                }
            }
        }

        public int NonzeroSamplesCount
        {
            get
            {
                lock (m_lock)
                {
                    return m_nonzeroSamplesCount;
                }
            }
        }

        public double Sum
        {
            get
            {
                lock (m_lock)
                {
                    return m_sum;
                }
            }
        }

        public double SumOfPowers
        {
            get
            {
                lock (m_lock)
                {
                    return m_sumOfPowers;
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
                    return (m_max - m_min);
                }
            }
        }

        public double ArithAvg
        {
            get
            {
                lock (m_lock)
                {
                    Recompute();
                    return m_arithAvg;
                }
            }
        }

        public double MeanSquare
        {
            get
            {
                lock (m_lock)
                {
                    Recompute();
                    return m_meanSquare;
                }
            }
        }

        public double RootMeanSquare
        {
            get
            {
                lock (m_lock)
                {
                    Recompute();
                    return m_rootMeanSquare;
                }
            }
        }

        public double Variance
        {
            get
            {
                lock (m_lock)
                {
                    Recompute();
                    return m_variance;
                }
            }
        }

        public double StdDev
        {
            get
            {
                lock (m_lock)
                {
                    Recompute();
                    return m_stdDev;
                }
            }
        }

        public double SpanDev
        {
            get
            {
                lock (m_lock)
                {
                    Recompute();
                    return m_spanDev;
                }
            }
        }

        //Methods
        public void Reset()
        {
            lock(m_lock)
            {
                m_sum = 0;
                m_sumOfPowers = 0;
                m_min = 0;
                m_max = 0;
                m_samplesCount = 0;
                m_nonzeroSamplesCount = 0;
                m_recompute = true;
                Recompute();
            }
            return;
        }

        public void Adopt(BasicStat source)
        {
            lock(m_lock)
            {
                m_sum = source.m_sum;
                m_sumOfPowers = source.m_sumOfPowers;
                m_min = source.m_min;
                m_max = source.m_max;
                m_samplesCount = source.m_samplesCount;
                m_nonzeroSamplesCount = source.m_nonzeroSamplesCount;
                m_recompute = source.m_recompute;
                m_arithAvg = source.m_arithAvg;
                m_rootMeanSquare = source.m_rootMeanSquare;
                m_variance = source.m_variance;
                m_stdDev = source.m_stdDev;
                m_spanDev = source.m_spanDev;
            }
            return;
        }

        private void Recompute()
        {
            if(m_recompute)
            {
                if (m_samplesCount > 0)
                {
                    m_arithAvg = m_sum / (double)(m_samplesCount);
                    m_meanSquare = m_sumOfPowers / (double)(m_samplesCount);
                    m_rootMeanSquare = Math.Sqrt(m_meanSquare);
                    m_variance = (m_sumOfPowers / (double)(m_samplesCount)) - m_arithAvg.Power(2);
                    m_stdDev = (m_variance > 0) ? m_stdDev = Math.Sqrt(m_variance) : 0;
                    m_spanDev = (m_max - m_min) * m_stdDev;
                }
                else
                {
                    m_arithAvg = 0;
                    m_meanSquare = 0;
                    m_rootMeanSquare = 0;
                    m_variance = 0;
                    m_stdDev = 0;
                    m_spanDev = 0;
                }
                m_recompute = false;
            }
            return;
        }

        public void AddSampleValue(double value)
        {
            lock(m_lock)
            {
                m_sum += value;
                m_sumOfPowers += value.Power(2);
                if (m_samplesCount == 0)
                {
                    m_min = m_max = value;
                }
                else
                {
                    if (value < m_min) m_min = value;
                    else if (value > m_max) m_max = value;
                }
                ++m_samplesCount;
                if (value != 0)
                {
                    ++m_nonzeroSamplesCount;
                }
                m_recompute = true;
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