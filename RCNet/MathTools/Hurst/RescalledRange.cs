using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace RCNet.MathTools.Hurst
{
    /// <summary>
    /// Class implements rescalled range computation
    /// </summary>
    [Serializable]
    public class RescalledRange
    {
        //Attributes
        private List<double> _valueCollection;
        private double _sum;

        /// <summary>
        /// Constructs an uninitialized instance
        /// </summary>
        /// <param name="expectedNumOfValues">Expected number of values</param>
        public RescalledRange(int expectedNumOfValues)
        {
            _valueCollection = new List<double>(expectedNumOfValues);
            Reset();
            return;
        }

        /// <summary>
        /// Resets instance to its initial state
        /// </summary>
        public void Reset()
        {
            _valueCollection.Clear();
            _sum = 0;
            return;
        }

        /// <summary>
        /// Adds value
        /// </summary>
        /// <param name="value">Value</param>
        public void AddValue(double value)
        {
            _valueCollection.Add(value);
            _sum += value;
            return;
        }

        /// <summary>
        /// Computes the rescalled range
        /// </summary>
        public double Compute()
        {
            double rescalledRange = 0;
            if (_valueCollection.Count > 0)
            {
                BasicStat devStat = new BasicStat();
                Interval cumulRange = new Interval();
                double mean = _sum / _valueCollection.Count;
                double cumulDeviation = 0;
                for (int i = 0; i < _valueCollection.Count; i++)
                {
                    devStat.AddSampleValue(_valueCollection[i] - mean);
                    cumulDeviation += _valueCollection[i] - mean;
                    cumulRange.Adjust(cumulDeviation);
                }
                if (devStat.StdDev != 0)
                {
                    rescalledRange = (cumulRange.Max - cumulRange.Min) / devStat.StdDev;
                }
            }
            return rescalledRange;
        }
    }//RescalledRange
}//Namespace
