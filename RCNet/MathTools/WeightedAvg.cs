using System;

namespace RCNet.MathTools
{
    /// <summary>
    /// Class implements iterative computation of the weighted average.
    /// </summary>
    [Serializable]
    public class WeightedAvg
    {
        //Attribute properties
        /// <summary>
        /// Number of considered samples
        /// </summary>
        public int NumOfSamples { get; private set; }
        /// <summary>
        /// The weighted average
        /// </summary>
        public double Avg { get; private set; }
        /// <summary>
        /// The sum of values
        /// </summary>
        public double SumOfValues { get; private set; }
        /// <summary>
        /// The sum of weights
        /// </summary>
        public double SumOfWeights { get; private set; }

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
        public bool Initialized { get { return (NumOfSamples > 0); } }

        //Methods
        /// <summary>
        /// Computes the weighted average value
        /// </summary>
        private double Compute()
        {
            if (SumOfWeights != 0 && NumOfSamples > 0)
            {
                Avg = SumOfValues / SumOfWeights;
            }
            else
            {
                Avg = 0;
            }
            return Avg;
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
            SumOfValues = 0;
            SumOfWeights = 0;
            Avg = 0;
            NumOfSamples = 0;
            return;
        }

        /// <summary>
        /// Adopts the source instance.
        /// </summary>
        /// <param name="source">Source instance</param>
        public void Adopt(WeightedAvg source)
        {
            SumOfValues = source.SumOfValues;
            SumOfWeights = source.SumOfWeights;
            Avg = source.Avg;
            NumOfSamples = source.NumOfSamples;
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
            SumOfValues += value * weight;
            SumOfWeights += weight;
            ++NumOfSamples;
            return Compute();
        }

        /// <summary>
        /// Removes the sample value and its weight from the weighted average
        /// </summary>
        /// <param name="value">Value</param>
        /// <param name="weight">Weight</param>
        /// <returns>Weighted average</returns>
        public double RemoveSampleValue(double value, double weight = 1)
        {
            if (NumOfSamples > 0)
            {
                SumOfValues -= value * weight;
                SumOfWeights -= weight;
                --NumOfSamples;
                if(NumOfSamples == 0)
                {
                    SumOfValues = 0;
                    SumOfWeights = 0;
                }
            }
            else
            {
                throw new Exception("Can't remove sample value because there is no samples.");
            }
            return Compute();
        }

        /// <summary>
        /// Function computes weighted average for next hypothetical sample value.
        /// Function does not change instance, it is a simulation only.
        /// </summary>
        /// <param name="simValue">Next hypothetical sample value</param>
        /// <param name="simWeight">Next hypothetical sample value weight</param>
        /// <returns>Weighted average</returns>
        public double SimulateNext(double simValue, double simWeight = 1)
        {
            return (SumOfValues + (simValue * simWeight)) / (SumOfWeights + simWeight);
        }



    }//WeightedAvg
}//Namespace
