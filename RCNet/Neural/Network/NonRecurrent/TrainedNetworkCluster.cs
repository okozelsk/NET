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
    /// Computation cluster of trained networks.
    /// Supported are only single output member networks.
    /// </summary>
    [Serializable]
    public class TrainedNetworkCluster
    {
        /// <summary>
        /// Second level computation modes
        /// </summary>
        public enum SecondLevelCompMode
        {
            /// <summary>
            /// Used is only output based on 2nd level networks.
            /// </summary>
            SecondLevelOutputOnly,
            /// <summary>
            /// Used is average value of the first level output and the second level output.
            /// </summary>
            AveragedOutputs
        }

        //Constants
        //Macro-weights
        //Measures group weights
        private const double TrainGroupWeight = 1d;
        private const double TestGroupWeight = 1d;
        //Measures weights
        private const double SamplesWeight = 1d;
        private const double PrecisionWeight = 1d;
        private const double MisrecognizedWeight = 1d;
        private const double UnrecognizedWeight = 0d;

        //Switches
        private const bool Join1stLevelOutputTo2ndLevelInput = true;

        //Events
        /// <summary>
        /// This informative event occurs every time the regression epoch is done
        /// </summary>
        [field: NonSerialized]
        public event TrainedNetworkBuilder.RegressionEpochDoneHandler RegressionEpochDone;

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
        /// Error statistics of the cluster
        /// </summary>
        public ClusterErrStatistics ErrorStats { get; }

        /// <summary>
        /// Indicates the cluster is finalized and ready
        /// </summary>
        public bool Finalized { get; private set; }

        //Attributes
        /// <summary>
        /// Member networks
        /// </summary>
        private readonly List<TrainedNetwork> _memberNetCollection;

        /// <summary>
        /// First level weights
        /// </summary>
        private double[] _firstLevelWeights;

        /// <summary>
        /// Second level networks and weights
        /// </summary>
        private readonly NetworkClusterSecondLevelCompSettings _secondLevelCompCfg;
        private readonly List<TrainedNetwork> _secondLevelNetCollection;
        private double[] _secondLevelWeights;

        //Constructors
        /// <summary>
        /// Creates an uninitialized instance
        /// </summary>
        /// <param name="clusterName">Name of the cluster</param>
        /// <param name="dataRange">Range of input and output data</param>
        /// <param name="binBorder">If specified, it indicates that the whole network output is binary and specifies numeric border where GE network output is decided as a 1 and LT output as a 0.</param>
        /// <param name="secondLevelCompCfg">Configuration of the network cluster 2nd level computation</param>
        public TrainedNetworkCluster(string clusterName,
                                     Interval dataRange,
                                     double binBorder = double.NaN,
                                     NetworkClusterSecondLevelCompSettings secondLevelCompCfg = null
                                     )
        {
            ClusterName = clusterName;
            BinBorder = binBorder;
            DataRange = dataRange.DeepClone();
            ErrorStats = new ClusterErrStatistics(ClusterName, BinBorder);
            Finalized = false;
            _memberNetCollection = new List<TrainedNetwork>();
            _firstLevelWeights = null;
            _secondLevelCompCfg = (NetworkClusterSecondLevelCompSettings)secondLevelCompCfg?.DeepClone();
            _secondLevelNetCollection = new List<TrainedNetwork>();
            _secondLevelWeights = null;
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
            DataRange = source.DataRange.DeepClone();
            ErrorStats = source.ErrorStats.DeepClone();
            Finalized = source.Finalized;
            _memberNetCollection = new List<TrainedNetwork>(source._memberNetCollection.Count);
            foreach (TrainedNetwork tn in source._memberNetCollection)
            {
                _memberNetCollection.Add(tn.DeepClone());
            }
            _firstLevelWeights = (double[])source._firstLevelWeights?.Clone();
            _secondLevelCompCfg = (NetworkClusterSecondLevelCompSettings)source._secondLevelCompCfg?.DeepClone();
            _secondLevelNetCollection = new List<TrainedNetwork>(source._secondLevelNetCollection.Count);
            foreach (TrainedNetwork tn in source._secondLevelNetCollection)
            {
                _secondLevelNetCollection.Add(tn.DeepClone());
            }
            _secondLevelWeights = (double[])source._secondLevelWeights?.Clone();
            return;
        }

        //Properties
        /// <summary>
        /// Indicates that the whole network output is binary
        /// </summary>
        public bool BinaryOutput { get { return !double.IsNaN(BinBorder); } }

        /// <summary>
        /// Indicates the 2nd level computation
        /// </summary>
        public bool SecondLevelComputation { get { return _secondLevelCompCfg != null; } }

        /// <summary>
        /// Number of member networks
        /// </summary>
        public int NumOfMembers { get { return _memberNetCollection.Count; } }


        //Methods
        private void OnRegressionEpochDone(TrainedNetworkBuilder.BuildingState buildingState, bool foundBetter)
        {
            //Only raise up
            RegressionEpochDone(buildingState, foundBetter);
            return;
        }

        /// <summary>
        /// Adds new member network and updates cluster error statistics
        /// </summary>
        /// <param name="memberNet">New member network</param>
        /// <param name="testData">Testing data</param>
        /// <param name="filter">Filter to be used for denormalization</param>
        public void AddMember(TrainedNetwork memberNet, VectorBundle testData, FeatureFilterBase filter)
        {
            //Add member to inner collection
            _memberNetCollection.Add(memberNet);
            //Update cluster error statistics
            for (int sampleIdx = 0; sampleIdx < testData.OutputVectorCollection.Count; sampleIdx++)
            {
                double nrmComputedValue = memberNet.Network.Compute(testData.InputVectorCollection[sampleIdx])[0];
                double naturalComputedValue = filter.ApplyReverse(nrmComputedValue);
                double naturalIdealValue = filter.ApplyReverse(testData.OutputVectorCollection[sampleIdx][0]);
                ErrorStats.Update(nrmComputedValue,
                                  testData.OutputVectorCollection[sampleIdx][0],
                                  naturalComputedValue,
                                  naturalIdealValue
                                  );
            }//sampleIdx
            return;
        }

        /// <summary>
        /// Computes weights of given trained networks
        /// </summary>
        private double[] ComputeTrainedNetworksWeights(List<TrainedNetwork> trainedNetworks, bool probabilistic)
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
                    wHolderTrainMisrecognized[memberIdx] = probabilistic ? (1d - trainedNetworks[memberIdx].TrainingBinErrorStat.BinValErrStat[0].ArithAvg) : 1d;
                    wHolderTrainUnrecognized[memberIdx] = probabilistic ? (1d - trainedNetworks[memberIdx].TrainingBinErrorStat.BinValErrStat[1].ArithAvg) : 1d;
                    wHolderTestSamples[memberIdx] = trainedNetworks[memberIdx].TestingErrorStat.NumOfSamples;
                    wHolderTestPrecision[memberIdx] = trainedNetworks[memberIdx].TestingErrorStat.ArithAvg;
                    wHolderTestMisrecognized[memberIdx] = probabilistic ? (1d - trainedNetworks[memberIdx].TestingBinErrorStat.BinValErrStat[0].ArithAvg) : 1d;
                    wHolderTestUnrecognized[memberIdx] = probabilistic ? (1d - trainedNetworks[memberIdx].TestingBinErrorStat.BinValErrStat[1].ArithAvg) : 1d;
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
                                                              (probabilistic ? MisrecognizedWeight * wHolderTrainMisrecognized[i] : 0d) +
                                                              (probabilistic ? UnrecognizedWeight * wHolderTrainUnrecognized[i] : 0d)
                                                              ) +
                                          TestGroupWeight * (SamplesWeight * wHolderTestSamples[i] +
                                                             PrecisionWeight * wHolderTestPrecision[i] +
                                                             (probabilistic ? MisrecognizedWeight * wHolderTestMisrecognized[i] : 0d) +
                                                             (probabilistic ? UnrecognizedWeight * wHolderTestUnrecognized[i] : 0d)
                                                             );
                }
                //Softmax transformation
                weightCollection.Softmax();
            }
            return weightCollection;
        }

        /// <summary>
        /// Computes the composite result
        /// </summary>
        private double ComputeCompositeOutput(double[] values, double[] weights, bool probabilistic)
        {
            if (probabilistic)
            {
                //Compute mixed probability
                //Rescale probabilities
                double[] probabilities = new double[values.Length];
                for (int i = 0; i < values.Length; i++)
                {
                    probabilities[i] = PMixer.ProbabilityRange.Rescale(values[i], DataRange);
                }
                //Compute mixed probability and return back-rescaled value
                return DataRange.Rescale(PMixer.MixP(probabilities, weights), PMixer.ProbabilityRange);
            }
            else
            {
                //Compute simple weighted average of member results
                WeightedAvg wAvg = new WeightedAvg();
                for (int i = 0; i < values.Length; i++)
                {
                    //Add sub-result to weighted average
                    wAvg.AddSampleValue(values[i], weights[i]);
                }
                //Return weighted average value
                return wAvg.Avg;
            }
        }

        /// <summary>
        /// Computes and returns outputs of all cluster member networks
        /// </summary>
        private double[] ComputeClusterMemberNetworks(double[] inputVector)
        {
            double[] outputVector = new double[NumOfMembers];
            for (int memberIdx = 0; memberIdx < NumOfMembers; memberIdx++)
            {
                outputVector[memberIdx] = _memberNetCollection[memberIdx].Network.Compute(inputVector)[0];
            }
            return outputVector;
        }

        private double[] BuildSecondLevelInputVector(double[] memberResults, double firstLevelOutput)
        {
            double[] inputVector = new double[memberResults.Length + (Join1stLevelOutputTo2ndLevelInput ? 1 : 0)];
            memberResults.CopyTo(inputVector, 0);
            if(Join1stLevelOutputTo2ndLevelInput)
            {
                inputVector[memberResults.Length] = firstLevelOutput;
            }
            return inputVector;
        }

        /// <summary>
        /// Initialized second level networks
        /// </summary>
        private void InitSecondLevelNetworks(VectorBundle dataBundle)
        {
            //Process data by cluster networks and prepare decision training bundle
            VectorBundle shuffledDataBundle = new VectorBundle(dataBundle.InputVectorCollection.Count);
            for (int sampleIdx = 0; sampleIdx < dataBundle.InputVectorCollection.Count; sampleIdx++)
            {
                double[] memberResults = ComputeClusterMemberNetworks(dataBundle.InputVectorCollection[sampleIdx]);
                double[] inputVector = BuildSecondLevelInputVector(memberResults, ComputeCompositeOutput(memberResults, _firstLevelWeights, BinaryOutput));
                double[] outputVector = new double[1];
                outputVector[0] = dataBundle.OutputVectorCollection[sampleIdx][0];
                shuffledDataBundle.AddPair(inputVector, outputVector);
            }
            Random random = new Random(0);
            for (int repetitionIdx = 0; repetitionIdx < _secondLevelCompCfg.CrossvalidationCfg.Repetitions; repetitionIdx++)
            {
                //Reshuffle the data
                shuffledDataBundle.Shuffle(random);
                //Split shuffled data into the folds
                List<VectorBundle> subBundleCollection = shuffledDataBundle.CreateFolds(_secondLevelCompCfg.CrossvalidationCfg.FoldDataRatio, BinBorder);
                int numOfFoldsToBeProcessed = Math.Min(_secondLevelCompCfg.CrossvalidationCfg.Folds <= 0 ? subBundleCollection.Count : _secondLevelCompCfg.CrossvalidationCfg.Folds, subBundleCollection.Count);
                //Build trained network for each fold
                for (int foldIdx = 0; foldIdx < numOfFoldsToBeProcessed; foldIdx++)
                {
                    //Prepare training data bundle
                    VectorBundle trainingData = new VectorBundle();
                    for (int bundleIdx = 0; bundleIdx < subBundleCollection.Count; bundleIdx++)
                    {
                        if (bundleIdx != foldIdx)
                        {
                            trainingData.Add(subBundleCollection[bundleIdx]);
                        }
                    }
                    //Initialize network builder
                    TrainedNetworkBuilder netBuilder = new TrainedNetworkBuilder(ClusterName + " - 2nd level net",
                                                                                 _secondLevelCompCfg.NetCfg,
                                                                                 (repetitionIdx * numOfFoldsToBeProcessed) + foldIdx + 1,
                                                                                 _secondLevelCompCfg.CrossvalidationCfg.Repetitions * numOfFoldsToBeProcessed,
                                                                                 1,
                                                                                 1,
                                                                                 trainingData,
                                                                                 subBundleCollection[foldIdx],
                                                                                 BinBorder,
                                                                                 random,
                                                                                 null
                                                                                 );
                    //Register notification
                    netBuilder.RegressionEpochDone += OnRegressionEpochDone;
                    //Add trained network into the holder
                    _secondLevelNetCollection.Add(netBuilder.Build());
                }
            }
            //Init second level networks weights
            _secondLevelWeights = ComputeTrainedNetworksWeights(_secondLevelNetCollection, BinaryOutput);
            return;
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
            //In all cases initialize the first level members' weights (softmax)
            _firstLevelWeights = ComputeTrainedNetworksWeights(_memberNetCollection, BinaryOutput);
            //When necessary, prepare the second level networks
            if (SecondLevelComputation)
            {
                InitSecondLevelNetworks(dataBundle);
            }
            Finalized = true;
            return;
        }

        /// <summary>
        /// Computes second level output
        /// </summary>
        private double ComputeSecondLevelOutput(double[] memberResults, double firstLevelOutput)
        {
            double[] inputVector = BuildSecondLevelInputVector(memberResults, firstLevelOutput);
            double[] secondLevelMemberOutputCollection = new double[_secondLevelNetCollection.Count];
            for (int i = 0; i < _secondLevelNetCollection.Count; i++)
            {
                secondLevelMemberOutputCollection[i] = _secondLevelNetCollection[i].Network.Compute(inputVector)[0];
            }
            return ComputeCompositeOutput(secondLevelMemberOutputCollection, _secondLevelWeights, true);
        }

        /// <summary>
        /// Computes cluster output
        /// </summary>
        /// <param name="predictors">Input predictors</param>
        /// <param name="memberOutputs">Collection of cluster member networks outputs</param>
        public double Compute(double[] predictors, out double[] memberOutputs)
        {
            if (!Finalized)
            {
                throw new InvalidOperationException("Cluster is not finalized. Call FinalizeCluster method first.");
            }
            //Collect member networks output
            memberOutputs = ComputeClusterMemberNetworks(predictors);
            //Compute first level result
            double firstLevelOutput = ComputeCompositeOutput(memberOutputs, _firstLevelWeights, BinaryOutput);

            //Final result
            if (!SecondLevelComputation)
            {
                return firstLevelOutput;
            }
            else
            {
                double secondLevelOutput = ComputeSecondLevelOutput(memberOutputs, firstLevelOutput);
                switch (_secondLevelCompCfg.CompMode)
                {
                    case SecondLevelCompMode.SecondLevelOutputOnly:
                        return secondLevelOutput;
                    case SecondLevelCompMode.AveragedOutputs:
                        {
                            WeightedAvg finalResult = new WeightedAvg();
                            finalResult.AddSampleValue(firstLevelOutput, 1d);
                            finalResult.AddSampleValue(secondLevelOutput, 1d);
                            return finalResult.Avg;
                        }
                    default:
                        return secondLevelOutput;
                }
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
            /// <param name="binBorder">Binary 0/1 border. Double value LT this border is considered as 0 and GE as 1.
            /// (relevant only if task type is Classification)
            /// </param>
            public ClusterErrStatistics(string clusterName, double binBorder = double.NaN)
            {
                ClusterName = clusterName;
                BinBorder = binBorder;
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
