using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using RCNet.Extensions;

namespace RCNet.Neural.Network.SM.Neuron
{
    /// <summary>
    /// Class implements predictors of the hidden neuron
    /// </summary>
    [Serializable]
    class HiddenNeuronPredictors
    {
        //Constants
        private const int SpikesBuffLength = sizeof(ulong) * 8;

        //Static members
        private static readonly double[] _spikeExpValueCache;
        private static readonly double _sumOfSpikeExpValues;
        private static readonly ulong _highestBit;

        //Instance members
        //Attribute properties
        /// <summary>
        /// Predictors configuration.
        /// </summary>
        public HiddenNeuronPredictorsSettings Cfg { get; }

        //Attributes
        private double _lastActivation;
        private ulong _spikesBuffer;
        private int _bufferedHistLength;
        private int _firingCount;
        private readonly ulong _firingCountBit;
        private double _firingFadingSum;


        /// <summary>
        /// Static constructor
        /// </summary>
        static HiddenNeuronPredictors()
        {
            _spikeExpValueCache = new double[SpikesBuffLength];
            _sumOfSpikeExpValues = 0;
            _highestBit = 1ul;
            for (int i = 0; i < SpikesBuffLength; i++)
            {
                double val = Math.Exp(-i);
                _spikeExpValueCache[i] = val;
                _sumOfSpikeExpValues += val;
                if (i > 0) _highestBit <<= 1;
            }
            return;
        }

        /// <summary>
        /// Creates new initialized instance.
        /// </summary>
        /// <param name="cfg">Configuration to be used. Note that Params member must not be null.</param>
        public HiddenNeuronPredictors(HiddenNeuronPredictorsSettings cfg)
        {
            Cfg = cfg;
            if (Cfg.Params.FiringCountWindow == SpikesBuffLength)
            {
                _firingCountBit = _highestBit;
            }
            else
            {
                _firingCountBit = 1ul;
                for (int i = 1; i < Cfg.Params.FiringCountWindow; i++)
                {
                    _firingCountBit <<= 1;
                }
            }
            Reset();
            return;
        }

        //Methods
        /// <summary>
        /// Resets internal state to initial state
        /// </summary>
        public void Reset()
        {
            _lastActivation = 0;
            _spikesBuffer = 0;
            _bufferedHistLength = 0;
            _firingCount = 0;
            _firingFadingSum = 0;
            return;
        }

        /// <summary>
        /// Updates internal state
        /// </summary>
        /// <param name="activation">Current value of the activation</param>
        /// <param name="spike">Is firing spike?</param>
        public void Update(double activation, bool spike)
        {
            _lastActivation = activation;
            //Rotate buffer and update some predictors
            _firingCount -= (_spikesBuffer & _firingCountBit) > 0ul ? 1 : 0;
            _firingFadingSum *= (1d - Cfg.Params.FiringFadingSumStrength);
            _spikesBuffer <<= 1;
            if(spike)
            {
                _spikesBuffer |= 1ul;
                ++_firingCount;
                ++_firingFadingSum;
            }
            //Update buffer usage
            if (_bufferedHistLength < SpikesBuffLength)
            {
                ++_bufferedHistLength;
            }
            return;
        }

        /// <summary>
        /// Returns last spikes as an integer number where bits are in order that the higher bit represents the more recent spike
        /// </summary>
        /// <returns>Last spikes history as an integer</returns>
        private ulong GetFiringBinPattern(int histLength)
        {
            //Checks and corrections
            if (_bufferedHistLength == 0 || histLength < 1)
            {
                return 0;
            }
            if(histLength > _bufferedHistLength)
            {
                histLength = _bufferedHistLength;
            }
            //Roll requested history
            ulong result = 0;
            ulong tmp = _spikesBuffer;
            for (int i = 0; i < histLength; i++)
            {
                result <<= 1;
                result += tmp & 1ul;
                tmp >>= 1;
            }
            return result;
        }

        /// <summary>
        /// Returns exponentially weighted average of spikes in order that the stronger weight represents the more recent spike
        /// </summary>
        /// <returns>Exponentially weighted average of spikes</returns>
        private double GetFiringExpWAvgRate(int histLength)
        {
            //Checks and corrections
            if (_bufferedHistLength == 0 || histLength < 1)
            {
                return 0;
            }
            if (histLength > _bufferedHistLength)
            {
                histLength = _bufferedHistLength;
            }
            //Roll requested history
            double result = 0;
            ulong tmp = _spikesBuffer;
            for (int i = 0; i < histLength; i++)
            {
                result += (tmp & 1ul) > 0 ? _spikeExpValueCache[i] : 0d;
                tmp >>= 1;
            }
            return result / _sumOfSpikeExpValues;
        }

        /// <summary>
        /// Copies values of enabled predictors to a given buffer starting from specified position (idx)
        /// </summary>
        /// <param name="predictors">Buffer where to be copied enabled predictors</param>
        /// <param name="idx">Starting position index</param>
        /// <returns></returns>
        public int CopyPredictorsTo(double[] predictors, int idx)
        {
            if (Cfg.NumOfEnabledPredictors == 0)
            {
                return 0;
            }
            if (Cfg.Activation)
            {
                predictors[idx++] = _lastActivation;
            }
            if (Cfg.SquaredActivation)
            {
                predictors[idx++] = _lastActivation.Power(2);
            }
            if (Cfg.FiringExpWRate)
            {
                predictors[idx++] = GetFiringExpWAvgRate(Math.Min(_bufferedHistLength, Cfg.Params.FiringExpWRateWindow));
            }
            if (Cfg.FiringFadingSum)
            {
                predictors[idx++] = _firingFadingSum;
            }
            if (Cfg.FiringCount)
            {
                predictors[idx++] = _firingCount;
            }
            if (Cfg.FiringBinPattern)
            {
                predictors[idx++] = GetFiringBinPattern(Cfg.Params.FiringBinPatternWindow);
            }
            return Cfg.NumOfEnabledPredictors;
        }

        /// <summary>
        /// Returns array containing values of enabled predictors
        /// </summary>
        /// <returns></returns>
        public double[] GetPredictors()
        {
            if (Cfg.NumOfEnabledPredictors == 0)
            {
                return null;
            }
            double[] predictors = new double[Cfg.NumOfEnabledPredictors];
            CopyPredictorsTo(predictors, 0);
            return predictors;
        }

    }//HiddenNeuronPredictors

}//Namespace
