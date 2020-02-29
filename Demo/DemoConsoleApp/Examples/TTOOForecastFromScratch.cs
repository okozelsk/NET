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
using RCNet.Neural.Network.SM.Preprocessing.Reservoir.Neuron;
using RCNet.Neural.Network.SM.Preprocessing.Reservoir.Space3D;
using RCNet.Neural.Network.SM.Preprocessing.Reservoir.Pool.NeuronGroup;
using RCNet.Neural.Network.SM.Preprocessing.Reservoir.SynapseNS;
using RCNet.Neural.Network.SM.Preprocessing.Reservoir.Neuron.Predictor;
using RCNet.Neural.Network.SM.Preprocessing;
using RCNet.Neural.Network.SM.Readout;
using RCNet.Neural.Network.SM;

namespace Demo.DemoConsoleApp.Examples
{
    /// <summary>
    /// Example uses TTOO.csv from ./Data subfolder and stores data to ./Examples folder.
    /// Example code shows how to:
    ///   setup StateMachine from scratch,
    ///   store configuration xml,
    ///   train and serialize StateMachine,
    ///   load serialized StateMachine and forecast next values
    /// </summary>
    public class TTOOForecastFromScratch : BaseExample
    {
        /// <summary>
        /// Creates input part of the neural preprocessor's configuration.
        /// </summary>
        private InputSettings CreateInputCfg()
        {
            //Definition of input external fields
            //In this example we will use three of available input fields: "High" price, "Low" price and "Adj Close" price.
            //We want to route input fields to readout layer together with other predictors
            const bool RouteToReadout = true;
            //All 3 input fields are real numbers and thus they should be standardly normalized and standardized.
            RealFeatureFilterSettings realFeatureFilterCfg = new RealFeatureFilterSettings(true, true, false);
            //Input fields collection
            ExternalFieldSettings extFieldHighCfg = new ExternalFieldSettings("High", realFeatureFilterCfg, RouteToReadout);
            ExternalFieldSettings extFieldLowCfg = new ExternalFieldSettings("Low", realFeatureFilterCfg, RouteToReadout);
            ExternalFieldSettings extFieldAdjCloseCfg = new ExternalFieldSettings("Adj Close", realFeatureFilterCfg, RouteToReadout);
            ExternalFieldsSettings externalFieldsCfg = new ExternalFieldsSettings(extFieldHighCfg, extFieldLowCfg, extFieldAdjCloseCfg);
            //Definition of the continuous input feeding
            //We use FeedingContinuousSettings.AutoBootCyclesNum so necessary number of boot cycles will be automatically determined
            //based on neural preprocessor structure
            FeedingContinuousSettings feedingContinuousCfg = new FeedingContinuousSettings(FeedingContinuousSettings.AutoBootCyclesNum, RouteToReadout);
            //Create and return input configuration
            return new InputSettings(feedingContinuousCfg, new FieldsSettings(externalFieldsCfg));
        }

        /// <summary>
        /// Creates configuration of group of analog neurons having TanH activation.
        /// </summary>
        /// <param name="groupName">Name of the group</param>
        /// <param name="role">Role of the neurons within the group (Excitatory/Inhibitory)</param>
        /// <param name="relShare">Relative share. It determines how big part of the pool will be occupied by this neuron group</param>
        private AnalogNeuronGroupSettings CreateTanHGroup(string groupName, NeuronCommon.NeuronRole role, double relShare)
        {
            //Each neuron within the group will have its own constant bias
            //selected from the range -0.05 to 0.05 using gaussian (normal) distribution
            RandomValueSettings biasCfg = new RandomValueSettings(-0.05, 0.05, false, new GaussianDistrSettings(0, 1));
            //We want retainment property to be set for every neuron within the group
            const double RetainmentDensity = 1d;
            //Each neuron will have its own constant retainment strength
            //selected from the range 0 to 0.75 using uniform distribution
            URandomValueSettings retainmentStrengthCfg = new URandomValueSettings(0, 0.75);
            AnalogRetainmentSettings retainmentCfg = new AnalogRetainmentSettings(RetainmentDensity, retainmentStrengthCfg);
            //Create neuron group configuration
            AnalogNeuronGroupSettings groupCfg = new AnalogNeuronGroupSettings(groupName,
                                                                               role,
                                                                               relShare,
                                                                               new TanHSettings(),
                                                                               AnalogNeuronGroupSettings.DefaultFiringThreshold,
                                                                               AnalogNeuronGroupSettings.DefaultSignalingRestriction,
                                                                               AnalogNeuronGroupSettings.DefaultReadoutDensity,
                                                                               biasCfg,
                                                                               retainmentCfg
                                                                               );
            return groupCfg;
        }

