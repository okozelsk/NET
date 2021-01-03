using RCNet.RandomValue;
using System;
using System.Globalization;
using System.Xml.Linq;

namespace RCNet.Neural.Activation
{
    /// <summary>
    /// Configuration of the AFSpikingSimpleIF activation function.
    /// </summary>
    [Serializable]
    public class AFSpikingSimpleIFSettings : RCNetBaseSettings, IActivationSettings
    {
        //Constants
        /// <summary>
        /// The name of the associated xsd type.
        /// </summary>
        public const string XsdTypeName = "ActivationSimpleIFType";

        //Typical values
        /// <summary>
        /// The typical value of resistance.
        /// </summary>
        public const double TypicalResistance = 20;
        /// <summary>
        /// The typical value of decay rate.
        /// </summary>
        public const double TypicalDecayRate = 0.05;
        /// <summary>
        /// The typical value of reset voltage.
        /// </summary>
        public const double TypicalResetV = 5;
        /// <summary>
        /// The typical value of firing voltage.
        /// </summary>
        public const double TypicalFiringThresholdV = 20;

        //Attribute properties
        /// <summary>
        /// The membrane resistance (Mohm).
        /// </summary>
        public URandomValueSettings Resistance { get; }
        /// <summary>
        /// The membrane potential decay rate.
        /// </summary>
        public URandomValueSettings DecayRate { get; }
        /// <summary>
        /// The membrane reset potential (mV).
        /// </summary>
        public URandomValueSettings ResetV { get; }
        /// <summary>
        /// The membrane firing threshold (mV).
        /// </summary>
        public URandomValueSettings FiringThresholdV { get; }
        /// <summary>
        /// The number of after-spike computation cycles while an input stimuli to be ignored (cycles).
        /// </summary>
        public int RefractoryPeriods { get; }

        //Constructors
        /// <summary>
        /// Creates an initialized instance.
        /// </summary>
        /// <remarks>
        /// The arguments are in RandomValue form to allow their dynamic random initialization within the specified ranges.
        /// </remarks>
        /// <param name="resistance">The membrane resistance (Mohm).</param>
        /// <param name="decayRate">The membrane potential decay rate.</param>
        /// <param name="resetV">The membrane reset potential (mV).</param>
        /// <param name="firingThresholdV">The membrane firing threshold (mV).</param>
        /// <param name="refractoryPeriods">The number of after-spike computation cycles while an input stimuli to be ignored (cycles).</param>
        public AFSpikingSimpleIFSettings(URandomValueSettings resistance = null,
                                         URandomValueSettings decayRate = null,
                                         URandomValueSettings resetV = null,
                                         URandomValueSettings firingThresholdV = null,
                                         int refractoryPeriods = 1
                                         )
        {
            Resistance = URandomValueSettings.CloneOrCreate(resistance, TypicalResistance);
            DecayRate = URandomValueSettings.CloneOrCreate(decayRate, TypicalDecayRate);
            ResetV = URandomValueSettings.CloneOrCreate(resetV, TypicalResetV);
            FiringThresholdV = URandomValueSettings.CloneOrCreate(firingThresholdV, TypicalFiringThresholdV);
            RefractoryPeriods = refractoryPeriods;
            Check();
            return;
        }

        /// <summary>
        /// The copy constructor.
        /// </summary>
        /// <param name="source">The source instance.</param>
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
        /// Creates an initialized instance.
        /// </summary>
        /// <param name="elem">A xml element containing the configuration data.</param>
        public AFSpikingSimpleIFSettings(XElement elem)
        {
            //Validation
            XElement activationSettingsElem = Validate(elem, XsdTypeName);
            //Parsing
            Resistance = URandomValueSettings.LoadOrCreate(activationSettingsElem, "resistance", TypicalResistance);
            DecayRate = URandomValueSettings.LoadOrCreate(activationSettingsElem, "decayRate", TypicalDecayRate);
            ResetV = URandomValueSettings.LoadOrCreate(activationSettingsElem, "resetV", TypicalResetV);
            FiringThresholdV = URandomValueSettings.LoadOrCreate(activationSettingsElem, "firingThresholdV", TypicalFiringThresholdV);
            RefractoryPeriods = int.Parse(activationSettingsElem.Attribute("refractoryPeriods").Value, CultureInfo.InvariantCulture);
            Check();
            return;
        }

        //Properties
        /// <inheritdoc/>
        public ActivationType TypeOfActivation { get { return ActivationType.Spiking; } }

        /// <summary>
        /// Checks the defaults.
        /// </summary>
        public bool IsDefaultResistance { get { return (Resistance.Min == TypicalResistance && Resistance.Max == TypicalResistance && Resistance.DistrType == RandomCommon.DistributionType.Uniform); } }

        /// <summary>
        /// Checks the defaults.
        /// </summary>
        public bool IsDefaultDecayRate { get { return (DecayRate.Min == TypicalDecayRate && DecayRate.Max == TypicalDecayRate && DecayRate.DistrType == RandomCommon.DistributionType.Uniform); } }

        /// <summary>
        /// Checks the defaults.
        /// </summary>
        public bool IsDefaultResetV { get { return (ResetV.Min == TypicalResetV && ResetV.Max == TypicalResetV && ResetV.DistrType == RandomCommon.DistributionType.Uniform); } }

        /// <summary>
        /// Checks the defaults.
        /// </summary>
        public bool IsDefaultFiringThresholdV { get { return (FiringThresholdV.Min == TypicalFiringThresholdV && FiringThresholdV.Max == TypicalFiringThresholdV && FiringThresholdV.DistrType == RandomCommon.DistributionType.Uniform); } }

        /// <summary>
        /// Checks the defaults.
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
