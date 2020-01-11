using RCNet.Neural.Data;
using RCNet.Neural.Data.Filter;
using System;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace RCNet.Neural.Network.NonRecurrent
{
    /// <summary>
    /// Builds computation cluster of trained networks
    /// </summary>
    public class TrainedNetworkClusterBuilder
    {
        //Constants
        /// <summary>
        /// Maximum part of available samples useable for test purposes
        /// </summary>
        public const double MaxRatioOfTestData = 0.5d;
        /// <summary>
        /// Minimum length of the test dataset
        /// </summary>
        public const int MinLengthOfTestDataset = 2;

        //Delegates
        /// <summary>
        /// Delegate of RegressionEpochDone event handler.
        /// </summary>
        /// <param name="buildingState">Current state of the regression process</param>
        /// <param name="foundBetter">Indicates that the best network was found as a result of the performed epoch</param>
        public delegate void RegressionEpochDoneHandler(TrainedNetworkBuilder.BuildingState buildingState, bool foundBetter);

        //Events
        /// <summary>
        /// This informative event occurs every time the regression epoch is done
        /// </summary>
        public event RegressionEpochDoneHandler RegressionEpochDone;

        //Attributes
        private readonly string _clusterName;
        private readonly object _networkSettings;
        private readonly double _binBorder;
        private readonly Random _rand;
        private readonly TrainedNetworkBuilder.RegressionControllerDelegate _controller;

        //Constructor
        /// <summary>
        /// Creates an instance ready to build computation cluster
        /// </summary>
        /// <param name="clusterName">Name of the cluster</param>
        /// <param name="networkSettings">Network configuration (FeedForwardNetworkSettings or ParallelPerceptronSettings object)</param>
        /// <param name="binBorder">If specified, it indicates that the whole network output is binary and specifies numeric border where GE network output is decided as a 1 and LT output as a 0.</param>
        /// <param name="rand">Random generator to be used (optional)</param>
        /// <param name="controller">Regression controller (optional)</param>
        public TrainedNetworkClusterBuilder(string clusterName,
                                            object networkSettings,
                                            double binBorder = double.NaN,
                                            Random rand = null,
                                            TrainedNetworkBuilder.RegressionControllerDelegate controller = null
                                            )
        {
            _clusterName = clusterName;
            _networkSettings = networkSettings;
            _binBorder = binBorder;
            _rand = rand ?? new Random(0);
            _controller = controller;
            return;
        }

        //Methods
        private void OnRegressionEpochDone(TrainedNetworkBuilder.BuildingState buildingState, bool foundBetter)
        {
            //Only raise up
            RegressionEpochDone(buildingState, foundBetter);
            return;
        }

        /// <summary>
        /// Builds computation cluster of trained networks
        /// </summary>
        /// <param name="dataBundle">Data to be used for training</param>
        /// <param name="testDataRatio">Ratio of test data to be used (determines fold size)</param>
        /// <param name="numOfFolds">Requested number of testing folds (determines number of cluster members). Value LE 0 causes automatic setup. </param>
        /// <param name="outputFeatureFilterCollection">Output feature filters to be used for output data denormalization.</param>
        public TrainedNetworkCluster Build(VectorBundle dataBundle,
                                           double testDataRatio,
                                           int numOfFolds,
                                           BaseFeatureFilter[] outputFeatureFilterCollection
                                           )
        {
            //Test fold size
            if (testDataRatio > MaxRatioOfTestData)
            {
                throw new ArgumentException($"Test data ratio is greater than {MaxRatioOfTestData.ToString(CultureInfo.InvariantCulture)}", "testingDataRatio");
            }
            int testDataSetLength = (int)Math.Round(dataBundle.OutputVectorCollection.Count * testDataRatio, 0);
            if (testDataSetLength < MinLengthOfTestDataset)
            {
                throw new ArgumentException($"Num of resulting test samples is less than {MinLengthOfTestDataset.ToString(CultureInfo.InvariantCulture)}", "testingDataRatio");
            }
            //Number of folds
            if (numOfFolds <= 0)
            {
                //Auto setup
                numOfFolds = dataBundle.OutputVectorCollection.Count / testDataSetLength;
            }
            //Data split to folds
            List<VectorBundle> subBundleCollection = dataBundle.Split(testDataSetLength, _binBorder);
            numOfFolds = Math.Min(numOfFolds, subBundleCollection.Count);
            //Clusters of trained networks
            TrainedNetworkCluster cluster = new TrainedNetworkCluster(_clusterName, subBundleCollection.Count, _binBorder);
            //Train alone network for each fold in the cluster.
            for (int foldIdx = 0; foldIdx < numOfFolds; foldIdx++)
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
                TrainedNetworkBuilder netBuilder = new TrainedNetworkBuilder(_clusterName,
                                                                             _networkSettings,
                                                                             foldIdx + 1,
                                                                             numOfFolds,
                                                                             trainingData,
                                                                             subBundleCollection[foldIdx],
                                                                             _binBorder,
                                                                             _rand,
                                                                             _controller
                                                                             );
                //Register notification
                netBuilder.RegressionEpochDone += OnRegressionEpochDone;
                //Build trained network. Trained network becomes to be the cluster member
                cluster.Members.Add(netBuilder.Build());
                //Update cluster error statistics (pesimistic approach)
                for (int sampleIdx = 0; sampleIdx < subBundleCollection[foldIdx].OutputVectorCollection.Count; sampleIdx++)
                {
                    double[] nrmComputedValues = cluster.Members.Last().Network.Compute(subBundleCollection[foldIdx].InputVectorCollection[sampleIdx]);
                    for (int i = 0; i < nrmComputedValues.Length; i++)
                    {
                        double natComputedValue = outputFeatureFilterCollection[i].ApplyReverse(nrmComputedValues[i]);
                        double natIdealValue = outputFeatureFilterCollection[i].ApplyReverse(subBundleCollection[foldIdx].OutputVectorCollection[sampleIdx][i]);
                        cluster.ErrorStats.Update(nrmComputedValues[i],
                                                  subBundleCollection[foldIdx].OutputVectorCollection[sampleIdx][i],
                                                  natComputedValue,
                                                  natIdealValue
                                                  );
                    }//i
                }//sampleIdx

            }//foldIdx

            //Return built cluster
            return cluster;
        }


    }//TrainedNetworkClusterBuilder

}//Namespace
