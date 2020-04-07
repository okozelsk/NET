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

namespace RCNet.Neural.Network.SM
{
    /// <summary>
    /// Helper clas for the non-xml configuration of the StateMachine
    /// </summary>
    [Serializable]
    public class StateMachineDesigner
    {
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
        public InputSettings InputCfg { get; }
        
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
        public StateMachineDesigner(InputSettings inputCfg, ReadoutLayerSettings readoutCfg)
        {
            InputCfg = inputCfg ?? throw new ArgumentNullException("inputCfg");
            ReadoutCfg = readoutCfg ?? throw new ArgumentNullException("readoutCfg");
            _rand = new Random(0);
            return;
        }


        //Static methods
        /// <summary>
        /// Builds input configuration
        /// </summary>
        /// <param name="feedingCfg">Input feeding configuration</param>
        /// <param name="externalFieldCfg">External input field configuration</param>
        public static InputSettings CreateInputCfg(IFeedingSettings feedingCfg, params ExternalFieldSettings[] externalFieldCfg)
        {
            if (feedingCfg == null)
            {
                throw new ArgumentNullException("feedingCfg");
            }
            return new InputSettings(feedingCfg, new FieldsSettings(new ExternalFieldsSettings(externalFieldCfg)));
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
            for(int i = 0; i < numOfHiddenLayers; i++)
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
            return "ResStruct-"+ activationContent.ToString() + "-Cfg" + (resStructIdx + 1).ToString();
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
        /// <param name="maxAbsBias">Maximum absolute value of the bias (0 means bias is not required)</param>
        /// <param name="maxRetainmentStrength">Maximum retainment strength (0 means retainment property is not required)</param>
        private AnalogNeuronGroupSettings CreateAnalogGroup(RCNetBaseSettings activationCfg,
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
                                                                               AnalogNeuronGroupSettings.DefaultFiringThreshold,
                                                                               AnalogNeuronGroupSettings.DefaultSignalingRestriction,
                                                                               biasCfg,
                                                                               retainmentCfg,
                                                                               null
                                                                               );
            return groupCfg;
        }

        /// <summary>
        /// Creates configuration of group of spiking neurons having specified spiking activation.
        /// </summary>
        /// <param name="activationCfg">Activation function configuration</param>
        /// <param name="heCfg">Configuration of the homogenous excitability</param>
        /// <param name="steadyBias">Constant bias (0 means bias is not required)</param>
        private SpikingNeuronGroupSettings CreateSpikingGroup(RCNetBaseSettings activationCfg, HomogenousExcitabilitySettings heCfg, double steadyBias = 0d)
        {
            //Bias configuration
            RandomValueSettings biasCfg = steadyBias == 0 ? null : new RandomValueSettings(steadyBias, steadyBias);
            //Create neuron group configuration
            SpikingNeuronGroupSettings groupCfg = new SpikingNeuronGroupSettings(GetNeuronGroupName(activationCfg),
                                                                                 1d,
                                                                                 activationCfg,
                                                                                 heCfg,
                                                                                 biasCfg,
                                                                                 null
                                                                                 );
            return groupCfg;
        }


