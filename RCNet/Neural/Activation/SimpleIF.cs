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
        /// <summary>
        /// Spike value
        /// </summary>
        private const double Spike = 1d;

        //Attribute properties
        /// <summary>
        /// Normal range of the internal state
        /// </summary>
        public Interval InternalStateRange { get; }

        //Attributes
        private static readonly Interval _outputRange = new Interval(0, 1);
        private readonly double _resistance;
        private readonly double _decayRate;
        private readonly double _restV;
        private readonly double _resetV;
        private readonly double _firingThresholdV;
        private readonly int _refractoryPeriods;
        private double _membraneV;
        private bool _inRefractory;
        private int _refractoryPeriod;
        private double _initialMembranePotential;

        //Constructor
        /// <summary>
        /// Creates an initialized instance
        /// </summary>
        /// <param name="resistance">Membrane resistance (Mohm)</param>
        /// <param name="decayRate">Membrane potential decay rate</param>
        /// <param name="resetV">Membrane reset potential (mV)</param>
        /// <param name="firingThresholdV">Membrane firing threshold (mV)</param>
        /// <param name="refractoryPeriods">Number of after spike computation cycles while an input stimuli is ignored (ms)</param>
        public SimpleIF(double resistance,
                        double decayRate,
                        double resetV,
                        double firingThresholdV,
                        int refractoryPeriods
                        )
        {
            _resistance = resistance;
            _decayRate = decayRate;
            _restV = 0;
            _resetV = Math.Abs(resetV);
            _firingThresholdV = Math.Abs(firingThresholdV);
            _refractoryPeriods = refractoryPeriods;
            _initialMembranePotential = _restV;
            InternalStateRange = new Interval(_restV, _firingThresholdV);
            Reset();
            return;
        }

        //Properties
        /// <summary>
        /// Type of the activation function
        /// </summary>
        public ActivationType TypeOfActivation { get { return ActivationType.Spiking; } }

        /// <summary>
        /// Output range of the Compute method
        /// </summary>
        public Interval OutputRange { get { return _outputRange; } }

        /// <summary>
        /// Specifies whether the activation function supports derivative calculation
        /// </summary>
        public bool SupportsDerivative { get { return false; } }

        /// <summary>
        /// Specifies whether the activation function is independent on its previous states
        /// </summary>
        public bool Stateless { get { return false; } }

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
            _membraneV = _initialMembranePotential;
            _inRefractory = false;
            _refractoryPeriod = 0;
            return;
        }

        /// <summary>
        /// Sets initial state of the membrane potential
        /// </summary>
        /// <param name="state">0 LE state LT 1, where 0 means rest potential and 1 means firing threshold</param>
        public void SetInitialInternalState(double state)
        {
            _initialMembranePotential = InternalStateRange.Min + state * InternalStateRange.Span;
            Reset();
            return;
        }

        /// <summary>
        /// Updates state of the membrane according to an input stimuli and when the firing
        /// condition is met, it produces a spike
        /// </summary>
        /// <param name="x">Input stimuli (interpreted as an electric current)</param>
        public double Compute(double x)
        {
            x = (x).Bound();
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
                    //Ignore stimulation
                    x = 0;
                }
            }

            //Compute membrane new potential
            //Apply decay
            _membraneV = _restV + (_membraneV - _restV) * (1d - _decayRate);
            //Apply increment
            _membraneV += _resistance * x;
            //Output
            if (_membraneV >= _firingThresholdV)
            {
                _membraneV = _firingThresholdV;
                return Spike;
            }
            else
            {
                return 0d;
            }
        }

        /// <summary>
        /// Computes derivative (with respect to x)
        /// </summary>
        /// <param name="c">The result of the activation (Compute method)</param>
        /// <param name="x">Activation input (x argument of the Compute method)</param>
        public double ComputeDerivative(double c = double.NaN, double x = double.NaN)
        {
            throw new NotImplementedException("ComputeDerivative is unsupported method in case of spiking activation.");
        }

    }//SimpleIF

}//Namespace
