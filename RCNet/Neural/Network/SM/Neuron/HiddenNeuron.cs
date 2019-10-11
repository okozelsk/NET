using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using RCNet.Extensions;
using RCNet.MathTools;
using RCNet.Neural.Activation;
using RCNet.Neural.Network.SM.Preprocessing;

namespace RCNet.Neural.Network.SM.Neuron
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
        /// Home pool identifier and neuron placement within the reservoir
        /// </summary>
        public NeuronPlacement Placement { get; }

        /// <summary>
        /// Neuron's key statistics
        /// </summary>
        public NeuronStatistics Statistics { get; }

        /// <summary>
        /// Neuron's role within the reservoir (excitatory or inhibitory)
        /// </summary>
        public NeuronCommon.NeuronRole Role { get; }

        /// <summary>
        /// Output signaling restriction
        /// </summary>
        public NeuronCommon.NeuronSignalingRestrictionType SignalingRestriction { get; }

        /// <summary>
        /// Constant bias
        /// </summary>
        public double Bias { get; }

        /// <summary>
        /// Computation cycles gone from the last emitted spike or start (if no spike emitted before current computation cycle)
        /// </summary>
        public int SpikeLeak { get; private set; }

        /// <summary>
        /// Specifies, if neuron has already emitted spike before current computation cycle
        /// </summary>
        public bool AfterFirstSpike { get; private set; }

        /// <summary>
        /// Swithes enabling/disabling neuron's predictors
        /// </summary>
        public HiddenNeuronPredictorsSettings PredictorsCfg { get; }

        //Attributes
        /// <summary>
        /// Neuron's activation function
        /// </summary>
        private readonly IActivationFunction _activation;

        /// <summary>
        /// Firing
        /// </summary>
        private readonly double _analogFiringThreshold;

        /// <summary>
        /// Stimulation
        /// </summary>
        private double _iStimuli;
        private double _rStimuli;
        private double _tStimuli;

        /// <summary>
        /// Retainment
        /// </summary>
        private readonly double _retainmentStrength;

        /// <summary>
        /// Activation state
        /// </summary>
        private double _activationState;

        /// <summary>
        /// Signals
        /// </summary>
        private double _analogSignal;
        private double _spikingSignal;

        /// <summary>
        /// Predictors
        /// </summary>
        private readonly HiddenNeuronPredictors _predictors;


        //Constructor
        /// <summary>
        /// Creates an initialized instance
        /// </summary>
        /// <param name="placement">Home pool identificator and neuron placement within the pool.</param>
        /// <param name="role">Neuron's role (Excitatory/Inhibitory).</param>
        /// <param name="activation">Instantiated activation function.</param>
        /// <param name="signalingRestriction">Output signaling restriction. Spiking activation causes output signal always restricted to SpikingOnly.</param>
        /// <param name="predictorsCfg">Enabled/Disabled predictors for the neuron.</param>
        /// <param name="bias">Constant bias to be applied.</param>
        /// <param name="analogFiringThreshold">A number between 0 and 1 (LT1). Every time the new activation value is higher than the previous activation value by at least the threshold, it is evaluated as a firing event. Ignored in case of spiking activation.</param>
        /// <param name="retainmentStrength">Strength of the analog neuron's retainment property. Ignored in case of spiking activation.</param>
        public HiddenNeuron(NeuronPlacement placement,
                            NeuronCommon.NeuronRole role,
                            IActivationFunction activation,
                            NeuronCommon.NeuronSignalingRestrictionType signalingRestriction,
                            HiddenNeuronPredictorsSettings predictorsCfg,
                            double bias = 0,
                            double analogFiringThreshold = PoolSettings.NeuronGroupSettings.DefaultAnalogFiringThreshold,
                            double retainmentStrength = 0
                            )
        {
            Placement = placement;
            Statistics = new NeuronStatistics();
            if (role == NeuronCommon.NeuronRole.Input)
            {
                throw new ArgumentException("Role of the hidden neuron can not be Input.", "role");
            }
            Role = role;
            Bias = bias;
            PredictorsCfg = predictorsCfg;
            //Activation specific
            _activation = activation;
            if (activation.TypeOfActivation == ActivationType.Spiking)
            {
                //Spiking
                SignalingRestriction = NeuronCommon.NeuronSignalingRestrictionType.SpikingOnly;
                _analogFiringThreshold = 0;
                _retainmentStrength = 0;
            }
            else
            {
                //Analog
                SignalingRestriction = signalingRestriction;
                _analogFiringThreshold = analogFiringThreshold;
                _retainmentStrength = retainmentStrength;
            }
            _predictors = predictorsCfg != null ? new HiddenNeuronPredictors(predictorsCfg) : null;
            Reset(false);
            return;
        }

        //Properties
        /// <summary>
        /// Type of the activation function
        /// </summary>
        public ActivationType TypeOfActivation { get { return _activation.TypeOfActivation; } }

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
            _analogSignal = 0;
            _spikingSignal = 0;
            SpikeLeak = 0;
            AfterFirstSpike = false;
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
        public void ComputeSignal(bool collectStatistics)
        {
            //Spike leak handling
            if (_spikingSignal > 0)
            {
                //Spike during previous cycle, so reset the counter
                AfterFirstSpike = true;
                SpikeLeak = 0;
            }
            ++SpikeLeak;

            if(_activation.TypeOfActivation == ActivationType.Spiking)
            {
                //Spiking activation
                _spikingSignal = _activation.Compute(_tStimuli);
                _activationState = _activation.InternalState;
                _analogSignal = _spikingSignal;
            }
            else
            {
                //Analog activation
                double prevActivationState = _activationState;
                double newState = _activation.Compute(_tStimuli);
                _activationState = (_retainmentStrength * _activationState) + (1d - _retainmentStrength) * newState;
                _analogSignal = _outputRange.Rescale(_activationState, _activation.OutputRange);
                bool firingEvent = (_activationState - prevActivationState) > _analogFiringThreshold;
                _spikingSignal = firingEvent ? 1d : 0d;
            }
            //Update predictors
            _predictors?.Update(_activationState, (_spikingSignal > 0));
            //Update statistics
            if (collectStatistics)
            {
                Statistics.Update(_iStimuli, _rStimuli, _tStimuli, _activationState, _analogSignal, _spikingSignal);
            }
            return;
        }

        /// <summary>
        /// Neuron returns previously computed signal of required type (if possible).
        /// Type of finally returned signal depends on specified targetActivationType and signaling restriction of the neuron.
        /// Signal is always within the range [0,1]
        /// </summary>
        /// <param name="targetActivationType">Specifies what type of the signal is preferred.</param>
        public double GetSignal(ActivationType targetActivationType)
        {
            if (SignalingRestriction != NeuronCommon.NeuronSignalingRestrictionType.NoRestriction)
            {
                //Apply internal restriction
                return SignalingRestriction == NeuronCommon.NeuronSignalingRestrictionType.AnalogOnly ? _analogSignal : _spikingSignal;
            }
            else
            {
                //Return signal according to targetActivationType
                return targetActivationType == ActivationType.Analog ? _analogSignal : _spikingSignal;
            }
        }

        /// <summary>
        /// Copies values of enabled predictors to a given buffer starting from specified position (idx)
        /// </summary>
        /// <param name="predictors">Buffer where to be copied enabled predictors</param>
        /// <param name="idx">Starting position index</param>
        /// <returns></returns>
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
        /// <returns></returns>
        public double[] GetPredictors()
        {
            return _predictors?.GetPredictors();
        }


    }//HiddenNeuron

}//Namespace
