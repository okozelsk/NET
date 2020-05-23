using RCNet.Queue;
using System;
using System.Collections.Generic;

namespace RCNet.Neural.Data.Transformers
{
    /// <summary>
    /// Transforms input field value as a difference between current value and a past value
    /// </summary>
    [Serializable]
    public class DiffTransformer : ITransformer
    {

        //Attributes
        private readonly int _fieldIdx;
        private readonly SimpleQueue<double> _lastValues;
        private readonly DiffTransformerSettings _settings;

        //Constructor
        /// <summary>
        /// Creates an initialized instance
        /// </summary>
        /// <param name="availableFieldNames">Collection of names of all available input fields</param>
        /// <param name="settings">Configuration</param>
        public DiffTransformer(List<string> availableFieldNames, DiffTransformerSettings settings)
        {
            _settings = (DiffTransformerSettings)settings.DeepClone();
            _fieldIdx = availableFieldNames.IndexOf(_settings.InputFieldName);
            if (_fieldIdx == -1)
            {
                throw new InvalidOperationException($"Input field name {_settings.InputFieldName} not found among given available fields.");
            }
            _lastValues = new SimpleQueue<double>(_settings.Interval);
            return;
        }

        //Methods
        /// <summary>
        /// Resets generator to its initial state
        /// </summary>
        public void Reset()
        {
            _lastValues.Reset();
            return;
        }

        /// <summary>
        /// Computes transformed value
        /// </summary>
        /// <param name="data">Collection of natural values of the already known input fields</param>
        public double Next(double[] data)
        {
            double transVal = 0d;
            if (double.IsNaN(data[_fieldIdx]))
            {
                throw new InvalidOperationException($"Invalid data value at input field index {_fieldIdx} (NaN).");
            }
            if (_lastValues.Full)
            {
                //Moving data window is ready
                transVal = data[_fieldIdx] - _lastValues.GetElementAt(_settings.Interval - 1, true);
            }
            _lastValues.Enqueue(data[_fieldIdx], true);
            return transVal;
        }

    }//DiffTransformer
}//Namespace
