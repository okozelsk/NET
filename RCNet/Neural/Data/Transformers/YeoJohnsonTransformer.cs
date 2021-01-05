using RCNet.Extensions;
using System;
using System.Collections.Generic;

namespace RCNet.Neural.Data.Transformers
{
    /// <summary>
    /// Implements the transformer of values from one input field. Calculates the Yeo-Johnson transformation.
    /// </summary>
    /// <remarks>
    /// For more detailed information see the https://en.wikipedia.org/wiki/Power_transform#Yeo%E2%80%93Johnson_transformation wiki pages or read the  https://www.stat.umn.edu/arc/yjpower.pdf paper.
    /// </remarks>
    [Serializable]
    public class YeoJohnsonTransformer : ITransformer
    {

        //Attributes
        private readonly int _fieldIdx;
        private readonly YeoJohnsonTransformerSettings _cfg;

        //Constructor
        /// <summary>
        /// Creates an initialized instance.
        /// </summary>
        /// <param name="availableFieldNames">The collection of names of all available input fields.</param>
        /// <param name="cfg">The configuration.</param>
        public YeoJohnsonTransformer(List<string> availableFieldNames, YeoJohnsonTransformerSettings cfg)
        {
            _cfg = (YeoJohnsonTransformerSettings)cfg.DeepClone();
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
            double result;
            if (_cfg.Lambda != 0 && arg >= 0)
            {
                result = (Math.Pow(arg + 1d, _cfg.Lambda) - 1d) / _cfg.Lambda;
            }
            else if (_cfg.Lambda == 0 && arg >= 0)
            {
                result = Math.Log(arg + 1d);
            }
            else if (_cfg.Lambda != 2 && arg < 0)
            {
                result = -(Math.Pow(-arg + 1d, 2d - _cfg.Lambda) - 1d) / (2d - _cfg.Lambda);
            }
            else
            {
                result = -Math.Log(-arg + 1d);
            }
            return result.Bound();
        }

    }//YeoJohnsonTransformer

}//Namespace
