using RCNet.MathTools.Hurst;
using RCNet.Queue;
using System;

namespace RCNet.MathTools
{
    /// <summary>
    /// Implements the moving data window providing additional functions such as statistics, weighted average, etc.
    /// </summary>
    [Serializable]
    public class MovingDataWindow
    {
        //Attributes
        private readonly SimpleQueue<double> _dataQueue;

        //Constructors
        /// <summary>
        /// Creates an uninitialized instance.
        /// </summary>
        /// <param name="size">The moving data window size.</param>
        public MovingDataWindow(int size)
        {
            _dataQueue = new SimpleQueue<double>(size);
            return;
        }

        /// <summary>
        /// The copy constructor.
        /// </summary>
        /// <param name="source">The source instance.</param>
        public MovingDataWindow(MovingDataWindow source)
        {
            _dataQueue = source._dataQueue.ShallowClone();
            return;
        }

        //Properties
        /// <summary>
        /// Gets the capacity.
        /// </summary>
        public int Capacity { get { return _dataQueue.Capacity; } }

        /// <summary>
        /// The used capacity of the windpw.
        /// </summary>
        public int UsedCapacity { get { return _dataQueue.Count; } }

        /// <summary>
        /// Indicates the capacity of the window is fully used.
        /// </summary>
        public bool Full { get { return _dataQueue.Full; } }

        //Methods
        /// <summary>
        /// Checks the number of samples and throws exception in case of insufficient number of samples.
        /// </summary>
        /// <param name="reqNumOfSamples">The requiered number of samples.</param>
        private void CheckReadyness(int reqNumOfSamples = -1)
        {
            reqNumOfSamples = reqNumOfSamples == -1 ? 1 : reqNumOfSamples;
            if (UsedCapacity < reqNumOfSamples)
            {
                throw new InvalidOperationException($"Insufficient number of samples ({reqNumOfSamples}/{UsedCapacity}).");
            }
            return;
        }

        /// <summary>
        /// Returns a number at the specified position within the moving data window.
        /// </summary>
        /// <param name="index">The zero-based index wthin the window.</param>
        /// <param name="latestFirst">Specifies a logical order (latest..oldest or oldest..latest).</param>
        public double GetAt(int index, bool latestFirst = false)
        {
            return _dataQueue.GetElementAt(index, latestFirst);
        }

        /// <summary>
        /// Computes the weighted average of the data currently stored in the window.
        /// </summary>
        /// <param name="weights">The weights.</param>
        public WeightedAvg GetDataWeightedAvg(double[] weights)
        {
            CheckReadyness(weights.Length);
            int numOfSamplesToBeProcessed = weights.Length;
            WeightedAvg wAvg = new WeightedAvg();
            for (int i = numOfSamplesToBeProcessed - 1, j = 0; i >= 0; i--, j++)
            {
                wAvg.AddSample(_dataQueue.GetElementAt(i, true), weights[j]);
            }
            return wAvg;
        }

        /// <summary>
        /// Computes the weighted average of the differences of the data currently stored in the window.
        /// </summary>
        /// <param name="weights">The weights.</param>
        public WeightedAvg GetDataDiffWeightedAvg(double[] weights)
        {
            CheckReadyness(weights.Length + 1);
            int numOfSamplesToBeProcessed = weights.Length + 1;
            WeightedAvg wAvg = new WeightedAvg();
            for (int i = numOfSamplesToBeProcessed - 2, j = 0; i >= 0; i--, j++)
            {
                wAvg.AddSample(_dataQueue.GetElementAt(i, true) - _dataQueue.GetElementAt(i + 1, true), weights[j]);
            }
            return wAvg;
        }

        /// <summary>
        /// Computes the linearly weighted average of the data in the window.
        /// </summary>
        /// <param name="reqNumOfSamples">The requiered mumber of samples (-1 means all).</param>
        public WeightedAvg GetDataLinWeightedAvg(int reqNumOfSamples = -1)
        {
            int numOfSamplesToBeProcessed = reqNumOfSamples == -1 ? _dataQueue.Count : reqNumOfSamples;
            CheckReadyness(numOfSamplesToBeProcessed);
            WeightedAvg wAvg = new WeightedAvg();
            for (int i = numOfSamplesToBeProcessed - 1, w = 1; i >= 0; i--, w++)
            {
                wAvg.AddSample(_dataQueue.GetElementAt(i, true), w);
            }
            return wAvg;
        }

