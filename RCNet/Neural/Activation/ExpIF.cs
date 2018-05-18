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
    /// Implements Exponential Integrate and Fire neuron model.
    /// For more information visit http://neuronaldynamics.epfl.ch/online/Ch5.S2.html
    /// </summary>
    [Serializable]
    public class ExpIF : ODESpikingMembrane
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
        /// <param name="membraneTimeScale">Membrane time scale (ms)</param>
        /// <param name="membraneResistance">Membrane resistance (Mohm)</param>
        /// <param name="restV">Membrane rest potential (mV)</param>
        /// <param name="resetV">Membrane reset potential (mV)</param>
        /// <param name="rheobaseThresholdV">Membrane rheobase threshold (mV)</param>
        /// <param name="firingThresholdV">Membrane firing threshold (mV)</param>
        /// <param name="sharpnessDeltaT">Sharpness of membrane potential change (mV)</param>
        /// <param name="refractoryPeriods">Number of after spike computation cycles while an input stimuli is ignored (ms).</param>
        /// <param name="stimuliCoeff">Input stimuli coefficient (nA)</param>
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
            :base(restV, resetV, firingThresholdV, refractoryPeriods, stimuliCoeff, 1, 2, 1)
        {
            _membraneTimeScale = membraneTimeScale;
            _membraneResistance = membraneResistance;
            _rheobaseThresholdV = rheobaseThresholdV;
            _sharpnessDeltaT = sharpnessDeltaT;
            return;
        }

        //Methods
        /// <summary>
        /// ExpIF autonomous ordinary differential equation.
        /// </summary>
        /// <param name="t">Time. Not used in autonomous ODE.</param>
        /// <param name="v">Membrane potential</param>
        /// <returns>dvdt</returns>
        protected override Vector MembraneDiffEq(double t, Vector v)
        {
            Vector dvdt = new Vector(1);
            //Ensure numerical stability
            double exponent = Math.Min((v[VarMembraneV] - _rheobaseThresholdV) / _sharpnessDeltaT, 20);
            dvdt[VarMembraneV] = (- (v[VarMembraneV] - _restV)
                                  + _sharpnessDeltaT * Math.Exp(exponent)
                                  + _membraneResistance * _stimuli
                                  ) / _membraneTimeScale;
            return dvdt;
        }

    }//ExpIF

}//Namespace
