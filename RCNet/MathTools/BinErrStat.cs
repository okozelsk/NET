using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace RCNet.MathTools
{
    /// <summary>
    /// Error statistics of binary 0/1 values
    /// </summary>
    [Serializable]
    public class BinErrStat
    {
        //Constants
        private const double MaxErr = 1E9;

        //Attribute properties
        /// <summary>
        /// Reference distribution of 0/1 values
        /// </summary>
        public BinDistribution RefBinValDistr { get; set; }
        /// <summary>
        /// This distribution of 0/1 values (from ideal)
        /// </summary>
        public BinDistribution ThisBinValDistr { get; set; }
        /// <summary>
        /// Statistics of errors on individual 0/1 values
        /// </summary>
        public BasicStat[] BinValErrStat { get; set; }
        /// <summary>
        /// Total error statistics
        /// </summary>
        public BasicStat TotalErrStat { get; set; }

        //Properties
        /// <summary>
        /// Proportional error based on bin values distributions and error rates.
        /// </summary>
        public double ProportionalErr
        {
            get
            {
                double err = 0;
                for (int binVal = 0; binVal <= 1; binVal++)
                {
                    double proportionCoeff = MaxErr;
                    if (ThisBinValDistr.BinRate[binVal] > 0)
                    {
                        proportionCoeff = RefBinValDistr.BinRate[binVal] / ThisBinValDistr.BinRate[binVal];
                    }
                    double errRate = 1;
                    if (ThisBinValDistr.NumOf[binVal] > 0)
                    {
                        errRate = BinValErrStat[binVal].Sum / BinValErrStat[binVal].NumOfSamples;
                    }
                    err += proportionCoeff * errRate;
                }
                return err;
            }
        }


        //Constructors
        /// <summary>
        /// Creates an uninitialized instance
        /// </summary>
        /// <param name="refBinValDistr">Reference distribution of 0/1 values</param>
        public BinErrStat(BinDistribution refBinValDistr)
        {
            RefBinValDistr = refBinValDistr.DeepClone();
            ThisBinValDistr = new BinDistribution(RefBinValDistr.BinBorder);
            BinValErrStat = new BasicStat[2];
            BinValErrStat[0] = new BasicStat();
            BinValErrStat[1] = new BasicStat();
            TotalErrStat = new BasicStat();
            return;
        }

        /// <summary>
        /// Construct an initialized instance
        /// </summary>
        /// <param name="refBinValDistr">Reference distribution of 0/1 values</param>
        /// <param name="computedVectorCollection">Collection of computed vectors</param>
        /// <param name="idealVectorCollection">Collection of ideal vectors</param>
        /// <param name="valueIdx">Index of a binary value within the vector</param>
        public BinErrStat(BinDistribution refBinValDistr, IEnumerable<double[]> computedVectorCollection, IEnumerable<double[]> idealVectorCollection, int valueIdx = 0)
            :this(refBinValDistr)
        {
            Update(computedVectorCollection, idealVectorCollection, valueIdx);
            return;
        }

        /// <summary>
        /// Construct an initialized instance
        /// </summary>
        /// <param name="refBinValDistr">Reference distribution of 0/1 values</param>
        /// <param name="computedValueCollection">Collection of computed vectors</param>
        /// <param name="idealValueCollection">Collection of ideal vectors</param>
        public BinErrStat(BinDistribution refBinValDistr, IEnumerable<double> computedValueCollection, IEnumerable<double> idealValueCollection)
            : this(refBinValDistr)
        {
            Update(computedValueCollection, idealValueCollection);
            return;
        }

        /// <summary>
        /// Deep copy constructor
        /// </summary>
        /// <param name="source">Source instance</param>
        public BinErrStat(BinErrStat source)
        {
            RefBinValDistr = source.RefBinValDistr.DeepClone();
            ThisBinValDistr = source.ThisBinValDistr.DeepClone();
            BinValErrStat = new BasicStat[2];
            BinValErrStat[0] = new BasicStat(source.BinValErrStat[0]);
            BinValErrStat[1] = new BasicStat(source.BinValErrStat[1]);
            TotalErrStat = new BasicStat(source.TotalErrStat);
            return;
        }

        //Methods
        /// <summary>
        /// Compares if two values represent are the same binary value
        /// </summary>
        /// <param name="computedValue">Computed binary value</param>
        /// <param name="idealValue">Ideal binary value</param>
        /// <returns></returns>
        private bool BinMatch(double computedValue, double idealValue)
        {
            if (computedValue >= RefBinValDistr.BinBorder && idealValue >= RefBinValDistr.BinBorder) return true;
            if (computedValue < RefBinValDistr.BinBorder && idealValue < RefBinValDistr.BinBorder) return true;
            return false;
        }

        /// <summary>
        /// Updates the statistics
        /// </summary>
        /// <param name="computedValue">Computed binary value</param>
        /// <param name="idealValue">Ideal binary value</param>
        public void Update(double computedValue, double idealValue)
        {
            ThisBinValDistr.Update(idealValue);
            int idealBinVal = (idealValue >= ThisBinValDistr.BinBorder) ? 1 : 0;
            int errValue = BinMatch(computedValue, idealValue) ? 0 : 1;
            BinValErrStat[idealBinVal].AddSampleValue(errValue);
            TotalErrStat.AddSampleValue(errValue);
            return;
        }

        /// <summary>
        /// Updates the statistics
        /// </summary>
        /// <param name="computedVectorCollection">Collection of computed vectors</param>
        /// <param name="idealVectorCollection">Collection of ideal vectors</param>
        /// <param name="valueIdx">Index of a binary value within the vectors</param>
        public void Update(IEnumerable<double[]> computedVectorCollection, IEnumerable<double[]> idealVectorCollection, int valueIdx = 0)
        {
            IEnumerator<double[]> idealVectorEnumerator = idealVectorCollection.GetEnumerator();
            foreach (double[] computedVector in computedVectorCollection)
            {
                idealVectorEnumerator.MoveNext();
                double[] idealVector = idealVectorEnumerator.Current;
                Update(computedVector[valueIdx], idealVector[valueIdx]);
            }
            return;
        }

        /// <summary>
        /// Updates the statistics
        /// </summary>
        /// <param name="computedValueCollection">Collection of computed binary values</param>
        /// <param name="idealValueCollection">Collection of ideal binary values</param>
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

    }//BinErrStat

}//Namespace
