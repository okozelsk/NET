using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using RCNet.MathTools;

namespace RCNet.Neural.Data.Filter
{
    /// <summary>
    /// Implements real number feature filter
    /// </summary>
    [Serializable]
    public class RealFeatureFilter : FeatureFilter
    {
        //Constants
        /// <summary>
        /// Standard reserve is 10% of the sampled range
        /// </summary>
        public const double RangeReserveCoeff = 1.1d;

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
            :base(FeatureType.Real, outputRange)
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
        /// <summary>
        /// Feature range
        /// </summary>
        public override Interval FeatureRange
        {
            get
            {
                RecomputeRange();
                return _range;
            }
        }

        //Methods
        /// <summary>
        /// Updates internal statistics
        /// </summary>
        /// <param name="sample">Feature sample value</param>
        public override void Update(double sample)
        {
            base.Update(sample);
            _invalidated = true;
            return;
        }

        /// <summary>
        /// Recomputes internal range when Stat has changed
        /// </summary>
        private void RecomputeRange()
        {
            if(_invalidated)
            {
                double min, max;
                if(_standardize)
                {
                    double hi = Math.Max(Math.Abs((Stat.Min - Stat.ArithAvg) / Stat.StdDev), Math.Abs((Stat.Max - Stat.ArithAvg) / Stat.StdDev));
                    min = -hi;
                    max = hi;
                }
                else
                {
                    min = Stat.Min;
                    max = Stat.Max;
                }
                if(_keepReserve)
                {
                    min *= RangeReserveCoeff;
                    max *= RangeReserveCoeff;
                }
                _range.Set(min, max);
                _invalidated = false;
            }
            return;
        }

        /// <summary>
        /// Applies filter
        /// </summary>
        /// <param name="value">Feature value</param>
        /// <returns>Filter value</returns>
        public override double ApplyFilter(double value)
        {
            RecomputeRange();
            if(_standardize)
            {
                value -= Stat.ArithAvg;
                value /= Stat.StdDev;
            }
            return base.ApplyFilter(value);
        }

        /// <summary>
        /// Applies filter reverse
        /// </summary>
        /// <param name="value">Filter value</param>
        /// <returns>Feature value</returns>
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
