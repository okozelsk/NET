using RCNet.MathTools;
using RCNet.Queue;
using System;
using System.Collections.Generic;

namespace RCNet.Neural.Data.Transformers
{
    /// <summary>
    /// Implements the statistical transformation. It keeps statistics of the input field recent values and provides specified statistical figure as the transformed value.
    /// </summary>
    [Serializable]
    public class MWStatTransformer : ITransformer
    {
        //Attributes
        private readonly int _fieldIdx;
        private readonly SimpleQueue<double> _lastValues;
        private readonly MWStatTransformerSettings _cfg;

        //Constructor
        /// <summary>
        /// Creates an initialized instance.
        /// </summary>
        /// <param name="availableFieldNames">The collection of names of all available input fields.</param>
        /// <param name="cfg">The configuration.</param>
        public MWStatTransformer(List<string> availableFieldNames, MWStatTransformerSettings cfg)
        {
            _cfg = (MWStatTransformerSettings)cfg.DeepClone();
            _fieldIdx = availableFieldNames.IndexOf(_cfg.InputFieldName);
            if (_fieldIdx == -1)
            {
                throw new InvalidOperationException($"Input field name {_cfg.InputFieldName} not found among given available fields.");
            }
            _lastValues = new SimpleQueue<double>(_cfg.WindowSize);
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
                stat.AddSample(_lastValues.GetElementAt(i, true));
            }
            return stat.Get(_cfg.OutputFigure);
        }

    }//MWStatTransformer

}//Namespace
