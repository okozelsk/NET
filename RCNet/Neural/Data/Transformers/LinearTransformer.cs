using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using RCNet.Extensions;
using RCNet.RandomValue;
using RCNet.Queue;

namespace RCNet.Neural.Data.Transformers
{
    /// <summary>
    /// Two input fields linear transformation
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
            if(double.IsNaN(data[_xFieldIdx]))
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
