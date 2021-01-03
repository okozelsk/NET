using RCNet.Neural.Data;
using System;
using System.Globalization;
using System.Linq;
using System.Xml.Linq;

namespace RCNet.Neural.Network.SM.Preprocessing.Input
{
    /// <summary>
    /// Configuration of the patterned input feeding regime
    /// </summary>
    [Serializable]
    public class FeedingPatternedSettings : RCNetBaseSettings, IFeedingSettings
    {
        //Constants
        /// <summary>
        /// The name of the associated xsd type.
        /// </summary>
        public const string XsdTypeName = "NPInpFeedingPatternedType";
        //Default values
        /// <summary>
        /// The default value of the parameter specifying how many times to collect predictors during the pattern preprocessing.
        /// </summary>
        public const int DefaultSlices = 1;
        /// <summary>
        /// The default value of the parameter specifying whether and how to preprocess pattern in both time directions.
        /// </summary>
        public const NeuralPreprocessor.BidirProcessing DefaultBidir = NeuralPreprocessor.BidirProcessing.Forbidden;
        /// <summary>
        /// The default value of the parameter specifying the variables organization schema in the pattern.
        /// </summary>
        public const InputPattern.VariablesSchema DefaultVarSchema = InputPattern.VariablesSchema.Groupped;

        //Attribute properties
        /// <summary>
        /// Specifies how many times to collect predictors during the pattern preprocessing.
        /// </summary>
        public int Slices { get; }

        /// <summary>
        /// Specifies whether and how to preprocess pattern in both time directions.
        /// </summary>
        public NeuralPreprocessor.BidirProcessing Bidir { get; }

        /// <summary>
        /// Specifies the variables organization schema in the pattern.
        /// </summary>
        public InputPattern.VariablesSchema VarSchema { get; }

        /// <summary>
        /// The configuration of the pattern unification.
        /// </summary>
        public UnificationSettings UnificationCfg { get; }

        /// <summary>
        /// The configuration of the steady external input fields.
        /// </summary>
        public SteadyFieldsSettings SteadyFieldsCfg { get; }

        //Constructors
        /// <summary>
        /// Creates an itialized instance.
        /// </summary>
        /// <param name="slices">Specifies how many times to collect predictors during the pattern preprocessing.</param>
        /// <param name="bidir">Specifies whether and how to preprocess pattern in both time directions.</param>
        /// <param name="varSchema">Specifies the variables organization schema in the pattern.</param>
        /// <param name="unificationCfg">The configuration of the pattern unification.</param>
        /// <param name="steadyFieldsCfg">The configuration of the steady external input fields.</param>
        public FeedingPatternedSettings(int slices = DefaultSlices,
                                        NeuralPreprocessor.BidirProcessing bidir = DefaultBidir,
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
        /// <param name="source">The source instance.</param>
        public FeedingPatternedSettings(FeedingPatternedSettings source)
            : this(source.Slices, source.Bidir, source.VarSchema, source.UnificationCfg, source.SteadyFieldsCfg)
        {
            return;
        }

        /// <summary>
        /// Creates an initialized instance.
        /// </summary>
        /// <param name="elem">A xml element containing the configuration data.</param>
        public FeedingPatternedSettings(XElement elem)
        {
            //Validation
            XElement settingsElem = Validate(elem, XsdTypeName);
            //Parsing
            Slices = int.Parse(settingsElem.Attribute("slices").Value, CultureInfo.InvariantCulture);
            Bidir = (NeuralPreprocessor.BidirProcessing)Enum.Parse(typeof(NeuralPreprocessor.BidirProcessing), settingsElem.Attribute("bidir").Value, true);
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
            if (steadyFieldsElem != null)
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
        /// <inheritdoc/>
        public InputEncoder.InputFeedingType FeedingType { get { return InputEncoder.InputFeedingType.Patterned; } }

        /// <summary>
        /// Checks the defaults.
        /// </summary>
        public bool IsDefaultSlices { get { return (Slices == DefaultSlices); } }

        /// <summary>
        /// Checks the defaults.
        /// </summary>
        public bool IsDefaultBidir { get { return (Bidir == DefaultBidir); } }

        /// <summary>
        /// Checks the defaults.
        /// </summary>
        public bool IsDefaultVarSchema { get { return (VarSchema == DefaultVarSchema); } }

        /// <inheritdoc/>
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
        /// <inheritdoc/>
        protected override void Check()
        {
            if (Slices <= 0)
            {
                throw new ArgumentException($"Invalid Slices {Slices.ToString(CultureInfo.InvariantCulture)}. Slices must be GT 0.", "Slices");
            }
            if (UnificationCfg.ResamplingCfg.TargetTimePoints != ResamplingSettings.AutoTargetTimePointsNum && Slices > UnificationCfg.ResamplingCfg.TargetTimePoints)
            {
                throw new ArgumentException($"Invalid Slices {Slices.ToString(CultureInfo.InvariantCulture)}. Slices must be LE to pattern timepoints ({UnificationCfg.ResamplingCfg.TargetTimePoints}).", "Slices");
            }
            return;
        }

        /// <inheritdoc/>
        public override RCNetBaseSettings DeepClone()
        {
            return new FeedingPatternedSettings(this);
        }

        /// <inheritdoc/>
        public override XElement GetXml(string rootElemName, bool suppressDefaults)
        {
            XElement rootElem = new XElement(rootElemName);
            if (!suppressDefaults || !IsDefaultSlices)
            {
                rootElem.Add(new XAttribute("slices", Slices.ToString(CultureInfo.InvariantCulture)));
            }
            if (!suppressDefaults || !IsDefaultBidir)
            {
                rootElem.Add(new XAttribute("bidir", Bidir.ToString()));
            }
            if (!suppressDefaults || !IsDefaultVarSchema)
            {
                rootElem.Add(new XAttribute("variablesSchema", VarSchema.ToString()));
            }
            if (!suppressDefaults || !UnificationCfg.ContainsOnlyDefaults)
            {
                rootElem.Add(UnificationCfg.GetXml(suppressDefaults));
            }
            if (SteadyFieldsCfg != null)
            {
                rootElem.Add(SteadyFieldsCfg.GetXml(suppressDefaults));
            }
            Validate(rootElem, XsdTypeName);
            return rootElem;
        }

        /// <inheritdoc/>
        public override XElement GetXml(bool suppressDefaults)
        {
            return GetXml("feedingPatterned", suppressDefaults);
        }

    }//FeedingPatternedSettings

}//Namespace

