using System;

namespace RCNet.MathTools
{
    /// <summary>
    /// Implements the weighted average.
    /// </summary>
    [Serializable]
    public class WeightedAvg
    {
        //Attribute properties
        /// <summary>
        /// The number of samples.
        /// </summary>
        public int NumOfSamples { get; private set; }

        /// <summary>
        /// The sum of samples.
        /// </summary>
        public double SumOfSamples { get; private set; }

        /// <summary>
        /// The sum of weights.
        /// </summary>
        public double SumOfWeights { get; private set; }

        /// <summary>
        /// The computed weighted average.
        /// </summary>
        public double Result { get; private set; }

        //Constructors
        /// <summary>
        /// Creates an unitialized instance.
        /// </summary>
        public WeightedAvg()
        {
            Reset();
            return;
        }

        /// <summary>
        /// The copy constructor.
        /// </summary>
        /// <param name="source">The source instance.</param>
        public WeightedAvg(WeightedAvg source)
        {
            Adopt(source);
            return;
        }

        //Methods
        /// <summary>
        /// Computes the resulting weighted average.
        /// </summary>
        private double Compute()
        {
            if (SumOfWeights != 0 && NumOfSamples > 0)
            {
                Result = SumOfSamples / SumOfWeights;
            }
            else
            {
                Result = 0;
            }
            return Result;
        }

        /// <summary>
        /// Creates the deep copy of this instance.
        /// </summary>
        public WeightedAvg DeepClone()
        {
            return new WeightedAvg(this);
        }

        /// <summary>
        /// Resets the instance.
        /// </summary>
        public void Reset()
        {
            SumOfSamples = 0;
            SumOfWeights = 0;
            Result = 0;
            NumOfSamples = 0;
            return;
        }

        /// <summary>
        /// Adopts the data from source instance.
        /// </summary>
        /// <param name="source">The source instance.</param>
        public void Adopt(WeightedAvg source)
        {
            SumOfSamples = source.SumOfSamples;
            SumOfWeights = source.SumOfWeights;
            Result = source.Result;
            NumOfSamples = source.NumOfSamples;
            return;
        }

        /// <summary>
        /// Adds the next sample value and its weight into the weighted average.
        /// </summary>
        /// <param name="value">The value.</param>
        /// <param name="weight">The value weight.</param>
        /// <returns>The resulting weighted average.</returns>
        public double AddSample(double value, double weight = 1d)
        {
            SumOfSamples += value * weight;
            SumOfWeights += weight;
            ++NumOfSamples;
            return Compute();
        }

        /// <summary>
        /// Removes the sample value and its weight from the weighted average.
        /// </summary>
        /// <param name="value">The value.</param>
        /// <param name="weight">The value weight.</param>
        /// <returns>The resulting weighted average.</returns>
        public double RemoveSample(double value, double weight = 1)
        {
            if (NumOfSamples > 0)
            {
                SumOfSamples -= value * weight;
                SumOfWeights -= weight;
                --NumOfSamples;
                if (NumOfSamples == 0)
                {
                    SumOfSamples = 0;
                    SumOfWeights = 0;
                }
            }
            else
            {
                throw new InvalidOperationException($"Can't remove the sample because there is no samples.");
            }
            return Compute();
        }

        /// <summary>
        /// Computes the weighted average for the next hypothetical sample.
        /// </summary>
        /// <remarks>
        /// Operation does not change the instance data.
        /// </remarks>
        /// <param name="simValue">The next sample value.</param>
        /// <param name="simWeight">The next sample value weight.</param>
        /// <returns>The resulting weighted average.</returns>
        public double SimulateNext(double simValue, double simWeight = 1)
        {
            return (SumOfSamples + (simValue * simWeight)) / (SumOfWeights + simWeight);
        }

    }//WeightedAvg

}//Namespace
