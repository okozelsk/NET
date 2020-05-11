using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using System.Globalization;
using System.Xml.Linq;
using System.Xml.XPath;
using System.IO;
using RCNet.Extensions;
using RCNet.XmlTools;
using RCNet.RandomValue;
using RCNet.Neural.Activation;

namespace RCNet.Neural.Network.SM.Preprocessing.Reservoir.Pool.NeuronGroup
{
    /// <summary>
    /// Configuration of homogenous excitability
    /// </summary>
    [Serializable]
    public class HomogenousExcitabilitySettings : RCNetBaseSettings
    {
        //Constants
        /// <summary>
        /// Name of the associated xsd type
        /// </summary>
        public const string XsdTypeName = "HomogenousExcitabilityType";
        //Default values
        /// <summary>
        /// Default input strength
        /// </summary>
        public const double DefaultInputStrength = 0.5d;
        /// <summary>
        /// Default excitatory strength
        /// </summary>
        public const double DefaultExcitatoryStrength = 0.5d;
        /// <summary>
        /// Default inhibitory ratio
        /// </summary>
        public const double DefaultInhibitoryRatio = 0.25d;

        //Attribute properties
        /// <summary>
        /// Strength of sum of all input synapses
        /// </summary>
        public double InputStrength { get; }

        /// <summary>
        /// Strength of sum of all excitatory synapses
        /// </summary>
        public double ExcitatoryStrength { get; }

        /// <summary>
        /// Determines inhibitory strength (inhibitory strength = InhibitoryRatio * (excitatory strength + input strength))
        /// </summary>
        public double InhibitoryRatio { get; }


        //Constructors
        /// <summary>
        /// Creates an initialized instance
        /// </summary>
        /// <param name="inputStrength">Strength of sum of all input synapses</param>
        /// <param name="excitatoryStrength">Strength of sum of all excitatory synapses</param>
        /// <param name="inhibitoryRatio">Determines inhibitory strength (inhibitory strength = InhibitoryRatio * (excitatory strength + input strength))</param>
        public HomogenousExcitabilitySettings(double inputStrength = DefaultInputStrength,
                                              double excitatoryStrength = DefaultExcitatoryStrength,
                                              double inhibitoryRatio = DefaultInhibitoryRatio
                                              )
        {
            InputStrength = inputStrength;
            ExcitatoryStrength = excitatoryStrength;
            InhibitoryRatio = inhibitoryRatio;
            Check();
            return;
        }

        /// <summary>
        /// The deep copy constructor
        /// </summary>
        /// <param name="source">Source instance</param>
        public HomogenousExcitabilitySettings(HomogenousExcitabilitySettings source)
            :this(source.InputStrength, source.ExcitatoryStrength, source.InhibitoryRatio)
        {
            return;
        }

        /// <summary>
        /// Creates the instance and initialize it from given xml element.
        /// </summary>
        /// <param name="elem">Xml data containing settings.</param>
        public HomogenousExcitabilitySettings(XElement elem)
        {
            //Validation
            XElement settingsElem = Validate(elem, XsdTypeName);
            //Parsing
            InputStrength = double.Parse(settingsElem.Attribute("inputStrength").Value, CultureInfo.InvariantCulture);
            ExcitatoryStrength = double.Parse(settingsElem.Attribute("excitatoryStrength").Value, CultureInfo.InvariantCulture);
            InhibitoryRatio = double.Parse(settingsElem.Attribute("inhibitoryRatio").Value, CultureInfo.InvariantCulture);
            Check();
            return;
        }

        //Properties
        /// <summary>
        /// Checks if settings are default
        /// </summary>
        public bool IsDefaultInputStrength { get { return (InputStrength == DefaultInputStrength); } }
        /// <summary>
        /// Checks if settings are default
        /// </summary>
        public bool IsDefaultExcitatoryStrength { get { return (ExcitatoryStrength == DefaultExcitatoryStrength); } }
        /// <summary>
        /// Checks if settings are default
        /// </summary>
        public bool IsDefaultInhibitoryRatio { get { return (InhibitoryRatio == DefaultInhibitoryRatio); } }

        /// <summary>
        /// Identifies settings containing only default values
        /// </summary>
        public override bool ContainsOnlyDefaults
        {
            get
            {
                return IsDefaultInputStrength &&
                       IsDefaultExcitatoryStrength &&
                       IsDefaultInhibitoryRatio;
            }
        }

        //Methods
        /// <summary>
        /// Checks validity
        /// </summary>
        private void Check()
        {
            if (InputStrength < 0)
            {
                throw new Exception($"Invalid InputStrength {InputStrength.ToString(CultureInfo.InvariantCulture)}. InputStrength must be GE to 0.");
            }
            if (ExcitatoryStrength <= InputStrength)
            {
                throw new Exception($"Invalid total ExcitatoryStrength {ExcitatoryStrength.ToString(CultureInfo.InvariantCulture)}. ExcitatoryStrength must be GT InputStrength.");
            }
            if (InhibitoryRatio < 0 || InhibitoryRatio > 1)
            {
                throw new Exception($"Invalid InhibitoryRatio {InhibitoryRatio.ToString(CultureInfo.InvariantCulture)}. InhibitoryRatio must be GE to 0 and LE to 1.");
            }
            return;
        }

        /// <summary>
        /// Creates the deep copy instance of this instance
        /// </summary>
        public override RCNetBaseSettings DeepClone()
        {
            return new HomogenousExcitabilitySettings(this);
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
            if (!suppressDefaults || !IsDefaultInputStrength)
            {
                rootElem.Add(new XAttribute("inputStrength", InputStrength.ToString(CultureInfo.InvariantCulture)));
            }
            if (!suppressDefaults || !IsDefaultExcitatoryStrength)
            {
                rootElem.Add(new XAttribute("excitatoryStrength", ExcitatoryStrength.ToString(CultureInfo.InvariantCulture)));
            }
            if (!suppressDefaults || !IsDefaultInhibitoryRatio)
            {
                rootElem.Add(new XAttribute("inhibitoryRatio", InhibitoryRatio.ToString(CultureInfo.InvariantCulture)));
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
            return GetXml("homogenousExcitability", suppressDefaults);
        }

    }//HomogenousExcitabilitySettings

}//Namespace
