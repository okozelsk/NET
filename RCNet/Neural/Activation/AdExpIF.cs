using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using RCNet.Extensions;
using RCNet.MathTools;
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
        protected const int VarAdaptationOmega = 1;

        //Attributes
        //Parameter attributes
        private double _membraneTimeScale;
        private double _membraneResistance;
        private double _rheobaseThresholdV;
        private double _sharpnessDeltaT;
        private double _adaptationVoltageCoupling;
        private double _adaptationTimeConstant;
        private double _spikeTriggeredAdaptationIncrement;


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
        /// <param name="adaptationVoltageCoupling">Adaptation voltage coupling (nS)</param>
        /// <param name="adaptationTimeConstant">Adaptation time constant (ms)</param>
        /// <param name="spikeTriggeredAdaptationIncrement">Spike triggered adaptation increment (pA)</param>
        /// <param name="stimuliCoeff">Input stimuli coefficient (pA)</param>
        public AdExpIF(double membraneTimeScale,
                       double membraneResistance,
                       double restV,
                       double resetV,
                       double rheobaseThresholdV,
                       double firingThresholdV,
                       double sharpnessDeltaT,
                       double adaptationVoltageCoupling,
                       double adaptationTimeConstant,
                       double spikeTriggeredAdaptationIncrement,
                       double stimuliCoeff
                       )
            : base(PhysUnit.ToBase(restV, PhysUnit.MetricPrefix.Milli),
                   PhysUnit.ToBase(resetV, PhysUnit.MetricPrefix.Milli),
                   PhysUnit.ToBase(firingThresholdV, PhysUnit.MetricPrefix.Milli),
                   0,
                   PhysUnit.ToBase(stimuliCoeff, PhysUnit.MetricPrefix.Piko),
                   PhysUnit.ToBase(1, PhysUnit.MetricPrefix.Milli),
                   2,
                   2)
        {
            _membraneTimeScale = PhysUnit.ToBase(membraneTimeScale, PhysUnit.MetricPrefix.Milli);
            _membraneResistance = PhysUnit.ToBase(membraneResistance, PhysUnit.MetricPrefix.Mega);
            _rheobaseThresholdV = PhysUnit.ToBase(rheobaseThresholdV, PhysUnit.MetricPrefix.Milli);
            _sharpnessDeltaT = PhysUnit.ToBase(sharpnessDeltaT, PhysUnit.MetricPrefix.Milli);
            _adaptationVoltageCoupling = PhysUnit.ToBase(adaptationVoltageCoupling, PhysUnit.MetricPrefix.Nano);
            _adaptationTimeConstant = PhysUnit.ToBase(adaptationTimeConstant, PhysUnit.MetricPrefix.Milli);
            _spikeTriggeredAdaptationIncrement = PhysUnit.ToBase(spikeTriggeredAdaptationIncrement, PhysUnit.MetricPrefix.Piko);
            _evolVars[VarAdaptationOmega] = 0;
            return;
        }

        //Methods
        /// <summary>
        /// Resets function to initial state
        /// </summary>
        public override void Reset()
        {
            base.Reset();
            _evolVars[VarAdaptationOmega] = 0;
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
            //Ensure numerical stability
            double exponent = Math.Min((v[VarMembraneV] - _rheobaseThresholdV) / _sharpnessDeltaT, 20);
            dvdt[VarMembraneV] = (- (v[VarMembraneV] - _restV)
                                  + _sharpnessDeltaT * Math.Exp(exponent)
                                  - _membraneResistance * _evolVars[VarAdaptationOmega]
                                  + _membraneResistance * _stimuli
                                  ) / _membraneTimeScale;
            dvdt[VarAdaptationOmega] = ((_adaptationVoltageCoupling * (_evolVars[VarMembraneV]- _restV)
                                        - v[VarAdaptationOmega])
                                        ) / _adaptationTimeConstant;
            return dvdt;
        }

        /// <summary>
        /// When firing, adjusts (increments) the adaptation omega.
        /// </summary>
        protected override void OnFiring()
        {
            _evolVars[VarAdaptationOmega] += _spikeTriggeredAdaptationIncrement;
            return;
        }


    }//AdExpIF

}//Namespace