        /// <summary>
        /// Creates 3D pool of analog neurons
        /// </summary>
        /// <param name="poolName">Name of the pool</param>
        /// <param name="dimX">Size on X dimension</param>
        /// <param name="dimY">Size on Y dimension</param>
        /// <param name="dimZ">Size on Z dimension</param>
        /// <param name="randomInterconnectionDensity">Random schema interconnection density</param>
        private PoolSettings CreateAnalogPoolCfg(string poolName, int dimX, int dimY, int dimZ, double randomInterconnectionDensity)
        {
            //Create excitatory and Inhibitory neuron groups configuration. Ratio of excitatory and inhibitory neurons will be 1:1,
            //because both groups have the same relative share.
            AnalogNeuronGroupSettings excitatoryGrpCfg = CreateTanHGroup("Exc-TanH-Grp", NeuronCommon.NeuronRole.Excitatory, 1);
            AnalogNeuronGroupSettings inhibitoryGrpCfg = CreateTanHGroup("Inh-TanH-Grp", NeuronCommon.NeuronRole.Inhibitory, 1);
            //We use two interconnection schemas
            //Random schema. We use ConnDistrFlatSettings so number of Excitatory->Excitatory,
            //Excitatory->Inhibitory, Inhibitory->Excitatory and Inhibitory->Inhibitory connections will be the same
            RandomSchemaSettings randomSchemaCfg = new RandomSchemaSettings(new ConnDistrFlatSettings(), randomInterconnectionDensity);
            //Chain circle shaped schema. We use ratio 1 so all neurons within the pool will be connected into the circle shaped chain.
            ChainSchemaSettings chainSchemaCfg = new ChainSchemaSettings(1d, true);
            //Create pool configuration
            PoolSettings poolCfg = new PoolSettings(poolName,
                                                    new ProportionsSettings(dimX, dimY, dimZ),
                                                    new NeuronGroupsSettings(excitatoryGrpCfg, inhibitoryGrpCfg),
                                                    new InterconnSettings(randomSchemaCfg, chainSchemaCfg)
                                                    );
            return poolCfg;
        }

        /// <summary>
        /// Creates reservoir structure configuration consisting of two interconnected pools.
        /// </summary>
        /// <param name="structName">Name of the reservoir structure</param>
        /// <param name="pool1Name">Name of the first pool</param>
        /// <param name="pool2Name">Name of the second pool</param>
        private ReservoirStructureSettings CreateResStructCfg(string structName, string pool1Name, string pool2Name)
        {
            //Our pools will have the 5x5x5 cube shape each. So 125 neurons in each pool and 250 neurons in total.
            const int DimX = 5, DimY = 5, DimZ = 5;
            //Each pool will have random internal interconnection of the density = 0.05. In our case it means that
            //each neuron will be randomly connected to 0.05 * 125 = 6 other neurons within the pool.
            const double RandomInterconnectionDensity = 0.05;
            //Create pools
            PoolSettings pool1Cfg = CreateAnalogPoolCfg(pool1Name, DimX, DimY, DimZ, RandomInterconnectionDensity);
            PoolSettings pool2Cfg = CreateAnalogPoolCfg(pool2Name, DimX, DimY, DimZ, RandomInterconnectionDensity);
            //Pool to pool interconnection
            //We use ConnDistrFlatSettings so number of Excitatory->Excitatory,
            //Excitatory->Inhibitory, Inhibitory->Excitatory and Inhibitory->Inhibitory connections will be the same.
            IConnDistrSettings connDistrCfg = new ConnDistrFlatSettings();
            //Connections from Pool1 to Pool2. We use sourcePoolDensity=1 and targetPoolDensity 0.02, so each neuron from
            //Pool1 will be randomly connected to 125 * 0.02 = 3 neurons from Pool2
            InterPoolConnSettings pool1To2ConnCfg = new InterPoolConnSettings(connDistrCfg, pool1Name, 1d, pool2Name, 0.02d);
            //Connections from Pool2 to Pool1. We use sourcePoolDensity=1 and targetPoolDensity 0.02, so each neuron from
            //Pool2 will be randomly connected to 125 * 0.02 = 3 neurons from Pool1
            InterPoolConnSettings pool2To1ConnCfg = new InterPoolConnSettings(connDistrCfg, pool2Name, 1d, pool1Name, 0.02d);
            //Create named reservoir structure configuration
            ReservoirStructureSettings resStructCfg = new ReservoirStructureSettings(structName,
                                                                                     new PoolsSettings(pool1Cfg, pool2Cfg),
                                                                                     new InterPoolConnsSettings(pool1To2ConnCfg, pool2To1ConnCfg)
                                                                                     );
            return resStructCfg;
        }

