using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using RCNet.Extensions;
using RCNet.MathTools;
using RCNet.Neural.Activation;
using RCNet.Neural.Network.SM.Neuron;

namespace RCNet.Neural.Network.SM.Synapse
{
    /// <summary>
    /// Abstract class covering the basic behaviour of implemented synapses.
    /// </summary>
    [Serializable]
    public abstract class BaseSynapse
    {
        //Enums
        /// <summary>
        /// Method to decide synapse delay
        /// </summary>
        public enum SynapticDelayMethod
        {
            /// <summary>
            /// Synapse delay is decided randomly
            /// </summary>
            Random,
            /// <summary>
            /// Synapse delay is decided according to Euclidean distance
            /// </summary>
            Distance
        }

        /// <summary>
        /// Target scope of the synapse
        /// </summary>
        public enum SynapticTargetScope
        {
            /// <summary>
            /// Both Excitatory and Inhibitory neurons
            /// </summary>
            All,
            /// <summary>
            /// Excitatory neurons only
            /// </summary>
            Excitatory,
            /// <summary>
            /// Inhibitory neurons only
            /// </summary>
            Inhibitory
        }

        //Attribute properties
        /// <summary>
        /// Source neuron - signal emitter
        /// </summary>
        public INeuron SourceNeuron { get; }

        /// <summary>
        /// Target neuron - signal receiver
        /// </summary>
        public INeuron TargetNeuron { get; }

        /// <summary>
        /// Euclidean distance between SourceNeuron and TargetNeuron
        /// </summary>
        public double Distance { get; }

        /// <summary>
        /// Weight of the synapse (the maximum weight synapse can achieve)
        /// </summary>
        public double Weight { get; protected set; }

        /// <summary>
        /// Efficacy statistics of the synapse.
        /// </summary>
        public BasicStat EfficacyStat { get; }

 
        //Constructor
        /// <summary>
        /// Creates initialized instance
        /// </summary>
        /// <param name="sourceNeuron">Source neuron</param>
        /// <param name="targetNeuron">Target neuron</param>
        /// <param name="weight">Synapse initial weight</param>
        public BaseSynapse(INeuron sourceNeuron,
                           INeuron targetNeuron,
                           double weight
                           )
        {
            //Neurons to be connected
            SourceNeuron = sourceNeuron;
            TargetNeuron = targetNeuron;
            //Euclidean distance
            Distance = EuclideanDistance.Compute(SourceNeuron.Placement.ReservoirCoordinates, TargetNeuron.Placement.ReservoirCoordinates);
            //Weight sign rules
            if (SourceNeuron.Role == NeuronCommon.NeuronRole.Input)
            {
                if (TargetNeuron.TypeOfActivation == ActivationType.Analog)
                {
                    //No change of the weight sign
                    Weight = weight;
                }
                else
                {
                    //Target is spiking neuron
                    //Weight must be always positive
                    Weight = Math.Abs(weight);
                }
            }
            else
            {
                //Weight sign is dependent on source neuron role
                Weight = Math.Abs(weight) * (SourceNeuron.Role == NeuronCommon.NeuronRole.Excitatory ? 1d : -1d);
            }
            //Efficacy statistics
            EfficacyStat = new BasicStat(false);
            return;
        }

        //Methods
        /// <summary>
        /// Parses method to decide synapse delay from a string code
        /// </summary>
        /// <param name="code">Method to decide synapse delay code</param>
        public static SynapticDelayMethod ParseSynapticDelayMethod(string code)
        {
            switch (code.ToUpper())
            {
                case "RANDOM": return SynapticDelayMethod.Random;
                case "DISTANCE": return SynapticDelayMethod.Distance;
                default:
                    throw new ArgumentException($"Unsupported synapse delay decision method: {code}", "code");
            }
        }

        /// <summary>
        /// Parses target scope of the synapse from a string code
        /// </summary>
        /// <param name="code">Scope code</param>
        public static SynapticTargetScope ParseSynapticTargetScope(string code)
        {
            switch (code.ToUpper())
            {
                case "ALL": return SynapticTargetScope.All;
                case "EXCITATORY": return SynapticTargetScope.Excitatory;
                case "INHIBITORY": return SynapticTargetScope.Inhibitory;
                default:
                    throw new ArgumentException($"Unsupported synaptic target scope {code}", "code");
            }
        }

        /// <summary>
        /// Rescales the synapse weight.
        /// </summary>
        /// <param name="factor">Scale factor</param>
        public void Rescale(double factor)
        {
            Weight *= factor;
            return;
        }

    }//BaseSynapse

}//Namespace
