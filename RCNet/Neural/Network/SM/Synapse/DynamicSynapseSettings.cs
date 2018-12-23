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

namespace RCNet.Neural.Network.SM.Synapse
{
    /// <summary>
    /// Setup parameters of dynamic synapse
    /// </summary>
    [Serializable]
    public class DynamicSynapseSettings
    {
        //Attribute properties
        /// <summary>
        /// Synapse's resting efficacy
        /// </summary>
        public double RestingEfficacy { get; set; }
        /// <summary>
        /// Synapse's efficacy facilitation
        /// </summary>
        public double TauFacilitation { get; set; }
        /// <summary>
        /// Synapse's efficacy recovery
        /// </summary>
        public double TauRecovery { get; set; }
        /// <summary>
        /// Synapse's efficacy decay
        /// </summary>
        public double TauDecay { get; set; }
        /// <summary>
        /// Synapse's random weight settings
        /// </summary>
        public RandomValueSettings WeightCfg { get; set; }

        //Constructors
        /// <summary>
        /// Creates an uninitialized instance
        /// </summary>
        public DynamicSynapseSettings()
        {
            RestingEfficacy = 0;
            TauFacilitation = 0;
            TauRecovery = 0;
            TauDecay = 0;
            WeightCfg = null;
            return;
        }

        /// <summary>
        /// The deep copy constructor
        /// </summary>
        /// <param name="source">Source instance</param>
        public DynamicSynapseSettings(DynamicSynapseSettings source)
        {
            RestingEfficacy = source.RestingEfficacy;
            TauFacilitation = source.TauFacilitation;
            TauRecovery = source.TauRecovery;
            TauDecay = source.TauDecay;
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
        public DynamicSynapseSettings(XElement elem)
        {
            //Validation
            ElemValidator validator = new ElemValidator();
            Assembly assemblyRCNet = Assembly.GetExecutingAssembly();
            validator.AddXsdFromResources(assemblyRCNet, "RCNet.Neural.Network.SM.Synapse.DynamicSynapseSettings.xsd");
            validator.AddXsdFromResources(assemblyRCNet, "RCNet.RCNetTypes.xsd");
            XElement settingsElem = validator.Validate(elem, "rootElem");
            //Parsing
            //Resting efficacy
            RestingEfficacy = double.Parse(settingsElem.Attribute("restingEfficacy").Value, CultureInfo.InvariantCulture);
            //Efficacy facilitation
            TauFacilitation = double.Parse(settingsElem.Attribute("tauFacilitation").Value, CultureInfo.InvariantCulture);
            //Efficacy recovery
            TauRecovery = double.Parse(settingsElem.Attribute("tauRecovery").Value, CultureInfo.InvariantCulture);
            //Efficacy decay
            TauDecay = double.Parse(settingsElem.Attribute("tauDecay").Value, CultureInfo.InvariantCulture);
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
            DynamicSynapseSettings cmpSettings = obj as DynamicSynapseSettings;
            if (RestingEfficacy != cmpSettings.RestingEfficacy ||
                TauFacilitation != cmpSettings.TauFacilitation ||
                TauRecovery != cmpSettings.TauRecovery ||
                TauDecay != cmpSettings.TauDecay ||
                !Equals(WeightCfg, cmpSettings.WeightCfg)
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
        public DynamicSynapseSettings DeepClone()
        {
            DynamicSynapseSettings clone = new DynamicSynapseSettings(this);
            return clone;
        }

    }//DynamicSynapseSettings

}//Namespace

