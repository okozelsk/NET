using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using RCNet.Extensions;
using RCNet.RandomValue;
using RCNet.MathTools.VectorMath;
using RCNet.MathTools.Differential;
using RCNet.MathTools;

namespace RCNet.Neural.Activation
{
    /// <summary>
    /// Implements Izhikevich Integrate and Fire neuron model.
    /// For more information visit https://www.izhikevich.org/publications/spikes.pdf
    /// </summary>
    [Serializable]
    public class IzhikevichIF : ODESpikingMembrane
    {
        //Constants
        /// <summary>
        /// Index of recovery evolving variable
        /// </summary>
        protected const int VarRecovery = 1;

        //Attributes
        //Parameters
        private readonly double _recoveryTimeScale;
        private readonly double _recoverySensitivity;
        private readonly double _recoveryReset;


        //Constructor
        /// <summary>
        /// Creates an initialized instance
        /// </summary>
        /// <param name="recoveryTimeScale">Time scale of the recovery variable</param>
        /// <param name="recoverySensitivity">Sensitivity of the recovery variable to the subthreshold fluctuations of the membrane potential</param>
        /// <param name="recoveryReset">After-spike reset of the recovery variable</param>
        /// <param name="restV">Membrane rest potential (mV)</param>
        /// <param name="resetV">Membrane reset potential (mV)</param>
        /// <param name="firingThresholdV">Membrane firing threshold (mV)</param>
        /// <param name="refractoryPeriods">Number of after spike computation cycles while an input stimuli is ignored (ms)</param>
        /// <param name="solverMethod">ODE numerical solver method</param>
        /// <param name="solverCompSteps">ODE numerical solver computation steps of the time step</param>
        /// <param name="solveGradually">Determines what variant of ODENumSolver method to use. Gradual solving can lead to shorter computation time (computation stops immediately the firing threshold is met) but it impacts membrane current potential prediction power.</param>
        public IzhikevichIF(double recoveryTimeScale,
                            double recoverySensitivity,
                            double recoveryReset,
                            double restV,
                            double resetV,
                            double firingThresholdV,
                            int refractoryPeriods,
                            ODENumSolver.Method solverMethod,
                            int solverCompSteps,
                            bool solveGradually
                            )
            : base(restV,
                   resetV,
                   firingThresholdV,
                   refractoryPeriods,
                   solverMethod,
                   1,
                   solverCompSteps,
                   solveGradually,
                   2
                  )
        {
            _recoveryTimeScale = recoveryTimeScale;
            _recoverySensitivity = recoverySensitivity;
            _recoveryReset = recoveryReset;
            _evolVars[VarRecovery] = (_recoverySensitivity * _evolVars[VarMembraneVIdx]);
            return;
        }

        //Properties
        //Methods
        /// <summary>
        /// Resets function to its initial state
        /// </summary>
        public override void Reset()
        {
            base.Reset();
            _evolVars[VarRecovery] = (_recoverySensitivity * _evolVars[VarMembraneVIdx]);
            return;
        }

        /// <summary>
        /// IzhikevichIF couple of the ordinary differential equations.
        /// </summary>
        /// <param name="t">Time. Not used in autonomous ODE.</param>
        /// <param name="v">Membrane potential and recovery variable</param>
        /// <returns>dvdt</returns>
        protected override Vector MembraneDiffEq(double t, Vector v)
        {
            Vector dvdt = new Vector(2);
            dvdt[VarMembraneVIdx] = 0.04 * v[VarMembraneVIdx].Power(2) + 5 * v[VarMembraneVIdx] + 140 - v[VarRecovery] + _stimuli;
            dvdt[VarRecovery] = _recoveryTimeScale * (_recoverySensitivity * v[VarMembraneVIdx] - v[VarRecovery]);
            return dvdt;
        }

        /// <summary>
        /// Adds reset of the recovery variable on firing.
        /// </summary>
        protected override void OnFiring()
        {
            _evolVars[VarRecovery] += _recoveryReset;
            return;
        }

    }//IzhikevichIF

}//Namespace
