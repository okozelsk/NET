using RCNet.Extensions;
using RCNet.Neural.Activation;
using RCNet.Neural.Data.Filter;
using RCNet.Neural.Network.NonRecurrent.FF;
using RCNet.Neural.Network.SM.Preprocessing;
using RCNet.Neural.Network.SM.Preprocessing.Input;
using RCNet.Neural.Network.SM.Preprocessing.Neuron;
using RCNet.Neural.Network.SM.Preprocessing.Neuron.Predictor;
using RCNet.Neural.Network.SM.Preprocessing.Reservoir;
using RCNet.Neural.Network.SM.Preprocessing.Reservoir.Pool;
using RCNet.Neural.Network.SM.Preprocessing.Reservoir.Pool.NeuronGroup;
using RCNet.Neural.Network.SM.Preprocessing.Reservoir.Space3D;
using RCNet.Neural.Network.SM.Preprocessing.Reservoir.SynapseNS;
using RCNet.Neural.Network.SM.Readout;
using RCNet.RandomValue;
using System;
using System.Collections.Generic;

namespace RCNet.Neural.Network.SM
{
    /// <summary>
    /// Helper clas for the non-xml configuration of the StateMachine
    /// </summary>
    [Serializable]
    public class StateMachineDesigner
    {
        //Constants
        /// <summary>
        /// Default max sum of weights of input synapses per analog hidden neuron
        /// </summary>
        public const double DefaultAnalogMaxInputStrength = 1d;

        //Enums
        /// <summary>
        /// Design of the preprocessor's reservoir
        /// </summary>
        public enum ResDesign
        {
            /// <summary>
            /// ESN design. One reservoir consisting of one randomly interconnected pool of analog neurons
            /// </summary>
            PureESN,
            /// <summary>
            /// LSM design. One reservoir consisting of one randomly interconnected pool of spiking neurons
            /// </summary>
            PureLSM
        }//ResDesign

        /// <summary>
        /// Type of activation content
        /// </summary>
        public enum ActivationContent
        {
            /// <summary>
            /// Contains only analog neurons
            /// </summary>
            Analog,
            /// <summary>
            /// Contains only spiking neurons
            /// </summary>
            Spiking,
            /// <summary>
            /// Contains mix of analog and spiking neurons
            /// </summary>
            Mixed
        }//ActivationContent


        //Attribute properties
        /// <summary>
        /// Input configuration
        /// </summary>
        public InputEncoderSettings InputCfg { get; }

        /// <summary>
        /// Readout configuration
        /// </summary>
        public ReadoutLayerSettings ReadoutCfg { get; }


        //Attributes
        private readonly Random _rand;

        //Constructor
        /// <summary>
        /// Instantiates an uninitialized instance
        /// </summary>
        public StateMachineDesigner(InputEncoderSettings inputCfg, ReadoutLayerSettings readoutCfg)
        {
            InputCfg = inputCfg ?? throw new ArgumentNullException("inputCfg");
            ReadoutCfg = readoutCfg ?? throw new ArgumentNullException("readoutCfg");
            _rand = new Random(0);
            return;
        }

        /// <summary>
        /// Instantiates an uninitialized instance
        /// </summary>
        public StateMachineDesigner(ReadoutLayerSettings readoutCfg)
        {
            InputCfg = null;
            ReadoutCfg = readoutCfg ?? throw new ArgumentNullException("readoutCfg");
            _rand = new Random(0);
            return;
        }

        //Properties
        /// <summary>
        /// Indicates target configuration with bypassed neural preprocessor
        /// </summary>
        public bool BypassedNP { get { return (InputCfg == null); } }

