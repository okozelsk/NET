using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using RCNet.Extensions;
using RCNet.MathTools;
using RCNet.MathTools.Differential;
using RCNet.MathTools.VectorMath;

namespace RCNet.Neural.Activation
{
    /// <summary>
    /// Base class for spiking neuron models using ODE (Ordinary Differential Equation(s)) driving membrane state evolution.
    /// </summary>
    [Serializable]
    public abstract class ODESpikingMembrane : IActivationFunction
    {
        //Constants
        protected const double Spike = 1;
        protected const int VarMembraneV = 0;

        //Attributes
        protected static readonly Interval _outputRange = new Interval(0, 1);

        //Parameter attributes
        protected Interval _stateRange;
        protected double _restV;
        protected double _resetV;
        protected double _firingThresholdV;
        protected int _refractoryPeriods;
        protected double _stimuliCoeff;
        protected double _timeStep;
        protected int _subSteps;

        //Operation attributes
        protected Vector _evolVars;
        protected bool _inRefractory;
        protected int _refractoryPeriod;
        protected double _stimuli;

        /// <summary>
        /// Constructs an initialized instance
        /// </summary>
        /// <param name="restV">Membrane rest voltage</param>
        /// <param name="resetV">Membrane reset voltage</param>
        /// <param name="firingThresholdV">Firing threshold</param>
        /// <param name="refractoryPeriods">Refractory periods</param>
        /// <param name="stimuliCoeff">Input stimuli coefficient</param>
        /// <param name="timeStep">Time step of Compute method</param>
        /// <param name="subSteps">Computation sub-steps of timeStep</param>
        /// <param name="numOfEvolvingVars">Number of evolving variables</param>
        protected ODESpikingMembrane(double restV,
                                     double resetV,
                                     double firingThresholdV,
                                     double refractoryPeriods,
                                     double stimuliCoeff,
                                     double timeStep,
                                     int subSteps,
                                     int numOfEvolvingVars
                                     )
        {
            _restV = restV;
            _resetV = resetV;
            _firingThresholdV = firingThresholdV;
            _refractoryPeriods = (int)refractoryPeriods;
            _stimuliCoeff = stimuliCoeff;
            _stateRange = new Interval(_restV, _firingThresholdV);
            _evolVars = new Vector(numOfEvolvingVars);
            _timeStep = timeStep;
            _subSteps = subSteps;
            _evolVars[VarMembraneV] = _restV;
            _inRefractory = false;
            _refractoryPeriod = 0;
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
        public double InternalState { get { return _evolVars[VarMembraneV]; } }

        //Methods
        /// <summary>
        /// Resets function to its initial state
        /// </summary>
        public virtual void Reset()
        {
            _evolVars[VarMembraneV] = _restV;
            _inRefractory = false;
            _refractoryPeriod = 0;
            return;
        }

        /// <summary>
        /// Updates state of the membrane according to an input stimuli and when the firing
        /// condition is met, it produces a spike
        /// </summary>
        /// <param name="x">Input stimuli (interpreted as an electric current)</param>
        public virtual double Compute(double x)
        {
            _stimuli = (x * _stimuliCoeff).Bound();
            double output = 0;
            if (_evolVars[VarMembraneV] >= _firingThresholdV)
            {
                _evolVars[VarMembraneV] = _resetV;
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
                    _stimuli = 0;
                }
            }
            //Compute membrane new potential
            foreach(ODENumSolver.Estimation subResult in ODENumSolver.Solve(MembraneDiffEq, 0, _evolVars, _timeStep, _subSteps, ODENumSolver.Method.Euler))
            {
                _evolVars = subResult.V;
                if(_evolVars[VarMembraneV] >= _firingThresholdV)
                {
                    break;
                }
            }
            //Output
            if (_evolVars[VarMembraneV] >= _firingThresholdV)
            {
                OnFiring();
                output = Spike;
                _evolVars[VarMembraneV] = _firingThresholdV;
            }
            return output;
        }

        /// <summary>
        /// Ordinary differential equation(s) evolving membrane's variable(s)
        /// </summary>
        /// <param name="t">Time</param>
        /// <param name="v">Vector of membrane variables</param>
        /// <returns>dvdt</returns>
        protected abstract Vector MembraneDiffEq(double t, Vector v);

        /// <summary>
        /// Triggered when membrane is firing a spike
        /// </summary>
        protected virtual void OnFiring()
        {
            //Does nothing in base implementation
            return;
        }

        /// <summary>
        /// Unsupported functionality!!!
        /// Computes derivative of the activation input (does not change internal state)
        /// </summary>
        /// <param name="c">The result of the activation (Compute method)</param>
        /// <param name="x">Activation input (x argument of the Compute method)</param>
        public double ComputeDerivative(double c, double x)
        {
            throw new NotImplementedException("ComputeDerivative is unsupported method in case of spiking activation.");
        }

    }//ODESpikingMembrane

}//Namespace
