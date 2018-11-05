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

namespace RCNet.Neural.Activation
{
    /// <summary>
    /// Class encaptulates arguments of the AdSimpleIF activation function
    /// </summary>
    [Serializable]
    public class AdSimpleIFSettings
    {
        //Attribute properties
        /// <summary>
        /// Initial input stimuli coefficient (pA)
        /// </summary>
        public double StimuliCoeff { get; set; }
        /// <summary>
        /// Membrane resistance (Mohm)
        /// </summary>
        public double Resistance { get; set; }
        /// <summary>
        /// Membrane potential decay rate
        /// </summary>
        public double DecayRate { get; set; }
        /// <summary>
        /// Membrane reset potential (mV)
        /// </summary>
        public double ResetV { get; set; }
        /// <summary>
        /// Membrane firing threshold (mV)
        /// </summary>
        public double FiringThresholdV { get; set; }

        //Constructors
        /// <summary>
        /// Creates an initialized instance
        /// </summary>
        /// <param name="stimuliCoeff">Initial input stimuli coefficient (pA)</param>
        /// <param name="resistance">Membrane resistance (Mohm)</param>
        /// <param name="decayRate">Membrane potential decay rate</param>
        /// <param name="resetV">Membrane reset potential (mV)</param>
        /// <param name="firingThresholdV">Membrane firing threshold (mV)</param>
        public AdSimpleIFSettings(double stimuliCoeff,
                                  double resistance,
                                  double decayRate,
                                  double resetV,
                                  double firingThresholdV
                                  )
        {
            StimuliCoeff = stimuliCoeff;
            Resistance = resistance;
            DecayRate = decayRate;
            ResetV = resetV;
            FiringThresholdV = firingThresholdV;
            return;
        }

        /// <summary>
        /// Copy constructor
        /// </summary>
        /// <param name="source">Source instance</param>
        public AdSimpleIFSettings(AdSimpleIFSettings source)
        {
            StimuliCoeff = source.StimuliCoeff;
            Resistance = source.Resistance;
            DecayRate = source.DecayRate;
            ResetV = source.ResetV;
            FiringThresholdV = source.FiringThresholdV;
            return;
        }

        /// <summary>
        /// Creates an instance and initializes it from given xml element.
        /// </summary>
        /// <param name="elem">
        /// Xml data containing AdSimpleIF activation settings.
        /// Content of xml element is always validated against the xml schema.
        /// </param>
        public AdSimpleIFSettings(XElement elem)
        {
            //Validation
            ElemValidator validator = new ElemValidator();
            Assembly assemblyRCNet = Assembly.GetExecutingAssembly();
            validator.AddXsdFromResources(assemblyRCNet, "RCNet.Neural.Activation.AdSimpleIFSettings.xsd");
            validator.AddXsdFromResources(assemblyRCNet, "RCNet.RCNetTypes.xsd");
            XElement activationSettingsElem = validator.Validate(elem, "rootElem");
            //Parsing
            StimuliCoeff = double.Parse(activationSettingsElem.Attribute("stimuliCoeff").Value, CultureInfo.InvariantCulture);
            Resistance = double.Parse(activationSettingsElem.Attribute("resistance").Value, CultureInfo.InvariantCulture);
            DecayRate = double.Parse(activationSettingsElem.Attribute("decayRate").Value, CultureInfo.InvariantCulture);
            ResetV = double.Parse(activationSettingsElem.Attribute("resetV").Value, CultureInfo.InvariantCulture);
            FiringThresholdV = double.Parse(activationSettingsElem.Attribute("firingThresholdV").Value, CultureInfo.InvariantCulture);
            return;
        }

        //Methods
        /// <summary>
        /// See the base.
        /// </summary>
        public override bool Equals(object obj)
        {
            if (obj == null) return false;
            AdSimpleIFSettings cmpSettings = obj as AdSimpleIFSettings;
            if (StimuliCoeff != cmpSettings.StimuliCoeff ||
                Resistance != cmpSettings.Resistance ||
                DecayRate != cmpSettings.DecayRate ||
                ResetV != cmpSettings.ResetV ||
                FiringThresholdV != cmpSettings.FiringThresholdV
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
        public AdSimpleIFSettings DeepClone()
        {
            AdSimpleIFSettings clone = new AdSimpleIFSettings(this);
            return clone;
        }

    }//AdSimpleIFSettings

}//Namespace
