using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using RCNet.MathTools;
using RCNet.Neural.Activation;

namespace RCNet.Neural.Network.SM
{
    /// <summary>
    /// Neuron is the basic unit of a neural network.
    /// </summary>
    public interface INeuron
    {
        //Attribute properties
        /// <summary>
        /// Home pool identificator and neuron placement within the pool
        /// </summary>
        NeuronPlacement Placement { get; }

        /// <summary>
        /// Neuron's key statistics
        /// </summary>
        NeuronStatistics Statistics { get; }

        /// <summary>
        /// Neuron's role within the reservoir (excitatory or inhibitory)
        /// </summary>
        CommonEnums.NeuronRole Role { get; }

        /// <summary>
        /// Specifies whether to use neuron's secondary predictor
        /// </summary>
        bool UseSecondaryPredictor { get; }

        /// <summary>
        /// Type of the output signal (spike or analog)
        /// </summary>
        ActivationFactory.FunctionOutputSignalType OutputType { get; }

        /// <summary>
        /// Output signal range
        /// </summary>
        Interval OutputRange { get; }

        /// <summary>
        /// Constant bias
        /// </summary>
        double Bias { get; }

        /// <summary>
        /// Output signal
        /// </summary>
        double OutputSignal { get; }

        /// <summary>
        /// Value to be passed to readout layer as a primary predictor.
        /// </summary>
        double PrimaryPredictor { get; }

        /// <summary>
        /// Value to be passed to readout layer as an augmented predictor.
        /// </summary>
        double SecondaryPredictor { get; }

        //Methods
        /// <summary>
        /// Resets neuron to its initial state
        /// </summary>
        /// <param name="statistics">Specifies whether to reset internal statistics</param>
        void Reset(bool statistics);

        /// <summary>
        /// Stores new incoming stimulation.
        /// </summary>
        /// <param name="stimuli">Input stimulation</param>
        void NewStimuli(double stimuli);

        /// <summary>
        /// Computes neuron's new output signal and updates statistics
        /// </summary>
        /// <param name="collectStatistics">Specifies whether to update internal statistics</param>
        void NewState(bool collectStatistics);

    }//INeuron

}//Namespace
