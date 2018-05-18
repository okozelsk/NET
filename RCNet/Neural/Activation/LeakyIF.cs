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
    /// Implements Leaky Integrate and Fire neuron model.
    /// For more information visit http://neuronaldynamics.epfl.ch/online/Ch1.S3.html
    /// </summary>
    [Serializable]
    public class LeakyIF : ODESpikingMembrane
    {
        //Attributes
        //Parameters
        private double _membraneTimeScale;
        private double _membraneResistance;

        /// <summary>
        /// Constructs an initialized instance
        /// </summary>
        /// <param name="membraneTimeScale">Membrane time scale (ms)</param>
        /// <param name="membraneResistance">Membrane resistance (Mohm)</param>
        /// <param name="restV">Membrane rest potential (mV)</param>
        /// <param name="resetV">Membrane reset potential (mV)</param>
        /// <param name="firingThresholdV">Membrane firing threshold (mV)</param>
        /// <param name="refractoryPeriods">Number of after spike computation cycles while an input stimuli is ignored.</param>
        /// <param name="stimuliCoeff">Input stimuli coefficient (nA)</param>
        public LeakyIF(double membraneTimeScale,
                       double membraneResistance,
                       double restV,
                       double resetV,
                       double firingThresholdV,
                       double refractoryPeriods,
                       double stimuliCoeff
                       )
            : base(restV, resetV, firingThresholdV, refractoryPeriods, stimuliCoeff, 1, 2, 1)
        {
            _membraneTimeScale = membraneTimeScale;
            _membraneResistance = membraneResistance;
            return;
        }

        //Methods
        /// <summary>
        /// LeakyIF autonomous ordinary differential equation.
        /// </summary>
        /// <param name="t">Time. Not used in autonomous ODE.</param>
        /// <param name="v">Membrane potential</param>
        /// <returns>dvdt</returns>
        protected override Vector MembraneDiffEq(double t, Vector v)
        {
            Vector dvdt = new Vector(1);
            dvdt[VarMembraneV] = (-(v[VarMembraneV] - _restV) + _membraneResistance * _stimuli) / _membraneTimeScale;
            return dvdt;
        }

    }//LeakyIF

}//Namespace
