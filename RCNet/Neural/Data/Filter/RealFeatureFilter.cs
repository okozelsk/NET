using RCNet.MathTools;
using System;

namespace RCNet.Neural.Data.Filter
{
    /// <summary>
    /// Implements the real number feature filter
    /// </summary>
    [Serializable]
    public class RealFeatureFilter : FeatureFilterBase
    {
        //Constants
        /// <summary>
        /// Standard reserve is 10% of the sampled range
        /// </summary>
        public const double RangeReserveCoeff = 0.1d;

        //Attributes
        private readonly bool _standardize;
        private readonly bool _keepReserve;
        private readonly Interval _range;
        private bool _invalidated;

        //Constructor
        /// <summary>
        /// Instantiates an initialized instance
        /// </summary>
        /// <param name="outputRange">Filter's output range</param>
        /// <param name="standardize">Apply data standardization</param>
        /// <param name="keepReserve">Keep range reserve for future unseen data</param>
        public RealFeatureFilter(Interval outputRange, bool standardize = true, bool keepReserve = true)
            : base(FeatureType.Real, outputRange)
        {
            _standardize = standardize;
            _keepReserve = keepReserve;
            _range = new Interval();
            _invalidated = true;
            return;
        }

        /// <summary>
        /// Instantiates an initialized instance
        /// </summary>
        /// <param name="outputRange">Filter's output range</param>
        /// <param name="settings">Settings class</param>
        public RealFeatureFilter(Interval outputRange, RealFeatureFilterSettings settings)
            : base(FeatureType.Real, outputRange)
        {
            _standardize = settings.Standardize;
            _keepReserve = settings.KeepReserve;
            _range = new Interval();
            _invalidated = true;
            return;
        }

        //Properties
        /// <inheritdoc/>
        public override Interval FeatureRange
        {
            get
            {
                RecomputeRange();
                return _range;
            }
        }

        //Methods
        /// <inheritdoc/>
        public override void Reset()
        {
            base.Reset();
            _invalidated = true;
            return;
        }

        /// <inheritdoc/>
        public override void Update(double sample)
        {
            base.Update(sample);
            _invalidated = true;
            return;
        }

        /// <summary>
        /// Recomputes range
        /// </summary>
        private void RecomputeRange(Interval range, BasicStat stat)
        {
            double min, max;
            if (_standardize)
            {
                double hi = Math.Max(Math.Abs((stat.Min - stat.ArithAvg) / stat.StdDev), Math.Abs((stat.Max - stat.ArithAvg) / stat.StdDev));
                //Following ensures mean at the center of the normalization range but depending on the data
                //it can lead to full utilization of only one half of the normalization interval.
                min = -hi;
                max = hi;
            }
            else
            {
                min = stat.Min;
                max = stat.Max;
            }
            if (_keepReserve)
            {
                double addSpan = ((max - min) / 2d) * RangeReserveCoeff;
                min -= addSpan;
                max += addSpan;
            }
            range.Set(min, max);
            return;
        }


        /// <summary>
        /// Recomputes internal range when invalidated
        /// </summary>
        private void RecomputeRange()
        {
            if (_invalidated)
            {
                RecomputeRange(_range, Stat);
                _invalidated = false;
            }
            return;
        }

        /// <inheritdoc/>
        public override double ApplyFilter(double value)
        {
            RecomputeRange();
            if (_standardize)
            {
                value -= Stat.ArithAvg;
                value /= Stat.StdDev;
            }
            return base.ApplyFilter(value);
        }

        /// <inheritdoc/>
        public override double ApplyReverse(double value)
        {
            RecomputeRange();
            value = base.ApplyReverse(value);
            if (_standardize)
            {
                value *= Stat.StdDev;
                value += Stat.ArithAvg;
            }
            return value;
        }

    }//RealFeatureFilter

}//Namespace