        /// <summary>
        /// Computes the linearly weighted average of the differences of the data in the window.
        /// </summary>
        /// <param name="reqNumOfSamples">The requiered mumber of samples (-1 means all).</param>
        public WeightedAvg GetDataDiffLinWeightedAvg(int reqNumOfSamples = -1)
        {
            int numOfSamplesToBeProcessed = reqNumOfSamples == -1 ? _dataQueue.Count : reqNumOfSamples;
            CheckReadyness(numOfSamplesToBeProcessed);
            WeightedAvg wAvg = new WeightedAvg();
            for (int i = numOfSamplesToBeProcessed - 2, w = 1; i >= 0; i--, w++)
            {
                wAvg.AddSample(_dataQueue.GetElementAt(i, true) - _dataQueue.GetElementAt(i + 1, true));
            }
            return wAvg;
        }

        /// <summary>
        /// Computes the statistics of the data in the window.
        /// </summary>
        /// <param name="reqNumOfSamples">The requiered mumber of samples (-1 means all).</param>
        public BasicStat GetDataStat(int reqNumOfSamples = -1)
        {
            int numOfSamplesToBeProcessed = reqNumOfSamples == -1 ? _dataQueue.Count : reqNumOfSamples;
            CheckReadyness(numOfSamplesToBeProcessed);
            BasicStat stat = new BasicStat();
            for (int i = numOfSamplesToBeProcessed - 1; i >= 0; i--)
            {
                stat.AddSample(_dataQueue.GetElementAt(i, true));
            }
            return stat;
        }

        /// <summary>
        /// Computes the statistics of the differences of the data in the window.
        /// </summary>
        /// <param name="reqNumOfSamples">The requiered mumber of samples (-1 means all).</param>
        public BasicStat GetDataDiffStat(int reqNumOfSamples = -1)
        {
            int numOfSamplesToBeProcessed = reqNumOfSamples == -1 ? _dataQueue.Count : reqNumOfSamples;
            CheckReadyness(numOfSamplesToBeProcessed);
            BasicStat stat = new BasicStat();
            for (int i = numOfSamplesToBeProcessed - 2; i >= 0; i--)
            {
                stat.AddSample(_dataQueue.GetElementAt(i, true) - _dataQueue.GetElementAt(i + 1, true));
            }
            return stat;
        }

        /// <summary>
        /// Computes the rescaled range of the data in the window.
        /// </summary>
        /// <param name="reqNumOfSamples">The requiered mumber of samples (-1 means all).</param>
        public double GetDataRescaledRange(int reqNumOfSamples = -1)
        {
            int numOfSamplesToBeProcessed = reqNumOfSamples == -1 ? _dataQueue.Count : reqNumOfSamples;
            CheckReadyness(numOfSamplesToBeProcessed);
            RescaledRange rr = new RescaledRange(numOfSamplesToBeProcessed);
            for (int i = numOfSamplesToBeProcessed - 1; i >= 0; i--)
            {
                rr.AddValue(_dataQueue.GetElementAt(i, true));
            }
            return rr.Compute();
        }

        /// <summary>
        /// Computes the rescaled range of the differences of the data in the window.
        /// </summary>
        /// <param name="reqNumOfSamples">The requiered mumber of samples (-1 means all).</param>
        public double GetDataDiffRescaledRange(int reqNumOfSamples = -1)
        {
            int numOfSamplesToBeProcessed = reqNumOfSamples == -1 ? _dataQueue.Count : reqNumOfSamples;
            CheckReadyness(numOfSamplesToBeProcessed);
            RescaledRange rr = new RescaledRange(numOfSamplesToBeProcessed - 1);
            for (int i = numOfSamplesToBeProcessed - 2; i >= 0; i--)
            {
                rr.AddValue(_dataQueue.GetElementAt(i, true) - _dataQueue.GetElementAt(i + 1, true));
            }
            return rr.Compute();
        }

        /// <summary>
        /// Creates the deep copy instance.
        /// </summary>
        public MovingDataWindow DeepClone()
        {
            return new MovingDataWindow(this);
        }

        /// <summary>
        /// Resets the instance to the initial state.
        /// </summary>
        public void Reset()
        {
            _dataQueue.Reset();
            return;
        }

        /// <summary>
        /// Adds the sample.
        /// </summary>
        /// <param name="sample">The sample.</param>
        public void AddSample(double sample)
        {
            _dataQueue.Enqueue(sample, true);
            return;
        }

    }//MovingDataWindow

}//Namespace