        //Static methods
        /// <summary>
        /// Creates simplified configuration of the InputEncoder.
        /// All input fields are considered as the real values.
        /// </summary>
        /// <param name="feedingCfg">Input feeding configuration</param>
        /// <param name="spikesCoderCfg">Configuration of the input spikes coder</param>
        /// <param name="extFieldNameCollection">Names of the external input fields</param>
        /// <param name="routeToReadout">Specifies whether to route input values to readout</param>
        public static InputEncoderSettings CreateInputCfg(IFeedingSettings feedingCfg,
                                                          InputSpikesCoderSettings spikesCoderCfg,
                                                          IEnumerable<string> extFieldNameCollection,
                                                          bool routeToReadout = true
                                                          )
        {
            if (feedingCfg == null)
            {
                throw new ArgumentNullException("feedingCfg");
            }
            if (spikesCoderCfg == null)
            {
                spikesCoderCfg = new InputSpikesCoderSettings();
            }
            List<ExternalFieldSettings> extFieldCollection = new List<ExternalFieldSettings>();
            foreach (string name in extFieldNameCollection)
            {
                extFieldCollection.Add(new ExternalFieldSettings(name, new RealFeatureFilterSettings(), routeToReadout));
            }
            ExternalFieldsSettings extFieldsCfg = new ExternalFieldsSettings(extFieldCollection);
            VaryingFieldsSettings fieldsCfg = new VaryingFieldsSettings(spikesCoderCfg, extFieldsCfg, null, null, routeToReadout);
            return new InputEncoderSettings(feedingCfg, fieldsCfg);
        }

        /// <summary>
        /// Creates configuration of the InputEncoder.
        /// Contains only external input fields.
        /// </summary>
        /// <param name="feedingCfg">Input feeding configuration</param>
        /// <param name="spikesCoderCfg">Configuration of the input spikes coder</param>
        /// <param name="routeToReadout">Specifies whether to route input values to readout</param>
        /// <param name="externalFieldCfg">External input field configuration</param>
        public static InputEncoderSettings CreateInputCfg(IFeedingSettings feedingCfg,
                                                          InputSpikesCoderSettings spikesCoderCfg,
                                                          bool routeToReadout,
                                                          params ExternalFieldSettings[] externalFieldCfg
                                                          )
        {
            if (feedingCfg == null)
            {
                throw new ArgumentNullException("feedingCfg");
            }
            return new InputEncoderSettings(feedingCfg,
                                            new VaryingFieldsSettings(spikesCoderCfg,
                                                                      new ExternalFieldsSettings(externalFieldCfg),
                                                                      null,
                                                                      null,
                                                                      routeToReadout
                                                                      )
                                            );
        }

        /// <summary>
        /// Creates configuration of single output layer FF network structure with associated resilient back propagation trainer
        /// </summary>
        /// <param name="aFnCfg">Activation of output layer</param>
        /// <param name="numOfAttempts">Number of regression attempts. Each readout network will try to learn numOfAttempts times</param>
        /// <param name="numOfEpochs">Number of training epochs within an attempt</param>
        public static FeedForwardNetworkSettings CreateSingleLayerRegrNet(RCNetBaseSettings aFnCfg, int numOfAttempts, int numOfEpochs)
        {
            return new FeedForwardNetworkSettings(aFnCfg, null, new RPropTrainerSettings(numOfAttempts, numOfEpochs));
        }

        /// <summary>
        /// Creates configuration of single output layer FF network structure with associated resilient back propagation trainer
        /// </summary>
        /// <param name="hiddenLayerSize">Number of hidden layer neurons</param>
        /// <param name="hiddenLayerAFnCfg">Activation of hidden layer</param>
        /// <param name="numOfHiddenLayers">Number of hidden layers</param>
        /// <param name="numOfAttempts">Number of regression attempts. Each readout network will try to learn numOfAttempts times</param>
        /// <param name="numOfEpochs">Number of training epochs within an attempt</param>
        public static FeedForwardNetworkSettings CreateMultiLayerRegrNet(int hiddenLayerSize, RCNetBaseSettings hiddenLayerAFnCfg, int numOfHiddenLayers, int numOfAttempts, int numOfEpochs)
        {
            List<HiddenLayerSettings> hiddenLayerCollection = new List<HiddenLayerSettings>(numOfHiddenLayers);
            for (int i = 0; i < numOfHiddenLayers; i++)
            {
                hiddenLayerCollection.Add(new HiddenLayerSettings(hiddenLayerSize, hiddenLayerAFnCfg));
            }
            HiddenLayersSettings hiddenLayersCfg = new HiddenLayersSettings(hiddenLayerCollection);
            return new FeedForwardNetworkSettings(new IdentitySettings(), hiddenLayersCfg, new RPropTrainerSettings(numOfAttempts, numOfEpochs));
        }

