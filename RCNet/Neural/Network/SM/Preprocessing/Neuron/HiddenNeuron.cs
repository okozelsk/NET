using RCNet.Extensions;
using RCNet.MathTools;
using RCNet.Neural.Activation;
using RCNet.Neural.Network.SM.Preprocessing.Neuron.Predictor;
using RCNet.Queue;
using System;
using System.Collections.Generic;

namespace RCNet.Neural.Network.SM.Preprocessing.Neuron
{
    /// <summary>
    /// Implements the hidden neuron.
    /// </summary>
    /// <remarks>
    /// Supports engagement of both analog and spiking activation functions and provides unified set of available predictors.
    /// </remarks>
    [Serializable]
    public class HiddenNeuron : INeuron
    {
        //Attribute properties
        /// <inheritdoc/>
        public NeuronLocation Location { get; }

        /// <inheritdoc/>
        public NeuronStatistics Statistics { get; }

        /// <summary>
        /// Constant bias
        /// </summary>
        public double Bias { get; }

        /// <inheritdoc/>
        public NeuronOutputData OutputData { get; }

        //Attributes
        private readonly IActivation _activationFn;
        private readonly PredictorsProvider _predictorsProvider;
        private readonly double _analogFiringThreshold;
        private readonly SimpleQueue<double> _histActivationsQueue;
        private readonly double _analogRetainmentStrength;
        private double _iStimuli;
        private double _rStimuli;
        private double _tStimuli;
        private double _activationState;

        //Constructor
        /// <summary>
        /// Creates an initialized instance.
        /// </summary>
        /// <param name="location">The neuron's location.</param>
        /// <param name="spikingActivation">The instance of a spiking activation function.</param>
        /// <param name="bias">The constant bias.</param>
        /// <param name="predictorsProviderCfg">The configuration of the predictors provider.</param>
        public HiddenNeuron(NeuronLocation location,
                            AFSpikingBase spikingActivation,
                            double bias,
                            PredictorsProviderSettings predictorsProviderCfg
                            )
        {
            Location = location;
            Statistics = new NeuronStatistics();
            Bias = bias;
            //Activation check
            if (spikingActivation.TypeOfActivation == ActivationType.Analog)
            {
                //Spiking
                throw new ArgumentException($"Wrong type of the activation function.", "spikingActivation");
            }
            _activationFn = spikingActivation;
            _analogFiringThreshold = 0;
            _histActivationsQueue = null;
            _analogRetainmentStrength = 0;
            _predictorsProvider = predictorsProviderCfg != null ? new PredictorsProvider(predictorsProviderCfg) : null;
            OutputData = new NeuronOutputData();
            Reset(false);
            return;
        }

        //Constructor
        /// <summary>
        /// Creates an initialized instance.
        /// </summary>
        /// <param name="location">The neuron's location.</param>
        /// <param name="analogActivation">The instance of an analog activation function.</param>
        /// <param name="bias">The constant bias.</param>
        /// <param name="firingThreshold">The firing threshold value. It must be GE0 and LT1. Every time the current normalized activation is higher than the normalized past reference activation by at least this threshold, it is evaluated as a firing event.</param>
        /// <param name="firingThresholdMaxRefDeepness">Maximum age of the past activation for the evaluation of the firing event.</param>
        /// <param name="retainmentStrength">The strength of the analog neuron's retainment property. It enables the leaky integrator feature of the neuron.</param>
        /// <param name="predictorsProviderCfg">The configuration of the predictors provider.</param>
        public HiddenNeuron(NeuronLocation location,
                            AFAnalogBase analogActivation,
                            double bias,
                            double firingThreshold,
                            int firingThresholdMaxRefDeepness,
                            double retainmentStrength,
                            PredictorsProviderSettings predictorsProviderCfg
                            )
        {
            Location = location;
            Statistics = new NeuronStatistics();
            Bias = bias;
            //Activation check
            if (analogActivation.TypeOfActivation == ActivationType.Spiking)
            {
                throw new ArgumentException($"Wrong type of the activation function.", "analogActivation");
            }
            _activationFn = analogActivation;
            _analogFiringThreshold = firingThreshold;
            _histActivationsQueue = firingThresholdMaxRefDeepness < 2 ? null : new SimpleQueue<double>(firingThresholdMaxRefDeepness);
            _analogRetainmentStrength = retainmentStrength;
            _predictorsProvider = predictorsProviderCfg != null ? new PredictorsProvider(predictorsProviderCfg) : null;
            OutputData = new NeuronOutputData();
            Reset(false);
            return;
        }

