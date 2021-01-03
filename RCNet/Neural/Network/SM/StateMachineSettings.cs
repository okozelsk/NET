using RCNet.Neural.Network.SM.PM;
using RCNet.Neural.Network.SM.Preprocessing;
using RCNet.Neural.Network.SM.Preprocessing.Reservoir;
using RCNet.Neural.Network.SM.Readout;
using System;
using System.Globalization;
using System.Linq;
using System.Xml.Linq;

namespace RCNet.Neural.Network.SM
{
    /// <summary>
    /// Configuration of the state machine.
    /// </summary>
    [Serializable]
    public class StateMachineSettings : RCNetBaseSettings
    {
        //Constants
        /// <summary>
        /// The name of the associated xsd type.
        /// </summary>
        public const string XsdTypeName = "SMType";
        //Default values
        /// <summary>
        /// The default value of random number generator initial seek.
        /// </summary>
        public const int DefaultRandomizerSeek = 0;

        //Attribute properties
        /// <summary>
        /// The configuration of the neural preprocessor.
        /// </summary>
        public NeuralPreprocessorSettings NeuralPreprocessorCfg { get; }

        /// <summary>
        /// The configuration of the readout layer.
        /// </summary>
        public ReadoutLayerSettings ReadoutLayerCfg { get; }

        /// <summary>
        /// The configuration of mapper of specific predictors to readout units.
        /// </summary>
        public MapperSettings MapperCfg { get; }

        /// <summary>
        /// Specifies the random number generator initial seek.
        /// </summary>
        /// <remarks>
        /// A value greater than or equal to 0 will always ensure the same initialization of the internal random number generator and therefore also the same internal configuration each time the state machine to be instantiated.
        /// A value less than 0 causes different internal configuration each time the state machine to be instantiated.
        /// </remarks>
        public int RandomizerSeek { get; }

        //Constructors
        /// <summary>
        /// Creates an initialized instance.
        /// </summary>
        /// <param name="neuralPreprocessorCfg">The configuration of the neural preprocessor.</param>
        /// <param name="readoutLayerCfg">The configuration of the readout layer.</param>
        /// <param name="mapperCfg">The configuration of mapper of specific predictors to readout units.</param>
        /// <param name="randomizerSeek">Specifies the random number generator initial seek.</param>
        public StateMachineSettings(NeuralPreprocessorSettings neuralPreprocessorCfg,
                                    ReadoutLayerSettings readoutLayerCfg,
                                    MapperSettings mapperCfg = null,
                                    int randomizerSeek = DefaultRandomizerSeek)
        {
            NeuralPreprocessorCfg = neuralPreprocessorCfg == null ? null : (NeuralPreprocessorSettings)neuralPreprocessorCfg.DeepClone();
            ReadoutLayerCfg = (ReadoutLayerSettings)readoutLayerCfg.DeepClone();
            MapperCfg = mapperCfg == null ? null : (MapperSettings)mapperCfg.DeepClone();
            RandomizerSeek = randomizerSeek;
            Check();
            return;
        }

        /// <summary>
        /// The deep copy constructor.
        /// </summary>
        /// <param name="source">The source instance.</param>
        public StateMachineSettings(StateMachineSettings source)
            : this(source.NeuralPreprocessorCfg, source.ReadoutLayerCfg, source.MapperCfg, source.RandomizerSeek)
        {
            return;
        }

        /// <summary>
        /// Creates an initialized instance.
        /// </summary>
        /// <param name="elem">A xml element containing the configuration data.</param>
        public StateMachineSettings(XElement elem)
        {
            //Validation
            XElement settingsElem = Validate(elem, XsdTypeName);
            //Parsing
            RandomizerSeek = int.Parse(settingsElem.Attribute("randomizerSeek").Value);
            //Neural preprocessor
            XElement neuralPreprocessorElem = settingsElem.Elements("neuralPreprocessor").FirstOrDefault();
            NeuralPreprocessorCfg = neuralPreprocessorElem == null ? null : new NeuralPreprocessorSettings(neuralPreprocessorElem);
            //Readout layer
            ReadoutLayerCfg = new ReadoutLayerSettings(settingsElem.Elements("readoutLayer").First());
            //Mapper
            XElement mapperSettingsElem = settingsElem.Elements("mapper").FirstOrDefault();
            MapperCfg = mapperSettingsElem == null ? null : new MapperSettings(mapperSettingsElem);
            Check();
            return;
        }

