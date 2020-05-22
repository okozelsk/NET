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
    public class SpikeCodeSettings : RCNetBaseSettings
    {
        //Constants
        /// <summary>
        /// Name of the associated xsd type
        /// </summary>
        public const string XsdTypeName = "NPInputSpikeCodeType";
        //Default values
        /// <summary>
        /// Default value of parameter specifying length of the half of component code
        /// </summary>
        public const int DefaultComponentHalfCodeLength = 16;
        /// <summary>
        /// Default value of parameter specifying firing threshold of the most sensitive input neuron
        /// </summary>
        public const double DefaultLowestThreshold = 1e-5;
        /// <summary>
        /// Default value of parameter specifying if to use strength of the deviation from middle value as a component of the spike code
        /// </summary>
        public const bool DefaultUseDeviation = true;
        /// <summary>
        /// Default value of parameter specifying if to use strength of the deviation from previous value as a component of the spike code
        /// </summary>
        public const bool DefaultUseDifference = false;


        //Attribute properties
        /// <summary>
        /// Length of the half of component code
        /// </summary>
        public int ComponentHalfCodeLength { get; }

        /// <summary>
        /// Firing threshold of the most sensitive input neuron
        /// </summary>
        public double LowestThreshold { get; }

        /// <summary>
        /// Specifies if to use strength of the deviation from middle value as a component of the spike code
        /// </summary>
        public bool UseDeviation { get; }

        /// <summary>
        /// Specifies if to use strength of the deviation from previous value as a component of the spike code
        /// </summary>
        public bool UseDifference { get; }


        //Constructors
        /// <summary>
        /// Creates an itialized instance.
        /// </summary>
        /// <param name="componentHalfCodeLength">Length of the half of component code</param>
        /// <param name="lowestThreshold">Firing threshold of the most sensitive input neuron</param>
        /// <param name="useDeviation">Specifies if to use strength of the deviation from middle value as a component of the spike code</param>
        /// <param name="useDifference">Specifies if to use strength of the deviation from previous value as a component of the spike code</param>
        public SpikeCodeSettings(int componentHalfCodeLength = DefaultComponentHalfCodeLength,
                                 double lowestThreshold = DefaultLowestThreshold,
                                 bool useDeviation = DefaultUseDeviation,
                                 bool useDifference = DefaultUseDifference
                                 )
        {
            ComponentHalfCodeLength = componentHalfCodeLength;
            LowestThreshold = lowestThreshold;
            UseDeviation = useDeviation;
            UseDifference = useDifference;
            Check();
            return;
        }

        /// <summary>
        /// The deep copy constructor.
        /// </summary>
        /// <param name="source">Source instance</param>
        public SpikeCodeSettings(SpikeCodeSettings source)
            : this(source.ComponentHalfCodeLength, source.LowestThreshold, source.UseDeviation, source.UseDifference)
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
        public SpikeCodeSettings(XElement elem)
        {
            //Validation
            XElement settingsElem = Validate(elem, XsdTypeName);
            //Parsing
            ComponentHalfCodeLength = int.Parse(settingsElem.Attribute("componentHalfCodeLength").Value, CultureInfo.InvariantCulture);
            LowestThreshold = double.Parse(settingsElem.Attribute("lowestThreshold").Value, CultureInfo.InvariantCulture);
            UseDeviation = bool.Parse(settingsElem.Attribute("useDeviation").Value);
            UseDifference = bool.Parse(settingsElem.Attribute("useDifference").Value);
            Check();
            return;
        }

        //Properties
        /// <summary>
        /// Checks if settings are default
        /// </summary>
        public bool IsDefaultComponentHalfCodeLength { get { return (ComponentHalfCodeLength == DefaultComponentHalfCodeLength); } }

        /// <summary>
        /// Checks if settings are default
        /// </summary>
        public bool IsDefaultLowestThreshold { get { return (LowestThreshold == DefaultLowestThreshold); } }

        /// <summary>
        /// Checks if settings are default
        /// </summary>
        public bool IsDefaultUseDeviation { get { return (UseDeviation == DefaultUseDeviation); } }

        /// <summary>
        /// Checks if settings are default
        /// </summary>
        public bool IsDefaultUseDifference { get { return (UseDifference == DefaultUseDifference); } }

        /// <summary>
        /// Identifies settings containing only default values
        /// </summary>
        public override bool ContainsOnlyDefaults
        {
            get
            {
                return IsDefaultComponentHalfCodeLength &&
                       IsDefaultLowestThreshold &&
                       IsDefaultUseDeviation &&
                       IsDefaultUseDifference;
            }
        }

        //Methods
        /// <summary>
        /// Checks validity
        /// </summary>
        private void Check()
        {
            if (ComponentHalfCodeLength < 1 || ComponentHalfCodeLength > 1024)
            {
                throw new Exception($"Invalid ComponentHalfCodeLength {ComponentHalfCodeLength.ToString(CultureInfo.InvariantCulture)}. ComponentHalfCodeLength must be GE to 1 and LE to 1024.");
            }
            if (LowestThreshold <= 0 || LowestThreshold >= 1d)
            {
                throw new Exception($"Invalid LowestThreshold {LowestThreshold.ToString(CultureInfo.InvariantCulture)}. LowestThreshold must be GT 0 and LT 1.");
            }
            if(!UseDeviation && !UseDifference)
            {
                throw new Exception($"At least one component of the spike code has to be used.");
            }
            return;
        }

        /// <summary>
        /// Creates the deep copy instance of this instance
        /// </summary>
        public override RCNetBaseSettings DeepClone()
        {
            return new SpikeCodeSettings(this);
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
            if (!suppressDefaults || !IsDefaultComponentHalfCodeLength)
            {
                rootElem.Add(new XAttribute("componentHalfCodeLength", ComponentHalfCodeLength.ToString(CultureInfo.InvariantCulture)));
            }
            if (!suppressDefaults || !IsDefaultLowestThreshold)
            {
                rootElem.Add(new XAttribute("lowestThreshold", LowestThreshold.ToString(CultureInfo.InvariantCulture)));
            }
            if (!suppressDefaults || !IsDefaultUseDeviation)
            {
                rootElem.Add(new XAttribute("useDeviation", UseDeviation.ToString(CultureInfo.InvariantCulture).ToLowerInvariant()));
            }
            if (!suppressDefaults || !IsDefaultUseDifference)
            {
                rootElem.Add(new XAttribute("useDifference", UseDifference.ToString(CultureInfo.InvariantCulture).ToLowerInvariant()));
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
            return GetXml("spikeCode", suppressDefaults);
        }

    }//SpikeCodeSettings

}//Namespace

