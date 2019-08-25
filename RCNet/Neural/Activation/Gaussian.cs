﻿using System;
using RCNet.Extensions;
using RCNet.MathTools;

namespace RCNet.Neural.Activation
{
    /// <summary>
    /// Implements Gaussian activation function
    /// </summary>
    [Serializable]
    public class Gaussian : AnalogActivationFunction
    {

        //Static members
        private static readonly Interval _outputRange = new Interval(0, 1);

        //Constructor
        /// <summary>
        /// Creates an initialized instance
        /// </summary>
        public Gaussian()
            : base()
        {
            return;
        }

        //Properties
        /// <summary>
        /// Output signal range
        /// </summary>
        public override Interval OutputRange { get { return _outputRange; } }

        //Methods
        /// <summary>
        /// Computes output of the activation function
        /// </summary>
        /// <param name="x">Activation input</param>
        public override double Compute(double x)
        {
            x = x.Bound();
            return (Math.Exp(-(x.Power(2)))).Bound();
        }

        /// <summary>
        /// Computes derivative of the activation input
        /// </summary>
        /// <param name="c">The result of the activation (Compute method)</param>
        /// <param name="x">Activation input (x argument of the Compute method)</param>
        public override double ComputeDerivative(double c, double x)
        {
            c = c.Bound();
            x = x.Bound();
            return -2*x*c;
        }

    }//Gaussian

}//Namespace
