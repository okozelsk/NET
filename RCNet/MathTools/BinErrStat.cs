using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace RCNet.MathTools
{
    /// <summary>
    /// Simple error statistics of binary values (0/1)
    /// </summary>
    [Serializable]
    public class BinErrStat
    {
        //Attributes
        /// <summary>
        /// Binary 0/1 border
        /// </summary>
        private double _binBorder;
        //Attribute properties
        /// <summary>
        /// Total number of compared binary values.
        /// </summary>
        public int TotalNumOfBinValues { get; set; }
        /// <summary>
        /// Number of recognized binary 1 values in ideals
        /// </summary>
        public int NumOfBin1Values { get; set; }
        /// <summary>
        /// Number of errors on recognized binary 1 values
        /// </summary>
        public int NumOfBin1Errors { get; set; }
        /// <summary>
        /// Total number of binary errors
        /// </summary>
        public int TotalNumOfBinErrors { get; set; }
        /// <summary>
        /// BasicStat of binary errors
        /// </summary>
        public BasicStat ErrStat { get; set; }
        //Properties
        /// <summary>
        /// Number of recognized binary 0 values in ideals
        /// </summary>
        public int NumOfBin0Values { get { return TotalNumOfBinValues - NumOfBin1Values; } }
        /// <summary>
        /// Number of errors on recognized binary 0 values
        /// </summary>
        public int NumOfBin0Errors { get { return TotalNumOfBinErrors - NumOfBin1Errors; } }

        //Constructors
        /// <summary>
        /// Construct an initialized instance
        /// </summary>
        /// <param name="binBorder">Double value LT this border is considered 0 and GE 1</param>
        /// <param name="valuesCollection">Collection of values to be compared with ideals</param>
        /// <param name="idealsCollection">Collection of ideals</param>
        public BinErrStat(double binBorder, IEnumerable<double[]> valuesCollection, IEnumerable<double[]> idealsCollection)
        {
            _binBorder = binBorder;
            ErrStat = new BasicStat();
            //Evaluation
            IEnumerator<double[]> idealsEnumerator = idealsCollection.GetEnumerator();
            foreach (double[] values in valuesCollection)
            {
                idealsEnumerator.MoveNext();
                double[] ideals = idealsEnumerator.Current;
                for(int i = 0; i < values.Length; i++)
                {
                    ++TotalNumOfBinValues;
                    if (ideals[i] >= _binBorder) ++NumOfBin1Values;
                    if(!BinMatch(values[i], ideals[i]))
                    {
                        //Error
                        ++TotalNumOfBinErrors;
                        if (ideals[i] >= _binBorder) ++NumOfBin1Errors;
                        ErrStat.AddSampleValue(1);
                    }
                    else
                    {
                        //OK
                        ErrStat.AddSampleValue(0);
                    }
                }
            }
            return;
        }

        /// <summary>
        /// Deep copy constructor
        /// </summary>
        /// <param name="source">Source instance</param>
        public BinErrStat(BinErrStat source)
        {
            _binBorder = source._binBorder;
            TotalNumOfBinValues = source.TotalNumOfBinValues;
            NumOfBin1Values = source.NumOfBin1Values;
            NumOfBin1Errors = source.NumOfBin1Errors;
            TotalNumOfBinErrors = source.TotalNumOfBinErrors;
            ErrStat = new BasicStat(source.ErrStat);
            return;
        }

        //Methods
        private bool BinMatch(double value, double idealValue)
        {
            if (value >= _binBorder && idealValue >= _binBorder) return true;
            if (value < _binBorder && idealValue < _binBorder) return true;
            return false;
        }


    }//BinErrStat

}//Namespace
