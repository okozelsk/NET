using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using OKOSW.Extensions;

namespace OKOSW.MathTools
{
    /// <summary>
    /// Implements thread safe normalizer and denormalizer. Scales input to desired normalized range and vice versa.
    /// </summary>
    [Serializable]
    public class Normalizer
    {
        //Constants
        public const double DEFAULT_LOW_BORDER = 0;
        public const double DEFAULT_HIGH_BORDER = 1;

        //Attributes
        private double m_reserveRatio;
        private Interval m_normalizedRange;
        private Interval m_samplesRange;

        //Constructors
        public Normalizer(Normalizer source)
        {
            m_reserveRatio = source.m_reserveRatio;
            m_normalizedRange = new Interval(source.m_normalizedRange);
            m_samplesRange = new Interval(source.m_samplesRange);
            return;
        }

        public Normalizer(double reserveRatio)
        {
            m_normalizedRange = new Interval(DEFAULT_LOW_BORDER, DEFAULT_HIGH_BORDER);
            m_samplesRange = new Interval();
            m_reserveRatio = reserveRatio;
            return;
        }

        public Normalizer(Interval normalizedRange, double reserveRatio)
        {
            m_normalizedRange = new Interval(normalizedRange);
            m_samplesRange = new Interval();
            m_reserveRatio = reserveRatio;
            return;
        }

        //Properties
        public bool Initialized { get { return m_samplesRange.Initialized; } }
        public double ReserveRatio { get { return m_reserveRatio; } }
        public double SamplesMin { get { return m_samplesRange.Min; } }
        public double SamplesMax { get { return m_samplesRange.Max; } }
        public double SamplesSpan { get { return m_samplesRange.Span; } }
        public Interval SamplesSafeInterval
        {
            get
            {
                return new Interval(m_samplesRange.Min - (m_samplesRange.Span * (m_reserveRatio / 2d)), m_samplesRange.Max + (m_samplesRange.Span * (m_reserveRatio / 2d)));
            }
        }

        //methods
        public void SetSamplesRange(double min, double max)
        {
            m_samplesRange.Set(min, max);
            return;
        }

        public void SetSamplesRange(Interval samplesRange)
        {
            m_samplesRange.Set(samplesRange);
            return;
        }

        public void Adjust(double sampleValue)
        {
            m_samplesRange.Adjust(sampleValue);
            return;
        }

        public void Adjust(Interval samplesRange)
        {
            m_samplesRange.Adjust(samplesRange.Min);
            m_samplesRange.Adjust(samplesRange.Max);
            return;
        }

        public void SetNormalizedRange(Interval normalizedRange)
        {
            m_normalizedRange.Set(normalizedRange);
            return;
        }

        public void SetReserveRatio(double reserveRatio)
        {
            m_reserveRatio = reserveRatio;
            return;
        }

        public double GetNormalizedValue(double naturalValue)
        {
            if(!Initialized)
            {
                throw new ApplicationException("Not properly initialized");
            }
            Interval naturalRange = SamplesSafeInterval;
            if(!naturalRange.Belongs(naturalValue))
            {
                throw new ApplicationException("NaturalValue out of allowed samples range (including reserve)");
            }
            return (m_normalizedRange.Min + (m_normalizedRange.Span * ((naturalValue - naturalRange.Min) / naturalRange.Span))).Bound(m_normalizedRange.Min, m_normalizedRange.Max);
        }

        public double GetNaturalValue(double normalizedValue)
        {
            if (!Initialized)
            {
                throw new ApplicationException("Not properly initialized");
            }
            if (!m_normalizedRange.Belongs(normalizedValue))
            {
                throw new ApplicationException("NormalizedValue out of allowed range");
            }
            Interval naturalRange = SamplesSafeInterval;
            return (naturalRange.Min + (naturalRange.Span * ((normalizedValue - m_normalizedRange.Min) / m_normalizedRange.Span))).Bound(naturalRange.Min, naturalRange.Max);
        }

    }
}
