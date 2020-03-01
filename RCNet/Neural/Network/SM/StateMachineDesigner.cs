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
            /// ESN design. One reservoir consisting of one randomly interconnected pool
            /// </summary>
            PureESN,
            /// <summary>
            /// ESN design. One reservoir consisting of one circled pool receiving the input
            /// followed by one unidirectionally connected randomly interconnected pool
            /// </summary>
            ESN_CIP_URP,
            /// <summary>
            /// LSM design
            /// </summary>
            PureLSM,
            /// <summary>
            /// Hybrid (mixed) design
            /// </summary>
            Hybrid
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
        /// Creates configuration of single output layer Identity FF network structure with associated resilient back propagation trainer
        /// </summary>
        /// <param name="numOfAttempts">Number of regression attempts. Each readout network will try to learn numOfAttempts times</param>
        /// <param name="numOfEpochs">Number of training epochs within an attempt</param>
        public static FeedForwardNetworkSettings CreateIdentityRegrNet(int numOfAttempts, int numOfEpochs)
        {
            return new FeedForwardNetworkSettings(new IdentitySettings(), null, new RPropTrainerSettings(numOfAttempts, numOfEpochs));
        }

        /// <summary>
        /// Creates configuration of single output layer Elliot FF network structure with associated resilient back propagation trainer
        /// </summary>
        /// <param name="numOfAttempts">Number of regression attempts. Each readout network will try to learn numOfAttempts times</param>
        /// <param name="numOfEpochs">Number of training epochs within an attempt</param>
        public static FeedForwardNetworkSettings CreateElliotRegrNet(int numOfAttempts, int numOfEpochs)
        {
            return new FeedForwardNetworkSettings(new ElliotSettings(), null, new RPropTrainerSettings(numOfAttempts, numOfEpochs));
        }

        /// <summary>
        /// Creates readout layer configuration to solve forecast task
        /// </summary>
        /// <param name="netCfg">FF network configuration to be associated with readout units</param>
        /// <param name="testDataRatio">Specifies what part of available data to be used as test data</param>
        /// <param name="unitName">Readout unit name</param>
        public static ReadoutLayerSettings CreateForecastReadoutCfg(FeedForwardNetworkSettings netCfg, double testDataRatio, params string[] unitName)
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
                                            1,
                                            new DefaultNetworksSettings(null, new ForecastNetworksSettings(netCfg))
                                            );
        }

        /// <summary>
        /// Creates readout layer configuration to solve classification task
        /// </summary>
        /// <param name="netCfg">FF network configuration to be associated with readout units</param>
        /// <param name="testDataRatio">Specifies what part of available data to be used as test data</param>
        /// <param name="oneWinnerGroupName">Name of the "one winner" group encapsulating classification readout units</param>
        /// <param name="unitName">Readout unit name</param>
        public static ReadoutLayerSettings CreateClassificationReadoutCfg(FeedForwardNetworkSettings netCfg,
                                                                          double testDataRatio,
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
                                            1,
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
        /// <param name="role">Role of the neurons inside the group</param>
        /// <param name="activationCfg">Activation function configuration</param>
        private string GetNeuronGroupName(NeuronCommon.NeuronRole role, RCNetBaseSettings activationCfg)
        {
            return "Grp-" + role.ToString().Substring(0, 3) + "-" + GetActivationName(activationCfg);
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
        /// <param name="role">Role of the neurons within the group (Excitatory/Inhibitory)</param>
        /// <param name="activationCfg">Activation function configuration</param>
        /// <param name="maxAbsBias">Maximum absolute value of the bias (0 means bias is not required)</param>
        /// <param name="maxRetainmentStrength">Maximum retainment strength (0 means retainment property is not required)</param>
        private AnalogNeuronGroupSettings CreateAnalogGroup(NeuronCommon.NeuronRole role,
                                                            RCNetBaseSettings activationCfg,
                                                            double maxAbsBias = 0d,
                                                            double maxRetainmentStrength = 0d
                                                            )
        {
            //Bias configuration
            RandomValueSettings biasCfg = maxAbsBias == 0 ? null : new RandomValueSettings(-maxAbsBias, maxAbsBias);
            //Retainment configuration
            const double RetainmentDensity = 1d;
            AnalogRetainmentSettings retainmentCfg = maxRetainmentStrength == 0 ? null : new AnalogRetainmentSettings(RetainmentDensity, new URandomValueSettings(0, maxRetainmentStrength));
            //Create neuron group configuration
            AnalogNeuronGroupSettings groupCfg = new AnalogNeuronGroupSettings(GetNeuronGroupName(role, activationCfg),
                                                                               role,
                                                                               1d,
                                                                               activationCfg,
                                                                               AnalogNeuronGroupSettings.DefaultFiringThreshold,
                                                                               AnalogNeuronGroupSettings.DefaultSignalingRestriction,
                                                                               AnalogNeuronGroupSettings.DefaultReadoutDensity,
                                                                               biasCfg,
                                                                               retainmentCfg
                                                                               );
            return groupCfg;
        }


        /// <summary>
        /// Creates StateMachine configuration following pure ESN design
        /// </summary>
        /// <param name="totalSize">Total number of hidden neurons</param>
        /// <param name="interconnectionDensity">Density of the hidden neurons interconnection</param>
        /// <param name="maxInputDelay">Maximum delay of input synapse</param>
        /// <param name="maxInternalDelay">Maximum delay of internal synapse</param>
        /// <param name="maxAbsBias">Maximum absolute value of the bias (0 means bias is not required)</param>
        /// <param name="maxRetainmentStrength">Maximum retainment strength (0 means retainment property is not required)</param>
        /// <param name="allowedPredictor">Allowed predictor</param>
        public StateMachineSettings CreatePureESNCfg(int totalSize,
                                                     double interconnectionDensity,
                                                     int maxInputDelay,
                                                     int maxInternalDelay,
                                                     double maxAbsBias,
                                                     double maxRetainmentStrength,
                                                     params PredictorsProvider.PredictorID[] allowedPredictor
                                                     )
        {
            RCNetBaseSettings aFnCfg = new TanHSettings();
            AnalogNeuronGroupSettings excGrp = CreateAnalogGroup(NeuronCommon.NeuronRole.Excitatory, aFnCfg, maxAbsBias, maxRetainmentStrength);
            AnalogNeuronGroupSettings inhGrp = CreateAnalogGroup(NeuronCommon.NeuronRole.Inhibitory, aFnCfg, maxAbsBias, maxRetainmentStrength);
            PoolSettings poolCfg = new PoolSettings(GetPoolName(ActivationContent.Analog, 0),
                                                    new ProportionsSettings(totalSize, 1, 1),
                                                    new NeuronGroupsSettings(excGrp, inhGrp),
                                                    new InterconnSettings(new RandomSchemaSettings(new ConnDistrFlatSettings(), interconnectionDensity))
                                                    );
            ReservoirStructureSettings resStructCfg = new ReservoirStructureSettings(GetResStructName(ActivationContent.Analog, 0),
                                                                                     new PoolsSettings(poolCfg)
                                                                                     );
            List<InputUnitSettings> inputUnits = new List<InputUnitSettings>(InputCfg.FieldsCfg.ExternalFieldsCfg.FieldCfgCollection.Count);
            double maxInpSynWeight = 2d / InputCfg.FieldsCfg.ExternalFieldsCfg.FieldCfgCollection.Count;
            foreach (ExternalFieldSettings fieldCfg in InputCfg.FieldsCfg.ExternalFieldsCfg.FieldCfgCollection)
            {
                InputUnitConnSettings inputUnitConnCfg = new InputUnitConnSettings(poolCfg.Name,
                                                                                   InputUnit.AnalogCodingMethod.Actual,
                                                                                   false,
                                                                                   NeuronCommon.NeuronSignalingRestrictionType.AnalogOnly,
                                                                                   null,
                                                                                   new AnalogTargetSettings(Synapse.SynapticTargetScope.All,
                                                                                                            1,
                                                                                                            new URandomValueSettings(0d, maxInpSynWeight)
                                                                                                            )
                                                                                   );
                inputUnits.Add(new InputUnitSettings(fieldCfg.Name, new InputUnitConnsSettings(inputUnitConnCfg)));
            }
            SynapseSettings synapseCfg = new SynapseSettings(Synapse.SynapticDelayMethod.Random, maxInputDelay,
                                                             Synapse.SynapticDelayMethod.Random, maxInternalDelay
                                                             );

            //Initially we set all switches to false - all available predictors are forbidden
            bool[] predictorSwitches = new bool[PredictorsProvider.NumOfPredictors];
            predictorSwitches.Populate(false);
            //Now enable specific predictors
            foreach(PredictorsProvider.PredictorID predictorID in allowedPredictor)
            {
                predictorSwitches[(int)predictorID] = true;
            }
            //Create predictors configuration using default params
            PredictorsSettings predictorsCfg = new PredictorsSettings(predictorSwitches, null);

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



    }//StateMachineDesigner

}//Namespace
