using System;
using System.Globalization;
using System.Xml.Linq;

namespace RCNet.Neural.Data.Filter
{
    /// <summary>
    /// Configuration of the real number feature filter
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
        /// <inheritdoc/>
        public FeatureFilterBase.FeatureType Type { get { return FeatureFilterBase.FeatureType.Real; } }

        /// <summary>
        /// Checks the defaults
        /// </summary>
        public bool IsDefaultStandardize { get { return (Standardize == DefaultStandardize); } }

        /// <summary>
        /// Checks the defaults
        /// </summary>
        public bool IsDefaultKeepReserve { get { return (KeepReserve == DefaultKeepReserve); } }

        /// <inheritdoc/>
        public override bool ContainsOnlyDefaults { get { return IsDefaultStandardize && IsDefaultKeepReserve; } }

        //Methods
        /// <inheritdoc/>
        protected override void Check()
        {
            return;
        }

        /// <inheritdoc/>
        public override RCNetBaseSettings DeepClone()
        {
            return new RealFeatureFilterSettings(this);
        }

        /// <inheritdoc/>
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

        /// <inheritdoc/>
        public override XElement GetXml(bool suppressDefaults)
        {
            return GetXml("realFeature", suppressDefaults);
        }

    }//RealFeatureFilterSettings

}//Namespace
