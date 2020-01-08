using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using RCNet.Extensions;
using RCNet.MathTools;

namespace RCNet.Neural.Network.SM.Neuron
{
    /// <summary>
    /// Class implements predictors of the hidden neuron
    /// </summary>
    [Serializable]
    class HiddenNeuronPredictors
    {
        //Constants
        public const int MaxBinBufferSize = sizeof(ulong) * 8;
        public const int MaxExpWeightsSize = 64;
        public const int MaxLinWeightsSize = 10*1024;

        //Static members
        private static readonly double[] _expWeightsCache;
        private static readonly double[] _linWeightsCache;
        private static readonly ulong _highestBit;

        //Instance members
        //Attribute properties
        /// <summary>
        /// Predictors configuration.
        /// </summary>
        public HiddenNeuronPredictorsSettings Cfg { get; }

        //Attributes
        private double _lastActivation;
        private double _activationFadingSum;
        private readonly MovingDataWindow _activationMDW;
        private readonly double[] _activationMWAvgWeights;
        private int _activationMWAvgLeakage;
        private ulong _spikesBuffer;
        private int _bufferedHistLength;
        private int _firingCount;
        private readonly ulong _firingCountBit;
        private double _firingFadingSum;
        private readonly MovingDataWindow _firingMDW;
        private readonly double[] _firingMWAvgWeights;
        private int _firingMWAvgLeakage;


        /// <summary>
        /// Static constructor
        /// </summary>
        static HiddenNeuronPredictors()
        {
            _highestBit = 1ul;
            for (int i = 0; i < MaxBinBufferSize; i++)
            {
                if (i > 0) _highestBit <<= 1;
            }
            _expWeightsCache = new double[MaxExpWeightsSize];
            for (int i = 0; i < MaxExpWeightsSize; i++)
            {
                _expWeightsCache[i] = Math.Exp(-((MaxExpWeightsSize - 1) - i));
            }
            _linWeightsCache = new double[MaxLinWeightsSize];
            for (int i = 0; i < MaxLinWeightsSize; i++)
            {
                _linWeightsCache[i] = i + 1;
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
            if (Cfg.Params.FiringCountWindow == MaxBinBufferSize)
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
            switch(Cfg.Params.ActivationMWAvgWeightsType)
            {
                case NeuronCommon.NeuronPredictorMWAvgWeightsType.Exponential:
                    _activationMWAvgWeights = _expWeightsCache;
                    _activationMDW = new MovingDataWindow(Math.Min(Cfg.Params.ActivationMWAvgWindow, _expWeightsCache.Length));
                    break;
                case NeuronCommon.NeuronPredictorMWAvgWeightsType.Linear:
                    _activationMWAvgWeights = _linWeightsCache;
                    _activationMDW = new MovingDataWindow(Math.Min(Cfg.Params.ActivationMWAvgWindow, _linWeightsCache.Length));
                    break;
                case NeuronCommon.NeuronPredictorMWAvgWeightsType.Constant:
                    _activationMWAvgWeights = null;
                    _activationMDW = new MovingDataWindow(Cfg.Params.ActivationMWAvgWindow);
                    break;
                default:
                    throw new Exception("Unsupported weights type.");
            }
            switch (Cfg.Params.FiringMWAvgWeightsType)
            {
                case NeuronCommon.NeuronPredictorMWAvgWeightsType.Exponential:
                    _firingMWAvgWeights = _expWeightsCache;
                    _firingMDW = new MovingDataWindow(Math.Min(Cfg.Params.FiringMWAvgWindow, _expWeightsCache.Length));
                    break;
                case NeuronCommon.NeuronPredictorMWAvgWeightsType.Linear:
                    _firingMWAvgWeights = _linWeightsCache;
                    _firingMDW = new MovingDataWindow(Math.Min(Cfg.Params.FiringMWAvgWindow, _linWeightsCache.Length));
                    break;
                case NeuronCommon.NeuronPredictorMWAvgWeightsType.Constant:
                    _firingMWAvgWeights = null;
                    _firingMDW = new MovingDataWindow(Cfg.Params.FiringMWAvgWindow);
                    break;
                default:
                    throw new Exception("Unsupported weights type.");
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
            _lastActivation = 0d;
            _activationFadingSum = 0d;
            _activationMDW.Reset();
            _activationMWAvgLeakage = 0;
            _spikesBuffer = 0;
            _bufferedHistLength = 0;
            _firingCount = 0;
            _firingFadingSum = 0d;
            _firingMDW.Reset();
            _firingMWAvgLeakage = 0;
            return;
        }

        /// <summary>
        /// Updates internal state
        /// </summary>
        /// <param name="activation">Current value of the activation</param>
        /// <param name="spike">Is firing spike?</param>
        public void Update(double activation, bool spike)
        {
            //Update predictors
            _lastActivation = activation;
            _activationFadingSum *= (1d - Cfg.Params.ActivationFadingSumStrength);
            _activationFadingSum += activation;
            if (_activationMWAvgLeakage == Cfg.Params.ActivationMWAvgLeakage)
            {
                _activationMDW.AddSampleValue(activation);
                _activationMWAvgLeakage = 0;
            }
            else
            {
                ++_activationMWAvgLeakage;
            }
            _firingCount -= (_spikesBuffer & _firingCountBit) > 0ul ? 1 : 0;
            _firingFadingSum *= (1d - Cfg.Params.FiringFadingSumStrength);
            if (_firingMWAvgLeakage == Cfg.Params.FiringMWAvgLeakage)
            {
                _firingMDW.AddSampleValue(spike ? 1d : 0d);
                _firingMWAvgLeakage = 0;
            }
            else
            {
                ++_firingMWAvgLeakage;
            }
            _spikesBuffer <<= 1;
            if(spike)
            {
                _spikesBuffer |= 1ul;
                ++_firingCount;
                ++_firingFadingSum;
            }
            //Update buffer usage
            if (_bufferedHistLength < MaxBinBufferSize)
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
            if (Cfg.ActivationSquare)
            {
                predictors[idx++] = _lastActivation.Power(2);
            }
            if (Cfg.ActivationFadingSum)
            {
                predictors[idx++] = _activationFadingSum;
            }
            if (Cfg.ActivationMWAvg)
            {
                predictors[idx++] = _activationMDW.NumOfSamples > 0 ? _activationMDW.GetWeightedAvg(_activationMWAvgWeights, false).Avg : 0d;
            }
            if (Cfg.FiringFadingSum)
            {
                predictors[idx++] = _firingFadingSum;
            }
            if (Cfg.FiringMWAvg)
            {
                predictors[idx++] = _firingMDW.NumOfSamples > 0 ? _firingMDW.GetWeightedAvg(_firingMWAvgWeights, false).Avg : 0d;
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
