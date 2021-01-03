using RCNet.Extensions;
using System;
using System.Collections.Generic;

namespace RCNet.Neural.Data.Transformers
{
    /// <summary>
    /// Implements the exponential transformation of the input field ("Base^Input field value").
    /// </summary>
    [Serializable]
    public class ExpTransformer : ITransformer
    {

        //Attributes
        private readonly int _fieldIdx;
        private readonly ExpTransformerSettings _cfg;

        //Constructor
        /// <summary>
        /// Creates an initialized instance.
        /// </summary>
        /// <param name="availableFieldNames">The collection of names of all available input fields.</param>
        /// <param name="cfg">The configuration.</param>
        public ExpTransformer(List<string> availableFieldNames, ExpTransformerSettings cfg)
        {
            _cfg = (ExpTransformerSettings)cfg.DeepClone();
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
            double arg = data[_fieldIdx].Bound();
            return Math.Pow(_cfg.Base, arg).Bound();
        }

    }//ExpTransformer
}//Namespace
