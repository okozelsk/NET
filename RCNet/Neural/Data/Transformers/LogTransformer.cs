using RCNet.Extensions;
using System;
using System.Collections.Generic;

namespace RCNet.Neural.Data.Transformers
{
    /// <summary>
    /// Implements transformation of the input field value to its logarithm of the specified base.
    /// </summary>
    [Serializable]
    public class LogTransformer : ITransformer
    {

        //Attributes
        private readonly int _fieldIdx;
        private readonly LogTransformerSettings _cfg;

        //Constructor
        /// <summary>
        /// Creates an initialized instance.
        /// </summary>
        /// <param name="availableFieldNames">The collection of names of all available input fields.</param>
        /// <param name="cfg">The configuration.</param>
        public LogTransformer(List<string> availableFieldNames, LogTransformerSettings cfg)
        {
            _cfg = (LogTransformerSettings)cfg.DeepClone();
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
            if (arg < DoubleExtensions.ReasonableAbsMin)
            {
                arg = DoubleExtensions.ReasonableAbsMin;
            }
            return Math.Log(arg, _cfg.Base).Bound();
        }

    }//LogTransformer

}//Namespace
