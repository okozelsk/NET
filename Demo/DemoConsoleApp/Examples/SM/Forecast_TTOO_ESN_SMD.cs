using RCNet.Neural.Activation;
using RCNet.Neural.Data.Filter;
using RCNet.Neural.Network.NonRecurrent;
using RCNet.Neural.Network.SM;
using RCNet.Neural.Network.SM.Preprocessing.Input;
using RCNet.Neural.Network.SM.Preprocessing.Neuron.Predictor;
using RCNet.Neural.Network.SM.Readout;
using System;

namespace Demo.DemoConsoleApp.Examples.SM
{
    /// <summary>
    /// Example code shows how to use StateMachineDesigner to setup StateMachine as a pure ESN for multivariate timeseries forecast.
    /// Example uses TTOO.csv from ./Data subfolder.
    /// Time series contains real share prices of TTOO title from https://finance.yahoo.com/quote/TTOO/history?p=TTOO.
    /// The last recorded prices are from 2021/02/09 so StateMachine is predicting next High and Low prices for the following
    /// business day 2021/02/10 (where real prices were High = 3.61$ and Low=3.10$).
    /// </summary>
    public class Forecast_TTOO_ESN_SMD : StateMachineExampleBase
    {
        //Constructor
        public Forecast_TTOO_ESN_SMD()
            : base()
        {
            return;
        }

        //Methods
        /// <summary>
        /// Runs the example code.
        /// </summary>
        public void Run()
        {
            //Create StateMachine configuration
            //Simplified input configuration
            InputEncoderSettings inputCfg = StateMachineDesigner.CreateInputCfg(new FeedingContinuousSettings(FeedingContinuousSettings.AutoBootCyclesNum),
                                                                                new InputSpikesCoderSettings(),
                                                                                true,
                                                                                new ExternalFieldSettings("High", new RealFeatureFilterSettings()),
                                                                                new ExternalFieldSettings("Low", new RealFeatureFilterSettings()),
                                                                                new ExternalFieldSettings("Adj Close", new RealFeatureFilterSettings())
                                                                                );
            //Simplified readout layer configuration
            ReadoutLayerSettings readoutCfg = StateMachineDesigner.CreateForecastReadoutCfg(new CrossvalidationSettings(0.1d, 0, 1),
                                                                                            StateMachineDesigner.CreateSingleLayerFFNetCfg(new AFAnalogIdentitySettings(), 2, 1000),
                                                                                            1,
                                                                                            "High",
                                                                                            "Low"
                                                                                            );
            //Create designer instance
            StateMachineDesigner smd = new StateMachineDesigner(inputCfg, readoutCfg);
            //Create pure ESN fashioned StateMachine configuration
            StateMachineSettings stateMachineCfg = smd.CreatePureESNCfg(250, //Total size of the reservoir (number of hidden neurons within the reservoir).
                                                                        2.5, //Maximum stimulation strength through hidden neuron's input synapses.
                                                                        1d, //Connection density of an input field. 1 means that each input field will be synaptically connected to all neurons.
                                                                        0, //Maximum delay on an input synapse. 0 means no delay.
                                                                        0.1d, //Interconnection density. 0.1 means that each hidden neuron will be synaptically internally connected to 10% of other hidden neurons.
                                                                        0, //Maximum delay on an internal synapse. 0 means no delay.
                                                                        0d, //Maximum absolute value of the hidden neuron bias. 0 means no bias.
                                                                        0d, //Maximum retainment on an hidden neuron. 0 means no retainment.
                                                                        new PredictorsProviderSettings(new PredictorActivationSettings(),
                                                                                                       new PredictorActivationPowerSettings(2d, true),
                                                                                                       new PredictorFiringTraceSettings(0.05, 30)
                                                                                                       )
                                                                        );
            //Display StateMachine xml configuration
            string xmlConfig = stateMachineCfg.GetXml(true).ToString();
            _log.Write("StateMachine configuration xml:");
            _log.Write("-------------------------------");
            _log.Write(xmlConfig);
            _log.Write(string.Empty);
            _log.Write("Press Enter to continue (StateMachine training)...");
            _log.Write(string.Empty);
            Console.ReadLine();

            //Instantiation and training
            _log.Write("StateMachine training:");
            _log.Write("----------------------");
            _log.Write(string.Empty);
            //StateMachine instance
            StateMachine stateMachine = new StateMachine(stateMachineCfg);
            //StateMachine training
            TrainStateMachine(stateMachine, "./Data/TTOO.csv", out double[] predictionInputVector);

            //Forecasting
            double[] outputVector = stateMachine.Compute(predictionInputVector, out ReadoutLayer.ReadoutData readoutData);
            _log.Write("    Forecasted next High and Low TTOO prices (real prices were High = 3.61$ and Low=3.10$):", false);
            _log.Write(stateMachine.RL.GetForecastReport(readoutData.NatDataVector, 6));
            _log.Write(string.Empty);

            return;
        }

    }//Forecast_TTOO_ESN_SMD

}//Namespace
