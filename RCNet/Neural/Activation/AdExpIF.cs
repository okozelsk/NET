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
    /// Adaptive Exponential Integrate and Fire neuron model.
    /// </summary>
    public class AdExpIF : SpikingMembrane
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
        /// <param name="membraneTimeScale">(ms)</param>
        /// <param name="membraneResistance">(MOhm)</param>
        /// <param name="restV">(mV)</param>
        /// <param name="resetV">(mV)</param>
        /// <param name="rheobaseThresholdV">(mV)</param>
        /// <param name="firingThresholdV">(mV)</param>
        /// <param name="sharpnessDeltaT">(mV)</param>
        /// <param name="adaptationVoltageCoupling">(nS)</param>
        /// <param name="adaptationTimeConstant">(ms)</param>
        /// <param name="spikeTriggeredAdaptationIncrement">(pA)</param>
        /// <param name="stimuliCoeff">(pA)</param>
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
            : base(PhysUnit.ToBase(restV, PhysUnit.QPrefix.Milli),
                   PhysUnit.ToBase(resetV, PhysUnit.QPrefix.Milli),
                   PhysUnit.ToBase(firingThresholdV, PhysUnit.QPrefix.Milli),
                   0,
                   PhysUnit.ToBase(stimuliCoeff, PhysUnit.QPrefix.Piko),
                   PhysUnit.ToBase(1, PhysUnit.QPrefix.Milli),
                   2,
                   2)
        {
            _membraneTimeScale = PhysUnit.ToBase(membraneTimeScale, PhysUnit.QPrefix.Milli);
            _membraneResistance = PhysUnit.ToBase(membraneResistance, PhysUnit.QPrefix.Mega);
            _rheobaseThresholdV = PhysUnit.ToBase(rheobaseThresholdV, PhysUnit.QPrefix.Milli);
            _sharpnessDeltaT = PhysUnit.ToBase(sharpnessDeltaT, PhysUnit.QPrefix.Milli);
            _adaptationVoltageCoupling = PhysUnit.ToBase(adaptationVoltageCoupling, PhysUnit.QPrefix.Nano);
            _adaptationTimeConstant = PhysUnit.ToBase(adaptationTimeConstant, PhysUnit.QPrefix.Milli);
            _spikeTriggeredAdaptationIncrement = PhysUnit.ToBase(spikeTriggeredAdaptationIncrement, PhysUnit.QPrefix.Piko);
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

        protected override void OnFiring()
        {
            _evolVars[VarAdaptationOmega] += _spikeTriggeredAdaptationIncrement;
            return;
        }


    }//AdExpIF

}//Namespace
