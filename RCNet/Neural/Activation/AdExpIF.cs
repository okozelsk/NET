using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using RCNet.Extensions;
using RCNet.MathTools.Differential;

namespace RCNet.Neural.Activation
{
    /// <summary>
    /// Adaptive Exponential Integrate and Fire neuron model.
    /// </summary>
    public class AdExpIF : SpikingMembrane
    {
        //Attributes
        //Parameter attributes
        private double _membraneTimeScale;
        private double _membraneResistance;
        private double _rheobaseThresholdV;
        private double _sharpnessDeltaT;
        private double _adaptationVoltageCoupling;
        private double _adaptationTimeConstant;
        private double _spikeTriggeredAdaptationIncrement;

        //Operation attributes
        private double _adaptationOmega;

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
        /// <param name="stimuliCoeff"></param>
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
            : base(restV, resetV, firingThresholdV, 0, stimuliCoeff)
        {
            _membraneTimeScale = membraneTimeScale;
            _membraneResistance = membraneResistance;
            _rheobaseThresholdV = rheobaseThresholdV;
            _sharpnessDeltaT = sharpnessDeltaT;
            _adaptationVoltageCoupling = adaptationVoltageCoupling;
            _adaptationTimeConstant = adaptationTimeConstant;
            _spikeTriggeredAdaptationIncrement = spikeTriggeredAdaptationIncrement;
            _stimuliCoeff = stimuliCoeff;
            _adaptationOmega = 0;
            return;
        }

        //Methods
        /// <summary>
        /// Resets function to initial state
        /// </summary>
        public override void Reset()
        {
            base.Reset();
            _adaptationOmega = 0;
            return;
        }

        /// <summary>
        /// Computes the result of the activation function
        /// </summary>
        /// <param name="x">Argument</param>
        public override double Compute(double x)
        {
            _stimuli = (x * _stimuliCoeff).Bound();
            double output = 0;
            if (_membraneV >= _firingThresholdV)
            {
                _membraneV = _resetV;
            }
            //Compute adaptation
            _adaptationOmega = AutonomousODE.Solve(AdaptationDiffEq, _adaptationOmega, 1, 10, AutonomousODE.Method.Euler);
            //Compute membrane new potential
            _membraneV = AutonomousODE.Solve(MembraneVoltageDiffEq, _membraneV, 1, 10, AutonomousODE.Method.Euler);
            //Output
            if (_membraneV >= _firingThresholdV)
            {
                output = 1;
                _membraneV = _firingThresholdV;
                //Adaptation
                _adaptationOmega += _spikeTriggeredAdaptationIncrement;
            }
            return output;
        }

        /// <summary>
        /// Adaptive Exponential Integrate and Fire membrane differential equation
        /// </summary>
        /// <param name="membraneV">Membrane voltage</param>
        protected override double MembraneVoltageDiffEq(double membraneV)
        {
            //Ensure numerical stability
            double exponent = Math.Min((membraneV - _rheobaseThresholdV) / _sharpnessDeltaT, 20);
            return ((-(membraneV - _restV) + _sharpnessDeltaT * Math.Exp(exponent) - _membraneResistance * _adaptationOmega + _membraneResistance * _stimuli)) / (_membraneTimeScale);
        }

        /// <summary>
        /// Adaptive Exponential Integrate and Fire adaptation differential equation
        /// </summary>
        /// <param name="adaptationOmega">Membrane current adaptation</param>
        private double AdaptationDiffEq(double adaptationOmega)
        {
            return ((_adaptationVoltageCoupling * (_membraneV - _restV) - adaptationOmega)) / _adaptationTimeConstant;

        }

    }//AdExpIF

}//Namespace
