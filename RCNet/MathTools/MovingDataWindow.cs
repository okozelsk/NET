using RCNet.MathTools.Hurst;
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
        private readonly SimpleQueue<double> _dataQueue;

        //Constructors
        /// <summary>
        /// Creates an empty instance.
        /// </summary>
        /// <param name="size">Defines internal moving data window size.</param>
        public MovingDataWindow(int size)
        {
            _dataQueue = new SimpleQueue<double>(size);
            return;
        }

        /// <summary>
        /// Deep copy constructor
        /// </summary>
        /// <param name="source">Source instance</param>
        public MovingDataWindow(MovingDataWindow source)
        {
            _dataQueue = source._dataQueue.ShallowClone();
            return;
        }

        //Properties
        /// <summary>
        /// Internal capacity
        /// </summary>
        public int Capacity { get { return _dataQueue.Capacity; } }

        /// <summary>
        /// Indicates the full data in the window
        /// </summary>
        public bool Full { get { return _dataQueue.Full; } }

        /// <summary>
        /// Number of samples in the moving data window
        /// </summary>
        public int NumOfSamples { get { return _dataQueue.Count; } }

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
            return _dataQueue.GetElementAt(index, latestFirst);
        }

        /// <summary>
        /// Returns weighted average of data samples currently stored in the internal sliding window
        /// </summary>
        /// <param name="weights">Weights to be used</param>
        public WeightedAvg GetDataWeightedAvg(double[] weights)
        {
            CheckReadyness(weights.Length);
            int numOfSamplesToBeProcessed = weights.Length;
            WeightedAvg wAvg = new WeightedAvg();
            for (int i = numOfSamplesToBeProcessed - 1, j = 0; i >= 0; i--, j++)
            {
                wAvg.AddSampleValue(_dataQueue.GetElementAt(i, true), weights[j]);
            }
            return wAvg;
        }

        /// <summary>
        /// Returns weighted average of differences of data samples currently stored in the internal sliding window
        /// </summary>
        /// <param name="weights">Weights to be used</param>
        public WeightedAvg GetDataDiffWeightedAvg(double[] weights)
        {
            CheckReadyness(weights.Length + 1);
            int numOfSamplesToBeProcessed = weights.Length + 1;
            WeightedAvg wAvg = new WeightedAvg();
            for (int i = numOfSamplesToBeProcessed - 2, j = 0; i >= 0; i--, j++)
            {
                wAvg.AddSampleValue(_dataQueue.GetElementAt(i, true) - _dataQueue.GetElementAt(i + 1, true), weights[j]);
            }
            return wAvg;
        }

        /// <summary>
        /// Returns linearly weighted average of data samples currently stored in the internal sliding window
        /// </summary>
        /// <param name="reqNumOfSamples">Number of requiered samples</param>
        public WeightedAvg GetDataLinWeightedAvg(int reqNumOfSamples = -1)
        {
            int numOfSamplesToBeProcessed = reqNumOfSamples == -1 ? _dataQueue.Count : reqNumOfSamples;
            CheckReadyness(numOfSamplesToBeProcessed);
            WeightedAvg wAvg = new WeightedAvg();
            for (int i = numOfSamplesToBeProcessed - 1, w = 1; i >= 0; i--, w++)
            {
                wAvg.AddSampleValue(_dataQueue.GetElementAt(i, true), w);
            }
            return wAvg;
        }

        /// <summary>
        /// Returns linearly weighted average of differences of data samples currently stored in the internal sliding window
        /// </summary>
        /// <param name="reqNumOfSamples">Number of requiered samples</param>
        public WeightedAvg GetDataDiffLinWeightedAvg(int reqNumOfSamples = -1)
        {
            int numOfSamplesToBeProcessed = reqNumOfSamples == -1 ? _dataQueue.Count : reqNumOfSamples;
            CheckReadyness(numOfSamplesToBeProcessed);
            WeightedAvg wAvg = new WeightedAvg();
            for (int i = numOfSamplesToBeProcessed - 2, w = 1; i >= 0; i--, w++)
            {
                wAvg.AddSampleValue(_dataQueue.GetElementAt(i, true) - _dataQueue.GetElementAt(i + 1, true));
            }
            return wAvg;
        }

        /// <summary>
        /// Returns statistics of data samples currently stored in the internal sliding window
        /// </summary>
        /// <param name="reqNumOfSamples">Number of requiered samples</param>
        public BasicStat GetDataStat(int reqNumOfSamples = -1)
        {
            int numOfSamplesToBeProcessed = reqNumOfSamples == -1 ? _dataQueue.Count : reqNumOfSamples;
            CheckReadyness(numOfSamplesToBeProcessed);
            BasicStat stat = new BasicStat();
            for (int i = numOfSamplesToBeProcessed - 1; i >= 0; i--)
            {
                stat.AddSampleValue(_dataQueue.GetElementAt(i, true));
            }
            return stat;
        }

        /// <summary>
        /// Returns statistics of differences of data samples currently stored in the internal sliding window
        /// </summary>
        /// <param name="reqNumOfSamples">Number of requiered samples</param>
        public BasicStat GetDataDiffStat(int reqNumOfSamples = -1)
        {
            int numOfSamplesToBeProcessed = reqNumOfSamples == -1 ? _dataQueue.Count : reqNumOfSamples;
            CheckReadyness(numOfSamplesToBeProcessed);
            BasicStat stat = new BasicStat();
            for (int i = numOfSamplesToBeProcessed - 2; i >= 0; i--)
            {
                stat.AddSampleValue(_dataQueue.GetElementAt(i, true) - _dataQueue.GetElementAt(i + 1, true));
            }
            return stat;
        }

        /// <summary>
        /// Returns rescalled range of data samples currently stored in the internal sliding window
        /// </summary>
        /// <param name="reqNumOfSamples">Number of requiered samples</param>
        public double GetDataRescalledRange(int reqNumOfSamples = -1)
        {
            int numOfSamplesToBeProcessed = reqNumOfSamples == -1 ? _dataQueue.Count : reqNumOfSamples;
            CheckReadyness(numOfSamplesToBeProcessed);
            RescalledRange rr = new RescalledRange(numOfSamplesToBeProcessed);
            for (int i = numOfSamplesToBeProcessed - 1; i >= 0; i--)
            {
                rr.AddValue(_dataQueue.GetElementAt(i, true));
            }
            return rr.Compute();
        }

        /// <summary>
        /// Returns rescalled range of differences of data samples currently stored in the internal sliding window
        /// </summary>
        /// <param name="reqNumOfSamples">Number of requiered samples</param>
        public double GetDataDiffRescalledRange(int reqNumOfSamples = -1)
        {
            int numOfSamplesToBeProcessed = reqNumOfSamples == -1 ? _dataQueue.Count : reqNumOfSamples;
            CheckReadyness(numOfSamplesToBeProcessed);
            RescalledRange rr = new RescalledRange(numOfSamplesToBeProcessed - 1);
            for (int i = numOfSamplesToBeProcessed - 2; i >= 0; i--)
            {
                rr.AddValue(_dataQueue.GetElementAt(i, true) - _dataQueue.GetElementAt(i + 1, true));
            }
            return rr.Compute();
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
            _dataQueue.Reset();
            return;
        }

        /// <summary>
        /// Adds the sample value into the internal moving data window
        /// </summary>
        /// <param name="value">Value</param>
        public void AddSampleValue(double value)
        {
            _dataQueue.Enqueue(value, true);
            return;
        }

    }//MovingDataWindow

}//Namespace
