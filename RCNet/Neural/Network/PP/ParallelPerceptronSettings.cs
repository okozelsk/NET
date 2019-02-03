using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using System.Globalization;
using System.Xml.Linq;
using System.IO;
using RCNet.XmlTools;
using RCNet.MathTools;

namespace RCNet.Neural.Network.PP
{
    /// <summary>
    /// The class contains parallel perceptron configuration parameters
    /// The easiest and safest way to create an instance is to use the xml constructor.
    /// </summary>
    [Serializable]
    public class ParallelPerceptronSettings
    {
        //Attribute properties
        /// <summary>
        /// Number of treshold gates inside the parallel perceptron
        /// </summary>
        public int NumOfGates { get; set; }
        /// <summary>
        /// Requiered output resolution (2 means binary output)
        /// </summary>
        public int Resolution { get; set; }
        /// <summary>
        /// Network output values range.
        /// </summary>
        public Interval OutputRange { get; }
        /// <summary>
        /// Startup parameters for the parallel perceptron p-delta rule trainer
        /// </summary>
        public PDeltaRuleTrainerSettings PDeltaRuleTrainerCfg { get; set; }

        //Constructors
        /// <summary>
        /// Creates an uninitialized instance
        /// </summary>
        public ParallelPerceptronSettings()
        {
            NumOfGates = 0;
            Resolution = 0;
            OutputRange = new Interval(-1, 1);
            PDeltaRuleTrainerCfg = null;
            return;
        }

        /// <summary>
        /// The deep copy constructor
        /// </summary>
        /// <param name="source">Source instance</param>
        public ParallelPerceptronSettings(ParallelPerceptronSettings source)
        {
            NumOfGates = source.NumOfGates;
            Resolution = source.Resolution;
            OutputRange = source.OutputRange.DeepClone();
            PDeltaRuleTrainerCfg = null;
            if (source.PDeltaRuleTrainerCfg != null)
            {
                PDeltaRuleTrainerCfg = (PDeltaRuleTrainerSettings)source.PDeltaRuleTrainerCfg.DeepClone();
            }
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
            ElemValidator validator = new ElemValidator();
            Assembly assemblyRCNet = Assembly.GetExecutingAssembly();
            validator.AddXsdFromResources(assemblyRCNet, "RCNet.Neural.Network.PP.ParallelPerceptronSettings.xsd");
            validator.AddXsdFromResources(assemblyRCNet, "RCNet.RCNetTypes.xsd");
            XElement settingsElem = validator.Validate(elem, "rootElem");
            //Parsing
            NumOfGates = int.Parse(settingsElem.Attribute("gates").Value, CultureInfo.InvariantCulture);
            Resolution = int.Parse(settingsElem.Attribute("resolution").Value, CultureInfo.InvariantCulture);
            OutputRange = new Interval(-1, 1);
            XElement pDeltaRuleTrainerElem = settingsElem.Descendants("pDeltaRuleTrainer").First();
            PDeltaRuleTrainerCfg = new PDeltaRuleTrainerSettings(pDeltaRuleTrainerElem);
            return;
        }

        //Methods
        /// <summary>
        /// See the base.
        /// </summary>
        public override bool Equals(object obj)
        {
            if (obj == null) return false;
            ParallelPerceptronSettings cmpSettings = obj as ParallelPerceptronSettings;
            if (NumOfGates != cmpSettings.NumOfGates ||
                Resolution != cmpSettings.Resolution ||
                !Equals(OutputRange, cmpSettings.OutputRange) ||
                (PDeltaRuleTrainerCfg == null && cmpSettings.PDeltaRuleTrainerCfg != null) ||
                (PDeltaRuleTrainerCfg != null && cmpSettings.PDeltaRuleTrainerCfg == null) ||
                (PDeltaRuleTrainerCfg != null && !PDeltaRuleTrainerCfg.Equals(cmpSettings.PDeltaRuleTrainerCfg))
                )
            {
                return false;
            }
            return true;
        }

        /// <summary>
        /// See the base.
        /// </summary>
        public override int GetHashCode()
        {
            return base.GetHashCode();
        }

        /// <summary>
        /// Creates the deep copy instance of this instance
        /// </summary>
        public ParallelPerceptronSettings DeepClone()
        {
            return new ParallelPerceptronSettings(this);
        }

    }//ParallelPerceptronSettings

}//Namespace

