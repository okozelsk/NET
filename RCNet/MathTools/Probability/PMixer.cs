using RCNet.Extensions;
using System;
using System.Collections.Generic;

namespace RCNet.MathTools.Probability
{
    /// <summary>
    /// Implements methods for weighted probabilities mixing to final probability
    /// </summary>
    public static class PMixer
    {
        /// <summary>
        /// Probability interval
        /// </summary>
        public static Interval ProbabilityRange = new Interval(0d, 1d);

        //Constants
        //Operating bounds
        private const double MinP = 1e-6;
        private const double MaxP = 1d - MinP;
        private const double MinX = -40d;
        private const double MaxX = 40d;

        //Methods
        /// <summary>
        /// Logarithmically stretches given probability
        /// </summary>
        /// <param name="p">Probability (between 0 and 1) to be stretched</param>
        public static double Stretch(double p)
        {
            p = p.Bound(MinP, MaxP);
            return Math.Log(p / (1d - p));
        }

        /// <summary>
        /// Exponentially squashes back previously stretched probability.
        /// Reversal funtion to Stretch.
        /// </summary>
        /// <param name="x">Stretched probability</param>
        public static double Squash(double x)
        {
            x = x.Bound(MinX, MaxX);
            return 1d / (1d + Math.Exp(-x));
        }

        /// <summary>
        /// Mixes given weighted probabilities to the resulting probability (between 0 and 1).
        /// </summary>
        /// <param name="probabilities">Probabilities (between 0 and 1)</param>
        /// <param name="weights">Weights corresponding to probabilities. When not specified, flat weights will be used.</param>
        public static double MixP(double[] probabilities, double[] weights = null)
        {
            if(probabilities == null)
            {
                throw new ArgumentNullException("probabilities");
            }
            if(weights == null)
            {
                //Weights are not specified
                //Prepare flat weights having sum equal to 1
                weights = new double[probabilities.Length];
                weights.Populate(1d / probabilities.Length);
            }
            //Compute sum of stretched weighted probabilities
            double sum = 0;
            for(int i = 0; i < probabilities.Length; i++)
            {
                sum += weights[i] * Stretch(probabilities[i]);
            }
            //Return resulting probability (squashed previously stretched weighted probabilities)
            return Squash(sum);
        }


    }//PMixer

}//Namespace
