using System;
using System.Collections.Generic;
using RCNet.Extensions;
using RCNet.MathTools;
using RCNet.MathTools.Probability;
using RCNet.Neural.Data;
using RCNet.Neural.Data.Filter;

namespace RCNet.Neural.Network.NonRecurrent
{
    /// <summary>
    /// Computation cluster of trained "One Takes All" networks.
    /// </summary>
    [Serializable]
    public class TrainedOneTakesAllNetworkCluster
    {

        //Constants
        //Macro-weights
        //Measures group weights
        private const double TrainGroupWeight = 1d;
        private const double TestGroupWeight = 1d;
        //Measures weights
        private const double SamplesWeight = 1d;
        private const double PrecisionWeight = 1d;
        private const double MisrecognizedWeight = 1d;
        private const double UnrecognizedWeight = 1d;

        //Attribute properties
        /// <summary>
        /// Name of the cluster
        /// </summary>
        public string ClusterName { get; }

        /// <summary>
        /// Indicates the cluster is finalized and ready
        /// </summary>
        public bool Finalized { get; private set; }

        //Attributes
        /// <summary>
        /// Member networks
        /// </summary>
        private readonly List<TrainedOneTakesAllNetwork> _memberNetCollection;

        /// <summary>
        /// First level weights
        /// </summary>
        private double[] _membersWeights;

        //Constructors
        /// <summary>
        /// Creates an uninitialized instance
        /// </summary>
        /// <param name="clusterName">Name of the cluster</param>
        public TrainedOneTakesAllNetworkCluster(string clusterName)
        {
            ClusterName = clusterName;
            Finalized = false;
            _memberNetCollection = new List<TrainedOneTakesAllNetwork>();
            _membersWeights = null;
            return;
        }

        /// <summary>
        /// Copy constructor
        /// </summary>
        /// <param name="source">Source cluster</param>
        public TrainedOneTakesAllNetworkCluster(TrainedOneTakesAllNetworkCluster source)
        {
            ClusterName = source.ClusterName;
            Finalized = source.Finalized;
            _memberNetCollection = new List<TrainedOneTakesAllNetwork>(source._memberNetCollection.Count);
            foreach (TrainedOneTakesAllNetwork tn in source._memberNetCollection)
            {
                _memberNetCollection.Add(tn.DeepClone());
            }
            _membersWeights = (double[])source._membersWeights?.Clone();
            return;
        }

        //Properties
        /// <summary>
        /// Number of member networks
        /// </summary>
        public int NumOfMembers { get { return _memberNetCollection.Count; } }

        //Methods
        /// <summary>
        /// Adds new member network and updates cluster error statistics
        /// </summary>
        /// <param name="memberNet">New member network</param>
        /// <param name="testData">Testing data</param>
        public void AddMember(TrainedOneTakesAllNetwork memberNet, VectorBundle testData)
        {
            //Add member to inner collection
            _memberNetCollection.Add(memberNet);
            return;
        }