        //Static properties
        private static Interval OutputRange { get { return Interval.IntZP1; } }

        //Properties
        /// <inheritdoc/>
        public NeuronType Type { get { return NeuronType.Hidden; } }

        /// <inheritdoc/>
        public ActivationType TypeOfActivation { get { return _activationFn.TypeOfActivation; } }

        /// <summary>
        /// The number of computation cycles necessary to make the neuron and its predictors fully operable.
        /// </summary>
        public int RequiredHistLength { get { return _predictorsProvider == null ? 1 : _predictorsProvider.RequiredHistLength; } }

        /// <summary>
        /// The number of provided predictors.
        /// </summary>
        public int NumOfProvidedPredictors { get { return _predictorsProvider == null ? 0 : _predictorsProvider.NumOfProvidedPredictors; } }

        //Methods
        /// <inheritdoc/>
        public void Reset(bool statistics)
        {
            if (_activationFn.TypeOfActivation == ActivationType.Spiking)
            {
                ((AFSpikingBase)_activationFn).Reset();
            }
            _predictorsProvider?.Reset();
            _iStimuli = 0;
            _rStimuli = 0;
            _tStimuli = 0;
            _activationState = 0;
            _histActivationsQueue?.Reset();
            OutputData.Reset();
            if (statistics)
            {
                Statistics.Reset();
            }
            return;
        }

        /// <inheritdoc/>
        public void NewStimulation(double iStimuli, double rStimuli)
        {
            _iStimuli = iStimuli;
            _rStimuli = rStimuli;
            _tStimuli = (_iStimuli + _rStimuli + Bias).Bound();
            return;
        }

        /// <inheritdoc/>
        public void Recompute(bool collectStatistics)
        {
            //Spike leak handling
            if (OutputData._spikingSignal > 0)
            {
                //Spike during previous cycle, so reset the counter
                OutputData._afterFirstSpike = true;
                OutputData._spikeLeak = 0;
            }
            ++OutputData._spikeLeak;

            double normalizedActivation;
            if (_activationFn.TypeOfActivation == ActivationType.Spiking)
            {
                //Spiking activation
                AFSpikingBase af = (AFSpikingBase)_activationFn;
                OutputData._spikingSignal = af.Compute(_tStimuli);
                //OutputData._analogSignal = OutputData._spikingSignal;
                _activationState = af.InternalState;
                normalizedActivation = OutputRange.Rescale(af.InternalState, af.InternalStateRange).Bound(OutputRange.Min, OutputRange.Max);
                OutputData._analogSignal = normalizedActivation;
            }
            else
            {
                //Analog activation
                _activationState = (_analogRetainmentStrength * _activationState) + (1d - _analogRetainmentStrength) * _activationFn.Compute(_tStimuli);
                normalizedActivation = OutputRange.Rescale(_activationState, _activationFn.OutputRange).Bound(OutputRange.Min, OutputRange.Max);
                double activationDifference = _histActivationsQueue == null ? ((normalizedActivation - OutputData._analogSignal)) : (_histActivationsQueue.Full ? (normalizedActivation - _histActivationsQueue.Dequeue()) : (normalizedActivation - 0.5d));
                //Firing event decision
                bool firingEvent = activationDifference > _analogFiringThreshold;
                //Enqueue last normalized activation
                _histActivationsQueue?.Enqueue(normalizedActivation);
                //New output data
                OutputData._analogSignal = normalizedActivation;
                OutputData._spikingSignal = firingEvent ? 1d : 0d;
            }
            //Update predictors
            _predictorsProvider?.Update(_activationState, normalizedActivation, (OutputData._spikingSignal > 0));
            //Update statistics
            if (collectStatistics)
            {
                Statistics.Update(_iStimuli, _rStimuli, _tStimuli, _activationState, OutputData._analogSignal, OutputData._spikingSignal);
            }
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
            if (_predictorsProvider == null)
            {
                return 0;
            }
            else
            {
                return _predictorsProvider.CopyPredictorsTo(buffer, idx);
            }
        }

        /// <summary>
        /// Gets the array of computed predictors.
        /// </summary>
        public double[] GetPredictors()
        {
            return _predictorsProvider?.GetPredictors();
        }

        /// <summary>
        /// Gets the identifiers of provided predictors in the same order as in the methods CopyPredictorsTo and GetPredictors.
        /// </summary>
        public List<PredictorsProvider.PredictorID> GetPredictorsIDs()
        {
            return _predictorsProvider?.GetIDs();
        }


    }//HiddenNeuron

}//Namespace
