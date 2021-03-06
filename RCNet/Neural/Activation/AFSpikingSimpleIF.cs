﻿using System;

namespace RCNet.Neural.Activation
{
    /// <summary>
    /// Implements a very simple form of the Integrate and Fire neuron model.
    /// </summary>
    [Serializable]
    public class AFSpikingSimpleIF : AFSpikingBase
    {
        //Attributes
        private readonly double _resistance;
        private readonly double _decayRate;

        //Constructor
        /// <summary>
        /// Creates an initialized instance.
        /// </summary>
        /// <param name="resistance">The membrane resistance (Mohm).</param>
        /// <param name="decayRate">The membrane potential decay rate.</param>
        /// <param name="resetV">The membrane reset potential (mV).</param>
        /// <param name="firingThresholdV">The membrane firing threshold (mV).</param>
        /// <param name="refractoryPeriods">The number of after-spike computation cycles while an input stimuli to be ignored (cycles).</param>
        /// <param name="initialVRatio">The membrane initial potential in form of the ratio between 0 and 1 where 0 corresponds to a restV potential and 1 corresponds to a firingThreshold.</param>
        public AFSpikingSimpleIF(double resistance,
                                 double decayRate,
                                 double resetV,
                                 double firingThresholdV,
                                 int refractoryPeriods,
                                 double initialVRatio = 0d
                                 )
            : base(0d, Math.Abs(resetV), Math.Abs(firingThresholdV), refractoryPeriods, 1, 1, 1, initialVRatio)
        {
            _resistance = resistance;
            _decayRate = decayRate;
            return;
        }

        //Methods
        /// <inheritdoc/>
        public override bool ComputeEvolVars()
        {
            //Compute membrane new potential
            //Apply decay
            _evolVars[VarMembraneVIdx] = _restV + (_evolVars[VarMembraneVIdx] - _restV) * (1d - _decayRate);
            //Apply increment
            _evolVars[VarMembraneVIdx] += _resistance * _stimuli;
            //Output
            if (_evolVars[VarMembraneVIdx] >= _firingThresholdV)
            {
                _evolVars[VarMembraneVIdx] = _firingThresholdV;
                return true;
            }
            else
            {
                if (_evolVars[VarMembraneVIdx] < _restV)
                {
                    _evolVars[VarMembraneVIdx] = _restV;
                }
                return false;
            }
        }

    }//AFSpikingSimpleIF

}//Namespace
