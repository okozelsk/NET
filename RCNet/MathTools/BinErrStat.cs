using System;
using System.Collections.Generic;

namespace RCNet.MathTools
{
    /// <summary>
    /// Implements the error statistics of computed and ideal binary values.
    /// </summary>
    [Serializable]
    public class BinErrStat
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
        /// The error statistcs [0,1].
        /// </summary>
        public BasicStat[] BinValErrStat { get; }
        /// <summary>
        /// The total error statistics.
        /// </summary>
        public BasicStat TotalErrStat { get; }

        //Constructors
        /// <summary>
        /// Creates an uninitialized instance.
        /// </summary>
        /// <param name="binBorder">The binary border. A value less than this border is considered as the 0. The 1 otherwise.</param>
        public BinErrStat(double binBorder)
        {
            BinBorder = binBorder;
            BinValErrStat = new BasicStat[2];
            BinValErrStat[0] = new BasicStat();
            BinValErrStat[1] = new BasicStat();
            TotalErrStat = new BasicStat();
            return;
        }

        /// <summary>
        /// Creates an initialized instance.
        /// </summary>
        /// <param name="binBorder">The binary border. A value less than this border is considered as the 0. The 1 otherwise.</param>
        /// <param name="computedVectorCollection">The collection of computed vectors.</param>
        /// <param name="idealVectorCollection">The collection of ideal vectors.</param>
        public BinErrStat(double binBorder,
                          IEnumerable<double[]> computedVectorCollection,
                          IEnumerable<double[]> idealVectorCollection
                          )
            : this(binBorder)
        {
            Update(computedVectorCollection, idealVectorCollection);
            return;
        }

        /// <summary>
        /// Construct an initialized instance
        /// </summary>
        /// <param name="binBorder">The binary border. A value less than this border is considered as the 0. The 1 otherwise.</param>
        /// <param name="computedValueCollection">The collection of computed values.</param>
        /// <param name="idealValueCollection">The collection of ideal values.</param>
        public BinErrStat(double binBorder,
                          IEnumerable<double> computedValueCollection,
                          IEnumerable<double> idealValueCollection
                          )
            : this(binBorder)
        {
            Update(computedValueCollection, idealValueCollection);
            return;
        }

        /// <summary>
        /// The deep copy constructor.
        /// </summary>
        /// <param name="source">The source instance.</param>
        public BinErrStat(BinErrStat source)
        {
            BinBorder = source.BinBorder;
            BinValErrStat = new BasicStat[2];
            BinValErrStat[0] = new BasicStat(source.BinValErrStat[0]);
            BinValErrStat[1] = new BasicStat(source.BinValErrStat[1]);
            TotalErrStat = new BasicStat(source.TotalErrStat);
            return;
        }

        //Methods
        /// <summary>
        /// Decides whether the computed and ideal value represent the same binary value.
        /// </summary>
        /// <param name="computedValue">The computed value.</param>
        /// <param name="idealValue">The ideal value.</param>
        private bool BinMatch(double computedValue, double idealValue)
        {
            if (computedValue >= BinBorder && idealValue >= BinBorder) return true;
            if (computedValue < BinBorder && idealValue < BinBorder) return true;
            return false;
        }

        /// <summary>
        /// Updates the error statistics.
        /// </summary>
        /// <param name="computedValue">The computed value.</param>
        /// <param name="idealValue">The ideal value.</param>
        public void Update(double computedValue, double idealValue)
        {
            int idealBinVal = (idealValue >= BinBorder) ? 1 : 0;
            int errValue = BinMatch(computedValue, idealValue) ? 0 : 1;
            BinValErrStat[idealBinVal].AddSample(errValue);
            TotalErrStat.AddSample(errValue);
            return;
        }

        /// <summary>
        /// Updates the error statistics.
        /// </summary>
        /// <param name="computedValueCollection">The collection of computed values.</param>
        /// <param name="idealValueCollection">The collection of ideal values.</param>
        public void Update(IEnumerable<double> computedValueCollection, IEnumerable<double> idealValueCollection)
        {
            IEnumerator<double> idealValueEnumerator = idealValueCollection.GetEnumerator();
            foreach (double computedValue in computedValueCollection)
            {
                idealValueEnumerator.MoveNext();
                double idealValue = idealValueEnumerator.Current;
                Update(computedValue, idealValue);
            }
            return;
        }

        /// <summary>
        /// Updates the error statistics.
        /// </summary>
        /// <param name="computedVectorCollection">The collection of computed vectors.</param>
        /// <param name="idealVectorCollection">The collection of ideal vectors.</param>
        public void Update(IEnumerable<double[]> computedVectorCollection, IEnumerable<double[]> idealVectorCollection)
        {
            IEnumerator<double[]> idealVectorEnumerator = idealVectorCollection.GetEnumerator();
            foreach (double[] computedVector in computedVectorCollection)
            {
                idealVectorEnumerator.MoveNext();
                double[] idealVector = idealVectorEnumerator.Current;
                for (int i = 0; i < idealVector.Length; i++)
                {
                    Update(computedVector[i], idealVector[i]);
                }
            }
            return;
        }

        /// <summary>
        /// Creates the deep copy instance.
        /// </summary>
        public BinErrStat DeepClone()
        {
            return new BinErrStat(this);
        }

    }//BinErrStat

}//Namespace
