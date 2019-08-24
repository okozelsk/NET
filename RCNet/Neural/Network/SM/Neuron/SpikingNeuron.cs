using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using RCNet.Extensions;
using RCNet.MathTools;
using RCNet.Neural.Activation;

namespace RCNet.Neural.Network.SM.Neuron
{
    /// <summary>
    /// Spiking neuron has spiking activation function and produces spikes
    /// </summary>
    [Serializable]
    public class SpikingNeuron : INeuron
    {
        //Static attributes
        /// <summary>
        /// Range of the rescalled state value. Allways (0,1)
        /// </summary>
        private static readonly Interval _rescalledStateRange = new Interval(0, 1);
        
        //Attribute properties
        /// <summary>
        /// Home pool identificator and neuron placement within the reservoir
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
        public CommonEnums.NeuronSignalType OutputType { get { return _activation.OutputSignalType; } }

        /// <summary>
        /// Output signal range
        /// </summary>
        public Interval OutputRange { get { return _activation.OutputRange; } }

        /// <summary>
        /// Constant bias
        /// </summary>
        public double Bias { get; }

        /// <summary>
        /// Output signal
        /// </summary>
        public double OutputSignal { get; private set; }

        /// <summary>
        /// Computation cycles gone from the last emitted signal
        /// </summary>
        public int OutputSignalLeak { get; private set; }

        /// <summary>
        /// Specifies, if neuron has already emitted output signal before current signal
        /// </summary>
        public bool AfterFirstOutputSignal { get; private set; }

        /// <summary>
        /// Value to be passed to readout layer as a primary predictor
        /// (number of recent spikes + rescalled current membrane potential as a fraction)
        /// </summary>
        public double PrimaryPredictor
        {
            get
            {
                double rescalledState = _rescalledStateRange.Rescale(_activation.InternalState, _activation.InternalStateRange);
                double fraction = 1d / (1d + Math.Exp(-_activation.InternalState));
                return (_firingRate.NumOfRecentSpikes) + fraction;
            }
        }

        /// <summary>
        /// Value to be passed to readout layer as an augmented predictor
        /// (exponentially weighted firing rate)
        /// </summary>
        public double SecondaryPredictor { get { return _firingRate.GetRate(); } }

        //Attributes
        /// <summary>
        /// Neuron's activation function (the heart of the neuron)
        /// </summary>
        private readonly IActivationFunction _activation;

        /// <summary>
        /// Firing rate computer
        /// </summary>
        private readonly FiringRate _firingRate;

        /// <summary>
        /// Stimulation
        /// </summary>
        private double _iStimuli;
        private double _rStimuli;
        private double _tStimuli;

        //Constructor
        /// <summary>
        /// Creates an initialized instance
        /// </summary>
        /// <param name="placement">Home pool identificator and neuron placement within the pool.</param>
        /// <param name="role">Neuron's signal role (Excitatory/Inhibitory).</param>
        /// <param name="activation">Instantiated activation function.</param>
        /// <param name="bias">Constant bias.</param>
        public SpikingNeuron(NeuronPlacement placement,
                             CommonEnums.NeuronRole role,
                             IActivationFunction activation,
                             double bias
                             )
        {
            Placement = placement;
            Role = role;
            Bias = bias;
            //Check whether function is spiking
            if (activation.OutputSignalType != CommonEnums.NeuronSignalType.Spike)
            {
                throw new ArgumentException("Activation function is not spiking.", "activation");
            }
            _activation = activation;
            _firingRate = new FiringRate();
            Statistics = new NeuronStatistics(_activation.InternalStateRange);
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
            _iStimuli = 0;
            _rStimuli = 0;
            _tStimuli = 0;
            OutputSignal = 0;
            OutputSignalLeak = 0;
            AfterFirstOutputSignal = false;
            if (statistics)
            {
                Statistics.Reset();
            }
            return;
        }

        /// <summary>
        /// Stores new incoming stimulation.
        /// </summary>
        /// <param name="iStimuli">Stimulation comming from input neurons</param>
        /// <param name="rStimuli">Stimulation comming from reservoir neurons</param>
        public void NewStimuli(double iStimuli, double rStimuli)
        {
            _iStimuli = iStimuli;
            _rStimuli = rStimuli;
            _tStimuli = (iStimuli + rStimuli + Bias).Bound();
            return;
        }

        /// <summary>
        /// Computes neuron's new output signal and updates statistics
        /// </summary>
        /// <param name="collectStatistics">Specifies whether to update internal statistics</param>
        public void NewState(bool collectStatistics)
        {
            //Output signal leak handling
            if (OutputSignal > 0)
            {
                //Spike during previous cycle, so reset the counter
                AfterFirstOutputSignal = true;
                OutputSignalLeak = 0;
            }
            ++OutputSignalLeak;
            //New output signal
            OutputSignal = _activation.Compute(_tStimuli);
            _firingRate.Update(OutputSignal > 0);
            if (collectStatistics)
            {
                Statistics.Update(_iStimuli, _rStimuli, _tStimuli, _activation.InternalState, OutputSignal);
            }
            return;
        }

    }//SpikingNeuron

}//Namespace
