﻿using RCNet.Neural.Activation;
using RCNet.Neural.Data.Filter;
using RCNet.Neural.Network.NonRecurrent;
using RCNet.Neural.Network.NonRecurrent.FF;
using RCNet.Neural.Network.SM;
using RCNet.Neural.Network.SM.Preprocessing;
using RCNet.Neural.Network.SM.Preprocessing.Input;
using RCNet.Neural.Network.SM.Preprocessing.Neuron.Predictor;
using RCNet.Neural.Network.SM.Preprocessing.Reservoir;
using RCNet.Neural.Network.SM.Preprocessing.Reservoir.Pool;
using RCNet.Neural.Network.SM.Preprocessing.Reservoir.Pool.NeuronGroup;
using RCNet.Neural.Network.SM.Preprocessing.Reservoir.Space3D;
using RCNet.Neural.Network.SM.Preprocessing.Reservoir.SynapseNS;
using RCNet.Neural.Network.SM.Readout;
using RCNet.RandomValue;
using System;
using System.IO;

namespace Demo.DemoConsoleApp.Examples.SM
{
    /// <summary>
    /// Example code shows how to: setup StateMachine from scratch, store configuration xml (writes it in Examples sub-folder),
    /// train and serialize StateMachine (writes it in Examples sub-folder), load serialized StateMachine and forecast next values.
    /// Example uses TTOO.csv from ./Data subfolder.
    /// Time series contains real share prices of TTOO title from https://finance.yahoo.com/quote/TTOO/history?p=TTOO.
    /// The last recorded prices are from 2021/02/09 so StateMachine is predicting next High and Low prices for the following
    /// business day 2021/02/10 (where real prices were High = 3.61$ and Low=3.10$).
    /// </summary>
    public class Forecast_TTOO_ESN_FromScratch : StateMachineExampleBase
    {
        //Constructor
        public Forecast_TTOO_ESN_FromScratch()
            : base()
        {
            return;
        }

        //Methods
        /// <summary>
        /// Creates the InputEncoder configuration.
        /// </summary>
        private InputEncoderSettings CreateInputCfg()
        {
            //Definition of input external fields
            //In this example we will use three of available input fields: "High" price, "Low" price and "Adj Close" price.
            //We want to route input fields to readout layer together with other predictors
            const bool RouteToReadout = true;
            //All 3 input fields are real numbers and thus they should be standardly normalized and standardized.
            RealFeatureFilterSettings realFeatureFilterCfg = new RealFeatureFilterSettings(true, true);
            //Input fields collection
            ExternalFieldSettings extFieldHighCfg = new ExternalFieldSettings("High", realFeatureFilterCfg, RouteToReadout);
            ExternalFieldSettings extFieldLowCfg = new ExternalFieldSettings("Low", realFeatureFilterCfg, RouteToReadout);
            ExternalFieldSettings extFieldAdjCloseCfg = new ExternalFieldSettings("Adj Close", realFeatureFilterCfg, RouteToReadout);
            ExternalFieldsSettings externalFieldsCfg = new ExternalFieldsSettings(extFieldHighCfg, extFieldLowCfg, extFieldAdjCloseCfg);
            //Definition of the continuous input feeding
            //We use FeedingContinuousSettings.AutoBootCyclesNum so necessary number of boot cycles will be automatically determined
            //based on neural preprocessor structure
            FeedingContinuousSettings feedingContinuousCfg = new FeedingContinuousSettings(FeedingContinuousSettings.AutoBootCyclesNum);
            //Create and return the InputEncoder configuration
            return new InputEncoderSettings(feedingContinuousCfg,
                                            new VaryingFieldsSettings(new InputSpikesCoderSettings(), externalFieldsCfg, null, null, RouteToReadout)
                                            );
        }

