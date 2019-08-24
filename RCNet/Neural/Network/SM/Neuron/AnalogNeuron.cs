using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using RCNet.Extensions;
using RCNet.MathTools;
using RCNet.Neural.Activation;

namespace RCNet.Neural.Network.SM.Neuron
{
    /// <summary>
    /// Analog neuron has analog activation function and produces analog output
    /// </summary>
    [Serializable]
    public class AnalogNeuron : INeuron
    {
        //Attribute properties
        /// <summary>
        /// Home pool identificator and neuron placement within the reservoir
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
        /// Type of the output signal (spike or analog)
        /// This neuron is analog
        /// </summary>
        public CommonEnums.NeuronSignalType OutputType { get { return _activation.OutputSignalType; } }

        /// <summary>
        /// Output signal range
        /// </summary>
        public Interval OutputRange { get { return _activation.OutputRange; } }

        /// <summary>
        /// Constant bias
        /// </summary>
        public double Bias { get; }

        /// <summary>
        /// Output signal
        /// </summary>
        public double OutputSignal { get; private set; }

        /// <summary>
        /// Computation cycles left without output signal
        /// </summary>
        public int OutputSignalLeak { get; private set; }

        /// <summary>
        /// Specifies, if neuron has already emitted output signal before current signal
        /// </summary>
        public bool AfterFirstOutputSignal { get; private set; }

        /// <summary>
        /// Value to be passed to readout layer as a primary predictor.
        /// </summary>
        public double PrimaryPredictor { get { return OutputSignal; } }

        /// <summary>
        /// Value to be passed to readout layer as an augmented predictor.
        /// </summary>
        public double SecondaryPredictor { get { return OutputSignal * OutputSignal; } }

        //Attributes
        /// <summary>
        /// Neuron's activation function (the heart of the neuron)
        /// </summary>
        private readonly IActivationFunction _activation;

        /// <summary>
        /// If specified, neuron is the leaky intgrator
        /// </summary>
        private readonly double _retainmentRatio;

        /// <summary>
        /// Stimulation
        /// </summary>
        private double _iStimuli;
        private double _rStimuli;
        private double _tStimuli;

        //Constructor
        /// <summary>
        /// Creates an initialized instance
        /// </summary>
        /// <param name="placement">Home pool identificator and neuron placement within the pool.</param>
        /// <param name="role">Neuron's signal role (Excitatory/Inhibitory).</param>
        /// <param name="activation">Instantiated activation function.</param>
        /// <param name="bias">Constant bias.</param>
        /// <param name="retainmentRatio">Retainment ratio.</param>
        public AnalogNeuron(NeuronPlacement placement,
                            CommonEnums.NeuronRole role,
                            IActivationFunction activation,
                            double bias,
                            double retainmentRatio
                            )
        {
            Placement = placement;
            Role = role;
            Bias = bias;
            //Check whether function is analog
            if (activation.OutputSignalType != CommonEnums.NeuronSignalType.Analog)
            {
                throw new ArgumentException("Activation function is not analog.", "activation");
            }
            _activation = activation;
            _retainmentRatio = retainmentRatio;
            Statistics = new NeuronStatistics(OutputRange);
            Reset(false);
            return;
        }


        //Methods
        /// <summary>
        /// Resets the neuron to its initial state
        /// </summary>
        /// <param name="statistics">Specifies whether to reset also internal statistics</param>
        public void Reset(bool statistics)
        {
            _activation.Reset();
            _iStimuli = 0;
            _rStimuli = 0;
            _tStimuli = 0;
            OutputSignal = _activation.Compute(_tStimuli);
            OutputSignalLeak = 0;
            AfterFirstOutputSignal = false;
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
        public void NewStimuli(double iStimuli, double rStimuli)
        {
            _iStimuli = iStimuli;
            _rStimuli = rStimuli;
            _tStimuli = (iStimuli + rStimuli + Bias);
            return;
        }

        /// <summary>
        /// Computes neuron's new output signal and updates statistics
        /// </summary>
        /// <param name="collectStatistics">Specifies whether to update internal statistics</param>
        public void NewState(bool collectStatistics)
        {
            //Output signal leak handling
            if (OutputSignal != _activation.OutputRange.Mid)
            {
                AfterFirstOutputSignal = true;
                OutputSignalLeak = 0;
            }
            ++OutputSignalLeak;
            //New output signal
            double state = _activation.Compute(_tStimuli);
            OutputSignal = (_retainmentRatio * OutputSignal) + (1d - _retainmentRatio) * state;
            if (collectStatistics)
            {
                Statistics.Update(_iStimuli, _rStimuli, _tStimuli, state, OutputSignal);
            }
            return;
        }


    }//AnalogNeuron

}//Namespace
