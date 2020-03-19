using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using RCNet.Extensions;
using RCNet.MathTools;

namespace RCNet.Neural.Data.Filter
{
    /// <summary>
    /// Implements enumeration feature filter
    /// </summary>
    [Serializable]
    public class EnumFeatureFilter : BaseFeatureFilter
    {
        //Attribute properties
        /// <summary>
        /// Number of enumeration elements
        /// </summary>
        public int NumOfElements { get; }

        /// <summary>
        /// Feature range
        /// </summary>
        public override Interval FeatureRange { get; }

        //Constructor
        /// <summary>
        /// Instantiates an initialized instance
        /// </summary>
        /// <param name="outputRange">Filter's output range</param>
        /// <param name="numOfElements">Number of enumeration elements</param>
        public EnumFeatureFilter(Interval outputRange, int numOfElements)
            :base(FeatureType.Enum, outputRange)
        {
            NumOfElements = numOfElements;
            FeatureRange = new Interval(0.5d, NumOfElements + 0.5d);
            return;
        }

        /// <summary>
        /// Instantiates an initialized instance
        /// </summary>
        /// <param name="outputRange">Filter's output range</param>
        /// <param name="settings">Settings class</param>
        public EnumFeatureFilter(Interval outputRange, EnumFeatureFilterSettings settings)
            : base(FeatureType.Enum, outputRange)
        {
            NumOfElements = settings.NumOfElements;
            FeatureRange = new Interval(0.5d, NumOfElements + 0.5d);
            return;
        }

        //Methods
        /// <summary>
        /// Resets filter to its initial state
        /// </summary>
        public override void Reset()
        {
            base.Reset();
            return;
        }

        /// <summary>
        /// Updates internal statistics
        /// </summary>
        /// <param name="sample">Feature sample value</param>
        public override void Update(double sample)
        {
            if(sample < 1d || sample > NumOfElements || Math.Ceiling(sample) != sample)
            {
                throw new ArgumentException($"Sample value {sample} is not allowed. Sample value must be an integer value from 1..{NumOfElements}.", "sample");
            }
            base.Update(sample);
            return;
        }


        /// <summary>
        /// Applies filter reverse
        /// </summary>
        /// <param name="value">Filter value</param>
        /// <returns>Feature value</returns>
        public override double ApplyReverse(double value)
        {
            return base.ApplyReverse(value).Bound(FeatureRange.Min, FeatureRange.Max);
        }

    }//EnumFeatureFilter

}//Namespace
