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
    /// Input neuron is the special type of very simple neuron. Its purpose is only to mediate
    /// external input for a synapse.
    /// </summary>
    [Serializable]
    public class InputNeuron : INeuron
    {
        //Attribute properties
        /// <summary>
        /// Home pool identificator and neuron placement within the reservoir
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
        /// Type of the output signal (spike or analog)
        /// Input neuron is an analog neuron.
        /// </summary>
        public CommonEnums.NeuronSignalType OutputType { get { return CommonEnums.NeuronSignalType.Analog; } }

        /// <summary>
        /// Output signal range
        /// </summary>
        public Interval OutputRange { get; }

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
        /// Computation cycles gone from the last emitted signal
        /// </summary>
        public int OutputSignalLeak { get; private set; }

        /// <summary>
        /// Specifies, if neuron has already emitted output signal before current signal
        /// </summary>
        public bool AfterFirstOutputSignal { get; private set; }

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
        private double _iStimuli;
        private double _rStimuli;
        private double _tStimuli;

        //Constructor
        /// <summary>
        /// Creates an initialized instance
        /// </summary>
        /// <param name="inputEntryPoint">Input entry point coordinates within the reservoir.</param>
        /// <param name="inputFieldIdx">Index of the corresponding reservoir's input field.</param>
        /// <param name="inputRange">
        /// Range of input value.
        /// It is very recommended to have input values normalized and standardized before
        /// they are passed as an input.
        /// </param>
        public InputNeuron(int[] inputEntryPoint, int inputFieldIdx, Interval inputRange)
        {
            Placement = new NeuronPlacement(-1, inputFieldIdx, - 1, inputFieldIdx, 0, inputEntryPoint[0], inputEntryPoint[1], inputEntryPoint[2]);
            OutputRange = inputRange.DeepClone();
            Statistics = new NeuronStatistics(OutputRange);
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
            _iStimuli = 0;
            _rStimuli = 0;
            _tStimuli = 0;
            OutputSignal = 0;
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
        /// <param name="iStimuli">External input analog signal</param>
        /// <param name="rStimuli">Stimulation comming from reservoir neurons. Should be always 0.</param>
        public void NewStimuli(double iStimuli, double rStimuli)
        {
            _iStimuli = iStimuli;
            _rStimuli = rStimuli;
            _tStimuli = (iStimuli + rStimuli).Bound();
            return;
        }

        /// <summary>
        /// Computes neuron's new output signal and updates statistics
        /// </summary>
        /// <param name="collectStatistics">Specifies whether to update internal statistics</param>
        public void NewState(bool collectStatistics)
        {
            //Output signal leak handling
            if (OutputSignal != OutputRange.Mid)
            {
                AfterFirstOutputSignal = true;
                OutputSignalLeak = 0;
            }
            ++OutputSignalLeak;
            //New output signal
            OutputSignal = _tStimuli;
            if (collectStatistics)
            {
                Statistics.Update(_iStimuli, _rStimuli, _tStimuli, OutputSignal, OutputSignal);
            }
            return;
        }


    }//InputNeuron

}//Namespace
