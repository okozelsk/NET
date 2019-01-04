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
    /// Implements Exponential Integrate and Fire neuron model.
    /// For more information visit http://neuronaldynamics.epfl.ch/online/Ch5.S2.html
    /// </summary>
    [Serializable]
    public class ExpIF : ODESpikingMembrane
    {
        //Constants
        //Typical values
        /// <summary>
        /// Typical value of time scale
        /// </summary>
        public const double TypicalTimeScale = 12;
        /// <summary>
        /// Typical value of resistance
        /// </summary>
        public const double TypicalResistance = 20;
        /// <summary>
        /// Typical value of resting voltage
        /// </summary>
        public const double TypicalRestV = -65;
        /// <summary>
        /// Typical value of reset voltage
        /// </summary>
        public const double TypicalResetV = -60;
        /// <summary>
        /// Typical value of rheobase
        /// </summary>
        public const double TypicalRheobaseV = -55;
        /// <summary>
        /// Typical value of firing voltage
        /// </summary>
        public const double TypicalFiringThresholdV = -30;
        /// <summary>
        /// Typical value of sharpness delta
        /// </summary>
        public const double TypicalSharpnessDeltaT = 2;

        //Attributes
        //Parameters
        private readonly double _timeScale;
        private readonly double _resistance;
        private readonly double _rheobaseV;
        private readonly double _sharpnessDeltaT;

        //Constructor
        /// <summary>
        /// Creates an initialized instance
        /// </summary>
        /// <param name="stimuliCoeff">Input stimuli coefficient (pA)</param>
        /// <param name="timeScale">Membrane time scale (ms)</param>
        /// <param name="resistance">Membrane resistance (Mohm)</param>
        /// <param name="restV">Membrane rest potential (mV)</param>
        /// <param name="resetV">Membrane reset potential (mV)</param>
        /// <param name="rheobaseV">Membrane rheobase threshold (mV)</param>
        /// <param name="firingThresholdV">Membrane firing threshold (mV)</param>
        /// <param name="sharpnessDeltaT">Sharpness of membrane potential change (mV)</param>
        /// <param name="refractoryPeriods">Number of after spike computation cycles while an input stimuli is ignored (ms)</param>
        /// <param name="solverMethod">ODE numerical solver method</param>
        /// <param name="solverCompSteps">ODE numerical solver computation steps of the time step</param>
        public ExpIF(double stimuliCoeff,
                     double timeScale,
                     double resistance,
                     double restV,
                     double resetV,
                     double rheobaseV,
                     double firingThresholdV,
                     double sharpnessDeltaT,
                     int refractoryPeriods,
                     ODENumSolver.Method solverMethod,
                     int solverCompSteps
                     )
            : base(restV,
                   resetV,
                   firingThresholdV,
                   refractoryPeriods,
                   stimuliCoeff,
                   solverMethod,
                   1,
                   solverCompSteps,
                   1
                   )
        {
            _timeScale = timeScale;
            _resistance = resistance;
            _rheobaseV = rheobaseV;
            _sharpnessDeltaT = sharpnessDeltaT;
            return;
        }

        //Methods
        /// <summary>
        /// Triggered when membrane is firing a spike
        /// </summary>
        protected override void OnFiring()
        {
            //Does nothing
            return;
        }

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
            double exponent = Math.Min((v[VarMembraneVIdx] - _rheobaseV) / _sharpnessDeltaT, 20);
            dvdt[VarMembraneVIdx] = (- (v[VarMembraneVIdx] - _restV)
                                  + _sharpnessDeltaT * Math.Exp(exponent)
                                  + _resistance * _stimuli
                                  ) / _timeScale;
            return dvdt;
        }

    }//ExpIF

}//Namespace
