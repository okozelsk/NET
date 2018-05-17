using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using RCNet.Extensions;
using RCNet.MathTools.VectorMath;

namespace RCNet.Neural.Activation
{
    /// <summary>
    /// Leaky Integrate and Fire neuron model.
    /// </summary>
    public class LeakyIF : SpikingMembrane
    {
        //Attributes
        //Parameters
        private double _membraneTimeScale;
        private double _membraneResistance;

        /// <summary>
        /// Constructs an initialized instance
        /// </summary>
        /// <param name="membraneTimeScale">(ms)</param>
        /// <param name="membraneResistance">(MOhm)</param>
        /// <param name="restV">(mV)</param>
        /// <param name="resetV">(mV)</param>
        /// <param name="firingThresholdV">(mV)</param>
        /// <param name="refractoryPeriods">(ms)</param>
        /// <param name="stimuliCoeff"></param>
        public LeakyIF(double membraneTimeScale,
                       double membraneResistance,
                       double restV,
                       double resetV,
                       double firingThresholdV,
                       double refractoryPeriods,
                       double stimuliCoeff
                       )
            : base(restV, resetV, firingThresholdV, refractoryPeriods, stimuliCoeff, 1, 10, 1)
        {
            _membraneTimeScale = membraneTimeScale;
            _membraneResistance = membraneResistance;
            return;
        }

        //Methods
        protected override Vector MembraneDiffEq(double t, Vector v)
        {
            Vector dvdt = new Vector(1);
            dvdt[VarMembraneV] = (-(v[VarMembraneV] - _restV) + _membraneResistance * _stimuli) / _membraneTimeScale;
            return dvdt;
        }

        protected override void OnFiring()
        {
            //Does nothing
            return;
        }

    }//LeakyIF

}//Namespace
