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
    /// Implements binary feature filter
    /// </summary>
    [Serializable]
    public class BinFeatureFilter : BaseFeatureFilter
    {
        //Static members
        private static readonly Interval _range = new Interval(0d, 1d);
        
        //Constructor
        /// <summary>
        /// Instantiates an initialized instance
        /// </summary>
        /// <param name="outputRange">Filter's output range</param>
        public BinFeatureFilter(Interval outputRange)
            :base(FeatureType.Binary, outputRange)
        {
            return;
        }

        /// <summary>
        /// Instantiates an initialized instance
        /// </summary>
        /// <param name="outputRange">Filter's output range</param>
        /// <param name="settings">Settings class</param>
        public BinFeatureFilter(Interval outputRange, BinFeatureFilterSettings settings)
            : base(FeatureType.Binary, outputRange)
        {
            return;
        }


        //Properties
        /// <summary>
        /// Feature range
        /// </summary>
        public override Interval FeatureRange { get { return _range; } }

        //Methods
        /// <summary>
        /// Updates internal statistics
        /// </summary>
        /// <param name="sample">Feature sample value</param>
        public override void Update(double sample)
        {
            if(sample != _range.Min && sample != _range.Max)
            {
                throw new ArgumentException($"Sample value {sample} is not allowed. Sample value must be {_range.Min} or {_range.Max}.", "sample");
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
            return base.ApplyReverse(value).Bound(_range.Min, _range.Max);
        }

    }//BinFeatureFilter

}//Namespace
