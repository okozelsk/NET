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
        /// <summary>
        /// Spike value
        /// </summary>
        protected const double Spike = 1;
        /// <summary>
        /// Index of MembraneV evolving variable
        /// </summary>
        protected const int VarMembraneVIdx = 0;

        //Attributes
        /// <summary>
        /// Output range
        /// </summary>
        protected static readonly Interval _outputRange = new Interval(0, 1);

        //Parameters
        /// <summary>
        /// Normal range of the internal state
        /// </summary>
        protected Interval _stateRange;
        /// <summary>
        /// Membrane rest voltage
        /// </summary>
        protected double _restV;
        /// <summary>
        /// Membrane after reset voltage
        /// </summary>
        protected double _resetV;
        /// <summary>
        /// Membrane firing voltage
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

        //Operation attributes
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
        /// Coefficient for conversion of incoming stimuli current to expected physical unit 
        /// </summary>
        protected double _currentCoeff;
        /// <summary>
        /// Coefficient for conversion of membrane potential to expected physical unit 
        /// </summary>
        protected double _potentialCoeff;
        /// <summary>
        /// Adjusted (modified) input stimuli
        /// </summary>
        protected double _stimuli;

        /// <summary>
        /// Constructs an initialized instance
        /// </summary>
        /// <param name="restV">Membrane rest voltage</param>
        /// <param name="resetV">Membrane reset voltage</param>
        /// <param name="firingThresholdV">Firing threshold</param>
        /// <param name="refractoryPeriods">Refractory periods</param>
        /// <param name="solvingMethod">ODE numerical solver method</param>
        /// <param name="stepTimeScale">Computation step time scale</param>
        /// <param name="subSteps">Computation sub-steps</param>
        /// <param name="numOfEvolvingVars">Number of evolving variables</param>
        protected ODESpikingMembrane(double restV,
                                     double resetV,
                                     double firingThresholdV,
                                     int refractoryPeriods,
                                     ODENumSolver.Method solvingMethod,
                                     double stepTimeScale,
                                     int subSteps,
                                     int numOfEvolvingVars,
                                     double currentCoeff = 1d,
                                     double potentialCoeff = 1d
                                     )
        {
            _restV = restV;
            _resetV = resetV;
            _firingThresholdV = firingThresholdV;
            _refractoryPeriods = refractoryPeriods;
            _stateRange = new Interval(Math.Min(_resetV, _restV), _firingThresholdV);
            _evolVars = new Vector(numOfEvolvingVars);
            _solvingMethod = solvingMethod;
            _stepTimeScale = stepTimeScale;
            _subSteps = subSteps;
            _evolVars[VarMembraneVIdx] = _resetV;
            _inRefractory = false;
            _refractoryPeriod = 0;
            _currentCoeff = currentCoeff;
            _potentialCoeff = potentialCoeff;
            return;
        }

        //Properties
        /// <summary>
        /// Optimal strength of the stimulation
        /// </summary>
        public abstract double OptimalStimulationStrength { get; }

        /// <summary>
        /// Range of reasonable incoming current
        /// </summary>
        public abstract Interval StimuliRange { get; }

        /// <summary>
        /// Range of reasonable incoming current
        /// </summary>
        //public abstract Interval StimuliRange { get; }

        /// <summary>
        /// Type of the output
        /// </summary>
        public CommonEnums.NeuronSignalType OutputSignalType { get { return CommonEnums.NeuronSignalType.Spike; } }

        /// <summary>
        /// Output signal range
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
            _evolVars[VarMembraneVIdx] = _resetV;
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
            _stimuli = (x * _currentCoeff).Bound();
            double output = 0;
            if (_evolVars[VarMembraneVIdx] >= _firingThresholdV)
            {
                _evolVars[VarMembraneVIdx] = _resetV;
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
                    _stimuli = 0;
                }
            }
            
            //Compute membrane new potential
            foreach (ODENumSolver.Estimation subResult in ODENumSolver.Solve(MembraneDiffEq, 0, _evolVars, _stepTimeScale, _subSteps, _solvingMethod))
            {
                _evolVars = subResult.V;
                if (_evolVars[VarMembraneVIdx] >= _firingThresholdV)
                {
                    break;
                }
            }
            //Output
            if (_evolVars[VarMembraneVIdx] >= _firingThresholdV)
            {
                OnFiring();
                output = Spike;
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
        protected abstract void OnFiring();

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
