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
    /// Implements Leaky Integrate and Fire neuron model.
    /// For more information visit http://neuronaldynamics.epfl.ch/online/Ch1.S3.html
    /// </summary>
    [Serializable]
    public class LeakyIF : ODESpikingMembrane
    {
        //Attributes
        //Parameters
        private readonly double _timeScale;
        private readonly double _resistance;

        //Constructor
        /// <summary>
        /// Constructs an initialized instance
        /// </summary>
        /// <param name="settings">Encapsulated arguments</param>
        /// <param name="rand">Random object to be used for randomly generated parameters</param>
        public LeakyIF(LeakyIFSettings settings, Random rand)
            : base(rand,
                   rand.NextDouble(settings.RestV),
                   rand.NextDouble(settings.ResetV),
                   rand.NextDouble(settings.FiringThresholdV),
                   settings.RefractoryPeriods,
                   settings.StimuliCoeff,
                   settings.SolverMethod,
                   1,
                   settings.SolverCompSteps,
                   1
                   )
        {
            _timeScale = rand.NextDouble(settings.TimeScale);
            _resistance = rand.NextDouble(settings.Resistance);
            return;
        }

        //Methods
        /// <summary>
        /// Triggered when membrane is firing a spike
        /// </summary>
        protected override void OnFiring()
        {
            //Does nothing
            return;
        }

        /// <summary>
        /// LeakyIF autonomous ordinary differential equation.
        /// </summary>
        /// <param name="t">Time. Not used in autonomous ODE.</param>
        /// <param name="v">Membrane potential</param>
        /// <returns>dvdt</returns>
        protected override Vector MembraneDiffEq(double t, Vector v)
        {
            Vector dvdt = new Vector(1);
            dvdt[VarMembraneVIdx] = (-(v[VarMembraneVIdx] - _restV) + _resistance * _stimuli) / _timeScale;
            return dvdt;
        }

    }//LeakyIF

}//Namespace
