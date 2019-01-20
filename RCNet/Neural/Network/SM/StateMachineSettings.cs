using System;
using System.Collections.Generic;
using System.Linq;
using System.Globalization;
using System.Xml.Linq;
using System.Reflection;
using RCNet.Extensions;
using RCNet.XmlTools;
using RCNet.RandomValue;
using RCNet.Neural.Data.Generators;
using RCNet.Neural.Network.SM.Synapse;
using RCNet.Neural.Network.SM.Preprocessing;
using RCNet.Neural.Network.SM.Readout;

namespace RCNet.Neural.Network.SM
{
    /// <summary>
    /// The class contains State Machine configuration.
    /// The easiest and safest way to create an instance is to use the xml constructor.
    /// </summary>
    [Serializable]
    public class StateMachineSettings
    {
        //Attribute properties
        /// <summary>
        /// A value greater than or equal to 0 will always ensure the same initialization of the internal
        /// random number generator and therefore the same network structure, which is good for tuning
        /// other parameters.
        /// A value less than 0 causes a fully random initialization when creating a network instance.
        /// </summary>
        public int RandomizerSeek { get; set; }
        /// <summary>
        /// Settings of Neural Preprocessor
        /// </summary>
        public NeuralPreprocessorSettings NeuralPreprocessorConfig { get; set; }
        /// <summary>
        /// Configuration of the readout layer
        /// </summary>
        public ReadoutLayerSettings ReadoutLayerConfig { get; set; }

        //Constructors
        /// <summary>
        /// Creates an uninitialized instance.
        /// </summary>
        public StateMachineSettings()
        {
            //Default settings
            RandomizerSeek = 0;
            NeuralPreprocessorConfig = new NeuralPreprocessorSettings();
            ReadoutLayerConfig = new ReadoutLayerSettings();
            return;
        }

        /// <summary>
        /// The deep copy constructor
        /// </summary>
        /// <param name="source">Source instance</param>
        public StateMachineSettings(StateMachineSettings source)
        {
            //Copy
            RandomizerSeek = source.RandomizerSeek;
            NeuralPreprocessorConfig = source.NeuralPreprocessorConfig.DeepClone();
            ReadoutLayerConfig = new ReadoutLayerSettings(source.ReadoutLayerConfig);
            return;
        }

        /// <summary>
        /// Creates the instance and initializes it from given xml element.
        /// This is the preferred way to instantiate State Machine settings.
        /// </summary>
        /// <param name="elem">
        /// Xml data containing State Machine settings.
        /// Content of xml element is always validated against the xml schema.
        /// </param>
        public StateMachineSettings(XElement elem)
        {
            //Validation
            ElemValidator validator = new ElemValidator();
            Assembly assemblyRCNet = Assembly.GetExecutingAssembly();
            validator.AddXsdFromResources(assemblyRCNet, "RCNet.Neural.Network.SM.StateMachineSettings.xsd");
            validator.AddXsdFromResources(assemblyRCNet, "RCNet.RCNetTypes.xsd");
            XElement stateMachineSettingsElem = validator.Validate(elem, "rootElem");
            //Parsing
            //Randomizer seek
            RandomizerSeek = int.Parse(stateMachineSettingsElem.Attribute("randomizerSeek").Value);
            //Neural preprocessor
            NeuralPreprocessorConfig = new NeuralPreprocessorSettings(stateMachineSettingsElem.Descendants("neuralPreprocessor").First());
            //Readout layer
            ReadoutLayerConfig = new ReadoutLayerSettings(stateMachineSettingsElem.Descendants("readoutLayer").First());
            return;
        }

        //Methods
        /// <summary>
        /// See the base.
        /// </summary>
        public override bool Equals(object obj)
        {
            if (obj == null) return false;
            StateMachineSettings cmpSettings = obj as StateMachineSettings;
            if (RandomizerSeek != cmpSettings.RandomizerSeek ||
                !Equals(NeuralPreprocessorConfig, cmpSettings.NeuralPreprocessorConfig) ||
                !Equals(ReadoutLayerConfig, cmpSettings.ReadoutLayerConfig)
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
        public StateMachineSettings DeepClone()
        {
            StateMachineSettings clone = new StateMachineSettings(this);
            return clone;
        }

    }//StateMachineSettings

}//Namespace
