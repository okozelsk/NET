using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using RCNet.Extensions;
using RCNet.MathTools;

namespace RCNet.Neural.Network.SM.Preprocessing.Reservoir.Neuron.Predictor
{
    /// <summary>
    /// Class provides computed predictors of the hidden neuron
    /// </summary>
    [Serializable]
    public class PredictorsProvider
    {
        /// <summary>
        /// Identifiers of implemented predictors
        /// </summary>
        public enum PredictorID
        {
            /// <summary>
            /// Current activation state
            /// </summary>
            Activation,
            /// <summary>
            /// Squared current activation state
            /// </summary>
            ActivationSquare,
            /// <summary>
            /// Fading sum of the activation state
            /// </summary>
            ActivationFadingSum,
            /// <summary>
            /// Activation state moving weighted average
            /// </summary>
            ActivationMWAvg,
            /// <summary>
            /// Fading number of firings
            /// </summary>
            FiringFadingSum,
            /// <summary>
            /// Moving weighted average firing
            /// </summary>
            FiringMWAvg,
            /// <summary>
            /// Number of firings within the last N cycles window
            /// </summary>
            FiringCount,
            /// <summary>
            /// Bitwise spike-train within the last N cycles window as an unsigned integer number
            /// </summary>
            FiringBinPattern
        }//PredictorID

        //Constants
        /// <summary>
        /// Maximum number of buffered bits
        /// </summary>
        public const int MaxBinBufferSize = sizeof(ulong) * 8;
        
        /// <summary>
        /// Maximum number of exponential weights
        /// </summary>
        public const int MaxExpWeightsSize = 64;
        
        /// <summary>
        /// Maximum number of linear weights
        /// </summary>
        public const int MaxLinWeightsSize = 10*1024;

        //Static
        /// <summary>
        /// Number of supported analog coding methods
        /// </summary>
        public static readonly int NumOfPredictors;
        private static readonly double[] _expWeightsCache;
        private static readonly double[] _linWeightsCache;
        private static readonly ulong _highestBit;

        //Attribute properties
        /// <summary>
        /// Configuration of the predictors
        /// </summary>
        public PredictorsSettings Cfg { get; }

        //Attributes
        private double _lastActivation;
        private double _activationFadingSum;
        private readonly MovingDataWindow _activationMDW;
        private readonly double[] _activationMWAvgWeights;
        private int _activationMWAvgLeakage;
        private ulong _spikesBuffer;
        private int _bufferedHistLength;
        private int _firingCount;
        private readonly ulong _firingCountDecrementBit;
        private double _firingFadingSum;
        private readonly MovingDataWindow _firingMDW;
        private readonly double[] _firingMWAvgWeights;
        private int _firingMWAvgLeakage;


