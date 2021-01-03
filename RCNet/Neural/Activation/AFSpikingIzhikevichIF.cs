using RCNet.Extensions;
using RCNet.MathTools.Differential;
using RCNet.MathTools.VectorMath;
using System;

namespace RCNet.Neural.Activation
{
    /// <summary>
    /// Implements the Izhikevich Integrate and Fire neuron model.
    /// </summary>
    /// <remarks>
    /// For more information visit the https://www.izhikevich.org/publications/spikes.pdf site.
    /// </remarks>
    [Serializable]
    public class AFSpikingIzhikevichIF : AFSpikingODE
    {
        //Constants
        /// <summary>
        /// An index of recovery evolving variable.
        /// </summary>
        protected const int VarRecovery = 1;

        //Attributes
        private readonly double _recoveryTimeScale;
        private readonly double _recoverySensitivity;
        private readonly double _recoveryReset;

        //Constructor
        /// <summary>
        /// Creates an initialized instance.
        /// </summary>
        /// <param name="recoveryTimeScale">The time scale of the recovery variable.</param>
        /// <param name="recoverySensitivity">The sensitivity of the recovery variable to the sub-threshold fluctuations of the membrane potential.</param>
        /// <param name="recoveryReset">The after-spike reset of the recovery variable.</param>
        /// <param name="restV">The membrane rest potential (mV).</param>
        /// <param name="resetV">The membrane reset potential (mV).</param>
        /// <param name="firingThresholdV">The membrane firing threshold (mV).</param>
        /// <param name="refractoryPeriods">The number of after-spike computation cycles while an input stimuli to be ignored (cycles).</param>
        /// <param name="solverMethod">The ODE numerical solver method to be used.</param>
        /// <param name="solverCompSteps">The number of computation sub-steps of the ODE numerical solver.</param>
        /// <param name="stimuliDuration">The duration of the membrane stimulation (ms).</param>
        /// <param name="initialVRatio">The membrane initial potential in form of a ratio between 0 and 1, where 0 corresponds to a Min(resetV, restV) potential and 1 corresponds to a firingThreshold.</param>
        public AFSpikingIzhikevichIF(double recoveryTimeScale,
                                     double recoverySensitivity,
                                     double recoveryReset,
                                     double restV,
                                     double resetV,
                                     double firingThresholdV,
                                     int refractoryPeriods,
                                     ODENumSolver.Method solverMethod,
                                     int solverCompSteps,
                                     double stimuliDuration,
                                     double initialVRatio = 0d
                                     )
            : base(restV,
                   resetV,
                   firingThresholdV,
                   refractoryPeriods,
                   solverMethod,
                   stimuliDuration,
                   solverCompSteps,
                   2,
                   100,
                   1,
                   initialVRatio
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
        /// <inheritdoc/>
        public override void Reset()
        {
            base.Reset();
            _evolVars[VarRecovery] = (_recoverySensitivity * _evolVars[VarMembraneVIdx]);
            return;
        }

        /// <inheritdoc/>
        protected override Vector MembraneDiffEq(double t, Vector v)
        {
            Vector dvdt = new Vector(2);
            dvdt[VarMembraneVIdx] = 0.04 * v[VarMembraneVIdx].Power(2) + 5 * v[VarMembraneVIdx] + 140 - v[VarRecovery] + _stimuli;
            dvdt[VarRecovery] = _recoveryTimeScale * (_recoverySensitivity * v[VarMembraneVIdx] - v[VarRecovery]);
            return dvdt;
        }

        /// <inheritdoc/>
        protected override void OnFiring()
        {
            _evolVars[VarRecovery] += _recoveryReset;
            return;
        }

    }//AFSpikingIzhikevichIF

}//Namespace
