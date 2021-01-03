using System;
using System.Linq;
using System.Xml.Linq;

namespace RCNet.Neural.Network.SM.Readout
{
    /// <summary>
    /// Configuration of the readout unit.
    /// </summary>
    [Serializable]
    public class ReadoutUnitSettings : RCNetBaseSettings
    {
        //Constants
        /// <summary>
        /// The name of the associated xsd type.
        /// </summary>
        public const string XsdTypeName = "ROutUnitType";

        //Attribute properties
        /// <summary>
        /// The name of the readout unit (the output field name).
        /// </summary>
        public string Name { get; }

        /// <summary>
        /// The readout unit's task configuration.
        /// </summary>
        public ITaskSettings TaskCfg { get; }

        //Constructors
        /// <summary>
        /// Creates an initialized instance.
        /// </summary>
        /// <param name="name">The name of the readout unit (the output field name).</param>
        /// <param name="taskCfg">The readout unit's task configuration.</param>
        public ReadoutUnitSettings(string name, ITaskSettings taskCfg)
        {
            Name = name;
            TaskCfg = (ITaskSettings)taskCfg.DeepClone();
            Check();
            return;
        }

        /// <summary>
        /// The copy constructor.
        /// </summary>
        /// <param name="source">The source instance.</param>
        public ReadoutUnitSettings(ReadoutUnitSettings source)
            : this(source.Name, source.TaskCfg)
        {
            return;
        }

        /// <summary>
        /// Creates an initialized instance.
        /// </summary>
        /// <param name="elem">A xml element containing the configuration data.</param>
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
            else if (taskSettingsElem.Name.LocalName == "classification")
            {
                TaskCfg = new ClassificationTaskSettings(taskSettingsElem);
            }
            else
            {
                throw new ArgumentException("Configuration element does not contain valid task specification.", "elem");
            }
            Check();
            return;
        }

        //Properties
        /// <inheritdoc />
        public override bool ContainsOnlyDefaults { get { return false; } }

        //Methods
        /// <inheritdoc />
        protected override void Check()
        {
            if (Name.Length == 0)
            {
                throw new ArgumentException($"Name can not be empty.", "Name");
            }
            return;
        }

        /// <inheritdoc />
        public override RCNetBaseSettings DeepClone()
        {
            return new ReadoutUnitSettings(this);
        }

        /// <inheritdoc />
        public override XElement GetXml(string rootElemName, bool suppressDefaults)
        {
            XElement rootElem = new XElement(rootElemName);
            rootElem.Add(new XAttribute("name", Name));
            rootElem.Add(TaskCfg.GetXml(suppressDefaults));
            Validate(rootElem, XsdTypeName);
            return rootElem;
        }

        /// <inheritdoc />
        public override XElement GetXml(bool suppressDefaults)
        {
            return GetXml("readoutUnit", suppressDefaults);
        }

    }//ReadoutUnitSettings

}//Namespace
