using System;
using System.Globalization;
using System.Xml.Linq;

namespace RCNet.Neural.Data.Filter
{
    /// <summary>
    /// Startup parameters for the real number feature filter
    /// </summary>
    [Serializable]
    public class RealFeatureFilterSettings : RCNetBaseSettings, IFeatureFilterSettings
    {
        //Constants
        /// <summary>
        /// Name of the associated xsd type
        /// </summary>
        public const string XsdTypeName = "RealFeatureFilterType";

        //Default values
        /// <summary>
        /// Default value of Standardize
        /// </summary>
        public const bool DefaultStandardize = true;
        /// <summary>
        /// Default value of KeepReserve
        /// </summary>
        public const bool DefaultKeepReserve = true;

        //Attribute properties
        /// <summary>
        /// Standardize?
        /// </summary>
        public bool Standardize { get; }
        /// <summary>
        /// Keep range reserve?
        /// </summary>
        public bool KeepReserve { get; }

        //Constructors
        /// <summary>
        /// Constructs an initialized instance
        /// </summary>
        /// <param name="standardize">Standardize?</param>
        /// <param name="keepReserve">Keep range reserve?</param>
        public RealFeatureFilterSettings(bool standardize = DefaultStandardize,
                                         bool keepReserve = DefaultKeepReserve
                                         )
        {
            Standardize = standardize;
            KeepReserve = keepReserve;
            return;
        }

        /// <summary>
        /// Deep copy constructor
        /// </summary>
        /// <param name="source">Source instance</param>
        public RealFeatureFilterSettings(RealFeatureFilterSettings source)
            : this(source.Standardize, source.KeepReserve)
        {
            return;
        }

        /// <summary>
        /// Creates an initialized instance.
        /// </summary>
        /// <param name="elem">Xml element containing the initialization settings</param>
        public RealFeatureFilterSettings(XElement elem)
        {
            //Validation
            XElement settingsElem = Validate(elem, XsdTypeName);
            //Parsing
            Standardize = bool.Parse(settingsElem.Attribute("standardize").Value);
            KeepReserve = bool.Parse(settingsElem.Attribute("keepReserve").Value);
            return;
        }

        //Properties
        /// <summary>
        /// Feature type
        /// </summary>
        public FeatureFilterBase.FeatureType Type { get { return FeatureFilterBase.FeatureType.Real; } }

        /// <summary>
        /// Checks if settings are default
        /// </summary>
        public bool IsDefaultStandardize { get { return (Standardize == DefaultStandardize); } }

        /// <summary>
        /// Checks if settings are default
        /// </summary>
        public bool IsDefaultKeepReserve { get { return (KeepReserve == DefaultKeepReserve); } }

        /// <summary>
        /// Identifies settings containing only default values
        /// </summary>
        public override bool ContainsOnlyDefaults { get { return IsDefaultStandardize && IsDefaultKeepReserve; } }

        //Methods
        /// <summary>
        /// Checks consistency
        /// </summary>
        protected override void Check()
        {
            return;
        }

        /// <summary>
        /// Creates the deep copy instance of this instance
        /// </summary>
        public override RCNetBaseSettings DeepClone()
        {
            return new RealFeatureFilterSettings(this);
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
            if (!suppressDefaults || !IsDefaultStandardize)
            {
                rootElem.Add(new XAttribute("standardize", Standardize.ToString(CultureInfo.InvariantCulture).ToLowerInvariant()));
            }
            if (!suppressDefaults || !IsDefaultKeepReserve)
            {
                rootElem.Add(new XAttribute("keepReserve", KeepReserve.ToString(CultureInfo.InvariantCulture).ToLowerInvariant()));
            }
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
            return GetXml("realFeature", suppressDefaults);
        }

    }//RealFeatureFilterSettings

}//Namespace