        /// <summary>
        /// Creates configuration of input connection (from input unit to target pool)
        /// </summary>
        /// <param name="poolName">Target pool name</param>
        /// <param name="codingMethod">Analog coding method. It specifies what type of available analog value to be transmitted through input connection.</param>
        /// <param name="connMaxWeight">Maximum weight of the inut connection</param>
        private InputUnitConnSettings CreateInputUnitConnCfg(string poolName,
                                                             InputUnit.AnalogCodingMethod codingMethod,
                                                             double connMaxWeight
                                                             )
        {
            //We have no neurons with spikng activation so spiking target configuration is irrelevant
            SpikingTargetSettings spikingTargetCfg = null;
            //Inpt connection weight will be randomly selected from the range 0 to connMaxWeight using uniform distribution
            URandomValueSettings weightCfg = new URandomValueSettings(0, connMaxWeight);
            //We specify target scope = Synapse.SynapticTargetScope.All so both Excitatory and Inhibitory hidden neurons are
            //allowed to get input connection.
            //We use density = 1 so each hidden neuron within the scope (Synapse.SynapticTargetScope.All) will be really connected.
            AnalogTargetSettings analogTargetCfg = new AnalogTargetSettings(Synapse.SynapticTargetScope.All, 1, weightCfg);
            //Create connection configuration
            InputUnitConnSettings inputUnitConnCfg = new InputUnitConnSettings(poolName,
                                                                               codingMethod,
                                                                               InputUnitConnSettings.DefaultOppositeAmplitude,
                                                                               InputUnitConnSettings.DefaultSignalingRestriction,
                                                                               spikingTargetCfg,
                                                                               analogTargetCfg
                                                                               );
            return inputUnitConnCfg;
        }

        /// <summary>
        /// Creates input unit configuration associated with given input field
        /// (version of input connections to single pool)
        /// </summary>
        /// <param name="inpFieldName">Input field name</param>
        /// <param name="poolName">Target pool name</param>
        /// <param name="codingMethod">Analog coding method. It specifies what type of available analog value to be transmitted through input connection.</param>
        /// <param name="connMaxWeight">Maximum weight of the inut connection</param>
        private InputUnitSettings CreateInputUnitCfg(string inpFieldName, string poolName, InputUnit.AnalogCodingMethod codingMethod, double connMaxWeight)
        {
            return new InputUnitSettings(inpFieldName,
                                         new InputUnitConnsSettings(CreateInputUnitConnCfg(poolName, codingMethod, connMaxWeight))
                                         );
        }


