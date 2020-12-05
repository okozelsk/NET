using RCNet.Extensions;
using System;
using System.Collections.Generic;

namespace RCNet.Neural.Data.Transformers
{
    /// <summary>
    /// Implements the "two input fields division" transformation
    /// </summary>
    [Serializable]
    public class DivTransformer : ITransformer
    {

        //Attributes
        private readonly int _xFieldIdx;
        private readonly int _yFieldIdx;
        private readonly DivTransformerSettings _settings;

        //Constructor
        /// <summary>
        /// Creates an initialized instance
        /// </summary>
        /// <param name="availableFieldNames">Collection of names of all available input fields</param>
        /// <param name="settings">Configuration</param>
        public DivTransformer(List<string> availableFieldNames, DivTransformerSettings settings)
        {
            _settings = (DivTransformerSettings)settings.DeepClone();
            _xFieldIdx = availableFieldNames.IndexOf(_settings.XInputFieldName);
            _yFieldIdx = availableFieldNames.IndexOf(_settings.YInputFieldName);
            return;
        }

        //Methods
        /// <inheritdoc />
        public void Reset()
        {
            return;
        }

        /// <inheritdoc />
        public double Transform(double[] data)
        {
            if (double.IsNaN(data[_xFieldIdx]))
            {
                throw new InvalidOperationException($"Invalid data value at input field index {_xFieldIdx} (NaN).");
            }
            if (double.IsNaN(data[_yFieldIdx]))
            {
                throw new InvalidOperationException($"Invalid data value at input field index {_yFieldIdx} (NaN).");
            }
            double denominator = data[_yFieldIdx].Bound();
            if (Math.Abs(denominator) < DoubleExtensions.ReasonableAbsMin)
            {
                denominator = denominator < 0 ? -1d * DoubleExtensions.ReasonableAbsMin : DoubleExtensions.ReasonableAbsMin;
            }
            return data[_xFieldIdx].Bound() / denominator;
        }

    }//DivTransformer
}//Namespace