        /// <summary>
        /// Computes weights of given trained networks
        /// </summary>
        private double[] ComputeTrainedNetworksWeights(List<TrainedOneTakesAllNetwork> trainedNetworks)
        {
            double[] weightCollection = new double[trainedNetworks.Count];
            if (weightCollection.Length == 1)
            {
                weightCollection[0] = 1d;
            }
            else
            {
                //Holders for metrics from member's training and testing phases
                //Training
                double[] wHolderTrainSamples = new double[trainedNetworks.Count];
                double[] wHolderTrainPrecision = new double[trainedNetworks.Count];
                double[] wHolderTrainMisrecognized = new double[trainedNetworks.Count];
                double[] wHolderTrainUnrecognized = new double[trainedNetworks.Count];
                //Testing
                double[] wHolderTestSamples = new double[trainedNetworks.Count];
                double[] wHolderTestPrecision = new double[trainedNetworks.Count];
                double[] wHolderTestMisrecognized = new double[trainedNetworks.Count];
                double[] wHolderTestUnrecognized = new double[trainedNetworks.Count];
                //Collect members' metrics
                for (int memberIdx = 0; memberIdx < trainedNetworks.Count; memberIdx++)
                {
                    wHolderTrainSamples[memberIdx] = trainedNetworks[memberIdx].TrainingErrorStat.NumOfSamples;
                    wHolderTrainPrecision[memberIdx] = trainedNetworks[memberIdx].TrainingErrorStat.ArithAvg;
                    wHolderTrainMisrecognized[memberIdx] = (1d - trainedNetworks[memberIdx].TrainingBinErrorStat.BinValErrStat[0].ArithAvg);
                    wHolderTrainUnrecognized[memberIdx] = (1d - trainedNetworks[memberIdx].TrainingBinErrorStat.BinValErrStat[1].ArithAvg);
                    wHolderTestSamples[memberIdx] = trainedNetworks[memberIdx].TestingErrorStat.NumOfSamples;
                    wHolderTestPrecision[memberIdx] = trainedNetworks[memberIdx].TestingErrorStat.ArithAvg;
                    wHolderTestMisrecognized[memberIdx] = (1d - trainedNetworks[memberIdx].TestingBinErrorStat.BinValErrStat[0].ArithAvg);
                    wHolderTestUnrecognized[memberIdx] = (1d - trainedNetworks[memberIdx].TestingBinErrorStat.BinValErrStat[1].ArithAvg);
                }
                //Turn the metrics to have the same meaning and scale them to be useable as the sub-weights
                wHolderTrainSamples.ScaleToNewSum(1d);
                wHolderTestSamples.ScaleToNewSum(1d);
                wHolderTrainPrecision.RevertMeaning();
                wHolderTrainPrecision.ScaleToNewSum(1d);
                wHolderTestPrecision.RevertMeaning();
                wHolderTestPrecision.ScaleToNewSum(1d);
                wHolderTrainMisrecognized.ScaleToNewSum(1d);
                wHolderTestMisrecognized.ScaleToNewSum(1d);
                wHolderTrainUnrecognized.ScaleToNewSum(1d);
                wHolderTestUnrecognized.ScaleToNewSum(1d);
                //Build the final weights
                //Combine the sub-weights using defined macro weights of the metrics
                for (int i = 0; i < trainedNetworks.Count; i++)
                {
                    weightCollection[i] = TrainGroupWeight * (SamplesWeight * wHolderTrainSamples[i] +
                                                              PrecisionWeight * wHolderTrainPrecision[i] +
                                                              MisrecognizedWeight * wHolderTrainMisrecognized[i] +
                                                              UnrecognizedWeight * wHolderTrainUnrecognized[i]
                                                              ) +
                                          TestGroupWeight * (SamplesWeight * wHolderTestSamples[i] +
                                                             PrecisionWeight * wHolderTestPrecision[i] +
                                                             MisrecognizedWeight * wHolderTestMisrecognized[i] +
                                                             UnrecognizedWeight * wHolderTestUnrecognized[i]
                                                             );
                }
                //Softmax transformation
                weightCollection.Softmax();
            }
            return weightCollection;
        }

        /// <summary>
        /// Computes the composite probabilistic result
        /// </summary>
        private double[] ComputeCompositeOutput(List<double[]> membersResults)
        {
            int numOfOutputProbabilities = membersResults[0].Length;
            double[] outProbabilities = new double[numOfOutputProbabilities];
            for(int pIdx = 0; pIdx < numOfOutputProbabilities; pIdx++)
            {
                double[] memberPs = new double[NumOfMembers];
                for(int i = 0; i < NumOfMembers; i++)
                {
                    memberPs[i] = membersResults[i][pIdx];
                }
                //Compute members mixed probability
                outProbabilities[pIdx] = PMixer.MixP(memberPs, _membersWeights);
            }
            //outProbabilities.Softmax();
            outProbabilities.ScaleToNewSum(1d);
            return outProbabilities;
        }

        /// <summary>
        /// Computes and returns probabilistic outputs of all cluster member networks
        /// </summary>
        private List<double[]> ComputeClusterMemberNetworks(double[] inputVector)
        {
            List<double[]> outputVectors = new List<double[]>(NumOfMembers);
            for (int memberIdx = 0; memberIdx < NumOfMembers; memberIdx++)
            {
                outputVectors.Add(_memberNetCollection[memberIdx].Network.Compute(inputVector));
            }
            return outputVectors;
        }

        /// <summary>
        /// Makes cluster ready to operate
        /// </summary>
        /// <param name="dataBundle">Whole data used for training of inner members</param>
        public void FinalizeCluster(VectorBundle dataBundle)
        {
            if (Finalized)
            {
                throw new InvalidOperationException("Cluster was already finalized.");
            }
            //Initialize the members' weights
            _membersWeights = ComputeTrainedNetworksWeights(_memberNetCollection);
            //When necessary, prepare the second level networks
            Finalized = true;
            return;
        }

        /// <summary>
        /// Computes the cluster probabilistic output
        /// </summary>
        /// <param name="predictors">Input predictors</param>
        public double[] Compute(double[] predictors)
        {
            if (!Finalized)
            {
                throw new InvalidOperationException("Cluster is not finalized. Call FinalizeCluster method first.");
            }
            //Collect member networks outputs
            List<double[]> memberOutputs = ComputeClusterMemberNetworks(predictors);
            //Compute the probabilistic result
            double[] output = ComputeCompositeOutput(memberOutputs);
            return output;
        }

        /// <summary>
        /// Creates the deep copy of this instance
        /// </summary>
        public TrainedOneTakesAllNetworkCluster DeepClone()
        {
            return new TrainedOneTakesAllNetworkCluster(this);
        }

    }//TrainedOneTakesAllNetworkCluster

}//Namespace
