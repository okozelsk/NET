using RCNet.Queue;
using System;

namespace RCNet.MathTools
{
    /// <summary>
    /// Class implements moving data window providing additional functions such as statistics, weighted average, etc.
    /// </summary>
    [Serializable]
    public class MovingDataWindow
    {
        //Attributes
        private readonly SimpleQueue<double> _dataWindow;

        //Constructors
        /// <summary>
        /// Creates an empty instance.
        /// </summary>
        /// <param name="size">Defines internal moving data window size.</param>
        public MovingDataWindow(int size)
        {
            _dataWindow = new SimpleQueue<double>(size);
            return;
        }

        /// <summary>
        /// Deep copy constructor
        /// </summary>
        /// <param name="source">Source instance</param>
        public MovingDataWindow(MovingDataWindow source)
        {
            _dataWindow = source._dataWindow.ShallowClone();
            return;
        }

        //Properties
        /// <summary>
        /// Internal capacity
        /// </summary>
        public int Capacity { get { return _dataWindow.Capacity; } }

        /// <summary>
        /// Indicates the full data in the window
        /// </summary>
        public bool Full { get { return _dataWindow.Full; } }

        /// <summary>
        /// Number of samples in the moving data window
        /// </summary>
        public int NumOfSamples { get { return _dataWindow.Count; } }

        //Methods
        /// <summary>
        /// Checks number of samples and throws exception in case of insufficient number of samples.
        /// </summary>
        /// <param name="reqNumOfSamples">Number of requiered samples</param>
        private void CheckReadyness(int reqNumOfSamples = -1)
        {
            reqNumOfSamples = reqNumOfSamples == -1 ? 1 : reqNumOfSamples;
            if (NumOfSamples < reqNumOfSamples)
            {
                throw new InvalidOperationException($"Insufficient number of samples ({reqNumOfSamples}/{NumOfSamples}).");
            }
            return;
        }

        /// <summary>
        /// Returns number at the specified position within the moving data window
        /// </summary>
        /// <param name="index">Position</param>
        /// <param name="latestFirst">Specifies logical order (latest..oldest or oldest..latest)</param>
        public double GetAt(int index, bool latestFirst = false)
        {
            return _dataWindow.GetElementAt(index, latestFirst);
        }

        /// <summary>
        /// Returns weighted average
        /// </summary>
        /// <param name="weights">Weights to be used</param>
        /// <param name="latestFirst">Specifies logical order (latest..oldest or oldest..latest)</param>
        /// <param name="reqNumOfSamples">Number of requiered samples</param>
        public WeightedAvg GetWeightedAvg(double[] weights = null, bool latestFirst = false, int reqNumOfSamples = -1)
        {
            CheckReadyness(reqNumOfSamples);
            int numOfSamplesToBeProcessed = weights == null ? reqNumOfSamples == -1 ? _dataWindow.Count : reqNumOfSamples : Math.Min(weights.Length, reqNumOfSamples == -1 ? _dataWindow.Count : reqNumOfSamples);
            WeightedAvg wAvg = new WeightedAvg();
            for (int i = 0; i < numOfSamplesToBeProcessed; i++)
            {
                wAvg.AddSampleValue(_dataWindow.GetElementAt(i, latestFirst), weights == null ? 1d : weights[i]);
            }
            return wAvg;
        }

        /// <summary>
        /// Returns statistics of data samples currently stored in the internal sliding window
        /// </summary>
        /// <param name="latestFirst">Specifies logical order (latest..oldest or oldest..latest)</param>
        /// <param name="reqNumOfSamples">Number of requiered samples</param>
        public BasicStat GetDataStat(bool latestFirst = false, int reqNumOfSamples = -1)
        {
            int numOfSamplesToBeProcessed = reqNumOfSamples == -1 ? _dataWindow.Count : reqNumOfSamples;
            BasicStat stat = new BasicStat();
            for (int i = 0; i < numOfSamplesToBeProcessed; i++)
            {
                stat.AddSampleValue(_dataWindow.GetElementAt(i, latestFirst));
            }
            return stat;
        }

        /// <summary>
        /// Creates a deep copy of this instance
        /// </summary>
        public MovingDataWindow DeepClone()
        {
            return new MovingDataWindow(this);
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
        /// Adds the sample value into the internal moving data window
        /// </summary>
        /// <param name="value">Value</param>
        public void AddSampleValue(double value)
        {
            _dataWindow.Enqueue(value, true);
            return;
        }

    }//MovingDataWindow

}//Namespace
