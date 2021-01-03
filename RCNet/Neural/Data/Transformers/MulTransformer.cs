using RCNet.Extensions;
using System;
using System.Collections.Generic;

namespace RCNet.Neural.Data.Transformers
{
    /// <summary>
    /// Implements the multiplication transformation. It multiplies the value of the first field by the value of the second field.
    /// </summary>
    [Serializable]
    public class MulTransformer : ITransformer
    {

        //Attributes
        private readonly int _xFieldIdx;
        private readonly int _yFieldIdx;
        private readonly MulTransformerSettings _cfg;

        //Constructor
        /// <summary>
        /// Creates an initialized instance.
        /// </summary>
        /// <param name="availableFieldNames">The collection of names of all available input fields.</param>
        /// <param name="cfg">The configuration.</param>
        public MulTransformer(List<string> availableFieldNames, MulTransformerSettings cfg)
        {
            _cfg = (MulTransformerSettings)cfg.DeepClone();
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
            return data[_xFieldIdx].Bound() * data[_yFieldIdx].Bound();
        }

    }//MulTransformer

}//Namespace
