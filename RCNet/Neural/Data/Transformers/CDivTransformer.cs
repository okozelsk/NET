using RCNet.Extensions;
using System;
using System.Collections.Generic;

namespace RCNet.Neural.Data.Transformers
{
    /// <summary>
    /// Implements the transformer of values from one input field. Divides the constant by the value from the input field.
    /// </summary>
    [Serializable]
    public class CDivTransformer : ITransformer
    {

        //Attributes
        private readonly int _fieldIdx;
        private readonly CDivTransformerSettings _cfg;

        //Constructor
        /// <summary>
        /// Creates an initialized instance.
        /// </summary>
        /// <param name="availableFieldNames">The collection of names of all available input fields.</param>
        /// <param name="cfg">The configuration.</param>
        public CDivTransformer(List<string> availableFieldNames, CDivTransformerSettings cfg)
        {
            _cfg = (CDivTransformerSettings)cfg.DeepClone();
            _fieldIdx = availableFieldNames.IndexOf(_cfg.InputFieldName);
            return;
        }

        //Methods
        /// <inheritdoc/>
        public void Reset()
        {
            return;
        }

        /// <inheritdoc/>
        public double Transform(double[] data)
        {
            if (double.IsNaN(data[_fieldIdx]))
            {
                throw new InvalidOperationException($"Invalid data value at input field index {_fieldIdx} (NaN).");
            }
            double arg = data[_fieldIdx].Bound();
            if (Math.Abs(arg) < DoubleExtensions.ReasonableAbsMin)
            {
                arg = arg < 0 ? -1d * DoubleExtensions.ReasonableAbsMin : DoubleExtensions.ReasonableAbsMin;
            }
            return (_cfg.C / arg).Bound();
        }

    }//CDivTransformer

}//Namespace
