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
using RCNet.Neural.Network.SM.Preprocessing;
using System.Xml.XPath;

namespace RCNet.Neural.Network.SM.Synapse
{
    /// <summary>
    /// Configuration parameters of an input synapse
    /// </summary>
    [Serializable]
    public class InputSynapseSettings
    {
        //Attribute properties
        /// <summary>
        /// Synapse's scope when targeting spiking neurons
        /// </summary>
        public BaseSynapse.SynapticTargetScope SpikingTargetScope { get; set; }
        /// <summary>
        /// Synapse's random weight settings for Input->Spiking connection
        /// </summary>
        public RandomValueSettings SpikingTargetWeightCfg { get; set; }

        /// <summary>
        /// Synapse's scope when targeting analog neurons
        /// </summary>
        public BaseSynapse.SynapticTargetScope AnalogTargetScope { get; set; }
        /// <summary>
        /// Synapse's random weight settings for Input->Analog connection
        /// </summary>
        public RandomValueSettings AnalogTargetWeightCfg { get; set; }

        /// <summary>
        /// Relative shares of analog signal tranformations
        /// </summary>
        public RelShareRealtime<int> AnalogTransRelShares { get; set; }

        /// <summary>
        /// Specifies how will be decided synaptic delay
        /// </summary>
        public BaseSynapse.SynapticDelayMethod DelayMethod { get; set; }
        /// <summary>
        /// Maximum delay of the input signal
        /// </summary>
        public int MaxDelay { get; set; }


        //Constructors
        /// <summary>
        /// The deep copy constructor
        /// </summary>
        /// <param name="source">Source instance</param>
        public InputSynapseSettings(InputSynapseSettings source)
        {
            SpikingTargetScope = source.SpikingTargetScope;
            SpikingTargetWeightCfg = null;
            if (source.SpikingTargetWeightCfg != null)
            {
                SpikingTargetWeightCfg = source.SpikingTargetWeightCfg.DeepClone();
            }
            AnalogTargetScope = source.AnalogTargetScope;
            AnalogTransRelShares = source.AnalogTransRelShares.DeepClone();
            AnalogTargetWeightCfg = null;
            if (source.AnalogTargetWeightCfg != null)
            {
                AnalogTargetWeightCfg = source.AnalogTargetWeightCfg.DeepClone();
            }
            DelayMethod = source.DelayMethod;
            MaxDelay = source.MaxDelay;
            return;
        }

