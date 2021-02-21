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
    /// Example code shows how to use TNRNetClusterChain and TNRNetClusterChainBuilder as the standalone components for classification.
    /// Example uses following csv datafiles from ./Data subfolder:
    ///   LibrasMovement_train.csv and LibrasMovement_verify.csv
    ///   ProximalPhalanxOutlineAgeGroup_train.csv and ProximalPhalanxOutlineAgeGroup_verify.csv
    /// </summary>
    class Classification_TNRNetClusterChain_FromScratch : NonRecurrentExampleBase
    {
        //Constructor
        public Classification_TNRNetClusterChain_FromScratch()
            : base()
        {
            return;
        }

        //Methods
        /// <summary>
        /// Displays information about the network cluster chain build process progress.
        /// </summary>
        /// <param name="buildProgress">The current state of the build process.</param>
        protected void OnClusterChainBuildProgressChanged(TNRNetClusterChainBuilder.BuildProgress buildProgress)
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
            _log.Write($"{name} classification performed by the Probabilistic cluster chain ({numOfClasses.ToString(CultureInfo.InvariantCulture)} classes).");
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
            //need to modify it. We only allocate the binary feature filters requested by the cluster chain builder.
            FeatureFilterBase[] outputFeatureFilters = new BinFeatureFilter[numOfClasses];
            for (int i = 0; i < numOfClasses; i++)
            {
                outputFeatureFilters[i] = new BinFeatureFilter(Interval.IntZP1);
            }
            //Cluster chain configuration (we will have two chained clusters)
            //Configuration of the first cluster in the chain
            //End-networks configuration for the first cluster in the chain. For every testing fold will be trained two end-networks with different structure.
            List<FeedForwardNetworkSettings> netCfgs1 = new List<FeedForwardNetworkSettings>
            {
                //The first FF network will have two hidden layers of 30 TanH activated neurons.
                //Output layer will have the SoftMax activation (it must be SoftMax because we will use the Probabilistic cluster).
                new FeedForwardNetworkSettings(new AFAnalogSoftMaxSettings(),
                                               new HiddenLayersSettings(new HiddenLayerSettings(30, new AFAnalogTanHSettings()),
                                                                        new HiddenLayerSettings(30, new AFAnalogTanHSettings())
                                                                        ),
                                               new RPropTrainerSettings(3, 1000)
                                               ),
                //The second FF network will have two hidden layers of 30 LeakyReLU activated neurons.
                //Output layer will have the SoftMax activation (it must be SoftMax because we will use the Probabilistic cluster).
                new FeedForwardNetworkSettings(new AFAnalogSoftMaxSettings(),
                                               new HiddenLayersSettings(new HiddenLayerSettings(30, new AFAnalogLeakyReLUSettings()),
                                                                        new HiddenLayerSettings(30, new AFAnalogLeakyReLUSettings())
                                                                        ),
                                               new RPropTrainerSettings(3, 1000)
                                               )
            };
            //The first probabilistic network cluster configuration instance
            TNRNetClusterProbabilisticSettings clusterCfg1 =
                new TNRNetClusterProbabilisticSettings(new TNRNetClusterProbabilisticNetworksSettings(netCfgs1),
                                                       new TNRNetClusterProbabilisticWeightsSettings()
                                                       );
            //Configuration of the second cluster in the chain
            //End-network configuration for the second cluster in the chain. For every testing fold will be trained one end-network.
            List<FeedForwardNetworkSettings> netCfgs2 = new List<FeedForwardNetworkSettings>
            {
                //FF network will have two hidden layers of 30 Elliot activated neurons.
                //Output layer will have the SoftMax activation (it must be SoftMax because we will use the Probabilistic cluster chain).
                new FeedForwardNetworkSettings(new AFAnalogSoftMaxSettings(),
                                               new HiddenLayersSettings(new HiddenLayerSettings(30, new AFAnalogElliotSettings()),
                                                                        new HiddenLayerSettings(30, new AFAnalogElliotSettings())
                                                                        ),
                                               new RPropTrainerSettings(3, 1000)
                                               )
            };
            //The second probabilistic network cluster configuration instance
            TNRNetClusterProbabilisticSettings clusterCfg2 =
                new TNRNetClusterProbabilisticSettings(new TNRNetClusterProbabilisticNetworksSettings(netCfgs2),
                                                       new TNRNetClusterProbabilisticWeightsSettings()
                                                       );

            //Probabilistic network cluster chain configuration instance
            ITNRNetClusterChainSettings chainCfg =
                new TNRNetClusterChainProbabilisticSettings(new CrossvalidationSettings(foldDataRatio),
                                                            new TNRNetClustersProbabilisticSettings(clusterCfg1,
                                                                                                    clusterCfg2
                                                                                                    )
                                                            );

            //Training
            _log.Write($"Cluster chain training on {trainDataFile}...");
            //An instance of network cluster chain builder.
            TNRNetClusterChainBuilder builder =
                new TNRNetClusterChainBuilder("Probabilistic Cluster Chain", chainCfg);
            //Register progress event handler
            builder.ChainBuildProgressChanged += OnClusterChainBuildProgressChanged;
            //Build the trained network cluster chain.
            TNRNetClusterChain trainedClusterChain = builder.Build(trainData, outputFeatureFilters);

            //Verification
            _log.Write(string.Empty);
            _log.Write(string.Empty);
            _log.Write($"Cluster chain verification on {verifyDataFile}...");
            _log.Write(string.Empty);
            int numOfErrors = 0;
            for (int i = 0; i < verifyData.InputVectorCollection.Count; i++)
            {
                double[] computed = trainedClusterChain.Compute(verifyData.InputVectorCollection[i], out _);
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



    }//Classification_TNRNetClusterChain_FromScratch

}//Namespace
