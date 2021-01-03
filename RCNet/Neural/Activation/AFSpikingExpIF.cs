using RCNet.MathTools.Differential;
using RCNet.MathTools.VectorMath;
using System;

namespace RCNet.Neural.Activation
{
    /// <summary>
    /// Implements the Exponential Integrate and Fire neuron model.
    /// </summary>
    /// <remarks>
    /// For more information visit the http://neuronaldynamics.epfl.ch/online/Ch5.S2.html site.
    /// </remarks>
    [Serializable]
    public class AFSpikingExpIF : AFSpikingODE
    {
        //Constants
        //Attributes
        //Parameters
        private readonly double _timeScale;
        private readonly double _resistance;
        private readonly double _rheobaseV;
        private readonly double _sharpnessDeltaT;

        //Constructor
        /// <summary>
        /// Creates an initialized instance.
        /// </summary>
        /// <param name="timeScale">The membrane time scale (ms).</param>
        /// <param name="resistance">The membrane resistance (Mohm).</param>
        /// <param name="restV">The membrane rest potential (mV).</param>
        /// <param name="resetV">The membrane reset potential (mV).</param>
        /// <param name="rheobaseV">The membrane rheobase threshold (mV).</param>
        /// <param name="firingThresholdV">The membrane firing threshold (mV).</param>
        /// <param name="sharpnessDeltaT">The sharpness of membrane potential change (mV).</param>
        /// <param name="refractoryPeriods">The number of after-spike computation cycles while an input stimuli to be ignored (cycles).</param>
        /// <param name="solverMethod">The ODE numerical solver method to be used.</param>
        /// <param name="solverCompSteps">The number of computation sub-steps of the ODE numerical solver.</param>
        /// <param name="stimuliDuration">The duration of the membrane stimulation (ms).</param>
        /// <param name="initialVRatio">The membrane initial potential in form of a ratio between 0 and 1, where 0 corresponds to a Min(resetV, restV) potential and 1 corresponds to a firingThreshold.</param>
        public AFSpikingExpIF(double timeScale,
                              double resistance,
                              double restV,
                              double resetV,
                              double rheobaseV,
                              double firingThresholdV,
                              double sharpnessDeltaT,
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
            _rheobaseV = rheobaseV;
            _sharpnessDeltaT = sharpnessDeltaT;
            return;
        }

        //Methods
        /// <inheritdoc/>
        protected override Vector MembraneDiffEq(double t, Vector v)
        {
            Vector dvdt = new Vector(1);
            //Ensure numerical stability
            double exponent = Math.Min((v[VarMembraneVIdx] - _rheobaseV) / _sharpnessDeltaT, 20);
            dvdt[VarMembraneVIdx] = (-(v[VarMembraneVIdx] - _restV)
                                  + _sharpnessDeltaT * Math.Exp(exponent)
                                  + _resistance * _stimuli
                                  ) / _timeScale;
            return dvdt;
        }

    }//AFSpikingExpIF

}//Namespace
