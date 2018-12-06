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
        /// <param name="settings">Encapsulated arguments</param>
        /// <param name="rand">Random object to be used for randomly generated parameters</param>

        public AdExpIF(AdExpIFSettings settings, Random rand)
            : base(rand,
                   PhysUnit.ToBase(rand.NextDouble(settings.RestV), PhysUnit.MetricPrefix.Milli),
                   PhysUnit.ToBase(rand.NextDouble(settings.ResetV), PhysUnit.MetricPrefix.Milli),
                   PhysUnit.ToBase(rand.NextDouble(settings.FiringThresholdV), PhysUnit.MetricPrefix.Milli),
                   0,
                   PhysUnit.ToBase(settings.StimuliCoeff, PhysUnit.MetricPrefix.Piko),
                   settings.SolverMethod,
                   PhysUnit.ToBase(1, PhysUnit.MetricPrefix.Milli),
                   settings.SolverCompSteps,
                   2)
        {
            _timeScale = PhysUnit.ToBase(rand.NextDouble(settings.TimeScale), PhysUnit.MetricPrefix.Milli);
            _resistance = PhysUnit.ToBase(rand.NextDouble(settings.Resistance), PhysUnit.MetricPrefix.Mega);
            _rheobaseV = PhysUnit.ToBase(rand.NextDouble(settings.RheobaseV), PhysUnit.MetricPrefix.Milli);
            _sharpnessDeltaT = PhysUnit.ToBase(rand.NextDouble(settings.SharpnessDeltaT), PhysUnit.MetricPrefix.Milli);
            _adaptationVoltageCoupling = PhysUnit.ToBase(rand.NextDouble(settings.AdaptationVoltageCoupling), PhysUnit.MetricPrefix.Nano);
            _adaptationTimeConstant = PhysUnit.ToBase(rand.NextDouble(settings.AdaptationTimeConstant), PhysUnit.MetricPrefix.Milli);
            _spikeTriggeredAdaptationIncrement = PhysUnit.ToBase(rand.NextDouble(settings.AdaptationSpikeTriggeredIncrement), PhysUnit.MetricPrefix.Piko);
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
