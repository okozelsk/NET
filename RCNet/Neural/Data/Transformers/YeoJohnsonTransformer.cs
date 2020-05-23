using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using RCNet.Extensions;
using RCNet.RandomValue;
using RCNet.Queue;
using System.Globalization;

namespace RCNet.Neural.Data.Transformers
{
    /// <summary>
    /// Applies Yeo-Johnson transformation to input field value
    /// </summary>
    [Serializable]
    public class YeoJohnsonTransformer : ITransformer
    {

        //Attributes
        private readonly int _fieldIdx;
        private readonly YeoJohnsonTransformerSettings _settings;

        //Constructor
        /// <summary>
        /// Creates an initialized instance
        /// </summary>
        /// <param name="availableFieldNames">Collection of names of all available input fields</param>
        /// <param name="settings">Configuration</param>
        public YeoJohnsonTransformer(List<string> availableFieldNames, YeoJohnsonTransformerSettings settings)
        {
            _settings = (YeoJohnsonTransformerSettings)settings.DeepClone();
            _fieldIdx = availableFieldNames.IndexOf(_settings.InputFieldName);
            return;
        }

        //Methods
        /// <summary>
        /// Resets generator to its initial state
        /// </summary>
        public void Reset()
        {
            return;
        }

        /// <summary>
        /// Computes transformed value
        /// </summary>
        /// <param name="data">Collection of natural values of the already known input fields</param>
        public double Next(double[] data)
        {
            if(double.IsNaN(data[_fieldIdx]))
            {
                throw new InvalidOperationException($"Invalid data value at input field index {_fieldIdx} (NaN).");
            }
            double arg = data[_fieldIdx].Bound();
            double result;
            if(_settings.Lambda != 0 && arg >= 0)
            {
                result = (Math.Pow(arg + 1d, _settings.Lambda) - 1d) / _settings.Lambda;
            }
            else if(_settings.Lambda == 0 && arg >= 0)
            {
                result = Math.Log(arg + 1d);
            }
            else if (_settings.Lambda != 2 && arg < 0)
            {
                result = -(Math.Pow(-arg + 1d, 2d - _settings.Lambda) - 1d) / (2d - _settings.Lambda);
            }
            else
            {
                result = -Math.Log(-arg + 1d);
            }
            return result.Bound();
        }

    }//YeoJohnsonTransformer
}//Namespace
