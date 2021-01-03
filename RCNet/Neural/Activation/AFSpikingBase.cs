using RCNet.Extensions;
using RCNet.MathTools;
using RCNet.MathTools.VectorMath;
using System;

namespace RCNet.Neural.Activation
{
    /// <summary>
    /// The base class for the spiking activation functions.
    /// </summary>
    [Serializable]
    public abstract class AFSpikingBase : IActivation
    {
        //Constants
        /// <summary>
        /// An index of the MembraneV evolving variable.
        /// </summary>
        protected const int VarMembraneVIdx = 0;

        //Parameters
        /// <summary>
        /// The range of the membrane potential.
        /// </summary>
        protected Interval _vRange;
        /// <summary>
        /// The membrane rest potential.
        /// </summary>
        protected double _restV;
        /// <summary>
        /// The mmbrane after reset potential.
        /// </summary>
        protected double _resetV;
        /// <summary>
        /// The minimum membrane voltage.
        /// </summary>
        protected double _minV;
        /// <summary>
        /// The membrane initial potential.
        /// </summary>
        protected double _initialV;
        /// <summary>
        /// The membrane firing potential.
        /// </summary>
        protected double _firingThresholdV;
        /// <summary>
        /// The number of refractory periods after the firing.
        /// </summary>
        protected int _refractoryPeriods;
        /// <summary>
        /// The coefficient for conversion of incoming stimuli current to expected quantity unit.
        /// </summary>
        protected double _currentCoeff;
        /// <summary>
        /// The coefficient for conversion of membrane potential to expected quantity unit.
        /// </summary>
        protected double _potentialCoeff;

        //Operation attributes
        /// <summary>
        /// The inner evolving variables.
        /// </summary>
        protected Vector _evolVars;
        /// <summary>
        /// Indicates whether the membrane is in the refractory mode.
        /// </summary>
        protected bool _inRefractory;
        /// <summary>
        /// The current refractory period number of the membrane.
        /// </summary>
        protected int _refractoryPeriod;
        /// <summary>
        /// The adjusted input stimuli.
        /// </summary>
        protected double _stimuli;

        /// <summary>
        /// Creates an initialized instance.
        /// </summary>
        /// <param name="restV">The membrane rest potential.</param>
        /// <param name="resetV">The membrane reset potential.</param>
        /// <param name="firingThresholdV">The membrane firing threshold.</param>
        /// <param name="refractoryPeriods">The number of refractory periods.</param>
        /// <param name="numOfEvolvingVars">The number of inner evolving variables.</param>
        /// <param name="inputCurrentCoeff">The coefficient of the input current.</param>
        /// <param name="membranePotentialCoeff">The coefficient of the membrane potential.</param>
        /// <param name="initialVRatio">The membrane initial potential in form of a ratio between 0 and 1, where 0 corresponds to a Min(resetV, restV) potential and 1 corresponds to a firingThreshold.</param>
        protected AFSpikingBase(double restV,
                                double resetV,
                                double firingThresholdV,
                                int refractoryPeriods,
                                int numOfEvolvingVars,
                                double inputCurrentCoeff = 1d,
                                double membranePotentialCoeff = 1d,
                                double initialVRatio = 0d
                                )
        {
            _restV = restV;
            _resetV = resetV;
            _minV = Math.Min(_resetV, _restV);
            _firingThresholdV = firingThresholdV;
            _refractoryPeriods = refractoryPeriods;
            _evolVars = new Vector(numOfEvolvingVars);
            _inRefractory = false;
            _refractoryPeriod = 0;
            _currentCoeff = inputCurrentCoeff;
            _potentialCoeff = membranePotentialCoeff;
            InternalStateRange = new Interval(_potentialCoeff * Math.Min(_resetV, _restV), _potentialCoeff * _firingThresholdV);
            _initialV = _minV + initialVRatio * (_firingThresholdV - _minV);
            _evolVars[VarMembraneVIdx] = _initialV;
            return;
        }

        //Properties
        ///<inheritdoc/>
        public ActivationType TypeOfActivation { get { return ActivationType.Spiking; } }

        ///<inheritdoc/>
        public Interval OutputRange { get { return Interval.IntZP1; } }

        //Attribute properties
        /// <summary>
        /// The range of the state of the membrane potential.
        /// </summary>
        public Interval InternalStateRange { get; }

        /// <summary>
        /// The state of the membrane potential.
        /// </summary>
        public double InternalState { get { return _potentialCoeff * _evolVars[VarMembraneVIdx]; } }

        /// <summary>
        /// The state of the membrane potential normalized between 0 and 1.
        /// </summary>
        public double NormalizedInternalState { get { return InternalStateRange.Rescale(InternalState, Interval.IntZP1); } }

        //Methods
        /// <summary>
        /// Resets the activation function to its initial state.
        /// </summary>
        public virtual void Reset()
        {
            _evolVars[VarMembraneVIdx] = _initialV;
            _inRefractory = false;
            _refractoryPeriod = 0;
            return;
        }

        /// <summary>
        /// Computes the inner evolving variables.
        /// </summary>
        /// <returns>The resulting spike (true) or no spike (false).</returns>
        public abstract bool ComputeEvolVars();


        /// <summary>
        /// Triggered hook when membrane is firing a spike.
        /// </summary>
        protected virtual void OnFiring()
        {
            //The base implementation does nothing
            return;
        }

        /// <inheritdoc/>
        public virtual double Compute(double x)
        {
            //Reset the membrane potential?
            if (_evolVars[VarMembraneVIdx] == _firingThresholdV)
            {
                _evolVars[VarMembraneVIdx] = _resetV;
                //Enter refractory?
                if (_refractoryPeriods > 0)
                {
                    _refractoryPeriod = 0;
                    _inRefractory = true;
                }
            }
            //Exit refractory?
            if (_inRefractory)
            {
                ++_refractoryPeriod;
                if (_refractoryPeriod > _refractoryPeriods)
                {
                    _refractoryPeriod = 0;
                    _inRefractory = false;
                }
                else
                {
                    //Ignore stimuli
                    x = 0d;
                }
            }

            //Compute evolving variables
            _stimuli = (x * _currentCoeff).Bound();
            bool spike = ComputeEvolVars();
            //Firing?
            if (spike)
            {
                OnFiring();
                //Spike
                return Interval.IntZP1.Max;
            }
            else
            {
                //No spike
                return Interval.IntZP1.Min;
            }
        }

    }//AFSpikingBase

}//Namespace
