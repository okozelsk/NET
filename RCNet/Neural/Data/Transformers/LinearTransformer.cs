using System;
using System.Collections.Generic;

namespace RCNet.Neural.Data.Transformers
{
    /// <summary>
    /// Implements the "two input fields (a*X + b*Y) linear" transformation
    /// </summary>
    [Serializable]
    public class LinearTransformer : ITransformer
    {

        //Attributes
        private readonly int _xFieldIdx;
        private readonly int _yFieldIdx;
        private readonly LinearTransformerSettings _settings;

        //Constructor
        /// <summary>
        /// Creates an initialized instance
        /// </summary>
        /// <param name="availableFieldNames">Collection of names of all available input fields</param>
        /// <param name="settings">Configuration</param>
        public LinearTransformer(List<string> availableFieldNames, LinearTransformerSettings settings)
        {
            _settings = (LinearTransformerSettings)settings.DeepClone();
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
            return _settings.A * data[_xFieldIdx] + _settings.B * data[_yFieldIdx];
        }

    }//LinearTransformer

}//Namespace
