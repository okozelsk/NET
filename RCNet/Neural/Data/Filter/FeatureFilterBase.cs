using RCNet.MathTools;
using System;

namespace RCNet.Neural.Data.Filter
{
    /// <summary>
    /// Base class of all the feature filters
    /// </summary>
    [Serializable]
    public abstract class FeatureFilterBase
    {
        //Enumerations
        /// <summary>
        /// Type of the feature
        /// </summary>
        public enum FeatureType
        {
            /// <summary>
            /// Feature value is 0 or 1
            /// </summary>
            Binary,
            /// <summary>
            /// Feature value is one of the enumerated values 1..N
            /// </summary>
            Enum,
            /// <summary>
            /// Feature value is the Real number
            /// </summary>
            Real
        }

        //Attribute properties
        /// <inheritdoc cref="FeatureType"/>
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
        protected FeatureFilterBase(FeatureType featureType, Interval outputRange)
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
        /// Resets the filter to its initial state
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
        /// Applies the filter
        /// </summary>
        /// <param name="value">Feature value</param>
        /// <returns>Filter value</returns>
        public virtual double ApplyFilter(double value)
        {
            //Default implementation
            return OutputRange.Min + OutputRange.Span * ((value - FeatureRange.Min) / FeatureRange.Span);
        }

        /// <summary>
        /// Applies the reverse filter
        /// </summary>
        /// <param name="value">Filter value</param>
        /// <returns>Feature value</returns>
        public virtual double ApplyReverse(double value)
        {
            //Default implementation
            return FeatureRange.Min + FeatureRange.Span * ((value - OutputRange.Min) / OutputRange.Span);
        }


    }//FeatureFilterBase

}//Namespace
