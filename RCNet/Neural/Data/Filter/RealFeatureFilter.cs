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
    public class RealFeatureFilter : BaseFeatureFilter
    {
        //Constants
        /// <summary>
        /// Standard reserve is 10% of the sampled range
        /// </summary>
        public const double RangeReserveCoeff = 1.1d;

        //Attribute properties
        /// <summary>
        /// Positive samples statistics
        /// </summary>
        public BasicStat PositiveStat { get; }

        /// <summary>
        /// Negative samples statistics
        /// </summary>
        public BasicStat NegativeStat { get; }

        //Attributes
        private readonly bool _standardize;
        private readonly bool _keepReserve;
        private readonly bool _keepSign;
        private readonly Interval _range;
        private readonly Interval _positiveRange;
        private readonly Interval _negativeRange;
        private bool _invalidated;

        //Constructor
        /// <summary>
        /// Instantiates an initialized instance
        /// </summary>
        /// <param name="outputRange">Filter's output range</param>
        /// <param name="standardize">Apply data standardization</param>
        /// <param name="keepReserve">Keep range reserve for future unseen data</param>
        /// <param name="keepSign">Original sign will be kept</param>
        public RealFeatureFilter(Interval outputRange, bool standardize = true, bool keepReserve = true, bool keepSign = false)
            :base(FeatureType.Real, outputRange)
        {
            PositiveStat = new BasicStat();
            NegativeStat = new BasicStat();
            _standardize = standardize;
            _keepReserve = keepReserve;
            _keepSign = keepSign;
            _range = new Interval();
            _positiveRange = new Interval();
            _negativeRange = new Interval();
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
            PositiveStat = new BasicStat();
            NegativeStat = new BasicStat();
            _standardize = settings.Standardize;
            _keepReserve = settings.KeepReserve;
            _keepSign = settings.KeepSign;
            _range = new Interval();
            _positiveRange = new Interval();
            _negativeRange = new Interval();
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

        /// <summary>
        /// Feature positive range
        /// </summary>
        public Interval FeaturePositiveRange
        {
            get
            {
                RecomputeRange();
                return _positiveRange;
            }
        }

        /// <summary>
        /// Feature negative range
        /// </summary>
        public Interval FeatureNegativeRange
        {
            get
            {
                RecomputeRange();
                return _negativeRange;
            }
        }

        //Methods
        /// <summary>
        /// Resets filter to its initial state
        /// </summary>
        public override void Reset()
        {
            PositiveStat.Reset();
            NegativeStat.Reset();
            base.Reset();
            _invalidated = true;
            return;
        }

        /// <summary>
        /// Updates internal statistics
        /// </summary>
        /// <param name="sample">Feature sample value</param>
        public override void Update(double sample)
        {
            base.Update(sample);
            if (sample > 0)
            {
                PositiveStat.AddSampleValue(sample);
            }
            else if (sample < 0)
            {
                NegativeStat.AddSampleValue(Math.Abs(sample));
            }
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
                min *= RangeReserveCoeff;
                max *= RangeReserveCoeff;
            }
            range.Set(min, max);
            return;
        }


        /// <summary>
        /// Recomputes internal range when invalidated
        /// </summary>
        private void RecomputeRange()
        {
            if(_invalidated)
            {
                RecomputeRange(_range, Stat);
                RecomputeRange(_positiveRange, PositiveStat);
                RecomputeRange(_negativeRange, NegativeStat);
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
            if (!_keepSign)
            {
                if (_standardize)
                {
                    value -= Stat.ArithAvg;
                    value /= Stat.StdDev;
                }
                return base.ApplyFilter(value);
            }
            else
            {
                if (value == 0d)
                {
                    return OutputRange.Mid;
                }
                else if (value > 0d)
                {
                    //Positive
                    value -= PositiveStat.ArithAvg;
                    value /= PositiveStat.StdDev;
                    return OutputRange.Mid + (OutputRange.Span / 2d) * ((value - _positiveRange.Min) / _positiveRange.Span);

                }
                else
                {
                    //Negative
                    value = Math.Abs(value);
                    value -= NegativeStat.ArithAvg;
                    value /= NegativeStat.StdDev;
                    return OutputRange.Mid - (OutputRange.Span / 2d) * ((value - _negativeRange.Min) / _negativeRange.Span);
                }
            }
        }

        /// <summary>
        /// Applies filter reverse
        /// </summary>
        /// <param name="value">Filter value</param>
        /// <returns>Feature value</returns>
        public override double ApplyReverse(double value)
        {
            RecomputeRange();
            if (!_keepSign)
            {
                value = base.ApplyReverse(value);
                if (_standardize)
                {
                    value *= Stat.StdDev;
                    value += Stat.ArithAvg;
                }
                return value;
            }
            else
            {
                if (value == OutputRange.Mid)
                {
                    return 0d;
                }
                else if (value > OutputRange.Mid)
                {
                    //Positive
                    value = _positiveRange.Min + _positiveRange.Span * ((value - OutputRange.Mid) / (OutputRange.Span / 2d));
                    if (_standardize)
                    {
                        value *= PositiveStat.StdDev;
                        value += PositiveStat.ArithAvg;
                    }
                    return value;
                }
                else
                {
                    //Negative
                    value = _negativeRange.Min + _negativeRange.Span * ((Math.Abs(value) - OutputRange.Mid) / (OutputRange.Span / 2d));
                    if (_standardize)
                    {
                        value *= NegativeStat.StdDev;
                        value += NegativeStat.ArithAvg;
                    }
                    return -value;
                }
            }
        }

    }//RealFeatureFilter

}//Namespace
