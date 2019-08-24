using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Globalization;
using System.Xml.Linq;
using System.Reflection;
using RCNet.Extensions;
using RCNet.MathTools;
using RCNet.XmlTools;
using RCNet.MathTools.Differential;
using RCNet.RandomValue;

namespace RCNet.Neural.Activation
{
    /// <summary>
    /// Class encaptulates arguments of the SimpleIF activation function.
    /// Arguments are in RandomValue form to allow their dynamic random initialization within the specified ranges.
    /// </summary>
    [Serializable]
    public class SimpleIFSettings
    {
        //Constants
        //Typical values
        /// <summary>
        /// Typical value of resistance
        /// </summary>
        public const double TypicalResistance = 15;
        /// <summary>
        /// Typical value of decay rate
        /// </summary>
        public const double TypicalDecayRate = 0.05;
        /// <summary>
        /// Typical value of reset voltage
        /// </summary>
        public const double TypicalResetV = 5;
        /// <summary>
        /// Typical value of firing voltage
        /// </summary>
        public const double TypicalFiringThresholdV = 20;

        //Attribute properties
        /// <summary>
        /// Membrane resistance (Mohm)
        /// </summary>
        public RandomValueSettings Resistance { get; }
        /// <summary>
        /// Membrane potential decay rate
        /// </summary>
        public RandomValueSettings DecayRate { get; }
        /// <summary>
        /// Membrane reset potential (mV)
        /// </summary>
        public RandomValueSettings ResetV { get; }
        /// <summary>
        /// Membrane firing threshold (mV)
        /// </summary>
        public RandomValueSettings FiringThresholdV { get; }
        /// <summary>
        /// Number of after spike computation cycles while an input stimuli is ignored (ms)
        /// </summary>
        public int RefractoryPeriods { get; }

        //Constructors
        /// <summary>
        /// Creates an initialized instance
        /// </summary>
        /// <param name="resistance">Membrane resistance (Mohm)</param>
        /// <param name="decayRate">Membrane potential decay rate</param>
        /// <param name="resetV">Membrane reset potential (mV)</param>
        /// <param name="firingThresholdV">Membrane firing threshold (mV)</param>
        /// <param name="refractoryPeriods">Number of after spike computation cycles while an input stimuli is ignored (ms)</param>
        public SimpleIFSettings(RandomValueSettings resistance = null,
                                RandomValueSettings decayRate = null,
                                RandomValueSettings resetV = null,
                                RandomValueSettings firingThresholdV = null,
                                int refractoryPeriods = 1
                                )
        {
            Resistance = RandomValueSettings.CloneOrDefault(resistance, TypicalResistance);
            DecayRate = RandomValueSettings.CloneOrDefault(decayRate, TypicalDecayRate);
            ResetV = RandomValueSettings.CloneOrDefault(resetV, TypicalResetV);
            FiringThresholdV = RandomValueSettings.CloneOrDefault(firingThresholdV, TypicalFiringThresholdV);
            RefractoryPeriods = refractoryPeriods;
            return;
        }

        /// <summary>
        /// Copy constructor
        /// </summary>
        /// <param name="source">Source instance</param>
        public SimpleIFSettings(SimpleIFSettings source)
        {
            Resistance = source.Resistance.DeepClone();
            DecayRate = source.DecayRate.DeepClone();
            ResetV = source.ResetV.DeepClone();
            FiringThresholdV = source.FiringThresholdV.DeepClone();
            RefractoryPeriods = source.RefractoryPeriods;
            return;
        }

        /// <summary>
        /// Creates an instance and initializes it from given xml element.
        /// </summary>
        /// <param name="elem">
        /// Xml data containing SimpleIF activation settings.
        /// Content of xml element is always validated against the xml schema.
        /// </param>
        public SimpleIFSettings(XElement elem)
        {
            //Validation
            ElemValidator validator = new ElemValidator();
            Assembly assemblyRCNet = Assembly.GetExecutingAssembly();
            validator.AddXsdFromResources(assemblyRCNet, "RCNet.Neural.Activation.SimpleIFSettings.xsd");
            validator.AddXsdFromResources(assemblyRCNet, "RCNet.RCNetTypes.xsd");
            XElement activationSettingsElem = validator.Validate(elem, "rootElem");
            //Parsing
            Resistance = RandomValueSettings.LoadOrDefault(activationSettingsElem, "resistance", TypicalResistance);
            DecayRate = RandomValueSettings.LoadOrDefault(activationSettingsElem, "decayRate", TypicalDecayRate);
            ResetV = RandomValueSettings.LoadOrDefault(activationSettingsElem, "resetV", TypicalResetV);
            FiringThresholdV = RandomValueSettings.LoadOrDefault(activationSettingsElem, "firingThresholdV", TypicalFiringThresholdV);
            RefractoryPeriods = int.Parse(activationSettingsElem.Attribute("refractoryPeriods").Value, CultureInfo.InvariantCulture);
            return;
        }

        //Methods
        /// <summary>
        /// See the base.
        /// </summary>
        public override bool Equals(object obj)
        {
            if (obj == null) return false;
            SimpleIFSettings cmpSettings = obj as SimpleIFSettings;
            if (!Equals(Resistance, cmpSettings.Resistance) ||
                !Equals(DecayRate, cmpSettings.DecayRate) ||
                !Equals(ResetV, cmpSettings.ResetV) ||
                !Equals(FiringThresholdV, cmpSettings.FiringThresholdV) ||
                RefractoryPeriods != cmpSettings.RefractoryPeriods
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
        public SimpleIFSettings DeepClone()
        {
            SimpleIFSettings clone = new SimpleIFSettings(this);
            return clone;
        }

    }//SimpleIFSettings

}//Namespace
