using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using RCNet.MathTools;
using RCNet.Queue;
using RCNet.Neural.Network.SM.Neuron;

namespace RCNet.Neural.Network.SM.Synapse
{
    /// <summary>
    /// Abstract class covering the basic behaviour of StateMachine synapses.
    /// (TODO - Consider removal of the ISynapse interface)
    /// </summary>
    [Serializable]
    public abstract class BaseSynapse : ISynapse
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
        public double Weight { get; private set; }

        /// <summary>
        /// Signal delay
        /// </summary>
        public int Delay { get; private set; }

        /// <summary>
        /// Resulting efficacy statistics of the synapse.
        /// (product of Pre-synaptic and Post-synaptic)
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

        /// <summary>
        /// Moving signal queue
        /// </summary>
        protected SimpleQueue<Signal> _qSig;

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
            Distance = EuclideanDistance.Compute(SourceNeuron.Placement.Coordinates, TargetNeuron.Placement.Coordinates);
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
            _qSig = new SimpleQueue<Signal>(Delay + 1);
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
        /// Sets the synapse signal delay
        /// </summary>
        /// <param name="delay">Signal delay (reservoir cycles)</param>
        public void SetDelay(int delay)
        {
            //Set synapse signal delay
            Delay = delay;
            _qSig.Resize(Delay + 1);
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
            /// <summary>
            /// Weighted signal with no adjustments
            /// </summary>
            public double _weightedSignal;

            /// <summary>
            /// Computed synapse efficacy based on pre-synaptic activity
            /// </summary>
            public double _preSynapticEfficacy;

        }//Signal

    }//Synapse

}//Namespace
