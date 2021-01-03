using RCNet.Neural.Activation;
using RCNet.Neural.Data.Filter;
using RCNet.Neural.Network.NonRecurrent;
using RCNet.Neural.Network.NonRecurrent.FF;
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
using System.Collections.Generic;
using System.Linq;

namespace RCNet.Neural.Network.SM
{
    /// <summary>
    /// Implements the state machine designer.
    /// </summary>
    /// <remarks>
    /// Helper class for the non-xml configuration of the state machine.
    /// </remarks>
    [Serializable]
    public class StateMachineDesigner
    {
        //Constants
        /// <summary>
        /// The default max sum of weights of the input synapses per an hidden analog neuron.
        /// </summary>
        public const double DefaultAnalogMaxInputStrength = 1d;

        //Enums
        /// <summary>
        /// The type of the reservoir design.
        /// </summary>
        public enum ResDesign
        {
            /// <summary>
            /// The pure ESN design. The single reservoir consisting of one randomly interconnected pool of analog neurons.
            /// </summary>
            PureESN,
            /// <summary>
            /// The pure LSM design. The single reservoir consisting of one randomly interconnected pool of spiking neurons.
            /// </summary>
            PureLSM
        }//ResDesign

        /// <summary>
        /// The type of activation content.
        /// </summary>
        public enum ActivationContent
        {
            /// <summary>
            /// The analog neurons only.
            /// </summary>
            Analog,
            /// <summary>
            /// The spiking neurons only.
            /// </summary>
            Spiking,
            /// <summary>
            /// The mixed analog and spiking neurons.
            /// </summary>
            Mixed
        }//ActivationContent


        //Attribute properties
        /// <summary>
        /// The configuration of the input encoder.
        /// </summary>
        public InputEncoderSettings InputEncoderCfg { get; }

        /// <summary>
        /// The configuration of the readout layer.
        /// </summary>
        public ReadoutLayerSettings ReadoutLayerCfg { get; }


        //Attributes
        private readonly Random _rand;

        //Constructor
        /// <summary>
        /// Creates an uninitialized instance.
        /// </summary>
        public StateMachineDesigner(InputEncoderSettings inputEncoderCfg,
                                    ReadoutLayerSettings readoutLayerCfg
                                    )
        {
            InputEncoderCfg = inputEncoderCfg ?? throw new ArgumentNullException("inputCfg");
            ReadoutLayerCfg = readoutLayerCfg ?? throw new ArgumentNullException("readoutCfg");
            _rand = new Random(0);
            return;
        }

        /// <summary>
        /// Creates an uninitialized instance.
        /// </summary>
        public StateMachineDesigner(ReadoutLayerSettings readoutLayerCfg)
        {
            InputEncoderCfg = null;
            ReadoutLayerCfg = readoutLayerCfg ?? throw new ArgumentNullException("readoutCfg");
            _rand = new Random(0);
            return;
        }

        //Properties
        /// <summary>
        /// Indicates the bypassed neural preprocessor.
        /// </summary>
        public bool BypassedNP { get { return (InputEncoderCfg == null); } }

        //Static methods
        /// <summary>
        /// Creates the simplified configuration of the input encoder.
        /// </summary>
        /// <remarks>
        /// Supports only external input fields of the the real numbers.
        /// </remarks>
        /// <param name="feedingCfg">The configuration of the input feeding.</param>
        /// <param name="spikesCoderCfg">The configuration of the input spikes coder.</param>
        /// <param name="extFieldNameCollection">The collection of the external input field names.</param>
        /// <param name="routeToReadout">Specifies whether to route inputs to readout layer.</param>
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
        /// Creates the simplified configuration of the input encoder.
        /// </summary>
        /// <remarks>
        /// Supports only external input fields.
        /// </remarks>
        /// <param name="feedingCfg">The configuration of the input feeding.</param>
        /// <param name="spikesCoderCfg">The configuration of the input spikes coder.</param>
        /// <param name="routeToReadout">Specifies whether to route inputs to readout layer.</param>
        /// <param name="externalFieldCfg">The external input field configurations.</param>
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
        /// Creates the configuration of the feed forward network having an output layer and associated the resilient backpropagation trainer.
        /// </summary>
        /// <param name="aFnCfg">The configuration of the output layer activation.</param>
        /// <param name="numOfAttempts">The number of regression attempts.</param>
        /// <param name="numOfEpochs">The number of training epochs within an attempt.</param>
        public static FeedForwardNetworkSettings CreateSingleLayerFFNetCfg(IActivationSettings aFnCfg, int numOfAttempts, int numOfEpochs)
        {
            return new FeedForwardNetworkSettings(aFnCfg, null, new RPropTrainerSettings(numOfAttempts, numOfEpochs));
        }

