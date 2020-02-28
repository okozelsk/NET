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

namespace Demo.DemoConsoleApp.Examples
{
    /// <summary>
    /// Example uses TTOO.csv from ./Data subfolder.
    /// Example code shows how to:
    ///   setup StateMachine from scratch,
    ///   store configuration xml,
    ///   train StateMachine,
    ///   serialize StateMachine,
    ///   load serialized StateMachine and forecast next value,
    ///   instantiate new StateMachine using stored configuration xml,
    ///   train new instance of the StateMachine,
    ///   forecast next value using new instance of the StateMachine
    /// </summary>
    public class ForecastFromScratch : BaseExample
    {
    
        /// <summary>
        /// Runs the example code.
        /// </summary>
        public void Run()
        {
            ////////////////////////////////////////////////////////////////////////////////
            // Input
            ////////////////////////////////////////////////////////////////////////////////
            //Definition of input external fields
            ExternalFieldsSettings externalFieldsCfg = new ExternalFieldsSettings(new ExternalFieldSettings("High", new RealFeatureFilterSettings()),
                                                                                  new ExternalFieldSettings("Low", new RealFeatureFilterSettings()),
                                                                                  new ExternalFieldSettings("Adj Close", new RealFeatureFilterSettings())
                                                                                  );
            //Definition of the input
            InputSettings InputCfg = new InputSettings(new FeedingContinuousSettings(FeedingContinuousSettings.DefaultBootCycles, true),
                                                       new FieldsSettings(externalFieldsCfg)
                                                       );


            ////////////////////////////////////////////////////////////////////////////////
            // RESERVOIR STRUCTURE
            ////////////////////////////////////////////////////////////////////////////////
            AnalogNeuronGroupSettings excitatoryGroup = new AnalogNeuronGroupSettings("ExcitatoryGrp",
                                                                                      NeuronCommon.NeuronRole.Excitatory,
                                                                                      1,
                                                                                      new TanHSettings(),
                                                                                      AnalogNeuronGroupSettings.DefaultFiringThreshold,
                                                                                      AnalogNeuronGroupSettings.DefaultSignalingRestriction,
                                                                                      1,
                                                                                      null,
                                                                                      new AnalogRetainmentSettings(1, new URandomValueSettings(0.25, 0.75))
                                                                                      );

            AnalogNeuronGroupSettings inhibitoryGroup = new AnalogNeuronGroupSettings("InhibitoryGrp",
                                                                                      NeuronCommon.NeuronRole.Inhibitory,
                                                                                      1,
                                                                                      new TanHSettings(),
                                                                                      AnalogNeuronGroupSettings.DefaultFiringThreshold,
                                                                                      AnalogNeuronGroupSettings.DefaultSignalingRestriction,
                                                                                      1,
                                                                                      null,
                                                                                      new AnalogRetainmentSettings(1, new URandomValueSettings(0.25, 0.75))
                                                                                      );

            PoolSettings pool1Cfg = new PoolSettings("Pool1",
                                                     new ProportionsSettings(5, 5, 5),
                                                     new NeuronGroupsSettings(excitatoryGroup, inhibitoryGroup),
                                                     new InterconnSettings(new RandomSchemaSettings(new ConnDistrFlatSettings(), 0.1d),
                                                                           new ChainSchemaSettings(1d)
                                                                           )
                                                     );

            PoolSettings pool2Cfg = new PoolSettings("Pool2",
                                                     new ProportionsSettings(5, 5, 5),
                                                     new NeuronGroupsSettings(excitatoryGroup, inhibitoryGroup),
                                                     new InterconnSettings(new RandomSchemaSettings(new ConnDistrFlatSettings(), 0.1d),
                                                                           new ChainSchemaSettings(1d)
                                                                           )
                                                     );
            InterPoolConnSettings interPool1ToPool2ConnCfg = new InterPoolConnSettings(new ConnDistrFlatSettings(),
                                                                                       "Pool1",
                                                                                       1d,
                                                                                       "Pool2",
                                                                                       0.02d,
                                                                                       false
                                                                                       );

            InterPoolConnSettings interPool2ToPool1ConnCfg = new InterPoolConnSettings(new ConnDistrFlatSettings(),
                                                                                       "Pool2",
                                                                                       1d,
                                                                                       "Pool1",
                                                                                       0.02d,
                                                                                       false
                                                                                       );

            ReservoirStructureSettings reservoirStructureCfg = new ReservoirStructureSettings("ResStruct1",
                                                                                              new PoolsSettings(pool1Cfg, pool2Cfg),
                                                                                              new InterPoolConnsSettings(interPool1ToPool2ConnCfg,
                                                                                                                         interPool2ToPool1ConnCfg
                                                                                                                         )
                                                                                              );

            ////////////////////////////////////////////////////////////////////////////////
            // RESERVOIR INSTANCE
            ////////////////////////////////////////////////////////////////////////////////
            InputUnitSettings highInputUnitCfg = new InputUnitSettings("High",
                                                                       new InputUnitConnsSettings(new InputUnitConnSettings("Pool1",
                                                                                                                            InputUnitConnSettings.DefaultAnalogCoding,
                                                                                                                            InputUnitConnSettings.DefaultOppositeAmplitude,
                                                                                                                            InputUnitConnSettings.DefaultSignalingRestriction,
                                                                                                                            null,
                                                                                                                            new AnalogTargetSettings(Synapse.SynapticTargetScope.All,
                                                                                                                                                     1,
                                                                                                                                                     new URandomValueSettings(0, 0.333d)
                                                                                                                                                     )
                                                                                                                            ),
                                                                                                  new InputUnitConnSettings("Pool2",
                                                                                                                            InputUnitConnSettings.DefaultAnalogCoding,
                                                                                                                            InputUnitConnSettings.DefaultOppositeAmplitude,
                                                                                                                            InputUnitConnSettings.DefaultSignalingRestriction,
                                                                                                                            null,
                                                                                                                            new AnalogTargetSettings(Synapse.SynapticTargetScope.All,
                                                                                                                                                     1,
                                                                                                                                                     new URandomValueSettings(0, 0.333d)
                                                                                                                                                     )
                                                                                                                            )
                                                                                                  )
                                                                       );

            InputUnitSettings lowInputUnitCfg = new InputUnitSettings("Low",
                                                                       new InputUnitConnsSettings(new InputUnitConnSettings("Pool1",
                                                                                                                            InputUnitConnSettings.DefaultAnalogCoding,
                                                                                                                            InputUnitConnSettings.DefaultOppositeAmplitude,
                                                                                                                            InputUnitConnSettings.DefaultSignalingRestriction,
                                                                                                                            null,
                                                                                                                            new AnalogTargetSettings(Synapse.SynapticTargetScope.All,
                                                                                                                                                     1,
                                                                                                                                                     new URandomValueSettings(0, 0.333d)
                                                                                                                                                     )
                                                                                                                            ),
                                                                                                  new InputUnitConnSettings("Pool2",
                                                                                                                            InputUnitConnSettings.DefaultAnalogCoding,
                                                                                                                            InputUnitConnSettings.DefaultOppositeAmplitude,
                                                                                                                            InputUnitConnSettings.DefaultSignalingRestriction,
                                                                                                                            null,
                                                                                                                            new AnalogTargetSettings(Synapse.SynapticTargetScope.All,
                                                                                                                                                     1,
                                                                                                                                                     new URandomValueSettings(0, 0.333d)
                                                                                                                                                     )
                                                                                                                            )
                                                                                                  )
                                                                       );
            InputUnitSettings adjCloseInputUnitCfg = new InputUnitSettings("Adj Close",
                                                                       new InputUnitConnsSettings(new InputUnitConnSettings("Pool1",
                                                                                                                            InputUnitConnSettings.DefaultAnalogCoding,
                                                                                                                            InputUnitConnSettings.DefaultOppositeAmplitude,
                                                                                                                            InputUnitConnSettings.DefaultSignalingRestriction,
                                                                                                                            null,
                                                                                                                            new AnalogTargetSettings(Synapse.SynapticTargetScope.All,
                                                                                                                                                     1,
                                                                                                                                                     new URandomValueSettings(0, 0.333d)
                                                                                                                                                     )
                                                                                                                            ),
                                                                                                  new InputUnitConnSettings("Pool2",
                                                                                                                            InputUnitConnSettings.DefaultAnalogCoding,
                                                                                                                            InputUnitConnSettings.DefaultOppositeAmplitude,
                                                                                                                            InputUnitConnSettings.DefaultSignalingRestriction,
                                                                                                                            null,
                                                                                                                            new AnalogTargetSettings(Synapse.SynapticTargetScope.All,
                                                                                                                                                     1,
                                                                                                                                                     new URandomValueSettings(0, 0.333d)
                                                                                                                                                     )
                                                                                                                            )
                                                                                                  )
                                                                       );

            SynapseSettings synapseCfg = new SynapseSettings(Synapse.SynapticDelayMethod.Random,
                                                             0,
                                                             Synapse.SynapticDelayMethod.Random,
                                                             0
                                                             );

            PredictorsSettings predictorsCfg = new PredictorsSettings(false,
                                                                      true,
                                                                      true,
                                                                      false,
                                                                      false,
                                                                      false,
                                                                      false,
                                                                      false,
                                                                      null
                                                                      );

            ReservoirInstanceSettings reservoirInstanceCfg = new ReservoirInstanceSettings("Main",
                                                                                           "ResStruct1",
                                                                                           new InputUnitsSettings(highInputUnitCfg,
                                                                                                                  lowInputUnitCfg,
                                                                                                                  adjCloseInputUnitCfg
                                                                                                                  ),
                                                                                           synapseCfg,
                                                                                           predictorsCfg
                                                                                           );

            ////////////////////////////////////////////////////////////////////////////////
            // NEURAL PREPROCESSOR
            ////////////////////////////////////////////////////////////////////////////////
            NeuralPreprocessorSettings neuralPreprocessorCfg = new NeuralPreprocessorSettings(InputCfg,
                                                                                              new ReservoirStructuresSettings(reservoirStructureCfg),
                                                                                              new ReservoirInstancesSettings(reservoirInstanceCfg),
                                                                                              NeuralPreprocessorSettings.DefaultPredictorsReductionRatio
                                                                                              );

            ////////////////////////////////////////////////////////////////////////////////
            // READOUT LAYER
            ////////////////////////////////////////////////////////////////////////////////
            ForecastNetworksSettings forecastNetworksCfg = new ForecastNetworksSettings(new FeedForwardNetworkSettings(new ElliotSettings(),
                                                                                                                       null,
                                                                                                                       new RPropTrainerSettings(2, 800)
                                                                                                                       )
                                                                                        );
            DefaultNetworksSettings defaultNetworksCfg = new DefaultNetworksSettings(null, forecastNetworksCfg);
            
            ReadoutUnitSettings highReadoutUnitCfg = new ReadoutUnitSettings("High", new ForecastTaskSettings(new RealFeatureFilterSettings()));
            ReadoutUnitSettings lowReadoutUnitCfg = new ReadoutUnitSettings("Low", new ForecastTaskSettings(new RealFeatureFilterSettings()));
            ReadoutUnitSettings adjCloseReadoutUnitCfg = new ReadoutUnitSettings("Adj Close", new ForecastTaskSettings(new RealFeatureFilterSettings()));

            ReadoutLayerSettings readoutLayerCfg = new ReadoutLayerSettings(new ReadoutUnitsSettings(highReadoutUnitCfg, lowReadoutUnitCfg, adjCloseReadoutUnitCfg),
                                                                            0.1d,
                                                                            ReadoutLayerSettings.DefaultFoldsNum,
                                                                            ReadoutLayerSettings.DefaultRepetitions,
                                                                            defaultNetworksCfg
                                                                            );

            ////////////////////////////////////////////////////////////////////////////////
            // STATE MACHINE
            ////////////////////////////////////////////////////////////////////////////////
            StateMachineSettings stateMachineCfg = new StateMachineSettings(neuralPreprocessorCfg, readoutLayerCfg);

            //StateMachine instance
            StateMachine stateMachine = new StateMachine(stateMachineCfg);
            
            //StateMachine training
            TrainStateMachine(stateMachine, ".\\Data\\TTOO.csv", out double[] predictionInputVector);

            //Create Examples directory
            Directory.CreateDirectory(".\\Examples");

            //Store StateMachine xml configuration
            string configXml = stateMachineCfg.GetXml(true).ToString();
            string configFileName = ".\\Examples\\ForecastFromScratchSMConfig.xml";
            using (StreamWriter writer = new StreamWriter(File.Create(configFileName)))
            {
                writer.Write(configXml);
            }

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

            //New StateMachine instance prediction
            readoutData = stateMachineNewInstance.ComputeReadoutData(predictionInputVector);
            predictionReport = stateMachineNewInstance.RL.GetForecastReport(readoutData.DataVector, 6);
            _log.Write("    Forecasts (new StateMachine instance)", false);
            _log.Write(predictionReport);
            _log.Write(string.Empty);





            _log.Write(stateMachineCfg.GetXml(true).ToString());

            return;
        }
    
    }//ForecastFromScratch

}//Namespace