        /// <summary>
        /// Creates readout layer configuration to solve forecast task
        /// </summary>
        /// <param name="netCfg">FF network configuration to be associated with readout units</param>
        /// <param name="testDataRatio">Specifies what part of available data to be used as test data</param>
        /// <param name="repetitions">Number of repetitions of the folds regression</param>
        /// <param name="unitName">Readout unit name</param>
        public static ReadoutLayerSettings CreateForecastReadoutCfg(FeedForwardNetworkSettings netCfg,
                                                                    double testDataRatio,
                                                                    int repetitions,
                                                                    params string[] unitName
                                                                    )
        {
            if (netCfg == null)
            {
                throw new ArgumentNullException("netCfg");
            }
            List<ReadoutUnitSettings> unitCfgCollection = new List<ReadoutUnitSettings>();
            foreach (string name in unitName)
            {
                unitCfgCollection.Add(new ReadoutUnitSettings(name, new ForecastTaskSettings(new RealFeatureFilterSettings())));
            }
            return new ReadoutLayerSettings(new ReadoutUnitsSettings(unitCfgCollection),
                                            testDataRatio,
                                            ReadoutLayerSettings.AutoFolds,
                                            repetitions,
                                            new DefaultNetworksSettings(null, new ForecastNetworksSettings(netCfg))
                                            );
        }

        /// <summary>
        /// Creates readout layer configuration to solve classification task
        /// </summary>
        /// <param name="netCfg">FF network configuration to be associated with readout units</param>
        /// <param name="testDataRatio">Specifies what part of available data to be used as test data</param>
        /// <param name="repetitions">Number of repetitions of the folds regression</param>
        /// <param name="oneWinnerGroupName">Name of the "one winner" group encapsulating classification readout units</param>
        /// <param name="unitName">Readout unit name</param>
        public static ReadoutLayerSettings CreateClassificationReadoutCfg(FeedForwardNetworkSettings netCfg,
                                                                          double testDataRatio,
                                                                          int repetitions,
                                                                          string oneWinnerGroupName,
                                                                          params string[] unitName
                                                                          )
        {
            if (netCfg == null)
            {
                throw new ArgumentNullException("netCfg");
            }
            List<ReadoutUnitSettings> unitCfgCollection = new List<ReadoutUnitSettings>();
            foreach (string name in unitName)
            {
                unitCfgCollection.Add(new ReadoutUnitSettings(name, new ClassificationTaskSettings(oneWinnerGroupName)));
            }
            return new ReadoutLayerSettings(new ReadoutUnitsSettings(unitCfgCollection),
                                            testDataRatio,
                                            ReadoutLayerSettings.AutoFolds,
                                            repetitions,
                                            new DefaultNetworksSettings(new ClassificationNetworksSettings(netCfg), null)
                                            );
        }

        //Methods
        /// <summary>
        /// Builds name of the specified activation function
        /// </summary>
        /// <param name="activationCfg">Activation function configuration</param>
        private string GetActivationName(RCNetBaseSettings activationCfg)
        {
            IActivationFunction aFn = ActivationFactory.Create(activationCfg, _rand);
            return aFn.TypeOfActivation.ToString() + "-" + aFn.GetType().Name.Replace("Settings", string.Empty);
        }

        /// <summary>
        /// Builds name of neuron group
        /// </summary>
        /// <param name="activationCfg">Activation function configuration</param>
        private string GetNeuronGroupName(RCNetBaseSettings activationCfg)
        {
            return "Grp-" + GetActivationName(activationCfg);
        }

        /// <summary>
        /// Builds name of the pool
        /// </summary>
        /// <param name="activationContent">Specifies type of activation content</param>
        /// <param name="poolIdx">Zero based index of the pool</param>
        private string GetPoolName(ActivationContent activationContent, int poolIdx)
        {
            return activationContent.ToString() + "Pool" + (poolIdx + 1).ToString();
        }

        /// <summary>
        /// Builds name of the reservoir structure
        /// </summary>
        /// <param name="activationContent">Specifies type of activation content</param>
        /// <param name="resStructIdx">Zero based index of the reservoir structure</param>
        private string GetResStructName(ActivationContent activationContent, int resStructIdx)
        {
            return "ResStruct-" + activationContent.ToString() + "-Cfg" + (resStructIdx + 1).ToString();
        }

        /// <summary>
        /// Builds name of the reservoir instance
        /// </summary>
        /// <param name="resDesign">Design of the reservoir</param>
        /// <param name="resInstIdx">Zero based index of the reservoir instance</param>
        private string GetResInstName(ResDesign resDesign, int resInstIdx)
        {
            return resDesign.ToString() + "-Reservoir" + (resInstIdx + 1).ToString();
        }