        /// <summary>
        /// Creates the configuration of the feed forward network having the hidden layers, the Identity output layer and associated the resilient backpropagation trainer.
        /// </summary>
        /// <param name="hiddenLayerSize">The number of hidden layer neurons.</param>
        /// <param name="hiddenLayerAFnCfg">The configuration of the hidden layer activation.</param>
        /// <param name="numOfHiddenLayers">The number of hidden layers.</param>
        /// <param name="numOfAttempts">The number of regression attempts.</param>
        /// <param name="numOfEpochs">The number of training epochs within an attempt.</param>
        public static FeedForwardNetworkSettings CreateMultiLayerFFNetCfg(int hiddenLayerSize,
                                                                          IActivationSettings hiddenLayerAFnCfg,
                                                                          int numOfHiddenLayers,
                                                                          int numOfAttempts,
                                                                          int numOfEpochs
                                                                          )
        {
            List<HiddenLayerSettings> hiddenLayerCollection = new List<HiddenLayerSettings>(numOfHiddenLayers);
            for (int i = 0; i < numOfHiddenLayers; i++)
            {
                hiddenLayerCollection.Add(new HiddenLayerSettings(hiddenLayerSize, hiddenLayerAFnCfg));
            }
            HiddenLayersSettings hiddenLayersCfg = new HiddenLayersSettings(hiddenLayerCollection);
            return new FeedForwardNetworkSettings(new AFAnalogIdentitySettings(), hiddenLayersCfg, new RPropTrainerSettings(numOfAttempts, numOfEpochs));
        }

        /// <summary>
        /// Creates the simplified configuration of the readout layer to solve the forecast task.
        /// </summary>
        /// <remarks>
        /// Supports the real numbers output only.
        /// </remarks>
        /// <param name="crossvalidationCfg">The crossvalidation configuration.</param>
        /// <param name="netCfg">The configuration of the FF network to be used in the cluster(s).</param>
        /// <param name="clusterChainLength">The number of chained clusters.</param>
        /// <param name="unitName">The readout unit names (the output field names).</param>
        public static ReadoutLayerSettings CreateForecastReadoutCfg(CrossvalidationSettings crossvalidationCfg,
                                                                    FeedForwardNetworkSettings netCfg,
                                                                    int clusterChainLength,
                                                                    params string[] unitName
                                                                    )
        {
            if (netCfg == null)
            {
                throw new ArgumentNullException("netL1Cfg");
            }
            List<ReadoutUnitSettings> unitCfgCollection = new List<ReadoutUnitSettings>();
            foreach (string name in unitName)
            {
                unitCfgCollection.Add(new ReadoutUnitSettings(name, new ForecastTaskSettings()));
            }
            TNRNetClusterRealNetworksSettings netsCfg = new TNRNetClusterRealNetworksSettings(netCfg);
            TNRNetClusterRealSettings clusterCfg = new TNRNetClusterRealSettings(netsCfg, new TNRNetClusterRealWeightsSettings());
            List<TNRNetClusterRealSettings> clusterCfgCollection = new List<TNRNetClusterRealSettings>();
            for (int i = 0; i < clusterChainLength; i++)
            {
                clusterCfgCollection.Add(clusterCfg);
            }
            TNRNetClustersRealSettings clustersCfg = new TNRNetClustersRealSettings(clusterCfgCollection);
            TNRNetClusterChainRealSettings clusterChainCfg = new TNRNetClusterChainRealSettings(crossvalidationCfg, clustersCfg);
            TaskDefaultsSettings taskDefaultsCfg = new TaskDefaultsSettings(null, clusterChainCfg);

            return new ReadoutLayerSettings(taskDefaultsCfg,
                                            new ReadoutUnitsSettings(unitCfgCollection),
                                            null
                                            );
        }

