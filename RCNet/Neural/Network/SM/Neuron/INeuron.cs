using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using RCNet.MathTools;
using RCNet.Neural.Activation;

namespace RCNet.Neural.Network.SM.Neuron
{
    /// <summary>
    /// Neuron is the basic unit of a neural network.
    /// </summary>
    public interface INeuron
    {
        //Attribute properties
        /// <summary>
        /// Home pool identifier and neuron placement within the pool
        /// </summary>
        NeuronPlacement Placement { get; }

        /// <summary>
        /// Neuron's key statistics
        /// </summary>
        NeuronStatistics Statistics { get; }

        /// <summary>
        /// Neuron's role within the reservoir (excitatory or inhibitory)
        /// </summary>
        NeuronCommon.NeuronRole Role { get; }

        /// <summary>
        /// Type of the activation function
        /// </summary>
        ActivationType TypeOfActivation { get; }

        /// <summary>
        /// Output signaling restriction
        /// </summary>
        NeuronCommon.NeuronSignalingRestrictionType SignalingRestriction { get; }
        
        /// <summary>
        /// Constant bias
        /// </summary>
        double Bias { get; }

        /// <summary>
        /// Computation cycles gone from the last emitted spike or start (if no spike emitted before current computation cycle)
        /// </summary>
        int SpikeLeak { get; }

        /// <summary>
        /// Specifies, if neuron has already emitted spike before current computation cycle
        /// </summary>
        bool AfterFirstSpike { get; }

        //Methods
        /// <summary>
        /// Resets neuron to its initial state
        /// </summary>
        /// <param name="statistics">Specifies whether to reset internal statistics</param>
        void Reset(bool statistics);

        /// <summary>
        /// Stores new incoming stimulation.
        /// </summary>
        /// <param name="iStimuli">Stimulation comming from input neurons</param>
        /// <param name="rStimuli">Stimulation comming from reservoir neurons</param>
        void NewStimulation(double iStimuli, double rStimuli);

        /// <summary>
        /// Computes neuron's new output signal, updates SpikeLeak, AfterFirstSpike and Statistics.
        /// Must be called only once per reservoir computation cycle.
        /// </summary>
        /// <param name="collectStatistics">Specifies whether to update internal statistics</param>
        void ComputeSignal(bool collectStatistics);

        /// <summary>
        /// Neuron's output signal.
        /// </summary>
        /// <param name="targetActivationType">Specifies what type of the signal is required if possible</param>
        double GetSignal(ActivationType targetActivationType);


    }//INeuron

}//Namespace
