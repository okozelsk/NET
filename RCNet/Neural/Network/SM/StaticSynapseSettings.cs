using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using System.Globalization;
using System.Xml.Linq;
using System.IO;
using RCNet.Extensions;
using RCNet.XmlTools;
using RCNet.RandomValue;

namespace RCNet.Neural.Network.SM
{
    /// <summary>
    /// Setup parameters of static synapse
    /// </summary>
    [Serializable]
    public class StaticSynapseSettings
    {
        //Attribute properties
        /// <summary>
        /// Maximum synapse delay
        /// </summary>
        public int MaxDelay { get; set; }
        /// <summary>
        /// Synapse's random weight settings
        /// </summary>
        public RandomValueSettings WeightCfg { get; set; }

        //Constructors
        /// <summary>
        /// Creates an uninitialized instance
        /// </summary>
        public StaticSynapseSettings()
        {
            MaxDelay = 0;
            WeightCfg = null;
            return;
        }

        /// <summary>
        /// The deep copy constructor
        /// </summary>
        /// <param name="source">Source instance</param>
        public StaticSynapseSettings(StaticSynapseSettings source)
        {
            MaxDelay = source.MaxDelay;
            WeightCfg = null;
            if (source.WeightCfg != null)
            {
                WeightCfg = source.WeightCfg.DeepClone();
            }
            return;
        }

        /// <summary>
        /// Creates the instance and initialize it from given xml element.
        /// </summary>
        /// <param name="elem">
        /// Xml data containing settings.
        /// Content of xml element is always validated against the xml schema.
        /// </param>
        public StaticSynapseSettings(XElement elem)
        {
            //Validation
            ElemValidator validator = new ElemValidator();
            Assembly assemblyRCNet = Assembly.GetExecutingAssembly();
            validator.AddXsdFromResources(assemblyRCNet, "RCNet.Neural.Network.SM.StaticSynapseSettings.xsd");
            validator.AddXsdFromResources(assemblyRCNet, "RCNet.RCNetTypes.xsd");
            XElement settingsElem = validator.Validate(elem, "rootElem");
            //Parsing
            //Max delay
            MaxDelay = int.Parse(settingsElem.Attribute("maxDelay").Value, CultureInfo.InvariantCulture);
            //Weight
            WeightCfg = new RandomValueSettings(settingsElem.Descendants("weight").First());
            return;
        }

        //Methods
        /// <summary>
        /// See the base.
        /// </summary>
        public override bool Equals(object obj)
        {
            if (obj == null) return false;
            StaticSynapseSettings cmpSettings = obj as StaticSynapseSettings;
            if (MaxDelay != cmpSettings.MaxDelay || !Equals(WeightCfg, cmpSettings.WeightCfg))
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
        public StaticSynapseSettings DeepClone()
        {
            StaticSynapseSettings clone = new StaticSynapseSettings(this);
            return clone;
        }

    }//StaticSynapseSettings

}//Namespace

