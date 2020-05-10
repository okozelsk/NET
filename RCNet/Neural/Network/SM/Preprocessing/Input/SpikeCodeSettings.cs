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
        public const int DefaultComponentHalfCodeLength = 8;
        /// <summary>
        /// Default value of parameter specifying exponential slicer
        /// </summary>
        public const double DefaultBoundariesSlicer = 2.7182818284590451d;
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
        /// Exponential slicer
        /// </summary>
        public double BoundariesSlicer { get; }

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
        /// <param name="boundariesSlicer">Exponential slicer</param>
        /// <param name="useDeviation">Specifies if to use strength of the deviation from middle value as a component of the spike code</param>
        /// <param name="useDifference">Specifies if to use strength of the deviation from previous value as a component of the spike code</param>
        public SpikeCodeSettings(int componentHalfCodeLength = DefaultComponentHalfCodeLength,
                                 double boundariesSlicer = DefaultBoundariesSlicer,
                                 bool useDeviation = DefaultUseDeviation,
                                 bool useDifference = DefaultUseDifference
                                 )
        {
            ComponentHalfCodeLength = componentHalfCodeLength;
            BoundariesSlicer = boundariesSlicer;
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
            : this(source.ComponentHalfCodeLength, source.BoundariesSlicer, source.UseDeviation, source.UseDifference)
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
            BoundariesSlicer = double.Parse(settingsElem.Attribute("boundariesSlicer").Value, CultureInfo.InvariantCulture);
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
        public bool IsDefaultBoundariesSlicer { get { return (BoundariesSlicer == DefaultBoundariesSlicer); } }

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
                       IsDefaultBoundariesSlicer &&
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
            if (ComponentHalfCodeLength < 1 || ComponentHalfCodeLength > 64)
            {
                throw new Exception($"Invalid ComponentHalfCodeLength {ComponentHalfCodeLength.ToString(CultureInfo.InvariantCulture)}. ComponentHalfCodeLength must be GE to 1 and LE to 64.");
            }
            if (BoundariesSlicer <= 1)
            {
                throw new Exception($"Invalid BoundariesSlicer {BoundariesSlicer.ToString(CultureInfo.InvariantCulture)}. BoundariesSlicer must be GT 1.");
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
            if (!suppressDefaults || !IsDefaultBoundariesSlicer)
            {
                rootElem.Add(new XAttribute("boundariesSlicer", BoundariesSlicer.ToString(CultureInfo.InvariantCulture)));
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