        /// <summary>
        /// Creates the instance and initialize it from given xml element.
        /// </summary>
        /// <param name="elem">
        /// Xml data containing settings.
        /// Content of xml element is always validated against the xml schema.
        /// </param>
        public InputSynapseSettings(XElement elem)
        {
            //Validation
            ElemValidator validator = new ElemValidator();
            Assembly assemblyRCNet = Assembly.GetExecutingAssembly();
            validator.AddXsdFromResources(assemblyRCNet, "RCNet.Neural.Network.SM.Synapse.InputSynapseSettings.xsd");
            validator.AddXsdFromResources(assemblyRCNet, "RCNet.RCNetTypes.xsd");
            XElement settingsElem = validator.Validate(elem, "rootElem");
            //Parsing
            XElement cfgElem;
            //Spiking target
            //Scope
            cfgElem = settingsElem.XPathSelectElement("./spikingTarget");
            if(cfgElem != null)
            {
                SpikingTargetScope = BaseSynapse.ParseSynapticTargetScope(cfgElem.Attribute("scope").Value);
            }
            else
            {
                SpikingTargetScope = BaseSynapse.SynapticTargetScope.Excitatory;
            }
            //Spiking target Weight
            cfgElem = settingsElem.XPathSelectElement("./spikingTarget/weight");
            if(cfgElem != null)
            {
                SpikingTargetWeightCfg = new RandomValueSettings(cfgElem);
            }
            else
            {
                SpikingTargetWeightCfg = new RandomValueSettings(0, 1);
            }
            //Analog target
            //Scope and analog rel shares of transformations
            AnalogTransRelShares = new RelShareRealtime<int>();
            cfgElem = settingsElem.XPathSelectElement("./analogTarget");
            if (cfgElem != null)
            {
                AnalogTargetScope = BaseSynapse.ParseSynapticTargetScope(cfgElem.Attribute("scope").Value);
                AnalogTransRelShares.Add(double.Parse(cfgElem.Attribute("relShareOriginal").Value, CultureInfo.InvariantCulture), (int)InputUnit.AnalogCodingMethod.Original);
                AnalogTransRelShares.Add(double.Parse(cfgElem.Attribute("relShareTransDiff").Value, CultureInfo.InvariantCulture), (int)InputUnit.AnalogCodingMethod.Difference);
                AnalogTransRelShares.Add(double.Parse(cfgElem.Attribute("relShareTransLinearSteps").Value, CultureInfo.InvariantCulture), (int)InputUnit.AnalogCodingMethod.LinearSteps);
                AnalogTransRelShares.Add(double.Parse(cfgElem.Attribute("relShareTransPower").Value, CultureInfo.InvariantCulture), (int)InputUnit.AnalogCodingMethod.Power);
                AnalogTransRelShares.Add(double.Parse(cfgElem.Attribute("relShareTransFoldedPower").Value, CultureInfo.InvariantCulture), (int)InputUnit.AnalogCodingMethod.FoldedPower);
                AnalogTransRelShares.Add(double.Parse(cfgElem.Attribute("relShareTransMovingAverage").Value, CultureInfo.InvariantCulture), (int)InputUnit.AnalogCodingMethod.MovingAverage);
            }
            else
            {
                AnalogTargetScope = BaseSynapse.SynapticTargetScope.All;
                AnalogTransRelShares.Add(1d, (int)InputUnit.AnalogCodingMethod.Original);
                AnalogTransRelShares.Add(0d, (int)InputUnit.AnalogCodingMethod.Difference);
                AnalogTransRelShares.Add(0d, (int)InputUnit.AnalogCodingMethod.LinearSteps);
                AnalogTransRelShares.Add(0d, (int)InputUnit.AnalogCodingMethod.Power);
                AnalogTransRelShares.Add(0d, (int)InputUnit.AnalogCodingMethod.FoldedPower);
                AnalogTransRelShares.Add(0d, (int)InputUnit.AnalogCodingMethod.MovingAverage);
            }
            //Analog target Weight
            cfgElem = settingsElem.XPathSelectElement("./analogTarget/weight");
            if (cfgElem != null)
            {
                AnalogTargetWeightCfg = new RandomValueSettings(cfgElem);
            }
            else
            {
                AnalogTargetWeightCfg = new RandomValueSettings(0, 1);
            }
            //Delay
            DelayMethod = BaseSynapse.ParseSynapticDelayMethod(settingsElem.Attribute("delayMethod").Value);
            MaxDelay = int.Parse(settingsElem.Attribute("maxDelay").Value, CultureInfo.InvariantCulture);
            return;
        }

        //Methods
        /// <summary>
        /// See the base.
        /// </summary>
        public override bool Equals(object obj)
        {
            if (obj == null) return false;
            InputSynapseSettings cmpSettings = obj as InputSynapseSettings;
            if (SpikingTargetScope != cmpSettings.SpikingTargetScope ||
                !Equals(SpikingTargetWeightCfg, cmpSettings.SpikingTargetWeightCfg) ||
                AnalogTargetScope != cmpSettings.AnalogTargetScope ||
                !Equals(AnalogTransRelShares, cmpSettings.AnalogTransRelShares) ||
                !Equals(AnalogTargetWeightCfg, cmpSettings.AnalogTargetWeightCfg) ||
                DelayMethod != cmpSettings.DelayMethod ||
                MaxDelay != cmpSettings.MaxDelay
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
        public InputSynapseSettings DeepClone()
        {
            InputSynapseSettings clone = new InputSynapseSettings(this);
            return clone;
        }

    }//InputSynapseSettings

}//Namespace

