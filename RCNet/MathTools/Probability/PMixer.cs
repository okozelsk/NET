using RCNet.Extensions;
using System;

namespace RCNet.MathTools.Probability
{
    /// <summary>
    /// Implements the mixer of the weighted probabilities.
    /// </summary>
    public static class PMixer
    {
        //Constants
        //The bounds
        private const double MinP = 1e-6;
        private const double MaxP = 1d - MinP;
        private const double MinX = -40d;
        private const double MaxX = 40d;

        //Properties
        /// <summary>
        /// Gets the probability range.
        /// </summary>
        public static Interval ProbabilityRange { get { return Interval.IntZP1; } }

        //Methods
        /// <summary>
        /// Logarithmically stretches the probability.
        /// </summary>
        /// <param name="p">The probability (between 0 and 1) to be stretched.</param>
        public static double Stretch(double p)
        {
            p = p.Bound(MinP, MaxP);
            return Math.Log(p / (1d - p));
        }

        /// <summary>
        /// Exponentially squashes the stretched probability.
        /// </summary>
        /// <remarks>
        /// The reversal funtion to Stretch.
        /// </remarks>
        /// <param name="x">The stretched probability.</param>
        public static double Squash(double x)
        {
            x = x.Bound(MinX, MaxX);
            return 1d / (1d + Math.Exp(-x));
        }

        /// <summary>
        /// Mixes specified weighted probabilities and computes the resulting probability.
        /// </summary>
        /// <param name="probabilities">The probabilities (between 0 and 1).</param>
        /// <param name="weights">The weights corresponding to the probabilities. When not specified the flat weights are used.</param>
        /// <returns>The resulting probability.</returns>
        public static double MixP(double[] probabilities, double[] weights = null)
        {
            if (probabilities == null)
            {
                throw new ArgumentNullException("probabilities");
            }
            if (weights == null)
            {
                //Weights are not specified
                //Prepare flat weights having the sum equal to 1
                weights = new double[probabilities.Length];
                weights.Populate(1d / probabilities.Length);
            }
            //Compute the weighted sum of stretched probabilities
            double sum = 0;
            for (int i = 0; i < probabilities.Length; i++)
            {
                sum += weights[i] * Stretch(probabilities[i]);
            }
            //Return the resulting probability
            return Squash(sum);
        }

    }//PMixer

}//Namespace
