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
    /// Implements Leaky Integrate and Fire neuron model.
    /// For more information visit http://neuronaldynamics.epfl.ch/online/Ch1.S3.html
    /// </summary>
    [Serializable]
    public class LeakyIF : ODESpikingMembrane
    {
        //Constants
        //Typical values
        /// <summary>
        /// Typical value of time scale
        /// </summary>
        public const double TypicalTimeScale = 8;
        /// <summary>
        /// Typical value of resistance
        /// </summary>
        public const double TypicalResistance = 10;
        /// <summary>
        /// Typical value of resting voltage
        /// </summary>
        public const double TypicalRestV = -70;
        /// <summary>
        /// Typical value of reset voltage
        /// </summary>
        public const double TypicalResetV = -65;
        /// <summary>
        /// Typical value of firing voltage
        /// </summary>
        public const double TypicalFiringThresholdV = -50;

        //Attributes
        //Parameters
        private readonly double _timeScale;
        private readonly double _resistance;

        //Constructor
        /// <summary>
        /// Creates an initialized instance
        /// </summary>
        /// <param name="stimuliCoeff">Input stimuli coefficient (pA)</param>
        /// <param name="timeScale">Membrane time scale (ms)</param>
        /// <param name="resistance">Membrane resistance (Mohm)</param>
        /// <param name="restV">Membrane rest potential (mV)</param>
        /// <param name="resetV">Membrane reset potential (mV)</param>
        /// <param name="firingThresholdV">Membrane firing threshold (mV)</param>
        /// <param name="refractoryPeriods">Number of after spike computation cycles while an input stimuli is ignored (ms)</param>
        /// <param name="solverMethod">ODE numerical solver method</param>
        /// <param name="solverCompSteps">ODE numerical solver computation steps of the time step</param>
        public LeakyIF(double stimuliCoeff,
                       double timeScale,
                       double resistance,
                       double restV,
                       double resetV,
                       double firingThresholdV,
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
        /// LeakyIF autonomous ordinary differential equation.
        /// </summary>
        /// <param name="t">Time. Not used in autonomous ODE.</param>
        /// <param name="v">Membrane potential</param>
        /// <returns>dvdt</returns>
        protected override Vector MembraneDiffEq(double t, Vector v)
        {
            Vector dvdt = new Vector(1);
            dvdt[VarMembraneVIdx] = (-(v[VarMembraneVIdx] - _restV) + _resistance * _stimuli) / _timeScale;
            return dvdt;
        }

    }//LeakyIF

}//Namespace
