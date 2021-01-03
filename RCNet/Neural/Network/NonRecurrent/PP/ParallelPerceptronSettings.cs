using System;
using System.Globalization;
using System.Linq;
using System.Xml.Linq;

namespace RCNet.Neural.Network.NonRecurrent.PP
{
    /// <summary>
    /// Configuration of the ParallelPerceptron.
    /// </summary>
    [Serializable]
    public class ParallelPerceptronSettings : RCNetBaseSettings, INonRecurrentNetworkSettings
    {
        //Constants
        /// <summary>
        /// The name of the associated xsd type.
        /// </summary>
        public const string XsdTypeName = "PPNetType";
        //Default values
        /// <summary>
        /// The default number of the threshold gates.
        /// </summary>
        public const int DefaultGates = 3;
        /// <summary>
        /// The default output resolution (2 means the binary output).
        /// </summary>
        public const int DefaultResolution = 2;

        //Attribute properties
        /// <summary>
        /// The number of the threshold gates.
        /// </summary>
        public int Gates { get; }
        /// <summary>
        /// The output resolution.
        /// </summary>
        public int Resolution { get; }
        /// <summary>
        /// The configuration of the p-delta rule trainer.
        /// </summary>
        public PDeltaRuleTrainerSettings PDeltaRuleTrainerCfg { get; }

        //Constructors
        /// <summary>
        /// Creates an initialized instance.
        /// </summary>
        /// <param name="gates">The number of the threshold gates.</param>
        /// <param name="resolution">The output resolution.</param>
        /// <param name="trainerCfg">The configuration of the p-delta rule trainer.</param>
        public ParallelPerceptronSettings(int gates,
                                          int resolution,
                                          PDeltaRuleTrainerSettings trainerCfg
                                          )
        {
            Gates = gates;
            Resolution = resolution;
            PDeltaRuleTrainerCfg = (PDeltaRuleTrainerSettings)trainerCfg.DeepClone();
            Check();
            return;
        }

        /// <summary>
        /// The deep copy constructor.
        /// </summary>
        /// <param name="source">The source instance.</param>
        public ParallelPerceptronSettings(ParallelPerceptronSettings source)
            : this(source.Gates, source.Resolution, source.PDeltaRuleTrainerCfg)
        {
            return;
        }

        /// <summary>
        /// Creates an initialized instance.
        /// </summary>
        /// <param name="elem">A xml element containing the configuration data.</param>
        public ParallelPerceptronSettings(XElement elem)
        {
            //Validation
            XElement settingsElem = Validate(elem, XsdTypeName);
            //Parsing
            Gates = int.Parse(settingsElem.Attribute("gates").Value, CultureInfo.InvariantCulture);
            Resolution = int.Parse(settingsElem.Attribute("resolution").Value, CultureInfo.InvariantCulture);
            XElement pDeltaRuleTrainerElem = settingsElem.Elements("pDeltaRuleTrainer").First();
            PDeltaRuleTrainerCfg = new PDeltaRuleTrainerSettings(pDeltaRuleTrainerElem);
            Check();
            return;
        }

        //Properties
        /// <summary>
        /// Checks the defaults.
        /// </summary>
        public bool IsDefaultGates { get { return (Gates == DefaultGates); } }

        /// <summary>
        /// Checks the defaults.
        /// </summary>
        public bool IsDefaultResolution { get { return (Resolution == DefaultResolution); } }

        /// <inheritdoc/>
        public override bool ContainsOnlyDefaults { get { return false; } }

        //Methods
        /// <inheritdoc/>
        protected override void Check()
        {
            if (Gates < 1)
            {
                throw new ArgumentException($"Invalid Gates {Gates.ToString(CultureInfo.InvariantCulture)}. Gates must be GE to 1.", "Gates");
            }
            if (Resolution < 2 || Resolution > Gates * 2)
            {
                throw new ArgumentException($"Invalid Resolution {Resolution.ToString(CultureInfo.InvariantCulture)}. Resolution must be GE 2 and LE to (2 * Gates).", "Resolution");
            }
            return;
        }

        /// <inheritdoc/>
        public override RCNetBaseSettings DeepClone()
        {
            return new ParallelPerceptronSettings(this);
        }

        /// <inheritdoc/>
        public override XElement GetXml(string rootElemName, bool suppressDefaults)
        {
            XElement rootElem = new XElement(rootElemName);
            if (!suppressDefaults || !IsDefaultGates)
            {
                rootElem.Add(new XAttribute("gates", Gates.ToString(CultureInfo.InvariantCulture)));
            }
            if (!suppressDefaults || !IsDefaultResolution)
            {
                rootElem.Add(new XAttribute("resolution", Resolution.ToString(CultureInfo.InvariantCulture)));
            }
            rootElem.Add(PDeltaRuleTrainerCfg.GetXml(suppressDefaults));
            Validate(rootElem, XsdTypeName);
            return rootElem;
        }

        /// <inheritdoc/>
        public override XElement GetXml(bool suppressDefaults)
        {
            return GetXml("pp", suppressDefaults);
        }

    }//ParallelPerceptronSettings

}//Namespace

