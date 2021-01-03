using RCNet.MathTools;
using RCNet.Queue;
using System;
using System.Collections.Generic;

namespace RCNet.Neural.Network.SM.Preprocessing.Neuron.Predictor
{
    /// <summary>
    /// Implements the provider of the unified set of computed predictors.
    /// </summary>
    [Serializable]
    public class PredictorsProvider
    {
        /// <summary>
        /// An identifier of the predictor.
        /// </summary>
        public enum PredictorID
        {
            /// <summary>
            /// The result of the activation function.
            /// </summary>
            Activation,
            /// <summary>
            /// The powered absolute value of the result of the activation function.
            /// </summary>
            ActivationPower,
            /// <summary>
            /// The statistical figure computed from the activation function results.
            /// </summary>
            ActivationStatFigure,
            /// <summary>
            /// The rescaled range computed from the activation function results.
            /// </summary>
            ActivationRescaledRange,
            /// <summary>
            /// The linearly weighted average computed from the activation function results.
            /// </summary>
            ActivationLinWAvg,
            /// <summary>
            /// The statistical figure computed from the differences of the activation function results (A[T] - A[T-1]).
            /// </summary>
            ActivationDiffStatFigure,
            /// <summary>
            /// The rescaled range computed from the differences of the activation function results (A[T] - A[T-1]).
            /// </summary>
            ActivationDiffRescaledRange,
            /// <summary>
            /// The linearly weighted average computed from the differences of the activation function results (A[T] - A[T-1]).
            /// </summary>
            ActivationDiffLinWAvg,
            /// <summary>
            /// The traced neuron's firing.
            /// </summary>
            FiringTrace

        }//PredictorID


        //Attribute properties
        /// <summary>
        /// The number of provided predictors.
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
        /// <param name="cfg">The configuration.</param>
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
                if (predictor.Cfg.NeedsContinuousActivationStat && _activationStat == null)
                {
                    _activationStat = new BasicStat();
                }
                if (predictor.Cfg.NeedsContinuousActivationDiffStat && _activationDiffStat == null)
                {
                    _activationDiffStat = new BasicStat();
                }
                _predictorCollection.Add(predictor);
            }

            if (reqAMDWCapacity > 0)
            {
                _activationMDW = new MovingDataWindow(reqAMDWCapacity);
            }
            if (reqFMDWCapacity > 0)
            {
                _firingMDW = new SimpleQueue<byte>(reqFMDWCapacity);
            }
            Reset();
            return;
        }

        //Properties
        /// <summary>
        /// The necessary length of recent history.
        /// </summary>
        public int RequiredHistLength { get { return Math.Max(_activationMDW == null ? 1 : _activationMDW.Capacity, _firingMDW == null ? 1 : _firingMDW.Capacity); } }

        //Methods
        /// <summary>
        /// Resets the predictors provider to its initial state.
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
        /// Gets the identifiers of provided predictors in the same order as in the methods CopyPredictorsTo and GetPredictors.
        /// </summary>
        public List<PredictorID> GetIDs()
        {
            List<PredictorID> ids = new List<PredictorID>(_predictorCollection.Count);
            foreach (IPredictor predictor in _predictorCollection)
            {
                ids.Add(predictor.Cfg.ID);
            }
            return ids;
        }

        /// <summary>
        /// Updates the predictors provider.
        /// </summary>
        /// <param name="activation">The current value of the activation.</param>
        /// <param name="normalizedActivation">The current value of the activation normalized between 0 and 1.</param>
        /// <param name="spike">Indicates whether the neuron is currently firing.</param>
        public void Update(double activation, double normalizedActivation, bool spike)
        {
            //Continuous statistics
            double activationDiff = activation - _lastActivation;
            _activationStat?.AddSample(activation);
            _activationDiffStat?.AddSample(activationDiff);
            //Data windows
            _activationMDW?.AddSample(activation);
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
        /// Copies the computed predictors into a buffer starting from the specified position.
        /// </summary>
        /// <param name="buffer">The buffer.</param>
        /// <param name="idx">The zero-based starting position.</param>
        /// <returns>The number of copied values.</returns>
        public int CopyPredictorsTo(double[] buffer, int idx)
        {
            int count = 0;
            foreach (IPredictor predictor in _predictorCollection)
            {
                buffer[idx++] = predictor.Compute(_activationStat,
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
        /// Gets the array of computed predictors.
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
