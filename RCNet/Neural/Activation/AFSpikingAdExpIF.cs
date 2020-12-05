using RCNet.MathTools;
using RCNet.MathTools.Differential;
using RCNet.MathTools.VectorMath;
using System;

namespace RCNet.Neural.Activation
{
    /// <summary>
    /// Implements Adaptive Exponential Integrate and Fire neuron model.
    /// For more information visit http://neuronaldynamics.epfl.ch/online/Ch6.S1.html
    /// </summary>
    [Serializable]
    public class AFSpikingAdExpIF : AFSpikingODE
    {
        //Constants
        /// <summary>
        /// Index of AdaptationOmega evolving variable
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
        /// Constructs an initialized instance
        /// </summary>
        /// <param name="timeScale">Membrane time scale (ms)</param>
        /// <param name="resistance">Membrane resistance (Mohm)</param>
        /// <param name="restV">Membrane rest potential (mV)</param>
        /// <param name="resetV">Membrane reset potential (mV)</param>
        /// <param name="rheobaseV">Membrane rheobase threshold (mV)</param>
        /// <param name="firingThresholdV">Membrane firing threshold (mV)</param>
        /// <param name="sharpnessDeltaT">Sharpness of membrane potential change (mV)</param>
        /// <param name="adaptationVoltageCoupling">Adaptation voltage coupling (nS)</param>
        /// <param name="adaptationTimeConstant">Adaptation time constant (ms)</param>
        /// <param name="adaptationSpikeTriggeredIncrement">Spike triggered adaptation increment (pA)</param>
        /// <param name="solverMethod">ODE numerical solver method</param>
        /// <param name="solverCompSteps">ODE numerical solver computation steps of the time step</param>
        /// <param name="stimuliDuration">Duration of the stimulation</param>
        /// <param name="initialVRatio">Initial membrane potential in form of the ratio between 0 and 1 where 0 corresponds to a Min(resetV, restV) potential and 1 corresponds to a firingThreshold.</param>
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
            : base(PhysUnit.ToBase(restV, PhysUnit.MetricPrefix.Milli),
                   PhysUnit.ToBase(resetV, PhysUnit.MetricPrefix.Milli),
                   PhysUnit.ToBase(firingThresholdV, PhysUnit.MetricPrefix.Milli),
                   0,
                   solverMethod,
                   PhysUnit.ToBase(stimuliDuration, PhysUnit.MetricPrefix.Milli),
                   solverCompSteps,
                   2,
                   PhysUnit.FromBase(1d, PhysUnit.MetricPrefix.Giga),
                   PhysUnit.FromBase(1d, PhysUnit.MetricPrefix.Milli),
                   initialVRatio
                  )
        {
            _timeScale = PhysUnit.ToBase(timeScale, PhysUnit.MetricPrefix.Milli);
            _resistance = PhysUnit.ToBase(resistance, PhysUnit.MetricPrefix.Mega);
            _rheobaseV = PhysUnit.ToBase(rheobaseV, PhysUnit.MetricPrefix.Milli);
            _sharpnessDeltaT = PhysUnit.ToBase(sharpnessDeltaT, PhysUnit.MetricPrefix.Milli);
            _adaptationVoltageCoupling = PhysUnit.ToBase(adaptationVoltageCoupling, PhysUnit.MetricPrefix.Nano);
            _adaptationTimeConstant = PhysUnit.ToBase(adaptationTimeConstant, PhysUnit.MetricPrefix.Milli);
            _spikeTriggeredAdaptationIncrement = PhysUnit.ToBase(adaptationSpikeTriggeredIncrement, PhysUnit.MetricPrefix.Piko);
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
