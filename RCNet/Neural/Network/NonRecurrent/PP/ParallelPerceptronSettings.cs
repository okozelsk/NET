using System;
using System.Globalization;
using System.Linq;
using System.Xml.Linq;

namespace RCNet.Neural.Network.NonRecurrent.PP
{
    /// <summary>
    /// Configuration of the ParallelPerceptron
    /// </summary>
    [Serializable]
    public class ParallelPerceptronSettings : RCNetBaseSettings, INonRecurrentNetworkSettings
    {
        //Constants
        /// <summary>
        /// Name of the associated xsd type
        /// </summary>
        public const string XsdTypeName = "PPNetType";

        //Default values
        /// <summary>
        /// Default number of treshold gates inside the parallel perceptron
        /// </summary>
        public const int DefaultGates = 3;
        /// <summary>
        /// Default output resolution (2 means binary output)
        /// </summary>
        public const int DefaultResolution = 2;

        //Attribute properties
        /// <summary>
        /// Number of treshold gates inside the parallel perceptron
        /// </summary>
        public int Gates { get; }
        /// <summary>
        /// Requiered output resolution (2 means binary output)
        /// </summary>
        public int Resolution { get; }
        /// <summary>
        /// Startup parameters for the parallel perceptron p-delta rule trainer
        /// </summary>
        public PDeltaRuleTrainerSettings PDeltaRuleTrainerCfg { get; }

        //Constructors
        /// <summary>
        /// Creates an initialized instance
        /// </summary>
        /// <param name="gates">Number of gates</param>
        /// <param name="resolution">Output resolution</param>
        /// <param name="trainerCfg">Trainer configuration</param>
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
        /// The deep copy constructor
        /// </summary>
        /// <param name="source">Source instance</param>
        public ParallelPerceptronSettings(ParallelPerceptronSettings source)
            :this(source.Gates, source.Resolution, source.PDeltaRuleTrainerCfg)
        {
            return;
        }

        /// <summary>
        /// Creates an initialized instance.
        /// </summary>
        /// <param name="elem">Xml element containing the initialization settings</param>
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
        /// Checks the defaults
        /// </summary>
        public bool IsDefaultGates { get { return (Gates == DefaultGates); } }

        /// <summary>
        /// Checks the defaults
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

