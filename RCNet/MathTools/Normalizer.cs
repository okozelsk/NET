using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using RCNet.Extensions;

namespace RCNet.MathTools
{
    /// <summary>
    /// Implements thread safe normalizer and denormalizer. Scales input to desired normalized range and vice versa.
    /// Normalizer supports gausse data standardization
    /// </summary>
    [Serializable]
    public class Normalizer
    {
        //Constants
        //Default borders of normalization range
        public const double DefaultNormRangeMin = -1;
        public const double DefaultNormRangeMax = 1;
        //Default is to use gausse standardization during normalization
        public const bool DefaultStandardizeSwitch = true;

        //Attributes
        private bool _standardize;
        private double _reserveRatio;
        private BasicStat _samplesStat;
        private Interval _normRange;

        //Constructors
        /// <summary>
        /// Constructs normalizer as a copy of another normalizer
        /// </summary>
        /// <param name="source">Source normalizer</param>
        public Normalizer(Normalizer source)
        {
            _standardize = source._standardize;
            _reserveRatio = source._reserveRatio;
            _samplesStat = new BasicStat(source._samplesStat);
            _normRange = new Interval(source._normRange);
            return;
        }

        /// <summary>
        /// Constructs normalizer
        /// </summary>
        /// <param name="reserveRatio">Reserved part of known samples range to avoid normalization overflow by future unseen data.</param>
        /// <param name="standardize">If to apply gausse data standardization</param>
        public Normalizer(double reserveRatio, bool standardize = DefaultStandardizeSwitch)
        {
            _standardize = standardize;
            _normRange = new Interval(DefaultNormRangeMin, DefaultNormRangeMax);
            _samplesStat = new BasicStat();
            _reserveRatio = reserveRatio;
            return;
        }

        /// <summary>
        /// Constructs normalizer
        /// </summary>
        /// <param name="normRange">Output range of normalized values</param>
        /// <param name="reserveRatio">Reserved part of known samples range to avoid overflow by future unseen data.</param>
        /// <param name="standardize">If to apply gausse data standardization</param>
        public Normalizer(Interval normRange, double reserveRatio, bool standardize = DefaultStandardizeSwitch)
        {
            _standardize = standardize;
            _normRange = new Interval(normRange);
            _samplesStat = new BasicStat();
            _reserveRatio = reserveRatio;
            return;
        }

        //Properties
        public bool Initialized { get { return (_samplesStat.SamplesCount > 0 && _samplesStat.Min != _samplesStat.Max); } }
        public double ReserveRatio { get { return _reserveRatio; } }
        public bool Standardize { get { return _standardize; } }
        public Interval NormRange { get { return _normRange; } }
        public BasicStat SamplesStat { get { return _samplesStat; } }
        private double VMin { get { return _samplesStat.Min - ((_samplesStat.Span * _reserveRatio) / 2); } }
        private double VMax { get { return _samplesStat.Max + ((_samplesStat.Span * _reserveRatio) / 2); } }

        //Methods
        /// <summary>
        /// Updates normalizer
        /// </summary>
        /// <param name="sampleValue">Sample value</param>
        public void Adjust(double sampleValue)
        {
            _samplesStat.AddSampleValue(sampleValue);
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
            return _normRange.Min + _normRange.Span * ((v - min) / (max - min));
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
            return min + (max - min) * ((n - _normRange.Min) / _normRange.Span);
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
            double gausseLo = Math.Abs((VMin - _samplesStat.ArithAvg) / _samplesStat.StdDev);
            double gausseHi = Math.Abs((VMax - _samplesStat.ArithAvg) / _samplesStat.StdDev);
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
            if (_standardize)
            {
                //Gausse standardization
                double gausseHalfInt = ComputeGausseHalfInterval();
                double gausseValue = (naturalValue - _samplesStat.ArithAvg) / _samplesStat.StdDev;
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
            if (_standardize)
            {
                //Denormalization
                double gausseHalfInt = ComputeGausseHalfInterval();
                double standardizedGausse = Naturalize(-gausseHalfInt, gausseHalfInt, normValue);
                //Destandardization -> natural value
                return (standardizedGausse * _samplesStat.StdDev) + _samplesStat.ArithAvg;
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
            return ((VMax - VMin) * Math.Abs(normError)) * (1 - _reserveRatio);
        }

    }//Normalizer
}//Namespace
