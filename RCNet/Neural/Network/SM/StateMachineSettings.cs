using System;
using System.Collections.Generic;
using System.Linq;
using System.Globalization;
using System.Xml.Linq;
using System.Reflection;
using RCNet.Extensions;
using RCNet.XmlTools;
using RCNet.RandomValue;
using RCNet.Neural.Network.SM.Preprocessing;
using RCNet.Neural.Network.SM.Preprocessing.Reservoir;
using RCNet.Neural.Network.SM.Readout;
using RCNet.Neural.Network.SM.PM;

namespace RCNet.Neural.Network.SM
{
    /// <summary>
    /// State Machine configuration.
    /// </summary>
    [Serializable]
    public class StateMachineSettings : RCNetBaseSettings
    {
        //Constants
        /// <summary>
        /// Name of the associated xsd type
        /// </summary>
        public const string XsdTypeName = "SMType";
        //Default values
        /// <summary>
        /// Default value of randomizer seek
        /// </summary>
        public const int DefaultRandomizerSeek = 0;

        //Attribute properties
        /// <summary>
        /// Configuration of the neural preprocessor
        /// </summary>
        public NeuralPreprocessorSettings NeuralPreprocessorCfg { get; }
        
        /// <summary>
        /// Configuration of the readout layer
        /// </summary>
        public ReadoutLayerSettings ReadoutLayerCfg { get; }
        
        /// <summary>
        /// Configuration of mapper of predictors to readout units
        /// </summary>
        public MapperSettings MapperCfg { get; }

        /// <summary>
        /// Specifies random number generator initialization seek.
        /// A value greater than or equal to 0 will always ensure the same initialization of the internal
        /// random number generator and therefore the same network structure.
        /// A value less than 0 causes a fully random initialization when creating an instance.
        /// </summary>
        public int RandomizerSeek { get; }

        //Constructors
        /// <summary>
        /// Creates an initialized instance
        /// </summary>
        /// <param name="neuralPreprocessorCfg">Configuration of the neural preprocessor</param>
        /// <param name="readoutLayerCfg">Configuration of the readout layer</param>
        /// <param name="mapperCfg">Configuration of mapper of predictors to readout units</param>
        /// <param name="randomizerSeek">Specifies random number generator initialization seek</param>
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
        /// The deep copy constructor
        /// </summary>
        /// <param name="source">Source instance</param>
        public StateMachineSettings(StateMachineSettings source)
            :this(source.NeuralPreprocessorCfg, source.ReadoutLayerCfg, source.MapperCfg, source.RandomizerSeek)
        {
            return;
        }

        /// <summary>
        /// Creates the instance and initializes it from given xml element.
        /// </summary>
        /// <param name="elem">Xml data containing settings</param>
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
        /// Checks if settings are default
        /// </summary>
        public bool IsDefaultRandomizerSeek { get { return RandomizerSeek == DefaultRandomizerSeek; } }

        /// <summary>
        /// Identifies settings containing only default values
        /// </summary>
        public override bool ContainsOnlyDefaults { get { return false; } }

        //Methods
        /// <summary>
        /// Checks consistency
        /// </summary>
        protected override void Check()
        {
            if (MapperCfg != null && NeuralPreprocessorCfg == null)
            {
                throw new ArgumentException($"Mapper can not be specified when neural preprocessor is not defined.", "MapperCfg");
            }
            if(MapperCfg != null)
            {
                foreach(ReadoutUnitMapSettings map in MapperCfg.MapCfgCollection)
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
                    if(map.AllowedInputFieldsCfg != null)
                    {
                        string[] routedFieldNames = NeuralPreprocessorCfg.InputEncoderCfg.GetRoutedFieldNames().ToArray();
                        foreach (AllowedInputFieldSettings aifs in map.AllowedInputFieldsCfg.AllowedInputFieldCfgCollection)
                        {
                            if(Array.IndexOf(routedFieldNames, aifs.Name) == -1)
                            {
                                throw new ArgumentException($"Specified input field {aifs.Name} to be allowed for readout unit {map.ReadoutUnitName} is not among fields routed to readout layer.", "MapperCfg");
                            }
                            NeuralPreprocessorCfg.InputEncoderCfg.FieldsCfg.GetFieldID(aifs.Name, true);
                        }
                    }
                }
            }
            return;
        }

        /// <summary>
        /// Creates the deep copy instance of this instance
        /// </summary>
        public override RCNetBaseSettings DeepClone()
        {
            return new StateMachineSettings(this);
        }

        /// <summary>
        /// Generates xml element containing the settings.
        /// </summary>
        /// <param name="rootElemName">Name to be used as a name of the root element.</param>
        /// <param name="suppressDefaults">Specifies if to ommit optional nodes having set default values</param>
        /// <returns>XElement containing the settings</returns>
        public override XElement GetXml(string rootElemName, bool suppressDefaults)
        {
            XElement rootElem = new XElement(rootElemName);
            if(!suppressDefaults || !IsDefaultRandomizerSeek)
            {
                rootElem.Add(new XAttribute("randomizerSeek", RandomizerSeek.ToString(CultureInfo.InvariantCulture)));
            }
            if (NeuralPreprocessorCfg != null)
            {
                rootElem.Add(NeuralPreprocessorCfg.GetXml(suppressDefaults));
            }
            rootElem.Add(ReadoutLayerCfg.GetXml(suppressDefaults));
            if(MapperCfg != null)
            {
                rootElem.Add(MapperCfg.GetXml(suppressDefaults));
            }
            Validate(rootElem, XsdTypeName);
            return rootElem;
        }

        /// <summary>
        /// Generates default named xml element containing the settings.
        /// </summary>
        /// <param name="suppressDefaults">Specifies if to ommit optional nodes having set default values</param>
        /// <returns>XElement containing the settings</returns>
        public override XElement GetXml(bool suppressDefaults)
        {
            return GetXml("stateMachine", suppressDefaults);
        }

    }//StateMachineSettings

}//Namespace
