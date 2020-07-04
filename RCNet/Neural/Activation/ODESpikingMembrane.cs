using RCNet.Extensions;
using RCNet.MathTools;
using RCNet.MathTools.Differential;
using RCNet.MathTools.VectorMath;
using System;

namespace RCNet.Neural.Activation
{
    /// <summary>
    /// Base class for spiking neuron models using ODE (Ordinary Differential Equation(s)) driving membrane state evolution.
    /// </summary>
    [Serializable]
    public abstract class ODESpikingMembrane : IActivationFunction
    {
        //Constants
        /// <summary>
        /// Spike value
        /// </summary>
        protected const double Spike = 1d;
        /// <summary>
        /// Index of MembraneV evolving variable
        /// </summary>
        protected const int VarMembraneVIdx = 0;

        //Attributes
        /// <summary>
        /// Output range (0 no spike, 1 spike)
        /// </summary>
        protected static readonly Interval _outputRange = new Interval(0, 1);

        //Parameters
        /// <summary>
        /// Normal range of the internal state
        /// </summary>
        protected Interval _stateRange;
        /// <summary>
        /// Membrane rest potential
        /// </summary>
        protected double _restV;
        /// <summary>
        /// Membrane after reset potential
        /// </summary>
        protected double _resetV;
        /// <summary>
        /// Membrane firing potential
        /// </summary>
        protected double _firingThresholdV;
        /// <summary>
        /// Refractory periods after firing
        /// </summary>
        protected int _refractoryPeriods;
        /// <summary>
        /// ODE numerical solver method
        /// </summary>
        protected ODENumSolver.Method _solvingMethod;
        /// <summary>
        /// Computation step time scale
        /// </summary>
        protected double _stepTimeScale;
        /// <summary>
        /// Time sub-steps within the computation step
        /// </summary>
        protected int _subSteps;
        /// <summary>
        /// Coefficient for conversion of incoming stimuli current to expected physical unit 
        /// </summary>
        protected double _currentCoeff;
        /// <summary>
        /// Coefficient for conversion of membrane potential to expected physical unit 
        /// </summary>
        protected double _potentialCoeff;

        //Operation attributes
        /// <summary>
        /// Membrane initial potential
        /// </summary>
        protected double _initialV;
        /// <summary>
        /// Evolving variables
        /// </summary>
        protected Vector _evolVars;
        /// <summary>
        /// Specifies whether the membrane is in refractory mode
        /// </summary>
        protected bool _inRefractory;
        /// <summary>
        /// Specifies current refractory period of the membrane
        /// </summary>
        protected int _refractoryPeriod;
        /// <summary>
        /// Adjusted (modified) input stimuli
        /// </summary>
        protected double _stimuli;
        /// <summary>
        /// Indicates spike during last computation
        /// </summary>
        protected bool _lastSpike;
        /// <summary>
        /// Minimum membrane voltage
        /// </summary>
        protected double _minV;

        /// <summary>
        /// Constructs an initialized instance
        /// </summary>
        /// <param name="restV">Membrane rest potential</param>
        /// <param name="resetV">Membrane reset potential</param>
        /// <param name="firingThresholdV">Firing threshold</param>
        /// <param name="refractoryPeriods">Refractory periods</param>
        /// <param name="solvingMethod">ODE numerical solver method</param>
        /// <param name="stepTimeScale">Computation step time scale</param>
        /// <param name="subSteps">Computation sub-steps</param>
        /// <param name="numOfEvolvingVars">Number of evolving variables</param>
        /// <param name="inputCurrentCoeff">Coefficient of the current</param>
        /// <param name="membranePotentialCoeff">Coefficient of the membrane potential</param>
        protected ODESpikingMembrane(double restV,
                                     double resetV,
                                     double firingThresholdV,
                                     int refractoryPeriods,
                                     ODENumSolver.Method solvingMethod,
                                     double stepTimeScale,
                                     int subSteps,
                                     int numOfEvolvingVars,
                                     double inputCurrentCoeff = 1d,
                                     double membranePotentialCoeff = 1d
                                     )
        {
            _restV = restV;
            _resetV = resetV;
            _minV = Math.Min(_resetV, _restV);
            _initialV = _resetV;
            _firingThresholdV = firingThresholdV;
            _refractoryPeriods = refractoryPeriods;
            _evolVars = new Vector(numOfEvolvingVars);
            _solvingMethod = solvingMethod;
            _stepTimeScale = stepTimeScale;
            _subSteps = subSteps;
            _evolVars[VarMembraneVIdx] = _initialV;
            _inRefractory = false;
            _refractoryPeriod = 0;
            _lastSpike = false;
            _currentCoeff = inputCurrentCoeff;
            _potentialCoeff = membranePotentialCoeff;
            _stateRange = new Interval(_potentialCoeff * Math.Min(_resetV, _restV), _potentialCoeff * _firingThresholdV);
            return;
        }