        /// <summary>
        /// Creates input unit configuration associated with given input field
        /// (version of input connections to both pools)
        /// </summary>
        /// <param name="inpFieldName">Input field name</param>
        /// <param name="pool1Name">First target pool name</param>
        /// <param name="pool2Name">Second target pool name</param>
        /// <param name="codingMethod">Analog coding method. It specifies what type of available analog value to be transmitted through input connection.</param>
        /// <param name="connMaxWeight">Maximum weight of the inut connection</param>
        private InputUnitSettings CreateInputUnitCfg(string inpFieldName, string pool1Name, string pool2Name, InputUnit.AnalogCodingMethod codingMethod, double connMaxWeight)
        {
            return new InputUnitSettings(inpFieldName,
                                         new InputUnitConnsSettings(CreateInputUnitConnCfg(pool1Name, codingMethod, connMaxWeight),
                                                                    CreateInputUnitConnCfg(pool2Name, codingMethod, connMaxWeight)
                                                                    )
                                         );
        }

        /// <summary>
        /// Creates reservoir instance configuration
        /// </summary>
        /// <param name="instName">Name of the reservoir instance</param>
        /// <param name="structName">Name of the associated reservoir structure configuration</param>
        /// <param name="pool1Name">Name of the first pool</param>
        /// <param name="pool2Name">Name of the second pool</param>
        /// <param name="inputMaxDelay">Maximum delay of input synapses</param>
        /// <param name="internalMaxDelay">Maximum delay of internal synapses</param>
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
            //Create input units (and connections) configurations for each input field
            //We use InputUnit.AnalogCodingMethod.Actual so synapses will transmit unchanged (not transformed) input values
            //We connect High input field to Pool1
            InputUnitSettings inpUnitHighCfg = CreateInputUnitCfg("High", pool1Name, InputUnit.AnalogCodingMethod.Actual, ConnMaxWeight);
            //We connect Low input field to Pool2
            InputUnitSettings inpUnitLowCfg = CreateInputUnitCfg("Low", pool2Name, InputUnit.AnalogCodingMethod.Actual, ConnMaxWeight);
            //We connect Adj Close input field to both pools
            InputUnitSettings inpUnitAdjCloseCfg = CreateInputUnitCfg("Adj Close", pool1Name, pool2Name, InputUnit.AnalogCodingMethod.Difference, ConnMaxWeight / 2d);
            //Synapse general configuration
            SynapseSettings synapseCfg = new SynapseSettings(Synapse.SynapticDelayMethod.Random, inputMaxDelay,
                                                             Synapse.SynapticDelayMethod.Random, internalMaxDelay
                                                             );
            //Predictors configuration
            //We use constructor accepting array of boolean switches
            //Initially we set all switches to false - all available predictors are forbidden
            bool[] predictorSwitches = new bool[PredictorsProvider.NumOfPredictors];
            predictorSwitches.Populate(false);
            //Now we enable ActivationSquare and ActivationFadingSum predictors
            predictorSwitches[(int)PredictorsProvider.PredictorID.ActivationSquare] = true;
            predictorSwitches[(int)PredictorsProvider.PredictorID.ActivationFadingSum] = true;
            //Create predictors configuration using boolean switches and default (null) predictors parameters
            PredictorsSettings predictorsCfg = new PredictorsSettings(predictorSwitches, null);
            //Create reservoir instance configuration
            ReservoirInstanceSettings resInstCfg = new ReservoirInstanceSettings(instName,
                                                                                 structName,
                                                                                 new InputUnitsSettings(inpUnitHighCfg, inpUnitLowCfg, inpUnitAdjCloseCfg),
                                                                                 synapseCfg,
                                                                                 predictorsCfg
                                                                                 );
            return resInstCfg;
        }