        /// <summary>
        /// Creates the simplified configuration of the readout layer to solve the classification task.
        /// </summary>
        /// <remarks>
        /// Supports the probabilistic output only.
        /// </remarks>
        /// <param name="crossvalidationCfg">The crossvalidation configuration.</param>
        /// <param name="netCfg">The configuration of the FF network to be used in the cluster(s).</param>
        /// <param name="clusterChainLength">The number of chained clusters.</param>
        /// <param name="oneTakesAllGroupName">The name of the "One Takes All" group in case of multiple classes or use the "NA" code when there is only the single class.</param>
        /// <param name="unitName">The readout unit names (the names of the classes).</param>
        public static ReadoutLayerSettings CreateClassificationReadoutCfg(CrossvalidationSettings crossvalidationCfg,
                                                                          FeedForwardNetworkSettings netCfg,
                                                                          int clusterChainLength,
                                                                          string oneTakesAllGroupName,
                                                                          params string[] unitName
                                                                          )
        {
            if (netCfg == null)
            {
                throw new ArgumentNullException("netCfg");
            }
            List<string> readoutUnitNames = new List<string>(unitName.AsEnumerable());
            oneTakesAllGroupName = readoutUnitNames.Count > 1 ? oneTakesAllGroupName : "NA";
            List<ReadoutUnitSettings> unitCfgCollection = new List<ReadoutUnitSettings>();
            foreach (string name in readoutUnitNames)
            {
                unitCfgCollection.Add(new ReadoutUnitSettings(name, new ClassificationTaskSettings(oneTakesAllGroupName)));
            }

            TNRNetClusterSingleBoolNetworksSettings netsCfg = new TNRNetClusterSingleBoolNetworksSettings(netCfg);
            TNRNetClusterSingleBoolSettings clusterCfg = new TNRNetClusterSingleBoolSettings(netsCfg, new TNRNetClusterSingleBoolWeightsSettings());
            List<TNRNetClusterSingleBoolSettings> clusterCfgCollection = new List<TNRNetClusterSingleBoolSettings>();
            for (int i = 0; i < clusterChainLength; i++)
            {
                clusterCfgCollection.Add(clusterCfg);
            }
            TNRNetClustersSingleBoolSettings clustersCfg = new TNRNetClustersSingleBoolSettings(clusterCfgCollection);
            TNRNetClusterChainSingleBoolSettings clusterChainCfg = new TNRNetClusterChainSingleBoolSettings(crossvalidationCfg, clustersCfg);
            TaskDefaultsSettings taskDefaultsCfg = new TaskDefaultsSettings(clusterChainCfg, null);
            return new ReadoutLayerSettings(taskDefaultsCfg,
                                            new ReadoutUnitsSettings(unitCfgCollection),
                                            readoutUnitNames.Count > 1 ? new OneTakesAllGroupsSettings(new OneTakesAllGroupSettings(oneTakesAllGroupName, new OneTakesAllBasicDecisionSettings())) : null
                                            );
        }

        //Methods
        /// <summary>
        /// Builds the name of the specified activation function.
        /// </summary>
        /// <param name="activationCfg">The activation function configuration.</param>
        private string BuildActivationName(IActivationSettings activationCfg)
        {
            IActivation aFn = ActivationFactory.CreateAF(activationCfg, _rand);
            return aFn.TypeOfActivation.ToString() + "-" + aFn.GetType().Name.Replace("Settings", string.Empty);
        }

        /// <summary>
        /// Builds the name of the neuron group.
        /// </summary>
        /// <param name="activationCfg">The activation function configuration.</param>
        private string BuildNeuronGroupName(IActivationSettings activationCfg)
        {
            return "Grp-" + BuildActivationName(activationCfg);
        }

        /// <summary>
        /// Builds the name of the neuron pool.
        /// </summary>
        /// <param name="activationContent">Specifies the type of activation content.</param>
        /// <param name="poolIdx">The zero-based index of the pool.</param>
        private string BuildPoolName(ActivationContent activationContent, int poolIdx)
        {
            return activationContent.ToString() + "Pool" + (poolIdx + 1).ToString();
        }

        /// <summary>
        /// Builds the name of the reservoir structure.
        /// </summary>
        /// <param name="activationContent">Specifies the type of activation content.</param>
        /// <param name="resStructIdx">The zero-based index of the reservoir structure.</param>
        private string BuildResStructName(ActivationContent activationContent, int resStructIdx)
        {
            return "ResStruct-" + activationContent.ToString() + "-Cfg" + (resStructIdx + 1).ToString();
        }

        /// <summary>
        /// Builds the name of the reservoir instance.
        /// </summary>
        /// <param name="resDesign">The type of reservoir design.</param>
        /// <param name="resInstIdx">The zero-based index of the reservoir instance.</param>
        private string GetResInstName(ResDesign resDesign, int resInstIdx)
        {
            return resDesign.ToString() + "-Reservoir" + (resInstIdx + 1).ToString();
        }

