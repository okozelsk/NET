using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using RCNet.Extensions;
using RCNet.MathTools;

namespace RCNet.Neural.Network.SM.Preprocessing.Neuron.Predictor
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

        /// <summary>
        /// Type of weights used by moving weighted average neuron's predictors
        /// </summary>
        public enum PredictorMWAvgWeightsType
        {
            /// <summary>
            /// Exponential weights.
            /// </summary>
            Exponential,
            /// <summary>
            /// Linear weigths.
            /// </summary>
            Linear,
            /// <summary>
            /// Constant weights.
            /// </summary>
            Constant
        }//PredictorMWAvgWeightsType



        //Constants
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
        /// Number of supported predictors
        /// </summary>
        public static readonly int NumOfSupportedPredictors;
        private static readonly double[] _expWeightsCache;
        private static readonly double[] _linWeightsCache;

        //Attribute properties
        /// <summary>
        /// Number of enabled predictors
        /// </summary>
        public int NumOfEnabledPredictors { get { return _cfg.NumOfEnabledPredictors; } }


        //Attributes
        private readonly PredictorsSettings _cfg;
        private double _lastActivation;
        private double _activationFadingSum;
        private readonly MovingDataWindow _activationMDW;
        private readonly double[] _activationMWAvgWeights;
        private double _firingFadingSum;
        private readonly Bitwise.Window _firingMDW;
        private readonly double[] _firingMWAvgWeights;


        /// <summary>
        /// Static constructor
        /// </summary>
        static PredictorsProvider()
        {
            NumOfSupportedPredictors = typeof(PredictorID).GetEnumValues().Length;
            _expWeightsCache = new double[MaxExpWeightsSize];
            for (int i = 0; i < MaxExpWeightsSize; i++)
            {
                _expWeightsCache[i] = Math.Exp(-i);
            }
            _linWeightsCache = new double[MaxLinWeightsSize];
            for (int i = 0; i < MaxLinWeightsSize; i++)
            {
                _linWeightsCache[i] = 1d / (i + 1);
            }
            return;
        }

        /// <summary>
        /// Creates new initialized instance.
        /// </summary>
        /// <param name="cfg">Configuration to be used. Note that ParamsCfg member inside the cfg must not be null.</param>
        public PredictorsProvider(PredictorsSettings cfg)
        {
            //Check
            if(cfg.ParamsCfg == null)
            {
                throw new ArgumentException("Invalid configuration. ParamsCfg inside the configuration is null.", "cfg");
            }
            //Store configuration
            _cfg = cfg;
            //Determine necessary size of the activation moving window and instantiate it
            int activationMWSize = 0;
            if(_cfg.IsEnabled(PredictorID.ActivationMWAvg))
            {
                switch (_cfg.ParamsCfg.ActivationMWAvgCfg.Weights)
                {
                    case PredictorMWAvgWeightsType.Exponential:
                        _activationMWAvgWeights = _expWeightsCache;
                        activationMWSize = Math.Min(_cfg.ParamsCfg.ActivationMWAvgCfg.Window, _expWeightsCache.Length);
                        break;
                    case PredictorMWAvgWeightsType.Linear:
                        _activationMWAvgWeights = _linWeightsCache;
                        activationMWSize = Math.Min(_cfg.ParamsCfg.ActivationMWAvgCfg.Window, _linWeightsCache.Length);
                        break;
                    case PredictorMWAvgWeightsType.Constant:
                        _activationMWAvgWeights = null;
                        activationMWSize = _cfg.ParamsCfg.ActivationMWAvgCfg.Window;
                        break;
                    default:
                        throw new Exception($"Unsupported weights type {_cfg.ParamsCfg.ActivationMWAvgCfg.Weights.ToString()}.");
                }
            }
            _activationMDW = activationMWSize == 0 ? null : new MovingDataWindow(activationMWSize);

            //Determine necessary size of the firing moving window and instantiate it
            int firingMWSize = 0;
            if(_cfg.IsEnabled(PredictorID.FiringMWAvg))
            {
                switch (_cfg.ParamsCfg.FiringMWAvgCfg.Weights)
                {
                    case PredictorMWAvgWeightsType.Exponential:
                        _firingMWAvgWeights = _expWeightsCache;
                        firingMWSize = Math.Min(_cfg.ParamsCfg.FiringMWAvgCfg.Window, _expWeightsCache.Length);
                        break;
                    case PredictorMWAvgWeightsType.Linear:
                        _firingMWAvgWeights = _linWeightsCache;
                        firingMWSize = Math.Min(_cfg.ParamsCfg.FiringMWAvgCfg.Window, _linWeightsCache.Length);
                        break;
                    case PredictorMWAvgWeightsType.Constant:
                        _firingMWAvgWeights = null;
                        firingMWSize = _cfg.ParamsCfg.FiringMWAvgCfg.Window;
                        break;
                    default:
                        throw new Exception($"Unsupported weights type {_cfg.ParamsCfg.FiringMWAvgCfg.Weights.ToString()}.");
                }
            }
            if (_cfg.IsEnabled(PredictorID.FiringBinPattern))
            {
                firingMWSize = Math.Max(firingMWSize, _cfg.ParamsCfg.FiringBinPatternCfg.Window);
            }
            if(_cfg.IsEnabled(PredictorID.FiringCount))
            {
                firingMWSize = Math.Max(firingMWSize, _cfg.ParamsCfg.FiringCountCfg.Window);
            }
            _firingMDW = firingMWSize == 0 ? null : new Bitwise.Window(firingMWSize);
            Reset();
            return;
        }

        //Properties
        /// <summary>
        /// Necessary number of updates to make enabled hist-based predictors ready
        /// </summary>
        public int RequiredHistLength { get { return Math.Max(_activationMDW == null ? 1 : _activationMDW.Capacity, _firingMDW == null ? 1 : _firingMDW.Capacity); } }

        //Methods
        /// <summary>
        /// Checks if given predictor is enabled
        /// </summary>
        /// <param name="predictorID">Identificator of the predictor</param>
        public bool IsPredictorEnabled(PredictorID predictorID)
        {
            return _cfg.IsEnabled(predictorID);
        }

        /// <summary>
        /// Resets internal state to initial state
        /// </summary>
        public void Reset()
        {
            _lastActivation = 0d;
            _activationFadingSum = 0d;
            _activationMDW?.Reset();
            _firingFadingSum = 0d;
            _firingMDW?.Reset();
            return;
        }

        /// <summary>
        /// Updates internal state
        /// </summary>
        /// <param name="activation">Current value of the activation</param>
        /// <param name="normalizedActivation">Current value of the activation normalized between 0 and 1</param>
        /// <param name="spike">Is firing spike?</param>
        public void Update(double activation, double normalizedActivation, bool spike)
        {
            //Update
            //Activation based
            _lastActivation = activation;
            _activationFadingSum *= (1d - _cfg.ParamsCfg.ActivationFadingSumCfg.Strength);
            _activationFadingSum += (normalizedActivation);
            if(_activationMDW != null)
            {
                _activationMDW.AddSampleValue(activation);
            }
            //Firing based
            _firingFadingSum *= (1d - _cfg.ParamsCfg.FiringFadingSumCfg.Strength);
            if (spike) ++_firingFadingSum;
            if(_firingMDW != null)
            {
                _firingMDW.AddNext(spike);
            }
            return;
        }

        /// <summary>
        /// Returns identifiers of enabled predictors in the same order as in the methods CopyPredictorsTo and GetPredictors
        /// </summary>
        public List<PredictorID> GetEnabledIDs()
        {
            List<PredictorID> result = new List<PredictorID>(NumOfEnabledPredictors);
            if (_cfg.IsEnabled(PredictorID.Activation))
            {
                result.Add(PredictorID.Activation);
            }
            if (_cfg.IsEnabled(PredictorID.ActivationSquare))
            {
                result.Add(PredictorID.ActivationSquare);
            }
            if (_cfg.IsEnabled(PredictorID.ActivationFadingSum))
            {
                result.Add(PredictorID.ActivationFadingSum);
            }
            if (_cfg.IsEnabled(PredictorID.ActivationMWAvg))
            {
                result.Add(PredictorID.ActivationMWAvg);
            }
            if (_cfg.IsEnabled(PredictorID.FiringFadingSum))
            {
                result.Add(PredictorID.FiringFadingSum);
            }
            if (_cfg.IsEnabled(PredictorID.FiringMWAvg))
            {
                result.Add(PredictorID.FiringMWAvg);
            }
            if (_cfg.IsEnabled(PredictorID.FiringCount))
            {
                result.Add(PredictorID.FiringCount);
            }
            if (_cfg.IsEnabled(PredictorID.FiringBinPattern))
            {
                result.Add(PredictorID.FiringBinPattern);
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
            if (_cfg.IsEnabled(PredictorID.Activation))
            {
                predictors[idx++] = _lastActivation;
                ++count;
            }
            if (_cfg.IsEnabled(PredictorID.ActivationSquare))
            {
                predictors[idx++] = _lastActivation.Power(2);
                ++count;
            }
            if (_cfg.IsEnabled(PredictorID.ActivationFadingSum))
            {
                predictors[idx++] = _activationFadingSum;
                ++count;
            }
            if (_cfg.IsEnabled(PredictorID.ActivationMWAvg))
            {
                predictors[idx++] = _activationMDW.NumOfSamples > 0 ? _activationMDW.GetWeightedAvg(_activationMWAvgWeights, true).Avg : 0d;
                ++count;
            }
            if (_cfg.IsEnabled(PredictorID.FiringFadingSum))
            {
                predictors[idx++] = _firingFadingSum;
                ++count;
            }
            if (_cfg.IsEnabled(PredictorID.FiringMWAvg))
            {
                double sum = 0;
                for (int bitIdx = 0; bitIdx < _cfg.ParamsCfg.FiringMWAvgCfg.Window; bitIdx++)
                {
                    if(_firingMDW.GetBit(bitIdx) > 0)
                    {
                        sum += _firingMWAvgWeights == null ? 1d : _firingMWAvgWeights[bitIdx];
                    }
                }
                predictors[idx++] = sum / _cfg.ParamsCfg.FiringMWAvgCfg.Window;
                ++count;
            }
            if (_cfg.IsEnabled(PredictorID.FiringCount))
            {
                predictors[idx++] = _firingMDW.GetNumOfSetBits(_cfg.ParamsCfg.FiringCountCfg.Window);
                ++count;
            }
            if (_cfg.IsEnabled(PredictorID.FiringBinPattern))
            {
                predictors[idx++] = _firingMDW.GetBits(_cfg.ParamsCfg.FiringBinPatternCfg.Window);
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
            if (_cfg.NumOfEnabledPredictors == 0)
            {
                return null;
            }
            double[] predictors = new double[_cfg.NumOfEnabledPredictors];
            CopyPredictorsTo(predictors, 0);
            return predictors;
        }

    }//PredictorsProvider

}//Namespace