        /// <summary>
        /// Creates configuration of the group of analog neurons having a TanH activation.
        /// </summary>
        /// <param name="groupName">The name of the group</param>
        /// <param name="relShare">The relative share. It determines how big part of the pool neurons will be occupied by this neuron group.</param>
        private AnalogNeuronGroupSettings CreateTanHGroup(string groupName, double relShare)
        {
            //Each neuron within the group will have its own constant bias
            //selected from the range -0.05 to 0.05 using gaussian (normal) distribution
            RandomValueSettings biasCfg = new RandomValueSettings(-0.05, 0.05, false, new GaussianDistrSettings(0, 1));
            //We want retainment property to be set for every neuron within the group
            const double RetainmentDensity = 1d;
            //Each neuron will have its own constant retainment strength
            //selected from the range 0 to 0.75 using uniform distribution
            URandomValueSettings retainmentStrengthCfg = new URandomValueSettings(0, 0.75);
            RetainmentSettings retainmentCfg = new RetainmentSettings(RetainmentDensity, retainmentStrengthCfg);
            //Predictors configuration
            //We will use the Activation and ActivationPower predictors
            PredictorsProviderSettings predictorsCfg = new PredictorsProviderSettings(new PredictorActivationSettings(),
                                                                                      new PredictorActivationPowerSettings()
                                                                                      );



            //Create the neuron group configuration
            AnalogNeuronGroupSettings groupCfg = new AnalogNeuronGroupSettings(groupName,
                                                                               relShare,
                                                                               new AFAnalogTanHSettings(),
                                                                               predictorsCfg,
                                                                               AnalogNeuronGroupSettings.DefaultFiringThreshold,
                                                                               AnalogNeuronGroupSettings.DefaultThresholdMaxRefDeepness,
                                                                               biasCfg,
                                                                               retainmentCfg
                                                                               );
            return groupCfg;
        }

        /// <summary>
        /// Creates the 3D pool of analog neurons.
        /// </summary>
        /// <param name="poolName">The name of the pool.</param>
        /// <param name="dimX">Size on X dimension.</param>
        /// <param name="dimY">Size on Y dimension.</param>
        /// <param name="dimZ">Size on Z dimension.</param>
        /// <param name="randomInterconnectionDensity">The density of the random interconnection.</param>
        private PoolSettings CreateAnalogPoolCfg(string poolName, int dimX, int dimY, int dimZ, double randomInterconnectionDensity)
        {
            //Create TanH group of neurons
            AnalogNeuronGroupSettings grpCfg = CreateTanHGroup("Exc-TanH-Grp", 1);
            //We use two interconnection schemas
            //Chain schema (circle shaped). We use ratio 1 so all the neurons within the pool will be connected into the circle shaped chain.
            ChainSchemaSettings chainSchemaCfg = new ChainSchemaSettings(1d, true);
            //Random schema
            RandomSchemaSettings randomSchemaCfg = new RandomSchemaSettings(randomInterconnectionDensity);
            //Create pool configuration
            PoolSettings poolCfg = new PoolSettings(poolName,
                                                    new ProportionsSettings(dimX, dimY, dimZ),
                                                    new NeuronGroupsSettings(grpCfg),
                                                    new InterconnSettings(chainSchemaCfg, randomSchemaCfg)
                                                    );
            return poolCfg;
        }

