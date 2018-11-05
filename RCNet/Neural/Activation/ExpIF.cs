using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using RCNet.Extensions;
using RCNet.MathTools.Differential;
using RCNet.MathTools.VectorMath;

namespace RCNet.Neural.Activation
{
    /// <summary>
    /// Implements Exponential Integrate and Fire neuron model.
    /// For more information visit http://neuronaldynamics.epfl.ch/online/Ch5.S2.html
    /// </summary>
    [Serializable]
    public class ExpIF : ODESpikingMembrane
    {
        //Attributes
        //Parameter attributes
        private readonly double _membraneTimeScale;
        private readonly double _membraneResistance;
        private readonly double _rheobaseThresholdV;
        private readonly double _sharpnessDeltaT;

        //Constructor
        /// <summary>
        /// Constructs an initialized instance
        /// </summary>
        /// <param name="settings">Encapsulated arguments</param>
        public ExpIF(ExpIFSettings settings)
            : base(settings.RestV, settings.ResetV, settings.FiringThresholdV, settings.RefractoryPeriods, settings.StimuliCoeff, settings.SolverMethod, settings.SolverCompSteps, 1)
        {
            _membraneTimeScale = settings.TimeScale;
            _membraneResistance = settings.Resistance;
            _rheobaseThresholdV = settings.RheobaseThresholdV;
            _sharpnessDeltaT = settings.SharpnessDeltaT;
            return;
        }

        //Methods
        /// <summary>
        /// ExpIF autonomous ordinary differential equation.
        /// </summary>
        /// <param name="t">Time. Not used in autonomous ODE.</param>
        /// <param name="v">Membrane potential</param>
        /// <returns>dvdt</returns>
        protected override Vector MembraneDiffEq(double t, Vector v)
        {
            Vector dvdt = new Vector(1);
            //Ensure numerical stability
            double exponent = Math.Min((v[VarMembraneV] - _rheobaseThresholdV) / _sharpnessDeltaT, 20);
            dvdt[VarMembraneV] = (- (v[VarMembraneV] - _restV)
                                  + _sharpnessDeltaT * Math.Exp(exponent)
                                  + _membraneResistance * _stimuli
                                  ) / _membraneTimeScale;
            return dvdt;
        }

    }//ExpIF

}//Namespace
