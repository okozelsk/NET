using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using RCNet.Extensions;

namespace RCNet.MathTools
{
    /// <summary>
    /// Class contains distribution of binary values
    /// </summary>
    [Serializable]
    public class BinDistribution
    {
        //Attribute properties
        /// <summary>
        /// Binary 0/1 border.
        /// Double value LT this border is considered 0 and GE 1
        /// </summary>
        public double BinBorder { get; set; }
        /// <summary>
        /// Number of bin 0/1.
        /// </summary>
        public int[] NumOf { get; set; }
        /// <summary>
        ///  Total count of bin 0/1.
        /// </summary>
        public int Count { get; set; }
        /// <summary>
        /// Rates of bin 0/1.
        /// </summary>
        public double[] BinRate { get; set; }

        //Constructors
        /// <summary>
        /// Ctrates an uninitialized instance
        /// </summary>
        /// <param name="binBorder">Binary 0/1 border. Double value LT this border is considered 0 and GE 1. Default is 0 for (-1,1)</param>
        public BinDistribution(double binBorder = 0)
        {
            BinBorder = binBorder;
            NumOf = new int[2];
            BinRate = new double[2];
            Reset();
            return;
        }

        /// <summary>
        /// Ctrates an initialized instance
        /// </summary>
        /// <param name="vectorCollection">Collection of vectors containing binary field to be inspected</param>
        /// <param name="valueIdx">Index of the binary field within the vector</param>
        /// <param name="binBorder">Binary 0/1 border. Double value LT this border is considered 0 and GE 1</param>
        public BinDistribution(IEnumerable<double[]> vectorCollection, int valueIdx, double binBorder)
            :this(binBorder)
        {
            Update(vectorCollection, valueIdx);
            return;
        }

        /// <summary>
        /// Ctrates an initialized instance
        /// </summary>
        /// <param name="valueCollection">Collection of binary values to be inspected</param>
        /// <param name="binBorder">Binary 0/1 border. Double value LT this border is considered 0 and GE 1</param>
        public BinDistribution(IEnumerable<double> valueCollection, double binBorder)
            : this(binBorder)
        {
            Update(valueCollection);
            return;
        }

        //Methods
        /// <summary>
        /// Updates counts but does not recompute the rates
        /// </summary>
        /// <param name="value">Value of the binary field</param>
        private void UpdateCounts(double value)
        {
            int binVal = (value >= BinBorder) ? 1 : 0;
            ++NumOf[binVal];
            ++Count;
            return;
        }

        /// <summary>
        /// Recomputes the rates
        /// </summary>
        private void RecomputeRates()
        {
            if (Count > 0)
            {
                for (int i = 0; i < 2; i++)
                {
                    BinRate[i] = (double)NumOf[i] / (double)Count;
                }
            }
            return;
        }

        /// <summary>
        /// Resets the instance to the initial state
        /// </summary>
        public void Reset()
        {
            NumOf.Populate(0);
            Count = 0;
            BinRate.Populate(0);
            return;
        }

        /// <summary>
        /// Creates a deep copy clone
        /// </summary>
        public BinDistribution DeepClone()
        {
            BinDistribution clone = new BinDistribution(this.BinBorder);
            clone.Count = Count;
            clone.NumOf = (int[])NumOf.Clone();
            clone.BinRate = (double[])BinRate.Clone();
            return clone;
        }

        /// <summary>
        /// Updates the distribution
        /// </summary>
        /// <param name="vectorCollection">Collection of vectors containing binary field to be inspected</param>
        /// <param name="valueIdx">Index of the binary field within the vector</param>
        public void Update(IEnumerable<double[]> vectorCollection, int valueIdx)
        {
            foreach(double[] vector in vectorCollection)
            {
                UpdateCounts(vector[valueIdx]);
            }
            RecomputeRates();
            return;
        }

        /// <summary>
        /// Updates the distribution
        /// </summary>
        /// <param name="valueCollection">Collection of binary values to be inspected</param>
        public void Update(IEnumerable<double> valueCollection)
        {
            foreach (double value in valueCollection)
            {
                UpdateCounts(value);
            }
            RecomputeRates();
            return;
        }

        /// <summary>
        /// Updates the distribution
        /// </summary>
        /// <param name="value">Value of the binary field</param>
        public void Update(double value)
        {
            UpdateCounts(value);
            RecomputeRates();
            return;
        }


    }//BinDistribution
}//Namespace
