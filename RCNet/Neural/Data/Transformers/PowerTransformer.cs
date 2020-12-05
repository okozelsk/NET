﻿using RCNet.Extensions;
using System;
using System.Collections.Generic;

namespace RCNet.Neural.Data.Transformers
{
    /// <summary>
    /// Implements the "value^exponent" input field value transformation
    /// </summary>
    [Serializable]
    public class PowerTransformer : ITransformer
    {

        //Attributes
        private readonly int _fieldIdx;
        private readonly PowerTransformerSettings _settings;

        //Constructor
        /// <summary>
        /// Creates an initialized instance
        /// </summary>
        /// <param name="availableFieldNames">Collection of names of all available input fields</param>
        /// <param name="settings">Configuration</param>
        public PowerTransformer(List<string> availableFieldNames, PowerTransformerSettings settings)
        {
            _settings = (PowerTransformerSettings)settings.DeepClone();
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
            double power = Math.Pow(arg, _settings.Exponent);
            if (_settings.KeepSign)
            {
                power = data[_fieldIdx] < 0 ? -1d * power : power;
            }
            return power.Bound();
        }

    }//PowerTransformer

}//Namespace
