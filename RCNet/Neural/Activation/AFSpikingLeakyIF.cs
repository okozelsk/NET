using RCNet.MathTools.Differential;
using RCNet.MathTools.VectorMath;
using System;

namespace RCNet.Neural.Activation
{
    /// <summary>
    /// Implements the Leaky Integrate and Fire neuron model.
    /// For more information visit http://neuronaldynamics.epfl.ch/online/Ch1.S3.html
    /// </summary>
    [Serializable]
    public class AFSpikingLeakyIF : AFSpikingODE
    {
        //Constants
        //Attributes
        //Parameters
        private readonly double _timeScale;
        private readonly double _resistance;

        //Constructor
        /// <summary>
        /// Creates an initialized instance
        /// </summary>
        /// <param name="timeScale">Membrane time scale (ms)</param>
        /// <param name="resistance">Membrane resistance (Mohm)</param>
        /// <param name="restV">Membrane rest potential (mV)</param>
        /// <param name="resetV">Membrane reset potential (mV)</param>
        /// <param name="firingThresholdV">Membrane firing threshold (mV)</param>
        /// <param name="refractoryPeriods">Number of after spike computation cycles while an input stimuli is ignored (ms)</param>
        /// <param name="solverMethod">ODE numerical solver method</param>
        /// <param name="solverCompSteps">ODE numerical solver computation steps of the time step</param>
        /// <param name="stimuliDuration">Duration of the stimulation</param>
        /// <param name="initialVRatio">Initial membrane potential in form of the ratio between 0 and 1 where 0 corresponds to a Min(resetV, restV) potential and 1 corresponds to a firingThreshold.</param>
        public AFSpikingLeakyIF(double timeScale,
                                double resistance,
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
                   1,
                   10,
                   1,
                   initialVRatio
                   )
        {
            _timeScale = timeScale;
            _resistance = resistance;
            return;
        }

        //Methods
        /// <inheritdoc/>
        protected override Vector MembraneDiffEq(double t, Vector v)
        {
            Vector dvdt = new Vector(1);
            dvdt[VarMembraneVIdx] = (-(v[VarMembraneVIdx] - _restV) + _resistance * _stimuli) / _timeScale;
            return dvdt;
        }

    }//AFSpikingLeakyIF

}//Namespace