        /// <summary>
        /// Creates the reservoir structure configuration consisting of two interconnected pools.
        /// </summary>
        /// <param name="structName">The name of the reservoir structure.</param>
        /// <param name="pool1Name">The name of the first pool.</param>
        /// <param name="pool2Name">The name of the second pool.</param>
        private ReservoirStructureSettings CreateResStructCfg(string structName, string pool1Name, string pool2Name)
        {
            //Our pools will have the 5x5x5 cube shape each. So 125 neurons in each pool and 250 neurons in total.
            const int DimX = 5, DimY = 5, DimZ = 5;
            //Each pool will have random internal interconnection of the density = 0.05. In our case it means that
            //each neuron will receive synapses from 0.05 * 125 = 6 other randomly selected neurons.
            const double RandomInterconnectionDensity = 0.05;
            //Create pools
            PoolSettings pool1Cfg = CreateAnalogPoolCfg(pool1Name, DimX, DimY, DimZ, RandomInterconnectionDensity);
            PoolSettings pool2Cfg = CreateAnalogPoolCfg(pool2Name, DimX, DimY, DimZ, RandomInterconnectionDensity);
            //Pool to pool interconnection
            //Connections from Pool1 to Pool2. We use targetPoolDensity=1 and sourcePoolDensity=0.02, so each neuron from
            //Pool2 will be randomly connected to 125 * 0.02 = 3 neurons from Pool1
            InterPoolConnSettings pool1To2ConnCfg = new InterPoolConnSettings(pool2Name, 1d, pool1Name, 0.02d);
            //Connections from Pool2 to Pool1. We use targetPoolDensity=1 and sourcePoolDensity=0.02, so each neuron from
            //Pool1 will be randomly connected to 125 * 0.02 = 3 neurons from Pool2
            InterPoolConnSettings pool2To1ConnCfg = new InterPoolConnSettings(pool1Name, 1d, pool2Name, 0.02d);
            //Create the reservoir structure configuration
            ReservoirStructureSettings resStructCfg = new ReservoirStructureSettings(structName,
                                                                                     new PoolsSettings(pool1Cfg, pool2Cfg),
                                                                                     new InterPoolConnsSettings(pool1To2ConnCfg, pool2To1ConnCfg)
                                                                                     );
            return resStructCfg;
        }

        /// <summary>
        /// Creates the configuration of an input connection.
        /// </summary>
        /// <param name="inputFieldName">The name of the input field.</param>
        /// <param name="poolName">The name of the target pool.</param>
        private InputConnSettings CreateInputConnCfg(string inputFieldName, string poolName)
        {
            //Create connection configuration
            InputConnSettings inputConnCfg = new InputConnSettings(inputFieldName,
                                                                   poolName,
                                                                   0,
                                                                   1
                                                                   );
            return inputConnCfg;
        }

        /// <summary>
        /// Creates the reservoir instance configuration.
        /// </summary>
        /// <param name="instName">The name of the reservoir instance.</param>
        /// <param name="structName">The name of the reservoir structure configuration.</param>
        /// <param name="pool1Name">The name of the first pool.</param>
        /// <param name="pool2Name">The name of the second pool.</param>
        /// <param name="inputMaxDelay">The maximum delay of input synapses.</param>
        /// <param name="internalMaxDelay">The maximum delay of internal synapses.</param>
        private ReservoirInstanceSettings CreateResInstCfg(string instName,
                                                           string structName,
                                                           string pool1Name,
                                                           string pool2Name,
                                                           int inputMaxDelay,
                                                           int internalMaxDelay
                                                           )
        {
            //Maximum weight of input connection
            const double ConnMaxWeight = 0.6d;
            //Create input connections configurations for each input field
            //We connect High input field to Pool1
            InputConnSettings inpConnHighCfg = CreateInputConnCfg("High", pool1Name);
            //We connect Low input field to Pool2
            InputConnSettings inpConnLowCfg = CreateInputConnCfg("Low", pool2Name);
            //We connect Adj Close input field to both pools
            InputConnSettings inpConnAdjCloseP1Cfg = CreateInputConnCfg("Adj Close", pool1Name);
            InputConnSettings inpConnAdjCloseP2Cfg = CreateInputConnCfg("Adj Close", pool2Name);
            //Synapse general configuration
            AnalogSourceSettings asc = new AnalogSourceSettings(new URandomValueSettings(0, ConnMaxWeight));
            SynapseATInputSettings synapseATInputSettings = new SynapseATInputSettings(Synapse.SynapticDelayMethod.Random, inputMaxDelay, asc, null);
            SynapseATIndifferentSettings synapseATIndifferentSettings = new SynapseATIndifferentSettings(Synapse.SynapticDelayMethod.Random, internalMaxDelay);
            SynapseATSettings synapseATCfg = new SynapseATSettings(SynapseATSettings.DefaultSpectralRadiusNum, synapseATInputSettings, synapseATIndifferentSettings);
            SynapseSettings synapseCfg = new SynapseSettings(null, synapseATCfg);
            //Create the reservoir instance configuration
            ReservoirInstanceSettings resInstCfg = new ReservoirInstanceSettings(instName,
                                                                                 structName,
                                                                                 new InputConnsSettings(inpConnHighCfg, inpConnLowCfg, inpConnAdjCloseP1Cfg, inpConnAdjCloseP2Cfg),
                                                                                 synapseCfg
                                                                                 );
            return resInstCfg;
        }

