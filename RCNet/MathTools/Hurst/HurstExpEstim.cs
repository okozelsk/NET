using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace RCNet.MathTools.Hurst
{
    /// <summary>
    /// Class implements calculation of the Hurst exponent estimation (rescalled range method)
    /// </summary>
    [Serializable]
    public class HurstExpEstim
    {
        //Constants
        /// <summary>
        /// The smallest interval length of RescalledRange
        /// </summary>
        public const int MinSubIntervalLength = 8;
        
        //Attributes
        private List<double> _valueCollection;
        private List<int> _subIntervalLengthCollection;
        private List<WeightedAvg> _avgCollection;

        /// <summary>
        /// Creates an initialized instance
        /// </summary>
        /// <param name="timeSeries">Time series data</param>
        public HurstExpEstim(IEnumerable<double> timeSeries)
        {
            _valueCollection = timeSeries.ToList();
            //Check time series length
            if(_valueCollection.Count < MinSubIntervalLength + 1)
            {
                throw new ArgumentException($"Time series is too short. Minimal length is {MinSubIntervalLength + 1}", "timeSeries");
            }
            //Subintervals
            _subIntervalLengthCollection = new List<int>((_valueCollection.Count - MinSubIntervalLength) + 1);
            for(int i = 0, length = MinSubIntervalLength; length <= _valueCollection.Count; i++, length++)
            {
                _subIntervalLengthCollection.Add(length);
            }
            _avgCollection = new List<WeightedAvg>(_subIntervalLengthCollection.Count);
            for(int i = 0; i < _subIntervalLengthCollection.Count; i++)
            {
                _avgCollection.Add(new WeightedAvg());
            }
            Parallel.For(0, _subIntervalLengthCollection.Count, _subIntervalIdx =>
            {
                int intervalLength = _subIntervalLengthCollection[_subIntervalIdx];
                RescalledRange rescalledRange = new RescalledRange(intervalLength);
                for (int startIdx = 0; startIdx <= _valueCollection.Count - intervalLength; startIdx++)
                {
                    rescalledRange.Reset();
                    for (int valueSubIdx = 0, timeSeriesIdx = startIdx; valueSubIdx < intervalLength; valueSubIdx++, timeSeriesIdx++)
                    {
                        rescalledRange.AddValue(_valueCollection[timeSeriesIdx]);
                    }
                    _avgCollection[_subIntervalIdx].AddSampleValue(rescalledRange.Compute());
                }
            });
            return;
        }

        //Methods
        /// <summary>
        /// Adds new value to stored time series
        /// </summary>
        /// <param name="nextValue">Next time series value</param>
        public void AddNextValue(double nextValue)
        {
            //Affect next value to existing averages
            Parallel.For(0, _subIntervalLengthCollection.Count, _subIntervalIdx =>
            {
                int intervalLength = _subIntervalLengthCollection[_subIntervalIdx];
                RescalledRange intervalRescalledRange = new RescalledRange(intervalLength);
                for (int valueIdx = (_valueCollection.Count - intervalLength) + 1; valueIdx < _valueCollection.Count; valueIdx++)
                {
                    intervalRescalledRange.AddValue(_valueCollection[valueIdx]);
                }
                intervalRescalledRange.AddValue(nextValue);
                _avgCollection[_subIntervalIdx].AddSampleValue(intervalRescalledRange.Compute());
            });
            //Add new value
            _valueCollection.Add(nextValue);
            _subIntervalLengthCollection.Add(_valueCollection.Count);
            _avgCollection.Add(new WeightedAvg());
            RescalledRange fullRescalledRange = new RescalledRange(_valueCollection.Count);
            foreach (double value in _valueCollection) fullRescalledRange.AddValue(value);
            _avgCollection[_avgCollection.Count - 1].AddSampleValue(fullRescalledRange.Compute());
            return;
        }
        
        /// <summary>
        /// Computes Hurst exponent estimation
        /// Function does not change the instance.
        /// </summary>
        /// <returns></returns>
        public double Compute()
        {
            LinearFit linFit = new LinearFit();
            for(int i = 0; i < _avgCollection.Count; i++)
            {
                double x = Math.Log(_subIntervalLengthCollection[i]);
                double avg = _avgCollection[i].Avg;
                double y = 0;
                if(avg != 0)
                {
                    y = Math.Log(avg);
                }
                linFit.AddSamplePoint(x, y);
            }
            return linFit.A;
        }

        /// <summary>
        /// Computes Hurst exponent estimation for next hypothetical value in time series.
        /// Function does not change the instance, it is a simulation only.
        /// </summary>
        /// <param name="simValue">Next time series value</param>
        public double ComputeNext(double simValue)
        {
            //Affect new value to existing averages
            double[] avgValues = new double[_avgCollection.Count];
            Parallel.For(0, _subIntervalLengthCollection.Count, _subIntervalIdx =>
            {
                int intervalLength = _subIntervalLengthCollection[_subIntervalIdx];
                RescalledRange intervalRescalledRange = new RescalledRange(intervalLength);
                for (int valueIdx = (_valueCollection.Count - intervalLength) + 1; valueIdx < _valueCollection.Count; valueIdx++)
                {
                    intervalRescalledRange.AddValue(_valueCollection[valueIdx]);
                }
                intervalRescalledRange.AddValue(simValue);
                avgValues[_subIntervalIdx] = _avgCollection[_subIntervalIdx].SimulateNext(intervalRescalledRange.Compute());
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
            //Add new point for sub-interval length = (_timeSeries.Length + 1)
            RescalledRange fullRescalledRange = new RescalledRange(_valueCollection.Count + 1);
            foreach(double value in _valueCollection)fullRescalledRange.AddValue(value);
            fullRescalledRange.AddValue(simValue);
            double fullRangeAvg = fullRescalledRange.Compute();
            linFit.AddSamplePoint(Math.Log(_valueCollection.Count + 1), fullRangeAvg == 0 ? 0 : Math.Log(fullRangeAvg));
            //Return
            return linFit.A;
        }

    }//HurstExpEstim

}//Namespace
