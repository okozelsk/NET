using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using RCNet.MathTools;
using RCNet.Queue;

namespace RCNet.Neural.Network.SM
{
    /// <summary>
    /// Abstract class covering the basic behaviour of StateMachine synapses.
    /// (TODO - Consider removing the ISynapse interface)
    /// </summary>
    [Serializable]
    public abstract class Synapse : ISynapse
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
        /// Resulting efficacy statistics of the synapse.
        /// (product of Pre-synaptic and Post-synaptic)
        /// </summary>
        public BasicStat EfficacyStat { get; }

        /// <summary>
        /// Weight of the synapse (the maximum weight synapse can achieve)
        /// </summary>
        public double Weight { get; private set; }

        //Attributes
        /// <summary>
        /// "Add" part of the signal conversion operation
        /// </summary>
        protected readonly double _add;

        /// <summary>
        /// "Divide by" part of the signal conversion operation
        /// </summary>
        protected readonly double _div;
        
        /// <summary>
        /// Moving signal queue
        /// </summary>
        protected readonly SimpleQueue<Signal> _qSig;

        //Constructor
        /// <summary>
        /// Creates initialized instance
        /// </summary>
        /// <param name="sourceNeuron">Source neuron</param>
        /// <param name="targetNeuron">Target neuron</param>
        /// <param name="weight">Synapse weight</param>
        /// <param name="delay">Delay (in cycles) of the signal delivery</param>
        public Synapse(INeuron sourceNeuron,
                       INeuron targetNeuron,
                       double weight,
                       int delay
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
            /*
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
            */

            //Setup signal queue
            _qSig = new SimpleQueue<Signal>(delay + 1);
            //Efficacy statistics
            EfficacyStat = new BasicStat();
            return;
        }

        //Methods
        /// <summary>
        /// Resets synapse.
        /// </summary>
        /// <param name="statistics">Specifies whether to reset also internal statistics</param>
        public virtual void Reset(bool statistics)
        {
            _qSig.Reset();
            if (statistics)
            {
                EfficacyStat.Reset();
            }
            return;
        }

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
        /// Computes synapse efficacy based on the pre-synaptic activity
        /// </summary>
        protected abstract double GetPreSynapticEfficacy();


        /// <summary>
        /// Computes synapse efficacy based on the post-synaptic activity
        /// </summary>
        protected abstract double GetPostSynapticEfficacy();

        /// <summary>
        /// Returns signal to be delivered to target neuron.
        /// Note that this function has to be invoked only once per cycle !!!
        /// </summary>
        /// <param name="collectStatistics">Specifies whether to update internal statistics</param>
        public double GetSignal(bool collectStatistics)
        {
            //We are getting source neuron signal
            double sourceSignal = SourceNeuron.OutputSignal;
            if(sourceSignal == 0)
            {
                //No need to adjust anything
                _qSig.Enqueue(new Signal { _weightedSignal = 0d, _preSynapticEfficacy = 1d });
            }
            else
            {
                //Compute pre-synaptic efficacy
                double preSynapticEfficacy = GetPreSynapticEfficacy();
                //Compute constantly weighted signal and pre-synaptic part of efficacy and put them into the queue simulating the signal traveling
                _qSig.Enqueue(new Signal { _weightedSignal = ((SourceNeuron.OutputSignal + _add) / _div) * Weight, _preSynapticEfficacy = preSynapticEfficacy });
            }
            //Is there any signal to be delivered?
            if (_qSig.Full)
            {
                //Queue is full, so synapse is ready to deliver
                Signal signal = _qSig.Dequeue();
                if (signal._weightedSignal == 0)
                {
                    //No need to adjust anything
                    return 0;
                }
                else
                {
                    //Compute current post-synaptic efficacy
                    double postSynapticEfficacy = GetPostSynapticEfficacy();
                    double efficacy = signal._preSynapticEfficacy * postSynapticEfficacy;
                    if (collectStatistics)
                    {
                        EfficacyStat.AddSampleValue(efficacy);
                    }
                    //Deliver the resulting signal
                    return signal._weightedSignal * efficacy;
                }
            }
            else
            {
                //No signal to be delivered, the first signal is "still on the road"
                return 0;
            }
        }

        //Inner classes
        /// <summary>
        /// Data to be queued
        /// </summary>
        [Serializable]
        protected class Signal
        {
            public double _weightedSignal;
            public double _preSynapticEfficacy;
        }//Signal

    }//Synapse

}//Namespace
