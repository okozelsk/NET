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
    /// Implements simple form of Adaptive Integrate and Fire neuron model
    /// </summary>
    [Serializable]
    public class AdSimpleIF : IActivationFunction
    {
        //Constants
        private const double Spike = 1d;
        private const double StimuliIncreaseThreshold = 0.1d;
        private const double StimuliIncrease = 1.1d;
        private const double StimuliDecreaseThreshold = 0.5d;
        private const double StimuliDecrease = 0.5d;

        //Attributes
        //Static working ranges
        private static readonly Interval _inputRange = new Interval(double.NegativeInfinity.Bound(), double.PositiveInfinity.Bound());
        private static readonly Interval _outputRange = new Interval(0, 1);

        //Parameters
        private Interval _stateRange;
        private double _membraneResistance;
        private double _membraneDecayRate;
        private double _restV;
        private double _resetV;
        private double _firingThresholdV;
        private double _initialStimuliCoeff;
        private double _stimuliCoeff;

        //Operation
        private double _membraneV;

        /// <summary>
        /// Constructs an initialized instance
        /// </summary>
        /// <param name="membraneResistance">Membrane resisatance (Mohm).</param>
        /// <param name="membraneDecayRate">Membrane potential decay</param>
        /// <param name="resetV">Membrane reset potential (mV)</param>
        /// <param name="firingThresholdV">Membrane firing threshold (mV)</param>
        /// <param name="stimuliCoeff">Input stimuli coefficient (nA)</param>
        public AdSimpleIF(double membraneResistance,
                          double membraneDecayRate,
                          double resetV,
                          double firingThresholdV,
                          double initialStimuliCoeff
                          )
        {
            _membraneResistance = membraneResistance;
            _membraneDecayRate = membraneDecayRate;
            _restV = 0;
            _resetV = Math.Abs(resetV);
            _firingThresholdV = Math.Abs(firingThresholdV);
            _initialStimuliCoeff = initialStimuliCoeff;
            _stateRange = new Interval(_restV, _firingThresholdV);
            Reset();
            return;
        }

        //Properties
        /// <summary>
        /// Type of the output
        /// </summary>
        public ActivationFactory.FunctionOutputType OutputType { get { return ActivationFactory.FunctionOutputType.Spike; } }

        /// <summary>
        /// Accepted input signal range
        /// </summary>
        public Interval InputRange { get { return _inputRange; } }

        /// <summary>
        /// Output signal range
        /// </summary>
        public Interval OutputRange { get { return _outputRange; } }

        /// <summary>
        /// Specifies whether the activation function supports derivative
        /// </summary>
        public bool SupportsDerivative { get { return false; } }

        /// <summary>
        /// Specifies whether the activation function is depending on its previous states
        /// </summary>
        public bool TimeDependent { get { return true; } }

        /// <summary>
        /// Range of the internal state
        /// </summary>
        public Interval InternalStateRange { get { return _stateRange; } }

        /// <summary>
        /// Internal state
        /// </summary>
        public double InternalState { get { return _membraneV; } }

        //Methods
        /// <summary>
        /// Resets function to initial state
        /// </summary>
        public void Reset()
        {
            _stimuliCoeff = _initialStimuliCoeff;
            _membraneV = _restV;
            return;
        }

        /// <summary>
        /// Computes the result of the activation function
        /// </summary>
        /// <param name="x">Argument</param>
        public double Compute(double x)
        {
            x = (x * _stimuliCoeff).Bound();
            double spike = 0;
            if (_membraneV >= _firingThresholdV)
            {
                //Membrane potential after spike
                _membraneV = _resetV;
            }
            //Compute membrane new potential
            double inputVoltage = _membraneResistance * x;
            _membraneV = _restV + (_membraneV - _restV) * (1d - _membraneDecayRate) + inputVoltage;
            //Adaptation
            if (inputVoltage > 0)
            {
                if (inputVoltage >= (_firingThresholdV - _resetV) * StimuliDecreaseThreshold)
                {
                    _stimuliCoeff *= StimuliDecrease;
                }
                else if (inputVoltage <= (_firingThresholdV - _resetV) * StimuliIncreaseThreshold)
                {
                    _stimuliCoeff *= StimuliIncrease;
                }
            }
            //Output
            if (_membraneV >= _firingThresholdV)
            {
                spike = Spike;
                _membraneV = _firingThresholdV;
            }
            return spike;
        }

        /// <summary>
        /// Unsupported functionality
        /// </summary>
        /// <param name="c">The result of the Compute method</param>
        /// <param name="x">The argument of the Compute method</param>
        public double ComputeDerivative(double c = double.NaN, double x = double.NaN)
        {
            throw new NotImplementedException("ComputeDerivative is unsupported method in case of spiking activation.");
        }

    }//AdSimpleIF

}//Namespace
