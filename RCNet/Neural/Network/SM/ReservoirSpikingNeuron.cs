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
    /// Spiking neuron produces spikes.
    /// </summary>
    [Serializable]
    public class ReservoirSpikingNeuron : INeuron
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
        /// Current state of the neuron in activation function range
        /// </summary>
        private double _state;

        /// <summary>
        /// Current state of the neuron rescaled to uniform range
        /// </summary>
        private double _rescaledState;

        /// <summary>
        /// Computed spike of the neuron
        /// </summary>
        private double _spike;

        /// <summary>
        /// Firing rate computer
        /// </summary>
        private FiringRate _firingRate;

        /// <summary>
        /// Signal ready for transmission
        /// </summary>
        private double _transmissionSignal;

        /// <summary>
        /// Computed readout value of the neuron
        /// </summary>
        private double _readout;

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
        public ReservoirSpikingNeuron(NeuronPlacement placement,
                                     CommonEnums.NeuronSignalType transmissionSignalType,
                                     IActivationFunction activation,
                                     double bias
                                     )
        {
            _firingRate = new FiringRate();
            Placement = placement;
            TransmissionSignalType = transmissionSignalType;
            Bias = bias;
            //Check whether function is spiking
            if (activation.OutputSignalType != ActivationFactory.FunctionOutputSignalType.Spike)
            {
                throw new ArgumentException("Activation function is not spiking.", "activation");
            }
            _activation = activation;
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
        public double ReadoutValue { get { return _readout; } }

        /// <summary>
        /// Value to be passed to readout layer as an augmented predictor value
        /// Available after the execution of PrepareTransmissionSignal function.
        /// </summary>
        public double ReadoutAugmentedValue { get { return _rescaledState; } }

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
            _spike = 0;
            _firingRate.Reset();
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
            _transmissionSignal = _spike;
            TransmissionSignalStat.AddSampleValue(_transmissionSignal);
            TransmissionFreqStat.AddSampleValue((_transmissionSignal == 0) ? 0 : 1);
            //Primary readout
            _readout = _firingRate.GetRate();
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
            //State and spike
            _spike = _activation.Compute(stimuli);
            _state = _activation.InternalState;
            _firingRate.Update(_spike > 0);
            //Compute rescaled state
            _rescaledState = _rescalledStateRange.Rescale(_state, _activation.InternalStateRange);
            //Statistics
            if (collectStatistics)
            {
                StatesStat.AddSampleValue(_rescaledState);
            }
            return;
        }

    }//ReservoirSpikingNeuron

}//Namespace
