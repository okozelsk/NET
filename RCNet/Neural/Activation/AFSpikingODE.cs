using System;
using RCNet.MathTools.Differential;
using RCNet.MathTools.VectorMath;

namespace RCNet.Neural.Activation
{
    /// <summary>
    /// Ancestor for activation functions using spiking neuron models with ODE (Ordinary Differential Equation(s)).
    /// </summary>
    [Serializable]
    public abstract class AFSpikingODE : AFSpikingBase
    {
        //Attributes
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
        /// <param name="initialVRatio">Initial membrane potential in form of the ratio between 0 and 1 where 0 corresponds to a Min(resetV, restV) potential and 1 corresponds to a firingThreshold.</param>
        protected AFSpikingODE(double restV,
                               double resetV,
                               double firingThresholdV,
                               int refractoryPeriods,
                               ODENumSolver.Method solvingMethod,
                               double stepTimeScale,
                               int subSteps,
                               int numOfEvolvingVars,
                               double inputCurrentCoeff = 1d,
                               double membranePotentialCoeff = 1d,
                               double initialVRatio = 0d
                               )
            : base(restV, resetV, firingThresholdV, refractoryPeriods, numOfEvolvingVars, inputCurrentCoeff, membranePotentialCoeff, initialVRatio)
        {
            _solvingMethod = solvingMethod;
            _stepTimeScale = stepTimeScale;
            _subSteps = subSteps;
            return;
        }


        //Methods
        /// <inheritdoc/>
        public override void Reset()
        {
            base.Reset();
            return;
        }

        /// <summary>
        /// Ordinary differential equation(s) evolving membrane potential
        /// </summary>
        /// <param name="t">Time</param>
        /// <param name="v">Vector of membrane variables</param>
        /// <returns>dvdt</returns>
        protected abstract Vector MembraneDiffEq(double t, Vector v);

        /// <inheritdoc/>
        public override bool ComputeEvolVars()
        {
            foreach (ODENumSolver.Estimation subResult in ODENumSolver.SolveGradually(MembraneDiffEq, 0, _evolVars, _stepTimeScale, _subSteps, _solvingMethod))
            {
                _evolVars = subResult.V;
                if (_evolVars[VarMembraneVIdx] >= _firingThresholdV)
                {
                    //Keep maximum voltage
                    _evolVars[VarMembraneVIdx] = _firingThresholdV;
                    return true;
                }
                else if(_evolVars[VarMembraneVIdx] < _minV)
                {
                    //Keep minimum voltage
                    _evolVars[VarMembraneVIdx] = _minV;
                }
            }
            return false;
        }

    }//AFSpikingODE

}//Namespace
