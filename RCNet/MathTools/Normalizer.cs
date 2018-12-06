using System;

namespace RCNet.MathTools
{
    /// <summary>
    /// Implements thread safe (if required) normalizer and denormalizer. Scales the input to desired normalized range and vice versa.
    /// Normalizer supports gausse data standardization
    /// </summary>
    [Serializable]
    public class Normalizer
    {
        //Constants
        /// <summary>
        /// Default min border of the normalization range
        /// </summary>
        public const double DefaultNormRangeMin = -1;
        /// <summary>
        /// Default max border of the normalization range
        /// </summary>
        public const double DefaultNormRangeMax = 1;

        //Attribute properties
        /// <summary>
        /// Reserve held by a normalizer to cover cases where future data exceeds a known range of sample data.
        /// </summary>
        public double ReserveRatio { get; private set; }
        /// <summary>
        /// Indicates whether the data standardization is applied
        /// </summary>
        public bool Standardization { get; private set; }
        /// <summary>
        /// The normalization range
        /// </summary>
        public Interval NormRange { get; private set; }
        /// <summary>
        /// The statistics of the sample data
        /// </summary>
        public BasicStat SamplesStat { get; private set; }

        //Constructors
        /// <summary>
        /// A deep copy constructor
        /// </summary>
        /// <param name="source">Source instance</param>
        /// <param name="threadSafe">Specifies if to create thread safe instance</param>
        public Normalizer(Normalizer source, bool threadSafe = false)
        {
            Standardization = source.Standardization;
            ReserveRatio = source.ReserveRatio;
            SamplesStat = new BasicStat(source.SamplesStat, threadSafe);
            NormRange = new Interval(source.NormRange, threadSafe);
            return;
        }

        /// <summary>
        /// Instantiates a normalizer
        /// </summary>
        /// <param name="reserveRatio">
        /// Reserve held by a normalizer to cover cases where future data exceeds a known range of sample data.
        /// </param>
        /// <param name="standardize">
        /// Specifies whether to apply data standardization
        /// </param>
        /// <param name="threadSafe">Specifies if to create thread safe instance</param>
        public Normalizer(double reserveRatio, bool standardize = true, bool threadSafe = false)
        {
            Standardization = standardize;
            NormRange = new Interval(DefaultNormRangeMin, DefaultNormRangeMax, threadSafe);
            SamplesStat = new BasicStat(threadSafe);
            ReserveRatio = reserveRatio;
            return;
        }

        /// <summary>
        /// Constructs normalizer
        /// </summary>
        /// <param name="normRange">
        /// Range of normalized values
        /// </param>
        /// <param name="reserveRatio">
        /// Reserve held by a normalizer to cover cases where future data exceeds a known range of sample data.
        /// </param>
        /// <param name="standardize">
        /// Specifies whether to apply data standardization
        /// </param>
        /// <param name="threadSafe">Specifies if to create thread safe instance</param>
        public Normalizer(Interval normRange, double reserveRatio, bool standardize = true, bool threadSafe = false)
        {
            Standardization = standardize;
            NormRange = new Interval(normRange, threadSafe);
            SamplesStat = new BasicStat(threadSafe);
            ReserveRatio = reserveRatio;
            return;
        }

        //Properties
        /// <summary>
        /// Indicates whether the normalizer is properly initialized
        /// </summary>
        public bool Initialized { get { return (SamplesStat.NumOfSamples > 0 && SamplesStat.Min != SamplesStat.Max); } }
        private double VMin { get { return SamplesStat.Min - ((SamplesStat.Span * ReserveRatio) / 2); } }
        private double VMax { get { return SamplesStat.Max + ((SamplesStat.Span * ReserveRatio) / 2); } }

        //Methods
        /// <summary>
        /// Resets normalizer to initial state
        /// </summary>
        public void Reset()
        {
            SamplesStat.Reset();
            return;
        }

        /// <summary>
        /// Adopts data from the source normalizer
        /// </summary>
        public void Adopt(Normalizer source)
        {
            Standardization = source.Standardization;
            ReserveRatio = source.ReserveRatio;
            SamplesStat = new BasicStat(source.SamplesStat);
            NormRange = new Interval(source.NormRange);
            return;
        }

        /// <summary>
        /// Adapts to the sample value
        /// </summary>
        /// <param name="sampleValue">Sample value</param>
        public void Adjust(double sampleValue)
        {
            SamplesStat.AddSampleValue(sampleValue);
            return;
        }

        /// <summary>
        /// The standard normalization
        /// </summary>
        /// <param name="min">Samples min</param>
        /// <param name="max">Samples max</param>
        /// <param name="v">Natural (sample) value to be normalized</param>
        /// <returns>Normalized v</returns>
        private double Normalize(double min, double max, double v)
        {
            return NormRange.Min + NormRange.Span * ((v - min) / (max - min));
        }

        /// <summary>
        /// The standard denormalization
        /// </summary>
        /// <param name="min">Samples min</param>
        /// <param name="max">Samples max</param>
        /// <param name="n">Normalized value</param>
        /// <returns>Natural value</returns>
        private double Naturalize(double min, double max, double n)
        {
            return min + (max - min) * ((n - NormRange.Min) / NormRange.Span);
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
        /// Computes the half of a gausse interval
        /// </summary>
        /// <returns>The half of a gausse interval</returns>
        private double ComputeGausseHalfInterval()
        {
            double gausseLo = Math.Abs((VMin - SamplesStat.ArithAvg) / SamplesStat.StdDev);
            double gausseHi = Math.Abs((VMax - SamplesStat.ArithAvg) / SamplesStat.StdDev);
            return Math.Max(gausseLo, gausseHi);
        }

        /// <summary>
        /// Normalizes the given natural (sample) value
        /// </summary>
        /// <param name="naturalValue">Natural value to be normalized</param>
        /// <returns>Normalized value</returns>
        public double Normalize(double naturalValue)
        {
            //Check readiness
            CheckInitiated();
            //Value preprocessing
            if (Standardization)
            {
                //Gausse standardization
                double gausseHalfInt = ComputeGausseHalfInterval();
                double gausseValue = (naturalValue - SamplesStat.ArithAvg) / SamplesStat.StdDev;
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
        /// Denormalizes the given normalized value
        /// </summary>
        /// <param name="normValue">Normalized value to be denormalized</param>
        /// <returns>Natural (denormalized) value</returns>
        public double Naturalize(double normValue)
        {
            //Check readiness
            CheckInitiated();
            //Value preprocessing
            if (Standardization)
            {
                //Denormalization
                double gausseHalfInt = ComputeGausseHalfInterval();
                double standardizedGausse = Naturalize(-gausseHalfInt, gausseHalfInt, normValue);
                //Destandardization -> natural value
                return (standardizedGausse * SamplesStat.StdDev) + SamplesStat.ArithAvg;
            }
            else
            {
                //Denormalization -> natural value
                return Naturalize(VMin, VMax, normValue);
            }
        }

        /// <summary>
        /// Computes what part of the natural values range is affected by the piece of the normalized range
        /// </summary>
        /// <param name="normSpan">Piece of normalized range</param>
        /// <returns>Piece of natural range</returns>
        public double ComputeNaturalSpan(double normSpan)
        {
            CheckInitiated();
            return ((VMax - VMin) * Math.Abs(normSpan)) * (1 - ReserveRatio);
        }

    }//Normalizer

}//Namespace