        /// <summary>
        /// Creates StateMachine configuration following pure ESN design
        /// </summary>
        /// <param name="totalSize">Total number of hidden neurons</param>
        /// <param name="inputConnectionDensity">Density of the input field connections to hidden neurons</param>
        /// <param name="maxInputDelay">Maximum delay of input synapse</param>
        /// <param name="interconnectionDensity">Density of the hidden neurons interconnection</param>
        /// <param name="maxInternalDelay">Maximum delay of internal synapse</param>
        /// <param name="maxAbsBias">Maximum absolute value of the bias (0 means bias is not required)</param>
        /// <param name="maxRetainmentStrength">Maximum retainment strength (0 means retainment property is not required)</param>
        /// <param name="predictorsParamsCfg">Predictors parameters (use null for defaults)</param>
        /// <param name="allowedPredictor">Allowed predictor(s)</param>
        public StateMachineSettings CreatePureESNCfg(int totalSize,
                                                     double inputConnectionDensity,
                                                     int maxInputDelay,
                                                     double interconnectionDensity,
                                                     int maxInternalDelay,
                                                     double maxAbsBias,
                                                     double maxRetainmentStrength,
                                                     PredictorsParamsSettings predictorsParamsCfg,
                                                     params PredictorsProvider.PredictorID[] allowedPredictor
                                                     )
        {
            const double MaxInputWeightSum = 2.75d;
            //Default ESN activation
            RCNetBaseSettings aFnCfg = new TanHSettings();
            //One neuron group
            AnalogNeuronGroupSettings grp = CreateAnalogGroup(aFnCfg, maxAbsBias, maxRetainmentStrength);
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
            //Input units and connections configuration
            List<InputUnitSettings> inputUnits = new List<InputUnitSettings>(InputCfg.FieldsCfg.ExternalFieldsCfg.FieldCfgCollection.Count);
            double maxInpSynWeight = MaxInputWeightSum / InputCfg.FieldsCfg.ExternalFieldsCfg.FieldCfgCollection.Count;
            foreach (ExternalFieldSettings fieldCfg in InputCfg.FieldsCfg.ExternalFieldsCfg.FieldCfgCollection)
            {
                InputUnitConnSettings inputUnitConnCfg = new InputUnitConnSettings(poolCfg.Name,
                                                                                   0,
                                                                                   inputConnectionDensity,
                                                                                   NeuronCommon.NeuronSignalingRestrictionType.AnalogOnly
                                                                                   );
                inputUnits.Add(new InputUnitSettings(fieldCfg.Name,
                                                     new InputUnitConnsSettings(inputUnitConnCfg),
                                                     InputUnitSettings.DefaultSpikeTrainLength,
                                                     InputUnitSettings.DefaultAnalogFiringThreshold,
                                                     false,
                                                     false
                                                     )
                               );
            }
            //Synapse general configuration
            AnalogSourceSettings asc = new AnalogSourceSettings(new URandomValueSettings(0, maxInpSynWeight));
            SynapseATInputSettings synapseATInputSettings = new SynapseATInputSettings(Synapse.SynapticDelayMethod.Random, maxInputDelay, asc, null);
            SynapseATIndifferentSettings synapseATIndifferentSettings = new SynapseATIndifferentSettings(Synapse.SynapticDelayMethod.Random, maxInternalDelay);
            SynapseATSettings synapseATCfg = new SynapseATSettings(SynapseATSettings.DefaultSpectralRadiusNum, synapseATInputSettings, synapseATIndifferentSettings);
            SynapseSettings synapseCfg = new SynapseSettings(null, synapseATCfg);

            //Initially set all switches to false - all available predictors are forbidden
            bool[] predictorSwitches = new bool[PredictorsProvider.NumOfSupportedPredictors];
            predictorSwitches.Populate(false);
            //Enable specified predictors
            foreach(PredictorsProvider.PredictorID predictorID in allowedPredictor)
            {
                predictorSwitches[(int)predictorID] = true;
            }
            //Create predictors configuration using default params
            PredictorsSettings predictorsCfg = new PredictorsSettings(predictorSwitches, predictorsParamsCfg);
            //Create reservoir instance
            ReservoirInstanceSettings resInstCfg = new ReservoirInstanceSettings(GetResInstName(ResDesign.PureESN, 0),
                                                                                 resStructCfg.Name,
                                                                                 new InputUnitsSettings(inputUnits),
                                                                                 synapseCfg,
                                                                                 predictorsCfg
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
        /// <param name="proportionsCfg">LSM pool proportions</param>
        /// <param name="inputSpikeTrainLength">Spike-train length (0 means no spike-train coding)</param>
        /// <param name="aFnCfg">Spiking activation function configuration</param>
        /// <param name="hes">Homogenous excitability configuration</param>
        /// <param name="inputConnectionDensity">Density of the input field connections to hidden neurons</param>
        /// <param name="maxInputDelay">Maximum delay of input synapse</param>
        /// <param name="interconnectionDensity">Density of the hidden neurons interconnection</param>
        /// <param name="maxInternalDelay">Maximum delay of internal synapse</param>
        /// <param name="steadyBias">Constant bias (0 means bias is not required)</param>
        /// <param name="predictorsParamsCfg">Predictors parameters (use null for defaults)</param>
        /// <param name="allowedPredictor">Allowed predictor(s)</param>
        public StateMachineSettings CreatePureLSMCfg(ProportionsSettings proportionsCfg,
                                                     int inputSpikeTrainLength,
                                                     RCNetBaseSettings aFnCfg,
                                                     HomogenousExcitabilitySettings hes,
                                                     double inputConnectionDensity,
                                                     int maxInputDelay,
                                                     double interconnectionDensity,
                                                     int maxInternalDelay,
                                                     double steadyBias,
                                                     PredictorsParamsSettings predictorsParamsCfg,
                                                     params PredictorsProvider.PredictorID[] allowedPredictor
                                                     )
        {
            //Activation check
            if(ActivationFactory.Create(aFnCfg, new Random()).TypeOfActivation != ActivationType.Spiking)
            {
                throw new ArgumentException("Specified activation must be spiking.", "aFnCfg");
            }
            //One neuron group
            SpikingNeuronGroupSettings grp = CreateSpikingGroup(aFnCfg, hes, steadyBias);
            //Simple spiking pool
            PoolSettings poolCfg = new PoolSettings(GetPoolName(ActivationContent.Spiking, 0),
                                                    proportionsCfg,
                                                    new NeuronGroupsSettings(grp),
                                                    new InterconnSettings(new RandomSchemaSettings(interconnectionDensity, 0d, false, false))
                                                    );
            //Simple reservoir structure
            ReservoirStructureSettings resStructCfg = new ReservoirStructureSettings(GetResStructName(ActivationContent.Spiking, 0),
                                                                                     new PoolsSettings(poolCfg)
                                                                                     );
            //Input units and connections configuration
            List<InputUnitSettings> inputUnits = new List<InputUnitSettings>(InputCfg.FieldsCfg.ExternalFieldsCfg.FieldCfgCollection.Count);
            foreach (ExternalFieldSettings fieldCfg in InputCfg.FieldsCfg.ExternalFieldsCfg.FieldCfgCollection)
            {
                InputUnitConnSettings inputUnitConnCfg = new InputUnitConnSettings(poolCfg.Name,
                                                                                   inputConnectionDensity,
                                                                                   0,
                                                                                   inputSpikeTrainLength > 0 ? NeuronCommon.NeuronSignalingRestrictionType.SpikingOnly : NeuronCommon.NeuronSignalingRestrictionType.AnalogOnly
                                                                                   );
                inputUnits.Add(new InputUnitSettings(fieldCfg.Name,
                                                     new InputUnitConnsSettings(inputUnitConnCfg), inputSpikeTrainLength > 0 ? inputSpikeTrainLength : 1,
                                                     InputUnitSettings.DefaultAnalogFiringThreshold,
                                                     true,
                                                     true
                                                     )
                               );
            }
            //Synapse general configuration
            SynapseSTInputSettings synapseSTInputSettings = new SynapseSTInputSettings(Synapse.SynapticDelayMethod.Random, maxInputDelay);
            SynapseSTExcitatorySettings synapseSTExcitatorySettings = new SynapseSTExcitatorySettings(Synapse.SynapticDelayMethod.Random, maxInternalDelay);
            SynapseSTInhibitorySettings synapseSTInhibitorySettings = new SynapseSTInhibitorySettings(Synapse.SynapticDelayMethod.Random, maxInternalDelay);
            SynapseSTSettings synapseSTCfg = new SynapseSTSettings(synapseSTInputSettings, synapseSTExcitatorySettings, synapseSTInhibitorySettings);
            SynapseSettings synapseCfg = new SynapseSettings(synapseSTCfg, null);

            //Initially set all switches to false - all available predictors are forbidden
            bool[] predictorSwitches = new bool[PredictorsProvider.NumOfSupportedPredictors];
            predictorSwitches.Populate(false);
            //Enable specified predictors
            foreach (PredictorsProvider.PredictorID predictorID in allowedPredictor)
            {
                predictorSwitches[(int)predictorID] = true;
            }
            //Create predictors configuration using default params
            PredictorsSettings predictorsCfg = new PredictorsSettings(predictorSwitches, predictorsParamsCfg);
            //Create reservoir instance
            ReservoirInstanceSettings resInstCfg = new ReservoirInstanceSettings(GetResInstName(ResDesign.PureLSM, 0),
                                                                                 resStructCfg.Name,
                                                                                 new InputUnitsSettings(inputUnits),
                                                                                 synapseCfg,
                                                                                 predictorsCfg
                                                                                 );
            //Build and return SM configuration
            return new StateMachineSettings(new NeuralPreprocessorSettings(InputCfg,
                                                                           new ReservoirStructuresSettings(resStructCfg),
                                                                           new ReservoirInstancesSettings(resInstCfg)
                                                                           ),
                                            ReadoutCfg
                                            );
        }



    }//StateMachineDesigner

}//Namespace
