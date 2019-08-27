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
        public CommonEnums.NeuronRole Role { get; }

        /// <summary>
        /// Output signaling restriction
        /// </summary>
        public CommonEnums.NeuronSignalingRestrictionType SignalingRestriction { get; }

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

        //Attributes
        /// <summary>
        /// Neuron's activation function (the heart of the neuron)
        /// </summary>
        private readonly IActivationFunction _activation;

        /// <summary>
        /// Firing
        /// </summary>
        private readonly double _analogFiringThreshold;
        private readonly FiringRate _firingRate;

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


        //Constructor
        /// <summary>
        /// Creates an initialized instance
        /// </summary>
        /// <param name="placement">Home pool identificator and neuron placement within the pool.</param>
        /// <param name="role">Neuron's role (Excitatory/Inhibitory).</param>
        /// <param name="activation">Instantiated activation function.</param>
        /// <param name="signalingRestriction">Output signaling restriction. Spiking activation has output signal always restricted to SpikingOnly.</param>
        /// <param name="bias">Constant bias to be applied.</param>
        /// <param name="analogFiringThreshold">Firing threshold to be applied in case of analog activation. Ignored in case of spiking activation.</param>
        /// <param name="retainmentStrength">Strength of the neuron's retainment property. Affected only in case of analog activation.</param>
        public HiddenNeuron(NeuronPlacement placement,
                            CommonEnums.NeuronRole role,
                            IActivationFunction activation,
                            CommonEnums.NeuronSignalingRestrictionType signalingRestriction,
                            double bias = 0,
                            double analogFiringThreshold = PoolSettings.NeuronGroupSettings.DefaultAnalogSpikeThreshold,
                            double retainmentStrength = 0
                            )
        {
            Placement = placement;
            Statistics = new NeuronStatistics();
            if(role == CommonEnums.NeuronRole.Input)
            {
                throw new ArgumentException("Role of the hidden neuron can not be Input.", "role");
            }
            Role = role;
            Bias = bias;
            //Activation specific
            _activation = activation;
            if (activation.ActivationType == CommonEnums.ActivationType.Spiking)
            {
                //Spiking
                SignalingRestriction = CommonEnums.NeuronSignalingRestrictionType.SpikingOnly;
                _analogFiringThreshold = 0;
                _retainmentStrength = 0;
            }
            else
            {
                //Anaolg
                SignalingRestriction = signalingRestriction;
                _analogFiringThreshold = analogFiringThreshold;
                _retainmentStrength = retainmentStrength;
            }
            _firingRate = new FiringRate();
            Reset(false);
            return;
        }

        //Properties
        /// <summary>
        /// Type of the activation function
        /// </summary>
        public CommonEnums.ActivationType ActivationType { get { return _activation.ActivationType; } }

        /// <summary>
        /// Value to be passed to readout layer as a primary predictor
        /// </summary>
        public double PrimaryPredictor
        {
            get
            {
                return _activationState;
            }
        }

        /// <summary>
        /// Value to be passed to readout layer as a secondary predictor
        /// </summary>
        public double SecondaryPredictor
        {
            get
            {
                if (SignalingRestriction == CommonEnums.NeuronSignalingRestrictionType.SpikingOnly)
                {
                    return _firingRate.GetRecentExpWRate();
                }
                else
                {
                    return _activationState * _activationState;
                }
            }
        }

        //Methods
        /// <summary>
        /// Resets the neuron to its initial state
        /// </summary>
        /// <param name="statistics">Specifies whether to reset also internal statistics</param>
        public void Reset(bool statistics)
        {
            _activation.Reset();
            _firingRate.Reset();
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

            if(_activation.ActivationType == CommonEnums.ActivationType.Spiking)
            {
                //Spiking activation
                _spikingSignal = _activation.Compute(_tStimuli);
                _activationState = _activation.InternalState;
                _firingRate.Update(_spikingSignal > 0);
                _analogSignal = _spikingSignal;
            }
            else
            {
                //Analog activation
                double newState = _activation.Compute(_tStimuli);
                _activationState = (_retainmentStrength * _activationState) + (1d - _retainmentStrength) * newState;
                _analogSignal = _outputRange.Rescale(_activationState, _activation.OutputRange);
                _firingRate.Update(_activationState > _analogFiringThreshold);
                _spikingSignal = _activationState > _analogFiringThreshold ? 1 : 0;
            }
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
        /// Signal is always within the range <0, 1>
        /// </summary>
        /// <param name="targetActivationType">Specifies what type of the signal is preferred.</param>
        public double GetSignal(CommonEnums.ActivationType targetActivationType)
        {
            if (SignalingRestriction != CommonEnums.NeuronSignalingRestrictionType.NoRestriction)
            {
                //Apply internal restriction
                return SignalingRestriction == CommonEnums.NeuronSignalingRestrictionType.AnalogOnly ? _analogSignal : _spikingSignal;
            }
            else
            {
                //Return signal according to targetActivationType
                return targetActivationType == CommonEnums.ActivationType.Analog ? _analogSignal : _spikingSignal;
            }
        }


    }//HiddenNeuron

}//Namespace
