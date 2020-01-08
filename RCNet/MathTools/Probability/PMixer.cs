using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using RCNet.Extensions;

namespace RCNet.MathTools.Probability
{
    /// <summary>
    /// Implements methods for weighted probabilities mixing to final probability
    /// </summary>
    public static class PMixer
    {
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
        /// <param name="weights">Weights corresponding to probabilities</param>
        public static double MixP(IEnumerable<double> probabilities, IEnumerable<double> weights = null)
        {
            IEnumerator<double> wEnum = weights?.GetEnumerator();
            //Compute stretched weighted sum
            double sum = 0;
            foreach (double p in probabilities)
            {
                wEnum?.MoveNext();
                sum += (wEnum == null ? 1d : wEnum.Current) * Stretch(p);
            }
            //Return squashed weighted sum
            return Squash(sum);
        }


    }//PMixer

}//Namespace
