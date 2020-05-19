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
using RCNet.Neural.Network.SM.Preprocessing.Input;
using RCNet.Neural.Network.SM.Preprocessing.Reservoir;
using RCNet.Neural.Network.SM.Preprocessing.Reservoir.Pool;
using RCNet.Neural.Network.SM.Preprocessing.Reservoir.Space3D;
using RCNet.Neural.Network.SM.Preprocessing.Reservoir.Pool.NeuronGroup;
using RCNet.Neural.Network.SM.Preprocessing.Reservoir.SynapseNS;
using RCNet.Neural.Network.SM.Preprocessing.Neuron;
using RCNet.Neural.Network.SM.Preprocessing.Neuron.Predictor;
using RCNet.Neural.Network.SM.Preprocessing;
using RCNet.Neural.Network.SM.Readout;
using RCNet.Neural.Network.SM;

namespace Demo.DemoConsoleApp.Examples
{
    /// <summary>
    /// Example code shows how to setup StateMachine using StateMachineDesigner.
    /// Example uses TTOO.csv from ./Data subfolder.
    /// Time series contains real share prices of TTOO title from https://finance.yahoo.com/quote/TTOO/history?p=TTOO.
    /// The last recorded prices are from 2018/03/02 so StateMachine is predicting next High and Low prices for the following
    /// business day 2018/03/05 (where real prices were High = 6.58$ and Low=5.99$).
    /// </summary>
    public class TTOOForecastDesigner : BaseExample
    {
        /// <summary>
        /// Runs the example code.
        /// </summary>
        public void Run()
        {
            //Create StateMachine configuration
            //Simplified input configuration
            InputEncoderSettings inputCfg = StateMachineDesigner.CreateInputCfg(new FeedingContinuousSettings(FeedingContinuousSettings.AutoBootCyclesNum, true),
                                                                                new ExternalFieldSettings("High", new RealFeatureFilterSettings()),
                                                                                new ExternalFieldSettings("Low", new RealFeatureFilterSettings()),
                                                                                new ExternalFieldSettings("Adj Close", new RealFeatureFilterSettings())
                                                                                );
            //Simplified readout layer configuration
            ReadoutLayerSettings readoutCfg = StateMachineDesigner.CreateForecastReadoutCfg(StateMachineDesigner.CreateSingleLayerRegrNet(new IdentitySettings(), 2, 1000),
                                                                                            0.1d,
                                                                                            1,
                                                                                            "High",
                                                                                            "Low"
                                                                                            );
            //Create designer instance
            StateMachineDesigner smd = new StateMachineDesigner(inputCfg, readoutCfg);
            //Create pure ESN fashioned StateMachine configuration
            StateMachineSettings stateMachineCfg = smd.CreatePureESNCfg(250, 1, 0, 0.2d, 0, 0.1d, 0.75d, null, PredictorsProvider.PredictorID.Activation, PredictorsProvider.PredictorID.ActivationSquare);
            //Display StateMachine xml configuration
            string xmlConfig = stateMachineCfg.GetXml(true).ToString();
            _log.Write("StateMachine configuration xml:");
            _log.Write("-------------------------------");
            _log.Write(xmlConfig);
            _log.Write(string.Empty);
            _log.Write("Pres Enter to continue (StateMachine training)...");
            _log.Write(string.Empty);
            Console.ReadLine();

            //Instantiation and training
            _log.Write("StateMachine training:");
            _log.Write("----------------------");
            _log.Write(string.Empty);
            //StateMachine instance
            StateMachine stateMachine = new StateMachine(stateMachineCfg);
            //StateMachine training
            TrainStateMachine(stateMachine, ".\\Data\\TTOO.csv", out double[] predictionInputVector);

            //Forecast
            ReadoutLayer.ReadoutData readoutData = stateMachine.ComputeReadoutData(predictionInputVector);
            _log.Write("    Forecast next High and Low (real values are High=6.58$ and Low=5.99$):", false);
            _log.Write(stateMachine.RL.GetForecastReport(readoutData.DataVector, 6));
            _log.Write(string.Empty);

            return;
        }

    }//TTOOForecastDesigner

}//Namespace
