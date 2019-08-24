using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using RCNet.Extensions;
using RCNet.MathTools;
using RCNet.Neural.Network.SM.Neuron;

namespace RCNet.Neural.Network.SM.Synapse
{
    /// <summary>
    /// Abstract class covering the basic behaviour of implemented synapses.
    /// </summary>
    [Serializable]
    public abstract class BaseSynapse
    {
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
        /// Signal delay
        /// </summary>
        public int Delay { get; protected set; }

        /// <summary>
        /// Efficacy statistics of the synapse.
        /// </summary>
        public BasicStat EfficacyStat { get; }

        //Attributes
        /// <summary>
        /// "Add" part of the signal conversion operation
        /// </summary>
        protected readonly double _add;

        /// <summary>
        /// "Divide by" part of the signal conversion operation
        /// </summary>
        protected readonly double _div;

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
            //Weight sign and signal range conversion rules
            if (SourceNeuron.Role == CommonEnums.NeuronRole.Input)
            {
                if (TargetNeuron.OutputType == CommonEnums.NeuronSignalType.Analog)
                {
                    //Target is analog neuron
                    //No signal conversion
                    _add = 0;
                    _div = 1;
                    //No change of the weight sign
                    Weight = weight;
                }
                else
                {
                    //Target is spiking neuron
                    //Convert signal to <0,1>
                    _add = -SourceNeuron.OutputRange.Min;
                    _div = SourceNeuron.OutputRange.Span;
                    //Weight is always positive
                    Weight = Math.Abs(weight);
                }
            }
            else
            {
                //Convert signal to <0,1>
                _add = -SourceNeuron.OutputRange.Min;
                _div = SourceNeuron.OutputRange.Span;
                //Weight sign is dependent on source neuron role
                Weight = Math.Abs(weight) * (SourceNeuron.Role == CommonEnums.NeuronRole.Excitatory ? 1d : -1d);
            }
            //Set Delay to 0 as default. It can be changed later by SetDelay method.
            Delay = 0;
            //Efficacy statistics
            EfficacyStat = new BasicStat(false);
            return;
        }


        //Methods
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