        /// <summary>
        /// Creates the neural preprocessor configuration.
        /// </summary>
        /// <param name="resInstName">The reservoir instance name.</param>
        /// <param name="resStructName">The reservoir structure configuration name.</param>
        /// <param name="pool1Name">The name of the pool1.</param>
        /// <param name="pool2Name">The name of the pool2.</param>
        NeuralPreprocessorSettings CreatePreprocessorCfg(string resInstName, string resStructName, string pool1Name, string pool2Name)
        {
            //Create input configuration
            InputEncoderSettings inputCfg = CreateInputCfg();
            //Create reservoir structure configuration
            ReservoirStructureSettings resStructCfg = CreateResStructCfg(resStructName, pool1Name, pool2Name);
            //Create reservoir instance configuration
            ReservoirInstanceSettings resInstCfg = CreateResInstCfg(resInstName, resStructName, pool1Name, pool2Name, 0, 1);
            //Create reservoir preprocessor configuration
            NeuralPreprocessorSettings preprocessorCfg = new NeuralPreprocessorSettings(inputCfg,
                                                                                        new ReservoirStructuresSettings(resStructCfg),
                                                                                        new ReservoirInstancesSettings(resInstCfg),
                                                                                        NeuralPreprocessorSettings.DefaultPredictorsReductionRatio
                                                                                        );
            return preprocessorCfg;
        }

        /// <summary>
        /// Creates the readout layer configuration.
        /// </summary>
        /// <param name="foldDataRatio">Specifies what part of available data to be used as the fold data.</param>
        /// <param name="numOfAttempts">Number of regression attempts. Each readout network will try to learn numOfAttempts times.</param>
        /// <param name="numOfEpochs">Number of training epochs within an attempt.</param>
        ReadoutLayerSettings CreateReadoutLayerCfg(double foldDataRatio, int numOfAttempts, int numOfEpochs)
        {
            //For each output field we will use prediction of two networks
            //First network having only Identity output neuron and associated the resilient back propagation trainer
            FeedForwardNetworkSettings ffNet1Cfg = new FeedForwardNetworkSettings(new AFAnalogIdentitySettings(),
                                                                                  null,
                                                                                  new RPropTrainerSettings(numOfAttempts, numOfEpochs)
                                                                                  );
            //Second network having Identity output neuron, hidden layer consisting of 5 LeakyReLU neurons
            //and associated the resilient back propagation trainer
            HiddenLayerSettings hiddenLayerCfg = new HiddenLayerSettings(5, new AFAnalogLeakyReLUSettings());
            FeedForwardNetworkSettings ffNet2Cfg = new FeedForwardNetworkSettings(new AFAnalogIdentitySettings(),
                                                                                  new HiddenLayersSettings(hiddenLayerCfg),
                                                                                  new RPropTrainerSettings(numOfAttempts, numOfEpochs)
                                                                                  );
            //Create the cluster chain configuration for the forecast and the default configuration for the forecast task.
            CrossvalidationSettings crossvalidationCfg = new CrossvalidationSettings(foldDataRatio);
            TNRNetClusterRealNetworksSettings networksCfg = new TNRNetClusterRealNetworksSettings(ffNet1Cfg, ffNet2Cfg);
            TNRNetClusterRealSettings realClusterCfg = new TNRNetClusterRealSettings(networksCfg, new TNRNetClusterRealWeightsSettings());
            TNRNetClusterChainRealSettings realClusterChainCfg = new TNRNetClusterChainRealSettings(crossvalidationCfg, new TNRNetClustersRealSettings(realClusterCfg));
            TaskDefaultsSettings taskDefaultsCfg = new TaskDefaultsSettings(null, realClusterChainCfg);
            //Create readout unit configurations. We will forecast next High and Low prices.
            ReadoutUnitSettings highReadoutUnitCfg = new ReadoutUnitSettings("High", new ForecastTaskSettings());
            ReadoutUnitSettings lowReadoutUnitCfg = new ReadoutUnitSettings("Low", new ForecastTaskSettings());
            //Create readout layer configuration
            ReadoutLayerSettings readoutLayerCfg = new ReadoutLayerSettings(taskDefaultsCfg,
                                                                            new ReadoutUnitsSettings(highReadoutUnitCfg,
                                                                                                     lowReadoutUnitCfg
                                                                                                     ),
                                                                            null
                                                                            );
            return readoutLayerCfg;
        }

