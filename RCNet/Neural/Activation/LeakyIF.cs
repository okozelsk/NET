﻿using System;
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
    /// Implements Leaky Integrate and Fire neuron model.
    /// For more information visit http://neuronaldynamics.epfl.ch/online/Ch1.S3.html
    /// </summary>
    [Serializable]
    public class LeakyIF : ODESpikingMembrane
    {
        //Attributes
        //Parameters
        private readonly double _membraneTimeScale;
        private readonly double _membraneResistance;

        /// <summary>
        /// Constructs an initialized instance
        /// </summary>
        /// <param name="settings">Encapsulated arguments</param>
        public LeakyIF(LeakyIFSettings settings)
            : base(settings.RestV, settings.ResetV, settings.FiringThresholdV, settings.RefractoryPeriods, settings.StimuliCoeff, settings.SolverMethod, settings.SolverCompSteps, 1)
        {
            _membraneTimeScale = settings.TimeScale;
            _membraneResistance = settings.Resistance;
            return;
        }

        //Methods
        /// <summary>
        /// LeakyIF autonomous ordinary differential equation.
        /// </summary>
        /// <param name="t">Time. Not used in autonomous ODE.</param>
        /// <param name="v">Membrane potential</param>
        /// <returns>dvdt</returns>
        protected override Vector MembraneDiffEq(double t, Vector v)
        {
            Vector dvdt = new Vector(1);
            dvdt[VarMembraneV] = (-(v[VarMembraneV] - _restV) + _membraneResistance * _stimuli) / _membraneTimeScale;
            return dvdt;
        }

    }//LeakyIF

}//Namespace