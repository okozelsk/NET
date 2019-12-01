using RCNet.Extensions;
using RCNet.Queue;
using System;
using System.Collections.Generic;

namespace RCNet.MathTools
{
    /// <summary>
    /// Class implements moving weighted average.
    /// </summary>
    [Serializable]
    public class MovingWeightedAvg
    {
        //Attributes
        private readonly SimpleQueue<double> _dataWindow;

        //Constructors
        /// <summary>
        /// Constructs an empty instance having all weights equal to 1.
        /// </summary>
        /// <param name="size">Determines internal sliding window size.</param>
        public MovingWeightedAvg(int size)
        {
            _dataWindow = new SimpleQueue<double>(size);
            return;
        }

        /// <summary>
        /// Deep copy constructor
        /// </summary>
        /// <param name="source">Source instance</param>
        public MovingWeightedAvg(MovingWeightedAvg source)
        {
            _dataWindow = source._dataWindow.ShallowClone();
            return;
        }
        
        //Properties
        /// <summary>
        /// Indicates the readyness
        /// </summary>
        public bool Initialized { get { return _dataWindow.Full; } }

        /// <summary>
        /// Number of samples in the sliding data window
        /// </summary>
        public int NumOfSamples { get { return _dataWindow.Count; } }

        //Methods
        /// <summary>
        /// Checks readyness (throws exception in case of no stored samples).
        /// </summary>
        private void CheckReadyness()
        {
            if (NumOfSamples == 0)
            {
                throw new Exception($"No stored samples.");
            }
            return;
        }

        /// <summary>
        /// Returns weighted average
        /// </summary>
        /// <param name="weights">Weights to be used</param>
        /// <param name="latestFirst">Specifies logical order (latest..oldest or oldest..latest)</param>
        public WeightedAvg GetWeightedAvg(double[] weights, bool latestFirst = false)
        {
            CheckReadyness();
            WeightedAvg wAvg = new WeightedAvg();
            for (int i = 0; i < _dataWindow.Count; i++)
            {
                wAvg.AddSampleValue(_dataWindow.GetElementAt(i, latestFirst), weights[i]);
            }
            return wAvg;
        }

        /// <summary>
        /// Returns statistics of data samples currently stored in the internal sliding window
        /// </summary>
        public BasicStat GetDataStat()
        {
            BasicStat stat = new BasicStat();
            for (int i = 0; i < _dataWindow.Count; i++)
            {
                stat.AddSampleValue(_dataWindow.GetElementAt(i));
            }
            return stat;
        }

        /// <summary>
        /// Creates a deep copy of this instance
        /// </summary>
        public MovingWeightedAvg DeepClone()
        {
            return new MovingWeightedAvg(this);
        }

        /// <summary>
        /// Resets the instance to the initial state
        /// </summary>
        public void Reset()
        {
            _dataWindow.Reset();
            return;
        }

        /// <summary>
        /// Adds the sample value into the internal sliding window
        /// </summary>
        /// <param name="value">Value</param>
        public void AddSampleValue(double value)
        {
            _dataWindow.Enqueue(value, true);
            return;
        }

    }//MovingWeightedAvg
}//Namespace
