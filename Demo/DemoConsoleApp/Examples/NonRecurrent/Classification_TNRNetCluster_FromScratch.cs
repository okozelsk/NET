using RCNet.CsvTools;
using RCNet.Extensions;
using RCNet.MathTools;
using RCNet.Neural.Activation;
using RCNet.Neural.Data;
using RCNet.Neural.Data.Filter;
using RCNet.Neural.Network.NonRecurrent;
using RCNet.Neural.Network.NonRecurrent.FF;
using System;
using System.Collections.Generic;
using System.Globalization;
using System.Text;

namespace Demo.DemoConsoleApp.Examples.NonRecurrent
{
    /// <summary>
    /// Example code shows how to use TNRNetCluster and TNRNetClusterBuilder as the standalone components for classification.
    /// Example uses following csv datafiles from ./Data subfolder:
    ///   LibrasMovement_train.csv and LibrasMovement_verify.csv
    ///   ProximalPhalanxOutlineAgeGroup_train.csv and ProximalPhalanxOutlineAgeGroup_verify.csv
    /// </summary>
    class Classification_TNRNetCluster_FromScratch : NonRecurrentExampleBase
    {
        //Constructor
        public Classification_TNRNetCluster_FromScratch()
            :base()
        {
            return;
        }

        //Methods
        /// <summary>
        /// Displays information about the network cluster build process progress.
        /// </summary>
        /// <param name="buildProgress">The current state of the build process.</param>
        protected void OnClusterBuildProgressChanged(TNRNetClusterBuilder.BuildProgress buildProgress)
        {
            int reportEpochsInterval = 5;
            //Progress info
            if (buildProgress.ShouldBeReported || (buildProgress.EndNetworkEpochNum % reportEpochsInterval == 0))
            {
                //Build progress report message
                string progressText = buildProgress.GetInfoText(4);
                //Report the progress
                _log.Write(progressText, !(buildProgress.NewEndNetwork));
            }
            return;
        }

