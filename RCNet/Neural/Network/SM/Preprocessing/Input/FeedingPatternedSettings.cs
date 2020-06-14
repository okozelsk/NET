using RCNet.Neural.Data;
using System;
using System.Globalization;
using System.Linq;
using System.Xml.Linq;

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
        /// Default value of parameter specifying how many times to collect predictors during pattern data preprocessing
        /// </summary>
        public const int DefaultSlices = 1;
        /// <summary>
        /// Default value of parameter specifying if to preprocess time series pattern in both time directions (doubles predictors in total)
        /// </summary>
        public const bool DefaultBidir = false;
        /// <summary>
        /// Default value of parameter specifying variables organization in the pattern
        /// </summary>
        public const InputPattern.VariablesSchema DefaultVarSchema = InputPattern.VariablesSchema.Groupped;

        //Attribute properties
        /// <summary>
        /// Specifies how many times to collect predictors during pattern data preprocessing
        /// </summary>
        public int Slices { get; }

        /// <summary>
        /// Specifies whether to preprocess time series pattern in both time directions (doubles predictors in total)
        /// </summary>
        public bool Bidir { get; }

        /// <summary>
        /// Specifies variables organization in the pattern
        /// </summary>
        public InputPattern.VariablesSchema VarSchema { get; }

        /// <summary>
        /// Configuration of an input pattern unification
        /// </summary>
        public UnificationSettings UnificationCfg { get; }

        /// <summary>
        /// Steady external input fields settings
        /// </summary>
        public SteadyFieldsSettings SteadyFieldsCfg { get; }

        //Constructors
        /// <summary>
        /// Creates an itialized instance.
        /// </summary>
        /// <param name="slices">Specifies how many times to collect predictors during pattern data preprocessing</param>
        /// <param name="bidir">Specifies whether to preprocess time series pattern in both time directions (doubles predictors in total)</param>
        /// <param name="varSchema">Specifies variables organization in the pattern</param>
        /// <param name="unificationCfg">Configuration of an input pattern unification</param>
        /// <param name="steadyFieldsCfg">Steady external input fields settings</param>
        public FeedingPatternedSettings(int slices = DefaultSlices,
                                        bool bidir = DefaultBidir,
                                        InputPattern.VariablesSchema varSchema = DefaultVarSchema,
                                        UnificationSettings unificationCfg = null,
                                        SteadyFieldsSettings steadyFieldsCfg = null
                                        )
        {
            Slices = slices;
            Bidir = bidir;
            VarSchema = varSchema;
            UnificationCfg = unificationCfg == null ? new UnificationSettings() : (UnificationSettings)unificationCfg.DeepClone();
            SteadyFieldsCfg = steadyFieldsCfg == null ? null : (SteadyFieldsSettings)steadyFieldsCfg.DeepClone();
            Check();
            return;
        }

        /// <summary>
        /// The deep copy constructor.
        /// </summary>
        /// <param name="source">Source instance</param>
        public FeedingPatternedSettings(FeedingPatternedSettings source)
            : this(source.Slices, source.Bidir, source.VarSchema, source.UnificationCfg, source.SteadyFieldsCfg)
        {
            return;
        }

        /// <summary>
        /// Creates an initialized instance from the given xml element.
        /// </summary>
        /// <param name="elem">Xml element containing the initialization settings</param>
        public FeedingPatternedSettings(XElement elem)
        {
            //Validation
            XElement settingsElem = Validate(elem, XsdTypeName);
            //Parsing
            Slices = int.Parse(settingsElem.Attribute("slices").Value, CultureInfo.InvariantCulture);
            Bidir = bool.Parse(settingsElem.Attribute("bidir").Value);
            VarSchema = (InputPattern.VariablesSchema)Enum.Parse(typeof(InputPattern.VariablesSchema), settingsElem.Attribute("variablesSchema").Value, true);
            XElement uniElem = settingsElem.Elements("unification").FirstOrDefault();
            if (uniElem != null)
            {
                UnificationCfg = new UnificationSettings(uniElem);
            }
            else
            {
                UnificationCfg = new UnificationSettings();
            }
            XElement steadyFieldsElem = settingsElem.Elements("steadyFields").FirstOrDefault();
            if(steadyFieldsElem != null)
            {
                SteadyFieldsCfg = new SteadyFieldsSettings(steadyFieldsElem);
            }
            else
            {
                SteadyFieldsCfg = null;
            }
            Check();
            return;
        }

        //Properties
        /// <summary>
        /// Type of input feeding
        /// </summary>
        public InputEncoder.InputFeedingType FeedingType { get { return InputEncoder.InputFeedingType.Patterned; } }

        /// <summary>
        /// Number of steady external input fields
        /// </summary>
        public int NumOfSteadyFields { get { return SteadyFieldsCfg == null ? 0 : SteadyFieldsCfg.FieldCfgCollection.Count; } }

        /// <summary>
        /// Checks if settings are default
        /// </summary>
        public bool IsDefaultSlices { get { return (Slices == DefaultSlices); } }

        /// <summary>
        /// Checks if settings are default
        /// </summary>
        public bool IsDefaultBidir { get { return (Bidir == DefaultBidir); } }

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
                return IsDefaultSlices &&
                       IsDefaultBidir &&
                       IsDefaultVarSchema &&
                       UnificationCfg.ContainsOnlyDefaults &&
                       SteadyFieldsCfg == null;
            }
        }

        //Methods
        /// <summary>
        /// Checks consistency
        /// </summary>
        protected override void Check()
        {
            if (Slices <= 0)
            {
                throw new ArgumentException($"Invalid Slices {Slices.ToString(CultureInfo.InvariantCulture)}. Slices must be GT 0.", "Slices");
            }
            if(UnificationCfg.ResamplingCfg.TargetTimePoints != ResamplingSettings.AutoTargetTimePointsNum && Slices > UnificationCfg.ResamplingCfg.TargetTimePoints)
            {
                throw new ArgumentException($"Invalid Slices {Slices.ToString(CultureInfo.InvariantCulture)}. Slices must be LE to pattern timepoints ({UnificationCfg.ResamplingCfg.TargetTimePoints}).", "Slices");
            }
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
        /// <param name="suppressDefaults">Specifies whether to ommit optional nodes having set default values</param>
        /// <returns>XElement containing the settings</returns>
        public override XElement GetXml(string rootElemName, bool suppressDefaults)
        {
            XElement rootElem = new XElement(rootElemName);
            if (!suppressDefaults || !IsDefaultSlices)
            {
                rootElem.Add(new XAttribute("slices", Slices.ToString(CultureInfo.InvariantCulture)));
            }
            if (!suppressDefaults || !IsDefaultBidir)
            {
                rootElem.Add(new XAttribute("bidir", Bidir.ToString(CultureInfo.InvariantCulture).ToLowerInvariant()));
            }
            if (!suppressDefaults || !IsDefaultVarSchema)
            {
                rootElem.Add(new XAttribute("variablesSchema", VarSchema.ToString()));
            }
            if (!suppressDefaults || !UnificationCfg.ContainsOnlyDefaults)
            {
                rootElem.Add(UnificationCfg.GetXml(suppressDefaults));
            }
            if(SteadyFieldsCfg != null)
            {
                rootElem.Add(SteadyFieldsCfg.GetXml(suppressDefaults));
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
            return GetXml("feedingPatterned", suppressDefaults);
        }

    }//FeedingPatternedSettings

}//Namespace

