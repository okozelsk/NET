using System;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using System.Reflection;
using System.Text;
using System.Threading.Tasks;
using System.Xml.Linq;
using RCNet.Extensions;

namespace RCNet.Neural.Network.SM.Preprocessing.Input
{
    /// <summary>
    /// Settings of input pattern resampling
    /// </summary>
    [Serializable]
    public class SpikingCodingSettings : RCNetBaseSettings
    {
        //Constants
        /// <summary>
        /// Name of the associated xsd type
        /// </summary>
        public const string XsdTypeName = "NPInputSpikingCodingType";
        //Default values
        /// <summary>
        /// Default value of parameter specifying size of coding spiking neurons population
        /// </summary>
        public const int DefaultPopulationSize = 16;

        //Attribute properties
        /// <summary>
        /// Size of coding spiking neurons population
        /// </summary>
        public int PopulationSize { get; }

        //Constructors
        /// <summary>
        /// Creates an itialized instance.
        /// </summary>
        /// <param name="populationSize">Size of coding spiking neurons population</param>
        public SpikingCodingSettings(int populationSize = DefaultPopulationSize)
        {
            PopulationSize = populationSize;
            Check();
            return;
        }

        /// <summary>
        /// The deep copy constructor.
        /// </summary>
        /// <param name="source">Source instance</param>
        public SpikingCodingSettings(SpikingCodingSettings source)
            : this(source.PopulationSize)
        {
            return;
        }

        /// <summary>
        /// Creates an initialized instance from the given xml element.
        /// </summary>
        /// <param name="elem">
        /// Xml data containing the settings.
        /// Content of xml element is always validated against the xml schema.
        /// </param>
        public SpikingCodingSettings(XElement elem)
        {
            //Validation
            XElement settingsElem = Validate(elem, XsdTypeName);
            //Parsing
            PopulationSize = int.Parse(settingsElem.Attribute("populationSize").Value, CultureInfo.InvariantCulture);
            Check();
            return;
        }

        //Properties
        /// <summary>
        /// Checks if settings are default
        /// </summary>
        public bool IsDefaultPopulationSize { get { return (PopulationSize == DefaultPopulationSize); } }

        /// <summary>
        /// Identifies settings containing only default values
        /// </summary>
        public override bool ContainsOnlyDefaults
        {
            get
            {
                return IsDefaultPopulationSize;
            }
        }

        //Methods
        /// <summary>
        /// Checks validity
        /// </summary>
        private void Check()
        {
            if (PopulationSize < 2 || PopulationSize > 128 || (PopulationSize % 2) != 0)
            {
                throw new Exception($"Invalid PopulationSize {PopulationSize.ToString(CultureInfo.InvariantCulture)}. PopulationSize must be even integer GE to 2 and LE to 128.");
            }
            return;
        }

        /// <summary>
        /// Creates the deep copy instance of this instance
        /// </summary>
        public override RCNetBaseSettings DeepClone()
        {
            return new SpikingCodingSettings(this);
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
            if (!suppressDefaults || !IsDefaultPopulationSize)
            {
                rootElem.Add(new XAttribute("populationSize", PopulationSize.ToString(CultureInfo.InvariantCulture)));
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
            return GetXml("spikingCoding", suppressDefaults);
        }

    }//SpikingCodingSettings

}//Namespace

