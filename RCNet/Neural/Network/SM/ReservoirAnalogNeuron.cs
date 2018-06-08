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
    /// Reservoir neuron is the main type of the neuron processing input stimuli and emitting output signal.
    /// Analog neuron produces analog output
    /// </summary>
    [Serializable]
    public class ReservoirAnalogNeuron : INeuron
    {
        //Attributes
        /// <summary>
        /// Range of the rescalled state value. Allways (0,1)
        /// </summary>
        private static readonly Interval _rescalledStateRange = new Interval(0, 1);

        /// <summary>
        /// Neuron's activation function (the heart of the neuron)
        /// </summary>
        private IActivationFunction _activation;

        /// <summary>
        /// If specified, neuron is the leaky intgrator
        /// </summary>
        private double _retainmentRatio;

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
        /// Signal ready for transmission
        /// </summary>
        private double _transmissionSignal;

        //Attribute properties
        /// <summary>
        /// Determines whether neuron's signal is excitatory or inhibitory.
        /// </summary>
        public CommonEnums.NeuronSignalType TransmissionSignalType { get; }

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
        /// Statistics of neuron's uniformly rescalled state values
        /// </summary>
        public BasicStat StatesStat { get; }

        /// <summary>
        /// Statistics of neuron transmission signal in _transmissionSignalRange
        /// </summary>
        public BasicStat TransmissionSignalStat { get; }

        /// <summary>
        /// Statistics of neuron's transmission signal frequency
        /// </summary>
        public BasicStat TransmissionFreqStat { get; }


        //Constructor
        /// <summary>
        /// Creates an initialized instance
        /// </summary>
        /// <param name="placement">Home pool identificator and neuron placement within the pool.</param>
        /// <param name="transmissionSignalType">Type of the neuron's signal (Excitatory/Inhibitory).</param>
        /// <param name="activation">Instantiated activation function.</param>
        /// <param name="bias">Constant bias.</param>
        /// <param name="retainmentRatio">Retainment ratio.</param>
        public ReservoirAnalogNeuron(NeuronPlacement placement,
                                     CommonEnums.NeuronSignalType transmissionSignalType,
                                     IActivationFunction activation,
                                     double bias,
                                     double retainmentRatio
                                     )
        {
            Placement = placement;
            TransmissionSignalType = transmissionSignalType;
            Bias = bias;
            //Check whether function is analog
            if (activation.OutputSignalType != ActivationFactory.FunctionOutputSignalType.Analog)
            {
                throw new ArgumentException("Activation function is not analog.", "activation");
            }
            _activation = activation;
            _retainmentRatio = retainmentRatio;
            StimuliStat = new BasicStat();
            StatesStat = new BasicStat();
            TransmissionSignalStat = new BasicStat();
            TransmissionFreqStat = new BasicStat();
            Reset(false);
            return;
        }

        //Properties
        /// <summary>
        /// Output range of associated activation function
        /// </summary>
        public Interval TransmissionSignalRange { get { return _activation.OutputSignalRange; } }

        /// <summary>
        /// Neuron's transmission signal
        /// </summary>
        public double TransmissionSignal { get { return _transmissionSignal; } }

        /// <summary>
        /// Value to be passed to readout layer as a predictor value.
        /// Available after the execution of PrepareTransmissionSignal function.
        /// </summary>
        public double ReadoutValue { get { return _transmissionSignal; } }

        /// <summary>
        /// Value to be passed to readout layer as an augmented predictor value
        /// Available after the execution of PrepareTransmissionSignal function.
        /// </summary>
        public double ReadoutAugmentedValue { get { return _transmissionSignal * _transmissionSignal; } }

        //Methods
        /// <summary>
        /// Resets the neuron to its initial state
        /// </summary>
        /// <param name="statistics">Specifies whether to reset also internal statistics</param>
        public void Reset(bool statistics)
        {
            _activation.Reset();
            _state = 0;
            _rescaledState = 0;
            _transmissionSignal = 0;
            if (statistics)
            {
                StimuliStat.Reset();
                StatesStat.Reset();
                TransmissionSignalStat.Reset();
                TransmissionFreqStat.Reset();
            }
            return;
        }

        /// <summary>
        /// Prepares and stores transmission signal
        /// </summary>
        public void PrepareTransmissionSignal()
        {
            _transmissionSignal = _signal;
            TransmissionSignalStat.AddSampleValue(_transmissionSignal);
            TransmissionFreqStat.AddSampleValue((_transmissionSignal == 0) ? 0 : 1);
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
            //State and signal
            _signal = (_retainmentRatio * _state) + (1d - _retainmentRatio) * _activation.Compute(stimuli);
            _state = _signal;
            //Compute rescaled state
            _rescaledState = _rescalledStateRange.Rescale(_state, _activation.InternalStateRange);
            //Statistics
            if (collectStatistics)
            {
                StatesStat.AddSampleValue(_rescaledState);
            }
            return;
        }

    }//ReservoirAnalogNeuron

}//Namespace
