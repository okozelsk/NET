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
    /// Settings of input pattern unification
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
        /// Specifies if to detrend input pattern data
        /// </summary>
        public bool Detrend { get; }

        /// <summary>
        /// Specifies if to unify amplitude of input pattern data
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
        /// <param name="detrend">Specifies if to detrend input pattern data</param>
        /// <param name="unifyAmplitude">Specifies if to unify amplitude of input pattern data</param>
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
        /// <param name="elem">
        /// Xml data containing the settings.
        /// Content of xml element is always validated against the xml schema.
        /// </param>
        public UnificationSettings(XElement elem)
        {
            //Validation
            XElement settingsElem = Validate(elem, XsdTypeName);
            //Parsing
            Detrend = bool.Parse(settingsElem.Attribute("detrend").Value);
            UnifyAmplitude = bool.Parse(settingsElem.Attribute("unifyAmplitude").Value);
            XElement resamplingElem = settingsElem.Elements("resampling").FirstOrDefault();
            if(resamplingElem != null)
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
        /// Checks if settings are default
        /// </summary>
        public bool IsDefaultDetrend { get { return (Detrend == DefaultDetrend); } }

        /// <summary>
        /// Checks if settings are default
        /// </summary>
        public bool IsDefaultUnifyAmplitude { get { return (UnifyAmplitude == DefaultUnifyAmplitude); } }

        /// <summary>
        /// Identifies settings containing only default values
        /// </summary>
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
            return new UnificationSettings(this);
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

        /// <summary>
        /// Generates default named xml element containing the settings.
        /// </summary>
        /// <param name="suppressDefaults">Specifies if to ommit optional nodes having set default values</param>
        /// <returns>XElement containing the settings</returns>
        public override XElement GetXml(bool suppressDefaults)
        {
            return GetXml("unification", suppressDefaults);
        }

    }//UnificationSettings

}//Namespace

