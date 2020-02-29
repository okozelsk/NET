using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.IO;
using RCNet.RandomValue;
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
using RCNet.Extensions;

namespace Demo.DemoConsoleApp.Examples
{
    /// <summary>
    /// Example uses TTOO.csv from ./Data subfolder and stores data to ./Examples folder.
    /// Example code shows how to:
    ///   setup StateMachine from scratch,
    ///   store configuration xml,
    ///   train and serialize StateMachine,
    ///   load serialized StateMachine and forecast next value
    /// </summary>
    public class TTOOForecastFromScratch : BaseExample
    {
        /// <summary>
        /// Creates input part of the neural preprocessor's configuration.
        /// </summary>
        private InputSettings CreateInputCfg()
        {
            //Definition of input external fields
            ExternalFieldSettings extFieldHighCfg = new ExternalFieldSettings("High", new RealFeatureFilterSettings(), true);
            ExternalFieldSettings extFieldLowCfg = new ExternalFieldSettings("Low", new RealFeatureFilterSettings(), true);
            ExternalFieldSettings extFieldAdjCloseCfg = new ExternalFieldSettings("Adj Close", new RealFeatureFilterSettings(), true);
            ExternalFieldsSettings externalFieldsCfg = new ExternalFieldsSettings(extFieldHighCfg, extFieldLowCfg, extFieldAdjCloseCfg);
            //Definition of the continuous input feeding
            FeedingContinuousSettings feedingContinuousCfg = new FeedingContinuousSettings(FeedingContinuousSettings.DefaultBootCycles,true);
            //Create and return definition of the input
            return new InputSettings(feedingContinuousCfg, new FieldsSettings(externalFieldsCfg));
        }

