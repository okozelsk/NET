using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using RCNet.MathTools;

namespace RCNet.Neural.Reservoir
{
    /// <summary>
    /// Static synapse transfers constantly weighted signal from source to target neuron.
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
        /// <summary>
        /// Statistics of the transported signals
        /// </summary>
        public BasicStat SignalStat { get; }

        //Constructor
        /// <summary>
        /// Creates initialized instance
        /// </summary>
        /// <param name="sourceNeuron">Source neuron</param>
        /// <param name="targetNeuron">Target neuron</param>
        /// <param name="weight">Synapse weight</param>
        public StaticSynapse(INeuron sourceNeuron,
                             INeuron targetNeuron,
                             double weight
                             )
        {
            SourceNeuron = sourceNeuron;
            TargetNeuron = targetNeuron;
            if(weight == 0)
            {
                throw new ArgumentOutOfRangeException("weight", "Weight can't be zero.");
            }
            Weight = weight;
            SignalStat = new BasicStat();
            return;
        }

        //Methods
        /// <summary>
        /// Does nothing, this is a static synapse so weight remaining unchanged all the time.
        /// </summary>
        public void Adjust()
        {
            return;
        }

        /// <summary>
        /// Computes stimulating signal to be passed to target neuron.
        /// </summary>
        /// <param name="collectStatistics">Specifies whether to add signal into the internal statistics</param>
        /// <returns>Signal to target neuron</returns>
        public double ComputeSignal(bool collectStatistics)
        {
            double signal = SourceNeuron.StoredSignal * Weight;
            if(collectStatistics)SignalStat.AddSampleValue(signal);
            return signal;
        }

    }//StaticSynapse

}//Namespace
