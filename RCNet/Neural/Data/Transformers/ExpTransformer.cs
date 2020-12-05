using RCNet.Extensions;
using System;
using System.Collections.Generic;

namespace RCNet.Neural.Data.Transformers
{
    /// <summary>
    /// Implements the "Base^value" transformation of the input field
    /// </summary>
    [Serializable]
    public class ExpTransformer : ITransformer
    {

        //Attributes
        private readonly int _fieldIdx;
        private readonly ExpTransformerSettings _settings;

        //Constructor
        /// <summary>
        /// Creates an initialized instance
        /// </summary>
        /// <param name="availableFieldNames">Collection of names of all available input fields</param>
        /// <param name="settings">Configuration</param>
        public ExpTransformer(List<string> availableFieldNames, ExpTransformerSettings settings)
        {
            _settings = (ExpTransformerSettings)settings.DeepClone();
            _fieldIdx = availableFieldNames.IndexOf(_settings.InputFieldName);
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
            return Math.Pow(_settings.Base, arg).Bound();
        }

    }//ExpTransformer
}//Namespace
