using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Globalization;

namespace RCNet.MathTools.Hurst
{
    /// <summary>
    /// The class implements the Hurst exponent vector.
    /// </summary>
    [Serializable]
    public class HurstExpVector
    {
        //Attributes
        private HurstExpEstim[] _hurstExpEstimCollection;

        //Constructor
        /// <summary>
        /// Creates an initialized instance
        /// </summary>
        /// <param name="vectorTimeSeries">Time series of fixed length vectors</param>
        public HurstExpVector(List<double[]> vectorTimeSeries)
        {
            //Transform to individual time series
            double[][] indivTSCollection = new double[vectorTimeSeries[0].Length][];
            for(int timeSeriesIdx = 0; timeSeriesIdx < vectorTimeSeries[0].Length; timeSeriesIdx++)
            {
                indivTSCollection[timeSeriesIdx] = new double[vectorTimeSeries.Count];
            }
            for(int row = 0; row < vectorTimeSeries.Count; row++)
            {
                for(int col = 0; col < indivTSCollection.Length; col++)
                {
                    indivTSCollection[col][row] = vectorTimeSeries[row][col];
                }
            }
            _hurstExpEstimCollection = new HurstExpEstim[indivTSCollection.Length];
            for(int i = 0; i < indivTSCollection.Length; i++)
            {
                _hurstExpEstimCollection[i] = new HurstExpEstim(indivTSCollection[i]);
            }
            return;
        }

        //Methods
        /// <summary>
        /// Adds next vector to stored time series of vectors
        /// </summary>
        /// <param name="nextVector">Next vector</param>
        public void AddNextVector(double[] nextVector)
        {
            for(int i = 0; i < _hurstExpEstimCollection.Length; i++)
            {
                _hurstExpEstimCollection[i].AddNextValue(nextVector[i]);
            }
            return;
        }

        /// <summary>
        /// Computes vector of hurst exponent estimations
        /// </summary>
        public double[] ComputeVector()
        {
            double[] vector = new double[_hurstExpEstimCollection.Length];
            for (int i = 0; i < _hurstExpEstimCollection.Length; i++)
            {
                vector[i] = _hurstExpEstimCollection[i].Compute();
            }
            return vector;
        }

        /// <summary>
        /// Computes basic statistics of hurst exponent estimations vector 
        /// </summary>
        public BasicStat ComputeVectorStat()
        {
            return new BasicStat(ComputeVector());
        }

        /// <summary>
        /// Computes Hurst exponent estimation vector for next hypothetical vector of values in time series.
        /// Function does not change the instance, it is a simulation only.
        /// </summary>
        /// <param name="simVector">Next time series vector</param>
        public double[] ComputeNextVector(double[] simVector)
        {
            double[] vector = new double[_hurstExpEstimCollection.Length];
            for (int i = 0; i < _hurstExpEstimCollection.Length; i++)
            {
                vector[i] = _hurstExpEstimCollection[i].ComputeNext(simVector[i]);
            }
            return vector;
        }

        /// <summary>
        /// Computes Hurst exponent estimation vector statistics for next hypothetical vector of values in time series.
        /// Function does not change the instance, it is a simulation only.
        /// </summary>
        /// <param name="simVector">Next time series vector</param>
        public BasicStat ComputeNextVectorStat(double[] simVector)
        {
            return new BasicStat(ComputeNextVector(simVector));
        }

    }//HurstExpVector
}//Namespace
