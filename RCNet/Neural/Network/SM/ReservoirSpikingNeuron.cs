using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using RCNet.Extensions;
using RCNet.MathTools;
using RCNet.Neural.Activation;

namespace RCNet.Neural.Network.SM
{
    /// <summary>
    /// Reservoir neuron is the main type of the neuron processing input stimuli and emitting output signal.
    /// Spiking neuron produces spikes.
    /// </summary>
    [Serializable]
    public class ReservoirSpikingNeuron : INeuron
    {
        //Static attributes
        /// <summary>
        /// Range of the rescalled state value. Allways (0,1)
        /// </summary>
        private static readonly Interval _rescalledStateRange = new Interval(0, 1);
        
        //Attribute properties
        /// <summary>
        /// Home pool identificator and neuron placement within the pool
        /// </summary>
        public NeuronPlacement Placement { get; }

        /// <summary>
        /// Neuron's key statistics
        /// </summary>
        public NeuronStatistics Statistics { get; }

        /// <summary>
        /// Neuron's role within the reservoir (excitatory or inhibitory)
        /// </summary>
        public CommonEnums.NeuronRole Role { get; }

        /// <summary>
        /// Type of the output signal (spike or analog)
        /// This neuron is spiking.
        /// </summary>
        public ActivationFactory.FunctionOutputSignalType OutputType { get { return _activation.OutputSignalType; } }

        /// <summary>
        /// Output signal range
        /// </summary>
        public Interval OutputRange { get { return _activation.OutputSignalRange; } }

        /// <summary>
        /// Constant bias
        /// </summary>
        public double Bias { get; }

        /// <summary>
        /// Output signal
        /// </summary>
        public double OutputSignal { get; private set; }

        /// <summary>
        /// Value to be passed to readout layer as a primary predictor.
        /// </summary>
        public double PrimaryPredictor { get { return _firingRate.GetRate(); } }

        /// <summary>
        /// Value to be passed to readout layer as an augmented predictor.
        /// </summary>
        public double SecondaryPredictor { get; private set; }

        //Attributes
        /// <summary>
        /// Neuron's activation function (the heart of the neuron)
        /// </summary>
        private IActivationFunction _activation;

        /// <summary>
        /// Firing rate computer
        /// </summary>
        private FiringRate _firingRate;

        /// <summary>
        /// Input stimulation
        /// </summary>
        private double _stimuli;


        //Constructor
        /// <summary>
        /// Creates an initialized instance
        /// </summary>
        /// <param name="placement">Home pool identificator and neuron placement within the pool.</param>
        /// <param name="role">Neuron's signal role (Excitatory/Inhibitory).</param>
        /// <param name="activation">Instantiated activation function.</param>
        /// <param name="bias">Constant bias.</param>
        public ReservoirSpikingNeuron(NeuronPlacement placement,
                                     CommonEnums.NeuronRole role,
                                     IActivationFunction activation,
                                     double bias
                                     )
        {
            Placement = placement;
            Statistics = new NeuronStatistics();
            Role = role;
            Bias = bias;
            //Check whether function is spiking
            if (activation.OutputSignalType != ActivationFactory.FunctionOutputSignalType.Spike)
            {
                throw new ArgumentException("Activation function is not spiking.", "activation");
            }
            _activation = activation;
            _firingRate = new FiringRate();
            Reset(false);
            return;
        }

        //Methods
        /// <summary>
        /// Resets the neuron to its initial state
        /// </summary>
        /// <param name="statistics">Specifies whether to reset also internal statistics</param>
        public void Reset(bool statistics)
        {
            _activation.Reset();
            _firingRate.Reset();
            _stimuli = 0;
            SecondaryPredictor = 0;
            if (statistics)
            {
                Statistics.Reset();
            }
            return;
        }

        /// <summary>
        /// Stores new incoming stimulation.
        /// </summary>
        /// <param name="stimuli">Input stimulation</param>
        public void NewStimuli(double stimuli)
        {
            _stimuli = (stimuli + Bias).Bound();
            return;
        }

        /// <summary>
        /// Computes neuron's new output signal and updates statistics
        /// </summary>
        /// <param name="collectStatistics">Specifies whether to update internal statistics</param>
        public void NewState(bool collectStatistics)
        {
            OutputSignal = _activation.Compute(_stimuli);
            _firingRate.Update(OutputSignal > 0);
            SecondaryPredictor = _rescalledStateRange.Rescale(_activation.InternalState, _activation.InternalStateRange);
            if (collectStatistics)
            {
                Statistics.Update(_stimuli, NeuronStatistics.NormalizedStateRange.Rescale(_activation.InternalState, _activation.InternalStateRange), OutputSignal);
            }
            return;
        }


    }//ReservoirSpikingNeuron

}//Namespace
