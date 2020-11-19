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
    /// Reservoir's hidden neuron
    /// </summary>
    [Serializable]
    public class HiddenNeuron : INeuron
    {
        //Static attributes
        private static readonly Interval _outputRange = new Interval(0, 1);

        //Attribute properties
        /// <summary>
        /// Information about a neuron location within the neural preprocessor
        /// </summary>
        public NeuronLocation Location { get; }

        /// <summary>
        /// Neuron's key statistics
        /// </summary>
        public NeuronStatistics Statistics { get; }

        /// <summary>
        /// Constant bias
        /// </summary>
        public double Bias { get; }

        /// <summary>
        /// Neuron's output data
        /// </summary>
        public NeuronOutputData OutputData { get; }

        //Attributes
        /// <summary>
        /// Neuron's activation function
        /// </summary>
        private readonly IActivationFunction _activation;

        /// <summary>
        /// Analog firing
        /// </summary>
        private readonly double _analogFiringThreshold;
        private readonly SimpleQueue<double> _histActivationsQueue;

        /// <summary>
        /// Retainment
        /// </summary>
        private readonly double _analogRetainmentStrength;

        /// <summary>
        /// Stimulation
        /// </summary>
        private double _iStimuli;
        private double _rStimuli;
        private double _tStimuli;

        /// <summary>
        /// Activation state
        /// </summary>
        private double _activationState;

        /// <summary>
        /// Predictors
        /// </summary>
        private readonly PredictorsProvider _predictors;

        //Constructor
        /// <summary>
        /// Creates an initialized instance of hidden neuron having spiking activation
        /// </summary>
        /// <param name="location">Information about a neuron location within the neural preprocessor</param>
        /// <param name="spikingActivation">Instantiated activation function.</param>
        /// <param name="bias">Constant bias to be applied.</param>
        /// <param name="predictorsCfg">Configuration of neuron's predictors</param>
        public HiddenNeuron(NeuronLocation location,
                            IActivationFunction spikingActivation,
                            double bias,
                            PredictorsSettings predictorsCfg
                            )
        {
            Location = location;
            Statistics = new NeuronStatistics();
            Bias = bias;
            //Activation check
            if (spikingActivation.TypeOfActivation == ActivationType.Analog)
            {
                //Spiking
                throw new InvalidOperationException($"Called wrong type of hidden neuron constructor for spiking activation.");
            }
            _activation = spikingActivation;
            _analogFiringThreshold = 0;
            _histActivationsQueue = null;
            _analogRetainmentStrength = 0;
            _predictors = predictorsCfg != null ? new PredictorsProvider(predictorsCfg) : null;
            OutputData = new NeuronOutputData();
            Reset(false);
            return;
        }

        //Constructor
        /// <summary>
        /// Creates an initialized instance of hidden neuron having analog activation
        /// </summary>
        /// <param name="location">Information about a neuron location within the neural preprocessor</param>
        /// <param name="analogActivation">Instantiated activation function.</param>
        /// <param name="bias">Constant bias to be applied.</param>
        /// <param name="firingThreshold">A number between 0 and 1 (LT1). Every time the new activation value is higher than the previous activation value by at least the threshold, it is evaluated as a firing event.</param>
        /// <param name="thresholdMaxRefDeepness">Maximum deepness of historical normalized activation value to be compared with current normalized activation value when evaluating firing event.</param>
        /// <param name="retainmentStrength">Strength of the analog neuron's retainment property.</param>
        /// <param name="predictorsCfg">Configuration of neuron's predictors</param>
        public HiddenNeuron(NeuronLocation location,
                            IActivationFunction analogActivation,
                            double bias,
                            double firingThreshold,
                            int thresholdMaxRefDeepness,
                            double retainmentStrength,
                            PredictorsSettings predictorsCfg
                            )
        {
            Location = location;
            Statistics = new NeuronStatistics();
            Bias = bias;
            //Activation check
            if (analogActivation.TypeOfActivation == ActivationType.Spiking)
            {
                throw new InvalidOperationException($"Called wrong type of hidden neuron constructor for analog activation.");
            }
            _activation = analogActivation;
            _analogFiringThreshold = firingThreshold;
            _histActivationsQueue = thresholdMaxRefDeepness < 2 ? null : new SimpleQueue<double>(thresholdMaxRefDeepness);
            _analogRetainmentStrength = retainmentStrength;
            _predictors = predictorsCfg != null ? new PredictorsProvider(predictorsCfg) : null;
            OutputData = new NeuronOutputData();
            Reset(false);
            return;
        }

        //Properties
        /// <summary>
        /// Neuron type
        /// </summary>
        public NeuronCommon.NeuronType Type { get { return NeuronCommon.NeuronType.Hidden; } }

        /// <summary>
        /// Type of the activation function
        /// </summary>
        public ActivationType TypeOfActivation { get { return _activation.TypeOfActivation; } }

        /// <summary>
        /// Number of computation cycles necessary to make neuron and its predictors fully operating
        /// </summary>
        public int RequiredHistLength { get { return _predictors == null ? 1 : _predictors.RequiredHistLength; } }

        /// <summary>
        /// Number of provided predictors
        /// </summary>
        public int NumOfEnabledPredictors { get { return _predictors == null ? 0 : _predictors.NumOfEnabledPredictors; } }

        //Methods
        /// <summary>
        /// Resets the neuron to its initial state
        /// </summary>
        /// <param name="statistics">Specifies whether to reset also internal statistics</param>
        public void Reset(bool statistics)
        {
            _activation.Reset();
            _predictors?.Reset();
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

        /// <summary>
        /// Stores new incoming stimulation.
        /// </summary>
        /// <param name="iStimuli">Stimulation comming from input neurons</param>
        /// <param name="rStimuli">Stimulation comming from reservoir neurons</param>
        public void NewStimulation(double iStimuli, double rStimuli)
        {
            _iStimuli = iStimuli;
            _rStimuli = rStimuli;
            _tStimuli = (_iStimuli + _rStimuli + Bias).Bound();
            return;
        }

        /// <summary>
        /// Computes neuron's new output signal and updates statistics
        /// </summary>
        /// <param name="collectStatistics">Specifies whether to update internal statistics</param>
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
            if (_activation.TypeOfActivation == ActivationType.Spiking)
            {
                //Spiking activation
                OutputData._spikingSignal = _activation.Compute(_tStimuli);
                //OutputData._analogSignal = OutputData._spikingSignal;
                _activationState = _activation.InternalState;
                normalizedActivation = _outputRange.Rescale(_activation.InternalState, _activation.InternalStateRange).Bound(_outputRange.Min, _outputRange.Max);
                OutputData._analogSignal = normalizedActivation;
            }
            else
            {
                //Analog activation
                _activationState = (_analogRetainmentStrength * _activationState) + (1d - _analogRetainmentStrength) * _activation.Compute(_tStimuli);
                normalizedActivation = _outputRange.Rescale(_activationState, _activation.OutputRange).Bound(_outputRange.Min, _outputRange.Max);
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
            _predictors?.Update(_activationState, normalizedActivation, (OutputData._spikingSignal > 0));
            //Update statistics
            if (collectStatistics)
            {
                Statistics.Update(_iStimuli, _rStimuli, _tStimuli, _activationState, OutputData._analogSignal, OutputData._spikingSignal);
            }
            return;
        }

        /// <summary>
        /// Checks if given predictor is enabled
        /// </summary>
        /// <param name="predictorID">Identificator of the predictor</param>
        public bool IsPredictorEnabled(PredictorsProvider.PredictorID predictorID)
        {
            if (_predictors != null && _predictors.IsPredictorEnabled(predictorID))
            {
                return true;
            }
            return false;
        }

        /// <summary>
        /// Copies values of enabled predictors to a given buffer starting from specified position (idx)
        /// </summary>
        /// <param name="predictors">Buffer where to be copied enabled predictors</param>
        /// <param name="idx">Starting position index</param>
        public int CopyPredictorsTo(double[] predictors, int idx)
        {
            if (_predictors == null)
            {
                return 0;
            }
            else
            {
                return _predictors.CopyPredictorsTo(predictors, idx);
            }
        }

        /// <summary>
        /// Returns array containing values of enabled predictors
        /// </summary>
        public double[] GetPredictors()
        {
            return _predictors?.GetPredictors();
        }

        /// <summary>
        /// Returns identifiers of enabled predictors in the same order as is used in the methods CopyPredictorsTo and GetPredictors
        /// </summary>
        public List<PredictorsProvider.PredictorID> GetEnabledPredictorsIDs()
        {
            return _predictors?.GetEnabledIDs();
        }


    }//HiddenNeuron

}//Namespace
