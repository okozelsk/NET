using RCNet.MathTools.Differential;
using RCNet.MathTools.VectorMath;
using System;

namespace RCNet.Neural.Activation
{
    /// <summary>
    /// Implements a common ancestor of activation functions that use spiking neuron models with ODE (Ordinary Differential Equation(s)).
    /// </summary>
    [Serializable]
    public abstract class AFSpikingODE : AFSpikingBase
    {
        //Attributes
        /// <summary>
        /// The ODE numerical solver method.
        /// </summary>
        protected ODENumSolver.Method _solvingMethod;
        /// <summary>
        /// The ODE computation step time scale.
        /// </summary>
        protected double _stepTimeScale;
        /// <summary>
        /// The number of computation sub-steps of the ODE numerical solver.
        /// </summary>
        protected int _subSteps;

        /// <summary>
        /// Creates an initialized instance.
        /// </summary>
        /// <param name="restV">The membrane rest potential.</param>
        /// <param name="resetV">The membrane reset potential.</param>
        /// <param name="firingThresholdV">The firing threshold.</param>
        /// <param name="refractoryPeriods">The number of refractory periods.</param>
        /// <param name="solvingMethod">The ODE numerical solver method to be used.</param>
        /// <param name="stepTimeScale">The ODE computation step time scale.</param>
        /// <param name="subSteps">The number of computation sub-steps of the ODE numerical solver.</param>
        /// <param name="numOfEvolvingVars">The number of inner evolving variables.</param>
        /// <param name="inputCurrentCoeff">The coefficient of the input current.</param>
        /// <param name="membranePotentialCoeff">The coefficient of the membrane potential.</param>
        /// <param name="initialVRatio">The membrane initial potential in form of a ratio between 0 and 1, where 0 corresponds to a Min(resetV, restV) potential and 1 corresponds to a firingThreshold.</param>
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
        /// Computes the Ordinary Differential Equation(s) evolving the inner variables.
        /// </summary>
        /// <param name="t">The time.</param>
        /// <param name="v">The vector of inner evolving variables.</param>
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
                else if (_evolVars[VarMembraneVIdx] < _minV)
                {
                    //Keep minimum voltage
                    _evolVars[VarMembraneVIdx] = _minV;
                }
            }
            return false;
        }

    }//AFSpikingODE

}//Namespace
