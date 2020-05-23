using RCNet.MathTools;
using RCNet.Queue;
using System;
using System.Collections.Generic;

namespace RCNet.Neural.Data.Transformers
{
    /// <summary>
    /// Keeps stat of input field recent values and provides statistical features as a transformed values
    /// </summary>
    [Serializable]
    public class MWStatTransformer : ITransformer
    {
        //Enums
        /// <summary>
        /// Available outputs
        /// </summary>
        public enum OutputValue
        {
            /// <summary>
            /// Sum of values
            /// </summary>
            Sum,
            /// <summary>
            /// Sum of negative values
            /// </summary>
            NegSum,
            /// <summary>
            /// Sum of positive values
            /// </summary>
            PosSum,
            /// <summary>
            /// Sum of squared values
            /// </summary>
            SumOfSquares,
            /// <summary>
            /// Min value
            /// </summary>
            Min,
            /// <summary>
            /// Max value
            /// </summary>
            Max,
            /// <summary>
            /// The center value between min and max
            /// </summary>
            Mid,
            /// <summary>
            /// Span between min and max
            /// </summary>
            Span,
            /// <summary>
            /// Arithmetic average
            /// </summary>
            ArithAvg,
            /// <summary>
            /// Mean of the squared values
            /// </summary>
            MeanSquare,
            /// <summary>
            /// Root of the mean of the squared values
            /// </summary>
            RootMeanSquare,
            /// <summary>
            /// The variance of the values
            /// </summary>
            Variance,
            /// <summary>
            /// The standard deviation of the values
            /// </summary>
            StdDev,
            /// <summary>
            /// The min-max span multiplicated by standard deviation of the values
            /// </summary>
            SpanDev
        }

        //Attributes
        private readonly int _fieldIdx;
        private readonly SimpleQueue<double> _lastValues;
        private readonly MWStatTransformerSettings _settings;

        //Constructor
        /// <summary>
        /// Creates an initialized instance
        /// </summary>
        /// <param name="availableFieldNames">Collection of names of all available input fields</param>
        /// <param name="settings">Configuration</param>
        public MWStatTransformer(List<string> availableFieldNames, MWStatTransformerSettings settings)
        {
            _settings = (MWStatTransformerSettings)settings.DeepClone();
            _fieldIdx = availableFieldNames.IndexOf(_settings.InputFieldName);
            if (_fieldIdx == -1)
            {
                throw new InvalidOperationException($"Input field name {_settings.InputFieldName} not found among given available fields.");
            }
            _lastValues = new SimpleQueue<double>(_settings.Window);
            return;
        }

        //Methods
        /// <summary>
        /// Resets generator to its initial state
        /// </summary>
        public void Reset()
        {
            _lastValues.Reset();
            return;
        }

        /// <summary>
        /// Computes transformed value
        /// </summary>
        /// <param name="data">Collection of natural values of the already known input fields</param>
        public double Next(double[] data)
        {
            if (double.IsNaN(data[_fieldIdx]))
            {
                throw new InvalidOperationException($"Invalid data value at input field index {_fieldIdx} (NaN).");
            }
            _lastValues.Enqueue(data[_fieldIdx], true);
            BasicStat stat = new BasicStat();
            for (int i = 0; i < _lastValues.Count; i++)
            {
                stat.AddSampleValue(_lastValues.GetElementAt(i, true));
            }
            switch (_settings.Output)
            {
                case OutputValue.Sum: return stat.Sum;
                case OutputValue.NegSum: return stat.NegSum;
                case OutputValue.PosSum: return stat.PosSum;
                case OutputValue.SumOfSquares: return stat.SumOfSquares;
                case OutputValue.Min: return stat.Min;
                case OutputValue.Max: return stat.Max;
                case OutputValue.Mid: return stat.Mid;
                case OutputValue.Span: return stat.Span;
                case OutputValue.ArithAvg: return stat.ArithAvg;
                case OutputValue.MeanSquare: return stat.MeanSquare;
                case OutputValue.RootMeanSquare: return stat.RootMeanSquare;
                case OutputValue.Variance: return stat.Variance;
                case OutputValue.StdDev: return stat.StdDev;
                case OutputValue.SpanDev: return stat.SpanDev;
                default: return 0;
            }
        }

    }//MWStatTransformer
}//Namespace
