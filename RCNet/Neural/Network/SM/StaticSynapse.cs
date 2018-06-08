using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using RCNet.MathTools;

namespace RCNet.Neural.Network.SM
{
    /// <summary>
    /// Static synapse computes constantly weighted signal from source to target neuron.
    /// </summary>
    [Serializable]
    public class StaticSynapse : ISynapse
    {
        //Properties
        /// <summary>
        /// Source neuron - signal emitor
        /// </summary>
        public INeuron SourceNeuron { get; }

        /// <summary>
        /// Target neuron - signal receiver
        /// </summary>
        public INeuron TargetNeuron { get; }
        
        /// <summary>
        /// Weight of the synapse
        /// </summary>
        public double Weight { get; set; }

        //Constructor
        /// <summary>
        /// Creates initialized instance
        /// </summary>
        /// <param name="sourceNeuron">Source neuron</param>
        /// <param name="targetNeuron">Target neuron</param>
        /// <param name="weight">Synapse weight (unsigned)</param>
        public StaticSynapse(INeuron sourceNeuron,
                             INeuron targetNeuron,
                             double weight
                             )
        {
            SourceNeuron = sourceNeuron;
            TargetNeuron = targetNeuron;
            if(weight == 0)
            {
                throw new ArgumentOutOfRangeException("weight", "Weight can't be equal to zero.");
            }
            //Weight absolute value
            Weight = Math.Abs(weight);
            //Weight sign
            Weight *= (SourceNeuron.TransmissionSignalType == CommonEnums.NeuronSignalType.Excitatory) ? 1 : -1;
            return;
        }

        //Methods
        /// <summary>
        /// Does nothing, this is a static synapse so weight remaining all the time unchanged.
        /// </summary>
        public void Adjust()
        {
            return;
        }

        /// <summary>
        /// Computes weighted signal of source neuron to be delivered to the target neuron.
        /// </summary>
        public double GetWeightedSignal()
        {
            double tSignal = TargetNeuron.TransmissionSignalRange.Rescale(SourceNeuron.TransmissionSignal, SourceNeuron.TransmissionSignalRange);
            tSignal *= Weight;
            return tSignal;
        }

    }//StaticSynapse

}//Namespace
