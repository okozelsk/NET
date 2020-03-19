using RCNet.MathTools;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace RCNet.Neural.Data.Filter
{
    /// <summary>
    /// Base class of feature filters
    /// </summary>
    [Serializable]
    public abstract class BaseFeatureFilter
    {
        //Enumerations
        /// <summary>
        /// Feature type
        /// </summary>
        public enum FeatureType
        {
            /// <summary>
            /// Values 0/1
            /// </summary>
            Binary,
            /// <summary>
            /// Enumeration of values 1..N
            /// </summary>
            Enum,
            /// <summary>
            /// Real numbers
            /// </summary>
            Real
        }

        //Attribute properties
        /// <summary>
        /// Feature type
        /// </summary>
        public FeatureType Type { get; }

        /// <summary>
        /// Samples statistics
        /// </summary>
        public BasicStat Stat { get; }

        /// <summary>
        /// Filter's output range
        /// </summary>
        public Interval OutputRange { get; }


        //Constructor
        /// <summary>
        /// Base constructor
        /// </summary>
        /// <param name="featureType">Feature type</param>
        /// <param name="outputRange">Filter's output range</param>
        protected BaseFeatureFilter(FeatureType featureType, Interval outputRange)
        {
            Type = featureType;
            Stat = new BasicStat();
            OutputRange = outputRange.DeepClone();
            return;
        }

        //Properties
        /// <summary>
        /// Feature range
        /// </summary>
        public abstract Interval FeatureRange { get; }

        //Methods
        /// <summary>
        /// Resets filter to its initial state
        /// </summary>
        public virtual void Reset()
        {
            Stat.Reset();
            return;
        }

        /// <summary>
        /// Updates internal statistics
        /// </summary>
        /// <param name="sample">Feature sample value</param>
        public virtual void Update(double sample)
        {
            Stat.AddSampleValue(sample);
            return;
        }

        /// <summary>
        /// Applies filter
        /// </summary>
        /// <param name="value">Feature value</param>
        /// <returns>Filter value</returns>
        public virtual double ApplyFilter(double value)
        {
            //Default implementation
            return OutputRange.Min + OutputRange.Span * ((value - FeatureRange.Min) / FeatureRange.Span);
        }

        /// <summary>
        /// Applies filter reverse
        /// </summary>
        /// <param name="value">Filter value</param>
        /// <returns>Feature value</returns>
        public virtual double ApplyReverse(double value)
        {
            //Default implementation
            return FeatureRange.Min + FeatureRange.Span * ((value - OutputRange.Min) / OutputRange.Span);
        }


    }//BaseFeatureFilter

}//Namespace
