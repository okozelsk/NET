using RCNet.Queue;
using System;
using System.Collections.Generic;

namespace RCNet.Neural.Data.Transformers
{
    /// <summary>
    /// Implements the transformer of values from one input field. Subtracts the previous value of the input field from the current value.
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
        /// Creates an initialized instance.
        /// </summary>
        /// <param name="availableFieldNames">The collection of names of all available input fields.</param>
        /// <param name="settings">The configuration.</param>
        public DiffTransformer(List<string> availableFieldNames, DiffTransformerSettings settings)
        {
            _settings = (DiffTransformerSettings)settings.DeepClone();
            _fieldIdx = availableFieldNames.IndexOf(_settings.InputFieldName);
            if (_fieldIdx == -1)
            {
                throw new InvalidOperationException($"Input field name {_settings.InputFieldName} not found among given available fields.");
            }
            _lastValues = new SimpleQueue<double>(_settings.PastInterval);
            return;
        }

        //Methods
        /// <inheritdoc />
        public void Reset()
        {
            _lastValues.Reset();
            return;
        }

        /// <inheritdoc />
        public double Transform(double[] data)
        {
            double transVal = 0d;
            if (double.IsNaN(data[_fieldIdx]))
            {
                throw new InvalidOperationException($"Invalid data value at input field index {_fieldIdx} (NaN).");
            }
            if (_lastValues.Full)
            {
                //Moving data window is ready
                transVal = data[_fieldIdx] - _lastValues.GetElementAt(_settings.PastInterval - 1, true);
            }
            _lastValues.Enqueue(data[_fieldIdx], true);
            return transVal;
        }

    }//DiffTransformer

}//Namespace
