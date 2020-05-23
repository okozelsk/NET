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
    /// Transformation "constant divided by an input value"
    /// </summary>
    [Serializable]
    public class CDivTransformer : ITransformer
    {

        //Attributes
        private readonly int _fieldIdx;
        private readonly CDivTransformerSettings _settings;

        //Constructor
        /// <summary>
        /// Creates an initialized instance
        /// </summary>
        /// <param name="availableFieldNames">Collection of names of all available input fields</param>
        /// <param name="settings">Configuration</param>
        public CDivTransformer(List<string> availableFieldNames, CDivTransformerSettings settings)
        {
            _settings = (CDivTransformerSettings)settings.DeepClone();
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
            if (Math.Abs(arg) < DoubleExtensions.ReasonableAbsMin)
            {
                arg = arg < 0 ? -1d * DoubleExtensions.ReasonableAbsMin : DoubleExtensions.ReasonableAbsMin;
            }
            return (_settings.C / arg).Bound();
        }

    }//CDivTransformer
}//Namespace
