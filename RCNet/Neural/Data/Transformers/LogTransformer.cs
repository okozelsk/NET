﻿using RCNet.Extensions;
using System;
using System.Collections.Generic;

namespace RCNet.Neural.Data.Transformers
{
    /// <summary>
    /// Implements transformation of the input field value to its logarithm of specified base
    /// </summary>
    [Serializable]
    public class LogTransformer : ITransformer
    {

        //Attributes
        private readonly int _fieldIdx;
        private readonly LogTransformerSettings _settings;

        //Constructor
        /// <summary>
        /// Creates an initialized instance
        /// </summary>
        /// <param name="availableFieldNames">Collection of names of all available input fields</param>
        /// <param name="settings">Configuration</param>
        public LogTransformer(List<string> availableFieldNames, LogTransformerSettings settings)
        {
            _settings = (LogTransformerSettings)settings.DeepClone();
            _fieldIdx = availableFieldNames.IndexOf(_settings.InputFieldName);
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
            double arg = Math.Abs(data[_fieldIdx]).Bound();
            if (arg < DoubleExtensions.ReasonableAbsMin)
            {
                arg = DoubleExtensions.ReasonableAbsMin;
            }
            return Math.Log(arg, _settings.Base).Bound();
        }

    }//LogTransformer

}//Namespace
