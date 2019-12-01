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
        public const int MaxWindowSize = sizeof(ulong) * 8;

        //Static members
        private static readonly double[] _expWeightsCache;
        private static readonly double[] _linWeightsCache;
        private static readonly double[] _constWeightsCache;
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
        private readonly MovingWeightedAvg _activationMWAvg;
        private readonly double[] _activationMWAvgWeights;
        private int _activationMWAvgLeakage;
        private ulong _spikesBuffer;
        private int _bufferedHistLength;
        private int _firingCount;
        private readonly ulong _firingCountBit;
        private double _firingFadingSum;
        private readonly MovingWeightedAvg _firingMWAvg;
        private readonly double[] _firingMWAvgWeights;
        private int _firingMWAvgLeakage;


        /// <summary>
        /// Static constructor
        /// </summary>
        static HiddenNeuronPredictors()
        {
            _expWeightsCache = new double[MaxWindowSize];
            _linWeightsCache = new double[MaxWindowSize];
            _constWeightsCache = new double[MaxWindowSize];
            _highestBit = 1ul;
            for (int i = 0; i < MaxWindowSize; i++)
            {
                _expWeightsCache[i] = Math.Exp(-((MaxWindowSize - 1) - i));
                _linWeightsCache[i] = i + 1;
                _constWeightsCache[i] = 1d;
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
            if (Cfg.Params.FiringCountWindow == MaxWindowSize)
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
            _activationMWAvg = new MovingWeightedAvg(Cfg.Params.ActivationMWAvgWindow);
            switch(Cfg.Params.ActivationMWAvgWeightsType)
            {
                case NeuronCommon.NeuronPredictorMWAvgWeightsType.Exponential:
                    _activationMWAvgWeights = _expWeightsCache;
                    break;
                case NeuronCommon.NeuronPredictorMWAvgWeightsType.Linear:
                    _activationMWAvgWeights = _linWeightsCache;
                    break;
                case NeuronCommon.NeuronPredictorMWAvgWeightsType.Constant:
                    _activationMWAvgWeights = _constWeightsCache;
                    break;
                default:
                    throw new Exception("Unsupported weights type.");
            }
            _firingMWAvg = new MovingWeightedAvg(Cfg.Params.FiringMWAvgWindow);
            switch (Cfg.Params.FiringMWAvgWeightsType)
            {
                case NeuronCommon.NeuronPredictorMWAvgWeightsType.Exponential:
                    _firingMWAvgWeights = _expWeightsCache;
                    break;
                case NeuronCommon.NeuronPredictorMWAvgWeightsType.Linear:
                    _firingMWAvgWeights = _linWeightsCache;
                    break;
                case NeuronCommon.NeuronPredictorMWAvgWeightsType.Constant:
                    _firingMWAvgWeights = _constWeightsCache;
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
            _activationMWAvg.Reset();
            _activationMWAvgLeakage = 0;
            _spikesBuffer = 0;
            _bufferedHistLength = 0;
            _firingCount = 0;
            _firingFadingSum = 0d;
            _firingMWAvg.Reset();
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
                _activationMWAvg.AddSampleValue(activation);
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
                _firingMWAvg.AddSampleValue(spike ? 1d : 0d);
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
            if (_bufferedHistLength < MaxWindowSize)
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
                predictors[idx++] = _activationMWAvg.NumOfSamples > 0 ? _activationMWAvg.GetWeightedAvg(_activationMWAvgWeights, false).Avg : 0d;
            }
            if (Cfg.FiringFadingSum)
            {
                predictors[idx++] = _firingFadingSum;
            }
            if (Cfg.FiringMWAvg)
            {
                predictors[idx++] = _firingMWAvg.NumOfSamples > 0 ? _firingMWAvg.GetWeightedAvg(_firingMWAvgWeights, false).Avg : 0d;
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
