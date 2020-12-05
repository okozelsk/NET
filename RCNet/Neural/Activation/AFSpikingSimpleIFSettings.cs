using System;
using System.Globalization;
using System.Xml.Linq;
using RCNet.RandomValue;

namespace RCNet.Neural.Activation
{
    /// <summary>
    /// Configuration of the AFSpikingSimpleIF activation function.
    /// Arguments are in RandomValue form to allow their dynamic random initialization within the specified ranges.
    /// </summary>
    [Serializable]
    public class AFSpikingSimpleIFSettings : RCNetBaseSettings, IActivationSettings
    {
        //Constants
        /// <summary>
        /// Name of the associated xsd type
        /// </summary>
        public const string XsdTypeName = "ActivationSimpleIFType";

        //Typical values
        /// <summary>
        /// Typical value of resistance
        /// </summary>
        public const double TypicalResistance = 20;
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
        public URandomValueSettings Resistance { get; }
        /// <summary>
        /// Membrane potential decay rate
        /// </summary>
        public URandomValueSettings DecayRate { get; }
        /// <summary>
        /// Membrane reset potential (mV)
        /// </summary>
        public URandomValueSettings ResetV { get; }
        /// <summary>
        /// Membrane firing threshold (mV)
        /// </summary>
        public URandomValueSettings FiringThresholdV { get; }
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
        public AFSpikingSimpleIFSettings(URandomValueSettings resistance = null,
                                URandomValueSettings decayRate = null,
                                URandomValueSettings resetV = null,
                                URandomValueSettings firingThresholdV = null,
                                int refractoryPeriods = 1
                                )
        {
            Resistance = URandomValueSettings.CloneOrDefault(resistance, TypicalResistance);
            DecayRate = URandomValueSettings.CloneOrDefault(decayRate, TypicalDecayRate);
            ResetV = URandomValueSettings.CloneOrDefault(resetV, TypicalResetV);
            FiringThresholdV = URandomValueSettings.CloneOrDefault(firingThresholdV, TypicalFiringThresholdV);
            RefractoryPeriods = refractoryPeriods;
            Check();
            return;
        }

        /// <summary>
        /// Copy constructor
        /// </summary>
        /// <param name="source">Source instance</param>
        public AFSpikingSimpleIFSettings(AFSpikingSimpleIFSettings source)
        {
            Resistance = (URandomValueSettings)source.Resistance.DeepClone();
            DecayRate = (URandomValueSettings)source.DecayRate.DeepClone();
            ResetV = (URandomValueSettings)source.ResetV.DeepClone();
            FiringThresholdV = (URandomValueSettings)source.FiringThresholdV.DeepClone();
            RefractoryPeriods = source.RefractoryPeriods;
            return;
        }

        /// <summary>
        /// Creates an instance and initializes it from given xml element.
        /// </summary>
        /// <param name="elem">Xml element containing the initialization settings</param>
        public AFSpikingSimpleIFSettings(XElement elem)
        {
            //Validation
            XElement activationSettingsElem = Validate(elem, XsdTypeName);
            //Parsing
            Resistance = URandomValueSettings.LoadOrDefault(activationSettingsElem, "resistance", TypicalResistance);
            DecayRate = URandomValueSettings.LoadOrDefault(activationSettingsElem, "decayRate", TypicalDecayRate);
            ResetV = URandomValueSettings.LoadOrDefault(activationSettingsElem, "resetV", TypicalResetV);
            FiringThresholdV = URandomValueSettings.LoadOrDefault(activationSettingsElem, "firingThresholdV", TypicalFiringThresholdV);
            RefractoryPeriods = int.Parse(activationSettingsElem.Attribute("refractoryPeriods").Value, CultureInfo.InvariantCulture);
            Check();
            return;
        }

        //Properties
        /// <inheritdoc/>
        public ActivationType TypeOfActivation { get { return ActivationType.Spiking; } }

        /// <summary>
        /// Checks the defaults
        /// </summary>
        public bool IsDefaultResistance { get { return (Resistance.Min == TypicalResistance && Resistance.Max == TypicalResistance && Resistance.DistrType == RandomCommon.DistributionType.Uniform); } }

        /// <summary>
        /// Checks the defaults
        /// </summary>
        public bool IsDefaultDecayRate { get { return (DecayRate.Min == TypicalDecayRate && DecayRate.Max == TypicalDecayRate && DecayRate.DistrType == RandomCommon.DistributionType.Uniform); } }

        /// <summary>
        /// Checks the defaults
        /// </summary>
        public bool IsDefaultResetV { get { return (ResetV.Min == TypicalResetV && ResetV.Max == TypicalResetV && ResetV.DistrType == RandomCommon.DistributionType.Uniform); } }

        /// <summary>
        /// Checks the defaults
        /// </summary>
        public bool IsDefaultFiringThresholdV { get { return (FiringThresholdV.Min == TypicalFiringThresholdV && FiringThresholdV.Max == TypicalFiringThresholdV && FiringThresholdV.DistrType == RandomCommon.DistributionType.Uniform); } }

        /// <summary>
        /// Checks the defaults
        /// </summary>
        public bool IsDefaultRefractoryPeriods { get { return (RefractoryPeriods == ActivationFactory.DefaultRefractoryPeriods); } }

        /// <inheritdoc/>
        public override bool ContainsOnlyDefaults
        {
            get
            {
                return IsDefaultResistance &&
                       IsDefaultDecayRate &&
                       IsDefaultResetV &&
                       IsDefaultFiringThresholdV &&
                       IsDefaultRefractoryPeriods;
            }
        }

        //Methods
        /// <inheritdoc/>
        protected override void Check()
        {
            if (RefractoryPeriods < 0)
            {
                throw new ArgumentException($"Invalid RefractoryPeriods {RefractoryPeriods.ToString(CultureInfo.InvariantCulture)}. RefractoryPeriods must be GE to 0.", "RefractoryPeriods");
            }
            return;
        }

        /// <inheritdoc/>
        public override RCNetBaseSettings DeepClone()
        {
            return new AFSpikingSimpleIFSettings(this);
        }

        /// <inheritdoc/>
        public override XElement GetXml(string rootElemName, bool suppressDefaults)
        {
            XElement rootElem = new XElement(rootElemName);
            if (!suppressDefaults || !IsDefaultRefractoryPeriods)
            {
                rootElem.Add(new XAttribute("refractoryPeriods", RefractoryPeriods.ToString(CultureInfo.InvariantCulture)));
            }
            if (!suppressDefaults || !IsDefaultResistance)
            {
                rootElem.Add(Resistance.GetXml("resistance", suppressDefaults));
            }
            if (!suppressDefaults || !IsDefaultDecayRate)
            {
                rootElem.Add(DecayRate.GetXml("decayRate", suppressDefaults));
            }
            if (!suppressDefaults || !IsDefaultResetV)
            {
                rootElem.Add(ResetV.GetXml("resetV", suppressDefaults));
            }
            if (!suppressDefaults || !IsDefaultFiringThresholdV)
            {
                rootElem.Add(FiringThresholdV.GetXml("firingThresholdV", suppressDefaults));
            }

            Validate(rootElem, XsdTypeName);
            return rootElem;
        }

        /// <inheritdoc/>
        public override XElement GetXml(bool suppressDefaults)
        {
            return GetXml("activationSimpleIF", suppressDefaults);
        }

    }//AFSpikingSimpleIFSettings

}//Namespace
