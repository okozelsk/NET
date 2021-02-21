using RCNet.Neural.Activation;
using RCNet.Neural.Data;
using RCNet.Neural.Network.NonRecurrent;
using RCNet.Neural.Network.NonRecurrent.FF;
using System;
using System.Globalization;

namespace Demo.DemoConsoleApp.Examples.NonRecurrent
{
    /// <summary>
    /// This code example shows how to use the feed forward network component independently.
    /// It trains a multilayer Feed Forward network to solve AND, OR and XOR, using
    /// two possible ways. A hand made low level training and an use of TNRNetBuilder.
    /// </summary>
    public class FeedForwardNetwork_BooleanAlgebra : ExampleBase
    {
        //Constructor
        public FeedForwardNetwork_BooleanAlgebra()
            :base()
        {
            return;
        }

        //Methods
        /// <summary>
        /// Displays information about the network build process progress.
        /// </summary>
        /// <param name="buildProgress">The current state of the build process.</param>
        private void OnNetworkBuildProgressChanged(TNRNetBuilder.BuildProgress buildProgress)
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
        /// Creates the training data.
        /// Input vector contains 0/1 combination and output vector contains appropriate results of the AND, OR and XOR operation.
        /// </summary>
        private VectorBundle CreateTrainingData()
        {
            VectorBundle trainingData = new VectorBundle();
            trainingData.AddPair(new double[] { 0, 0 }, new double[] { 0, 0, 0 });
            trainingData.AddPair(new double[] { 0, 1 }, new double[] { 0, 1, 1 });
            trainingData.AddPair(new double[] { 1, 0 }, new double[] { 0, 1, 1 });
            trainingData.AddPair(new double[] { 1, 1 }, new double[] { 1, 1, 0 });
            return trainingData;
        }

        /// <summary>
        /// Tests and displays network's computations.
        /// </summary>
        /// <param name="network">A network to be tested.</param>
        private void DisplayNetworkComputations(INonRecurrentNetwork network)
        {
            VectorBundle verificationData = CreateTrainingData();
            _log.Write("Trained network computations:");
            _log.Write("-----------------------------");
            int sampleIdx = 0;
            foreach (double[] input in verificationData.InputVectorCollection)
            {
                double[] results = network.Compute(input);
                //Compute absolute errors
                double[] absError = new double[results.Length];
                for(int i = 0; i < results.Length; i++)
                {
                    absError[i] = Math.Abs(results[i] - verificationData.OutputVectorCollection[sampleIdx][i]);
                }
                _log.Write($"  Input {input[0]} {input[1]} Results: AND={Math.Abs(Math.Round(results[0])).ToString(CultureInfo.InvariantCulture)} OR={Math.Abs(Math.Round(results[1])).ToString(CultureInfo.InvariantCulture)} XOR={Math.Abs(Math.Round(results[2])).ToString(CultureInfo.InvariantCulture)}, Absolute Errors: {absError[0].ToString("E3", CultureInfo.InvariantCulture)} {absError[1].ToString("E3", CultureInfo.InvariantCulture)} {absError[2].ToString("E3", CultureInfo.InvariantCulture)}");
                ++sampleIdx;
            }
            _log.Write(string.Empty);
            return;
        }

        /// <summary>
        /// Creates a configuration of the feed forward network having Identity output layer and two LeakyReLU hidden layers
        /// with associated resilient back propagation trainer.
        /// </summary>
        private FeedForwardNetworkSettings CreateFFNetConfig()
        {
            const int HiddenLayerSize = 5;
            HiddenLayerSettings hiddenLayerCfg = new HiddenLayerSettings(HiddenLayerSize, new AFAnalogLeakyReLUSettings());
            FeedForwardNetworkSettings ffNetCfg = new FeedForwardNetworkSettings(new AFAnalogIdentitySettings(),
                                                                                 new HiddenLayersSettings(hiddenLayerCfg, hiddenLayerCfg),
                                                                                 new RPropTrainerSettings(2, 100)
                                                                                 );
            return ffNetCfg;
        }

        /// <summary>
        /// Trains FF network to solve boolean algebra. It shows how to do it on the lowest level,
        /// without use of TNRNetBuilder.
        /// </summary>
        private void FullyManualLearning()
        {
            _log.Write("Example of FF network low level training.");
            //Create FF network configuration.
            FeedForwardNetworkSettings ffNetCfg = CreateFFNetConfig();
            //Collect training data
            VectorBundle trainingData = CreateTrainingData();
            //Create network instance
            //We specify 2 input values, 3 output values and previously prepared network structure configuration
            FeedForwardNetwork ffNet = new FeedForwardNetwork(2, 3, ffNetCfg);

            //Training
            _log.Write("Training");
            _log.Write("--------");
            //Create the trainer instance
            RPropTrainer trainer = new RPropTrainer(ffNet,
                                                    trainingData.InputVectorCollection,
                                                    trainingData.OutputVectorCollection,
                                                    (RPropTrainerSettings)ffNetCfg.TrainerCfg,
                                                    new Random(0)
                                                    );
            //Training loop
            while (trainer.Iteration() && trainer.MSE > 1e-6)
            {
                _log.Write($"  Attempt {trainer.Attempt} / Epoch {trainer.AttemptEpoch,3} Mean Squared Error = {Math.Round(trainer.MSE, 8).ToString(CultureInfo.InvariantCulture)}", true);
            }
            _log.Write(string.Empty);

            //Training is done
            //Display the network computation results
            DisplayNetworkComputations(ffNet);
            //Finished
            return;
        }

        /// <summary>
        /// Trains FF network to solve boolean algebra. It shows how to use the TNRNetBuilder.
        /// </summary>
        private void TNRNetBuilderLearning()
        {
            _log.Write("Example of FF network build using TNRNetBuilder component.");
            //Create FF network configuration.
            FeedForwardNetworkSettings ffNetCfg = CreateFFNetConfig();
            //Collect training data
            VectorBundle trainingData = CreateTrainingData();
            //In our case, testing data is the same as training data
            VectorBundle testingData = trainingData;
            //Training
            //Create builder instance
            TNRNetBuilder builder = new TNRNetBuilder("Boolean Algebra", ffNetCfg, TNRNet.OutputType.Real, trainingData, testingData);
            //Register notification event handler
            builder.NetworkBuildProgressChanged += OnNetworkBuildProgressChanged;
            //Build network
            TNRNet ffNet = builder.Build();
            //Training is done
            //Display the network computation results
            DisplayNetworkComputations(ffNet.Network);
            //Finished
            return;
        }

        /// <summary>
        /// Runs the example code.
        /// </summary>
        public void Run()
        {
            Console.Clear();
            FullyManualLearning();
            _log.Write(string.Empty);
            _log.Write("Press Enter to continue with the TNRNetBuilder learning...");
            Console.ReadLine();
            _log.Write(string.Empty);
            TNRNetBuilderLearning();
            return;
        }//Run

    }//FeedForwardNetwork_BooleanAlgebra

}//Namespace
