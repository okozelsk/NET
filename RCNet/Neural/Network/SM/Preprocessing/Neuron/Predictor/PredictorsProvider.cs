using RCNet.Extensions;
using RCNet.MathTools;
using RCNet.Queue;
using System;
using System.Collections.Generic;

namespace RCNet.Neural.Network.SM.Preprocessing.Neuron.Predictor
{
    /// <summary>
    /// Class provides computed predictors of the hidden neuron
    /// </summary>
    [Serializable]
    public class PredictorsProvider
    {
        /// <summary>
        /// Identifiers of the implemented predictors
        /// </summary>
        public enum PredictorID
        {
            /// <summary>
            /// Result of the activation function.
            /// </summary>
            Activation,
            /// <summary>
            /// Powered absolute value of the result of the activation function.
            /// </summary>
            ActivationPower,
            /// <summary>
            /// Statistical feature computed from the activation function results.
            /// </summary>
            ActivationStatFeature,
            /// <summary>
            /// Rescalled range computed from the activation function results.
            /// </summary>
            ActivationRescalledRange,
            /// <summary>
            /// Linearly weighted average computed from the activation function results.
            /// </summary>
            ActivationLinWAvg,
            /// <summary>
            /// Statistical feature computed from the differences of the activation function results (A[T] - A[T-1]).
            /// </summary>
            ActivationDiffStatFeature,
            /// <summary>
            /// Rescalled range computed from the differences of the activation function results (A[T] - A[T-1]).
            /// </summary>
            ActivationDiffRescalledRange,
            /// <summary>
            /// Linearly weighted average computed from the differences of the activation function results (A[T] - A[T-1]).
            /// </summary>
            ActivationDiffLinWAvg,
            /// <summary>
            /// Traced neuron's firing.
            /// </summary>
            FiringTrace

        }//PredictorID


        //Attribute properties
        /// <summary>
        /// Number of provided predictors
        /// </summary>
        public int NumOfProvidedPredictors { get { return _predictorCollection.Count; } }


        //Attributes
        private readonly List<IPredictor> _predictorCollection;
        private readonly MovingDataWindow _activationMDW;
        private readonly SimpleQueue<byte> _firingMDW;
        private readonly BasicStat _activationStat;
        private readonly BasicStat _activationDiffStat;
        private double _lastActivation;
        private double _lastNormalizedActivation;
        private bool _lastSpike;

        /// <summary>
        /// Creates new initialized instance.
        /// </summary>
        /// <param name="cfg">Configuration to be used. Note that ParamsCfg member inside the cfg must not be null.</param>
        public PredictorsProvider(PredictorsProviderSettings cfg)
        {
            int reqAMDWCapacity = 0;
            int reqFMDWCapacity = 0;
            _predictorCollection = new List<IPredictor>(cfg.NumOfPredictors);
            _activationMDW = null;
            _firingMDW = null;
            _activationStat = null;
            _activationDiffStat = null;

            foreach (IPredictorSettings predictorCfg in cfg.PredictorCfgCollection)
            {
                IPredictor predictor = PredictorFactory.CreatePredictor(predictorCfg);
                reqAMDWCapacity = Math.Max(reqAMDWCapacity, predictor.Cfg.RequiredWndSizeOfActivations);
                reqFMDWCapacity = Math.Max(reqFMDWCapacity, predictor.Cfg.RequiredWndSizeOfFirings);
                if(predictor.Cfg.NeedsContinuousActivationStat && _activationStat == null)
                {
                    _activationStat = new BasicStat();
                }
                if(predictor.Cfg.NeedsContinuousActivationDiffStat && _activationDiffStat == null)
                {
                    _activationDiffStat = new BasicStat();
                }
                _predictorCollection.Add(predictor);
            }

            if(reqAMDWCapacity > 0)
            {
                _activationMDW = new MovingDataWindow(reqAMDWCapacity);
            }
            if(reqFMDWCapacity > 0)
            {
                _firingMDW = new SimpleQueue<byte>(reqFMDWCapacity);
            }
            Reset();
            return;
        }

        //Properties
        /// <summary>
        /// Necessary number of updates to make the predictors ready
        /// </summary>
        public int RequiredHistLength { get { return Math.Max(_activationMDW == null ? 1 : _activationMDW.Capacity, _firingMDW == null ? 1 : _firingMDW.Capacity); } }

        //Methods
        /// <summary>
        /// Resets internal state to initial state
        /// </summary>
        public void Reset()
        {
            _activationMDW?.Reset();
            _firingMDW?.Reset();
            _activationStat?.Reset();
            _activationDiffStat?.Reset();
            _lastActivation = 0d;
            _lastNormalizedActivation = 0d;
            _lastSpike = false;
            //Predictors
            foreach (IPredictor predictor in _predictorCollection)
            {
                predictor.Reset();
            }
            return;
        }

        /// <summary>
        /// Returns identifiers of provided predictors in the same order as is used in the methods CopyPredictorsTo and GetPredictors
        /// </summary>
        public List<PredictorID> GetIDs()
        {
            List<PredictorID> ids = new List<PredictorID>(_predictorCollection.Count);
            foreach(IPredictor predictor in _predictorCollection)
            {
                ids.Add(predictor.Cfg.ID);
            }
            return ids;
        }

        /// <summary>
        /// Updates internal state
        /// </summary>
        /// <param name="activation">Current value of the activation</param>
        /// <param name="normalizedActivation">Current value of the activation normalized between 0 and 1</param>
        /// <param name="spike">Indicates whether the neuron is firing</param>
        public void Update(double activation, double normalizedActivation, bool spike)
        {
            //Continuous statistics
            double activationDiff = activation - _lastActivation;
            _activationStat?.AddSampleValue(activation);
            _activationDiffStat?.AddSampleValue(activationDiff);
            //Data windows
            _activationMDW?.AddSampleValue(activation);
            _firingMDW?.Enqueue(spike ? (byte)1 : (byte)0);
            //Predictors
            foreach (IPredictor predictor in _predictorCollection)
            {
                predictor.Update(activation, normalizedActivation, spike);
            }
            //Last activation
            _lastActivation = activation;
            _lastNormalizedActivation = normalizedActivation;
            _lastSpike = spike;
            return;
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
            foreach (IPredictor predictor in _predictorCollection)
            {
                predictors[idx++] = predictor.Compute(_activationStat,
                                                      _activationDiffStat,
                                                      _activationMDW,
                                                      _firingMDW,
                                                      _lastActivation,
                                                      _lastNormalizedActivation,
                                                      _lastSpike
                                                      );
                ++count;
            }
            return count;
        }

        /// <summary>
        /// Returns array containing values of enabled predictors
        /// </summary>
        public double[] GetPredictors()
        {
            if (_predictorCollection.Count == 0)
            {
                return null;
            }
            double[] predictors = new double[_predictorCollection.Count];
            CopyPredictorsTo(predictors, 0);
            return predictors;
        }

    }//PredictorsProvider

}//Namespace
