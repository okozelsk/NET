using RCNet.Extensions;
using RCNet.MathTools;
using RCNet.MathTools.Probability;
using System;
using System.Collections.Generic;

namespace RCNet.Neural.Network.NonRecurrent
{
    /// <summary>
    /// Computation cluster of trained networks.
    /// Supported are only single output networks.
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
        /// Range of input and output data
        /// </summary>
        public Interval DataRange { get; }
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
        /// <param name="numOfMembers">Expected number of trained networks in the cluster</param>
        /// <param name="dataRange">Range of input and output data</param>
        /// <param name="binBorder">If specified, it indicates that the whole network output is binary and specifies numeric border where GE network output is decided as a 1 and LT output as a 0.</param>
        public TrainedNetworkCluster(string clusterName,
                                     int numOfMembers,
                                     Interval dataRange,
                                     double binBorder = double.NaN
                                     )
        {
            ClusterName = clusterName;
            BinBorder = binBorder;
            DataRange = dataRange.DeepClone();
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
            foreach (TrainedNetwork tn in source.Members)
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
        public double Compute(double[] predictors, out double[] memberOutputCollection)
        {
            //Init member output collection
            memberOutputCollection = new double[Members.Count];
            if (!BinaryOutput)
            {
                //Result is exact value
                WeightedAvg weightedResult = new WeightedAvg();
                //Loop cluster members
                int clusterMemberIdx = 0;
                foreach (TrainedNetwork clusterMember in Members)
                {
                    //Compute member
                    double computedValue = clusterMember.Network.Compute(predictors)[0];
                    //Store sub-results
                    memberOutputCollection[clusterMemberIdx] = computedValue;
                    //Add sub-result to weighted average
                    weightedResult.AddSampleValue(computedValue, Weights[clusterMemberIdx]);
                    ++clusterMemberIdx;
                }
                //Return weighted average result
                return weightedResult.Avg;
            }
            else
            {
                //Result is a probability -> use probability mixer
                double[] probabilities = new double[Members.Count];
                double[] weights = new double[Members.Count];
                for (int i = 0; i < Members.Count; i++)
                {
                    memberOutputCollection[i] = Members[i].Network.Compute(predictors)[0];
                    probabilities[i] = PMixer.ProbabilityRange.Rescale(memberOutputCollection[i], DataRange);
                    weights[i] = Weights[i];
                }
                //Scale weights to ensure their sum is equal to 1
                weights.Scale(1d / weights.Sum());
                //Return resulting mixed probability rescalled back to members' result range
                return DataRange.Rescale(PMixer.MixP(probabilities, weights), PMixer.ProbabilityRange);
            }
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
