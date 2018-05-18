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
        private Interval _stateRange;
        private double _membraneResistance;
        private double _membraneDecayRate;
        private double _restV;
        private double _resetV;
        private double _firingThresholdV;
        private int _refractoryPeriods;
        private double _stimuliCoeff;
        private double _membraneV;
        private bool _inRefractory;
        private int _refractoryPeriod;

        /// <summary>
        /// Constructs an initialized instance
        /// </summary>
        /// <param name="membraneResistance">Membrane resisatance (Mohm).</param>
        /// <param name="membraneDecayRate">Membrane potential decay</param>
        /// <param name="resetV">Membrane reset potential (mV)</param>
        /// <param name="firingThresholdV">Membrane firing threshold (mV)</param>
        /// <param name="refractoryPeriods">Number of after spike computation cycles while an input stimuli is ignored (ms).</param>
        /// <param name="stimuliCoeff">Input stimuli coefficient (nA)</param>
        public SimpleIF(double membraneResistance,
                        double membraneDecayRate,
                        double resetV,
                        double firingThresholdV,
                        double refractoryPeriods,
                        double stimuliCoeff
                        )
        {
            _membraneResistance = membraneResistance;
            _membraneDecayRate = membraneDecayRate;
            _restV = 0;
            _resetV = Math.Abs(resetV);
            _firingThresholdV = Math.Abs(firingThresholdV);
            _refractoryPeriods = (int)Math.Round(refractoryPeriods, 0);
            _stimuliCoeff = stimuliCoeff;
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
        public bool Stateless { get { return true; } }

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
            _membraneV = _restV + (_membraneV - _restV) * (1d - _membraneDecayRate) + _membraneResistance * x;
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
