using System;

namespace RCNet.MathTools
{
    /// <summary>
    /// Class for iterative computation of the weighted average.
    /// </summary>
    [Serializable]
    public class WeightedAvg
    {
        //Attributes
        private double m_sumOfValues;
        private double m_sumOfWeights;
        private double m_avg;
        private int m_numOfSamples;
        
        //Constructors
        /// <summary>
        /// Constructs an unitialized instance
        /// </summary>
        public WeightedAvg()
        {
            Reset();
            return;
        }

        /// <summary>
        /// Deep copy constructor
        /// </summary>
        /// <param name="source">Source instance</param>
        public WeightedAvg(WeightedAvg source)
        {
            Adopt(source);
            return;
        }
        
        //Properties
        /// <summary>
        /// Indicates the readyness
        /// </summary>
        public bool Initialized { get { return (m_numOfSamples > 0); } }
        /// <summary>
        /// Number of considered samples
        /// </summary>
        public int NumOfSamples { get { return m_numOfSamples; } }
        /// <summary>
        /// The weighted average
        /// </summary>
        public double Avg { get { return m_avg; } }
        /// <summary>
        /// The sum of values
        /// </summary>
        public double SumOfValues { get { return m_sumOfValues; } }
        /// <summary>
        /// The sum of weights
        /// </summary>
        public double SumOfWeights { get { return m_sumOfWeights; } }

        //Methods
        /// <summary>
        /// Computes the weighted average value
        /// </summary>
        private double Compute()
        {
            if (m_sumOfWeights != 0 && m_numOfSamples > 0)
            {
                m_avg = m_sumOfValues / m_sumOfWeights;
            }
            else
            {
                m_avg = 0;
            }
            return m_avg;
        }

        /// <summary>
        /// Creates a deep copy of this instance
        /// </summary>
        public WeightedAvg DeepClone()
        {
            return new WeightedAvg(this);
        }

        /// <summary>
        /// Resets the instance to the initial state
        /// </summary>
        public void Reset()
        {
            m_sumOfValues = 0;
            m_sumOfWeights = 0;
            m_avg = 0;
            m_numOfSamples = 0;
            return;
        }

        /// <summary>
        /// Adopts the source instance.
        /// </summary>
        /// <param name="source">Source instance</param>
        public void Adopt(WeightedAvg source)
        {
            m_sumOfValues = source.m_sumOfValues;
            m_sumOfWeights = source.m_sumOfWeights;
            m_avg = source.m_avg;
            m_numOfSamples = source.m_numOfSamples;
            return;
        }

        /// <summary>
        /// Adds the sample value and its weight into the weighted average
        /// </summary>
        /// <param name="value">Value</param>
        /// <param name="weight">Weight</param>
        /// <returns>Weighted average</returns>
        public double AddSampleValue(double value, double weight = 1)
        {
            m_sumOfValues += value * weight;
            m_sumOfWeights += weight;
            ++m_numOfSamples;
            return Compute();
        }

        /// <summary>
        /// Changes or eliminates previously considered sample value
        /// </summary>
        /// <param name="oldValue">Value to be changed</param>
        /// <param name="oldWeight">Weight of the changing value</param>
        /// <param name="newValue">New value</param>
        /// <param name="newWeight">Weight of the new value</param>
        /// <returns>Weighted average</returns>
        public double ChangeSampleValue(double oldValue, double oldWeight, double newValue, double newWeight)
        {
            m_sumOfValues -= (oldValue * oldWeight);
            m_sumOfValues += (newValue * newWeight);
            m_sumOfWeights -= oldWeight;
            m_sumOfWeights += newWeight;
            return Compute();
        }

    }//WeightedAvg
}//Namespace
