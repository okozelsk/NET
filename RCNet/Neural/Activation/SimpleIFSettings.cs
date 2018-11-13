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
    /// Class encaptulates arguments of the SimpleIF activation function
    /// </summary>
    [Serializable]
    public class SimpleIFSettings
    {
        //Constants
        //Typical values
        public const double TypicalStimuliCoeff = 1;
        public const double TypicalResistance = 15;
        public const double TypicalDecayRate = 0.05;
        public const double TypicalResetV = 5;
        public const double TypicalFiringThresholdV = 20;

        //Attribute properties
        /// <summary>
        /// Initial input stimuli coefficient (pA)
        /// </summary>
        public double StimuliCoeff { get; }
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
        /// <param name="stimuliCoeff">Initial input stimuli coefficient (pA)</param>
        /// <param name="resistance">Membrane resistance (Mohm)</param>
        /// <param name="decayRate">Membrane potential decay rate</param>
        /// <param name="resetV">Membrane reset potential (mV)</param>
        /// <param name="firingThresholdV">Membrane firing threshold (mV)</param>
        /// <param name="refractoryPeriods">Number of after spike computation cycles while an input stimuli is ignored (ms)</param>
        public SimpleIFSettings(double stimuliCoeff,
                                RandomValueSettings resistance,
                                RandomValueSettings decayRate,
                                RandomValueSettings resetV,
                                RandomValueSettings firingThresholdV,
                                int refractoryPeriods
                                )
        {
            StimuliCoeff = stimuliCoeff;
            Resistance = resistance.DeepClone();
            DecayRate = decayRate.DeepClone();
            ResetV = resetV.DeepClone();
            FiringThresholdV = firingThresholdV.DeepClone();
            RefractoryPeriods = refractoryPeriods;
            return;
        }

        /// <summary>
        /// Copy constructor
        /// </summary>
        /// <param name="source">Source instance</param>
        public SimpleIFSettings(SimpleIFSettings source)
        {
            StimuliCoeff = source.StimuliCoeff;
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
            StimuliCoeff = double.Parse(activationSettingsElem.Attribute("stimuliCoeff").Value, CultureInfo.InvariantCulture);
            Resistance = new RandomValueSettings(activationSettingsElem.Descendants("resistance").FirstOrDefault());
            DecayRate = new RandomValueSettings(activationSettingsElem.Descendants("decayRate").FirstOrDefault());
            ResetV = new RandomValueSettings(activationSettingsElem.Descendants("resetV").FirstOrDefault());
            FiringThresholdV = new RandomValueSettings(activationSettingsElem.Descendants("firingThresholdV").FirstOrDefault());
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
            if (StimuliCoeff != cmpSettings.StimuliCoeff ||
                !Equals(Resistance, cmpSettings.Resistance) ||
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
