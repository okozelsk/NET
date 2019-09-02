using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using RCNet.Extensions;

namespace RCNet.Neural.Network.SM.Neuron
{
    /// <summary>
    /// Class converts recent spikes to an exponentially weighted firing rate (0-1)
    /// </summary>
    [Serializable]
    class HiddenNeuronPredictors
    {
        //Constants
        private const int HistBuffLength = 64;
        private const double FadingDecay = 0.005;

        //Static members
        private static readonly double[] _spikeExpValueCache;
        private static readonly double _sumOfSpikeExpValues;

        //Instance members
        //Attribute properties
        /// <summary>
        /// Enabling/Disabling switches of predictors
        /// </summary>
        public HiddenNeuronPredictorsSettings Settings { get; }

        //Attributes
        private readonly double[] _spikes;
        private readonly double[] _activations;
        private int _numOfBufferedData;
        //Predictors
        private int _numOfRecentFirings;
        private double _fadingSumOfFirings;
        private double _expWAvgOfFirings;

        /// <summary>
        /// Static constructor
        /// </summary>
        static HiddenNeuronPredictors()
        {
            _spikeExpValueCache = new double[HistBuffLength];
            _sumOfSpikeExpValues = 0;
            for (int i = 0; i < HistBuffLength; i++)
            {
                double val = Math.Exp(-i);
                _spikeExpValueCache[i] = val;
                _sumOfSpikeExpValues += val;
            }
            return;
        }

        /// <summary>
        /// Creates new initialized instance
        /// </summary>
        public HiddenNeuronPredictors(HiddenNeuronPredictorsSettings settings)
        {
            Settings = settings;
            _spikes = new double[HistBuffLength];
            _activations = new double[HistBuffLength];
            Reset();
            return;
        }

        //Methods
        /// <summary>
        /// Resets internal state to initial state
        /// </summary>
        public void Reset()
        {
            _spikes.Populate(0);
            _activations.Populate(0);
            _numOfBufferedData = 0;
            _numOfRecentFirings = 0;
            _fadingSumOfFirings = 0;
            _expWAvgOfFirings = 0;
            return;
        }

        /// <summary>
        /// Updates internal state
        /// </summary>
        /// <param name="activation">Current value of the activation</param>
        /// <param name="spike">Is firing spike?</param>
        public void Update(double activation, bool spike)
        {
            //Rotate buffers and compute predictors
            _numOfRecentFirings -= (_spikes[_spikes.Length - 1] > 0 ? 1 : 0);
            _fadingSumOfFirings *= (1d - FadingDecay);
            _expWAvgOfFirings = 0;
            for (int i = HistBuffLength - 1; i >= 0; i--)
            {
                _spikes[i] = (i > 0 ? _spikes[i - 1] : (spike ? 1d : 0d));
                _expWAvgOfFirings += _spikes[i] * _spikeExpValueCache[i];
                _activations[i] = (i > 0 ? _activations[i - 1] : activation);
            }
            //Finalize predictors
            _numOfRecentFirings += (_spikes[0] > 0 ? 1 : 0);
            _fadingSumOfFirings += _spikes[0];
            _expWAvgOfFirings /= _sumOfSpikeExpValues;
            //Update buffers usage
            if (_numOfBufferedData < HistBuffLength)
            {
                ++_numOfBufferedData;
            }
            return;
        }

        /// <summary>
        /// Returns last spikes as an integer number where bits are in order that the higher bit represents the more recent spike
        /// </summary>
        /// <returns>Last spikes history as an integer</returns>
        private ulong GetLastSpikes(int histLength)
        {
            //Checks and corrections
            if (_numOfBufferedData == 0 || histLength < 1)
            {
                return 0;
            }
            if(histLength > _numOfBufferedData)
            {
                histLength = _numOfBufferedData;
            }
            //Pick up requested history
            ulong result = 0;
            for (int i = 0; i < histLength; i++)
            {
                result <<= 1;
                result += (_spikes[i] > 0 ? (ulong)1 : (ulong)0);
            }
            return result;
        }

        /// <summary>
        /// Copies values of enabled predictors to a given buffer starting from specified position (idx)
        /// </summary>
        /// <param name="predictors">Buffer where to be copied enabled predictors</param>
        /// <param name="idx">Starting position index</param>
        /// <returns></returns>
        public int CopyPredictorsTo(double[] predictors, int idx)
        {
            if (Settings == null || Settings.NumOfEnabledPredictors == 0)
            {
                return 0;
            }
            if (Settings.Activation)
            {
                predictors[idx] = _activations[0];
                ++idx;
            }
            if (Settings.SquaredActivation)
            {
                predictors[idx] = _activations[0] * _activations[0];
                ++idx;
            }
            if (Settings.ExpWAvgFiringRate64)
            {
                predictors[idx] = _expWAvgOfFirings;
                ++idx;
            }
            if (Settings.FadingNumOfFirings)
            {
                predictors[idx] = _fadingSumOfFirings;
                ++idx;
            }
            if (Settings.NumOfFirings64)
            {
                predictors[idx] = _numOfRecentFirings;
                ++idx;
            }
            if (Settings.LastBin32FiringHist)
            {
                predictors[idx] = GetLastSpikes(32);
                ++idx;
            }
            if (Settings.LastBin16FiringHist)
            {
                predictors[idx] = GetLastSpikes(16);
                ++idx;
            }
            if (Settings.LastBin8FiringHist)
            {
                predictors[idx] = GetLastSpikes(8);
                ++idx;
            }
            if (Settings.LastBin1FiringHist)
            {
                predictors[idx] = GetLastSpikes(1);
                ++idx;
            }
            return Settings.NumOfEnabledPredictors;
        }

        /// <summary>
        /// Returns array containing values of enabled predictors
        /// </summary>
        /// <returns></returns>
        public double[] GetPredictors()
        {
            if (Settings == null || Settings.NumOfEnabledPredictors == 0)
            {
                return null;
            }
            double[] predictors = new double[Settings.NumOfEnabledPredictors];
            CopyPredictorsTo(predictors, 0);
            return predictors;
        }

    }//HiddenNeuronPredictors

}//Namespace