        /// <summary>
        /// Creates configuration of group of analog neurons having specified analog activation.
        /// </summary>
        /// <param name="activationCfg">Activation function configuration</param>
        /// <param name="predictorsCfg">Predictors configuration</param>
        /// <param name="maxAbsBias">Maximum absolute value of the bias (0 means bias is not required)</param>
        /// <param name="maxRetainmentStrength">Maximum retainment strength (0 means retainment property is not required)</param>
        private AnalogNeuronGroupSettings CreateAnalogGroup(RCNetBaseSettings activationCfg,
                                                            PredictorsProviderSettings predictorsCfg,
                                                            double maxAbsBias = 0d,
                                                            double maxRetainmentStrength = 0d
                                                            )
        {
            //Bias configuration
            RandomValueSettings biasCfg = maxAbsBias == 0 ? null : new RandomValueSettings(-maxAbsBias, maxAbsBias);
            //Retainment configuration
            const double RetainmentDensity = 1d;
            RetainmentSettings retainmentCfg = maxRetainmentStrength == 0 ? null : new RetainmentSettings(RetainmentDensity, new URandomValueSettings(0, maxRetainmentStrength));
            //Create neuron group configuration
            AnalogNeuronGroupSettings groupCfg = new AnalogNeuronGroupSettings(GetNeuronGroupName(activationCfg),
                                                                               1d,
                                                                               activationCfg,
                                                                               predictorsCfg,
                                                                               AnalogNeuronGroupSettings.DefaultFiringThreshold,
                                                                               AnalogNeuronGroupSettings.DefaultThresholdMaxRefDeepness,
                                                                               biasCfg,
                                                                               retainmentCfg
                                                                               );
            return groupCfg;
        }

        /// <summary>
        /// Creates configuration of group of spiking neurons having specified spiking activation.
        /// </summary>
        /// <param name="activationCfg">Activation function configuration</param>
        /// <param name="predictorsCfg">Predictors configuration</param>
        /// <param name="heCfg">Configuration of the homogenous excitability</param>
        /// <param name="steadyBias">Constant bias (0 means bias is not required)</param>
        private SpikingNeuronGroupSettings CreateSpikingGroup(RCNetBaseSettings activationCfg,
                                                              PredictorsProviderSettings predictorsCfg,
                                                              HomogenousExcitabilitySettings heCfg,
                                                              double steadyBias = 0d
                                                              )
        {
            //Bias configuration
            RandomValueSettings biasCfg = steadyBias == 0 ? null : new RandomValueSettings(steadyBias, steadyBias);
            //Create neuron group configuration
            SpikingNeuronGroupSettings groupCfg = new SpikingNeuronGroupSettings(GetNeuronGroupName(activationCfg),
                                                                                 1d,
                                                                                 activationCfg,
                                                                                 predictorsCfg,
                                                                                 heCfg,
                                                                                 biasCfg
                                                                                 );
            return groupCfg;
        }