        //Properties
        /// <summary>
        /// Checks the defaults.
        /// </summary>
        public bool IsDefaultRandomizerSeek { get { return RandomizerSeek == DefaultRandomizerSeek; } }

        /// <inheritdoc />
        public override bool ContainsOnlyDefaults { get { return false; } }

        //Methods
        /// <inheritdoc />
        protected override void Check()
        {
            if (MapperCfg != null && NeuralPreprocessorCfg == null)
            {
                throw new ArgumentException($"Mapper can not be specified when neural preprocessor is not defined.", "MapperCfg");
            }
            if (MapperCfg != null)
            {
                foreach (ReadoutUnitMapSettings map in MapperCfg.MapCfgCollection)
                {
                    ReadoutUnitSettings rus = ReadoutLayerCfg.ReadoutUnitsCfg.GetReadoutunitCfg(map.ReadoutUnitName);
                    //Pools
                    if (map.AllowedPoolsCfg != null)
                    {
                        foreach (AllowedPoolSettings aps in map.AllowedPoolsCfg.AllowedPoolCfgCollection)
                        {
                            ReservoirInstanceSettings ris = NeuralPreprocessorCfg.ReservoirInstancesCfg.GetReservoirInstanceCfg(aps.ReservoirInstanceName);
                            ReservoirStructureSettings rss = NeuralPreprocessorCfg.ReservoirStructuresCfg.GetReservoirStructureCfg(ris.StructureCfgName);
                            rss.PoolsCfg.GetPoolID(aps.PoolName);
                        }
                    }
                    //Input fields
                    if (map.AllowedInputFieldsCfg != null)
                    {
                        string[] routedFieldNames = NeuralPreprocessorCfg.InputEncoderCfg.GetRoutedFieldNames().ToArray();
                        foreach (AllowedInputFieldSettings aifs in map.AllowedInputFieldsCfg.AllowedInputFieldCfgCollection)
                        {
                            if (Array.IndexOf(routedFieldNames, aifs.Name) == -1)
                            {
                                throw new ArgumentException($"Specified input field {aifs.Name} to be allowed for readout unit {map.ReadoutUnitName} is not among fields routed to readout layer.", "MapperCfg");
                            }
                        }
                    }
                }
            }
            return;
        }

        /// <inheritdoc />
        public override RCNetBaseSettings DeepClone()
        {
            return new StateMachineSettings(this);
        }

        /// <inheritdoc />
        public override XElement GetXml(string rootElemName, bool suppressDefaults)
        {
            XElement rootElem = new XElement(rootElemName);
            if (!suppressDefaults || !IsDefaultRandomizerSeek)
            {
                rootElem.Add(new XAttribute("randomizerSeek", RandomizerSeek.ToString(CultureInfo.InvariantCulture)));
            }
            if (NeuralPreprocessorCfg != null)
            {
                rootElem.Add(NeuralPreprocessorCfg.GetXml(suppressDefaults));
            }
            rootElem.Add(ReadoutLayerCfg.GetXml(suppressDefaults));
            if (MapperCfg != null)
            {
                rootElem.Add(MapperCfg.GetXml(suppressDefaults));
            }
            Validate(rootElem, XsdTypeName);
            return rootElem;
        }

        /// <inheritdoc />
        public override XElement GetXml(bool suppressDefaults)
        {
            return GetXml("stateMachine", suppressDefaults);
        }

    }//StateMachineSettings

}//Namespace
