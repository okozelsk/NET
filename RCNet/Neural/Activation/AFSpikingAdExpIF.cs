using RCNet.MathTools;
using RCNet.MathTools.Differential;
using RCNet.MathTools.VectorMath;
using System;

namespace RCNet.Neural.Activation
{
    /// <summary>
    /// Implements the Adaptive Exponential Integrate and Fire neuron model.
    /// </summary>
    /// <remarks>
    /// For more information visit the http://neuronaldynamics.epfl.ch/online/Ch6.S1.html site.
    /// </remarks>
    [Serializable]
    public class AFSpikingAdExpIF : AFSpikingODE
    {
        //Constants
        /// <summary>
        /// An index of the AdaptationOmega evolving variable.
        /// </summary>
        private const int VarAdaptationOmegaIdx = 1;

        //Attributes
        //Parameters
        private readonly double _timeScale;
        private readonly double _resistance;
        private readonly double _rheobaseV;
        private readonly double _sharpnessDeltaT;
        private readonly double _adaptationVoltageCoupling;
        private readonly double _adaptationTimeConstant;
        private readonly double _spikeTriggeredAdaptationIncrement;

        /// <summary>
        /// Creates an initialized instance.
        /// </summary>
        /// <param name="timeScale">The membrane time scale (ms).</param>
        /// <param name="resistance">The membrane resistance (Mohm).</param>
        /// <param name="restV">The membrane rest potential (mV).</param>
        /// <param name="resetV">The membrane reset potential (mV).</param>
        /// <param name="rheobaseV">The membrane rheobase threshold (mV).</param>
        /// <param name="firingThresholdV">The membrane firing threshold (mV).</param>
        /// <param name="sharpnessDeltaT">The sharpness of the membrane potential change (mV).</param>
        /// <param name="adaptationVoltageCoupling">The adaptation voltage coupling (nS).</param>
        /// <param name="adaptationTimeConstant">The adaptation time constant (ms).</param>
        /// <param name="adaptationSpikeTriggeredIncrement">The spike triggered adaptation increment (pA).</param>
        /// <param name="solverMethod">The ODE numerical solver method to be used.</param>
        /// <param name="solverCompSteps">The number of computation sub-steps of the ODE numerical solver.</param>
        /// <param name="stimuliDuration">The duration of the membrane stimulation (ms).</param>
        /// <param name="initialVRatio">The membrane initial potential in form of a ratio between 0 and 1, where 0 corresponds to a Min(resetV, restV) potential and 1 corresponds to a firingThreshold.</param>
        public AFSpikingAdExpIF(double timeScale,
                                double resistance,
                                double restV,
                                double resetV,
                                double rheobaseV,
                                double firingThresholdV,
                                double sharpnessDeltaT,
                                double adaptationVoltageCoupling,
                                double adaptationTimeConstant,
                                double adaptationSpikeTriggeredIncrement,
                                ODENumSolver.Method solverMethod,
                                int solverCompSteps,
                                double stimuliDuration,
                                double initialVRatio = 0d
                                )
            : base(PhysUnit.ToBase(restV, PhysUnit.UnitPrefix.Milli),
                   PhysUnit.ToBase(resetV, PhysUnit.UnitPrefix.Milli),
                   PhysUnit.ToBase(firingThresholdV, PhysUnit.UnitPrefix.Milli),
                   0,
                   solverMethod,
                   PhysUnit.ToBase(stimuliDuration, PhysUnit.UnitPrefix.Milli),
                   solverCompSteps,
                   2,
                   PhysUnit.FromBase(1d, PhysUnit.UnitPrefix.Giga),
                   PhysUnit.FromBase(1d, PhysUnit.UnitPrefix.Milli),
                   initialVRatio
                  )
        {
            _timeScale = PhysUnit.ToBase(timeScale, PhysUnit.UnitPrefix.Milli);
            _resistance = PhysUnit.ToBase(resistance, PhysUnit.UnitPrefix.Mega);
            _rheobaseV = PhysUnit.ToBase(rheobaseV, PhysUnit.UnitPrefix.Milli);
            _sharpnessDeltaT = PhysUnit.ToBase(sharpnessDeltaT, PhysUnit.UnitPrefix.Milli);
            _adaptationVoltageCoupling = PhysUnit.ToBase(adaptationVoltageCoupling, PhysUnit.UnitPrefix.Nano);
            _adaptationTimeConstant = PhysUnit.ToBase(adaptationTimeConstant, PhysUnit.UnitPrefix.Milli);
            _spikeTriggeredAdaptationIncrement = PhysUnit.ToBase(adaptationSpikeTriggeredIncrement, PhysUnit.UnitPrefix.Piko);
            _evolVars[VarAdaptationOmegaIdx] = 0;
            return;
        }

        //Properties
        //Methods
        /// <inheritdoc/>
        public override void Reset()
        {
            base.Reset();
            _evolVars[VarAdaptationOmegaIdx] = 0;
            return;
        }

        /// <inheritdoc/>
        protected override Vector MembraneDiffEq(double t, Vector v)
        {
            Vector dvdt = new Vector(2);
            double exponent = (v[VarMembraneVIdx] - _rheobaseV) / _sharpnessDeltaT;
            //Ensure numerical stability
            exponent = Math.Max(Math.Min(exponent, 20), -20);
            //Compute deltas
            dvdt[VarMembraneVIdx] = (-(v[VarMembraneVIdx] - _restV)
                                  + (_sharpnessDeltaT * Math.Exp(exponent))
                                  - (_resistance * v[VarAdaptationOmegaIdx])
                                  + (_resistance * _stimuli)
                                  ) / _timeScale;
            dvdt[VarAdaptationOmegaIdx] = ((_adaptationVoltageCoupling * (v[VarMembraneVIdx] - _restV)
                                        - v[VarAdaptationOmegaIdx])
                                        ) / _adaptationTimeConstant;
            return dvdt;
        }

        /// <inheritdoc/>
        protected override void OnFiring()
        {
            _evolVars[VarAdaptationOmegaIdx] += _spikeTriggeredAdaptationIncrement;
            return;
        }

    }//AFSpikingAdExpIF

}//Namespace
