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
            : base(resetV, resetV, firingThresholdV, refractoryPeriods, stimuliCoeff)
        {
            _membraneTimeScale = membraneTimeScale;
            _membraneResistance = membraneResistance;
            return;
        }

        //Methods
        /// <summary>
        /// Exponential Integrate and Fire differential equation
        /// </summary>
        /// <param name="membraneV">Membrane voltage</param>
        protected override double MembraneVoltageDiffEq(double membraneV)
        {
            return (-(membraneV - _restV) + _membraneResistance * _stimuli) / _membraneTimeScale;

        }

    }//LeakyIF

}//Namespace