        /// <summary>
        /// Creates StateMachine configuration following pure ESN design
        /// </summary>
        /// <param name="totalSize">Total number of hidden neurons</param>
        /// <param name="maxInputStrength">Max sum of weights of input synapses per analog hidden neuron (see the constant DefaultAnalogMaxInputStrength)</param>
        /// <param name="inputConnectionDensity">Density of the input field connections to hidden neurons</param>
        /// <param name="maxInputDelay">Maximum delay of input synapse</param>
        /// <param name="interconnectionDensity">Density of the hidden neurons interconnection</param>
        /// <param name="maxInternalDelay">Maximum delay of internal synapse</param>
        /// <param name="maxAbsBias">Maximum absolute value of the bias (0 means bias is not required)</param>
        /// <param name="maxRetainmentStrength">Maximum retainment strength (0 means retainment property is not required)</param>
        /// <param name="predictorsProviderCfg">Predictors configuration</param>
        public StateMachineSettings CreatePureESNCfg(int totalSize,
                                                     double maxInputStrength,
                                                     double inputConnectionDensity,
                                                     int maxInputDelay,
                                                     double interconnectionDensity,
                                                     int maxInternalDelay,
                                                     double maxAbsBias,
                                                     double maxRetainmentStrength,
                                                     PredictorsProviderSettings predictorsProviderCfg
                                                     )
        {
            //Check NP is not bypassed
            if(BypassedNP)
            {
                throw new InvalidOperationException("Neural preprocessor is bypassed thus ESN design can't be created.");
            }
            //Default ESN activation
            RCNetBaseSettings aFnCfg = new TanHSettings();
            //One neuron group
            AnalogNeuronGroupSettings grp = CreateAnalogGroup(aFnCfg, predictorsProviderCfg, maxAbsBias, maxRetainmentStrength);
            //Simple analog pool
            PoolSettings poolCfg = new PoolSettings(GetPoolName(ActivationContent.Analog, 0),
                                                    new ProportionsSettings(totalSize, 1, 1),
                                                    new NeuronGroupsSettings(grp),
                                                    new InterconnSettings(new RandomSchemaSettings(interconnectionDensity))
                                                    );
            //Simple reservoir structure
            ReservoirStructureSettings resStructCfg = new ReservoirStructureSettings(GetResStructName(ActivationContent.Analog, 0),
                                                                                     new PoolsSettings(poolCfg)
                                                                                     );
            //Input connections configuration
            List<InputConnSettings> inputConns = new List<InputConnSettings>(InputCfg.VaryingFieldsCfg.ExternalFieldsCfg.FieldCfgCollection.Count);
            foreach (ExternalFieldSettings fieldCfg in InputCfg.VaryingFieldsCfg.ExternalFieldsCfg.FieldCfgCollection)
            {
                InputConnSettings inputConnCfg = new InputConnSettings(fieldCfg.Name,
                                                                       poolCfg.Name,
                                                                       0,
                                                                       inputConnectionDensity
                                                                       );
                inputConns.Add(inputConnCfg);
            }
            //Synapse general configuration
            SynapseATInputSettings synapseATInputSettings = new SynapseATInputSettings(Synapse.SynapticDelayMethod.Random, maxInputDelay, new AnalogSourceSettings(new URandomValueSettings(0d, maxInputStrength)));
            SynapseATIndifferentSettings synapseATIndifferentSettings = new SynapseATIndifferentSettings(Synapse.SynapticDelayMethod.Random, maxInternalDelay);
            SynapseATSettings synapseATCfg = new SynapseATSettings(SynapseATSettings.DefaultSpectralRadiusNum, synapseATInputSettings, synapseATIndifferentSettings);
            SynapseSettings synapseCfg = new SynapseSettings(null, synapseATCfg);

            //Create reservoir instance
            ReservoirInstanceSettings resInstCfg = new ReservoirInstanceSettings(GetResInstName(ResDesign.PureESN, 0),
                                                                                 resStructCfg.Name,
                                                                                 new InputConnsSettings(inputConns),
                                                                                 synapseCfg
                                                                                 );
            //Build and return SM configuration
            return new StateMachineSettings(new NeuralPreprocessorSettings(InputCfg,
                                                                           new ReservoirStructuresSettings(resStructCfg),
                                                                           new ReservoirInstancesSettings(resInstCfg)
                                                                           ),
                                            ReadoutCfg
                                            );
        }

