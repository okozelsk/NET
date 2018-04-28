using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using RCNet.Extensions;
using RCNet.MathTools;
using RCNet.Neural.Activation;

namespace RCNet.Neural.Network.SM
{
    /// <summary>
    /// Reservoir neuron is the main type of the neuron.
    /// Reservoir neuron processes input stimuli and produces output signal.
    /// </summary>
    [Serializable]
    public class ReservoirAnalogNeuron : INeuron
    {
        //Attributes
        /// <summary>
        /// Rescalled state value range allways (0,1)
        /// </summary>
        private static readonly Interval _rescalledStateRange = new Interval(0, 1);

        /// <summary>
        /// Transmission signal range allways (0,1)
        /// </summary>
        private static Interval _transmissionSignalRange = new Interval(0, 1);

        /// <summary>
        /// Neuron's activation function (the heart of the neuron)
        /// </summary>
        private IActivationFunction _activation;

        /// <summary>
        /// If specified, neuron is the leaky intgrator
        /// </summary>
        private double _retainmentRatio;

        /// <summary>
        /// Number of passed neuron computations
        /// </summary>
        private int _numOfComputationCycles;

        /// <summary>
        /// Current state of the neuron in activation function range
        /// </summary>
        private double _state;

        /// <summary>
        /// Current state of the neuron rescaled to uniform range
        /// </summary>
        private double _rescaledState;

        /// <summary>
        /// Computed signal of the neuron in _transmissionSignalRange
        /// </summary>
        private double _signal;

        /// <summary>
        /// Prepared transmission signal in _transmissionSignalRange
        /// </summary>
        private double _transmissionSignal;

        /// <summary>
        /// Computed readout value of the neuron in _rescalledStateRange
        /// </summary>
        private double _readout;


        //Attribute properties
        /// <summary>
        /// Home pool identificator and neuron placement within the pool
        /// </summary>
        public NeuronPlacement Placement { get; }

        /// <summary>
        /// Constant bias of the neuron
        /// </summary>
        public double Bias { get; }

        /// <summary>
        /// Statistics of incoming stimulations (input values)
        /// </summary>
        public BasicStat StimuliStat { get; }
        
        /// <summary>
        /// Statistics of neuron rescalled state values
        /// </summary>
        public BasicStat StatesStat { get; }

        /// <summary>
        /// Determines whether neuron's signal is excitatory or inhibitory.
        /// </summary>
        public CommonEnums.NeuronSignalType TransmissionSignalType { get; }

        /// <summary>
        /// Statistics of neuron transmission signal in _transmissionSignalRange
        /// </summary>
        public BasicStat TransmissinSignalStat { get; }


        //Constructor
        /// <summary>
        /// Creates an initialized instance
        /// </summary>
        /// <param name="placement">Home pool identificator and neuron placement within the pool.</param>
        /// <param name="signalType">Type of the neuron signal (inhibitory/excitatory).</param>
        /// <param name="activation">Instantiated activation function.</param>
        /// <param name="bias">Constant bias.</param>
        /// <param name="retainmentRatio">Retainment ratio.</param>
        public ReservoirAnalogNeuron(NeuronPlacement placement,
                                     CommonEnums.NeuronSignalType signalType,
                                     IActivationFunction activation,
                                     double bias,
                                     double retainmentRatio = 0
                                     )
        {
            Placement = placement;
            Bias = bias;
            TransmissionSignalType = signalType;
            //Check type of activation
            if (activation.TimeDependent)
            {
                throw new ArgumentException("Time dependent activation is not allowed for analog neuron", "activation");
            }
            //Check whether activation function input range meets the requirements
            if (activation.InputRange.Min != double.NegativeInfinity.Bound() ||
                activation.InputRange.Max != double.PositiveInfinity.Bound()
               )
            {
                throw new ArgumentException("Input range of the activation function does not meet neuron conditions.", "activation");
            }
            //Check whether activation function output range meets the requirements
            if (activation.OutputRange.Min <= double.NegativeInfinity.Bound() || activation.OutputRange.Max >= double.PositiveInfinity.Bound())
            {
                throw new ArgumentException("Output range of the activation function does not meet neuron conditions.", "activation");
            }
            //Check retainment ratio
            if (retainmentRatio < 0)
            {
                throw new ArgumentOutOfRangeException("retainmentRatio", "Retainment ratio must be GE 0.");
            }
            _activation = activation;
            _retainmentRatio = retainmentRatio;
            StimuliStat = new BasicStat();
            StatesStat = new BasicStat();
            TransmissinSignalStat = new BasicStat();
            Reset(false);
            return;
        }

        //Properties
        /// <summary>
        /// Stored output signal for transmission purposes
        /// </summary>
        public double TransmissinSignal { get { return _transmissionSignal; } }

        /// <summary>
        /// Value to be passed to readout layer as a predictor value
        /// </summary>
        public double ReadoutPredictorValue { get { return _readout; } }

        /// <summary>
        /// Value to be passed to readout layer as an augmented predictor value
        /// </summary>
        public double ReadoutAugmentedPredictorValue { get { return (_readout * _readout); } }

        //Methods
        /// <summary>
        /// Resets the neuron to its initial state
        /// </summary>
        /// <param name="statistics">Specifies whether to reset also internal statistics</param>
        public void Reset(bool statistics)
        {
            _activation.Reset();
            _numOfComputationCycles = 0;
            _state = 0;
            _rescaledState = 0;
            _signal = 0;
            _transmissionSignal = 0;
            if (statistics)
            {
                StimuliStat.Reset();
                StatesStat.Reset();
                TransmissinSignalStat.Reset();
            }
            return;
        }

        /// <summary>
        /// Prepares and stores transmission signal
        /// </summary>
        public void PrepareTransmissionSignal()
        {
            _transmissionSignal = _signal;
            TransmissinSignalStat.AddSampleValue(_transmissionSignal);
            return;
        }

        /// <summary>
        /// Prepares and stores readout value
        /// </summary>
        public void PrepareReadoutValue()
        {
            _readout = _transmissionSignal;
            return;
        }

        /// <summary>
        /// Computes the neuron.
        /// </summary>
        /// <param name="stimuli">Input stimulation</param>
        /// <param name="collectStatistics">Specifies whether to update internal statistics</param>
        public void Compute(double stimuli, bool collectStatistics)
        {
            stimuli = (stimuli + Bias).Bound();
            if (collectStatistics)
            {
                StimuliStat.AddSampleValue(stimuli);
            }

            //Analog leaky integrator
            if (_numOfComputationCycles == 0 || _retainmentRatio == 0)
            {
                //In case of the first computation or zero retairment, retainment formula is not applied
                _state = _activation.Compute(stimuli);
            }
            else
            {
                //Apply retainment
                _state = (_retainmentRatio * _state) + (1d - _retainmentRatio) * _activation.Compute(stimuli);
            }
            //Compute rescaled state
            _rescaledState = _rescalledStateRange.Rescale(_state, _activation.InternalStateRange);
            //Compute rescaled signal
            _signal = _transmissionSignalRange.Rescale(_state, _activation.InternalStateRange);
            //Statistics
            if (collectStatistics)
            {
                StatesStat.AddSampleValue(_rescaledState);
            }
            //Cycles counter
            ++_numOfComputationCycles;
            return;
        }


    }//ReservoirAnalogNeuron

}//Namespace
