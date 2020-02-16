using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using System.Globalization;
using System.Xml.Linq;
using System.IO;
using RCNet.XmlTools;
using RCNet.MathTools;

namespace RCNet.Neural.Network.NonRecurrent.PP
{
    /// <summary>
    /// The class contains parallel perceptron configuration parameters
    /// The easiest and safest way to create an instance is to use the xml constructor.
    /// </summary>
    [Serializable]
    public class ParallelPerceptronSettings : RCNetBaseSettings, INonRecurrentNetworkSettings
    {
        //Constants
        /// <summary>
        /// Name of the associated xsd type
        /// </summary>
        public const string XsdTypeName = "PPNetCfgType";

        //Default values
        /// <summary>
        /// Default number of treshold gates inside the parallel perceptron
        /// </summary>
        public const int DefaultGates = 3;
        /// <summary>
        /// Default output resolution (2 means binary output)
        /// </summary>
        public const int DefaultResolution = 2;

        //Static attributes
        private static readonly Interval GatesRange = new Interval(-1, 1);

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
        {
            Gates = source.Gates;
            Resolution = source.Resolution;
            PDeltaRuleTrainerCfg = (PDeltaRuleTrainerSettings)source.PDeltaRuleTrainerCfg.DeepClone();
            return;
        }

        /// <summary>
        /// Creates the instance and initialize it from given xml element.
        /// This is the preferred way to instantiate settings.
        /// </summary>
        /// <param name="elem">
        /// Xml data containing settings.
        /// Content of xml element is always validated against the xml schema.
        /// </param>
        public ParallelPerceptronSettings(XElement elem)
        {
            //Validation
            XElement settingsElem = Validate(elem, XsdTypeName);
            //Parsing
            Gates = int.Parse(settingsElem.Attribute("gates").Value, CultureInfo.InvariantCulture);
            Resolution = int.Parse(settingsElem.Attribute("resolution").Value, CultureInfo.InvariantCulture);
            XElement pDeltaRuleTrainerElem = settingsElem.Descendants("pDeltaRuleTrainer").First();
            PDeltaRuleTrainerCfg = new PDeltaRuleTrainerSettings(pDeltaRuleTrainerElem);
            Check();
            return;
        }

        //Properties
        /// <summary>
        /// Network output range.
        /// </summary>
        public Interval OutputRange { get { return GatesRange; } }

        /// <summary>
        /// Checks if settings are default
        /// </summary>
        public bool IsDefaultGates { get { return (Gates == DefaultGates); } }

        /// <summary>
        /// Checks if settings are default
        /// </summary>
        public bool IsDefaultResolution { get { return (Resolution == DefaultResolution); } }

        /// <summary>
        /// Identifies settings containing only default values
        /// </summary>
        public override bool ContainsOnlyDefaults { get { return false; } }

        //Methods
        /// <summary>
        /// Checks validity
        /// </summary>
        private void Check()
        {
            if (Gates < 1)
            {
                throw new Exception($"Invalid Gates {Gates.ToString(CultureInfo.InvariantCulture)}. Gates must be GE to 1.");
            }
            if (Resolution < 2 || Resolution > Gates * 2)
            {
                throw new Exception($"Invalid Resolution {Resolution.ToString(CultureInfo.InvariantCulture)}. Resolution must be GE 2 and LE to (2 * Gates).");
            }
            return;
        }

        /// <summary>
        /// Creates the deep copy instance of this instance
        /// </summary>
        public override RCNetBaseSettings DeepClone()
        {
            return new ParallelPerceptronSettings(this);
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

        /// <summary>
        /// Generates default named xml element containing the settings.
        /// </summary>
        /// <param name="suppressDefaults">Specifies if to ommit optional nodes having set default values</param>
        /// <returns>XElement containing the settings</returns>
        public override XElement GetXml(bool suppressDefaults)
        {
            return GetXml("pp", suppressDefaults);
        }

    }//ParallelPerceptronSettings

}//Namespace

