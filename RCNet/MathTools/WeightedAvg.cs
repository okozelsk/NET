using System;

namespace RCNet.MathTools
{
    /// <summary>
    /// Class implements iterative computation of the weighted average.
    /// </summary>
    [Serializable]
    public class WeightedAvg
    {
        //Attributes
        private double _sumOfWeightedValues;
        private double _sumOfWeights;
        private double _avg;
        private int _numOfSamples;
        
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
        public bool Initialized { get { return (_numOfSamples > 0); } }
        /// <summary>
        /// Number of considered samples
        /// </summary>
        public int NumOfSamples { get { return _numOfSamples; } }
        /// <summary>
        /// The weighted average
        /// </summary>
        public double Avg { get { return _avg; } }
        /// <summary>
        /// The sum of values
        /// </summary>
        public double SumOfValues { get { return _sumOfWeightedValues; } }
        /// <summary>
        /// The sum of weights
        /// </summary>
        public double SumOfWeights { get { return _sumOfWeights; } }

        //Methods
        /// <summary>
        /// Computes the weighted average value
        /// </summary>
        private double Compute()
        {
            if (_sumOfWeights != 0 && _numOfSamples > 0)
            {
                _avg = _sumOfWeightedValues / _sumOfWeights;
            }
            else
            {
                _avg = 0;
            }
            return _avg;
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
            _sumOfWeightedValues = 0;
            _sumOfWeights = 0;
            _avg = 0;
            _numOfSamples = 0;
            return;
        }

        /// <summary>
        /// Adopts the source instance.
        /// </summary>
        /// <param name="source">Source instance</param>
        public void Adopt(WeightedAvg source)
        {
            _sumOfWeightedValues = source._sumOfWeightedValues;
            _sumOfWeights = source._sumOfWeights;
            _avg = source._avg;
            _numOfSamples = source._numOfSamples;
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
            _sumOfWeightedValues += value * weight;
            _sumOfWeights += weight;
            ++_numOfSamples;
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
            if (_numOfSamples > 0)
            {
                _sumOfWeightedValues -= value * weight;
                _sumOfWeights -= weight;
                --_numOfSamples;
                if(_numOfSamples == 0)
                {
                    _sumOfWeightedValues = 0;
                    _sumOfWeights = 0;
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
            return (_sumOfWeightedValues + (simValue * simWeight)) / (_sumOfWeights + simWeight);
        }



    }//WeightedAvg
}//Namespace
