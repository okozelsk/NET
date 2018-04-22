using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using System.Globalization;
using System.Xml.Linq;
using System.IO;
using RCNet.XmlTools;
using RCNet.Neural.Activation;

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
            PDeltaRuleTrainerCfg = new PDeltaRuleTrainerSettings();
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
            PDeltaRuleTrainerCfg = null;
            if (source.PDeltaRuleTrainerCfg != null)
            {
                PDeltaRuleTrainerCfg = source.PDeltaRuleTrainerCfg.DeepClone();
            }
            return;
        }

        /// <summary>
        /// Creates the instance and initialize it from given xml element.
        /// This is the preferred way to instantiate reservoir settings.
        /// </summary>
        /// <param name="elem">
        /// Xml data containing parallel perceptron settings.
        /// Content of xml element is always validated against the xml schema.
        /// </param>
        public ParallelPerceptronSettings(XElement elem)
        {
            //Validation
            ElemValidator validator = new ElemValidator();
            Assembly assemblyRCNet = Assembly.GetExecutingAssembly();
            validator.AddXsdFromResources(assemblyRCNet, "RCNet.Neural.Network.PP.ParallelPerceptronSettings.xsd");
            validator.AddXsdFromResources(assemblyRCNet, "RCNet.RCNetTypes.xsd");
            XElement parallelPerceptronSettingsElem = validator.Validate(elem, "rootElem");
            //Parsing
            NumOfGates = int.Parse(parallelPerceptronSettingsElem.Attribute("gates").Value, CultureInfo.InvariantCulture);
            Resolution = int.Parse(parallelPerceptronSettingsElem.Attribute("resolution").Value, CultureInfo.InvariantCulture);
            XElement pDeltaRuleTrainerElem = parallelPerceptronSettingsElem.Descendants("pDeltaRuleTrainer").FirstOrDefault();
            if(pDeltaRuleTrainerElem != null)
            {
                PDeltaRuleTrainerCfg = new PDeltaRuleTrainerSettings(pDeltaRuleTrainerElem);
            }
            else
            {
                PDeltaRuleTrainerCfg = new PDeltaRuleTrainerSettings();
            }
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
            ParallelPerceptronSettings clone = new ParallelPerceptronSettings(this);
            return clone;
        }

    }//ParallelPerceptronSettings

}//Namespace