        /// <summary>
        /// Creates the configuration of neuron group having the specified analog activation function.
        /// </summary>
        /// <param name="activationCfg">The activation function configuration.</param>
        /// <param name="predictorsCfg">The predictors provider configuration.</param>
        /// <param name="maxAbsBias">The maximum absolute value of the bias (0 means no bias).</param>
        /// <param name="maxRetainmentStrength">The maximum retainment strength (0 means no retainment).</param>
        private AnalogNeuronGroupSettings CreateAnalogGroup(IActivationSettings activationCfg,
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
            AnalogNeuronGroupSettings groupCfg = new AnalogNeuronGroupSettings(BuildNeuronGroupName(activationCfg),
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
        /// Creates the configuration of neuron group having the specified spiking activation function.
        /// </summary>
        /// <param name="activationCfg">The activation function configuration.</param>
        /// <param name="predictorsCfg">The predictors provider configuration.</param>
        /// <param name="excitabilityCfg">The configuration of the homogenous excitability.</param>
        /// <param name="steadyBias">The constant bias (0 means no bias).</param>
        private SpikingNeuronGroupSettings CreateSpikingGroup(IActivationSettings activationCfg,
                                                              PredictorsProviderSettings predictorsCfg,
                                                              HomogenousExcitabilitySettings excitabilityCfg,
                                                              double steadyBias = 0d
                                                              )
        {
            //Bias configuration
            RandomValueSettings biasCfg = steadyBias == 0 ? null : new RandomValueSettings(steadyBias, steadyBias);
            //Create neuron group configuration
            SpikingNeuronGroupSettings groupCfg = new SpikingNeuronGroupSettings(BuildNeuronGroupName(activationCfg),
                                                                                 1d,
                                                                                 activationCfg,
                                                                                 predictorsCfg,
                                                                                 excitabilityCfg,
                                                                                 biasCfg
                                                                                 );
            return groupCfg;
        }


        /// <summary>
        /// Creates the simplified configuration of the state machine following the pure ESN design.
        /// </summary>
        /// <param name="totalSize">The total number of hidden neurons.</param>
        /// <param name="maxInputStrength">The max sum of weights of input synapses per an analog hidden neuron (see the constant DefaultAnalogMaxInputStrength).</param>
        /// <param name="inputConnectionDensity">The density of the input field connections to hidden neurons.</param>
        /// <param name="maxInputDelay">The maximum delay of an input synapse.</param>
        /// <param name="interconnectionDensity">The density of the hidden neurons recurrent interconnection.</param>
        /// <param name="maxInternalDelay">The maximum delay of an internal synapse.</param>
        /// <param name="maxAbsBias">The maximum absolute value of the bias (0 means no bias).</param>
        /// <param name="maxRetainmentStrength">The maximum retainment strength (0 means no retainment).</param>
        /// <param name="predictorsProviderCfg">The configuration of the predictors provider.</param>
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
            if (BypassedNP)
            {
                throw new InvalidOperationException("Neural preprocessor is bypassed thus ESN design can't be created.");
            }
            //Default ESN activation
            IActivationSettings analogActivationCfg = new AFAnalogTanHSettings();
            //One neuron group
            AnalogNeuronGroupSettings grp = CreateAnalogGroup(analogActivationCfg, predictorsProviderCfg, maxAbsBias, maxRetainmentStrength);
            //Simple analog pool
            PoolSettings poolCfg = new PoolSettings(BuildPoolName(ActivationContent.Analog, 0),
                                                    new ProportionsSettings(totalSize, 1, 1),
                                                    new NeuronGroupsSettings(grp),
                                                    new InterconnSettings(new RandomSchemaSettings(interconnectionDensity))
                                                    );
            //Simple reservoir structure
            ReservoirStructureSettings resStructCfg = new ReservoirStructureSettings(BuildResStructName(ActivationContent.Analog, 0),
                                                                                     new PoolsSettings(poolCfg)
                                                                                     );
            //Input connections configuration
            List<InputConnSettings> inputConns = new List<InputConnSettings>(InputEncoderCfg.VaryingFieldsCfg.ExternalFieldsCfg.FieldCfgCollection.Count);
            foreach (ExternalFieldSettings fieldCfg in InputEncoderCfg.VaryingFieldsCfg.ExternalFieldsCfg.FieldCfgCollection)
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
            return new StateMachineSettings(new NeuralPreprocessorSettings(InputEncoderCfg,
                                                                           new ReservoirStructuresSettings(resStructCfg),
                                                                           new ReservoirInstancesSettings(resInstCfg)
                                                                           ),
                                            ReadoutLayerCfg
                                            );
        }

        /// <summary>
        /// Creates the simplified configuration of the state machine following the pure LSM design.
        /// </summary>
        /// <param name="totalSize">The total number of hidden neurons.</param>
        /// <param name="spikingActivationCfg">The configuration of the spiking activation function.</param>
        /// <param name="excitabilityCfg">The homogenous excitability configuration.</param>
        /// <param name="inputConnectionDensity">The density of the input field connections to hidden neurons.</param>
        /// <param name="maxInputDelay">The maximum delay of an input synapse.</param>
        /// <param name="interconnectionDensity">The density of the hidden neurons recurrent interconnection.</param>
        /// <param name="maxInternalDelay">The maximum delay of an internal synapse.</param>
        /// <param name="steadyBias">The constant bias (0 means no bias).</param>
        /// <param name="predictorsProviderCfg">The configuration of the predictors provider.</param>
        public StateMachineSettings CreatePureLSMCfg(int totalSize,
                                                     IActivationSettings spikingActivationCfg,
                                                     HomogenousExcitabilitySettings excitabilityCfg,
                                                     double inputConnectionDensity,
                                                     int maxInputDelay,
                                                     double interconnectionDensity,
                                                     int maxInternalDelay,
                                                     double steadyBias,
                                                     PredictorsProviderSettings predictorsProviderCfg
                                                     )
        {
            //Check NP is not bypassed
            if (BypassedNP)
            {
                throw new InvalidOperationException("Neural preprocessor is bypassed thus LSM design can't be created.");
            }
            //Activation check
            if (ActivationFactory.CreateAF(spikingActivationCfg, new Random()).TypeOfActivation != ActivationType.Spiking)
            {
                throw new ArgumentException("Specified activation must be spiking.", "spikingActivationCfg");
            }
            //One neuron group
            SpikingNeuronGroupSettings grp = CreateSpikingGroup(spikingActivationCfg, predictorsProviderCfg, excitabilityCfg, steadyBias);
            //Simple spiking pool
            PoolSettings poolCfg = new PoolSettings(BuildPoolName(ActivationContent.Spiking, 0),
                                                    new ProportionsSettings(totalSize, 1, 1),
                                                    new NeuronGroupsSettings(grp),
                                                    new InterconnSettings(new RandomSchemaSettings(interconnectionDensity, 0d, false, false))
                                                    );
            //Simple reservoir structure
            ReservoirStructureSettings resStructCfg = new ReservoirStructureSettings(BuildResStructName(ActivationContent.Spiking, 0),
                                                                                     new PoolsSettings(poolCfg)
                                                                                     );
            //Input connections configuration
            List<InputConnSettings> inputConns = new List<InputConnSettings>(InputEncoderCfg.VaryingFieldsCfg.ExternalFieldsCfg.FieldCfgCollection.Count);
            foreach (ExternalFieldSettings fieldCfg in InputEncoderCfg.VaryingFieldsCfg.ExternalFieldsCfg.FieldCfgCollection)
            {
                InputConnSettings inputConnCfg = new InputConnSettings(fieldCfg.Name,
                                                                       poolCfg.Name,
                                                                       inputConnectionDensity,
                                                                       0
                                                                       );
                inputConns.Add(inputConnCfg);
            }
            //Synapse general configuration
            SpikingSourceSTInputSettings spikingSourceSTInputSettings = new SpikingSourceSTInputSettings(new URandomValueSettings(0, 1), new PlasticitySTInputSettings(new NonlinearDynamicsSTInputSettings()));
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
            return new StateMachineSettings(new NeuralPreprocessorSettings(InputEncoderCfg,
                                                                           new ReservoirStructuresSettings(resStructCfg),
                                                                           new ReservoirInstancesSettings(resInstCfg)
                                                                           ),
                                            ReadoutLayerCfg
                                            );
        }

        /// <summary>
        /// Creates the state machine configuration having bypassed the neural preprocessing.
        /// </summary>
        public StateMachineSettings CreateBypassedPreprocessingCfg()
        {
            //Build and return SM configuration
            return new StateMachineSettings(null,
                                            ReadoutLayerCfg
                                            );
        }


    }//StateMachineDesigner

}//Namespace