        private AnalogNeuronGroupSettings CreateTanHGroup(string groupName, NeuronCommon.NeuronRole role, double relShare)
        {
            RandomValueSettings biasCfg = new RandomValueSettings(-0.05, 0.05, false, new GaussianDistrSettings(0, 1));
            AnalogRetainmentSettings retainmentCfg = new AnalogRetainmentSettings(1, new URandomValueSettings(0, 0.75));
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

        private PoolSettings CreateAnalogPoolCfg(string poolName, int dimX, int dimY, int dimZ, double randomInterconnectionDensity)
        {
            AnalogNeuronGroupSettings excitatoryGrpCfg = CreateTanHGroup("Exc-TanH-Grp", NeuronCommon.NeuronRole.Excitatory, 1);
            AnalogNeuronGroupSettings inhibitoryGrpCfg = CreateTanHGroup("Inh-TanH-Grp", NeuronCommon.NeuronRole.Inhibitory, 1);
            RandomSchemaSettings randomSchemaCfg = new RandomSchemaSettings(new ConnDistrFlatSettings(), randomInterconnectionDensity);
            ChainSchemaSettings chainSchemaCfg = new ChainSchemaSettings(1d);

            PoolSettings pool1Cfg = new PoolSettings(poolName,
                                                     new ProportionsSettings(dimX, dimY, dimZ),
                                                     new NeuronGroupsSettings(excitatoryGrpCfg, inhibitoryGrpCfg),
                                                     new InterconnSettings(randomSchemaCfg, chainSchemaCfg)
                                                     );
            return pool1Cfg;
        }

        private ReservoirStructureSettings CreateResStructCfg(string structName, string pool1Name, string pool2Name)
        {
            PoolSettings pool1Cfg = CreateAnalogPoolCfg(pool1Name, 5, 5, 5, 0.05d);
            PoolSettings pool2Cfg = CreateAnalogPoolCfg(pool2Name, 5, 5, 5, 0.05d);
            IConnDistrSettings connDistrCfg = new ConnDistrFlatSettings();
            InterPoolConnSettings pool1To2ConnCfg = new InterPoolConnSettings(connDistrCfg, pool1Name, 1d, pool2Name, 0.02d);
            InterPoolConnSettings pool2To1ConnCfg = new InterPoolConnSettings(connDistrCfg, pool2Name, 1d, pool1Name, 0.02d);

            ReservoirStructureSettings resStructCfg = new ReservoirStructureSettings(structName,
                                                                                     new PoolsSettings(pool1Cfg, pool2Cfg),
                                                                                     new InterPoolConnsSettings(pool1To2ConnCfg, pool2To1ConnCfg)
                                                                                     );
            return resStructCfg;
        }

        private InputUnitConnSettings CreateInputUnitConnCfg(string poolName, InputUnit.AnalogCodingMethod codingMethod, double connMaxWeight)
        {
            return new InputUnitConnSettings(poolName,
                                             codingMethod,
                                             InputUnitConnSettings.DefaultOppositeAmplitude,
                                             InputUnitConnSettings.DefaultSignalingRestriction,
                                             null,
                                             new AnalogTargetSettings(Synapse.SynapticTargetScope.All,
                                                                      1,
                                                                      new URandomValueSettings(0, connMaxWeight)
                                                                      )
                                             );
        }

        private InputUnitSettings CreateInputUnitCfg(string inpFieldName, string poolName, InputUnit.AnalogCodingMethod codingMethod, double connMaxWeight)
        {
            return new InputUnitSettings(inpFieldName,
                                         new InputUnitConnsSettings(CreateInputUnitConnCfg(poolName, codingMethod, connMaxWeight))
                                         );
        }


        private InputUnitSettings CreateInputUnitCfg(string inpFieldName, string pool1Name, string pool2Name, InputUnit.AnalogCodingMethod codingMethod, double connMaxWeight)
        {
            return new InputUnitSettings(inpFieldName,
                                         new InputUnitConnsSettings(CreateInputUnitConnCfg(pool1Name, codingMethod, connMaxWeight),
                                                                    CreateInputUnitConnCfg(pool2Name, codingMethod, connMaxWeight)
                                                                    )
                                         );
        }

        private ReservoirInstanceSettings CreateResInstCfg(string instName,
                                                           string structName,
                                                           string pool1Name,
                                                           string pool2Name,
                                                           int inputMaxDelay,
                                                           int internalMaxDelay
                                                           )
        {
            const double ConnMaxWeight = 0.6d;
            InputUnitSettings inpUnitHighCfg = CreateInputUnitCfg("High", pool1Name, InputUnit.AnalogCodingMethod.Actual, ConnMaxWeight);
            InputUnitSettings inpUnitLowCfg = CreateInputUnitCfg("Low", pool2Name, InputUnit.AnalogCodingMethod.Actual, ConnMaxWeight);
            InputUnitSettings inpUnitAdjCloseCfg = CreateInputUnitCfg("Adj Close", pool1Name, pool2Name, InputUnit.AnalogCodingMethod.Difference, ConnMaxWeight / 2d);
            
            SynapseSettings synapseCfg = new SynapseSettings(Synapse.SynapticDelayMethod.Random, inputMaxDelay,
                                                             Synapse.SynapticDelayMethod.Random, internalMaxDelay
                                                             );
            
            bool[] predictorSwitches = new bool[PredictorsProvider.NumOfPredictors];
            predictorSwitches.Populate(false);
            predictorSwitches[(int)PredictorsProvider.PredictorID.ActivationSquare] = true;
            predictorSwitches[(int)PredictorsProvider.PredictorID.ActivationFadingSum] = true;
            PredictorsSettings predictorsCfg = new PredictorsSettings(predictorSwitches, null);

            ReservoirInstanceSettings resInstCfg = new ReservoirInstanceSettings(instName,
                                                                                 structName,
                                                                                 new InputUnitsSettings(inpUnitHighCfg, inpUnitLowCfg, inpUnitAdjCloseCfg),
                                                                                 synapseCfg,
                                                                                 predictorsCfg
                                                                                 );
            return resInstCfg;
        }

        NeuralPreprocessorSettings CreatePreprocessorCfg(string resInstName, string resStructName, string pool1Name, string pool2Name)
        {
            InputSettings inputCfg = CreateInputCfg();
            ReservoirStructureSettings resStructCfg = CreateResStructCfg(resStructName, pool1Name, pool2Name);
            ReservoirInstanceSettings resInstCfg = CreateResInstCfg(resInstName, resStructName, pool1Name, pool2Name, 0, 1);
            NeuralPreprocessorSettings preprocessorCfg = new NeuralPreprocessorSettings(inputCfg,
                                                                                        new ReservoirStructuresSettings(resStructCfg),
                                                                                        new ReservoirInstancesSettings(resInstCfg),
                                                                                        NeuralPreprocessorSettings.DefaultPredictorsReductionRatio
                                                                                        );
            return preprocessorCfg;
        }

        ReadoutLayerSettings CreateReadoutLayerCfg(double testDataRatio, int numOfAttempts, int numOfEpochs)
        {
            FeedForwardNetworkSettings ffNet1Cfg = new FeedForwardNetworkSettings(new IdentitySettings(),
                                                                                  null,
                                                                                  new RPropTrainerSettings(numOfAttempts, numOfEpochs)
                                                                                  );
            HiddenLayerSettings hiddenLayerCfg = new HiddenLayerSettings(5, new LeakyReLUSettings());
            FeedForwardNetworkSettings ffNet2Cfg = new FeedForwardNetworkSettings(new IdentitySettings(),
                                                                                  new HiddenLayersSettings(hiddenLayerCfg),
                                                                                  new RPropTrainerSettings(numOfAttempts, numOfEpochs)
                                                                                  );

            DefaultNetworksSettings defaultNetworksCfg = new DefaultNetworksSettings(null, new ForecastNetworksSettings(ffNet1Cfg, ffNet2Cfg));

            ReadoutUnitSettings highReadoutUnitCfg = new ReadoutUnitSettings("High", new ForecastTaskSettings(new RealFeatureFilterSettings()));
            ReadoutUnitSettings lowReadoutUnitCfg = new ReadoutUnitSettings("Low", new ForecastTaskSettings(new RealFeatureFilterSettings()));

            ReadoutLayerSettings readoutLayerCfg = new ReadoutLayerSettings(new ReadoutUnitsSettings(highReadoutUnitCfg,
                                                                                                     lowReadoutUnitCfg
                                                                                                     ),
                                                                            testDataRatio,
                                                                            ReadoutLayerSettings.DefaultFoldsNum,
                                                                            ReadoutLayerSettings.DefaultRepetitions,
                                                                            defaultNetworksCfg
                                                                            );
            return readoutLayerCfg;
        }


        private StateMachineSettings CreateStateMachineCfg()
        {
            NeuralPreprocessorSettings neuralPreprocessorCfg = CreatePreprocessorCfg("MainRes", "MainResStruct", "AnalogPool1", "AnalogPool2");
            ReadoutLayerSettings readoutCfg = CreateReadoutLayerCfg(0.1d, 2, 1000);
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
            string configFileName = ".\\Examples\\ForecastFromScratchSMConfig.xml";
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

            //Instance and training
            _log.Write("StateMachine training:");
            _log.Write("----------------------");
            _log.Write(string.Empty);
            //StateMachine instance
            StateMachine stateMachine = new StateMachine(stateMachineCfg);
            //StateMachine training
            TrainStateMachine(stateMachine, ".\\Data\\TTOO.csv", out double[] predictionInputVector);


            //Serialize StateMachine
            string serializationFileName = ".\\Examples\\ForecastFromScratchSM.dat";
            stateMachine.SaveToFile(serializationFileName);

            //Forecast
            ReadoutLayer.ReadoutData readoutData = stateMachine.ComputeReadoutData(predictionInputVector);
            string predictionReport = stateMachine.RL.GetForecastReport(readoutData.DataVector, 6);
            _log.Write("    Forecasts", false);
            _log.Write(predictionReport);
            _log.Write(string.Empty);

            //New StateMachine instance from the file
            StateMachine stateMachineNewInstance = StateMachine.LoadFromFile(serializationFileName);

            //New StateMachine instance forecast (exactly the same result as previously)
            readoutData = stateMachineNewInstance.ComputeReadoutData(predictionInputVector);
            predictionReport = stateMachineNewInstance.RL.GetForecastReport(readoutData.DataVector, 6);
            _log.Write("    Forecasts (new StateMachine instance)", false);
            _log.Write(predictionReport);
            _log.Write(string.Empty);


            return;
        }
    
    }//TTOOForecastFromScratch

}//Namespace