        /// <summary>
        /// Static constructor
        /// </summary>
        static PredictorsProvider()
        {
            NumOfPredictors = typeof(PredictorID).GetEnumValues().Length;
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
        /// <param name="aggregatedCfg">Aggregated configuration to be used. Note that ParamsCfg member inside the aggregatedCfg must not be null.</param>
        public PredictorsProvider(PredictorsSettings aggregatedCfg)
        {
            Cfg = aggregatedCfg;
            if (Cfg.ParamsCfg.FiringCountCfg.Window == MaxBinBufferSize)
            {
                _firingCountDecrementBit = _highestBit;
            }
            else
            {
                _firingCountDecrementBit = 1ul;
                for (int i = 1; i < Cfg.ParamsCfg.FiringCountCfg.Window; i++)
                {
                    _firingCountDecrementBit <<= 1;
                }
            }
            switch(Cfg.ParamsCfg.ActivationMWAvgCfg.Weights)
            {
                case NeuronCommon.NeuronPredictorMWAvgWeightsType.Exponential:
                    _activationMWAvgWeights = _expWeightsCache;
                    _activationMDW = new MovingDataWindow(Math.Min(Cfg.ParamsCfg.ActivationMWAvgCfg.Window, _expWeightsCache.Length));
                    break;
                case NeuronCommon.NeuronPredictorMWAvgWeightsType.Linear:
                    _activationMWAvgWeights = _linWeightsCache;
                    _activationMDW = new MovingDataWindow(Math.Min(Cfg.ParamsCfg.ActivationMWAvgCfg.Window, _linWeightsCache.Length));
                    break;
                case NeuronCommon.NeuronPredictorMWAvgWeightsType.Constant:
                    _activationMWAvgWeights = null;
                    _activationMDW = new MovingDataWindow(Cfg.ParamsCfg.ActivationMWAvgCfg.Window);
                    break;
                default:
                    throw new Exception("Unsupported weights type.");
            }
            switch (Cfg.ParamsCfg.FiringMWAvgCfg.Weights)
            {
                case NeuronCommon.NeuronPredictorMWAvgWeightsType.Exponential:
                    _firingMWAvgWeights = _expWeightsCache;
                    _firingMDW = new MovingDataWindow(Math.Min(Cfg.ParamsCfg.FiringMWAvgCfg.Window, _expWeightsCache.Length));
                    break;
                case NeuronCommon.NeuronPredictorMWAvgWeightsType.Linear:
                    _firingMWAvgWeights = _linWeightsCache;
                    _firingMDW = new MovingDataWindow(Math.Min(Cfg.ParamsCfg.FiringMWAvgCfg.Window, _linWeightsCache.Length));
                    break;
                case NeuronCommon.NeuronPredictorMWAvgWeightsType.Constant:
                    _firingMWAvgWeights = null;
                    _firingMDW = new MovingDataWindow(Cfg.ParamsCfg.FiringMWAvgCfg.Window);
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
            _activationFadingSum *= (1d - Cfg.ParamsCfg.ActivationFadingSumCfg.Strength);
            _activationFadingSum += activation;
            if (_activationMWAvgLeakage == Cfg.ParamsCfg.ActivationMWAvgCfg.Leakage)
            {
                _activationMDW.AddSampleValue(activation);
                _activationMWAvgLeakage = 0;
            }
            else
            {
                ++_activationMWAvgLeakage;
            }
            _firingCount -= (_spikesBuffer & _firingCountDecrementBit) > 0ul ? 1 : 0;
            _firingFadingSum *= (1d - Cfg.ParamsCfg.FiringFadingSumCfg.Strength);
            if (_firingMWAvgLeakage == Cfg.ParamsCfg.FiringMWAvgCfg.Leakage)
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
            int count = 0;
            if (Cfg.IsEnabled(PredictorID.Activation))
            {
                predictors[idx++] = _lastActivation;
                ++count;
            }
            if (Cfg.IsEnabled(PredictorID.ActivationSquare))
            {
                predictors[idx++] = _lastActivation.Power(2);
                ++count;
            }
            if (Cfg.IsEnabled(PredictorID.ActivationFadingSum))
            {
                predictors[idx++] = _activationFadingSum;
                ++count;
            }
            if (Cfg.IsEnabled(PredictorID.ActivationMWAvg))
            {
                predictors[idx++] = _activationMDW.NumOfSamples > 0 ? _activationMDW.GetWeightedAvg(_activationMWAvgWeights, false).Avg : 0d;
                ++count;
            }
            if (Cfg.IsEnabled(PredictorID.FiringFadingSum))
            {
                predictors[idx++] = _firingFadingSum;
                ++count;
            }
            if (Cfg.IsEnabled(PredictorID.FiringMWAvg))
            {
                predictors[idx++] = _firingMDW.NumOfSamples > 0 ? _firingMDW.GetWeightedAvg(_firingMWAvgWeights, false).Avg : 0d;
                ++count;
            }
            if (Cfg.IsEnabled(PredictorID.FiringCount))
            {
                predictors[idx++] = _firingCount;
                ++count;
            }
            if (Cfg.IsEnabled(PredictorID.FiringBinPattern))
            {
                predictors[idx++] = GetFiringBinPattern(Cfg.ParamsCfg.FiringBinPatternCfg.Window);
                ++count;
            }
            return count;
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

    }//PredictorsProvider

}//Namespace
