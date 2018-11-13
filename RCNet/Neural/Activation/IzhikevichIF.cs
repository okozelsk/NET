using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using RCNet.Extensions;
using RCNet.MathTools.Differential;
using RCNet.MathTools.VectorMath;

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

        /// <summary>
        /// Constructs an initialized instance
        /// </summary>
        /// <param name="settings">Encapsulated arguments</param>
        /// <param name="rand">Random object to be used for randomly generated parameters</param>
        public IzhikevichIF(IzhikevichIFSettings settings, Random rand)
            : base(rand.NextDouble(settings.RestV),
                   rand.NextDouble(settings.ResetV),
                   rand.NextDouble(settings.FiringThresholdV),
                   settings.RefractoryPeriods,
                   settings.StimuliCoeff,
                   settings.SolverMethod,
                   1,
                   settings.SolverCompSteps,
                   2
                  )
        {
            _recoveryTimeScale = rand.NextDouble(settings.RecoveryTimeScale);
            _recoverySensitivity = rand.NextDouble(settings.RecoverySensitivity);
            _recoveryReset = rand.NextDouble(settings.RecoveryReset);
            _evolVars[VarRecovery] = (_recoverySensitivity * _evolVars[VarMembraneVIdx]);
            return;
        }

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
            base.OnFiring();
            _evolVars[VarRecovery] += _recoveryReset;
            return;
        }

    }//IzhikevichIF

}//Namespace