        /// <summary>
        /// Creates StateMachine configuration following pure LSM design
        /// </summary>
        /// <param name="totalSize">Total number of hidden neurons</param>
        /// <param name="aFnCfg">Spiking activation function configuration</param>
        /// <param name="hes">Homogenous excitability configuration</param>
        /// <param name="inputConnectionDensity">Density of the input field connections to hidden neurons</param>
        /// <param name="maxInputDelay">Maximum delay of input synapse</param>
        /// <param name="interconnectionDensity">Density of the hidden neurons interconnection</param>
        /// <param name="maxInternalDelay">Maximum delay of internal synapse</param>
        /// <param name="steadyBias">Constant bias (0 means bias is not required)</param>
        /// <param name="predictorsCfg">Predictors configuration</param>
        public StateMachineSettings CreatePureLSMCfg(int totalSize,
                                                     RCNetBaseSettings aFnCfg,
                                                     HomogenousExcitabilitySettings hes,
                                                     double inputConnectionDensity,
                                                     int maxInputDelay,
                                                     double interconnectionDensity,
                                                     int maxInternalDelay,
                                                     double steadyBias,
                                                     PredictorsProviderSettings predictorsCfg
                                                     )
        {
            //Check NP is not bypassed
            if (BypassedNP)
            {
                throw new InvalidOperationException("Neural preprocessor is bypassed thus LSM design can't be created.");
            }
            //Activation check
            if (ActivationFactory.Create(aFnCfg, new Random()).TypeOfActivation != ActivationType.Spiking)
            {
                throw new ArgumentException("Specified activation must be spiking.", "aFnCfg");
            }
            //One neuron group
            SpikingNeuronGroupSettings grp = CreateSpikingGroup(aFnCfg, predictorsCfg, hes, steadyBias);
            //Simple spiking pool
            PoolSettings poolCfg = new PoolSettings(GetPoolName(ActivationContent.Spiking, 0),
                                                    new ProportionsSettings(totalSize, 1, 1),
                                                    new NeuronGroupsSettings(grp),
                                                    new InterconnSettings(new RandomSchemaSettings(interconnectionDensity, 0d, false, false))
                                                    );
            //Simple reservoir structure
            ReservoirStructureSettings resStructCfg = new ReservoirStructureSettings(GetResStructName(ActivationContent.Spiking, 0),
                                                                                     new PoolsSettings(poolCfg)
                                                                                     );
            //Input connections configuration
            List<InputConnSettings> inputConns = new List<InputConnSettings>(InputCfg.VaryingFieldsCfg.ExternalFieldsCfg.FieldCfgCollection.Count);
            foreach (ExternalFieldSettings fieldCfg in InputCfg.VaryingFieldsCfg.ExternalFieldsCfg.FieldCfgCollection)
            {
                InputConnSettings inputConnCfg = new InputConnSettings(fieldCfg.Name,
                                                                       poolCfg.Name,
                                                                       inputConnectionDensity,
                                                                       0
                                                                       );
                inputConns.Add(inputConnCfg);
            }
            //Synapse general configuration
            SpikingSourceSTInputSettings spikingSourceSTInputSettings = new SpikingSourceSTInputSettings(new URandomValueSettings(0,1), new PlasticitySTInputSettings(new NonlinearDynamicsSTInputSettings()));
            SpikingSourceSTExcitatorySettings spikingSourceSTExcitatorySettings = new SpikingSourceSTExcitatorySettings(new URandomValueSettings(0, 1), new PlasticitySTExcitatorySettings(new NonlinearDynamicsSTExcitatorySettings()));
            SpikingSourceSTInhibitorySettings spikingSourceSTInhibitorySettings = new SpikingSourceSTInhibitorySettings(new URandomValueSettings(0, 1), new PlasticitySTInhibitorySettings(new NonlinearDynamicsSTInhibitorySettings()));
            SynapseSTInputSettings synapseSTInputSettings = new SynapseSTInputSettings(Synapse.SynapticDelayMethod.Random, maxInputDelay, null, spikingSourceSTInputSettings);
            SynapseSTExcitatorySettings synapseSTExcitatorySettings = new SynapseSTExcitatorySettings(Synapse.SynapticDelayMethod.Random, maxInternalDelay, 4, null, spikingSourceSTExcitatorySettings);
            SynapseSTInhibitorySettings synapseSTInhibitorySettings = new SynapseSTInhibitorySettings(Synapse.SynapticDelayMethod.Random, maxInternalDelay, 1, null, spikingSourceSTInhibitorySettings);
            SynapseSTSettings synapseSTCfg = new SynapseSTSettings(synapseSTInputSettings, synapseSTExcitatorySettings, synapseSTInhibitorySettings);
            SynapseSettings synapseCfg = new SynapseSettings(synapseSTCfg, null);

            //Create reservoir instance
            ReservoirInstanceSettings resInstCfg = new ReservoirInstanceSettings(GetResInstName(ResDesign.PureLSM, 0),
                                                                                 resStructCfg.Name,
                                                                                 new InputConnsSettings(inputConns),
                                                                                 synapseCfg
                                                                                 );
            //Build and return SM configuration
            return new StateMachineSettings(new NeuralPreprocessorSettings(InputCfg,
                                                                           new ReservoirStructuresSettings(resStructCfg),
                                                                           new ReservoirInstancesSettings(resInstCfg)
                                                                           ),
                                            ReadoutCfg
                                            );
        }

        /// <summary>
        /// Creates StateMachine configuration having bypassed neural preprocessing
        /// </summary>
        /// <returns></returns>
        public StateMachineSettings CreateBypassedCfg()
        {
            //Build and return SM configuration
            return new StateMachineSettings(null,
                                            ReadoutCfg
                                            );
        }


    }//StateMachineDesigner

}//Namespace
