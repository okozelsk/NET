using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using RCNet.MathTools;
using RCNet.Neural.Network.SM.Neuron;

namespace RCNet.Neural.Network.SM.Synapse
{
    /// <summary>
    /// Abstract class covering the basic behaviour of StateMachine synapses.
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
        /// <param name="weight">Synapse weight</param>
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
                //Source is input neuron
                if (TargetNeuron.OutputType == CommonEnums.NeuronSignalType.Spike)
                {
                    //Target is spiking
                    //Ensure positive weight
                    Weight = Math.Abs(weight);
                    //Convert signal to <0,1>
                    _add = -SourceNeuron.OutputRange.Min;
                    _div = SourceNeuron.OutputRange.Span;
                }
                else
                {
                    //Target is also analog
                    //Weight is unchanged
                    Weight = weight;
                    //No signal conversion
                    _add = 0;
                    _div = 1;
                }
            }
            else
            {
                //Source is reservoir neuron
                if (SourceNeuron.OutputType == CommonEnums.NeuronSignalType.Spike)
                {
                    //Source reservoir neuron is spiking
                    if (TargetNeuron.OutputType == CommonEnums.NeuronSignalType.Spike)
                    {
                        //Target is also spiking
                        //Weight dependent on source neuron role
                        Weight = Math.Abs(weight) * ((SourceNeuron.Role == CommonEnums.NeuronRole.Excitatory) ? 1 : -1);
                        //No signal conversion
                        _add = 0;
                        _div = 1;
                    }
                    else
                    {
                        //Target is analog
                        //Weight is unchanged
                        Weight = weight;
                        //Convert signal to target neuron range
                        _add = (TargetNeuron.OutputRange.Min - SourceNeuron.OutputRange.Min);
                        _div = (SourceNeuron.OutputRange.Span / TargetNeuron.OutputRange.Span);
                    }
                }
                else
                {
                    //Source reservoir neuron is analog
                    if (TargetNeuron.OutputType == CommonEnums.NeuronSignalType.Spike)
                    {
                        //Target is spiking
                        //Weight dependent on source neuron role
                        Weight = Math.Abs(weight) * ((SourceNeuron.Role == CommonEnums.NeuronRole.Excitatory) ? 1 : -1);
                        //Convert signal to <0,1>
                        _add = -SourceNeuron.OutputRange.Min;
                        _div = SourceNeuron.OutputRange.Span;
                    }
                    else
                    {
                        //Target is also analog
                        //Weight is unchanged
                        Weight = weight;
                        //No signal conversion
                        _add = 0;
                        _div = 1;
                    }
                }
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
        /// <param name="scale">Scale factor</param>
        public void Rescale(double scale)
        {
            Weight *= scale;
            return;
        }

    }//Synapse

}//Namespace
