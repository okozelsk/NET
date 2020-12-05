using RCNet.Extensions;
using RCNet.MathTools;
using System;

namespace RCNet.Neural.Data.Filter
{
    /// <summary>
    /// Implements the enumeration feature filter
    /// </summary>
    [Serializable]
    public class EnumFeatureFilter : FeatureFilterBase
    {
        //Attribute properties
        /// <summary>
        /// Number of enumeration elements
        /// </summary>
        public int NumOfEnumElements { get; }

        /// <inheritdoc/>
        public override Interval FeatureRange { get; }

        //Constructor
        /// <summary>
        /// Instantiates an initialized instance
        /// </summary>
        /// <param name="outputRange">Filter's output range</param>
        /// <param name="numOfElements">Number of enumeration elements</param>
        public EnumFeatureFilter(Interval outputRange, int numOfElements)
            : base(FeatureType.Enum, outputRange)
        {
            NumOfEnumElements = numOfElements;
            FeatureRange = new Interval(1, NumOfEnumElements);
            return;
        }

        /// <summary>
        /// Instantiates an initialized instance
        /// </summary>
        /// <param name="outputRange">Filter's output range</param>
        /// <param name="settings">Settings class</param>
        public EnumFeatureFilter(Interval outputRange, EnumFeatureFilterSettings settings)
            : this(outputRange, settings.NumOfElements)
        {
            return;
        }

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
            if (sample < 1d || sample > NumOfEnumElements || Math.Ceiling(sample) != sample)
            {
                throw new ArgumentException($"Sample value {sample} is not allowed. Sample value must be an integer value from 1..{NumOfEnumElements}.", "sample");
            }
            base.Update(sample);
            return;
        }

        /// <inheritdoc/>
        public override double ApplyReverse(double value)
        {
            return Math.Round(base.ApplyReverse(value)).Bound(FeatureRange.Min, FeatureRange.Max);
        }

    }//EnumFeatureFilter

}//Namespace
