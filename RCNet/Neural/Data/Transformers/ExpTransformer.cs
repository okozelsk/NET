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
    /// Transforms input field value to Base^value
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
            return Math.Pow(_settings.Base, arg).Bound();
        }

    }//ExpTransformer
}//Namespace
