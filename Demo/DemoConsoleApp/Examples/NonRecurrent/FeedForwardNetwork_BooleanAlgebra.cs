using RCNet.Neural.Activation;
using RCNet.Neural.Data;
using RCNet.Neural.Network.NonRecurrent;
using RCNet.Neural.Network.NonRecurrent.FF;
using System;
using System.Globalization;

namespace Demo.DemoConsoleApp.Examples.NonRecurrent
{
    /// <summary>
    /// This code example shows how to use the FeedForwardNetwork component independently.
    /// </summary>
    /// <remarks>
    /// <para>
    /// Multilayer Feed Forward network here solves AND, OR and XOR.
    /// Three ways of a network training are shown:
    /// </para>
    /// <para>
    /// 1. A hand made low level training.
    /// </para>
    /// <para>
    /// 2. An use of TNRNetBuilder with custom build process controller.
    /// </para>
    /// <para>
    /// 3. An use of TNRNetBuilder with default build process controller.
    /// </para>
    /// </remarks>
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
        /// Evaluates whether the "candidate" network achieved a better result than the best network so far.
        /// </summary>
        /// <param name="candidate">The candidate network to be evaluated.</param>
        /// <param name="currentBest">The best network so far.</param>
        private static bool IsBetter(TNRNet candidate, TNRNet currentBest)
        {
            return candidate.CombinedPrecisionError < currentBest.CombinedPrecisionError;
        }

        /// <summary>
        /// Implements a simple build process controller which stops the build process when
        /// an average absolute error is less than 1e-4.
        /// </summary>
        private TNRNetBuilder.BuildInstr NetworkBuildController(TNRNetBuilder.BuildProgress buildProgress)
        {
            bool currentIsBetter = IsBetter(buildProgress.CurrNetwork,
                                            buildProgress.BestNetwork
                                            );
            TNRNetBuilder.BuildInstr instructions = new TNRNetBuilder.BuildInstr
            {
                CurrentIsBetter = currentIsBetter,
                StopProcess = (currentIsBetter && buildProgress.CurrNetwork.CombinedPrecisionError < 1e-4)
            };
            return instructions;
        }

        /// <summary>
        /// Displays information about the network build process progress.
        /// </summary>
        /// <param name="buildProgress">The current state of the build process.</param>
        private void OnNetworkBuildProgressChanged(TNRNetBuilder.BuildProgress buildProgress)
        {
            const int leftMargin = 4;
            const int reportEpochsInterval = 5;
            //Progress info
            if (buildProgress.ShouldBeReported || (buildProgress.EndNetworkEpochNum % reportEpochsInterval == 0))
            {
                //Build progress report message
                string progressText = buildProgress.GetInfoText(leftMargin);
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
            _log.Write("  Trained network computations:");
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
                _log.Write($"    Input {input[0]} {input[1]} Results: AND={Math.Abs(Math.Round(results[0])).ToString(CultureInfo.InvariantCulture)} OR={Math.Abs(Math.Round(results[1])).ToString(CultureInfo.InvariantCulture)} XOR={Math.Abs(Math.Round(results[2])).ToString(CultureInfo.InvariantCulture)}, Absolute Errors: {absError[0].ToString("E3", CultureInfo.InvariantCulture)} {absError[1].ToString("E3", CultureInfo.InvariantCulture)} {absError[2].ToString("E3", CultureInfo.InvariantCulture)}");
                ++sampleIdx;
            }
            _log.Write(string.Empty);
            return;
        }

        /// <summary>
        /// Creates a configuration of the feed forward network having Identity output layer, two LeakyReLU hidden layers and associated resilient back propagation trainer.
        /// </summary>
        private FeedForwardNetworkSettings CreateFFNetConfig()
        {
            const int HiddenLayerSize = 5;
            const int numOfTrainingAttempts = 2;
            const int numOfTrainingAttemptEpochs = 150;
            return new FeedForwardNetworkSettings(new AFAnalogIdentitySettings(), //Output layer activation
                                                  new HiddenLayersSettings(new HiddenLayerSettings(HiddenLayerSize, new AFAnalogLeakyReLUSettings()), //First hidden layer
                                                                           new HiddenLayerSettings(HiddenLayerSize, new AFAnalogLeakyReLUSettings()) //Second hidden layer
                                                                           ),
                                                  new RPropTrainerSettings(numOfTrainingAttempts, //The number of training attempts
                                                                           numOfTrainingAttemptEpochs //The number of epochs within a training attempt
                                                                           )
                                                  );
        }

