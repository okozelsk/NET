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
    public class ReservoirSpikingNeuron : INeuron
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
        /// Number of passed neuron computations
        /// </summary>
        private int _numOfComputationCycles;

        /// <summary>
        /// Spikes history
        /// </summary>
        private ulong _spikes;

        /// <summary>
        /// Spikes history to analog signal converter
        /// </summary>
        private SignalConverter _converter;

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
        /// Computed readout value of the neuron in _rescalledStateRange
        /// </summary>
        private double _readout;

        /// <summary>
        /// Prepared transmission signal in _transmissionSignalRange
        /// </summary>
        private double _transmissionSignal;


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
        /// <param name="bias">Constant input bias.</param>
        public ReservoirSpikingNeuron(NeuronPlacement placement,
                                     CommonEnums.NeuronSignalType signalType,
                                      IActivationFunction activation,
                                      double bias,
                                      int numOfCodingSpikes
                                      )
        {
            Placement = placement;
            Bias = bias;
            TransmissionSignalType = signalType;
            //Check type of activation
            if (!activation.TimeDependent)
            {
                throw new ArgumentException("Analog activation is not allowed for spiking neuron", "activation");
            }
            //Check whether activation function input range meets the requirements
            if (activation.InputRange.Min != double.NegativeInfinity.Bound() ||
                activation.InputRange.Max != double.PositiveInfinity.Bound()
               )
            {
                throw new ArgumentException("Input range of the activation function does not meet neuron conditions.", "activation");
            }
            //Check whether activation function output range meets the requirements
            if (activation.OutputRange.Min != 0 || activation.OutputRange.Max != 1)
            {
                throw new ArgumentException("Output range of the activation function does not meet spiking neuron conditions.", "activation");
            }
            _activation = activation;
            _converter = new SignalConverter(new Interval(0, 1), numOfCodingSpikes);
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
            _spikes = 0;
            _activation.Reset();
            _numOfComputationCycles = 0;
            _state = _activation.InternalState;
            _rescaledState = _rescalledStateRange.Rescale(_state, _activation.InternalStateRange);
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
            /*
            //A bit tricky fusion of the neuron firing rate, recent spiking history and current rescaled membrane potential
            double firingRate = TransmissinSignalStat.ArithAvg;
            _converter.EncodeSpikeTrain(_spikes);
            double recentSpikesValue = _converter.FetchAnalogValue();
            double membraneValue = _rescaledState;
            //Sigmoidal transformation
            _readout = 1d / (1d + Math.Exp(-(firingRate + recentSpikesValue + membraneValue)));
            */
            double firingRate = TransmissinSignalStat.ArithAvg;
            _converter.EncodeSpikeTrain(_spikes);
            double recentSpikes = _converter.FetchAnalogValue();
            _readout = SignalConverter.Mix(_rescalledStateRange,
                                           _rescaledState,
                                           firingRate,
                                           recentSpikes,
                                           32,
                                           20,
                                           1
                                           );
            if(TransmissinSignalStat.NumOfSamples >= 100)
            {
                ;
            }
            _readout = _rescaledState;
            return;
        }

        /// <summary>
        /// Computes neuron state and output signal.
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
            //Activation
            //Compute signal and neuron state
            _signal = _activation.Compute(stimuli);
            //Update spikes
            _spikes <<= 1;
            if (_signal > 0)
            {
                //Neuron is firing
                _spikes |= 1;
            }
            //Store membrane potential
            _state = _activation.InternalState;
            //Compute rescaled state
            _rescaledState = _rescalledStateRange.Rescale(_state, _activation.InternalStateRange);
            //Statistics
            if (collectStatistics)
            {
                StatesStat.AddSampleValue(_rescaledState);
            }
            //Cycles counter
            ++_numOfComputationCycles;
            return;
        }

    }//ReservoirSpikingNeuron

}//Namespace
