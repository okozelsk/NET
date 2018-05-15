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
    /// Exponential Integrate and Fire neuron model.
    /// </summary>
    [Serializable]
    public class ExpIF : SpikingMembrane
    {
        //Attributes
        //Parameter attributes
        private double _membraneTimeScale;
        private double _membraneResistance;
        private double _rheobaseThresholdV;
        private double _sharpnessDeltaT;

        /// <summary>
        /// Constructs an initialized instance
        /// </summary>
        /// <param name="membraneTimeScale">(ms)</param>
        /// <param name="membraneResistance">(MOhm)</param>
        /// <param name="restV">(mV)</param>
        /// <param name="resetV">(mV)</param>
        /// <param name="rheobaseThresholdV">(mV)</param>
        /// <param name="firingThresholdV">(mV)</param>
        /// <param name="sharpnessDeltaT">(mV)</param>
        /// <param name="refractoryPeriods">(ms)</param>
        /// <param name="stimuliCoeff"></param>
        public ExpIF(double membraneTimeScale,
                     double membraneResistance,
                     double restV,
                     double resetV,
                     double rheobaseThresholdV,
                     double firingThresholdV,
                     double sharpnessDeltaT,
                     double refractoryPeriods,
                     double stimuliCoeff
                     )
            :base(restV, resetV, firingThresholdV, refractoryPeriods, stimuliCoeff)
        {
            _membraneTimeScale = membraneTimeScale;
            _membraneResistance = membraneResistance;
            _rheobaseThresholdV = rheobaseThresholdV;
            _sharpnessDeltaT = sharpnessDeltaT;
            return;
        }

        //Methods
        /// <summary>
        /// Exponential Integrate and Fire differential equation
        /// </summary>
        /// <param name="membraneV">Membrane voltage</param>
        protected override double MembraneVoltageDiffEq(double membraneV)
        {
            //Ensure numerical stability
            double exponent = Math.Min((membraneV - _rheobaseThresholdV) / _sharpnessDeltaT, 20);
            return (-(membraneV - _restV) + _sharpnessDeltaT * Math.Exp(exponent) + _membraneResistance * _stimuli) / _membraneTimeScale;
        }


    }//ExpIF

}//Namespace
