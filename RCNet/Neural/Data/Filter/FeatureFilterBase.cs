using RCNet.MathTools;
using System;

namespace RCNet.Neural.Data.Filter
{
    /// <summary>
    /// Implements the base class of all the feature filters.
    /// </summary>
    [Serializable]
    public abstract class FeatureFilterBase
    {
        //Enumerations
        /// <summary>
        /// The feature type.
        /// </summary>
        public enum FeatureType
        {
            /// <summary>
            /// The feature value is 0 or 1.
            /// </summary>
            Binary,
            /// <summary>
            /// The feature value is the real number.
            /// </summary>
            Real
        }

        //Attribute properties
        /// <inheritdoc cref="FeatureType"/>
        public FeatureType Type { get; }

        /// <summary>
        /// The statistics of the samples.
        /// </summary>
        public BasicStat Stat { get; }

        /// <summary>
        /// The filter's output range.
        /// </summary>
        public Interval OutputRange { get; }


        //Constructor
        /// <summary>
        /// Base constructor
        /// </summary>
        /// <param name="featureType">The feature type.</param>
        /// <param name="outputRange">The filter's output range.</param>
        protected FeatureFilterBase(FeatureType featureType, Interval outputRange)
        {
            Type = featureType;
            Stat = new BasicStat();
            OutputRange = outputRange.DeepClone();
            return;
        }

        //Properties
        /// <summary>
        /// The feature range.
        /// </summary>
        public abstract Interval FeatureRange { get; }

        //Methods
        /// <summary>
        /// Resets the filter to its initial state.
        /// </summary>
        public virtual void Reset()
        {
            Stat.Reset();
            return;
        }

        /// <summary>
        /// Updates the inner statistics.
        /// </summary>
        /// <param name="sample">The sample.</param>
        public virtual void Update(double sample)
        {
            Stat.AddSample(sample);
            return;
        }

        /// <summary>
        /// Applies the filter.
        /// </summary>
        /// <param name="value">The value.</param>
        /// <returns>The result of the filter application.</returns>
        public virtual double ApplyFilter(double value)
        {
            //Default implementation
            return OutputRange.Min + OutputRange.Span * ((value - FeatureRange.Min) / FeatureRange.Span);
        }

        /// <summary>
        /// Applies the filter reverse.
        /// </summary>
        /// <param name="value">The result of the filter application.</param>
        /// <returns>The value.</returns>
        public virtual double ApplyReverse(double value)
        {
            //Default implementation
            return FeatureRange.Min + FeatureRange.Span * ((value - OutputRange.Min) / OutputRange.Span);
        }


    }//FeatureFilterBase

}//Namespace
