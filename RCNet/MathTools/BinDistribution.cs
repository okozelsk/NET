using RCNet.Extensions;
using System;
using System.Collections.Generic;

namespace RCNet.MathTools
{
    /// <summary>
    /// implements an overview of the distribution of binary values.
    /// </summary>
    [Serializable]
    public class BinDistribution
    {
        //Attribute properties
        /// <summary>
        /// The binary border.
        /// </summary>
        /// <remarks>
        /// A value less than this border is considered as the 0. The 1 otherwise.
        /// </remarks>
        public double BinBorder { get; }
        /// <summary>
        /// The number of samples [0,1].
        /// </summary>
        public int[] NumOf { get; private set; }
        /// <summary>
        ///  The total number of samples.
        /// </summary>
        public int Count { get; private set; }
        /// <summary>
        /// The ratio of samples [0,1].
        /// </summary>
        public double[] Ratio { get; private set; }

        //Constructors
        /// <summary>
        /// Creates an uninitialized instance.
        /// </summary>
        /// <param name="binBorder">The binary border. A value less than this border is considered as the 0. The 1 otherwise.</param>
        public BinDistribution(double binBorder = 0)
        {
            BinBorder = binBorder;
            NumOf = new int[2];
            Ratio = new double[2];
            Reset();
            return;
        }

        /// <summary>
        /// Creates an initialized instance.
        /// </summary>
        /// <param name="sampleCollection">The samples.</param>
        /// <param name="binBorder">The binary border. A value less than this border is considered as the 0. The 1 otherwise.</param>
        public BinDistribution(IEnumerable<double> sampleCollection, double binBorder = 0)
            : this(binBorder)
        {
            Update(sampleCollection);
            return;
        }

        /// <summary>
        /// Creates an initialized instance.
        /// </summary>
        /// <param name="sampleVectorCollection">The sample vectors.</param>
        /// <param name="valueIdx">The index of the binary value inside a vector.</param>
        /// <param name="binBorder">The binary border. A value less than this border is considered as the 0. The 1 otherwise.</param>
        public BinDistribution(IEnumerable<double[]> sampleVectorCollection, int valueIdx, double binBorder = 0)
            : this(binBorder)
        {
            Update(sampleVectorCollection, valueIdx);
            return;
        }

        //Methods
        /// <summary>
        /// Updates the counts but does not recompute the ratios.
        /// </summary>
        /// <param name="sample">The sample.</param>
        private void UpdateCounts(double sample)
        {
            int binVal = (sample >= BinBorder) ? 1 : 0;
            ++NumOf[binVal];
            ++Count;
            return;
        }

        /// <summary>
        /// Recomputes the ratios.
        /// </summary>
        private void RecomputeRatios()
        {
            if (Count > 0)
            {
                for (int i = 0; i < 2; i++)
                {
                    Ratio[i] = (double)NumOf[i] / (double)Count;
                }
            }
            return;
        }

        /// <summary>
        /// Resets the instance.
        /// </summary>
        public void Reset()
        {
            NumOf.Populate(0);
            Count = 0;
            Ratio.Populate(0);
            return;
        }

        /// <summary>
        /// Creates the deep clone.
        /// </summary>
        public BinDistribution DeepClone()
        {
            BinDistribution clone = new BinDistribution(this.BinBorder)
            {
                Count = Count,
                NumOf = (int[])NumOf.Clone(),
                Ratio = (double[])Ratio.Clone()
            };
            return clone;
        }

        /// <summary>
        /// Updates the distribution.
        /// </summary>
        /// <param name="sampleVectorCollection">The sample vectors.</param>
        /// <param name="valueIdx">The index of the binary value inside a vector.</param>
        public void Update(IEnumerable<double[]> sampleVectorCollection, int valueIdx)
        {
            foreach (double[] vector in sampleVectorCollection)
            {
                UpdateCounts(vector[valueIdx]);
            }
            RecomputeRatios();
            return;
        }

        /// <summary>
        /// Updates the distribution.
        /// </summary>
        /// <param name="sampleCollection">The samples.</param>
        public void Update(IEnumerable<double> sampleCollection)
        {
            foreach (double sample in sampleCollection)
            {
                UpdateCounts(sample);
            }
            RecomputeRatios();
            return;
        }

        /// <summary>
        /// Updates the distribution
        /// </summary>
        /// <param name="sample">The sample.</param>
        public void Update(double sample)
        {
            UpdateCounts(sample);
            RecomputeRatios();
            return;
        }

    }//BinDistribution

}//Namespace
