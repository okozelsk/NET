using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using RCNet.MathTools;

namespace RCNet.Neural.Network.SM
{
    /// <summary>
    /// Class encapsulates key statistics of the neuron
    /// </summary>
    [Serializable]
    public class NeuronStatistics
    {
        //Constants
        //Static attribute properties
        public static Interval NormalizedStateRange { get; } = new Interval(0, 1);

        //Attribute properties
        /// <summary>
        /// Statistics of incoming stimulations (input values)
        /// </summary>
        public BasicStat IncomingStimuli { get; }

        /// <summary>
        /// Statistics of neuron's uniformly rescalled state values
        /// </summary>
        public BasicStat NormalizedState { get; }

        /// <summary>
        /// Statistics of neuron transmission signal in _transmissionSignalRange
        /// </summary>
        public BasicStat OutgoingSignal { get; }

        /// <summary>
        /// Statistics of neuron's transmission signal frequency
        /// </summary>
        public BasicStat OutgoingSignalFreq { get; }


        //Constructor
        /// <summary>
        /// Creates an initialized instance
        /// </summary>
        public NeuronStatistics()
        {
            IncomingStimuli = new BasicStat();
            NormalizedState = new BasicStat();
            OutgoingSignal = new BasicStat();
            OutgoingSignalFreq = new BasicStat();
            return;
        }

        //Methods
        /// <summary>
        /// Resets all statistics
        /// </summary>
        public void Reset()
        {
            IncomingStimuli.Reset();
            NormalizedState.Reset();
            OutgoingSignal.Reset();
            OutgoingSignalFreq.Reset();
            return;
        }

        /// <summary>
        /// Updates statistics
        /// </summary>
        /// <param name="stimuli">Incoming stimuli</param>
        /// <param name="state">Neuron normalized state</param>
        /// <param name="signal">Outgoing signal</param>
        public void Update(double stimuli, double state, double signal)
        {
            IncomingStimuli.AddSampleValue(stimuli);
            NormalizedState.AddSampleValue(state);
            OutgoingSignal.AddSampleValue(signal);
            OutgoingSignalFreq.AddSampleValue((signal == 0) ? 0 : 1);
            return;
        }

    }//NeuronStatistics
}//Namespace
