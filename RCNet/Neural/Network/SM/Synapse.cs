using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using RCNet.Queue;

namespace RCNet.Neural.Network.SM
{
    /// <summary>
    /// Base abstract class for various types of StateMachine synapses.
    /// </summary>
    [Serializable]
    public abstract class Synapse : ISynapse
    {
        //Attribute properties
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
        public double Weight { get; private set; }

        //Attributes
        protected readonly double _add;
        protected readonly double _div;
        protected readonly SimpleQueue<double> _qSig;

        //Constructor
        /// <summary>
        /// Creates initialized instance
        /// </summary>
        /// <param name="sourceNeuron">Source neuron</param>
        /// <param name="targetNeuron">Target neuron</param>
        /// <param name="weight">Synapse weight</param>
        /// <param name="maxDelay">Maximum delay (in cycles) of the signal delivery</param>
        public Synapse(INeuron sourceNeuron,
                       INeuron targetNeuron,
                       double weight,
                       int maxDelay
                       )
        {
            SourceNeuron = sourceNeuron;
            TargetNeuron = targetNeuron;
            //Weight sign and signal range conversion rules
            if (SourceNeuron.Role == CommonEnums.NeuronRole.Input)
            {
                //Source is input neuron
                if (SourceNeuron.OutputType == Activation.ActivationFactory.FunctionOutputSignalType.Spike)
                {
                    //Source input neuron is spiking
                    if (TargetNeuron.OutputType == Activation.ActivationFactory.FunctionOutputSignalType.Spike)
                    {
                        //Target is also spiking
                        //Ensure positive weight
                        Weight = Math.Abs(weight);
                        //Convert signal to <0,1>
                        _add = -SourceNeuron.OutputRange.Min;
                        _div = SourceNeuron.OutputRange.Span;
                    }
                    else
                    {
                        //Target is analog
                        //Weight is unchanged
                        Weight = weight;
                        //No signal conversion
                        _add = 0;
                        _div = 1;
                    }
                }
                else
                {
                    //Source input neuron is analog
                    if (TargetNeuron.OutputType == Activation.ActivationFactory.FunctionOutputSignalType.Spike)
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
            }
            else
            {
                //Source is reservoir neuron
                if (SourceNeuron.OutputType == Activation.ActivationFactory.FunctionOutputSignalType.Spike)
                {
                    //Source reservoir neuron is spiking
                    if (TargetNeuron.OutputType == Activation.ActivationFactory.FunctionOutputSignalType.Spike)
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
                    if (TargetNeuron.OutputType == Activation.ActivationFactory.FunctionOutputSignalType.Spike)
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
            /////////////////////////////////////////////////////////
            //Delay
            double maxDistance = targetNeuron.Placement.PoolDim.ComputeMaxDistance();
            int delay = 0;
            int[] sCoordinates = null;
            if (SourceNeuron.Role == CommonEnums.NeuronRole.Input)
            {
                //Input to pool
                //Consider input in the centre of the pool
                sCoordinates = new int[3];
                sCoordinates[0] = (int)Math.Round(TargetNeuron.Placement.PoolDim.X / 2d);
                sCoordinates[1] = (int)Math.Round(TargetNeuron.Placement.PoolDim.Y / 2d);
                sCoordinates[2] = (int)Math.Round(TargetNeuron.Placement.PoolDim.Z / 2d);
            }
            else if (SourceNeuron.Placement.PoolID != TargetNeuron.Placement.PoolID)
            {
                //Pool to another pool
                //No delay
                delay = 0;
            }
            else
            {
                //Within the pool
                sCoordinates = SourceNeuron.Placement.Coordinates;
            }
            double distance = PoolDimensions.ComputeEuclideanDistance(sCoordinates, TargetNeuron.Placement.Coordinates);
            double relDistance = distance / maxDistance;
            delay = (int)Math.Round(maxDelay * relDistance);
            //Setup signal queue
            _qSig = new SimpleQueue<double>(delay + 1);
            return;
        }

        //Methods
        /// <summary>
        /// Resets synapse.
        /// </summary>
        public abstract void Reset();

        /// <summary>
        /// Rescales the synapse weight.
        /// </summary>
        /// <param name="scale">Scale factor</param>
        public void Rescale(double scale)
        {
            Weight *= scale;
            return;
        }

        /// <summary>
        /// Computes weighted signal and puts it into the internal queue
        /// </summary>
        protected abstract void EnqueueSignal();

        /// <summary>
        /// Returns signal to be delivered to target neuron.
        /// Function has to be invoced only once!!!
        /// </summary>
        public double GetSignal()
        {
            EnqueueSignal();
            if (_qSig.Full)
            {
                //Synapse is ready to deliver
                return _qSig.Dequeue();
            }
            else
            {
                //Synapse is not ready
                return 0;
            }
        }

    }//Synapse

}//Namespace
