using System;
using System.Collections.Generic;

namespace RCNet.Neural.Data.Transformers
{
    /// <summary>
    /// Implements the linear transformation. Uses the values of the two input fields and computes (a*X + b*Y).
    /// </summary>
    [Serializable]
    public class LinearTransformer : ITransformer
    {

        //Attributes
        private readonly int _xFieldIdx;
        private readonly int _yFieldIdx;
        private readonly LinearTransformerSettings _cfg;

        //Constructor
        /// <summary>
        /// Creates an initialized instance.
        /// </summary>
        /// <param name="availableFieldNames">The collection of names of all available input fields.</param>
        /// <param name="settings">The configuration.</param>
        public LinearTransformer(List<string> availableFieldNames, LinearTransformerSettings settings)
        {
            _cfg = (LinearTransformerSettings)settings.DeepClone();
            _xFieldIdx = availableFieldNames.IndexOf(_cfg.XInputFieldName);
            _yFieldIdx = availableFieldNames.IndexOf(_cfg.YInputFieldName);
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
            return _cfg.A * data[_xFieldIdx] + _cfg.B * data[_yFieldIdx];
        }

    }//LinearTransformer

}//Namespace
