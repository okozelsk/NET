using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using System.Globalization;
using System.Xml.Linq;
using System.IO;
using System.Xml.XPath;
using RCNet.Extensions;
using RCNet.XmlTools;
using RCNet.RandomValue;
using RCNet.Neural.Network.SM.Preprocessing.Input;
using RCNet.Neural.Network.SM.Preprocessing.Reservoir.Neuron.Predictor;
using RCNet.Neural.Network.SM.Preprocessing.Reservoir.SynapseNS;

namespace RCNet.Neural.Network.SM.Preprocessing.Reservoir
{
    /// <summary>
    /// Contains reservoir instance settings
    /// </summary>
    [Serializable]
    public class ReservoirInstanceSettings : RCNetBaseSettings
    {
        //Constants
        /// <summary>
        /// Name of the associated xsd type
        /// </summary>
        public const string XsdTypeName = "NPResInstanceType";

        //Attribute properties
        /// <summary>
        /// Name of the reservoir instance
        /// </summary>
        public string Name { get; }

        /// <summary>
        /// Name of the reservoir structure settings
        /// </summary>
        public string StructureCfgName { get; }

        /// <summary>
        /// Collection of input unit settings
        /// </summary>
        public InputUnitsSettings InputUnitsCfg { get; }

        /// <summary>
        /// Synapse configuration
        /// </summary>
        public SynapseSettings SynapseCfg { get; }

        /// <summary>
        /// Configuration of the predictors
        /// </summary>
        public PredictorsSettings PredictorsCfg { get; }

        //Constructors
        /// <summary>
        /// Creates an initialized instance
        /// </summary>
        /// <param name="name">Name of the reservoir structure settings</param>
        /// <param name="structureCfgName">Name of the reservoir structure settings</param>
        /// <param name="inputUnitsCfg">Collection of input unit settings</param>
        /// <param name="synapseCfg">Synapse configuration</param>
        /// <param name="predictorsCfg">Configuration of the predictors</param>
        public ReservoirInstanceSettings(string name,
                                         string structureCfgName,
                                         InputUnitsSettings inputUnitsCfg,
                                         SynapseSettings synapseCfg = null,
                                         PredictorsSettings predictorsCfg = null
                                         )
        {
            Name = name;
            StructureCfgName = structureCfgName;
            InputUnitsCfg = (InputUnitsSettings)inputUnitsCfg.DeepClone();
            SynapseCfg = synapseCfg == null ? null : (SynapseSettings)synapseCfg.DeepClone();
            PredictorsCfg = predictorsCfg == null ? null : (PredictorsSettings)predictorsCfg.DeepClone();
            Check();
            return;
        }

        /// <summary>
        /// The deep copy constructor
        /// </summary>
        /// <param name="source">Source instance</param>
        public ReservoirInstanceSettings(ReservoirInstanceSettings source)
            :this(source.Name, source.StructureCfgName, source.InputUnitsCfg, source.SynapseCfg, source.PredictorsCfg)
        {
            return;
        }

        /// <summary>
        /// Creates the instance and initialize it from given xml element.
        /// </summary>
        /// <param name="elem">
        /// Xml data containing reservoir settings.
        /// Content of xml element is always validated against the xml schema.
        /// </param>
        public ReservoirInstanceSettings(XElement elem)
        {
            //Validation
            XElement settingsElem = Validate(elem, XsdTypeName);
            //Parsing
            Name = settingsElem.Attribute("name").Value;
            StructureCfgName = settingsElem.Attribute("reservoirStructure").Value;
            //Input units
            InputUnitsCfg = new InputUnitsSettings(settingsElem.Descendants("inputUnits").First());
            //Synapse
            XElement synapseElem = settingsElem.Descendants("synapse").FirstOrDefault();
            SynapseCfg = synapseElem == null ? new SynapseSettings() : new SynapseSettings(synapseElem);
            //Predictors
            XElement predictorsElem = settingsElem.Descendants("predictors").FirstOrDefault();
            if (predictorsElem != null)
            {
                PredictorsCfg = new PredictorsSettings(predictorsElem);
            }
            Check();
            return;
        }

        //Properties
        /// <summary>
        /// Identifies settings containing only default values
        /// </summary>
        public override bool ContainsOnlyDefaults
        {
            get
            {
                return false;
            }
        }

        //Methods
        /// <summary>
        /// Checks validity
        /// </summary>
        private void Check()
        {
            if (Name.Length == 0)
            {
                throw new Exception($"Name can not be empty.");
            }
            if (StructureCfgName.Length == 0)
            {
                throw new Exception($"Name of the reservoir structure configuration can not be empty.");
            }
            return;
        }

        /// <summary>
        /// Checks consistency of this reservoir instance configuration and given
        /// preprocessor's input and reservoir structure configuration
        /// </summary>
        /// <param name="inputCfg">Preprocessor's input configuration</param>
        /// <param name="reservoirStructureCfg">Reservoir structure configuration</param>
        public void CheckConsistency(InputSettings inputCfg, ReservoirStructureSettings reservoirStructureCfg)
        {
            if (StructureCfgName != reservoirStructureCfg.Name)
            {
                throw new Exception($"Name of the reservoir structure configuration {StructureCfgName} is not equal to name of given reservoir structure configuration name {reservoirStructureCfg.Name}.");
            }
            foreach (InputUnitSettings inputUnitCfg in InputUnitsCfg.InputUnitCfgCollection)
            {
                inputCfg.FieldsCfg.GetFieldID(inputUnitCfg.InputFieldName, true);
                foreach (InputUnitConnSettings connCfg in inputUnitCfg.ConnsCfg.ConnCfgCollection)
                {
                    reservoirStructureCfg.PoolsCfg.GetPoolID(connCfg.PoolName);
                }
            }
            return;
        }

        /// <summary>
        /// Creates the deep copy instance of this instance
        /// </summary>
        public override RCNetBaseSettings DeepClone()
        {
            return new ReservoirInstanceSettings(this);
        }

        /// <summary>
        /// Generates xml element containing the settings.
        /// </summary>
        /// <param name="rootElemName">Name to be used as a name of the root element.</param>
        /// <param name="suppressDefaults">Specifies if to ommit optional nodes having set default values</param>
        /// <returns>XElement containing the settings</returns>
        public override XElement GetXml(string rootElemName, bool suppressDefaults)
        {
            XElement rootElem = new XElement(rootElemName,
                                             new XAttribute("name", Name),
                                             new XAttribute("reservoirStructure", StructureCfgName),
                                             InputUnitsCfg.GetXml(suppressDefaults));

            if (SynapseCfg != null && !SynapseCfg.ContainsOnlyDefaults)
            {
                rootElem.Add(SynapseCfg.GetXml(suppressDefaults));
            }
            if (PredictorsCfg != null && !PredictorsCfg.ContainsOnlyDefaults)
            {
                rootElem.Add(PredictorsCfg.GetXml(suppressDefaults));
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
            return GetXml("reservoirInstance", suppressDefaults);
        }

    }//ReservoirInstanceSettings

}//Namespace

