using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using RCNet.Extensions;
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
        private const int VarAdaptationOmega = 1;

        //Attributes
        //Parameter attributes
        private readonly double _membraneTimeScale;
        private readonly double _membraneResistance;
        private readonly double _rheobaseThresholdV;
        private readonly double _sharpnessDeltaT;
        private readonly double _adaptationVoltageCoupling;
        private readonly double _adaptationTimeConstant;
        private readonly double _spikeTriggeredAdaptationIncrement;


        /// <summary>
        /// Constructs an initialized instance
        /// </summary>
        /// <param name="settings">Encapsulated arguments</param>
        public AdExpIF(AdExpIFSettings settings)
            : base(PhysUnit.ToBase(settings.RestV, PhysUnit.MetricPrefix.Milli),
                   PhysUnit.ToBase(settings.ResetV, PhysUnit.MetricPrefix.Milli),
                   PhysUnit.ToBase(settings.FiringThresholdV, PhysUnit.MetricPrefix.Milli),
                   0,
                   PhysUnit.ToBase(settings.StimuliCoeff, PhysUnit.MetricPrefix.Piko),
                   settings.SolverMethod,
                   settings.SolverCompSteps,
                   2)
        {
            _membraneTimeScale = PhysUnit.ToBase(settings.TimeScale, PhysUnit.MetricPrefix.Milli);
            _membraneResistance = PhysUnit.ToBase(settings.Resistance, PhysUnit.MetricPrefix.Mega);
            _rheobaseThresholdV = PhysUnit.ToBase(settings.RheobaseThresholdV, PhysUnit.MetricPrefix.Milli);
            _sharpnessDeltaT = PhysUnit.ToBase(settings.SharpnessDeltaT, PhysUnit.MetricPrefix.Milli);
            _adaptationVoltageCoupling = PhysUnit.ToBase(settings.AdaptationVoltageCoupling, PhysUnit.MetricPrefix.Nano);
            _adaptationTimeConstant = PhysUnit.ToBase(settings.AdaptationTimeConstant, PhysUnit.MetricPrefix.Milli);
            _spikeTriggeredAdaptationIncrement = PhysUnit.ToBase(settings.SpikeTriggeredAdaptationIncrement, PhysUnit.MetricPrefix.Piko);
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
