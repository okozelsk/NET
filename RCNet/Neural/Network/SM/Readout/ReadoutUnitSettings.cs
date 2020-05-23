using System;
using System.Linq;
using System.Xml.Linq;

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
        public const string XsdTypeName = "ROutLayerUnitType";

        //Attribute properties
        /// <summary>
        /// Output field name
        /// </summary>
        public string Name { get; }

        /// <summary>
        /// Readout unit's Forecast or Classification task settings
        /// </summary>
        public ITaskSettings TaskCfg { get; }

        //Constructors
        /// <summary>
        /// Creates an unitialized instance
        /// </summary>
        /// <param name="name">Output field name</param>
        /// <param name="taskCfg">Readout unit's Forecast or Classification task settings</param>
        public ReadoutUnitSettings(string name, ITaskSettings taskCfg)
        {
            Name = name;
            TaskCfg = (ITaskSettings)taskCfg.DeepClone();
            Check();
            return;
        }

        /// <summary>
        /// Copy constructor
        /// </summary>
        /// <param name="source">Source instance</param>
        public ReadoutUnitSettings(ReadoutUnitSettings source)
            : this(source.Name, source.TaskCfg)
        {
            return;
        }

        /// <summary>
        /// Creates an initialized instance.
        /// </summary>
        /// <param name="elem">Xml data containing the settings.</param>
        public ReadoutUnitSettings(XElement elem)
        {
            //Validation
            XElement settingsElem = Validate(elem, XsdTypeName);
            //Parsing
            //Name
            Name = settingsElem.Attribute("name").Value;
            //Task
            XElement taskSettingsElem = settingsElem.Elements().First();
            if (taskSettingsElem.Name.LocalName == "forecast")
            {
                TaskCfg = new ForecastTaskSettings(taskSettingsElem);
            }
            else
            {
                TaskCfg = new ClassificationTaskSettings(taskSettingsElem);
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
        /// Checks consistency
        /// </summary>
        protected override void Check()
        {
            if (Name.Length == 0)
            {
                throw new ArgumentException($"Name can not be empty.", "Name");
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
        /// <param name="suppressDefaults">Specifies whether to ommit optional nodes having set default values</param>
        /// <returns>XElement containing the settings</returns>
        public override XElement GetXml(string rootElemName, bool suppressDefaults)
        {
            XElement rootElem = new XElement(rootElemName);
            rootElem.Add(new XAttribute("name", Name));
            rootElem.Add(TaskCfg.GetXml(suppressDefaults));
            Validate(rootElem, XsdTypeName);
            return rootElem;
        }

        /// <summary>
        /// Generates default named xml element containing the settings.
        /// </summary>
        /// <param name="suppressDefaults">Specifies whether to ommit optional nodes having set default values</param>
        /// <returns>XElement containing the settings</returns>
        public override XElement GetXml(bool suppressDefaults)
        {
            return GetXml("readoutUnit", suppressDefaults);
        }

    }//ReadoutUnitSettings

}//Namespace
