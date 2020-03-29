using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using System.Globalization;
using System.Xml.Linq;
using System.Xml.XPath;
using System.IO;
using RCNet.Extensions;
using RCNet.MathTools.Probability;
using RCNet.XmlTools;
using RCNet.RandomValue;
using RCNet.Neural.Network.SM.Preprocessing.Reservoir.Pool;

namespace RCNet.Neural.Network.SM.Preprocessing.Reservoir
{
    /// <summary>
    /// Collection of input unit settings
    /// </summary>
    [Serializable]
    public class InputUnitsSettings : RCNetBaseSettings
    {
        //Constants
        /// <summary>
        /// Name of the associated xsd type
        /// </summary>
        public const string XsdTypeName = "NPResInstanceInputUnitsType";

        //Attribute properties
        /// <summary>
        /// Collection of input unit settings
        /// </summary>
        public List<InputUnitSettings> InputUnitCfgCollection { get; }

        //Constructors
        /// <summary>
        /// Creates an initialized instance
        /// </summary>
        private InputUnitsSettings()
        {
            InputUnitCfgCollection = new List<InputUnitSettings>();
            return;
        }

        /// <summary>
        /// Creates an initialized instance
        /// </summary>
        /// <param name="inputUnitCfgCollection">Input unit settings collection</param>
        public InputUnitsSettings(IEnumerable<InputUnitSettings> inputUnitCfgCollection)
            : this()
        {
            AddPools(inputUnitCfgCollection);
            Check();
            return;
        }

        /// <summary>
        /// Creates an initialized instance
        /// </summary>
        /// <param name="inputUnitCfgCollection">Input unit settings collection</param>
        public InputUnitsSettings(params InputUnitSettings[] inputUnitCfgCollection)
            : this()
        {
            AddPools(inputUnitCfgCollection);
            Check();
            return;
        }

        /// <summary>
        /// The deep copy constructor
        /// </summary>
        /// <param name="source">Source instance</param>
        public InputUnitsSettings(InputUnitsSettings source)
            : this()
        {
            AddPools(source.InputUnitCfgCollection);
            return;
        }

        /// <summary>
        /// Creates the instance and initialize it from given xml element.
        /// </summary>
        /// <param name="elem">Xml data containing settings.</param>
        public InputUnitsSettings(XElement elem)
        {
            //Validation
            XElement settingsElem = Validate(elem, XsdTypeName);
            //Parsing
            InputUnitCfgCollection = new List<InputUnitSettings>();
            foreach (XElement inputUnitElem in settingsElem.Elements("inputUnit"))
            {
                InputUnitCfgCollection.Add(new InputUnitSettings(inputUnitElem));
            }
            Check();
            return;
        }

        //Properties
        /// <summary>
        /// Identifies settings containing only default values
        /// </summary>
        public override bool ContainsOnlyDefaults { get { return false; } }

        //Methods
        /// <summary>
        /// Checks validity
        /// </summary>
        private void Check()
        {
            if (InputUnitCfgCollection.Count == 0)
            {
                throw new Exception($"At least one input unit configuration must be specified.");
            }
            //Uniqueness
            string[] names = new string[InputUnitCfgCollection.Count];
            names[0] = InputUnitCfgCollection[0].InputFieldName;
            for(int i = 1; i < InputUnitCfgCollection.Count; i++)
            {
                if(names.Contains(InputUnitCfgCollection[i].InputFieldName))
                {
                    throw new Exception($"Referenced input field name {InputUnitCfgCollection[i].InputFieldName} is not unique within the input units scope.");
                }
                names[i] = InputUnitCfgCollection[i].InputFieldName;
            }
            return;
        }

        /// <summary>
        /// Adds cloned input unit configurations from given collection into the internal collection
        /// </summary>
        /// <param name="inputUnitCfgCollection"></param>
        private void AddPools(IEnumerable<InputUnitSettings> inputUnitCfgCollection)
        {
            foreach (InputUnitSettings inputUnitCfg in inputUnitCfgCollection)
            {
                InputUnitCfgCollection.Add((InputUnitSettings)inputUnitCfg.DeepClone());
            }
            return;
        }

        /// <summary>
        /// Returns ID (index) of the input unit referencing given input field name
        /// </summary>
        /// <param name="inputFieldName">Input field name</param>
        public int GetInputUnitID(string inputFieldName)
        {
            for(int i = 0; i < InputUnitCfgCollection.Count; i++)
            {
                if(InputUnitCfgCollection[i].InputFieldName == inputFieldName)
                {
                    return i;
                }
            }
            throw new Exception($"Input field name {inputFieldName} not found.");
        }

        /// <summary>
        /// Returns configuration of the input unit referencing given input field name
        /// </summary>
        /// <param name="inputFieldName">Input field name</param>
        public InputUnitSettings GetPoolCfg(string inputFieldName)
        {
            return InputUnitCfgCollection[GetInputUnitID(inputFieldName)];
        }

        /// <summary>
        /// Creates the deep copy instance of this instance
        /// </summary>
        public override RCNetBaseSettings DeepClone()
        {
            return new InputUnitsSettings(this);
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
            foreach (InputUnitSettings inputUnitCfg in InputUnitCfgCollection)
            {
                rootElem.Add(inputUnitCfg.GetXml(suppressDefaults));
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
            return GetXml("inputUnits", suppressDefaults);
        }

    }//InputUnitsSettings

}//Namespace