        //Properties
        /// <summary>
        /// Type of the activation
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
        /// Normal range of the internal state
        /// </summary>
        public Interval InternalStateRange { get { return _stateRange; } }

        /// <summary>
        /// Internal state
        /// </summary>
        public double InternalState { get { return _potentialCoeff * _evolVars[VarMembraneVIdx]; } }

        //Methods
        /// <summary>
        /// Resets function to its initial state
        /// </summary>
        public virtual void Reset()
        {
            _evolVars[VarMembraneVIdx] = _initialV;
            _inRefractory = false;
            _refractoryPeriod = 0;
            _lastSpike = false;
            return;
        }


        /// <summary>
        /// Sets initial state of the membrane potential
        /// </summary>
        /// <param name="state">0 LE state LT 1, where 0 correspondes to Min(reset, rest) potential and 1 correspondes to firing threshold</param>
        public void SetInitialInternalState(double state)
        {
            _initialV = _minV + state * (_firingThresholdV - _minV);
            Reset();
            return;
        }

        /// <summary>
        /// Updates state of the membrane according to an input stimuli and when the firing
        /// condition is met, it produces a spike
        /// </summary>
        /// <param name="x">Input stimuli (interpreted as an electric current)</param>
        public virtual double Compute(double x)
        {
            //Stimuli
            _stimuli = (x * _currentCoeff).Bound();
            //Membrane reset?
            if(_lastSpike)
            {
                _evolVars[VarMembraneVIdx] = _resetV;
                //Refractory
                //Enter refractory?
                if (_refractoryPeriods > 0)
                {
                    _refractoryPeriod = 0;
                    _inRefractory = true;
                }
            }
            //Exit refractory?
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
                    //Ignore stimuli
                    _stimuli = 0d;
                }
            }

            //Compute membrane new potential
            bool spike = false;
            foreach (ODENumSolver.Estimation subResult in ODENumSolver.SolveGradually(MembraneDiffEq, 0, _evolVars, _stepTimeScale, _subSteps, _solvingMethod))
            {
                _evolVars = subResult.V;
                if (_evolVars[VarMembraneVIdx] >= _firingThresholdV)
                {
                    //Keep maximum voltage
                    _evolVars[VarMembraneVIdx] = _firingThresholdV;
                    spike = true;
                    break;
                }
                else if(_evolVars[VarMembraneVIdx] < _minV)
                {
                    //Keep minimum voltage
                    _evolVars[VarMembraneVIdx] = _minV;
                }
            }


            //Firing
            if (spike)
            {
                OnFiring();
                //Spike
                _lastSpike = true;
                return Spike;
            }
            else
            {
                //No spike
                _lastSpike = false;
                return 0d;
            }
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
        protected abstract void OnFiring();

        /// <summary>
        /// Computes derivative (with respect to x)
        /// </summary>
        /// <param name="c">The result of the activation (Compute method)</param>
        /// <param name="x">Activation input (x argument of the Compute method)</param>
        public double ComputeDerivative(double c, double x)
        {
            throw new NotImplementedException("ComputeDerivative is unsupported method in case of spiking activation.");
        }

    }//ODESpikingMembrane

}//Namespace
