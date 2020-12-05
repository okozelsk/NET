using RCNet.Extensions;
using RCNet.MathTools;
using System;

namespace RCNet.Neural.Data.Filter
{
    /// <summary>
    /// Implements the binary feature filter
    /// </summary>
    [Serializable]
    public class BinFeatureFilter : FeatureFilterBase
    {
        //Constructor
        /// <summary>
        /// Instantiates an initialized instance
        /// </summary>
        /// <param name="outputRange">Filter's output range</param>
        public BinFeatureFilter(Interval outputRange)
            : base(FeatureType.Binary, outputRange)
        {
            return;
        }

        /// <summary>
        /// Instantiates an initialized instance
        /// </summary>
        /// <param name="outputRange">Filter's output range</param>
        /// <param name="settings">Settings class</param>
        public BinFeatureFilter(Interval outputRange, BinFeatureFilterSettings settings)
            : this(outputRange)
        {
            return;
        }


        //Properties
        /// <inheritdoc/>
        public override Interval FeatureRange { get { return Interval.IntZP1; } }

        //Methods
        /// <inheritdoc/>
        public override void Reset()
        {
            base.Reset();
            return;
        }

        /// <inheritdoc/>
        public override void Update(double sample)
        {
            if (sample != Interval.IntZP1.Min && sample != Interval.IntZP1.Max)
            {
                throw new ArgumentException($"Sample value {sample} is not allowed. Sample value must be {Interval.IntZP1.Min} or {Interval.IntZP1.Max}.", "sample");
            }
            base.Update(sample);
            return;
        }

        /// <inheritdoc/>
        public override double ApplyReverse(double value)
        {
            return Math.Round(base.ApplyReverse(value)).Bound(FeatureRange.Min, FeatureRange.Max);
        }

    }//BinFeatureFilter

}//Namespace
