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
        //Attribute properties
        /// <summary>
        /// Home pool identificator and neuron placement within the pool
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
        public ActivationFactory.FunctionOutputSignalType OutputType { get { return _activation.OutputSignalType; } }

        /// <summary>
        /// Output signal range
        /// </summary>
        public Interval OutputRange { get { return _activation.OutputSignalRange; } }

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
        private IActivationFunction _activation;

        /// <summary>
        /// If specified, neuron is the leaky intgrator
        /// </summary>
        private readonly double _retainmentRatio;

        /// <summary>
        /// Input stimulation
        /// </summary>
        private double _tStimuli;
        private double _rStimuli;

        //Constructor
        /// <summary>
        /// Creates an initialized instance
        /// </summary>
        /// <param name="placement">Home pool identificator and neuron placement within the pool.</param>
        /// <param name="role">Neuron's signal role (Excitatory/Inhibitory).</param>
        /// <param name="activation">Instantiated activation function.</param>
        /// <param name="bias">Constant bias.</param>
        /// <param name="retainmentRatio">Retainment ratio.</param>
        public ReservoirAnalogNeuron(NeuronPlacement placement,
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
            if (activation.OutputSignalType != ActivationFactory.FunctionOutputSignalType.Analog)
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
            _tStimuli = 0;
            _rStimuli = 0;
            OutputSignal = _activation.Compute(_tStimuli);
            OutputSignalLeak = 0;
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
            _tStimuli = (iStimuli + rStimuli + Bias).Bound();
            _rStimuli = rStimuli;
            return;
        }

        /// <summary>
        /// Computes neuron's new output signal and updates statistics
        /// </summary>
        /// <param name="collectStatistics">Specifies whether to update internal statistics</param>
        public void NewState(bool collectStatistics)
        {
            //Output signal leak handling
            if (OutputSignal != _activation.OutputSignalRange.Mid)
            {
                OutputSignalLeak = 0;
            }
            ++OutputSignalLeak;
            //New output signal
            double state = _activation.Compute(_tStimuli);
            OutputSignal = (_retainmentRatio * OutputSignal) + (1d - _retainmentRatio) * state;
            if (collectStatistics)
            {
                Statistics.Update(_tStimuli, _rStimuli, state, OutputSignal);
            }
            return;
        }


    }//ReservoirAnalogNeuron

}//Namespace
