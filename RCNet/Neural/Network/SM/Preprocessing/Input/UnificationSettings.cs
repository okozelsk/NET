using System;
using System.Globalization;
using System.Linq;
using System.Xml.Linq;

namespace RCNet.Neural.Network.SM.Preprocessing.Input
{
    /// <summary>
    /// Configuration of the input pattern unification
    /// </summary>
    [Serializable]
    public class UnificationSettings : RCNetBaseSettings
    {
        //Constants
        /// <summary>
        /// Name of the associated xsd type
        /// </summary>
        public const string XsdTypeName = "NPUnificationType";
        //Default values
        /// <summary>
        /// Default value of the parameter specifying if to detrend input pattern data
        /// </summary>
        public const bool DefaultDetrend = false;
        /// <summary>
        /// Default value of the parameter specifying if to unify amplitude of input pattern data
        /// </summary>
        public const bool DefaultUnifyAmplitude = false;

        //Attribute properties
        /// <summary>
        /// Specifies whether to detrend input pattern data
        /// </summary>
        public bool Detrend { get; }

        /// <summary>
        /// Specifies whether to unify amplitude of input pattern data
        /// </summary>
        public bool UnifyAmplitude { get; }

        /// <summary>
        /// Settings of input pattern resampling
        /// </summary>
        public ResamplingSettings ResamplingCfg { get; }

        //Constructors
        /// <summary>
        /// Creates an itialized instance.
        /// </summary>
        /// <param name="detrend">Specifies whether to detrend input pattern data</param>
        /// <param name="unifyAmplitude">Specifies whether to unify amplitude of input pattern data</param>
        /// <param name="resamplingCfg">Settings of input pattern resampling</param>
        public UnificationSettings(bool detrend = DefaultDetrend,
                                   bool unifyAmplitude = DefaultUnifyAmplitude,
                                   ResamplingSettings resamplingCfg = null
                                   )
        {
            Detrend = detrend;
            UnifyAmplitude = unifyAmplitude;
            ResamplingCfg = resamplingCfg == null ? new ResamplingSettings() : (ResamplingSettings)resamplingCfg.DeepClone();
            Check();
            return;
        }

        /// <summary>
        /// The deep copy constructor.
        /// </summary>
        /// <param name="source">Source instance</param>
        public UnificationSettings(UnificationSettings source)
            : this(source.Detrend, source.UnifyAmplitude, source.ResamplingCfg)
        {
            return;
        }

        /// <summary>
        /// Creates an initialized instance from the given xml element.
        /// </summary>
        /// <param name="elem">Xml element containing the initialization settings</param>
        public UnificationSettings(XElement elem)
        {
            //Validation
            XElement settingsElem = Validate(elem, XsdTypeName);
            //Parsing
            Detrend = bool.Parse(settingsElem.Attribute("detrend").Value);
            UnifyAmplitude = bool.Parse(settingsElem.Attribute("unifyAmplitude").Value);
            XElement resamplingElem = settingsElem.Elements("resampling").FirstOrDefault();
            if (resamplingElem != null)
            {
                ResamplingCfg = new ResamplingSettings(resamplingElem);
            }
            else
            {
                ResamplingCfg = new ResamplingSettings();
            }
            Check();
            return;
        }

        //Properties
        /// <summary>
        /// Checks the defaults
        /// </summary>
        public bool IsDefaultDetrend { get { return (Detrend == DefaultDetrend); } }

        /// <summary>
        /// Checks the defaults
        /// </summary>
        public bool IsDefaultUnifyAmplitude { get { return (UnifyAmplitude == DefaultUnifyAmplitude); } }

        /// <inheritdoc/>
        public override bool ContainsOnlyDefaults
        {
            get
            {
                return IsDefaultDetrend &&
                       IsDefaultUnifyAmplitude &&
                       ResamplingCfg.ContainsOnlyDefaults;
            }
        }

        //Methods
        /// <inheritdoc/>
        protected override void Check()
        {
            return;
        }

        /// <inheritdoc/>
        public override RCNetBaseSettings DeepClone()
        {
            return new UnificationSettings(this);
        }

        /// <inheritdoc/>
        public override XElement GetXml(string rootElemName, bool suppressDefaults)
        {
            XElement rootElem = new XElement(rootElemName);
            if (!suppressDefaults || !IsDefaultDetrend)
            {
                rootElem.Add(new XAttribute("detrend", Detrend.ToString(CultureInfo.InvariantCulture).ToLowerInvariant()));
            }
            if (!suppressDefaults || !IsDefaultUnifyAmplitude)
            {
                rootElem.Add(new XAttribute("unifyAmplitude", UnifyAmplitude.ToString(CultureInfo.InvariantCulture).ToLowerInvariant()));
            }
            if (!suppressDefaults || !ResamplingCfg.ContainsOnlyDefaults)
            {
                rootElem.Add(ResamplingCfg.GetXml(suppressDefaults));
            }
            Validate(rootElem, XsdTypeName);
            return rootElem;
        }

        /// <inheritdoc/>
        public override XElement GetXml(bool suppressDefaults)
        {
            return GetXml("unification", suppressDefaults);
        }

    }//UnificationSettings

}//Namespace

