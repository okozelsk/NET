using RCNet.MathTools;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace RCNet.Neural.Network.NonRecurrent
{
    /// <summary>
    /// Computation cluster of trained networks
    /// </summary>
    [Serializable]
    public class TrainedNetworkCluster
    {
        //Attribute properties
        /// <summary>
        /// Name of the cluster
        /// </summary>
        public string ClusterName { get; }
        /// <summary>
        /// If specified, it indicates that the whole network output is binary and specifies numeric border where GE network output is decided as a 1 and LT output as a 0.
        /// </summary>
        public double BinBorder { get; }
        /// <summary>
        /// Member networks
        /// </summary>
        public List<TrainedNetwork> Members { get; }
        /// <summary>
        /// Weights of cluster members
        /// </summary>
        public List<double> Weights { get; }
        /// <summary>
        /// Error statistics of the cluster
        /// </summary>
        public ClusterErrStatistics ErrorStats { get; }

        //Constructors
        /// <summary>
        /// Creates an uninitialized instance
        /// </summary>
        /// <param name="clusterName">Name of the cluster</param>
        /// <param name="numOfMembers">Number of trained networks in the cluster</param>
        /// <param name="binBorder">If specified, it indicates that the whole network output is binary and specifies numeric border where GE network output is decided as a 1 and LT output as a 0.</param>
        public TrainedNetworkCluster(string clusterName, int numOfMembers, double binBorder = double.NaN)
        {
            ClusterName = clusterName;
            BinBorder = binBorder;
            Members = new List<TrainedNetwork>(numOfMembers);
            Weights = new List<double>(numOfMembers);
            ErrorStats = new ClusterErrStatistics(ClusterName, numOfMembers, BinBorder);
            return;
        }

        /// <summary>
        /// Copy constructor
        /// </summary>
        /// <param name="source">Source cluster</param>
        public TrainedNetworkCluster(TrainedNetworkCluster source)
        {
            ClusterName = source.ClusterName;
            BinBorder = source.BinBorder;
            Members = new List<TrainedNetwork>(source.Members.Count);
            foreach(TrainedNetwork tn in source.Members)
            {
                Members.Add(tn.DeepClone());
            }
            Weights = new List<double>(source.Weights);
            ErrorStats = source.ErrorStats.DeepClone();
        }

        //Properties
        /// <summary>
        /// Indicates that the whole network output is binary
        /// </summary>
        public bool BinaryOutput { get { return !double.IsNaN(BinBorder); } }

        //Methods
        /// <summary>
        /// Computes weighted averaged output of the cluster member networks
        /// </summary>
        /// <param name="predictors">Input predictors</param>
        /// <param name="memberOutputCollection">Collection of outputs of cluster member networks</param>
        public double[] Compute(double[] predictors, out List<double[]> memberOutputCollection)
        {
            int numOfOutputValues = Members[0].Network.NumOfOutputValues;
            WeightedAvg[] weightedResultCollection = new WeightedAvg[numOfOutputValues];
            for(int i = 0; i < numOfOutputValues; i++)
            {
                weightedResultCollection[i] = new WeightedAvg();
            }
            //Init member output collection
            memberOutputCollection = new List<double[]>(Members.Count);
            //Loop cluster members
            int clusterMemberIdx = 0;
            foreach (TrainedNetwork clusterMember in Members)
            {
                //Compute member
                double[] computedValues = clusterMember.Network.Compute(predictors);
                //Store sub-results
                memberOutputCollection.Add(computedValues);
                //Add sub-results to weighted averages
                for (int i = 0; i < numOfOutputValues; i++)
                {
                    weightedResultCollection[i].AddSampleValue(computedValues[i], Weights[clusterMemberIdx]);
                }
                ++clusterMemberIdx;
            }
            //Return weighted average results
            double[] outputValues = new double[numOfOutputValues];
            for(int i = 0; i < numOfOutputValues; i++)
            {
                outputValues[i] = weightedResultCollection[i].Avg;
            }
            return outputValues;
        }

        /// <summary>
        /// Creates the deep copy of this instance
        /// </summary>
        public TrainedNetworkCluster DeepClone()
        {
            return new TrainedNetworkCluster(this);
        }


        //Inner classes
        /// <summary>
        /// Overall error statistics of the cluster of readout units
        /// </summary>
        [Serializable]
        public class ClusterErrStatistics
        {
            //Property attributes
            /// <summary>
            /// Name of the cluster
            /// </summary>
            public string ClusterName { get; }
            /// <summary>
            /// If specified, it indicates that the whole network output is binary and specifies numeric border where GE network output is decided as a 1 and LT output as a 0.
            /// </summary>
            public double BinBorder { get; }
            /// <summary>
            /// Number of computing networks within the cluster
            /// </summary>
            public int NumOfMembers { get; }
            /// <summary>
            /// Error statistics of the distance between computed and ideal valus in natural form
            /// </summary>
            public BasicStat NatPrecissionErrStat { get; }
            /// <summary>
            /// Error statistics of the distance between computed and ideal valus in normalized form
            /// </summary>
            public BasicStat NrmPrecissionErrStat { get; }
            /// <summary>
            /// Statistics of the binary errors.
            /// Relevant only when the network output is binary.
            /// </summary>
            public BinErrStat BinaryErrStat { get; }

            /// <summary>
            /// Constructs an instance prepared for initialization (updates)
            /// </summary>
            /// <param name="clusterName">Name of the cluster (and also clusterred networks)</param>
            /// <param name="numOfMembers">Number of computing networks within the cluster</param>
            /// <param name="binBorder">Binary 0/1 border. Double value LT this border is considered as 0 and GE as 1.
            /// (relevant only if task type is Classification)
            /// </param>
            public ClusterErrStatistics(string clusterName, int numOfMembers, double binBorder = double.NaN)
            {
                ClusterName = clusterName;
                BinBorder = binBorder;
                NumOfMembers = numOfMembers;
                NatPrecissionErrStat = new BasicStat();
                NrmPrecissionErrStat = new BasicStat();
                BinaryErrStat = null;
                if (BinaryOutput)
                {
                    BinaryErrStat = new BinErrStat(BinBorder);
                }
                return;
            }

            /// <summary>
            /// A deep copy constructor
            /// </summary>
            /// <param name="source">Source instance</param>
            public ClusterErrStatistics(ClusterErrStatistics source)
            {
                ClusterName = source.ClusterName;
                BinBorder = source.BinBorder;
                NumOfMembers = source.NumOfMembers;
                NatPrecissionErrStat = new BasicStat(source.NatPrecissionErrStat);
                NrmPrecissionErrStat = new BasicStat(source.NrmPrecissionErrStat);
                BinaryErrStat = null;
                BinaryErrStat = source.BinaryErrStat?.DeepClone();
                return;
            }

            //Properties
            /// <summary>
            /// Indicates that the whole network output is binary
            /// </summary>
            public bool BinaryOutput { get { return !double.IsNaN(BinBorder); } }

            /// <summary>
            /// Updates cluster statistics
            /// </summary>
            /// <param name="nrmComputedValue">Normalized value computed by the cluster</param>
            /// <param name="nrmIdealValue">Normalized ideal value</param>
            /// <param name="natComputedValue">Naturalized value computed by the cluster</param>
            /// <param name="natIdealValue">Naturalized ideal value</param>
            public void Update(double nrmComputedValue, double nrmIdealValue, double natComputedValue, double natIdealValue)
            {
                NatPrecissionErrStat.AddSampleValue(Math.Abs(natComputedValue - natIdealValue));
                NrmPrecissionErrStat.AddSampleValue(Math.Abs(nrmComputedValue - nrmIdealValue));
                if (BinaryOutput)
                {
                    BinaryErrStat.Update(nrmComputedValue, nrmIdealValue);
                }
                return;
            }

            /// <summary>
            /// Creates a deep copy instance of this instance
            /// </summary>
            public ClusterErrStatistics DeepClone()
            {
                return new ClusterErrStatistics(this);
            }

        }//ClusterErrStatistics


    }//TrainedNetworkCluster

}//Namespace
