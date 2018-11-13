using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using RCNet.Extensions;
using RCNet.MathTools;

namespace RCNet.Neural.Activation
{
    /// <summary>
    /// Implements very simple form of Integrate and Fire neuron model
    /// </summary>
    [Serializable]
    public class SimpleIF : IActivationFunction
    {
        //Constants
        private const double Spike = 1d;

        //Attributes
        private static readonly Interval _outputRange = new Interval(0, 1);
        private readonly Interval _stateRange;
        private readonly double _resistance;
        private readonly double _decayRate;
        private readonly double _restV;
        private readonly double _resetV;
        private readonly double _firingThresholdV;
        private readonly int _refractoryPeriods;
        private readonly double _stimuliCoeff;
        private double _membraneV;
        private bool _inRefractory;
        private int _refractoryPeriod;

        //Constructor
        /// <summary>
        /// Constructs an initialized instance
        /// </summary>
        /// <param name="settings">Encapsulated arguments settings</param>
        /// <param name="rand">Random object to be used for randomly generated parameters</param>
        public SimpleIF(SimpleIFSettings settings, Random rand)
        {
            _resistance = rand.NextDouble(settings.Resistance);
            _decayRate = rand.NextDouble(settings.DecayRate);
            _restV = 0;
            _resetV = Math.Abs(rand.NextDouble(settings.ResetV));
            _firingThresholdV = Math.Abs(rand.NextDouble(settings.FiringThresholdV));
            _refractoryPeriods = settings.RefractoryPeriods;
            _stimuliCoeff = settings.StimuliCoeff;
            _stateRange = new Interval(_restV, _firingThresholdV);
            Reset();
            return;
        }

        //Properties
        /// <summary>
        /// Type of the output
        /// </summary>
        public ActivationFactory.FunctionOutputSignalType OutputSignalType { get { return ActivationFactory.FunctionOutputSignalType.Spike; } }

        /// <summary>
        /// Output signal range
        /// </summary>
        public Interval OutputSignalRange { get { return _outputRange; } }

        /// <summary>
        /// Specifies whether the activation function supports derivative calculation
        /// </summary>
        public bool SupportsComputeDerivativeMethod { get { return false; } }

        /// <summary>
        /// Specifies whether the activation function is independent on its previous states
        /// </summary>
        public bool Stateless { get { return false; } }

        /// <summary>
        /// Normal range of the internal state
        /// </summary>
        public Interval InternalStateRange { get { return _stateRange; } }

        /// <summary>
        /// Internal state
        /// </summary>
        public double InternalState { get { return _membraneV; } }

        //Methods
        /// <summary>
        /// Resets function to its initial state
        /// </summary>
        public void Reset()
        {
            _membraneV = _restV;
            _inRefractory = false;
            _refractoryPeriod = 0;
            return;
        }

        /// <summary>
        /// Updates state of the membrane according to an input stimuli and when the firing
        /// condition is met, it produces a spike
        /// </summary>
        /// <param name="x">Input stimuli (interpreted as an electric current)</param>
        public double Compute(double x)
        {
            x = (x * _stimuliCoeff).Bound();
            double spike = 0;
            if (_membraneV >= _firingThresholdV)
            {
                //Membrane potential after spike
                _membraneV = _resetV;
                _refractoryPeriod = 0;
                _inRefractory = true;
            }
            if (_inRefractory)
            {
                ++_refractoryPeriod;
                if (_refractoryPeriod > _refractoryPeriods)
                {
                    _refractoryPeriod = 0;
                    _inRefractory = false;
                }
                else
                {
                    //Ignore input stimuli
                    x = 0;
                }
            }
            //Compute membrane new potential
            _membraneV = _restV + (_membraneV - _restV) * (1d - _decayRate) + _resistance * x;
            //Output
            if (_membraneV >= _firingThresholdV)
            {
                spike = Spike;
                _membraneV = _firingThresholdV;
            }
            return spike;
        }

        /// <summary>
        /// Unsupported functionality!!!
        /// Computes derivative of the activation input (does not change internal state)
        /// </summary>
        /// <param name="c">The result of the activation (Compute method)</param>
        /// <param name="x">Activation input (x argument of the Compute method)</param>
        public double ComputeDerivative(double c = double.NaN, double x = double.NaN)
        {
            throw new NotImplementedException("ComputeDerivative is unsupported method in case of spiking activation.");
        }

    }//SimpleIF

}//Namespace
