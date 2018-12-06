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
        private static readonly Interval _rescalledStateRange = new Interval(-1, 1);
        
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
        /// Specifies whether to use neuron's secondary predictor.
        /// </summary>
        public bool UseSecondaryPredictor { get; }

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
        /// Computation cycles between previous output signal and current output signal.
        /// </summary>
        public int NoSignalCycles { get; private set; }

        /// <summary>
        /// Value to be passed to readout layer as a primary predictor
        /// (rescalled internal state of the neuron - membrane potential)
        /// </summary>
        public double PrimaryPredictor { get { return _rescalledStateRange.Rescale(_activation.InternalState, _activation.InternalStateRange); } }

        /// <summary>
        /// Value to be passed to readout layer as an augmented predictor
        /// (exponentially weighted firing rate)
        /// </summary>
        public double SecondaryPredictor { get { return _firingRate.GetRate(); } }

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

        /// <summary>
        /// Local random generator
        /// </summary>
        private Random _rand;


        //Constructor
        /// <summary>
        /// Creates an initialized instance
        /// </summary>
        /// <param name="placement">Home pool identificator and neuron placement within the pool.</param>
        /// <param name="role">Neuron's signal role (Excitatory/Inhibitory).</param>
        /// <param name="activation">Instantiated activation function.</param>
        /// <param name="bias">Constant bias.</param>
        /// <param name="useSecondaryPredictor">Specifies whether to use neuron's secondary predictor.</param>
        public ReservoirSpikingNeuron(NeuronPlacement placement,
                                      CommonEnums.NeuronRole role,
                                      IActivationFunction activation,
                                      double bias,
                                      bool useSecondaryPredictor
                                      )
        {
            Placement = placement;
            Role = role;
            Bias = bias;
            //Check whether function is spiking
            if (activation.OutputSignalType != ActivationFactory.FunctionOutputSignalType.Spike)
            {
                throw new ArgumentException("Activation function is not spiking.", "activation");
            }
            _activation = activation;
            _firingRate = new FiringRate();
            UseSecondaryPredictor = useSecondaryPredictor;
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
            _stimuli = 0;
            if (statistics)
            {
                Statistics.Reset();
            }
            _rand = new Random(Placement.PoolFlatIdx);
            //Set initially -1 to counter (no one spike spiked)
            NoSignalCycles = -1;
            return;
        }

        /// <summary>
        /// Stores new incoming stimulation.
        /// </summary>
        /// <param name="externalStimuli">Stimulation comming from input neurons</param>
        /// <param name="internalStimuli">Stimulation comming from reservoir neurons</param>
        public void NewStimuli(double externalStimuli, double internalStimuli)
        {
            //double noiseStrength = 0.1;
            double noise = _rand.NextGaussianDouble(0, 0.15);
            noise *= _rand.NextSign();
            _stimuli = (externalStimuli + internalStimuli + Bias + noise.Bound());
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
            if (OutputSignal > 0)
            {
                //Spike, so reset the counter
                NoSignalCycles = 0;
            }
            else
            {
                //No spike
                if (NoSignalCycles != -1)
                {
                    //Neuron has already spiked, so standardly increment counter
                    ++NoSignalCycles;
                }
            }
            if (collectStatistics)
            {
                Statistics.Update(_stimuli, _activation.InternalState, OutputSignal);
            }
            return;
        }


    }//ReservoirSpikingNeuron

}//Namespace
