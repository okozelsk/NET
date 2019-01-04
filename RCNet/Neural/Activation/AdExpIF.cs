using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using RCNet.MathTools;
using RCNet.MathTools.Differential;
using RCNet.MathTools.VectorMath;

namespace RCNet.Neural.Activation
{
    /// <summary>
    /// Implements Adaptive Exponential Integrate and Fire neuron model.
    /// For more information visit http://neuronaldynamics.epfl.ch/online/Ch6.S1.html
    /// </summary>
    [Serializable]
    public class AdExpIF : ODESpikingMembrane
    {
        //Constants
        //Constants
        //Typical values
        /// <summary>
        /// Typical value of time scale
        /// </summary>
        public const double TypicalTimeScale = 5;
        /// <summary>
        /// Typical value of resistance
        /// </summary>
        public const double TypicalResistance = 500;
        /// <summary>
        /// Typical value of resting voltage
        /// </summary>
        public const double TypicalRestV = -70;
        /// <summary>
        /// Typical value of reset voltage
        /// </summary>
        public const double TypicalResetV = -51;
        /// <summary>
        /// Typical value of rheobase
        /// </summary>
        public const double TypicalRheobaseV = -50;
        /// <summary>
        /// Typical value of firing voltage
        /// </summary>
        public const double TypicalFiringThresholdV = -30;
        /// <summary>
        /// Typical value of sharpness delta
        /// </summary>
        public const double TypicalSharpnessDeltaT = 2;
        /// <summary>
        /// Typical value of adaptation voltage coupling
        /// </summary>
        public const double TypicalAdaptationVoltageCoupling = 0.5;
        /// <summary>
        /// Typical value of adaptation time constant
        /// </summary>
        public const double TypicalAdaptationTimeConstant = 100;
        /// <summary>
        /// Typical value of spike triggered increment
        /// </summary>
        public const double TypicalAdaptationSpikeTriggeredIncrement = 7;


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
        /// <param name="stimuliCoeff">Input stimuli coefficient (pA)</param>
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
        public AdExpIF(double stimuliCoeff,
                       double timeScale,
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
                       int solverCompSteps
                       )
            : base(PhysUnit.ToBase(restV, PhysUnit.MetricPrefix.Milli),
                   PhysUnit.ToBase(resetV, PhysUnit.MetricPrefix.Milli),
                   PhysUnit.ToBase(firingThresholdV, PhysUnit.MetricPrefix.Milli),
                   0,
                   PhysUnit.ToBase(stimuliCoeff, PhysUnit.MetricPrefix.Piko),
                   solverMethod,
                   PhysUnit.ToBase(1, PhysUnit.MetricPrefix.Milli),
                   solverCompSteps,
                   2
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

        //Methods
        /// <summary>
        /// Resets function to initial state
        /// </summary>
        public override void Reset()
        {
            base.Reset();
            _evolVars[VarAdaptationOmegaIdx] = 0;
            return;
        }

        /// <summary>
        /// AdExpIF couple of autonomous ordinary differential equations.
        /// </summary>
        /// <param name="t">Time. Not used in autonomous ODE.</param>
        /// <param name="v">Membrane potential and Adaptation omega.</param>
        /// <returns>dvdt</returns>
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

        /// <summary>
        /// When firing, adjusts (increments) the adaptation omega.
        /// </summary>
        protected override void OnFiring()
        {
            _evolVars[VarAdaptationOmegaIdx] += _spikeTriggeredAdaptationIncrement;
            return;
        }


    }//AdExpIF

}//Namespace
