using System;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using System.Reflection;
using System.Text;
using System.Threading.Tasks;
using System.Xml.Linq;
using RCNet.Extensions;
using RCNet.Neural.Data;

namespace RCNet.Neural.Network.SM.Preprocessing.Input
{
    /// <summary>
    /// Settings of continuous input feeding regime
    /// </summary>
    [Serializable]
    public class FeedingPatternedSettings : RCNetBaseSettings, IFeedingSettings
    {
        //Constants
        /// <summary>
        /// Name of the associated xsd type
        /// </summary>
        public const string XsdTypeName = "NPInpFeedingPatternedType";
        //Default values
        /// <summary>
        /// Default value of parameter specifying if to preprocess time series pattern in both time directions (doubles predictors)
        /// </summary>
        public const bool DefaultBidir = false;
        /// <summary>
        /// Default value of parameter specifying if to route input fields to readout layer together with other predictors 
        /// </summary>
        public const bool DefaultRouteToReadout = false;
        /// <summary>
        /// Default value of parameter specifying variables organization in the pattern
        /// </summary>
        public const InputPattern.VariablesSchema DefaultVarSchema = InputPattern.VariablesSchema.Groupped;

        //Attribute properties
        /// <summary>
        /// Specifies if to preprocess time series pattern in both time directions (doubles predictors)
        /// </summary>
        public bool Bidir { get; }

        /// <summary>
        /// Specifies if to route input fields to readout layer together with other predictors
        /// </summary>
        public bool RouteToReadout { get; }

        /// <summary>
        /// Specifies variables organization in the pattern
        /// </summary>
        public InputPattern.VariablesSchema VarSchema { get; }

        /// <summary>
        /// Configuration of an input pattern unification
        /// </summary>
        public UnificationSettings UnificationCfg { get; }

        //Constructors
        /// <summary>
        /// Creates an itialized instance.
        /// </summary>
        /// <param name="bidir">Specifies if to preprocess time series pattern in both time directions (doubles predictors)</param>
        /// <param name="routeToReadout">Specifies if to route input fields to readout layer together with other predictors</param>
        /// <param name="varSchema">Specifies variables organization in the pattern</param>
        /// <param name="unificationCfg">Configuration of an input pattern unification</param>
        public FeedingPatternedSettings(bool bidir = DefaultBidir,
                                        bool routeToReadout = DefaultRouteToReadout,
                                        InputPattern.VariablesSchema varSchema = DefaultVarSchema,
                                        UnificationSettings unificationCfg = null
                                        )
        {
            Bidir = bidir;
            RouteToReadout = routeToReadout;
            VarSchema = varSchema;
            UnificationCfg = unificationCfg == null ? new UnificationSettings() : (UnificationSettings)unificationCfg.DeepClone();
            Check();
            return;
        }

        /// <summary>
        /// The deep copy constructor.
        /// </summary>
        /// <param name="source">Source instance</param>
        public FeedingPatternedSettings(FeedingPatternedSettings source)
            : this(source.Bidir, source.RouteToReadout, source.VarSchema, source.UnificationCfg)
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
        public FeedingPatternedSettings(XElement elem)
        {
            //Validation
            XElement settingsElem = Validate(elem, XsdTypeName);
            //Parsing
            Bidir = bool.Parse(settingsElem.Attribute("bidir").Value);
            RouteToReadout = bool.Parse(settingsElem.Attribute("routeToReadout").Value);
            VarSchema = (InputPattern.VariablesSchema)Enum.Parse(typeof(InputPattern.VariablesSchema), settingsElem.Attribute("variablesSchema").Value, true);
            XElement uniElem = settingsElem.Elements("unification").FirstOrDefault();
            if(uniElem != null)
            {
                UnificationCfg = new UnificationSettings(uniElem);
            }
            else
            {
                UnificationCfg = new UnificationSettings();
            }
            Check();
            return;
        }

        //Properties
        /// <summary>
        /// Type of input feeding
        /// </summary>
        public NeuralPreprocessor.InputFeedingType FeedingType { get { return NeuralPreprocessor.InputFeedingType.Patterned; } }

        /// <summary>
        /// Checks if settings are default
        /// </summary>
        public bool IsDefaultBidir { get { return (Bidir == DefaultBidir); } }

        /// <summary>
        /// Checks if settings are default
        /// </summary>
        public bool IsDefaultRouteToReadout { get { return (RouteToReadout == DefaultRouteToReadout); } }

        /// <summary>
        /// Checks if settings are default
        /// </summary>
        public bool IsDefaultVarSchema { get { return (VarSchema == DefaultVarSchema); } }

        /// <summary>
        /// Identifies settings containing only default values
        /// </summary>
        public override bool ContainsOnlyDefaults
        {
            get
            {
                return IsDefaultBidir &&
                       IsDefaultRouteToReadout &&
                       IsDefaultVarSchema &&
                       UnificationCfg.ContainsOnlyDefaults;
            }
        }

        //Methods
        /// <summary>
        /// Checks validity
        /// </summary>
        private void Check()
        {
            return;
        }

        /// <summary>
        /// Creates the deep copy instance of this instance
        /// </summary>
        public override RCNetBaseSettings DeepClone()
        {
            return new FeedingPatternedSettings(this);
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
            if (!suppressDefaults || !IsDefaultBidir)
            {
                rootElem.Add(new XAttribute("bidir", Bidir.ToString(CultureInfo.InvariantCulture).ToLowerInvariant()));
            }
            if (!suppressDefaults || !IsDefaultRouteToReadout)
            {
                rootElem.Add(new XAttribute("routeToReadout", RouteToReadout.ToString(CultureInfo.InvariantCulture).ToLowerInvariant()));
            }
            if (!suppressDefaults || !IsDefaultVarSchema)
            {
                rootElem.Add(new XAttribute("variablesSchema", VarSchema.ToString()));
            }
            if (!suppressDefaults || !UnificationCfg.ContainsOnlyDefaults)
            {
                rootElem.Add(UnificationCfg.GetXml(suppressDefaults));
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
            return GetXml("feedingPatterned", suppressDefaults);
        }

    }//FeedingPatternedSettings

}//Namespace

