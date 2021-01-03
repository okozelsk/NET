using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace RCNet.MathTools.Hurst
{
    /// <summary>
    /// Implements the Hurst Exponent estimator using the rescaled range analysis.
    /// </summary>
    /// <remarks>
    /// See the https://en.wikipedia.org/wiki/Hurst_exponent.
    /// </remarks>
    [Serializable]
    public class HurstExpEstim
    {
        //Constants
        /// <summary>
        /// The smallest interval length of the rescaledRange.
        /// </summary>
        public const int MinSubIntervalLength = 2;

        //Attributes
        private readonly List<double> _valueCollection;
        private readonly List<int> _subIntervalLengthCollection;
        private readonly List<WeightedAvg> _avgCollection;

        /// <summary>
        /// Creates an initialized instance.
        /// </summary>
        /// <param name="timeSeries">The time series data.</param>
        /// <param name="subIntervalLengthCollection">The collection of the lengths of the rescaled range intervals.</param>
        public HurstExpEstim(IEnumerable<double> timeSeries, List<int> subIntervalLengthCollection)
        {
            _valueCollection = timeSeries.ToList();
            //Check the time series length
            if (_valueCollection.Count < MinSubIntervalLength + 1)
            {
                throw new ArgumentException($"Time series is too short. Minimal length is {MinSubIntervalLength + 1}", "timeSeries");
            }
            //Subintervals
            if (subIntervalLengthCollection != null)
            {
                _subIntervalLengthCollection = new List<int>(subIntervalLengthCollection);
            }
            else
            {
                _subIntervalLengthCollection = new List<int>((_valueCollection.Count - MinSubIntervalLength) + 1);
                for (int i = 0, length = MinSubIntervalLength; length <= _valueCollection.Count; i++, length++)
                {
                    _subIntervalLengthCollection.Add(length);
                }
            }
            _avgCollection = new List<WeightedAvg>(_subIntervalLengthCollection.Count);
            for (int i = 0; i < _subIntervalLengthCollection.Count; i++)
            {
                _avgCollection.Add(new WeightedAvg());
            }
            Parallel.For(0, _subIntervalLengthCollection.Count, _subIntervalIdx =>
            {
                int intervalLength = _subIntervalLengthCollection[_subIntervalIdx];
                RescaledRange rescaledRange = new RescaledRange(intervalLength);
                for (int startIdx = 0; startIdx <= _valueCollection.Count - intervalLength; startIdx++)
                {
                    rescaledRange.Reset();
                    for (int valueSubIdx = 0, timeSeriesIdx = startIdx; valueSubIdx < intervalLength; valueSubIdx++, timeSeriesIdx++)
                    {
                        rescaledRange.AddValue(_valueCollection[timeSeriesIdx]);
                    }
                    _avgCollection[_subIntervalIdx].AddSample(rescaledRange.Compute());
                }
            });
            return;
        }

        //Methods
        /// <summary>
        /// Adds the next value into the stored time series.
        /// </summary>
        /// <param name="nextValue">The next value to be added.</param>
        public void AddNextValue(double nextValue)
        {
            //Add new value
            _valueCollection.Add(nextValue);
            //Affect next value to existing averages
            Parallel.For(0, _subIntervalLengthCollection.Count, _subIntervalIdx =>
            {
                int intervalLength = _subIntervalLengthCollection[_subIntervalIdx];
                RescaledRange intervalRescaledRange = new RescaledRange(intervalLength);
                for (int valueIdx = (_valueCollection.Count - intervalLength); valueIdx < _valueCollection.Count; valueIdx++)
                {
                    intervalRescaledRange.AddValue(_valueCollection[valueIdx]);
                }
                _avgCollection[_subIntervalIdx].AddSample(intervalRescaledRange.Compute());
            });
            return;
        }

        /// <summary>
        /// Estimates the Hurst Exponent.
        /// </summary>
        /// <returns>The resulting linear fit object</returns>
        public LinearFit Compute()
        {
            LinearFit linFit = new LinearFit();
            for (int i = 0; i < _avgCollection.Count; i++)
            {
                double x = Math.Log(_subIntervalLengthCollection[i]);
                double avg = _avgCollection[i].Result;
                double y = 0;
                if (avg != 0)
                {
                    y = Math.Log(avg);
                }
                linFit.AddSamplePoint(x, y);
            }
            return linFit;
        }

        /// <summary>
        /// Estimates the Hurst Exponent, considering the specified hypothetical next value of the already stored time series.
        /// </summary>
        /// <remarks>
        /// Operation does not change the instance data.
        /// </remarks>
        /// <returns>The resulting linear fit object.</returns>
        public LinearFit ComputeNext(double simValue)
        {
            //Affect the simulated next value into the existing averages
            double[] avgValues = new double[_avgCollection.Count];
            Parallel.For(0, _subIntervalLengthCollection.Count, _subIntervalIdx =>
            {
                int intervalLength = _subIntervalLengthCollection[_subIntervalIdx];
                RescaledRange intervalRescaledRange = new RescaledRange(intervalLength);
                for (int valueIdx = (_valueCollection.Count - intervalLength) + 1; valueIdx < _valueCollection.Count; valueIdx++)
                {
                    intervalRescaledRange.AddValue(_valueCollection[valueIdx]);
                }
                intervalRescaledRange.AddValue(simValue);
                avgValues[_subIntervalIdx] = _avgCollection[_subIntervalIdx].SimulateNext(intervalRescaledRange.Compute());
            });
            //Add updated existing points
            LinearFit linFit = new LinearFit();
            for (int i = 0; i < _avgCollection.Count; i++)
            {
                double x = Math.Log(_subIntervalLengthCollection[i]);
                double avg = avgValues[i];
                double y = 0;
                if (avg != 0)
                {
                    y = Math.Log(avg);
                }
                linFit.AddSamplePoint(x, y);
            }
            //Return
            return linFit;
        }

    }//HurstExpEstim

}//Namespace