        /// <summary>
        /// Trains the network cluster to perform classification task and then verifies its performance.
        /// </summary>
        /// <param name="name">The name of a classification task.</param>
        /// <param name="trainDataFile">The name of a csv datafile containing the training data.</param>
        /// <param name="verifyDataFile">The name of a csv datafile containing the verification data.</param>
        /// <param name="numOfClasses">The number of classes.</param>
        /// <param name="foldDataRatio">Specifies what part of training data is reserved for testing. It determines the size of data fold and also number of networks within the cluster.</param>
        private void PerformClassification(string name, string trainDataFile, string verifyDataFile, int numOfClasses, double foldDataRatio)
        {
            _log.Write($"{name} classification performed by the Probabilistic cluster ({numOfClasses.ToString(CultureInfo.InvariantCulture)} classes).");
            //Load csv data and create vector bundles
            _log.Write($"Loading {trainDataFile}...");
            CsvDataHolder trainCsvData = new CsvDataHolder(trainDataFile);
            VectorBundle trainData = VectorBundle.Load(trainCsvData, numOfClasses);
            _log.Write($"Loading {verifyDataFile}...");
            CsvDataHolder verifyCsvData = new CsvDataHolder(verifyDataFile);
            VectorBundle verifyData = VectorBundle.Load(verifyCsvData, numOfClasses);
            //Input data standardization
            //Allocation and preparation of the input feature filters
            FeatureFilterBase[] inputFeatureFilters = PrepareInputFeatureFilters(trainData);
            //Standardize training input data
            StandardizeInputVectors(trainData, inputFeatureFilters);
            //Standardize verification input data
            StandardizeInputVectors(verifyData, inputFeatureFilters);
            //Output data
            //Output data is already in the 0/1 form requested by the SoftMax activation so we don't
            //need to modify it. We only allocate the binary feature filters requested by the cluster builder.
            FeatureFilterBase[] outputFeatureFilters = new BinFeatureFilter[numOfClasses];
            for (int i = 0; i < numOfClasses; i++)
            {
                outputFeatureFilters[i] = new BinFeatureFilter(Interval.IntZP1);
            }
            //Cluster configuration
            //End-networks configuration. For every testing fold will be trained two end-networks with different structure.
            List<FeedForwardNetworkSettings> netCfgs = new List<FeedForwardNetworkSettings>
            {
                //The first FF network will have two hidden layers of 30 TanH activated neurons.
                //Output layer will have the SoftMax activation (it must be SoftMax because we will use the Probabilistic cluster).
                new FeedForwardNetworkSettings(new AFAnalogSoftMaxSettings(),
                                               new HiddenLayersSettings(new HiddenLayerSettings(30, new AFAnalogTanHSettings()),
                                                                        new HiddenLayerSettings(30, new AFAnalogTanHSettings())
                                                                        ),
                                               new RPropTrainerSettings(3, 200)
                                               ),
                //The second FF network will have two hidden layers of 30 LeakyReLU activated neurons.
                //Output layer will have the SoftMax activation (it must be SoftMax because we will use the Probabilistic cluster).
                new FeedForwardNetworkSettings(new AFAnalogSoftMaxSettings(),
                                               new HiddenLayersSettings(new HiddenLayerSettings(30, new AFAnalogLeakyReLUSettings()),
                                                                        new HiddenLayerSettings(30, new AFAnalogLeakyReLUSettings())
                                                                        ),
                                               new RPropTrainerSettings(3, 200)
                                               )
            };
            //Probabilistic network cluster configuration instance
            ITNRNetClusterSettings clusterCfg = new TNRNetClusterProbabilisticSettings(new TNRNetClusterProbabilisticNetworksSettings(netCfgs),
                                                                                       new TNRNetClusterProbabilisticWeightsSettings()
                                                                                       );
            _log.Write($"Cluster configuration xml:");
            _log.Write(clusterCfg.GetXml(true).ToString());
            //Training
            _log.Write($"Cluster training on {trainDataFile}...");
            //An instance of network cluster builder.
            TNRNetClusterBuilder builder =
                new TNRNetClusterBuilder("Probabilistic Cluster",
                                         new CrossvalidationSettings(foldDataRatio),
                                         clusterCfg,
                                         null,
                                         null
                                         );
            //Register progress event handler
            builder.ClusterBuildProgressChanged += OnClusterBuildProgressChanged;
            //Build the trained network cluster.
            TNRNetCluster trainedCluster = builder.Build(trainData, outputFeatureFilters);

            //Verification
            _log.Write(string.Empty);
            _log.Write(string.Empty);
            _log.Write($"Cluster verification on {verifyDataFile}...");
            _log.Write(string.Empty);
            int numOfErrors = 0;
            for (int i = 0; i < verifyData.InputVectorCollection.Count; i++)
            {
                double[] computed = trainedCluster.Compute(verifyData.InputVectorCollection[i], out _);
                //Cluster result
                int computedWinnerIdx = computed.MaxIdx();
                //Real result
                int realWinnerIdx = verifyData.OutputVectorCollection[i].MaxIdx();

                if (computedWinnerIdx != realWinnerIdx) ++numOfErrors;
                _log.Write($"({i + 1}/{verifyData.InputVectorCollection.Count}) Errors: {numOfErrors}", true);
            }
            _log.Write(string.Empty);
            _log.Write($"Accuracy {(1d - (double)numOfErrors / (double)verifyData.InputVectorCollection.Count).ToString(CultureInfo.InvariantCulture)}");
            _log.Write(string.Empty);

            return;
        }

        /// <summary>
        /// Runs the example code.
        /// </summary>
        public void Run()
        {
            Console.Clear();
            PerformClassification("Libras Movement",
                                  "./Data/LibrasMovement_train.csv",
                                  "./Data/LibrasMovement_verify.csv",
                                  15, //Number of classes
                                  1e-20 //Requested testing data fold ratio from training data. It is too small, but it will be corrected automatically to minimum viable ratio.
                                  );
            _log.Write(string.Empty);
            _log.Write("Press Enter to continue with the next classification case...");
            Console.ReadLine();
            _log.Write(string.Empty);
            PerformClassification("Proximal Phalanx Outline Age Group",
                                  "./Data/ProximalPhalanxOutlineAgeGroup_train.csv",
                                  "./Data/ProximalPhalanxOutlineAgeGroup_verify.csv",
                                  3, //Number of classes.
                                  0.1d //Requested testing data fold ratio from training data.
                                  );
            return;
        }



    }//Classification_TNRNetCluster_FromScratch

}//Namespace
