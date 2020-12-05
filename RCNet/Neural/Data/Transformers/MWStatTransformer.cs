using RCNet.MathTools;
using RCNet.Queue;
using System;
using System.Collections.Generic;

namespace RCNet.Neural.Data.Transformers
{
    /// <summary>
    /// Keeps stat of input field recent values and provides statistical features as a transformed values
    /// </summary>
    [Serializable]
    public class MWStatTransformer : ITransformer
    {
        //Attributes
        private readonly int _fieldIdx;
        private readonly SimpleQueue<double> _lastValues;
        private readonly MWStatTransformerSettings _settings;

        //Constructor
        /// <summary>
        /// Creates an initialized instance
        /// </summary>
        /// <param name="availableFieldNames">Collection of names of all available input fields</param>
        /// <param name="settings">Configuration</param>
        public MWStatTransformer(List<string> availableFieldNames, MWStatTransformerSettings settings)
        {
            _settings = (MWStatTransformerSettings)settings.DeepClone();
            _fieldIdx = availableFieldNames.IndexOf(_settings.InputFieldName);
            if (_fieldIdx == -1)
            {
                throw new InvalidOperationException($"Input field name {_settings.InputFieldName} not found among given available fields.");
            }
            _lastValues = new SimpleQueue<double>(_settings.Window);
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
            if (double.IsNaN(data[_fieldIdx]))
            {
                throw new InvalidOperationException($"Invalid data value at input field index {_fieldIdx} (NaN).");
            }
            _lastValues.Enqueue(data[_fieldIdx], true);
            BasicStat stat = new BasicStat();
            for (int i = 0; i < _lastValues.Count; i++)
            {
                stat.AddSampleValue(_lastValues.GetElementAt(i, true));
            }
            return stat.Get(_settings.Output);
        }

    }//MWStatTransformer

}//Namespace