        /// <summary>
        /// Trains FF network to solve boolean algebra. It shows how to do it on the lowest level,
        /// without use of TNRNetBuilder.
        /// </summary>
        private void FullyManualLearning()
        {
            _log.Write("Example of a FF network low level training:");
            //Create FF network configuration.
            FeedForwardNetworkSettings ffNetCfg = CreateFFNetConfig();
            _log.Write($"Network configuration xml:");
            _log.Write(ffNetCfg.GetXml(true).ToString());
            //Collect training data
            VectorBundle trainingData = CreateTrainingData();
            //Create network instance
            //We specify 2 input values, 3 output values and previously prepared network structure configuration
            FeedForwardNetwork ffNet = new FeedForwardNetwork(2, //The number of input values
                                                              3, //The number of output values
                                                              ffNetCfg //Network structure and a trainer
                                                              );

            //Training
            _log.Write(string.Empty);
            _log.Write("  Training");
            _log.Write(string.Empty);
            //Create the trainer instance
            RPropTrainer trainer = new RPropTrainer(ffNet,
                                                    trainingData.InputVectorCollection,
                                                    trainingData.OutputVectorCollection,
                                                    (RPropTrainerSettings)ffNetCfg.TrainerCfg,
                                                    new Random(0)
                                                    );
            //Training loop
            while (trainer.Iteration())
            {
                _log.Write($"    Attempt {trainer.Attempt} / Epoch {trainer.AttemptEpoch,3} Mean Squared Error = {Math.Round(trainer.MSE, 8).ToString(CultureInfo.InvariantCulture)}", true);
                //Check training exit condition
                if(trainer.MSE < 1e-7)
                {
                    break;
                }
            }
            _log.Write(string.Empty);

            //Training is done
            //Display the network computation results
            DisplayNetworkComputations(ffNet);
            //Finished
            return;
        }

        /// <summary>
        /// Trains FF network to solve boolean algebra.
        /// It shows how to use the TNRNetBuilder (TNRNetBuilder is using here defined build controller).
        /// </summary>
        private void TNRNetBuilderLearning_CustomController()
        {
            _log.Write("Example of a FF network build using the TNRNetBuilder component with custom build controller:");
            //Create FF network configuration.
            FeedForwardNetworkSettings ffNetCfg = CreateFFNetConfig();
            _log.Write($"Network configuration xml:");
            _log.Write(ffNetCfg.GetXml(true).ToString());
            //Collect training data
            VectorBundle trainingData = CreateTrainingData();
            //In our case, testing data is the same as training data
            VectorBundle testingData = trainingData;
            //Training
            //Create builder instance
            TNRNetBuilder builder = new TNRNetBuilder("Boolean Algebra", //Network name
                                                      ffNetCfg, //Network configuration
                                                      TNRNet.OutputType.Real, //Network output is one or more real numbers
                                                      trainingData, //Training data
                                                      testingData, //Testing data
                                                      null, //No specific random generator object to be used
                                                      NetworkBuildController //Our custom build controller
                                                      );
            //Register notification event handler
            builder.NetworkBuildProgressChanged += OnNetworkBuildProgressChanged;
            //Build the network
            _log.Write(string.Empty);
            _log.Write("  Training");
            TNRNet ffNet = builder.Build();
            //Training is done
            _log.Write(string.Empty);
            //Display the network computation results
            DisplayNetworkComputations(ffNet.Network);
            //Finished
            return;
        }

        /// <summary>
        /// Trains FF network to solve boolean algebra.
        /// It shows how to use the TNRNetBuilder (TNRNetBuilder is using its default build controller).
        /// </summary>
        private void TNRNetBuilderLearning_DefaultController()
        {
            _log.Write("Example of a FF network build using the TNRNetBuilder component with default build controller:");
            //Create FF network configuration.
            FeedForwardNetworkSettings ffNetCfg = CreateFFNetConfig();
            _log.Write($"Network configuration xml:");
            _log.Write(ffNetCfg.GetXml(true).ToString());
            //Collect training data
            VectorBundle trainingData = CreateTrainingData();
            //In our case, testing data is the same as training data
            VectorBundle testingData = trainingData;
            //Training
            //Create builder instance
            TNRNetBuilder builder = new TNRNetBuilder("Boolean Algebra", //Network name
                                                      ffNetCfg, //Network configuration
                                                      TNRNet.OutputType.Real, //Network output is one or more real numbers
                                                      trainingData, //Training data
                                                      testingData, //Testing data
                                                      null, //No specific random generator object to be used
                                                      null //No specific build controller -> use default
                                                      );
            //Register notification event handler
            builder.NetworkBuildProgressChanged += OnNetworkBuildProgressChanged;
            //Build the network
            _log.Write(string.Empty);
            _log.Write("  Training");
            TNRNet ffNet = builder.Build();
            //Training is done
            _log.Write(string.Empty);
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
            _log.Write("Press Enter to continue with the TNRNetBuilder using the custom controller...");
            Console.ReadLine();
            _log.Write(string.Empty, true);
            _log.Write(string.Empty);
            TNRNetBuilderLearning_CustomController();
            _log.Write(string.Empty);
            _log.Write("Press Enter to continue with the TNRNetBuilder using the default controller...");
            Console.ReadLine();
            _log.Write(string.Empty, true);
            _log.Write(string.Empty);
            TNRNetBuilderLearning_DefaultController();
            return;
        }//Run

    }//FeedForwardNetwork_BooleanAlgebra

}//Namespace
