using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using RCNet.MathTools;
using RCNet.Neural.Network.FF;

namespace RCNet.Neural.Network.RCReadout
{
    /// <summary>
    /// Contains the feed forward network associated with output field and related
    /// important error statistics.
    /// </summary>
    [Serializable]
    public class ReadoutUnit
    {
        //Attribute properties
        /// <summary>
        /// Output field name for which the regression was performed
        /// </summary>
        public string OutputFieldName;
        /// <summary>
        /// Trained feed forward network
        /// </summary>
        public FeedForwardNetwork FFNet { get; set; }
        /// <summary>
        /// Training error statistics
        /// </summary>
        public BasicStat TrainingErrorStat { get; set; }
        /// <summary>
        /// Training binary error statistics
        /// </summary>
        public BinErrStat TrainingBinErrorStat { get; set; }
        /// <summary>
        /// Testing error statistics
        /// </summary>
        public BasicStat TestingErrorStat { get; set; }
        /// <summary>
        /// Testing binary error statistics
        /// </summary>
        public BinErrStat TestingBinErrorStat { get; set; }
        /// <summary>
        /// Statistics of the FF network weights
        /// </summary>
        public BasicStat OutputWeightsStat { get; set; }
        /// <summary>
        /// Achieved combined error.
        /// Formula for combined error calculation is Max(training error, testing error)
        /// </summary>
        public double CombinedError { get; set; }

        //Constructor
        /// <summary>
        /// Creates an unitialized instance
        /// </summary>
        public ReadoutUnit()
        {
            OutputFieldName = string.Empty;
            FFNet = null;
            TrainingErrorStat = null;
            TrainingBinErrorStat = null;
            TestingErrorStat = null;
            TestingBinErrorStat = null;
            OutputWeightsStat = null;
            CombinedError = -1;
            return;
        }

        /// <summary>
        /// The deep copy constructor.
        /// </summary>
        /// <param name="source">Source instance</param>
        public ReadoutUnit(ReadoutUnit source)
        {
            OutputFieldName = source.OutputFieldName;
            FFNet = null;
            if (source.FFNet != null)
            {
                FFNet = source.FFNet.Clone();
            }
            TrainingErrorStat = null;
            if (source.TrainingErrorStat != null)
            {
                TrainingErrorStat = new BasicStat(source.TrainingErrorStat);
            }
            TrainingBinErrorStat = null;
            if (source.TrainingBinErrorStat != null)
            {
                TrainingBinErrorStat = new BinErrStat(source.TrainingBinErrorStat);
            }
            TestingErrorStat = null;
            if (source.TestingErrorStat != null)
            {
                TestingErrorStat = new BasicStat(source.TestingErrorStat);
            }
            TestingBinErrorStat = null;
            if (source.TestingBinErrorStat != null)
            {
                TestingBinErrorStat = new BinErrStat(source.TestingBinErrorStat);
            }
            OutputWeightsStat = null;
            if (source.OutputWeightsStat != null)
            {
                OutputWeightsStat = new BasicStat(source.OutputWeightsStat);
            }
            CombinedError = source.CombinedError;
            return;
        }

        //Methods
        /// <summary>
        /// Creates the deep copy instance of this instance
        /// </summary>
        public ReadoutUnit DeepClone()
        {
            return new ReadoutUnit(this);
        }

    }//ReadoutUnit
}//Namespace
