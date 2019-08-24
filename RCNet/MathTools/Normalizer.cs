using System;

namespace RCNet.MathTools
{
    /// <summary>
    /// Implements thread safe (if required) normalizer and denormalizer. Scales the input to desired normalized range and vice versa.
    /// Normalizer supports data standardization.
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
        /// <param name="reserveRatio">Reserve held by a normalizer to cover cases where future data exceeds a known range of sample data.</param>
        /// <param name="standardize">Specifies whether to apply data standardization before normalization. </param>
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
        /// <param name="reserveRatio">Reserve held by a normalizer to cover cases where future data exceeds a known range of sample data.</param>
        /// <param name="standardize">Specifies whether to apply data standardization before normalization. </param>
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

        //Methods
        /// <summary>
        /// Checks normalizer readyness
        /// </summary>
        private void CheckReadiness()
        {
            if (!Initialized)
            {
                throw new Exception("Not properly initialized");
            }
            return;
        }

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
        /// Standardizes the given natural (sample) value
        /// </summary>
        /// <param name="naturalValue">Natural value to be standardized</param>
        /// <returns>Standardized value (z-skore)</returns>
        public double Standardize(double naturalValue)
        {
            //Check readiness
            CheckReadiness();
            return (naturalValue - SamplesStat.ArithAvg) / SamplesStat.StdDev;
        }

        /// <summary>
        /// Naturalizes the given standardized value
        /// </summary>
        /// <param name="normValue">Standardized value (z-score) to be naturalized</param>
        /// <returns>Destandardized (natural) value</returns>
        public double Destandardize(double standardizedValue)
        {
            //Check readiness
            CheckReadiness();
            return SamplesStat.ArithAvg + standardizedValue * SamplesStat.StdDev;
        }

        /// <summary>
        /// Normalizes the given natural (sample) value
        /// </summary>
        /// <param name="naturalValue">Natural value to be normalized</param>
        /// <returns>Normalized value</returns>
        public double Normalize(double naturalValue)
        {
            //Check readiness
            CheckReadiness();
            //Preprocessing
            double min, max, val;
            if(Standardization)
            {

                double hi = Math.Max(Math.Abs((SamplesStat.Min - SamplesStat.ArithAvg) / SamplesStat.StdDev), Math.Abs((SamplesStat.Max - SamplesStat.ArithAvg) / SamplesStat.StdDev));
                min = -hi;
                max = hi;
                val = (naturalValue - SamplesStat.ArithAvg) / SamplesStat.StdDev;
            }
            else
            {
                min = SamplesStat.Min;
                max = SamplesStat.Max;
                val = naturalValue;
            }
            val *= (1 - ReserveRatio);
            return NormRange.Min + NormRange.Span * ((val - min) / (max - min));
        }

        /// <summary>
        /// Naturalizes the given normalized value
        /// </summary>
        /// <param name="normValue">Normalized value to be denormalized</param>
        /// <returns>Denormalized (natural) value</returns>
        public double Denormalize(double normValue)
        {
            //Check readiness
            CheckReadiness();
            //Preprocessing
            double min, max, val;
            if (Standardization)
            {
                double hi = Math.Max(Math.Abs((SamplesStat.Min - SamplesStat.ArithAvg) / SamplesStat.StdDev), Math.Abs((SamplesStat.Max - SamplesStat.ArithAvg) / SamplesStat.StdDev));
                min = -hi;
                max = hi;
            }
            else
            {
                min = SamplesStat.Min;
                max = SamplesStat.Max;
            }
            val = (min + (max - min) * ((normValue - NormRange.Min) / NormRange.Span)) / (1d - ReserveRatio);
            if (Standardization)
            {
                val = SamplesStat.ArithAvg + val * SamplesStat.StdDev;
            }
            return val;
        }

    }//Normalizer

}//Namespace
