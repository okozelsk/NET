using System;
using System.Collections.Generic;

namespace RCNet.MathTools.Hurst
{
    /// <summary>
    /// Implements the Rescaled Range.
    /// </summary>
    /// <remarks>
    /// See the https://en.wikipedia.org/wiki/Rescaled_range.
    /// </remarks>
    [Serializable]
    public class RescaledRange
    {
        //Attributes
        private readonly List<double> _valueCollection;
        private double _sum;

        /// <summary>
        /// Creates an uninitialized instance
        /// </summary>
        /// <param name="expectedNumOfValues">An expected number of values.</param>
        public RescaledRange(int expectedNumOfValues)
        {
            _valueCollection = new List<double>(expectedNumOfValues);
            Reset();
            return;
        }

        /// <summary>
        /// Resets the instance to its initial state.
        /// </summary>
        public void Reset()
        {
            _valueCollection.Clear();
            _sum = 0;
            return;
        }

        /// <summary>
        /// Adds a value into the inner collection.
        /// </summary>
        /// <param name="value">The value</param>
        public void AddValue(double value)
        {
            _valueCollection.Add(value);
            _sum += value;
            return;
        }

        /// <summary>
        /// Computes the rescaled range.
        /// </summary>
        public double Compute()
        {
            double rescaledRange = 0;
            if (_valueCollection.Count > 0)
            {
                BasicStat devStat = new BasicStat();
                Interval cumulRange = new Interval();
                double mean = _sum / _valueCollection.Count;
                double cumulDeviation = 0;
                for (int i = 0; i < _valueCollection.Count; i++)
                {
                    devStat.AddSample(_valueCollection[i] - mean);
                    cumulDeviation += _valueCollection[i] - mean;
                    cumulRange.Adjust(cumulDeviation);
                }
                if (devStat.StdDev != 0)
                {
                    rescaledRange = cumulRange.Span / devStat.StdDev;
                }
            }
            return rescaledRange;
        }

    }//RescaledRange

}//Namespace
