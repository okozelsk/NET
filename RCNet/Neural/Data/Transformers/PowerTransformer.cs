using RCNet.Extensions;
using System;
using System.Collections.Generic;

namespace RCNet.Neural.Data.Transformers
{
    /// <summary>
    /// Implements the transformer of values from one input field. Raises the value from the input field to the power of fixed exponent.
    /// </summary>
    [Serializable]
    public class PowerTransformer : ITransformer
    {

        //Attributes
        private readonly int _fieldIdx;
        private readonly PowerTransformerSettings _cfg;

        //Constructor
        /// <summary>
        /// Creates an initialized instance.
        /// </summary>
        /// <param name="availableFieldNames">The collection of names of all available input fields.</param>
        /// <param name="cfg">The configuration.</param>
        public PowerTransformer(List<string> availableFieldNames, PowerTransformerSettings cfg)
        {
            _cfg = (PowerTransformerSettings)cfg.DeepClone();
            _fieldIdx = availableFieldNames.IndexOf(_cfg.InputFieldName);
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
            if (double.IsNaN(data[_fieldIdx]))
            {
                throw new InvalidOperationException($"Invalid data value at input field index {_fieldIdx} (NaN).");
            }

            double arg = Math.Abs(data[_fieldIdx]).Bound();
            double power = Math.Pow(arg, _cfg.Exponent);
            if (_cfg.KeepSign)
            {
                power = data[_fieldIdx] < 0 ? -1d * power : power;
            }
            return power.Bound();
        }

    }//PowerTransformer

}//Namespace