        /// <summary>
        /// Creates neural preprocessor configuration
        /// </summary>
        /// <param name="resInstName">Reservoir instance name</param>
        /// <param name="resStructName">Reservoir structure name</param>
        /// <param name="pool1Name">Name of the pool1</param>
        /// <param name="pool2Name">Name of the pool2</param>
        NeuralPreprocessorSettings CreatePreprocessorCfg(string resInstName, string resStructName, string pool1Name, string pool2Name)
        {
            //Create input configuration
            InputSettings inputCfg = CreateInputCfg();
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
        /// Creates readout layer configuration
        /// </summary>
        /// <param name="testDataRatio">Specifies what part of available data to be used as test data</param>
        /// <param name="numOfAttempts">Number of regression attempts. Each readout network will try to learn numOfAttempts times</param>
        /// <param name="numOfEpochs">Number of training epochs within an attempt</param>
        ReadoutLayerSettings CreateReadoutLayerCfg(double testDataRatio, int numOfAttempts, int numOfEpochs)
        {
            //For each output field we will use prediction of two networks
            //First network having only Identity output neuron and associated the resilient back propagation trainer
            FeedForwardNetworkSettings ffNet1Cfg = new FeedForwardNetworkSettings(new IdentitySettings(),
                                                                                  null,
                                                                                  new RPropTrainerSettings(numOfAttempts, numOfEpochs)
                                                                                  );
            //Second network having Identity output neuron, hidden layer consisting of 5 LeakyReLU neurons
            //and associated the resilient back propagation trainer
            HiddenLayerSettings hiddenLayerCfg = new HiddenLayerSettings(5, new LeakyReLUSettings());
            FeedForwardNetworkSettings ffNet2Cfg = new FeedForwardNetworkSettings(new IdentitySettings(),
                                                                                  new HiddenLayersSettings(hiddenLayerCfg),
                                                                                  new RPropTrainerSettings(numOfAttempts, numOfEpochs)
                                                                                  );
            //Create default networks configuration for forecasting
            DefaultNetworksSettings defaultNetworksCfg = new DefaultNetworksSettings(null, new ForecastNetworksSettings(ffNet1Cfg, ffNet2Cfg));
            //Create readout units. We will forecast next High and Low prices. Both fields are real numbers.
            ReadoutUnitSettings highReadoutUnitCfg = new ReadoutUnitSettings("High", new ForecastTaskSettings(new RealFeatureFilterSettings()));
            ReadoutUnitSettings lowReadoutUnitCfg = new ReadoutUnitSettings("Low", new ForecastTaskSettings(new RealFeatureFilterSettings()));
            //Create readout layer configuration
            ReadoutLayerSettings readoutLayerCfg = new ReadoutLayerSettings(new ReadoutUnitsSettings(highReadoutUnitCfg,
                                                                                                     lowReadoutUnitCfg
                                                                                                     ),
                                                                            testDataRatio,
                                                                            ReadoutLayerSettings.AutoFolds,
                                                                            ReadoutLayerSettings.DefaultRepetitions,
                                                                            defaultNetworksCfg
                                                                            );
            return readoutLayerCfg;
        }

        /// <summary>
        /// Creates state machine configuration
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
            Directory.CreateDirectory(".\\Examples");

            //Create StateMachine configuration
            StateMachineSettings stateMachineCfg = CreateStateMachineCfg();
            //Store StateMachine xml configuration
            string xmlConfig = stateMachineCfg.GetXml(true).ToString();
            string configFileName = ".\\Examples\\TTOOForecastFromScratchSMConfig.xml";
            using (StreamWriter writer = new StreamWriter(File.Create(configFileName)))
            {
                writer.Write(xmlConfig);
            }
            //Display StateMachine xml configuration
            _log.Write("StateMachine configuration xml:");
            _log.Write("-------------------------------");
            _log.Write(xmlConfig);
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


            //Serialize StateMachine
            string serializationFileName = ".\\Examples\\TTOOForecastFromScratchSM.dat";
            stateMachine.SaveToFile(serializationFileName);

            //Forecast
            ReadoutLayer.ReadoutData readoutData = stateMachine.ComputeReadoutData(predictionInputVector);
            _log.Write("    Forecast next High and Low (real values are High=6.58$ and Low=5.99$):", false);
            _log.Write(stateMachine.RL.GetForecastReport(readoutData.DataVector, 6));
            _log.Write(string.Empty);

            //New StateMachine instance from the file
            StateMachine stateMachineNewInstance = StateMachine.LoadFromFile(serializationFileName);

            //New StateMachine instance forecast (exactly the same results as previous)
            readoutData = stateMachineNewInstance.ComputeReadoutData(predictionInputVector);
            _log.Write("    Forecast of the new StateMachine instance:", false);
            _log.Write(stateMachineNewInstance.RL.GetForecastReport(readoutData.DataVector, 6));
            _log.Write(string.Empty);


            return;
        }
    
    }//TTOOForecastFromScratch

}//Namespace
