using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.IO;
using RCNet.RandomValue;
using RCNet.Extensions;
using RCNet.Neural.Data.Filter;
using RCNet.Neural.Activation;
using RCNet.Neural.Network.NonRecurrent.FF;
using RCNet.Neural.Data;
using System.Globalization;

namespace Demo.DemoConsoleApp.Examples
{
    /// <summary>
    /// This "Hello world" example shows how to use implemented FF network as the independent component.
    /// It trains the multilayer Feed Forward network to solve AND, OR and XOR.
    /// </summary>
    public class FFNetBoolAlg : BaseExample
    {
        /// <summary>
        /// Creates training data.
        /// Input vector contains 0/1 combination and output vector contains appropriate results of the AND, OR and XOR operation
        /// </summary>
        private VectorBundle CreateTrainingData()
        {
            VectorBundle trainingData = new VectorBundle();
            trainingData.AddPair(new double[] { 0, 0 }, new double[] { 0, 0, 0});
            trainingData.AddPair(new double[] { 0, 1 }, new double[] { 0, 1, 1 });
            trainingData.AddPair(new double[] { 1, 0 }, new double[] { 0, 1, 1 });
            trainingData.AddPair(new double[] { 1, 1 }, new double[] { 1, 1, 0 });
            return trainingData;
        }

        /// <summary>
        /// Runs the example code.
        /// </summary>
        public void Run()
        {
            //Create configuration of the feed forward network having Identity output layer and two LeakyReLU hidden layers
            //with associated resilient back propagation trainer configuration
            const int HiddenLayerSize = 3;
            HiddenLayerSettings hiddenLayerCfg = new HiddenLayerSettings(HiddenLayerSize, new LeakyReLUSettings());
            FeedForwardNetworkSettings ffNetCfg = new FeedForwardNetworkSettings(new IdentitySettings(),
                                                                                 new HiddenLayersSettings(hiddenLayerCfg, hiddenLayerCfg),
                                                                                 new RPropTrainerSettings(2, 200)
                                                                                 );
            //Collect training data
            VectorBundle trainingData = CreateTrainingData();
            //Create network instance
            //We specify 2 input values, 3 output values and previously prepared network structure configuration
            FeedForwardNetwork ffNet = new FeedForwardNetwork(2, 3, ffNetCfg);

            //Training
            _log.Write("Training");
            _log.Write("--------");
            //Create trainer instance
            RPropTrainer trainer = new RPropTrainer(ffNet,
                                                    trainingData.InputVectorCollection,
                                                    trainingData.OutputVectorCollection,
                                                    (RPropTrainerSettings)ffNetCfg.TrainerCfg,
                                                    new Random(0)
                                                    );
            //Training loop
            while (trainer.Iteration() && trainer.MSE > 1e-6)
            {
                _log.Write($"  Attempt {trainer.Attempt} / Epoch {trainer.AttemptEpoch.ToString().PadRight(3)} Mean Squared Error = {Math.Round(trainer.MSE, 8).ToString(CultureInfo.InvariantCulture)}", false);
            }
            _log.Write(string.Empty);

            //Training is done
            //Display network computation results
            _log.Write("Trained network computations:");
            _log.Write("-----------------------------");
            foreach (double[] input in trainingData.InputVectorCollection)
            {
                double[] results = ffNet.Compute(input);
                _log.Write($"  Input {input[0]} {input[1]} Results: AND={Math.Round(results[0])} OR={Math.Round(results[1])} XOR={Math.Round(results[2])}");
            }
            _log.Write(string.Empty);

            //Finished
            return;
        }//Run

    }//FFNetBoolAlg

}//Namespace
