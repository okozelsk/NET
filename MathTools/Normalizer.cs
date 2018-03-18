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
    /// Normalizer supports data standardization (gausse)
    /// </summary>
    [Serializable]
    public class Normalizer
    {
        //Constants
        //Default borders of normalization range
        public const double DEFAULT_NR_MIN = -1;
        public const double DEFAULT_NR_MAX = 1;
        //Default is to use gausse standardization during normalization
        public const bool DEFAULT_STANDARDIZE = true;

        //Attributes
        private bool m_standardize;
        private double m_reserveRatio;
        private BasicStat m_samplesStat;
        private Interval m_normRange;

        //Constructors
        /// <summary>
        /// Constructs normalizer as a copy of another normalizer
        /// </summary>
        /// <param name="source">Source normalizer</param>
        public Normalizer(Normalizer source)
        {
            m_standardize = source.m_standardize;
            m_reserveRatio = source.m_reserveRatio;
            m_samplesStat = new BasicStat(source.m_samplesStat);
            m_normRange = new Interval(source.m_normRange);
            return;
        }

        /// <summary>
        /// Constructs normalizer
        /// </summary>
        /// <param name="reserveRatio">Reserved part of known samples range to avoid overflow by future unseen data.</param>
        /// <param name="standardize">If to apply gausse data standardization</param>
        public Normalizer(double reserveRatio, bool standardize = DEFAULT_STANDARDIZE)
        {
            m_standardize = standardize;
            m_normRange = new Interval(DEFAULT_NR_MIN, DEFAULT_NR_MAX);
            m_samplesStat = new BasicStat();
            m_reserveRatio = reserveRatio;
            return;
        }

        /// <summary>
        /// Constructs normalizer
        /// </summary>
        /// <param name="normRange">Output range of normalized values</param>
        /// <param name="reserveRatio">Reserved part of known samples range to avoid overflow by future unseen data.</param>
        /// <param name="standardize">If to apply gausse data standardization</param>
        public Normalizer(Interval normRange, double reserveRatio, bool standardize = DEFAULT_STANDARDIZE)
        {
            m_standardize = standardize;
            m_normRange = new Interval(normRange);
            m_samplesStat = new BasicStat();
            m_reserveRatio = reserveRatio;
            return;
        }

        //Properties
        public bool Initialized { get { return (m_samplesStat.SamplesCount > 0 && m_samplesStat.Min != m_samplesStat.Max); } }
        public double ReserveRatio { get { return m_reserveRatio; } }
        public bool Standardize { get { return m_standardize; } }
        public Interval NormRange { get { return m_normRange; } }
        public BasicStat SamplesStat { get { return m_samplesStat; } }
        private double VMin { get { return m_samplesStat.Min - ((m_samplesStat.Span * m_reserveRatio) / 2); } }
        private double VMax { get { return m_samplesStat.Max + ((m_samplesStat.Span * m_reserveRatio) / 2); } }

        //Methods
        /// <summary>
        /// Updates normalizer
        /// </summary>
        /// <param name="sampleValue">Sample value</param>
        public void Adjust(double sampleValue)
        {
            m_samplesStat.AddSampleValue(sampleValue);
            return;
        }

        /// <summary>
        /// Standard normalization
        /// </summary>
        /// <param name="min">Samples min</param>
        /// <param name="max">Samples max</param>
        /// <param name="v">Natural (sample) value to be normalized</param>
        /// <returns>Normalized v</returns>
        private double Normalize(double min, double max, double v)
        {
            return m_normRange.Min + m_normRange.Span * ((v - min) / (max - min));
        }

        /// <summary>
        /// Standard denormalization
        /// </summary>
        /// <param name="min">Samples min</param>
        /// <param name="max">Samples max</param>
        /// <param name="n">Normalized value</param>
        /// <returns>Natural value</returns>
        private double Naturalize(double min, double max, double n)
        {
            return min + (max - min) * ((n - m_normRange.Min) / m_normRange.Span);
        }

        /// <summary>
        /// Checks normalizer readyness
        /// </summary>
        private void CheckInitiated()
        {
            if (!Initialized)
            {
                throw new Exception("Not properly initialized");
            }
            return;
        }

        /// <summary>
        /// Computes half of gausse interval
        /// </summary>
        /// <returns>Half of gausse interval</returns>
        private double ComputeGausseHalfInterval()
        {
            double gausseLo = Math.Abs((VMin - m_samplesStat.ArithAvg) / m_samplesStat.StdDev);
            double gausseHi = Math.Abs((VMax - m_samplesStat.ArithAvg) / m_samplesStat.StdDev);
            return Math.Max(gausseLo, gausseHi);
        }

        /// <summary>
        /// Normalizes given natural (sample) value
        /// </summary>
        /// <param name="naturalValue">Natural value to be normalized</param>
        /// <returns>Normalized value</returns>
        public double Normalize(double naturalValue)
        {
            //Check readiness
            CheckInitiated();
            //Value preprocessing
            if (m_standardize)
            {
                //Gausse standardization
                double gausseHalfInt = ComputeGausseHalfInterval();
                double gausseValue = (naturalValue - m_samplesStat.ArithAvg) / m_samplesStat.StdDev;
                //Normalization
                return Normalize(-gausseHalfInt, gausseHalfInt, gausseValue);
            }
            else
            {
                //Normalization
                return Normalize(VMin, VMax, naturalValue);
            }
        }

        /// <summary>
        /// Denormalizes given normalized value
        /// </summary>
        /// <param name="normValue">Normalized value to be denormalized</param>
        /// <returns>Natural (denormalized) value</returns>
        public double Naturalize(double normValue)
        {
            //Check readiness
            CheckInitiated();
            //Value preprocessing
            if (m_standardize)
            {
                //Denormalization
                double gausseHalfInt = ComputeGausseHalfInterval();
                double standardizedGausse = Naturalize(-gausseHalfInt, gausseHalfInt, normValue);
                //Destandardization -> natural value
                return (standardizedGausse * m_samplesStat.StdDev) + m_samplesStat.ArithAvg;
            }
            else
            {
                //Denormalization -> natural value
                return Naturalize(VMin, VMax, normValue);
            }
        }

        /// <summary>
        /// Computes what part of natural range is affected by normalized error (piece of normalized range)
        /// </summary>
        /// <param name="normError">Piece of normalized range</param>
        /// <returns>Piece of natural range</returns>
        public double ComputeNaturalError(double normError)
        {
            CheckInitiated();
            return ((VMax - VMin) * Math.Abs(normError)) * (1 - m_reserveRatio);
        }

    }//Normalizer
}//Namespace
