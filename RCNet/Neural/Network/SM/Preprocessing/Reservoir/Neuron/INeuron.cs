using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using RCNet.MathTools;
using RCNet.Neural.Activation;
using RCNet.Neural.Network.SM.Preprocessing.Reservoir.Neuron.Predictor;

namespace RCNet.Neural.Network.SM.Preprocessing.Reservoir.Neuron
{
    /// <summary>
    /// Neuron is the basic unit of a neural network.
    /// </summary>
    public interface INeuron
    {
        //Attribute properties
        /// <summary>
        /// Information about a neuron location within the neural preprocessor
        /// </summary>
        NeuronLocation Location { get; }

        /// <summary>
        /// Neuron's key statistics
        /// </summary>
        NeuronStatistics Statistics { get; }

        /// <summary>
        /// Neuron type (input or hidden)
        /// </summary>
        NeuronCommon.NeuronType Type { get; }

        /// <summary>
        /// Type of the activation function
        /// </summary>
        ActivationType TypeOfActivation { get; }

        /// <summary>
        /// Output signaling restriction
        /// </summary>
        NeuronCommon.NeuronSignalingRestrictionType SignalingRestriction { get; }

        /// <summary>
        /// Computation cycles gone from the last emitted spike or start (if no spike emitted before current computation cycle)
        /// </summary>
        int SpikeLeak { get; }

        /// <summary>
        /// Specifies, if neuron has already emitted spike before current computation cycle
        /// </summary>
        bool AfterFirstSpike { get; }

        /// <summary>
        /// Number of provided predictors
        /// </summary>
        int NumOfEnabledPredictors { get; }
        
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
        /// Must be called only once per stored incoming stimulation.
        /// </summary>
        /// <param name="collectStatistics">Specifies whether to update internal statistics</param>
        void Recompute(bool collectStatistics);

        /// <summary>
        /// Neuron's output signal.
        /// </summary>
        /// <param name="targetActivationType">Specifies what type of the signal is required if possible</param>
        double GetSignal(ActivationType targetActivationType);

        /// <summary>
        /// Checks if given predictor is enabled
        /// </summary>
        /// <param name="predictorID">Identificator of the predictor</param>
        bool IsPredictorEnabled(PredictorsProvider.PredictorID predictorID);

        /// <summary>
        /// Copies values of enabled predictors to a given buffer starting from specified position (idx)
        /// </summary>
        /// <param name="predictors">Buffer where to be copied enabled predictors</param>
        /// <param name="idx">Starting position index</param>
        int CopyPredictorsTo(double[] predictors, int idx);

        /// <summary>
        /// Returns array containing values of enabled predictors
        /// </summary>
        double[] GetPredictors();

    }//INeuron

}//Namespace
