using System;
using RCNet.Extensions;
using RCNet.MathTools;
using RCNet.MathTools.VectorMath;

namespace RCNet.Neural.Activation
{
    /// <summary>
    /// Base class for the spiking activation functions.
    /// </summary>
    [Serializable]
    public abstract class AFSpikingBase : IActivation
    {
        //Constants
        /// <summary>
        /// Index of MembraneV evolving variable
        /// </summary>
        protected const int VarMembraneVIdx = 0;

        //Parameters
        /// <summary>
        /// Range of the membrane potential
        /// </summary>
        protected Interval _vRange;
        /// <summary>
        /// Membrane rest potential
        /// </summary>
        protected double _restV;
        /// <summary>
        /// Membrane after reset potential
        /// </summary>
        protected double _resetV;
        /// <summary>
        /// Minimum membrane voltage
        /// </summary>
        protected double _minV;
        /// <summary>
        /// Membrane initial potential
        /// </summary>
        protected double _initialV;
        /// <summary>
        /// Membrane firing potential
        /// </summary>
        protected double _firingThresholdV;
        /// <summary>
        /// Refractory periods after firing
        /// </summary>
        protected int _refractoryPeriods;
        /// <summary>
        /// Coefficient for conversion of incoming stimuli current to expected physical unit 
        /// </summary>
        protected double _currentCoeff;
        /// <summary>
        /// Coefficient for conversion of membrane potential to expected physical unit 
        /// </summary>
        protected double _potentialCoeff;

        //Operation attributes
        /// <summary>
        /// Evolving variables
        /// </summary>
        protected Vector _evolVars;
        /// <summary>
        /// Specifies whether the membrane is in refractory mode
        /// </summary>
        protected bool _inRefractory;
        /// <summary>
        /// Specifies current refractory period of the membrane
        /// </summary>
        protected int _refractoryPeriod;
        /// <summary>
        /// Adjusted (modified) input stimuli
        /// </summary>
        protected double _stimuli;

        /// <summary>
        /// Constructs an initialized instance
        /// </summary>
        /// <param name="restV">Membrane rest potential</param>
        /// <param name="resetV">Membrane reset potential</param>
        /// <param name="firingThresholdV">Firing threshold</param>
        /// <param name="refractoryPeriods">Refractory periods</param>
        /// <param name="numOfEvolvingVars">Number of evolving variables</param>
        /// <param name="inputCurrentCoeff">Coefficient of the current</param>
        /// <param name="membranePotentialCoeff">Coefficient of the membrane potential</param>
        /// <param name="initialVRatio">Initial membrane potential in form of the ratio between 0 and 1 where 0 corresponds to a Min(resetV, restV) potential and 1 corresponds to a firingThreshold.</param>
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
        /// Range of state of the membrane potential
        /// </summary>
        public Interval InternalStateRange { get; }

        /// <summary>
        /// State of the membrane potential
        /// </summary>
        public double InternalState { get { return _potentialCoeff * _evolVars[VarMembraneVIdx]; } }

        /// <summary>
        /// State of the membrane potential normalized between 0 and 1
        /// </summary>
        public double NormalizedInternalState { get { return InternalStateRange.Rescale(InternalState, Interval.IntZP1); } }

        //Methods
        /// <summary>
        /// Resets activation function to its initial state
        /// </summary>
        public virtual void Reset()
        {
            _evolVars[VarMembraneVIdx] = _initialV;
            _inRefractory = false;
            _refractoryPeriod = 0;
            return;
        }

        /// <summary>
        /// Computes evolving variables
        /// </summary>
        /// <returns>Resulting spike (true) or no spike (false)</returns>
        public abstract bool ComputeEvolVars();


        /// <summary>
        /// Triggered when membrane is firing a spike
        /// </summary>
        protected virtual void OnFiring()
        {
            //Base implementation does nothing
            return;
        }

        /// <inheritdoc/>
        public virtual double Compute(double x)
        {
            //Reset the membrane potential?
            if(_evolVars[VarMembraneVIdx] == _firingThresholdV)
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