        /// <summary>
        /// Creates the state machine configuration.
        /// </summary>
        private StateMachineSettings CreateStateMachineCfg()
        {
            //Create neural preprocessor configuration
            NeuralPreprocessorSettings neuralPreprocessorCfg = CreatePreprocessorCfg("MainRes", "MainResStruct", "AnalogPool1", "AnalogPool2");
            //Create readout layer configuration.
            //We use test data ratio = 0.1, number of regression attempts = 2 and number of attempt epochs = 1000
            ReadoutLayerSettings readoutCfg = CreateReadoutLayerCfg(0.1d, 2, 1000);
            //Create state machine configuration
            StateMachineSettings stateMachineCfg = new StateMachineSettings(neuralPreprocessorCfg, readoutCfg);
            return stateMachineCfg;
        }

        /// <summary>
        /// Runs the example code.
        /// </summary>
        public void Run()
        {
            //Create Examples directory
            var binDir = System.IO.Path.GetDirectoryName(System.Reflection.Assembly.GetExecutingAssembly().Location);
            var examplesDir = Path.Combine(binDir, "Examples");
            Directory.CreateDirectory(examplesDir);

            //Create StateMachine configuration
            StateMachineSettings stateMachineCfg = CreateStateMachineCfg();
            //Store StateMachine xml configuration
            string xmlConfig = stateMachineCfg.GetXml(true).ToString();
            string configFileName = Path.Combine(examplesDir, "TTOOForecastFromScratchSMConfig.xml");
            using (StreamWriter writer = new StreamWriter(File.Create(configFileName)))
            {
                writer.Write(xmlConfig);
            }
            //Display StateMachine xml configuration
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
            TrainStateMachine(stateMachine, Path.Combine(binDir, "Data", "TTOO.csv"), out double[] predictionInputVector);


            //Serialize StateMachine
            string serializationFileName = Path.Combine(examplesDir, "TTOOForecastFromScratchSM.dat");
            stateMachine.Serialize(serializationFileName);

            //Forecasting
            double[] outputVector = stateMachine.Compute(predictionInputVector, out ReadoutLayer.ReadoutData readoutData);
            _log.Write("    Forecasted next High and Low TTOO prices (real prices on 2021/02/10 are High=3.61$ and Low=3.10$):", false);
            _log.Write(stateMachine.RL.GetForecastReport(outputVector, 6));
            _log.Write(string.Empty);

            //Create new StateMachine instance from the file
            //Instance was serialized before forecasting of the next values
            StateMachine stateMachineNewInstance = StateMachine.Deserialize(serializationFileName);
            //Forecasting of the deserialized instance (exactly the same results as in previous forecasting)
            outputVector = stateMachineNewInstance.Compute(predictionInputVector, out readoutData);
            _log.Write("    Forecast of the new StateMachine instance:", false);
            _log.Write(stateMachineNewInstance.RL.GetForecastReport(outputVector, 6));
            _log.Write(string.Empty);


            return;
        }

    }//Forecast_TTOO_ESN_FromScratch

}//Namespace
