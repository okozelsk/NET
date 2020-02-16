using System;
using System.Collections.Generic;
using System.Linq;
using System.Globalization;
using System.Xml.Linq;
using System.Reflection;
using System.IO;
using RCNet.Extensions;
using RCNet.Neural.Network.NonRecurrent.FF;
using RCNet.Neural.Network.NonRecurrent.PP;
using RCNet.XmlTools;
using RCNet.MathTools;
using RCNet.Neural.Data.Filter;
using RCNet.Neural.Network.NonRecurrent;
using RCNet.Neural.Activation;

namespace RCNet.Neural.Network.SM.Readout
{
    /// <summary>
    /// Readout unit settings
    /// </summary>
    [Serializable]
    public class ReadoutUnitSettings : RCNetBaseSettings
    {
        //Constants
        /// <summary>
        /// Name of the associated xsd type
        /// </summary>
        public const string XsdTypeName = "ROutLayerUnitCfgType";

        //Attribute properties
        /// <summary>
        /// Readout unit's zero-based index within output fields
        /// </summary>
        public int Index { get; }

        /// <summary>
        /// Output field name
        /// </summary>
        public string Name { get; }
        
        /// <summary>
        /// Readout unit's Forecast or Classification task settings
        /// </summary>
        public ITaskSettings TaskSettings { get; }

        //Constructors
        /// <summary>
        /// Creates an unitialized instance
        /// </summary>
        /// <param name="index">Readout unit's zero-based index within output fields</param>
        /// <param name="name">Output field name</param>
        /// <param name="taskSettings">Readout unit's Forecast or Classification task settings</param>
        public ReadoutUnitSettings(int index, string name, ITaskSettings taskSettings)
        {
            Index = index;
            Name = name;
            TaskSettings = (ITaskSettings)taskSettings.DeepClone();
            Check();
            return;
        }

        /// <summary>
        /// Copy constructor
        /// </summary>
        /// <param name="source">Source instance</param>
        public ReadoutUnitSettings(ReadoutUnitSettings source)
            :this(source.Index, source.Name, source.TaskSettings)
        {
            return;
        }

        /// <summary>
        /// Creates the instance and initializes it from given xml element.
        /// </summary>
        /// <param name="index">Readout unit's zero-based index within output fields</param>
        /// <param name="elem">Xml data containing the settings.</param>
        public ReadoutUnitSettings(int index, XElement elem)
        {
            Index = index;
            //Validation
            XElement settingsElem = Validate(elem, XsdTypeName);
            //Parsing
            //Name
            Name = settingsElem.Attribute("name").Value;
            //Task
            XElement taskSettingsElem = settingsElem.Descendants().First();
            if (taskSettingsElem.Name.LocalName == "forecast")
            {
                TaskSettings = new ForecastTaskSettings(taskSettingsElem);
            }
            else
            {
                TaskSettings = new ClassificationTaskSettings(taskSettingsElem);
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
            if (Index < 0)
            {
                throw new Exception($"Invalid Index {Index.ToString(CultureInfo.InvariantCulture)}. Index must be GE to 0.");
            }
            if (Name.Length == 0)
            {
                throw new Exception($"Name can not be empty.");
            }
            return;
        }

        /// <summary>
        /// Creates the deep copy instance of this instance.
        /// </summary>
        public override RCNetBaseSettings DeepClone()
        {
            return new ReadoutUnitSettings(this);
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
            rootElem.Add(new XAttribute("name", Name));
            rootElem.Add(TaskSettings.GetXml(suppressDefaults));
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
            return GetXml("readoutUnit", suppressDefaults);
        }

    }//ReadoutUnitSettings

}//Namespace
