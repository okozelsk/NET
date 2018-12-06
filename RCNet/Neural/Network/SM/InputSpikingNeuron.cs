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
    /// Spiking input neuron is the special type of neuron. Its purpose is to preprocess input analog data
    /// to be deliverable as the spike train signal into the reservoir neurons.
    /// </summary>
    [Serializable]
    public class InputSpikingNeuron : INeuron
    {
        //Static attributes
        /// <summary>
        /// Common output range 0/1 - no spike/spike
        /// </summary>
        private static readonly Interval _outputRange = new Interval(0, 1);

        //Attribute properties
        /// <summary>
        /// Home pool identificator and neuron placement within the pool
        /// Note that Input neuron home PoolID is always -1, because Input neurons do not belong to a physical pool.
        /// </summary>
        public NeuronPlacement Placement { get; }

        /// <summary>
        /// Neuron's key statistics
        /// </summary>
        public NeuronStatistics Statistics { get; }

        /// <summary>
        /// Neuron's role within the reservoir (input, excitatory or inhibitory)
        /// Note that Input neuron is always input.
        /// </summary>
        public CommonEnums.NeuronRole Role { get { return CommonEnums.NeuronRole.Input; } }

        /// <summary>
        /// Specifies whether to use neuron's secondary predictor.
        /// Input neuron never generates secondary predictor
        /// </summary>
        public bool UseSecondaryPredictor { get { return false; } }

        /// <summary>
        /// Type of the output signal (spike or analog)
        /// This is a spiking neuron.
        /// </summary>
        public ActivationFactory.FunctionOutputSignalType OutputType { get { return ActivationFactory.FunctionOutputSignalType.Spike; } }

        /// <summary>
        /// Output signal range
        /// </summary>
        public Interval OutputRange { get { return _outputRange; } }

        /// <summary>
        /// Constant bias.
        /// Note that Input neuron has bias always 0.
        /// </summary>
        public double Bias { get { return 0; } }

        /// <summary>
        /// Output signal
        /// </summary>
        public double OutputSignal { get; private set; }

        /// <summary>
        /// Computation cycles left without output signal
        /// </summary>
        public int NoSignalCycles { get; private set; }

        /// <summary>
        /// Value to be passed to readout layer as a primary predictor.
        /// Predictor value does not make sense in case of Input neuron.
        /// </summary>
        public double PrimaryPredictor { get { return double.NaN; } }

        /// <summary>
        /// Value to be passed to readout layer as an augmented predictor.
        /// Augmented predictor value does not make sense in case of Input neuron.
        /// </summary>
        public double SecondaryPredictor { get { return double.NaN; } }

        //Attributes
        private readonly Interval _inputRange;
        private double _stimuli;
        private SignalConverter _signalConverter;

        //Constructor
        /// <summary>
        /// Creates an initialized instance
        /// </summary>
        /// <param name="inputFieldIdx">Index of the corresponding reservoir's input field.</param>
        /// <param name="inputRange">
        /// Range of input value.
        /// It is very recommended to have input values normalized and standardized before
        /// they are passed to input neuron.
        /// </param>
        /// <param name="inputCodingFractions">Number of coding fractions (see SpikeTrainConverter to understand)</param>
        public InputSpikingNeuron(int inputFieldIdx, Interval inputRange, int inputCodingFractions)
        {
            Placement = new NeuronPlacement(inputFieldIdx , - 1, null, inputFieldIdx, 0, inputFieldIdx, 0, 0);
            _inputRange = inputRange.DeepClone();
            _signalConverter = new SignalConverter(_inputRange, inputCodingFractions);
            Statistics = new NeuronStatistics(_outputRange);
            Reset(false);
            return;
        }

        //Methods
        /// <summary>
        /// Resets neuron to its initial state
        /// </summary>
        /// <param name="statistics">Specifies whether to reset internal statistics</param>
        public void Reset(bool statistics)
        {
            _stimuli = 0;
            if (statistics)
            {
                Statistics.Reset();
            }
            //Set initially -1 to counter (no one spike spiked)
            NoSignalCycles = -1;
            return;
        }

        /// <summary>
        /// Stores new incoming stimulation.
        /// </summary>
        /// <param name="externalStimuli">Stimulation comming from input neurons</param>
        /// <param name="internalStimuli">Stimulation comming from reservoir neurons</param>
        public void NewStimuli(double externalStimuli, double internalStimuli)
        {
            _stimuli = (externalStimuli + internalStimuli).Bound();
            _signalConverter.EncodeAnalogValue(_stimuli);
            return;
        }

        /// <summary>
        /// Computes neuron's new output signal and updates statistics
        /// </summary>
        /// <param name="collectStatistics">Specifies whether to update internal statistics</param>
        public void NewState(bool collectStatistics)
        {
            OutputSignal = _signalConverter.FetchSpike();
            if (OutputSignal > 0)
            {
                //Spike, so reset the counter
                NoSignalCycles = 0;
            }
            else
            {
                //No spike
                if (NoSignalCycles != -1)
                {
                    //Neuron has already spiked, so standardly increment counter
                    ++NoSignalCycles;
                }
            }
            if (collectStatistics)
            {
                Statistics.Update(_stimuli, OutputSignal, OutputSignal);
            }
            return;
        }

    }//InputSpikingNeuron

}//Namespace
